# Backend Team Handoff Letter

## Cybersecurity Education and Awareness Training Application

**To:** Ankitkumar Chimanbhai Patel and Samuel John Gabo  
**From:** Harsh Chaudhary  
**Project:** COMP6900 Computing Project, University of Newcastle

## 1. Purpose of this handoff

Harsh is completing the learner-facing frontend and adding the approved content for all eight cybersecurity training modules. After the updated frontend package is delivered, Ankit and Samuel should begin the backend and database work described below.

The goal is to make this complete learner journey work:

**Login → choose a module → read content → start an assessment → answer ten questions → submit answers → receive a calculated result → save progress and attempt history.**

## 2. Their main responsibility

Ankit and Samuel jointly own the part of the application behind the screens. They must build the database and services that store, validate, calculate and return real application data.

Their backend must:

- store modules, sections, questions, banks, users, attempts, answers, scores and progress;
- provide module and question data to the frontend;
- save learner activity;
- calculate scores on the server;
- support repeated attempts;
- calculate real module and dashboard progress;
- protect learner data;
- provide reliable services for the frontend;
- include tests and local setup instructions.

## 3. Database work

Create database entities for:

1. Users or learners, including role and account status.
2. Training modules, including title, summary and active status.
3. Module sections, including order, content and completion status.
4. Question banks, linked to a module and identified as A, B or C.
5. Questions, including text, type, order, explanation, business impact and reference.
6. Answer options, including the securely stored correct answer.
7. Assessment attempts, including learner, module, bank, status, dates, score and percentage.
8. Selected answers, including attempt, question and selected option.
9. Learning progress, including learner, module, section and completion date.

Database requirements:

- use primary keys, foreign keys, required fields and useful indexes;
- prevent invalid module, bank, question and answer relationships;
- use Entity Framework Core migrations or a clearly documented setup process;
- make development seeding repeatable and safe;
- do not delete previous attempts whenever the application starts;
- never store passwords as plain text;
- never commit passwords, tokens or connection secrets to GitHub.

## 4. Question-bank work

The project requires eight modules, three banks per module and ten questions per bank: 240 planned questions in total.

Ankit and Samuel must:

- import the approved questions into the correct module and bank;
- preserve every answer option, explanation, business impact and reference;
- ensure every bank contains exactly ten questions;
- ensure every question has a valid correct answer;
- check for missing, duplicate or incorrectly assigned questions;
- keep correct answers on the backend and do not reveal them before submission.

The existing project structure is `02_Question Banks`, with Module 1 to Module 8 and Question Bank A, B and C inside each module.

## 5. Assessment functionality

### Starting an assessment

The backend must confirm the module and bank, create a new attempt, select ten questions, record the selected bank and return an attempt ID and questions to the frontend.

### Saving answers

The backend must confirm that the attempt belongs to the learner, that the question belongs to the attempt and that the selected option belongs to the question. It must save the answer and handle refreshes safely.

### Submitting and marking

The backend must load the stored correct answers, compare them with the learner's answers, calculate the score and percentage, save the completed attempt, save answer-level correctness and return the result and feedback.

The browser must not decide the final score. The server must be the trusted source for marking.

### Repeated attempts

Previous attempts must not be overwritten. Each new attempt must record its own bank, answers, score and completion status. Learners must not choose a bank themselves.

The backend assignment rule should be:

1. For a first attempt, randomly select one of Banks A, B or C for that module.
2. For a repeat attempt, randomly select from the banks that learner has not yet used for that module, where one is available.
3. After the learner has used all three banks, randomly select from all three banks for later attempts. Avoid immediately repeating the last assigned bank when another bank is available.
4. Store the assigned bank against the attempt before any questions are returned.
5. Return only the questions for the assigned bank. The frontend should show that an assessment set was assigned automatically, not offer a bank picker.

## 6. Progress and dashboard work

The current frontend must eventually display real progress rather than fixed 0% or 100% values.

The recommended calculation is:

- module learning progress = completed required sections divided by total required sections;
- assessment status = not started, in progress or completed;
- module completion = required learning and assessment completed;
- overall progress = completed modules divided by eight modules.

The backend should return each module's completed sections, total sections, percentage, assessment status, latest result and the learner's overall progress.

## 7. Services required by the frontend

Ankit and Samuel should agree the request and response format with Harsh before implementation. The frontend will need services equivalent to:

| Service | Required result |
|---|---|
| Module catalogue | Eight modules, summaries and real progress |
| Module details | Sections, content, checklist and assessment availability |
| Module progress | Completed sections, total sections and percentage |
| Mark section complete | Saves learner progress |
| Start assessment | Attempt ID, bank used and ten questions |
| Save answer | Saves the selected option |
| Submit assessment | Score, percentage, status and feedback |
| Get result | Stored result and attempt details |
| Dashboard | Overall progress, module status and recent attempts |

Each service should document required inputs, returned fields, validation, authentication requirements and error responses.

## 8. Validation and security

The backend must validate data even if the frontend validates it first. It must:

- reject invalid IDs and invalid answer options;
- prevent one learner accessing another learner's attempts;
- prevent a learner changing another learner's progress;
- prevent browser-submitted scores from being trusted;
- prevent correct answers being exposed before submission;
- handle duplicate submissions safely;
- return safe errors without exposing database details;
- coordinate authentication, roles and privacy controls with Vraj.

## 9. Testing required from the backend team

Test database creation, migrations, module lookup, bank lookup, ten-question selection, correct scoring, incorrect scoring, incomplete submissions, invalid IDs, repeated attempts, bank selection, attempt history, progress calculation, unauthorized access, duplicate submission and database errors.

The main integration test should be:

1. Open Module 1.
2. Complete learning sections.
3. Start an assessment.
4. Answer ten questions.
5. Submit the assessment.
6. Confirm the real score appears.
7. Confirm progress changes.
8. Repeat using another bank.
9. Confirm both attempts remain recorded.

## 10. Suggested division between Ankit and Samuel

### Ankit

- database schema and relationships;
- Entity Framework Core models;
- migrations and seed process;
- importing modules and question banks;
- data-access methods;
- database integrity tests.

### Samuel

- assessment services or endpoints;
- attempt creation and question selection;
- answer saving and server-side marking;
- score, result and feedback responses;
- progress and dashboard services;
- frontend integration tests.

### Joint work

They should jointly agree the service contract, review each other's code, debug integration issues, test repeat attempts and document local setup.

## 11. Work remaining after Harsh completes the frontend package

Completing the frontend does not mean the whole project is finished. The following work will remain:

### Backend and database

- complete the schema and relationships;
- import and verify all 240 questions;
- replace hard-coded assessment data;
- save attempts, answers, selected banks and scores;
- implement real progress and dashboard data;
- connect all backend services to the frontend.

### Authentication and roles

Vraj should lead learner login, manager or administrator login, role-based permissions, password security, privacy controls and authorization tests. Ankit and Samuel must connect these controls to the database and services.

### Frontend integration

Harsh will still need to connect the pages to the real services, remove sample questions and fixed results, display real errors and progress, test authenticated users and fix integration issues.

### Quality assurance

The team must complete unit tests, integration tests, security tests, responsive browser testing, accessibility checks, content review, reference review, all-eight-module testing and repeated-attempt testing.

### Deployment and submission

The team must configure hosting and the production database, keep secrets out of GitHub, verify the deployed application, update the README and technical documentation, record limitations, prepare screenshots and complete the final presentation and demonstration.

## 12. Definition of done for Ankit and Samuel

Their backend work is complete when:

- the database can be created using documented instructions;
- all 240 planned questions are in the correct banks;
- the frontend can request modules and progress;
- a learner can start a ten-question assessment;
- answers are stored correctly;
- the server calculates the real score;
- selected banks and previous attempts are preserved;
- progress comes from stored activity;
- invalid and unauthorized requests are rejected;
- backend tests pass;
- Harsh can demonstrate Module 1 from learning content through results;
- the same process can be applied to Modules 2–8.

## 13. Immediate next actions

1. Harsh delivers the latest frontend and confirms what each screen needs.
2. Ankit and Samuel inspect the current models, seed data and content files.
3. They agree the database schema and service contract.
4. They implement Module 1 end to end first.
5. The team tests Module 1 together.
6. They then apply the working pattern to Modules 2–8.
7. Vraj integrates authentication, roles, privacy and security controls.
8. The team completes final testing, documentation and deployment.

## 14. AI project boundary

The AI Literacy and AI Security module is educational content only. The current project does not require an AI chatbot, AI scoring, AI-generated training content or another AI-powered feature. Those features remain future scope unless the approved project requirements change.
