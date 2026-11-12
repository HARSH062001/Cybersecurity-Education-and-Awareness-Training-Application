# Cybersecurity Training Web Application

This folder contains the main web application for the COMP6900
Cybersecurity Education and Awareness Training project.

The application provides workplace cybersecurity learning modules,
multiple-choice assessments, progress tracking and training-management
features.

It is built using ASP.NET Core Razor Pages, ASP.NET Core Identity,
Entity Framework Core and SQLite.

## Folder contents

### `Areas/`

Contains application areas, including files associated with ASP.NET Core
Identity account pages.

### `Data/`

Contains the application’s database context, data models, endpoints and
content files.

Important files include:

- `ApplicationDbContext.cs` — defines the database context and
  relationships.
- `ApplicationUser.cs` — extends the user account with profile fields.
- `module-content.json` — supplies the learning module content.
- `question_banks.json` — supplies assessment questions, options and
  answer information for database seeding.
- `QuestionBankSeeder.cs` — helps add question-bank content to the database.
- `ProgressEndpoints.cs` — handles learning-section progress requests.
- `AssessmentSubmissionEndpoints.cs` — handles assessment answers and
  submissions.

### `Migrations/`

Contains Entity Framework Core migration files. These describe changes to
the SQLite database structure and are used when creating or updating the
database.

Keep these files with the project.

### `Pages/`

Contains the Razor Pages that make up the application, together with their
server-side page models.

Examples include:

- `Index` — home page.
- `Modules` — learning module catalogue.
- `Module` — individual module and section progress.
- `Assessment` — assessment questions and attempt workflow.
- `Results` — scores and question feedback.
- `AssessmentHistory` — previous and unfinished assessment attempts.
- `Dashboard` — learner progress overview.
- `TrainingManagement` — enrolments and schedules for authorised users.
- `Notifications` — in-app training reminders.
- `AdminProgress`, `AdminModuleOwners` and `OwnerDashboard` —
  administration and module-owner functions.
- `Profile` — learner profile.
- `Status` — recovery page for unavailable pages and links.

The `Pages/Shared/` folder contains shared layout and navigation files.

### `Properties/`

Contains local launch settings, including the HTTP profile used when
running the application for development.

### `Utils/`

Contains helper logic used by application features. For example:

- `AttemptUtils.cs` supports assessment attempt and bank-selection logic.
- `ResultUtils.cs` contains assessment-result and passing rules.

### `wwwroot/`

Contains files served to the browser:

- `css/` — stylesheets.
- `js/` — client-side scripts.
- `lib/` — third-party libraries.
- `favicon.ico` — the application icon.

Keep the third-party libraries and their licence files with the application
unless the project is configured and tested to obtain them another way.

### `bin/` and `obj/`

These folders contain files generated when the project is built. They can
be recreated by .NET and are not normally needed in a clean source-code
submission.

### `.vscode/`

Contains local Visual Studio Code settings. It is not required for the
application to run.

## Important files

### `Program.cs`

Configures and starts the application. It sets up services such as the
database, authentication, Identity, routes and application middleware.

### `CybersecurityTrainingWeb.csproj`

Defines the web project, its target framework, dependencies and build
settings.

### `appsettings.json`

Contains application configuration, including the database connection and
assessment passing percentage.

### `appsettings.Development.json`

Contains settings used for the Development environment.

## Database files

### `cybertraining.db`

This is the application's local SQLite database. It can contain accounts,
roles, enrolments, learning progress, assessment records and schedules.

The team should review the contents of any database before sharing it and
use appropriate test accounts for demonstrations.

### `cybertraining.before-*.db`

These are earlier database backups created during development. They can be
kept for recovery or project evidence, but they are separate from the
current working database.

### Files ending in `-wal` and `-shm`

SQLite may create these supporting files while a database is open. The
`-wal` file can contain changes that have not yet been written into the
main database file.

Do not delete or move these files independently while the database is open.
For a submission copy, close the application and database tools first, and
use a SQLite-aware backup or export so the copied database includes its
latest changes.

## Running the application

Install the .NET 10 SDK and a compatible `dotnet-ef` tool. Then open
PowerShell in this `CybersecurityTrainingWeb` folder and run:

```powershell
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

Open the following address in a browser:

```text
http://localhost:5260
```

The database connection is relative to the web project directory. If you
already have a database with data you need, back it up before applying
migrations.

For full setup instructions, see the `README.md` in the main project folder.

## Assessments

A learner completes the seven sections in a module before starting its
assessment. The server saves and marks the multiple-choice answers.

The default passing mark is **90%**. Learners can retry a failed assessment.
After a learner passes, further attempts for that module are locked.

The team should test the final feedback behaviour before submission:
failed attempts should show the learner’s selections and explanations
without highlighting an unselected correct option. A passing attempt can
show the correct options.

## Running automated tests

The tests are in the separate `CybersecurityTrainingWeb.Tests` folder.
From the **main project folder**, run:

```powershell
dotnet test CybersecurityTrainingWeb.Tests --verbosity minimal
```

Record the date and actual result of the test run in the team’s testing
documentation.

## Current scope

This is a coursework prototype. It uses a local SQLite database and
in-app notifications. Background email or push notifications, production
hosting, operational backups and a full privacy and security review are
not completed as part of the verified prototype.