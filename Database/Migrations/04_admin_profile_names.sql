-- Stores real names for administrator accounts, which have no profile table today.
-- Run against gradebook_db before using Admin > User Management name editing.
CREATE TABLE IF NOT EXISTS admin_profile (
    user_id INT NOT NULL,
    first_name VARCHAR(80) NOT NULL,
    middle_name VARCHAR(80) NULL,
    last_name VARCHAR(80) NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT fk_admin_profile_user
        FOREIGN KEY (user_id) REFERENCES users(user_id)
        ON UPDATE CASCADE ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

-- After creating the table, edit each Admin in User Management and enter their
-- real full name. Usernames cannot reliably be converted into names automatically.
