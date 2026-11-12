# Backend and Database: Current Team Handoff

**Project:** Cybersecurity Education and Awareness Training Application  
**Course:** COMP6900 Computing Project, University of Newcastle  
**Audience:** Ankit, Sam and the project team  
**Status:** Working document for team review

## 1. Purpose

This document gives the team a shared overview of the backend and database in the current application. It explains how the main features work, where important files can be found and what should be checked before submission.

An earlier backend handoff is kept in the `Archive - Earlier Planning` folder. That document recorded proposed responsibilities at an earlier stage of the project. Each team member should confirm their actual contributions for the final project report.

## 2. Application Overview

The Cybersecurity Education and Awareness Training Application is a web-based prototype designed to help employees build safer cybersecurity habits.

It contains eight learning modules. Each module includes learning sections and a multiple-choice knowledge assessment. The application records learner progress, assessment attempts, answers and results.

The application uses:

- **ASP.NET Core Razor Pages** for the web application.
- **ASP.NET Core Identity** for accounts, authentication and roles.
- **Entity Framework Core** for database access and migrations.
- **SQLite** for local data storage.
- **HTML, CSS and JavaScript** for the user interface.

## 3. Learner Journey

The intended learner journey is:

**Sign in → enrol in a module → read and complete its sections → start the assessment → answer the questions → submit → receive a result and feedback.**

The application saves section progress and assessment activity to the learner’s account. Learners can return to their assessment history to review completed attempts or continue an unfinished attempt.

## 4. Assessment Process

Each module has three question banks, with ten multiple-choice questions in each bank. The server selects an available bank for an assessment attempt and marks the submitted answers.

The default passing mark is **90%** and can be configured in `appsettings.json`.

- A learner who scores below the passing mark can review the material and try again.
- Once a learner passes the module assessment, further attempts for that module are locked.
- The application keeps the learner’s assessment history and identifies their best completed score.

The team should test these behaviours in the final version, including attempts created before the latest changes.

## 5. Assessment Feedback

The team decided that the correct option should not be highlighted for an unanswered or incorrectly answered question when a learner has failed the assessment.

For a **failed attempt**, the intended feedback is:

- Show the learner’s selected answer.
- Indicate whether their selected answer was correct or incorrect.
- Show the question’s explanation.
- Do not highlight an unselected correct option.

For a **passed attempt**, the intended feedback can also highlight the correct options.

The explanations may still provide clues to the correct answers. The team should review the wording and agree whether it supports the intended learning experience.

Before submission, test both failed and passed assessments. Also check what happens when a learner reviews an earlier failed attempt after passing the module later.

## 6. Database

The application uses a local **SQLite** database. Entity Framework Core migrations create and update the database structure.

The database stores records such as:

- User accounts and roles.
- Module enrolments and ownership assignments.
- Completed learning sections.
- Assessment banks, questions and answer options.
- Assessment attempts, selected answers, scores and feedback.
- Module assessment schedules.

ASP.NET Core Identity also uses supporting tables for account-related information. Passwords are managed through Identity’s password-hashing system.

### Database and content files

| File or folder | Purpose |
| --- | --- |
| `CybersecurityTrainingWeb/Data/` | Data models, database context, endpoints and question-bank seeding logic. |
| `CybersecurityTrainingWeb/Migrations/` | Entity Framework Core migrations for the current SQLite database. |
| `CybersecurityTrainingWeb/Data/module-content.json` | Learning module content used by the application. |
| `CybersecurityTrainingWeb/Data/question_banks.json` | Assessment question-bank data used for seeding. |
| `CybersecurityTrainingWeb/Pages/` | Application pages and their server-side page models. |
| `CybersecurityTrainingWeb.Tests/` | Automated tests for application features and access rules. |
| `README.md` | Setup, database and application-running instructions. |
| `06_Technical Design/Current System Architecture.md` | Overview of the architecture and data relationships. |

The old SQL Server schema sketch in the technical-design archive is a historical planning document. The current application uses SQLite and the Entity Framework Core migrations.

## 7. User Roles and Access

The application supports different responsibilities:

- **Learners** access their modules, progress, assessments and results.
- **Module owners** can access management functions for their assigned modules.
- **Administrators** can access broader training administration functions.

The application checks permissions on the server. Each learner should only be able to access their own assessment attempts and results.

Two-factor authentication using an authenticator app is available, but it is not currently compulsory.

## 8. Training Schedules and Notifications

Administrators and assigned module owners can manage training schedules. Schedules can include an assessment due date and a repeat interval.

The notifications page displays a learner’s module assignments and assessment reminders within the application. The current prototype does not send background email or push notifications.

A schedule provides a reminder; it does not by itself prevent a learner from starting an assessment.

## 9. Testing Before Submission

The team should test the final agreed version from a clean copy using the instructions in the README. Record the date, expected result, actual result and any issue found.

Suggested checks include:

1. Register a user and sign in.
2. Enrol in a module and mark all learning sections complete.
3. Confirm the assessment becomes available after the sections are complete.
4. Start an assessment, save answers and continue an unfinished attempt.
5. Submit a failed attempt and check the score and feedback.
6. Retry the assessment and submit a passing attempt.
7. Confirm further attempts are locked after passing.
8. Review the assessment history and best result.
9. Check learner, module-owner and administrator access.
10. Check enrolment, assessment scheduling and notifications.
11. Check how the application responds to an invalid page or unavailable assessment attempt.
12. Run the automated tests and record the actual results.

The team should document any feature that does not work as intended in the final report.

## 10. Database for Submission

The assignment brief asks the team to submit relevant project data and databases. A separate demonstration database has been prepared with the database structure and assessment question banks. It initially contains no learner accounts, assessment attempts or progress.

The team should confirm with the lecturer whether this demonstration database is sufficient or whether additional sample accounts and progress are expected.

The working database and backup files may contain account details and learner activity. Review any database before sharing it. The demonstration database is ignored by Git, so it must be copied into the submission archive manually if the team decides to include it.

## 11. Project Contributions and Evidence

The final project report should describe what each member actually contributed. Each member should review and confirm their own contribution.

The team should also include genuine evidence where required, such as:

- Testing procedures and results.
- Meeting records and project updates.
- The updated Gantt chart.
- Changes to the team pre-action plan.
- The final presentation and project documentation.

Earlier task allocations show what was planned; they should not be treated as proof that a particular person completed a task.

## 12. Current Limitations

The application is a coursework prototype. The following items are outside the verified scope of this handoff:

- Production hosting and production email configuration.
- Background email or push notifications.
- An operational backup process.
- A mandatory two-factor authentication policy.
- A completed full privacy and security review.

The team should also report any limitations or unresolved issues discovered during final testing.

## 13. Final Team Checklist

- [ ] Agree on the final application version.
- [ ] Confirm the assessment feedback behaves as agreed.
- [ ] Test the application from a clean copy.
- [ ] Record actual test results and outstanding issues.
- [ ] Confirm the database and sample data required for submission.
- [ ] Review the README and setup instructions.
- [ ] Confirm each team member’s actual contributions.
- [ ] Complete the final project report and supporting evidence.
- [ ] Prepare the final team presentation.
- [ ] Review the submission files against the assignment brief.