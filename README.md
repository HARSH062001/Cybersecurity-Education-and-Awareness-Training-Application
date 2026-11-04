# Cybersecurity Education and Awareness Training Application

COMP6900 coursework prototype — University of Newcastle.

## Stack and features

ASP.NET Core Razor Pages (.NET 10), EF Core, SQLite and ASP.NET Core Identity.

- Eight learning modules with seven sections each; account-specific saved progress.
- Registration, profiles, sign-in, authenticator-app 2FA via Account security.
- Learner self-enrolment and owner/admin enrolment into individual modules. Assign several modules to construct a learner pathway; there is no separate named pathway editor.
- Database-backed question banks, server-assigned attempts, server-side objective marking, saved responses, results, feedback and account-specific history.
- Admin progress reporting and module-owner assignment; owner reporting limited to assigned modules.
- Manage training: admins manage all modules; module owners manage only their assigned modules. Enrol existing accounts by email and set a module's first assessment due date and repeat interval.
- Notifications: signed-in learners see their own enrolments, due dates, overdue reminders and completion status. These are computed when the page opens; no background email, push delivery or read/unread inbox is implemented.

## Setup

Install the .NET 10 SDK and a compatible dotnet-ef tool (10.0.13 was used locally).

```powershell
cd CybersecurityTrainingWeb
dotnet restore
dotnet ef database update
dotnet run --launch-profile http
```

Open http://localhost:5260. Run EF commands from the web project folder: the database connection is relative to that directory. Migrations create the schema, and EF's configured seeder imports Data/question_banks.json. Existing bank module/code/version combinations are skipped; publish a new version to preserve historical banks rather than silently changing existing keys. Back up an existing SQLite database before applying migrations.

Development permits registration without email confirmation. Other environments require confirmation; configure an email provider before deploying. Accounts and local database files are not included in source control.

## Roles and account security

Register a local test account first. To bootstrap an administrator in Development, configure AdminBootstrap:Email using local user secrets, then restart. Use the registered account's email; never commit passwords or secrets. Admins can still enrol in and complete learner training.

```powershell
dotnet user-secrets set "AdminBootstrap:Email" "your-registered-test-email"
```

The admin Module owners page assigns owners to modules. Owners use Owner dashboard for reports and Manage training for enrolment and scheduling. Learners enable authenticator 2FA through Account security. 2FA is available but is not mandatory for every account.

## Scheduling rules

All entered and displayed deadlines use UTC. A repeat interval of 0 is a one-time assessment; 1–3650 sets recurring days. Before the first completed assessment the first due date applies. Afterwards the next deadline is the latest completed submission plus the repeat interval. One-time schedules show completed after a completed attempt. Existing completed attempts count. Awaiting-review attempts do not count as completed.

Schedules provide deadlines and reminders; they do not lock assessment availability or prevent early practice/retakes. Updating a module schedule affects all currently enrolled learners and appears on their next Notifications visit. Enrolment is idempotent. Ownership is rechecked server-side for each management submission, and forms use Razor Pages antiforgery protection.

## Verification

From the repository root:

```powershell
dotnet build CybersecurityTrainingWeb
dotnet test CybersecurityTrainingWeb.Tests --verbosity minimal
```

Stop the running local app before building if Windows reports locked output files. Tests use temporary databases for assessment history and management checks. Coverage includes signed-out assessment access, learner isolation, owner scope, enrolment duplication, schedule updates, notification isolation and question-bank validation; it is not exhaustive security assurance.

## Demonstration checklist

1. Register two disposable learner accounts; enable authenticator 2FA on one and demonstrate sign-in.
2. Self-enrol, complete a section, refresh and sign in again; confirm saved progress and isolation between accounts.
3. Complete an assessment; inspect feedback and history. Confirm another learner cannot retrieve its result.
4. Bootstrap an admin and assign a module owner. Confirm the owner sees only assigned-module reports.
5. As owner, use Manage training to enrol the second learner into an assigned module. Repeat enrolment and confirm one row remains.
6. Save a first due date in the past and a repeat interval of seven days. Sign in as the assigned learner; confirm Notifications shows overdue and links to study/assessment.
7. Complete the assessment; refresh Notifications and confirm the recurring due date changes. Save a zero interval and confirm one-time completion status.
8. Verify an unassigned owner cannot manage another module and an ordinary learner cannot access Manage training. Confirm admins can manage all modules and still use learner features.
9. Check phone-width layouts, keyboard navigation and validation messages; record final team acceptance.

## Remaining limitations

- The project author reports checking the eight module documents and 24 source question banks against the app content and answer keys. Structural tests do not replace that content review.
- Written responses need an approved marking rubric and a complete review workflow before marks can be treated as final.
- No named pathway editor, enrolment removal workflow, background notifications, mandatory 2FA policy, pass threshold or retake enforcement.
- The project author reports manually checking the application flows, including schedules, notifications and assessment results. Capture final demonstration evidence for the report.
- Hosting, production email, operational backups and privacy/security review remain outstanding. This is a coursework prototype, not a production-ready service.

The detailed project report is prepared separately. Do not commit local databases, demo passwords, personal learner data or unrelated resume files.
