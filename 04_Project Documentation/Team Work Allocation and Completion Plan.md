# Cybersecurity Education and Awareness Training Application

## Team work allocation and completion plan

**Project:** COMP6900 Computing Project  
**Team:** Harsh Chaudhary, Vraj Ajaykumar Chauhan, Ankitkumar Chimanbhai Patel and Samuel John Gabo  
**Prepared:** 21 September 2026

## 1. Agreed responsibility split

### Harsh: frontend, learning experience and content integration

Harsh owns the learner-facing part of the application and coordinates the approved learning content.

Main responsibilities:

1. Build and maintain the Razor Pages frontend.
2. Complete the learner journey for all eight modules:
   - module catalogue;
   - module overview;
   - learning sections;
   - workplace examples;
   - checklist or knowledge check;
   - assessment entry;
   - assessment completion;
   - results and feedback.
3. Use the approved Word module content and references in the web pages.
4. Make the interface responsive for desktop, tablet and mobile screens.
5. Display real progress returned by the backend rather than fixed or invented percentages.
6. Connect the frontend to Ankit and Samuel's agreed API or service methods.
7. Add useful loading, empty, validation and error states.
8. Test navigation and the complete learner journey with realistic data.
9. Keep the AI Literacy and AI Security module educational only. No AI-powered feature is required in this project phase.
10. Review wording, accessibility, consistency and visual quality across the eight modules.

Frontend completion means a learner can open any module, read its content, see their actual progress, start an available assessment, submit answers and view the returned result.

### Ankit and Samuel: backend, database and application services

Ankit and Samuel can divide these items between themselves. They should agree on the database schema and API contracts first, because the frontend depends on those contracts.

#### Database and Entity Framework Core

1. Replace the current sample/seed-only assessment data with the complete data model.
2. Create and test tables/entities for:
   - modules;
   - module sections or learning activities;
   - question banks;
   - questions;
   - answer options;
   - assessment attempts;
   - selected answers;
   - scores and completion status;
   - learner/user identifier;
   - timestamps and audit fields where needed.
3. Add primary keys, foreign keys, required fields, indexes and sensible delete behaviour.
4. Import the planned content: eight modules, three banks per module and ten questions per bank, for 240 questions in total.
5. Make seeding repeatable and safe for development. Do not silently delete real attempts when the application starts.
6. Add migrations or an agreed database-initialisation process so every team member can create the same database locally.
7. Document the database connection string and local setup without committing secrets.

#### Assessment and attempt logic

1. Start an assessment for a selected module.
2. Select the requested bank or select an appropriate different bank for a repeated attempt.
3. Create an attempt record before the learner answers questions.
4. Return the ten questions for that attempt in a stable order.
5. Save each selected answer against the attempt.
6. Prevent submission of an invalid question, option or attempt identifier.
7. Calculate the score on the server from the stored correct answers.
8. Mark the attempt as completed only after valid submission.
9. Save the selected question bank, start time, completion time, score and answer details.
10. Return result data including score, percentage, pass/complete status, bank used and feedback for incorrect answers.
11. Ensure a repeated attempt does not overwrite the previous attempt.
12. Ensure the learner cannot change answers after completion unless the project requirements explicitly allow it.

#### Progress and dashboard data

1. Define what counts as module progress. Recommended initial rule:
   - completed learning sections divided by total learning sections;
   - assessment completion shown separately;
   - module completion only when the required learning content and assessment are complete.
2. Store progress in the database for the authenticated learner once authentication is available.
3. Keep the current anonymous learner cookie only as a development fallback.
4. Return dashboard data for:
   - completed modules;
   - overall progress;
   - latest attempts;
   - best or latest score, according to the agreed rule;
   - next recommended module.
5. Make progress consistent across browsers and devices after login.

#### Backend interface for the frontend

Ankit and Samuel should publish a short API contract before implementation. At minimum, it should define the request and response for:

| Operation | Required result |
|---|---|
| Get module catalogue | Eight modules with title, summary and progress |
| Get module details | Content sections, checklist and assessment availability |
| Get module progress | Completed sections, total sections and percentage |
| Start assessment | Attempt ID, module ID, bank used and ten questions |
| Save answer | Confirmation for the current attempt/question |
| Submit assessment | Score, percentage, completion status and feedback |
| Get attempt/result | A previously saved result and its details |
| Get dashboard | Overall progress, module status and recent attempts |

The contract must state error responses, required identifiers, whether authentication is required, and whether a request can safely be repeated.

#### Validation, security and testing

1. Validate all IDs and submitted answer values on the server.
2. Never trust a score calculated by the browser.
3. Never send correct answers to the browser before submission.
4. Prevent one learner from reading another learner's attempts.
5. Add authentication and role checks with Vraj's security work.
6. Use anti-forgery protection for state-changing requests where applicable.
7. Handle duplicate submissions and refreshes safely.
8. Add unit tests for bank selection, scoring, progress calculation and attempt isolation.
9. Add integration tests for the assessment API and database.
10. Test empty data, invalid IDs, incomplete submissions, repeat attempts and database errors.

## 2. Work that remains outside the frontend/backend split

These items still need an owner or joint decision:

### Authentication, roles, privacy and security controls

Vraj owns authentication, role-based access, privacy, security controls and quality assurance. Ankit and Samuel must coordinate with Vraj on learner identity, authorization rules, database access and secure error handling.

Required outcome:

- a learner can only access their own progress and attempts;
- administrator or trainer access is clearly separated if included;
- passwords and secrets are not stored in source control;
- privacy wording explains what learner data is stored and why;
- security and privacy tests are recorded.

### Content and assessment quality

Harsh owns final content, references, learning outcomes and question wording. The team should jointly review:

- accuracy of all 240 planned questions;
- correct answers and explanations;
- workplace suitability;
- consistent difficulty across Banks A, B and C;
- references for content and questions;
- accessibility and plain employee-focused language.

### Deployment and project evidence

The team still needs to decide where the final application will run and document:

- local setup;
- database setup;
- test evidence;
- screenshots or demonstration evidence;
- GitHub contribution history;
- final presentation and demonstration script;
- known limitations and future scope.

## 3. What is currently completed

The repository currently provides a working .NET web prototype with the following completed items:

- ASP.NET Core Razor Pages solution and web project.
- Main navigation for Home, Dashboard, Modules and Privacy.
- Catalogue page listing all eight modules.
- Imported module content available through `Data/module-content.json`.
- Module 1 redesigned as the current guided-learning reference page.
- Module 1 learning-section progress stored through the current SQLite-backed progress implementation and browser support.
- Module overview and assessment-bank selection screens.
- Basic assessment page and results page prototypes.
- SQLite/Entity Framework Core project wiring and development seed data.
- Existing module Word files, question-bank folders and reference files preserved in the repository structure.
- README and progress documentation describing the current milestone.
- Local project builds successfully when the running development process is stopped before rebuilding.

These items are not yet complete production features:

- the assessment page still contains prototype/hard-coded question behaviour;
- the results page still contains prototype/hard-coded score behaviour;
- the seed data contains sample assessment content rather than all 240 connected questions;
- attempts, answers, scores and selected banks are not yet fully persisted end to end;
- Modules 2–8 do not yet have the same guided progress experience as Module 1;
- authentication and role-based access are not yet integrated;
- the privacy page is still a basic placeholder;
- progress is not yet a complete account-based dashboard across devices;
- automated tests and deployment are still required.

## 4. Remaining work by delivery stage

### Stage 1: Agree the contracts

Owners: Ankit, Samuel, Vraj and Harsh.

- Confirm the database entities and relationships.
- Confirm the progress calculation.
- Confirm the assessment API/request flow.
- Confirm authentication and learner identity requirements.
- Confirm which screens are required for the first demonstration.

### Stage 2: Complete Module 1 end to end

Owners: Ankit and Samuel for backend/database; Harsh for frontend; Vraj for security review.

- Load all Module 1 questions into Banks A, B and C.
- Implement real start, answer, submit and results behaviour.
- Connect the frontend to real backend data.
- Confirm progress changes when sections are completed.
- Test a first attempt and a repeated attempt.
- Record evidence of the working flow.

### Stage 3: Expand the working pattern to Modules 2–8

- Import and verify all remaining question banks.
- Connect all modules to the same assessment and progress services.
- Apply the approved Module 1 frontend design consistently.
- Verify each module's content, references and assessment coverage.

### Stage 4: Security, quality and usability

- Integrate authentication and roles.
- Add authorization and privacy protections.
- Add unit and integration tests.
- Test responsive layouts and accessibility.
- Test invalid submissions, repeated attempts and data isolation.

### Stage 5: Demonstration and submission preparation

- Prepare installation instructions.
- Prepare database setup instructions.
- Capture evidence of the complete learner journey.
- Update the README and progress log.
- Prepare the final presentation, known limitations and future scope.

## 5. Immediate next actions for the meeting

1. Ankit and Samuel agree who will own database/schema work and who will own assessment services, while both review each other's work.
2. They write the assessment and progress API contract before changing the frontend.
3. Harsh supplies the final Module 1 content and question-bank files and confirms the desired learner screens.
4. Vraj confirms the authentication, roles, privacy and security requirements that affect the schema and APIs.
5. The team completes and demonstrates Module 1 end to end before copying the pattern to Modules 2–8.
6. Every completed item is recorded in the progress document and committed to GitHub with a clear message.

## 6. Definition of project completion

The project should not be called complete until the team can demonstrate all of the following:

- all eight modules are available and contain approved learning content;
- all 24 question banks are connected and contain ten verified questions each;
- the application records attempts, answers, scores and selected banks;
- repeated attempts use the agreed bank-selection rule;
- progress and dashboard percentages come from stored activity;
- authentication, roles, privacy and access controls work as required;
- the frontend works on desktop and mobile;
- invalid and unsafe requests are handled securely;
- tests and demonstration evidence are recorded;
- setup, limitations, references and future scope are documented.

## 7. Important project boundary

The AI Literacy and AI Security module teaches employees about responsible AI use, privacy, misinformation, prompt risks and sensitive-data handling. The current application does not need an AI-powered chatbot, AI scoring system, AI content generator or other AI feature. Those items remain future scope unless the team formally changes the project requirements.
