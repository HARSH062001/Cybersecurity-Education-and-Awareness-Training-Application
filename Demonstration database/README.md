# Demonstration Database

This folder contains `cybertraining-demo.db`, a sample SQLite database for
the Cybersecurity Education and Awareness Training Application.

The sample gives the team or assessor a starting database with the assessment
question banks already included. It is separate from `cybertraining.db`, the
working database used by the application.

## What the database contains

When checked, the demonstration database contained:

| Item | Number |
| --- | ---: |
| Assessment banks | 24 |
| Assessment questions | 240 |
| Answer options | 960 |
| User accounts | 0 |
| Assessment attempts | 0 |
| Saved assessment answers | 0 |
| Section-progress records | 0 |
| Module enrolments | 0 |

The sample has no existing learner accounts or assessment history. A user
can create an account and generate new progress and assessment records when
testing the application.

The database passed SQLite's integrity and foreign-key checks when reviewed.

## How to view the database

You can inspect the file using **DB Browser for SQLite**:

1. Open DB Browser for SQLite.
2. Select **Open Database**.
3. Choose `cybertraining-demo.db` from this folder.
4. Select **Database Structure** to see the tables.
5. Select **Browse Data** and choose a table to view its records.

For example, `AssessmentBanks`, `AssessmentQuestions` and
`AssessmentOptions` contain the prepared assessment content.

## Using the database with the application

The application normally looks for a database named `cybertraining.db` in
the `CybersecurityTrainingWeb` folder. To try this demonstration database:

1. Make a separate copy of the project for testing.
2. In that copy, check whether `CybersecurityTrainingWeb` already contains
   a `cybertraining.db` file. Keep that file if it contains data you need.
3. Copy `cybertraining-demo.db` into the copied
   `CybersecurityTrainingWeb` folder.
4. Rename the copied database to `cybertraining.db`.
5. Follow the setup and run instructions in the project's main `README.md`.

Create a test account after starting the application. Use test information
when demonstrating learner progress and assessment results.

## Database for project submission

The team has also decided to consider submitting the working
`cybertraining.db` because it contains test accounts and activity. The team
should review that database before sharing it and confirm that its contents
are appropriate for submission.

This demonstration database is an optional fresh starting point. The team
should confirm with the lecturer whether it meets the project's database
requirements or whether sample accounts and progress are expected.