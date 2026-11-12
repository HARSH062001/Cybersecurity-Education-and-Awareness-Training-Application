# Test Scenarios and Results

**Project:** Cybersecurity Education and Awareness Training Application  
**Course:** COMP6900 Computing Project, University of Newcastle  
**Tester:** Harsh Chaudhary  
**Document status:** Team testing record

## 1. Purpose

This document records checks performed on the Cybersecurity Education and
Awareness Training Application. It covers the learner journey, assessments,
account access, management features, database and automated tests.

Harsh reported that the manual scenarios below worked when tested. The
automated test result is supported by the PowerShell output shown on
11 November 2026. The team should add any further test dates, screenshots,
videos or issues to the final project report.

## 2. Test Environment

The application was tested locally using:

- The Cybersecurity Training Application project.
- ASP.NET Core and .NET 10.
- A local SQLite database.
- A web browser for manual application checks.
- PowerShell for the automated test run.
- Test accounts for learner and management workflows.

## 3. Manual Test Scenarios

The following results were reported by Harsh after testing the application.
These are manual checks, separate from the automated tests.

### F01 — Register and sign in

**Expected:** A test learner can create an account and sign in.

**Reported result:** Registration and sign-in worked.

**Status:** Passed.

### F02 — Open the learning modules

**Expected:** The eight modules open and their learning content is available.

**Reported result:** The modules opened successfully.

**Status:** Passed.

### F03 — Complete learning sections

**Expected:** A learner can mark sections complete, and saved progress remains
available after returning.

**Reported result:** Section completion and progress worked as expected.

**Status:** Passed.

### F04 — Check assessment availability

**Expected:** The assessment becomes available after all seven sections of
the module are marked complete.

**Reported result:** The assessment became available after section completion.

**Status:** Passed.

### F05 — Save and resume an assessment

**Expected:** Selected answers remain available when an unfinished assessment
is resumed.

**Reported result:** The attempt and saved answers could be resumed.

**Status:** Passed.

### F06 — Submit a failed assessment

**Expected:** The learner receives a score and explanations and can retry.
An unselected correct option is not highlighted.

**Reported result:** The failed assessment and retry flow worked as expected.

**Status:** Passed.

### F07 — Submit a passing assessment

**Expected:** A score at or above the passing mark is recorded, correct
options can be shown and further attempts are locked.

**Reported result:** The passed result and attempt lock worked as expected.

**Status:** Passed.

### F08 — Review an earlier failed attempt

**Expected:** Feedback for an earlier failed attempt follows the team’s
agreed rule when viewed after a later pass.

**Reported result:** The review behaved as expected.

**Status:** Passed.

### F09 — Check learner data isolation

**Expected:** One learner cannot access another learner’s assessment
attempts or results.

**Reported result:** Learner assessment access was checked and worked as
expected.

**Status:** Passed.

### F10 — Check administrator access

**Expected:** A test administrator can access the appropriate administration
functions.

**Reported result:** Administrator access worked as expected.

**Status:** Passed.

### F11 — Check module-owner access

**Expected:** A module owner can access management functions for assigned
modules.

**Reported result:** Module-owner access worked as expected.

**Status:** Passed.

### F12 — Check enrolments and schedules

**Expected:** An authorised user can enrol a learner and set a schedule. The
learner can see the relevant notification.

**Reported result:** Enrolment, scheduling and notifications worked as
expected.

**Status:** Passed.

### F13 — Check two-factor authentication

**Expected:** A user can enable optional authenticator-based two-factor
authentication and use it during sign-in.

**Reported result:** Two-factor authentication worked as expected.

**Status:** Passed.

### F14 — Check invalid pages and assessment links

**Expected:** An unavailable page or assessment link provides a clear
recovery response.

**Reported result:** Invalid-page and assessment-link handling worked as
expected.

**Status:** Passed.

### F15 — Check responsive layout

**Expected:** Navigation and module content adjust for smaller screen widths.

**Reported result:** The layout and navigation adjusted during the check.

**Status:** Passed.

### F16 — Check setup from a separate copy

**Expected:** A clean project copy can be set up using the README
instructions and can access its database.

**Reported result:** Setup and database access worked as expected.

**Status:** Passed.

## 4. Automated Test Result

The automated test suite was run from the project directory using
PowerShell:

```powershell
dotnet test CybersecurityTrainingWeb.Tests --verbosity minimal
```

The result shown in PowerShell was:

- **Total tests:** 31
- **Passed:** 31
- **Failed:** 0
- **Skipped:** 0
- **Test duration:** 23 seconds
- **Overall build duration shown:** 33.4 seconds
- **Build result:** Succeeded
- **Tester:** Harsh Chaudhary
- **Date recorded:** 11 November 2026

The PowerShell screenshot should be kept with the team’s testing evidence.

An earlier automated test output is also stored in
`Project Documentation/navigation-test-results.log`. That earlier run
reported 31 passed tests in 14 seconds.

## 5. Demonstration Database Check

The separate demonstration database was checked to confirm its structure
and assessment content.

The SQLite integrity check returned **`ok`**, and the foreign-key check found
no violations. At the time of the check, the database contained:

- 24 assessment banks.
- 240 questions.
- 960 answer options.
- No user accounts.
- No assessment attempts or saved answers.
- No section-progress records or module enrolments.

This was a database-content and integrity check. It was not a complete
assessment workflow test.

## 6. Evidence and Team Review

The team can use the following material to support the final report:

- The PowerShell screenshot showing the automated test result.
- `navigation-test-results.log` for the earlier automated run.
- Screenshots or recordings of the learner journey and assessment feedback.
- The project presentation video.
- Additional screenshots or notes from the team’s manual checks.

Before submission, the team should confirm the manual results and add the
dates on which they were checked. The team should also confirm that the
application version tested is the version being submitted and record any
issues found.