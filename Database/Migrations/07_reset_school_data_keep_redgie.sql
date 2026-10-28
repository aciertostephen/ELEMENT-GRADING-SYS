-- DESTRUCTIVE RESET. Run only after taking a full backup of gradebook_db.
-- Keeps the existing admin account dean (Redgie Tan) and that admin's profile.
-- Removes other users and all school-year, class, section, enrollment, grade,
-- notification, student, and teacher records.
-- Preserves the program/course catalogs and grade_scale because the application
-- needs them to create new sections/classes and calculate grades after the reset.

DELIMITER $$
DROP PROCEDURE IF EXISTS sp_reset_school_data_keep_redgie$$
CREATE PROCEDURE sp_reset_school_data_keep_redgie()
BEGIN
    DECLARE v_keep_user_id INT DEFAULT NULL;
    DECLARE v_matches INT DEFAULT 0;
    DECLARE v_old_fk_checks INT DEFAULT 1;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_fk_checks;
        RESIGNAL;
    END;

    -- Refuse to run unless the intended admin can be identified by username,
    -- role, and the Redgie Tan admin profile name.
    SELECT COUNT(*), MIN(u.user_id)
      INTO v_matches, v_keep_user_id
      FROM users u
      JOIN admin_profile ap ON ap.user_id = u.user_id
     WHERE u.username = 'dean'
       AND u.role = 'Admin'
       AND LOWER(TRIM(ap.first_name)) = 'redgie'
       AND LOWER(TRIM(ap.last_name)) = 'tan';

    IF v_matches <> 1 OR v_keep_user_id IS NULL THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'Reset stopped: dean / Redgie Tan was not found as exactly one Admin with an admin_profile row.';
    END IF;

    SET v_old_fk_checks = @@FOREIGN_KEY_CHECKS;
    SET FOREIGN_KEY_CHECKS = 0;
    START TRANSACTION;

    DELETE FROM grade_entry;
    DELETE FROM enrollment;
    DELETE FROM student_section_history;
    DELETE FROM notification;

    DELETE FROM student_profile;
    DELETE FROM teacher_profile;

    -- The fixed rubric trigger blocks deletion of fixed components.
    UPDATE grading_rubric SET is_fixed = 0;
    DELETE FROM grade_item;
    DELETE FROM grading_rubric;
    DELETE FROM classes;

    DELETE FROM academic_term;
    DELETE FROM section;
    DELETE FROM school_year_calendar;

    DELETE FROM admin_profile WHERE user_id <> v_keep_user_id;
    DELETE FROM users WHERE user_id <> v_keep_user_id;

    COMMIT;
    SET FOREIGN_KEY_CHECKS = v_old_fk_checks;
END$$
DELIMITER ;

CALL sp_reset_school_data_keep_redgie();
DROP PROCEDURE sp_reset_school_data_keep_redgie;

-- Verify only Redgie Tan's admin account remains and that school-year records
-- have been cleared. Program, course, and grade-scale reference rows remain.
SELECT u.user_id, u.username, u.role,
       CONCAT_WS(' ', ap.first_name, NULLIF(ap.middle_name, ''), ap.last_name) AS full_name
FROM users u
JOIN admin_profile ap ON ap.user_id = u.user_id;

SELECT
    (SELECT COUNT(*) FROM users WHERE NOT (username = 'dean' AND role = 'Admin')) AS other_users,
    (SELECT COUNT(*) FROM school_year_calendar) AS school_years,
    (SELECT COUNT(*) FROM academic_term) AS academic_terms,
    (SELECT COUNT(*) FROM section) AS sections,
    (SELECT COUNT(*) FROM classes) AS classes,
    (SELECT COUNT(*) FROM student_profile) AS students,
    (SELECT COUNT(*) FROM teacher_profile) AS teachers,
    (SELECT COUNT(*) FROM enrollment) AS enrollments,
    (SELECT COUNT(*) FROM grade_entry) AS grade_entries,
    (SELECT COUNT(*) FROM notification) AS notifications;
