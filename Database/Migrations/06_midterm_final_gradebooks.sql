-- Separate the existing class gradebook into Midterm and Final periods.
-- Run once against gradebook_db after backing up the database.
-- Existing rubric rows and assessment items remain the Final gradebook.

ALTER TABLE grading_rubric
    ADD COLUMN period ENUM('Midterm','Final') NOT NULL DEFAULT 'Final' AFTER class_id;

-- Rubric component names must be unique within a period, not across both.
ALTER TABLE grading_rubric
    DROP INDEX uq_rubric_class_component,
    ADD UNIQUE KEY uq_rubric_class_period_component (class_id, period, component_name);

ALTER TABLE classes
    ADD COLUMN midterm_released_at DATETIME NULL AFTER grades_released_at,
    ADD COLUMN finals_released_at DATETIME NULL AFTER midterm_released_at;

ALTER TABLE enrollment
    ADD COLUMN midterm_grade DECIMAL(5,2) NULL AFTER grade_equivalent,
    ADD COLUMN midterm_equivalent DECIMAL(3,2) NULL AFTER midterm_grade,
    ADD COLUMN finals_grade DECIMAL(5,2) NULL AFTER midterm_equivalent,
    ADD COLUMN finals_equivalent DECIMAL(3,2) NULL AFTER finals_grade;

-- Preserve already released legacy grades as both periods so historical
-- transcripts remain complete and keep the same semester grade.
UPDATE classes SET finals_released_at = grades_released_at
WHERE grades_released_at IS NOT NULL AND finals_released_at IS NULL;

UPDATE enrollment e
JOIN classes c ON c.class_id = e.class_id
SET e.midterm_grade = e.final_grade,
    e.midterm_equivalent = e.grade_equivalent,
    e.finals_grade = e.final_grade,
    e.finals_equivalent = e.grade_equivalent,
    e.final_grade = e.final_grade,
    e.grade_equivalent = e.grade_equivalent
WHERE c.grades_released_at IS NOT NULL;

UPDATE classes
SET midterm_released_at = grades_released_at
WHERE grades_released_at IS NOT NULL AND midterm_released_at IS NULL;

-- Make a separate empty Midterm rubric for every class. Existing scores stay
-- attached to their original Final-period grade items.
INSERT INTO grading_rubric (class_id, period, component_name, weight_percent, is_fixed)
SELECT class_id, 'Midterm', component_name, weight_percent, is_fixed
FROM grading_rubric
WHERE period = 'Final';

DELIMITER $$
-- New classes start with both periods. The app will add the starter assessment
-- items the first time a teacher opens each period.
DROP PROCEDURE IF EXISTS sp_create_class$$
CREATE PROCEDURE sp_create_class(IN p_course_id INT, IN p_section_id INT, IN p_teacher_id VARCHAR(16), IN p_term_id INT)
BEGIN
  DECLARE v_class_id INT;
  DECLARE EXIT HANDLER FOR SQLEXCEPTION
  BEGIN
    ROLLBACK;
    RESIGNAL;
  END;

  START TRANSACTION;
  INSERT INTO classes (course_id, section_id, teacher_id, term_id)
  VALUES (p_course_id, p_section_id, p_teacher_id, p_term_id);
  SET v_class_id = LAST_INSERT_ID();

  INSERT INTO grading_rubric (class_id, period, component_name, weight_percent, is_fixed) VALUES
    (v_class_id, 'Final', 'Participation', 10.00, 0),
    (v_class_id, 'Final', 'Quizzes', 20.00, 0),
    (v_class_id, 'Final', 'Projects', 30.00, 0),
    (v_class_id, 'Final', 'Final Exam', 40.00, 1),
    (v_class_id, 'Midterm', 'Participation', 10.00, 0),
    (v_class_id, 'Midterm', 'Quizzes', 20.00, 0),
    (v_class_id, 'Midterm', 'Projects', 30.00, 0),
    (v_class_id, 'Midterm', 'Final Exam', 40.00, 1);

  COMMIT;
  SELECT v_class_id AS class_id;
END$$

DROP PROCEDURE IF EXISTS sp_compute_class_period_grades$$
CREATE PROCEDURE sp_compute_class_period_grades(IN p_class_id INT, IN p_period VARCHAR(10))
BEGIN
  UPDATE enrollment e
  LEFT JOIN (
    SELECT t.enrollment_id,
           ROUND(LEAST(100, SUM(ROUND(t.avg_gra * t.weight_percent / 100, 2))), 2) AS pct
    FROM (
      SELECT ge.enrollment_id, r.rubric_id, r.weight_percent,
             AVG(ROUND(ge.score / gi.max_score * 50 + 50, 2)) AS avg_gra
      FROM grade_entry ge
      JOIN grade_item gi ON gi.item_id = ge.item_id AND gi.max_score > 0
      JOIN grading_rubric r ON r.rubric_id = gi.rubric_id
      WHERE r.class_id = p_class_id AND r.period = p_period AND ge.score IS NOT NULL
      GROUP BY ge.enrollment_id, r.rubric_id, r.weight_percent
    ) t
    GROUP BY t.enrollment_id
  ) g ON g.enrollment_id = e.enrollment_id
  SET e.midterm_grade = IF(p_period = 'Midterm', IF(e.status = 'Dropped', NULL, g.pct), e.midterm_grade),
      e.midterm_equivalent = IF(p_period = 'Midterm', IF(e.status = 'Dropped' OR g.pct IS NULL, NULL,
          (SELECT gs.grade_equivalent FROM grade_scale gs WHERE gs.min_percent <= g.pct ORDER BY gs.min_percent DESC LIMIT 1)), e.midterm_equivalent),
      e.finals_grade = IF(p_period = 'Final', IF(e.status = 'Dropped', NULL, g.pct), e.finals_grade),
      e.finals_equivalent = IF(p_period = 'Final', IF(e.status = 'Dropped' OR g.pct IS NULL, NULL,
          (SELECT gs.grade_equivalent FROM grade_scale gs WHERE gs.min_percent <= g.pct ORDER BY gs.min_percent DESC LIMIT 1)), e.finals_equivalent)
  WHERE e.class_id = p_class_id;

  -- Show the released period as provisional until both are available. Once
  -- both have been released, publish their arithmetic mean as the semester grade.
  UPDATE enrollment e
  SET e.final_grade = IF(e.status = 'Dropped', NULL,
      IF(e.midterm_grade IS NOT NULL AND e.finals_grade IS NOT NULL,
         ROUND((e.midterm_grade + e.finals_grade) / 2, 2),
         COALESCE(e.midterm_grade, e.finals_grade))),
      e.grade_equivalent = IF(e.status = 'Dropped', NULL,
      (SELECT gs.grade_equivalent FROM grade_scale gs
       WHERE gs.min_percent <= IF(e.midterm_grade IS NOT NULL AND e.finals_grade IS NOT NULL,
             ROUND((e.midterm_grade + e.finals_grade) / 2, 2),
             COALESCE(e.midterm_grade, e.finals_grade))
       ORDER BY gs.min_percent DESC LIMIT 1))
  WHERE e.class_id = p_class_id;
END$$

DROP PROCEDURE IF EXISTS sp_release_class_grades$$
CREATE PROCEDURE sp_release_class_grades(IN p_class_id INT, IN p_period VARCHAR(10))
BEGIN
  IF p_period NOT IN ('Midterm','Final') THEN
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Period must be Midterm or Final.';
  END IF;
  CALL sp_compute_class_period_grades(p_class_id, p_period);
  IF p_period = 'Midterm' THEN
    UPDATE classes SET midterm_released_at = NOW() WHERE class_id = p_class_id;
  ELSE
    UPDATE classes SET finals_released_at = NOW(), grades_released_at = NOW() WHERE class_id = p_class_id;
  END IF;
END$$
DELIMITER ;

-- Verify the periods and released grade fields after migration.
SELECT period, COUNT(*) AS rubric_rows FROM grading_rubric GROUP BY period;
SELECT SUM(midterm_released_at IS NOT NULL) AS midterm_released_classes,
       SUM(finals_released_at IS NOT NULL) AS finals_released_classes
FROM classes;
