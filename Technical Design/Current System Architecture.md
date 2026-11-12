# Cybersecurity Training Application: System Architecture and Data Design

## 1. Application overview

The Cybersecurity Training Application is a web-based learning prototype designed to help employees develop safer cybersecurity habits at work.

The application contains eight learning modules covering topics such as cybersecurity fundamentals, phishing, identity security, data privacy and AI security. Learners can read the material, mark sections as complete and take a multiple-choice assessment for each module. Their progress and assessment results are saved to their account.

Administrators and assigned module owners can manage aspects of training, including learner enrolments and assessment schedules. Learners can view their progress, assessment history and training notifications.

The application is built with **ASP.NET Core Razor Pages** and targets **.NET 10**. It uses **ASP.NET Core Identity** for accounts and authentication, **Entity Framework Core** for database access, and **SQLite** for local data storage.

## 2. How the system is organised

The application has three main parts:

1. **User interface:** The pages learners and administrators interact with in a web browser. These use Razor Pages, HTML, CSS and JavaScript.
2. **Application logic:** The server-side code that handles requests, checks permissions, manages progress and marks assessments.
3. **Database:** A local SQLite database that stores accounts, roles, enrolments, section progress, assessment questions, attempts and results.

The following table gives an overview of how these parts work together:


flowchart LR
   Browser[Web browser] --> App[ASP.NET Core Razor Pages and API handlers]
    App --> Identity[ASP.NET Core Identity]
    App --> EF[Entity Framework Core]
    EF --> DB[(SQLite database)]
    App --> Modules[Module content JSON]
    Banks[Question-bank JSON] --> EF
    Migrations[Database migrations] --> DB


The main application pages are in `CybersecurityTrainingWeb/Pages`. Styling is provided through files in `wwwroot/css`, and client-side scripts are stored in `wwwroot/js`.

Learning content is read from `Data/module-content.json`. The assessment questions, options and explanations are supplied through `Data/question_banks.json` and added to the database through the application’s seeding process. Assessment attempts then use the database-backed question banks.

The `wwwroot/lib` folder contains third-party libraries used by the application. These libraries are dependencies; they are not application logic written by the project team.

## 3. Accounts and access

The application uses ASP.NET Core Identity to manage user accounts, password hashing and authentication. Users can also enable two-factor authentication with an authenticator app.

Different roles provide access to different functions:

- **Learner:** Accesses training modules, completes sections and assessments, and reviews personal progress.
- **Module owner:** Manages the modules assigned to them.
- **Administrator:** Has access to administration functions across modules.

Management permissions are checked on the server. Requests that change protected data also use antiforgery validation.

Two-factor authentication is available but is not compulsory. In the development setup, users can register without confirming their email address. A deployed version would need an appropriate email-confirmation service and further security review.

## 4. Learning and assessment process

A typical learner follows these steps:

1. The learner signs in and enrols, or is enrolled, in a module.
2. They work through the module’s seven learning sections and mark each section as complete.
3. Once all sections are complete, they can access the module assessment.
4. The application assigns an available question bank and saves the learner’s selected answers as they work.
5. When the assessment is submitted, the server checks and marks the answers.
6. The learner receives a score and feedback. The default pass mark is **90%**.
7. If the learner does not pass, they can review the learning material and try again. Once they pass the module assessment, further attempts are locked.

For failed attempts, the feedback page displays the learner’s selected answers, indicates whether they were correct or incorrect, and provides explanations. It does not highlight an unselected correct option. After a passing attempt, the correct options can be highlighted in the results.

The application records assessment attempts and displays the learner’s best completed score for the module. Learners can review their own assessment history and results.

The pass mark can be configured in `appsettings.json`. Assessment access and result ownership are checked on the server.

## 5. Database design

The application uses a local SQLite database. Entity Framework Core migrations define and update its structure.

The main records include:

- **Users and roles:** Account details and access roles.
- **Section progress:** The learning sections a user has marked complete.
- **Module enrolments:** The modules assigned to each learner.
- **Module ownership:** The modules assigned to a module owner.
- **Assessment banks, questions and options:** The content used for assessments.
- **Assessment attempts and answers:** The learner’s selected answers, scores and feedback.
- **Module schedules:** Assessment due dates and repeat intervals.

The following shows the main relationships:


ER Diagram
    AspNetUsers ||--o{ SectionProgressEntries : completes
    AspNetUsers ||--o{ ModuleEnrollments : enrols
    AspNetUsers ||--o{ ModuleOwnerAssignments : owns
    AspNetUsers ||--o{ AssessmentAttempts : makes
    AssessmentBanks ||--o{ AssessmentQuestions : contains
    AssessmentQuestions ||--o{ AssessmentOptions : offers
    AssessmentBanks ||--o{ AssessmentAttempts : used_by
    AssessmentAttempts ||--o{ AssessmentAnswers : contains
    AssessmentQuestions ||--o{ AssessmentAnswers : answered_by


ASP.NET Core Identity also uses supporting tables for roles, claims, logins and tokens. The entity classes, `ApplicationDbContext` and the migration files provide the detailed database structure.

## 6. Schedules and notifications

Administrators can manage training across modules, while module owners can manage their assigned modules. A schedule can record an assessment due date and a repeat interval.

The notifications page shows a learner’s module assignments and assessment reminders when they open the page. The current implementation does not send background email or push notifications. Schedules provide reminders and do not prevent a learner from starting an assessment.

## 7. Invalid pages and assessment links

If a learner opens a page or assessment attempt that is unavailable, the application provides a recovery page with a way to return to the application. Assessment attempts are checked against the signed-in user so that a learner cannot access another user’s attempt through its URL.

## 8. Running and testing the application

The project’s `README.md` explains the required setup, how to apply the database migrations and how to run the application.

The test project uses temporary SQLite databases to check areas such as assessment access, navigation, question-bank structure, enrolments and role-based management. The team should record the date, environment and actual results of its final test run in the project report.

Testing the application’s functions does not by itself establish that the content is error-free or that the system is ready for production use.

## 9. Current scope and future improvements

The application is a coursework prototype. Future work could include:

- A named learning-pathway editor.
- A way for authorised users to remove enrolments.
- Background email or push notifications.
- A mandatory two-factor authentication policy.
- Production hosting, email configuration and operational backups.
- A full privacy, accessibility and security review.
- A user study to evaluate the application’s effect on employee knowledge and behaviour.

