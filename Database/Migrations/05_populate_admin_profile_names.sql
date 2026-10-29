-- Add names for the supplied administrator accounts, if those accounts exist.
-- Run this after 04_admin_profile_names.sql against gradebook_db.
START TRANSACTION;

INSERT INTO admin_profile (user_id, first_name, middle_name, last_name)
SELECT user_id, 'Redgie', NULL, 'Tan'
FROM users
WHERE username = 'dean' AND role = 'Admin'
ON DUPLICATE KEY UPDATE
    first_name = VALUES(first_name),
    middle_name = VALUES(middle_name),
    last_name = VALUES(last_name);

INSERT INTO admin_profile (user_id, first_name, middle_name, last_name)
SELECT user_id, 'Rubie', 'Jane', 'Diosa'
FROM users
WHERE username = 'secretary' AND role = 'Admin'
ON DUPLICATE KEY UPDATE
    first_name = VALUES(first_name),
    middle_name = VALUES(middle_name),
    last_name = VALUES(last_name);

COMMIT;

-- Verify both expected accounts, including a clear result if an account is absent.
SELECT expected.username,
       CASE WHEN u.user_id IS NULL THEN 'account not found'
            WHEN ap.user_id IS NULL THEN 'profile missing'
            ELSE 'profile saved' END AS result,
       CONCAT_WS(' ', ap.first_name, NULLIF(ap.middle_name, ''), ap.last_name) AS full_name
FROM (SELECT 'dean' AS username UNION ALL SELECT 'secretary') expected
LEFT JOIN users u ON u.username = expected.username AND u.role = 'Admin'
LEFT JOIN admin_profile ap ON ap.user_id = u.user_id
ORDER BY expected.username;
