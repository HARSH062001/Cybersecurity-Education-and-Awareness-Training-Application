# Cybersecurity Education and Awareness Training Application

COMP6900 Computing Project — University of Newcastle

**Progress snapshot:** 20 September 2026, 11:00 pm (Australia/Sydney)

## Project summary

This repository contains the learning content, assessment-question documents, references, technical design and working ASP.NET Core prototype for a web-based cybersecurity education and awareness training application.

The application is designed for employees rather than cybersecurity specialists. It teaches practical workplace behaviours, provides assessments and is intended to retain module progress, question-bank selection, answers, scores and attempt history.

## Team

- Harsh Chaudhary
- Vraj Ajaykumar Chauhan
- Ankitkumar Chimanbhai Patel
- Samuel John Gabo

### Current responsibility allocation

| Team member | Primary responsibilities |
| --- | --- |
| Harsh Chaudhary | Eight-module learning content, workplace and business-impact examples, employee behaviours, references, 24 question-bank documents, cybersecurity review and project coordination support |
| Vraj Ajaykumar Chauhan | Authentication, role-based access, security controls, privacy, testing and quality assurance |
| Ankitkumar Chimanbhai Patel | Database, SQL, Entity Framework Core, backend logic, question-bank selection, attempts, answers, scores and results |
| Samuel John Gabo | Frontend, dashboards, module pages, assessment and result pages, responsive design and accessibility |

All team members will contribute to integration, peer review, testing, documentation, meetings and the final presentation.

## Planned learning scope

The project contains eight modules:

1. Cybersecurity Fundamentals
2. Phishing and Social Engineering
3. Password and Identity Security
4. Data Security and Privacy
5. Network and Internet Security
6. Device and Physical Security
7. AI Literacy and AI Security
8. Incident Reporting and Cyber Hygiene

Every module has three planned question banks—A, B and C—with ten questions in each bank:

```text
8 modules × 3 banks × 10 questions = 240 planned questions
```

The question documents use multiple-choice and workplace-scenario questions. Each question includes the correct answer, explanation, business impact and reference.

## AI scope boundary

Module 7 teaches AI literacy and AI security, including responsible use, sensitive-information risks, inaccurate outputs, bias, deepfakes, copyright and human review.

The current application does **not** implement AI-powered functionality. Chatbots, generative features, AI recommendations, automated AI marking, predictive analytics and machine-learning models remain future scope only.

## Completed work as of 11:00 pm

### Learning and assessment content

- Eight complete module Word documents.
- Module purpose, importance, outcomes, key terms and main topics.
- Employee-focused topic explanations.
- Workplace and business-impact examples.
- Safe employee behaviours and security checklists.
- Assessment coverage, summaries and supporting references.
- Three Word question-bank documents for each module.
- Ten planned questions in every bank and 240 questions overall.
- Separate reference documents for Modules 2–8; Module 1 references are included in its module document and imported web content.

### Application foundation

- .NET 10 solution and ASP.NET Core Razor Pages web project.
- Entity Framework Core with a local SQLite development database.
- Bootstrap-based responsive frontend.
- Home, dashboard, module catalogue, module, assessment-prototype, results-prototype and privacy pages.
- All eight module documents imported into structured application data.
- All eight modules available as web content previews.
- Initial entities for modules, question banks, questions and answer options.
- Initial technical-design document and SQL schema draft.

### Finalised Module 1 vertical slice

Module 1 — Cybersecurity Fundamentals is the approved design candidate for team review. It currently includes:

- A guided seven-section learning layout.
- Clear section navigation and readable employee-level content.
- Key-term, business-impact and employee-checklist tables.
- Explicit **Mark section complete** controls.
- A percentage calculated from confirmed sections rather than an estimated or decorative value.
- Consistent progress on the Module 1 page, module catalogue and dashboard.
- SQLite persistence for module, learner, section and completion timestamp.
- An anonymous browser identifier until authentication is connected.
- Local browser storage as a temporary resilience fallback.
- Complete/undo behaviour and progress retention after refresh.
- Responsive desktop and mobile styling.

The seven-section completion rule is:

```text
completed sections ÷ 7 × 100
```

For example, one completed section displays 14%, and all seven sections display 100%.

## Current architecture

```text
Browser
  ├─ Razor Pages user interface
  ├─ Bootstrap and project CSS
  └─ Module progress JavaScript
          │
          ▼
ASP.NET Core application
  ├─ Module-content reader
  ├─ Progress GET/POST endpoints
  └─ Entity Framework Core
          │
          ▼
SQLite development database
  ├─ Training modules
  ├─ Question-bank prototype entities
  └─ Module section progress
```

Authentication is not connected yet. The temporary learner cookie will later be replaced or associated with the authenticated user ID.

## Repository structure

```text
01_Module Content/
  Module 1 – Cybersecurity Fundamentals.docx
  ...
  Module 8 – Incident Reporting and Cyber Hygiene.docx

02_Question Banks/
  Module 1/
    Question Bank A.docx
    Question Bank B.docx
    Question Bank C.docx
  ...
  Module 8/

03_References/
  Module-specific reference documents

06_Technical Design/
  Phase 2 Application Foundation.md
  phase2_initial_schema.sql

CybersecurityTrainingApplication.Web/
  Data/                    Entity Framework context, seed data and imported module content
  Models/                  Application data models
  Pages/                   Razor Pages frontend
  wwwroot/css/             Application styling
  wwwroot/js/              Module progress and site JavaScript
  Program.cs               Application startup and progress endpoints

CybersecurityTrainingApplication.slnx
README.md
```

Build output, local database files and user-specific development settings are excluded through `.gitignore` and should not be uploaded.

## Running the application

### Requirements

- .NET SDK 10.0 or a compatible SDK specified by the team.
- Git for cloning and collaboration.
- Visual Studio, VS Code or another compatible editor.

### Commands

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet run --project .\CybersecurityTrainingApplication.Web --no-launch-profile --urls http://localhost:5090
```

Then open:

```text
http://localhost:5090/
```

Module 1 is available at:

```text
http://localhost:5090/Module?id=1
```

### Common build issue

If the build reports `MSB3026`, `MSB3027` or `MSB3021`, the website is probably already running and Windows has locked the application executable. Press `Ctrl+C` in the terminal running the website, then run `dotnet build` again.

## Current database and progress behaviour

Module 1 progress is stored in `ModuleSectionProgress` with:

- Learner key
- Module ID
- Section key
- Completion timestamp

The development application exposes same-origin progress endpoints for reading and updating Module 1 section completion. These endpoints are prototype infrastructure and must be connected to authentication and reviewed for production security during backend integration.

The local SQLite database is intentionally not committed. Each developer creates their own development database when the application starts.

## Current limitations and unfinished work

The following items are **not complete** and must not be presented as finished:

- User registration and sign-in.
- Multi-factor authentication.
- User, Module Owner and Administrator role enforcement.
- Production privacy and security controls.
- Importing all 240 Word-document questions into the database.
- Fully functional assessment submission and marking.
- Repeated-attempt question-bank selection.
- Recording assessment answers, scores, selected banks and attempt history.
- Connecting assessment completion to overall module completion rules.
- Applying the final Module 1 learning design to Modules 2–8.
- Module-owner and administrator management pages.
- Production database configuration and formal migrations.
- Automated, security, accessibility and full regression testing.
- Deployment to a production hosting environment.

The existing assessment and result pages are interface prototypes only.

## Development phases

### Phase 1 — Content, scope and planning

Status: substantially complete, subject to team and academic review.

- Defined the eight-module scope.
- Prepared the learning content and references.
- Prepared three ten-question banks per module.
- Defined roles, responsibilities and assessment requirements.
- Created the shared GitHub repository.
- Prepared the initial technical design and schema draft.

### Phase 2 — Application development and integration

Status: in progress.

Completed foundation:

- .NET solution and Razor Pages application.
- Entity Framework Core and SQLite development setup.
- Module catalogue, dashboard and content pages.
- Web content for all eight modules.
- Finalised Module 1 design candidate.
- Working section-level Module 1 progress persistence.

Next team-approved work:

1. Review and approve the Module 1 learning experience.
2. Confirm the progress data model with the backend owner.
3. Implement authentication and role-based access.
4. Import and connect Module 1 question banks.
5. Record attempts, selected bank, answers, score and result.
6. Test the complete Module 1 vertical slice.
7. Apply the approved design and data pattern to Modules 2–8.

Target vertical slice:

```text
Login → Dashboard → Module 1 → Assessment → Score → Saved Result → Retake
```

### Phase 3 — Testing and final delivery

Status: not started.

This phase will include functional, role, security, privacy, responsive, accessibility and regression testing; defect correction; contribution records; technical documentation; presentation preparation; individual reflections; unfinished-feature disclosure; and final source-code/database packaging.

## Git and collaboration workflow

Recommended team process:

1. Pull the latest `main` branch before starting work.
2. Create a separate feature branch for each team task.
3. Keep commits focused and describe the completed change.
4. Do not commit local databases, build folders, passwords, tokens or secrets.
5. Open a pull request for peer review before merging major work.
6. Record decisions, screenshots, test results and contribution evidence.
7. Resolve overlapping changes with the responsible team member rather than overwriting their work.

## Evidence to retain

- GitHub commits and pull requests.
- Project-board history and task assignments.
- Weekly meeting notes and decisions.
- Gantt-chart updates.
- Individual contribution records.
- Module and question-bank review records.
- Architecture and database diagrams.
- Interface screenshots.
- Test cases, results and defect records.
- Integration issues and resolutions.
- Unfinished work, limitations and reasons.
- A final section titled **Mistakes Made and Lessons Learned — What We Would Do Differently**.

## Security and academic-quality notes

- Use fictional users and test data during development.
- Never commit passwords, API keys, tokens or private connection details.
- Review generated learning content and references before submission.
- Confirm current guidance against authoritative sources.
- Maintain privacy, accessibility and secure-development evidence.
- Keep AI-powered application functionality outside the current implementation.
- Clearly distinguish completed features from interface prototypes and future scope.
