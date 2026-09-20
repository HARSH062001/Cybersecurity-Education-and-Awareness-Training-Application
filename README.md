# Cybersecurity Education and Awareness Training Application

COMP6900 Computing Project at the University of Newcastle.

## Project overview

This project is a web-based cybersecurity education and awareness training application. It is designed to help employees or students complete cybersecurity learning modules, answer assessments, receive feedback and maintain a record of learning progress.

The planned application has three main roles:

- User: completes modules and assessments and views progress.
- Module Owner: manages modules, learning content, questions, question banks, pathways and schedules.
- Administrator: manages accounts, access permissions and platform-level settings.

## Current learning content

The project contains eight cybersecurity training modules:

1. Cybersecurity Fundamentals
2. Phishing and Social Engineering
3. Password and Identity Security
4. Data Security and Privacy
5. Network and Internet Security
6. Device and Physical Security
7. AI Literacy and AI Security
8. Incident Reporting and Cyber Hygiene

Each module is planned to contain three assessment question banks. Each bank contains ten questions. The full assessment design therefore contains 8 x 3 x 10 = 240 planned questions.

The question banks contain a mixture of multiple-choice and workplace scenario questions. Each question includes the correct answer, an explanation, a business-impact explanation and a supporting reference.

## AI boundary

The AI Literacy and AI Security module teaches employees about responsible AI use, privacy risks, inaccurate outputs, sensitive information, deepfakes, bias, copyright and human review.

The current application does not implement AI-powered functionality. The following features are outside the current scope:

- Chatbots
- Generative AI features inside the application
- Machine-learning models
- AI-powered recommendations
- Automated AI assessment marking
- Predictive analytics

These may be considered as future enhancements after the core application is complete and tested.

## Repository structure

```text
01_Module Content/
  Module 1 to Module 8 Word documents

02_Question Banks/
  Module 1 to Module 8/
    Question Bank A/
    Question Bank B/
    Question Bank C/

03_References/
  Module-specific reference documents

04_Project Documentation/
  Scope, plans, meeting notes, testing records and final documentation

05_Presentation/
  Presentation drafts and final presentation material

06_Technical Design/
  Architecture, database, user journeys and design documentation
```

## Phase 1 Planning and Foundation

Phase 1 establishes the project foundation. The expected work includes:

- Confirming the project problem, target audience and purpose.
- Confirming the eight-module scope.
- Defining the User, Module Owner and Administrator roles.
- Defining the three-bank assessment model.
- Planning 240 questions.
- Defining the database entities and relationships.
- Designing the main user journeys.
- Selecting the technology stack.
- Preparing basic interface designs.
- Creating the GitHub repository and project board.
- Agreeing team responsibilities and communication arrangements.
- Building an initial prototype workflow.

### Phase 1 status

Completed or prepared in this repository:

- Eight module content documents.
- Three question banks for every module.
- 240 planned questions.
- Module reference documents.
- Learning outcomes, key terms, workplace examples and business impacts.
- Assessment coverage and Phase 1 planning evidence in module documents.

Still to confirm with the team:

- Final ASP.NET Core project configuration.
- Database schema and relationships.
- User-journey diagrams.
- Interface wireframes.
- Final task board and Gantt chart.
- Initial prototype acceptance criteria.

## Phase 2 Development and Integration

Phase 2 builds and connects the working application.

### Phase 2 steps

1. Create the ASP.NET Core solution.
2. Configure the shared GitHub development workflow.
3. Configure Entity Framework Core and SQL Server Express or LocalDB.
4. Implement authentication and two-factor authentication.
5. Implement role-based access for Users, Module Owners and Administrators.
6. Create database entities for users, roles, modules, question banks, questions, answer options, attempts, answers, scores, pathways, schedules and notifications.
7. Create the first database migration.
8. Build the login and dashboard pages.
9. Build the module content page.
10. Build the assessment page.
11. Implement scoring and feedback.
12. Implement question-bank selection for repeated attempts.
13. Save selected banks, answers, scores and attempt history.
14. Connect all eight modules and their question banks.
15. Add basic progress tracking.
16. Add module-owner and administrator management pages.
17. Integrate frontend, application logic and database.
18. Record screenshots, commits, decisions and integration issues.

### Phase 2 completion criteria

Phase 2 is complete when the team can demonstrate:

```text
Login -> Dashboard -> Module -> Assessment -> Score -> Saved Result -> Retake
```

The system should record the selected question bank, answers, score and result for every attempt. A repeated attempt should use a different bank where one is available.

## Phase 3 Testing and Final Delivery

Phase 3 prepares the final demonstration and submission.

### Phase 3 steps

- Test registration, login and two-factor authentication.
- Test role-based access and blocked unauthorised actions.
- Test all eight modules.
- Confirm three banks and ten questions per bank.
- Test question-bank selection for repeated attempts.
- Test scoring, feedback and saved results.
- Test progress and completion status.
- Test module-owner and administrator functions.
- Test validation, error handling and privacy controls.
- Test responsive layouts on common screen sizes.
- Fix defects and repeat regression testing.
- Update the Gantt chart and contribution records.
- Complete meeting records and individual reflections.
- Complete technical documentation and setup instructions.
- Document unfinished features and limitations.
- Include a section titled `Mistakes Made and Lessons Learned - What we would do differently`.
- Rehearse the final presentation and demonstration.
- Prepare the source code, database files, project report and presentation for submission.

## Recommended technology stack

- ASP.NET Core MVC or Razor Pages
- C#
- Entity Framework Core
- SQL Server Express or LocalDB
- ASP.NET Core Identity
- HTML, CSS and Bootstrap
- Visual Studio
- Git and GitHub

This stack supports the planned modules, assessments, role-based access, database storage and responsive web interface.

## Team responsibilities

### Harsh Chaudhary

- Module content and learning outcomes.
- Question-bank content and references.
- Cybersecurity accuracy review.
- Security, privacy and ethics requirements.
- Project coordination and documentation support.

### Vraj Ajaykumar Chauhan

- Authentication and role-based access review.
- Security controls and privacy requirements.
- Functional, security and quality-assurance testing.
- Defect records and verification.

### Ankitkumar Chimanbhai Patel

- Database design and SQL.
- Entity Framework Core.
- Backend and .NET logic.
- Question-bank selection.
- Attempts, answers, scores and results.

### Samuel John Gabo

- Frontend and responsive design.
- Dashboards and module pages.
- Assessment and results pages.
- User experience and accessibility.

All members should contribute to integration, peer review, testing, meetings, documentation and the final presentation.

## Progress evidence

The team should retain:

- GitHub commits and pull requests.
- Project-board task history.
- Weekly meeting notes.
- Gantt-chart updates.
- Individual contribution records.
- Module and question-bank review records.
- Database diagrams and interface designs.
- Test cases and test results.
- Screenshots of working features.
- A record of unfinished work and reasons.

## Initial implementation target

The first working vertical slice should be:

```text
Login -> Module 1 -> Question Bank A -> Assessment -> Score -> Saved Attempt
```

After this flow works, the team should add repeated-attempt bank selection and then connect Modules 2 to 8.

## Important project notes

- Use test accounts and fictional training data.
- Do not commit passwords, API keys, connection strings or other secrets.
- Review all generated content and references before final submission.
- Keep the current project focused on educational content and core training functionality.
- AI-powered application features remain future scope.
