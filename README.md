# Cybersecurity Education and Awareness Training Application

**COMP6900 Computing Project — University of Newcastle**

## About the project

This application is a cybersecurity education and awareness prototype for
workplace learners. It helps employees learn about common security risks,
practise safer behaviours and check their knowledge through assessments.

The application contains eight learning modules. Learners can read the
content, mark sections as complete and take a multiple-choice assessment.
Their progress and results are saved to their account.

The project was developed using ASP.NET Core Razor Pages, Entity Framework
Core, ASP.NET Core Identity and a local SQLite database.

## Main features

- Eight cybersecurity learning modules, each with seven sections.
- User registration, sign-in and profile management.
- Saved learning progress for signed-in users.
- Optional two-factor authentication using an authenticator app.
- Multiple-choice assessments with saved answers and server-side marking.
- Assessment history and review of completed attempts.
- A default passing mark of **90%**.
- Retakes after a failed attempt and a lock on further attempts after passing.
- A learner dashboard and module progress information.
- Administrator and module-owner functions.
- Learner enrolments, assessment schedules and in-app notifications.
- A responsive interface for different screen sizes.

## Project structure

| Folder or file | Purpose |
| --- | --- |
| `CybersecurityTrainingWeb/` | Main web application, learning content, database models and migrations. |
| `CybersecurityTrainingWeb.Tests/` | Automated tests for application features and access rules. |
| `01_Module Content/` | Source Word documents for the eight learning modules. |
| `02_Question Banks/` | Source Word documents for the assessment question banks. |
| `Demonstration database/` | A separate sample SQLite database and instructions. |
| `Project Documentation/` | Project planning and supporting documentation. |
| `Technical Design/` | Documentation about the application architecture and database design. |
| `Tools/` | Python scripts for preparing assessment data and updating existing module text. |
| `CybersecurityTrainingApplication.slnx` | Solution file for the application and test projects. |

The project’s main application code is in `CybersecurityTrainingWeb`.

## Requirements

To build and run the project, install:

- The **.NET 10 SDK**.
- A compatible **`dotnet-ef`** tool for database migrations.
- An editor or development environment such as Visual Studio or VS Code.

To inspect the SQLite database visually, you can use **DB Browser for
SQLite**. It is useful for viewing tables and records, but is not required
to run the application.

## Setting up and running the application

Open PowerShell in the project folder and run:

```powershell
cd CybersecurityTrainingWeb
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

Then open:

```text
http://localhost:5260
```

The database connection is relative to the web project directory, so run
the database commands from `CybersecurityTrainingWeb`.

If `dotnet ef` is not available, install a compatible version of the
`dotnet-ef` tool and then run the database update again.

The migrations create or update the database schema. The question-bank
seeding process uses `Data/question_banks.json` to add assessment content.
If you already have a database with data you want to keep, make a backup
before applying migrations.

## Database

The application uses a local SQLite database named:

```text
CybersecurityTrainingWeb/cybertraining.db
```

It stores information such as:

- User accounts and roles.
- Module enrolments and ownership assignments.
- Completed learning sections.
- Assessment banks, questions and answer options.
- Assessment attempts, selected answers and results.
- Module schedules.

To inspect the database, open `cybertraining.db` in DB Browser for SQLite.
Use **Database Structure** to view the tables and **Browse Data** to inspect
their records.

The `Demonstration database` folder also contains a separate sample
database. Its README explains what it contains and how to use it.

A working database can contain account details and learner activity. Review
its contents before sharing it, and use appropriate test accounts for
demonstrations.

## Learning and assessment journey

1. A learner registers or signs in.
2. They enrol, or are enrolled, in a module.
3. They read the learning material and mark all seven sections as complete.
4. The module assessment becomes available.
5. They answer the multiple-choice questions and submit their attempt.
6. The application marks the answers on the server and displays the result.

The default pass mark is **90%**. A learner can retry after a failed attempt.
Once they pass the module assessment, further attempts for that module are
locked. The module page shows the learner’s best completed score.

The pass mark can be configured using `Assessment:PassingPercentage` in
`appsettings.json`.

## Assessment feedback

The results page displays the learner’s selected answers and explanations.

For a failed attempt, the intended behaviour is to show whether the
learner’s selected answers were correct or incorrect without highlighting an
unselected correct option. After a passing attempt, the correct options can
also be highlighted.

The team should verify this behaviour in the final build, including an
earlier failed attempt viewed after a later pass.

## Accounts and roles

The application uses ASP.NET Core Identity to manage accounts and roles.

- **Learners** access modules, assessments, progress and their own results.
- **Module owners** can access management functions for their assigned
  modules.
- **Administrators** can access broader training administration functions.

To prepare an administrator account in the Development environment:

1. Register the account in the application.
2. From the `CybersecurityTrainingWeb` folder, configure the account’s email
   using local user secrets:

```powershell
dotnet user-secrets set "AdminBootstrap:Email" "your-registered-test-email"
```

3. Restart the application.

Use the email address of the registered test account. Do not place passwords
or secrets in project files or GitHub.

Two-factor authentication can be enabled through **Account security**. It is
available to users but is not compulsory in the current prototype.

## Training schedules and notifications

Administrators can manage training across modules. Assigned module owners
can manage their own modules.

The management area supports enrolling an existing account and setting an
assessment due date and repeat interval. Dates are shown in UTC.

The notifications page displays a signed-in learner’s assignments and
assessment reminders. These reminders are shown in the application when the
page is opened; background email and push notifications are not implemented.

A schedule provides a reminder. Assessment availability is controlled
separately by the learning-section and passing rules.

## Running the tests

From the project root, run:

```powershell
dotnet build CybersecurityTrainingWeb
dotnet test CybersecurityTrainingWeb.Tests --verbosity minimal
```

The test project includes checks for assessment access, learner isolation,
assessment navigation, question-bank structure, enrolments and management
features. The tests use temporary databases for relevant checks.

Record the actual date and result of the team’s final test run in the
project report. Automated tests and manual demonstrations do not replace a
full security or production-readiness review.

## Maintaining module and assessment content

The `Tools` folder provides developer tools for maintaining content:

- `ParseQuestions.py` prepares question-bank JSON, including answer
  information.
- `ImportQuestionPreviews.py` prepares preview data for an earlier frontend
  workflow.
- `UpdateModuleContent.py` updates an existing module paragraph or bullet in
  `CybersecurityTrainingWeb/Data/module-content.json`. It previews changes by
  default and creates a backup when a change is applied. It does not import
  module Word documents or add or delete modules.

For example, from the project root, list Module 1's content blocks:

```powershell
python Tools/UpdateModuleContent.py --module 1 --list
```

Place replacement text in a UTF-8 file, then preview the update by specifying
the zero-based block number shown in the list:

```powershell
python Tools/UpdateModuleContent.py --module 1 --block 0 --text-file replacement.txt
```

Add `--apply` to that command only after reviewing the preview. The tool
creates a timestamped backup before replacing the JSON. Headings and tables
cannot be edited with this tool because they help define the module layout.
Rebuild or restart the application as needed, then check the module page.

The question-bank scripts require the `python-docx` package and may overwrite
their output JSON, so back up and review the files before running them.
The application seeds question banks into SQLite; changing the JSON alone
does not automatically replace an existing bank with the same module, code
and version. Review the seeding and versioning process and test on a copy
before changing assessment data. The scripts are not needed for normal
day-to-day use of the application.

These tools require access to the project files and command line. The web
interface does not currently let an administrator add, edit or delete module
content, questions or question banks directly.

## Current limitations

This application is a coursework prototype. The current project does not
include:

- Production hosting or production email delivery.
- Background email or push notifications.
- An operational backup process.
- A named multi-module pathway editor.
- An enrolment-removal workflow.
- A mandatory two-factor authentication policy.
- A completed full privacy and security review.


## Project documentation

The `Technical Design` folder contains the system architecture and database
design documentation. The `Project Documentation` folder contains supporting
project material.

The final team report and presentation are separate deliverables. They should
describe the team’s actual contributions, testing results, completed goals,
unfinished work and lessons learned.
