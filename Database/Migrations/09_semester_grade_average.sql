-- Correct semester-grade calculation for databases that already ran migration 06.
-- Run once against gradebook_db after taking a backup.
-- A semester grade is the average of saved Midterm and Final grades. A single
-- period is not copied into final_grade as a provisional semester result.

DELIMITER $$
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

  UPDATE enrollment e
  SET e.final_grade = IF(e.status = 'Dropped' OR e.midterm_grade IS NULL OR e.finals_grade IS NULL, NULL,
         ROUND((e.midterm_grade + e.finals_grade) / 2, 2)),
      e.grade_equivalent = IF(e.status = 'Dropped' OR e.midterm_grade IS NULL OR e.finals_grade IS NULL, NULL,
         (SELECT gs.grade_equivalent FROM grade_scale gs
          WHERE gs.min_percent <= ROUND((e.midterm_grade + e.finals_grade) / 2, 2)
          ORDER BY gs.min_percent DESC LIMIT 1))
  WHERE e.class_id = p_class_id;
END$$
DELIMITER ;

-- Repair rows previously assigned a single-period provisional semester grade.
UPDATE enrollment e
SET e.final_grade = IF(e.status = 'Dropped' OR e.midterm_grade IS NULL OR e.finals_grade IS NULL, NULL,
       ROUND((e.midterm_grade + e.finals_grade) / 2, 2)),
    e.grade_equivalent = IF(e.status = 'Dropped' OR e.midterm_grade IS NULL OR e.finals_grade IS NULL, NULL,
       (SELECT gs.grade_equivalent FROM grade_scale gs
        WHERE gs.min_percent <= ROUND((e.midterm_grade + e.finals_grade) / 2, 2)
        ORDER BY gs.min_percent DESC LIMIT 1));

-- Verify that all populated semester grades are averages of both periods.
SELECT COUNT(*) AS invalid_semester_grade_rows
FROM enrollment e
WHERE (e.midterm_grade IS NULL OR e.finals_grade IS NULL) AND e.final_grade IS NOT NULL;
