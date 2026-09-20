# Phase 2 Application Foundation

## Purpose

This document defines the first development and integration foundation for the Cybersecurity Education and Awareness Training Application.

## Recommended application stack

- ASP.NET Core MVC or Razor Pages
- C#
- ASP.NET Core Identity for users, passwords, roles and two-factor authentication
- Entity Framework Core for database access
- SQL Server Express or LocalDB for development
- Bootstrap and HTML/CSS for responsive pages

## First vertical slice

The first working slice should follow this sequence:

```text
Login -> Dashboard -> Module 1 -> Question Bank A -> Assessment -> Score -> Saved Attempt
```

After this slice works, add repeated-attempt bank selection and then connect Modules 2 to 8.

## Application layers

### Presentation layer

Responsibilities:

- Login and account pages
- Role-aware navigation
- User dashboard
- Module list and module content pages
- Assessment question and answer pages
- Results and feedback pages
- Module Owner management pages
- Administrator pages

### Application layer

Responsibilities:

- Module and content retrieval
- Question-bank selection
- Assessment validation
- Score calculation
- Attempt and answer recording
- Progress calculation
- Role and permission checks
- Schedule and notification logic

### Data layer

Responsibilities:

- Entity Framework Core entities and relationships
- Database migrations
- Repository or service access where required
- Transactional saving of attempts, answers and results
- Seed data for roles, modules, banks and test users

## Initial database entities

- ApplicationUser
- ApplicationRole
- TrainingModule
- QuestionBank
- Question
- AnswerOption
- AssessmentAttempt
- UserAnswer
- AssessmentResult
- LearningPathway
- PathwayModule
- TrainingSchedule
- Notification

## Core relationships

- One TrainingModule has many QuestionBanks.
- One QuestionBank has many Questions.
- One Question has many AnswerOptions.
- One ApplicationUser has many AssessmentAttempts.
- One AssessmentAttempt uses one TrainingModule and one QuestionBank.
- One AssessmentAttempt has many UserAnswers.
- One AssessmentAttempt has one AssessmentResult.
- One LearningPathway has many PathwayModules.
- One TrainingSchedule can target a module or pathway.

## Question-bank selection rule

1. Find the available banks for the selected module.
2. Find the banks used by the user’s previous completed attempts for that module.
3. Prefer an unused bank.
4. If every bank has been used, select a bank according to the agreed repeat policy.
5. Save the selected QuestionBankId on AssessmentAttempt before answers are recorded.
6. Never change the selected bank after the attempt starts.

## Phase 2 acceptance checks

- The solution builds successfully on a machine with the approved .NET SDK.
- A test user can log in.
- The dashboard displays available modules.
- Module 1 content is displayed from the database.
- Question Bank A can produce an assessment.
- The system calculates a score.
- Answers and the selected bank are saved.
- The result page displays feedback.
- A second attempt can use another available bank.
- A user cannot access Module Owner or Administrator functions.

## Team handoff

- Ankit should implement the entities, relationships, migrations and selection service.
- Vraj should define authentication, role, privacy and security test cases.
- Samuel should implement the first vertical-slice pages using seeded Module 1 data.
- Harsh should review module content, references, question quality and security accuracy.

## Current environment note

The shared repository contains the learning content and project documentation. The development computer used for this foundation check did not have a usable .NET SDK installed. The team must install and agree on the SDK version before creating the solution and migrations.
