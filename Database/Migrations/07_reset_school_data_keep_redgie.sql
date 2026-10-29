-- OPTIONAL DESTRUCTIVE CLEANUP SCRIPT. It is not a normal migration.
-- Default behavior is a preview only; no school data is deleted.
-- To perform the reset, explicitly edit @confirm_school_data_reset below to:
-- DELETE ALL SCHOOL DATA, KEEP REDGIE
-- Take and verify a full backup before making that change.

DELIMITER $$
DROP PROCEDURE IF EXISTS sp_reset_school_data_keep_redgie$$
CREATE PROCEDURE sp_reset_school_data_keep_redgie(IN p_confirmation VARCHAR(64))
main: BEGIN
    DECLARE v_keep_user_id INT DEFAULT NULL;
    DECLARE v_matches INT DEFAULT 0;
    DECLARE v_old_fk_checks INT DEFAULT 1;

    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        ROLLBACK;
        SET FOREIGN_KEY_CHECKS = v_old_fk_checks;
        RESIGNAL;
    END;

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

    SELECT 'PREVIEW: rows listed below are the records this script would remove.' AS action,
           v_keep_user_id AS retained_admin_user_id;
    SELECT
        (SELECT COUNT(*) FROM users WHERE user_id <> v_keep_user_id) AS other_users,
        (SELECT COUNT(*) FROM school_year_calendar) AS school_years,
        (SELECT COUNT(*) FROM academic_term) AS academic_terms,
        (SELECT COUNT(*) FROM section) AS sections,
        (SELECT COUNT(*) FROM classes) AS classes,
        (SELECT COUNT(*) FROM student_profile) AS students,
        (SELECT COUNT(*) FROM teacher_profile) AS teachers,
        (SELECT COUNT(*) FROM enrollment) AS enrollments,
        (SELECT COUNT(*) FROM grade_entry) AS grade_entries,
        (SELECT COUNT(*) FROM notification) AS notifications;

    IF COALESCE(p_confirmation, '') <> 'DELETE ALL SCHOOL DATA, KEEP REDGIE' THEN
        SELECT 'Preview complete. No records were deleted.' AS result;
        LEAVE main;
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

    SELECT 'Reset complete. Redgie Tan was retained.' AS result;
END$$
DELIMITER ;

-- Preview is the default. Change NULL to the exact phrase only after reviewing
-- the counts above and confirming that the verified backup can be restored.
SET @confirm_school_data_reset = NULL;
CALL sp_reset_school_data_keep_redgie(@confirm_school_data_reset);
DROP PROCEDURE sp_reset_school_data_keep_redgie;

SELECT u.user_id, u.username, u.role,
       CONCAT_WS(' ', ap.first_name, NULLIF(ap.middle_name, ''), ap.last_name) AS full_name
FROM users u
JOIN admin_profile ap ON ap.user_id = u.user_id;
