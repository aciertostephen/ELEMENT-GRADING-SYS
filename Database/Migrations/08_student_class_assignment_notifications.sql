-- Allow the assigned teacher to notify a student when the student is added to a class.
-- This keeps the existing notification enum; no table change is needed.
-- Run once against gradebook_db after backing up the database.

DELIMITER $$
DROP TRIGGER IF EXISTS trg_notification_sender_teacher$$
CREATE TRIGGER trg_notification_sender_teacher
BEFORE INSERT ON notification
FOR EACH ROW
BEGIN
  DECLARE v_role VARCHAR(10);
  SET v_role = (SELECT role FROM users WHERE user_id = NEW.sender_id);

  IF v_role = 'Admin' THEN
    IF NEW.recipient_user_id IS NULL THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'An admin notification needs a recipient.';
    END IF;
  ELSEIF v_role = 'Teacher' THEN
    IF NEW.recipient_user_id IS NOT NULL AND NEW.type NOT IN ('Grade Release', 'Class Update') THEN
      SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Teachers can only send requests to the admin, grade release notices, or class assignment notices.';
    END IF;
  ELSE
    SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Students cannot send notifications.';
  END IF;
END$$
DELIMITER ;

-- Verify the updated teacher notification rule is installed.
SHOW CREATE TRIGGER trg_notification_sender_teacher;
