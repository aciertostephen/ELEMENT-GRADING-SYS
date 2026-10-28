-- Add the actual names for the two existing administrator accounts.
-- Run this after 04_admin_profile_names.sql against gradebook_db.
START TRANSACTION;

INSERT INTO admin_profile (user_id, first_name, middle_name, last_name)
SELECT user_id, 'Redgie', NULL, 'Tan'
FROM users
WHERE user_id = 1 AND username = 'dean' AND role = 'Admin'
ON DUPLICATE KEY UPDATE
    first_name = VALUES(first_name),
    middle_name = VALUES(middle_name),
    last_name = VALUES(last_name);

INSERT INTO admin_profile (user_id, first_name, middle_name, last_name)
SELECT user_id, 'Rubie', 'Jane', 'Diosa'
FROM users
WHERE user_id = 45 AND username = 'ruby' AND role = 'Admin'
ON DUPLICATE KEY UPDATE
    first_name = VALUES(first_name),
    middle_name = VALUES(middle_name),
    last_name = VALUES(last_name);

COMMIT;

-- Verify the entries created/updated by this script.
SELECT u.user_id, u.username,
       CONCAT_WS(' ', ap.first_name, NULLIF(ap.middle_name, ''), ap.last_name) AS full_name
FROM users u
JOIN admin_profile ap ON ap.user_id = u.user_id
WHERE u.username IN ('dean', 'ruby') AND u.role = 'Admin'
ORDER BY u.user_id;
