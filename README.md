# Cybersecurity Education and Awareness Training Application

COMP6900 Computing Project — University of Newcastle

This repository contains a workplace cybersecurity learning prototype, source learning and question-bank documents, and project documentation. The web application uses ASP.NET Core Razor Pages on .NET 10.

## Current working scope

- Eight modules appear in the catalogue and have seven learning sections each.
- Visitors can browse learning content and try the assessment preview.
- Users can register, sign in, sign out, and edit a profile containing a name, department, and job title. Account security settings use ASP.NET Core Identity.
- Signed-in users can confirm module sections as complete. Section progress is stored in SQLite against their user account and appears in the home page, catalogue, modules, and dashboard. Signed-out visitors show zero progress and cannot save completion. Progress was manually checked with two separate test accounts.
- The navigation badge displays the signed-in user's initials, or `CT` for a visitor.
- A practice assessment preview displays learner-facing questions and options from `CybersecurityTrainingWeb/Data/assessment-preview.json`; the results page confirms the number of responses entered.
- Entity Framework Core migrations create the Identity, profile, section-progress, and assessment-record tables.

The assessment **database schema is only groundwork**. The assessment preview does not yet write attempts to it or award marks.

## Known limitations and remaining work

1. The learner-facing JSON content has not been fully reconciled with the latest module and question-bank Word documents. The source documents must be reviewed before a final content import. Do not assume the preview questions are the approved final banks.
2. Assessment bank choice is currently a frontend preview: it is not assigned and persisted server-side. The current assessment route can accept a bank parameter. Production behaviour should expose one start action and assign a bank server-side.
3. Assessment answers remain in the browser session. There is no server-side answer checking, score, feedback, durable attempt record, or attempt history. The results page is not an official marked result.
4. Correct answers, marking rules, and explanations must remain server-side when the approved banks are imported. Multiple-choice questions can then be marked immediately; free-text answers need an approved rubric and review workflow before a reliable mark can be shown.
5. Roles infrastructure exists, but owner/admin role assignment, management screens, reporting, schedules, retakes, and notifications are not implemented.
6. Account confirmation is required outside Development, but a production email delivery service and deployment configuration are not in place. This prototype is not ready for real learner data or public deployment.
7. Automated test coverage, full accessibility/mobile checks, and final team acceptance testing remain to be completed.

## Run locally

Requirements: .NET 10 SDK. For migrations, install the matching EF Core tool if it is not already available:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.13
```

From the repository root:

```powershell
cd .\CybersecurityTrainingWeb
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

Open `http://localhost:5260`. The database is a local `cybertraining.db` file configured in `appsettings.json`; it is ignored by Git. Run migrations from the **web project folder** so the application and EF tool use the same relative database path. On a new clone, no user accounts or progress will be present. Development mode permits local registration without email confirmation; that is not a production setup.

To validate after changes, stop the running site with Ctrl+C, then run:

```powershell
dotnet build
dotnet ef migrations list
```

Useful routes: `/`, `/Dashboard`, `/Modules`, `/Module?id=1`, `/Assessment?moduleId=1`, `/Profile`, and `/Privacy`.

## Manual smoke test

1. While signed out, confirm that the badge shows `CT` and progress shows zero.
2. Register a local test account, fill in profile details, and complete one learning section. Refresh and confirm its progress remains.
3. Sign out and sign back in; confirm the same account's progress remains.
4. Register a second account and confirm that it starts at zero rather than inheriting the first user's progress.
5. Try an assessment and confirm that the results page **does not** claim to have marked or stored an official result.

Do not use real passwords or personal learner data for this prototype. Never commit local `.db` files, secrets, or test credentials.

## Repository layout

```text
01_Module Content/          Source learning documents
02_Question Banks/          Source question-bank documents
04_Project Documentation/   Planning and project records
05_Presentation/            Presentation material
06_Technical Design/        Architecture and design
CybersecurityTrainingWeb/    Razor Pages application, data, and EF migrations
```

## Next integration steps for the team

1. Agree which Word documents are the approved versions, then reconcile `module-content.json` and the learner-facing assessment data with those documents. Check module titles, section text, questions, and options. Keep answer keys out of learner-facing JSON.
2. Import approved banks, questions, options, answer keys, and scoring rules into protected server-side records; verify all eight modules and three banks per module.
3. Replace the assessment preview with authenticated, persisted bank assignment and submissions. Mark objective questions server-side and store responses, scores, and feedback. Define manual-review rules for any free-text questions.
4. Add results and attempt history, role-based owner/admin workflows, automated tests, and deployment/email configuration. Review privacy and security before using real learners.

Coordinate ownership and review changes with the team. Do not present assessment scoring, persistent attempts, or production readiness as completed features.
