ELEMENT Grading System database updates

Before running a migration, make a backup of the gradebook_db database.

Run these scripts against gradebook_db in order:
1. 03_allow_unplaced_students.sql
2. 04_admin_profile_names.sql
3. 05_populate_admin_profile_names.sql
4. 06_midterm_final_gradebooks.sql
5. 08_student_class_assignment_notifications.sql

Migration 03 allows an administrator to create student accounts before assigning
the students to a section. Existing section assignments are preserved.

When an administrator creates accounts by importing a section roster, the initial
password is the student's student number. Students should change it in Profile
Settings after their first sign-in.

Migration 04 adds name fields for administrator accounts. After running it, edit
each Admin in User Management and enter the administrator's real name. The system
cannot infer real names from usernames.

For the current dean and ruby accounts, Migration 05 inserts their supplied names
and displays the updated rows for verification. The user_id and username are both
checked before writing.

Migration 06 creates separate Midterm and Final rubrics and grade fields. Existing
released class grades are retained for both periods so historical records keep
their former displayed grade. New transcripts show a semester grade only after
both periods have been released; it is the average of the two period percentages,
with the equivalent calculated from that average.

Migration 08 permits teachers to send a Class Update notification to a student
when the student is added to or re-enrolled in a class. The notification table
and its existing type values are retained.
