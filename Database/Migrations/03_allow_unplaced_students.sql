-- Run once after backing up gradebook_db.
-- New student accounts can exist before the administrator places them in a section.
ALTER TABLE student_profile
  MODIFY section_id int(11) DEFAULT NULL,
  MODIFY year_level varchar(1) DEFAULT NULL;

DROP TRIGGER IF EXISTS trg_student_hist_ins;
DROP TRIGGER IF EXISTS trg_student_hist_upd;
DROP TRIGGER IF EXISTS trg_student_year_ins;
DROP TRIGGER IF EXISTS trg_student_year_upd;

DELIMITER $$
CREATE TRIGGER trg_student_hist_ins AFTER INSERT ON student_profile FOR EACH ROW
BEGIN
  IF NEW.section_id IS NOT NULL THEN
    INSERT INTO student_section_history (student_id, section_id, effective_from)
    VALUES (NEW.student_id, NEW.section_id, CURDATE());
  END IF;
END$$

CREATE TRIGGER trg_student_hist_upd AFTER UPDATE ON student_profile FOR EACH ROW
BEGIN
  IF NOT (NEW.section_id <=> OLD.section_id) THEN
    UPDATE student_section_history SET effective_to = CURDATE()
     WHERE student_id = NEW.student_id AND effective_to IS NULL;
    IF NEW.section_id IS NOT NULL THEN
      INSERT INTO student_section_history (student_id, section_id, effective_from)
      VALUES (NEW.student_id, NEW.section_id, CURDATE());
    END IF;
  END IF;
END$$

CREATE TRIGGER trg_student_year_ins BEFORE INSERT ON student_profile FOR EACH ROW
BEGIN
  IF NEW.section_id IS NULL THEN
    SET NEW.year_level = NULL;
  ELSE
    SET NEW.year_level = (SELECT CAST(year_level AS CHAR(1)) FROM section
                           WHERE section_id = NEW.section_id);
  END IF;
END$$

CREATE TRIGGER trg_student_year_upd BEFORE UPDATE ON student_profile FOR EACH ROW
BEGIN
  IF NOT (NEW.section_id <=> OLD.section_id) THEN
    IF NEW.section_id IS NULL THEN
      SET NEW.year_level = NULL;
    ELSE
      SET NEW.year_level = (SELECT CAST(year_level AS CHAR(1)) FROM section
                             WHERE section_id = NEW.section_id);
    END IF;
  END IF;
END$$
DELIMITER ;
