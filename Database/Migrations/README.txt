ELEMENT Grading System database updates

Before running a migration, make and verify a backup of the gradebook_db database.

The supplied gradebook_db (2).sql already contains the schema from migrations
03, 04, 06, and 08. Do not rerun those ALTER TABLE scripts on that database.
For the supplied schema, run 09_semester_grade_average.sql once. Run
05_populate_admin_profile_names.sql after 04 only if you want to populate the
known administrator names; it does not create accounts.

For a database that has not received these changes, apply the missing scripts
in this order: 03, 04, 05, 06, 08, then 09. Confirm each migration's schema
changes are not already present before running it.

Migration 03 allows an administrator to create student accounts before assigning
the students to a section. Existing section assignments are preserved.

When an administrator creates accounts by importing a section roster, the initial
password is the student's student number. Students should change it in Profile
Settings after their first sign-in.

Migration 04 adds name fields for administrator accounts. After running it, edit
each Admin in User Management and enter the administrator's real name. The system
cannot infer real names from usernames.

Migration 05 writes Redgie Tan for the Admin account named dean and Rubie Jane
Diosa for an existing Admin account named secretary. It matches by username and
role, not by a hard-coded user ID. If secretary does not exist, it reports that
no account was found and creates nothing.

Migration 06 creates separate Midterm and Final rubrics and grade fields. Existing
released class grades are retained for both periods so historical records keep
their former displayed grade. New transcripts show a semester grade only after
both periods have been released; it is the average of the two period percentages,
with the equivalent calculated from that average.

Migration 08 permits teachers to send a Class Update notification to a student
when the student is added to or re-enrolled in a class. The notification table
and its existing type values are retained.

Migration 09 corrects the semester-grade procedure used by migration 06 and
repairs rows that previously held a single-period provisional semester grade.
The transcript query also requires both periods to be released and calculates
the displayed semester result from their saved Midterm and Final percentages.

Migration 07 is an optional destructive cleanup script, not part of the normal
migration sequence. It runs in preview-only mode by default. Do not change its
confirmation variable unless you intend to delete school data and have verified
a restorable backup.
