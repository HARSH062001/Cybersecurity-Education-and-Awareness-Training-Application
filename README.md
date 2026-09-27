# Cybersecurity Education and Awareness Training Application

COMP6900 Computing Project — University of Newcastle

## What this project is

A team-built ASP.NET Core Razor Pages prototype for practical cybersecurity awareness training in the workplace. The repository contains the learning materials, references, question-bank documents, technical documentation and the current web application.

The learning pathway is planned to contain eight modules. The current website enables **Module 1: Cybersecurity Fundamentals** and **Module 2: Phishing and Social Engineering**. The other six modules will be added as team members provide and review their content and question banks.

## Current website features

- Cybersecurity-themed home page, learner dashboard and module catalogue.
- Module 1 and Module 2 pages, each organised into seven learning sections.
- Section completion controls and progress summaries.
- Assessment preview with ten questions, automatic A/B/C bank assignment, question navigation and answer selection.
- Results confirmation page that reports how many responses were entered.
- Responsive layout and browser-side progress/assessment state.

## Important prototype limitations

This version is a frontend prototype, not a production learning-management system:

- Module progress is saved in the current browser using local storage; it is not linked to an account or shared across devices.
- Assessment answers are held in the browser for the current session. The results page does not calculate a score.
- Bank selection is a frontend preview. Server-side bank history, marking, explanations and durable attempt/result storage are not implemented.
- Login, user roles, database persistence and deployment are not included in this frontend version.
- The website currently exposes only Modules 1 and 2. Do not treat the other six planned modules as available until their content has been integrated and tested.

## Team

- Harsh Chaudhary — current website contribution and Modules 1–2 content/question banks.
- Vraj Ajaykumar Chauhan
- Ankitkumar Chimanbhai Patel
- Samuel John Gabo

Each team member is contributing two modules under the team’s agreed allocation. Please coordinate module ownership and integration with the group before changing shared files. Other members’ modules and question banks should be added when supplied and reviewed.

## Run the website locally

### Requirements

- .NET 10 SDK.
- Visual Studio or Visual Studio Code (recommended).
- Git to clone and collaborate on the repository.

Check that .NET is available in PowerShell:

```powershell
dotnet --version
```

From the repository root, restore, build and run:

```powershell
dotnet restore .\CybersecurityTrainingApplication.slnx
dotnet build .\CybersecurityTrainingApplication.slnx
dotnet run --project .\CybersecurityTrainingWeb\CybersecurityTrainingWeb.csproj
```

Open the local address printed in the terminal. Useful routes are:

```text
/
/Dashboard
/Modules
/Module?id=1
/Module?id=2
/Assessment?moduleId=1
/Assessment?moduleId=2
```

If a build reports that files are locked, stop any running copy of the website with **Ctrl+C** in its terminal and build again.

## Updating question-bank preview data

After reviewed question-bank Word files change, regenerate the learner-facing preview from the repository root:

```powershell
python -m pip install python-docx
python .\Tools\ImportQuestionPreviews.py
```

The importer writes question text and answer options to `CybersecurityTrainingWeb/Data/assessment-preview.json`. Correct answers, explanations, business impacts and references remain in the source Word documents and are not included in the learner preview.

## Repository layout

```text
01_Module Content/          Module learning documents
02_Question Banks/          Question Bank A, B and C documents by module
03_References/              Source/reference documents
04_Project Documentation/   Project planning and records
05_Presentation/             Presentation material
06_Technical Design/         Architecture and technical design
CybersecurityTrainingWeb/    Current ASP.NET Core Razor Pages website
CybersecurityTrainingApplication.slnx
README.md
```

## Adding the remaining modules

When a team member submits a module, integrate its reviewed learning content and its question banks into the shared application. Keep the content/data structure consistent with Modules 1 and 2, update the catalogue and module-routing limits deliberately, and test the module and assessment routes before merging. Do not expose answer keys in learner-facing pages.

## Collaboration and safe Git use

1. Pull the latest `main` before starting shared work.
2. Use a feature branch for a change and make focused commits.
3. Review changes with the team before merging.
4. Do not commit passwords, tokens, private connection strings, database files, or build output.
5. Describe prototype limitations accurately; do not claim authentication, backend marking, or cross-device progress until implemented and tested.
