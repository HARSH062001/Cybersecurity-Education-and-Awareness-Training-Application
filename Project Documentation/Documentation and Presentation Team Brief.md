# Documentation and presentation team brief

**Project:** Cybersecurity Education and Awareness Training Application (COMP6900)    
**Purpose:** Give the team a shared, current structure for writing the final project documentation and presentation. Confirm the course rubric and update this brief if the final submission instructions require a different structure.

## Message to send the group

Hi everyone, I’ve drafted a proposed structure for our final project documentation and presentation based on the current application. Please review the outline below and let us know which sections/slides you can own. For each section, use evidence from the current code, README, test output, screenshots or project records. Please clearly separate implemented features, tested behavior, known limitations and future work. Don’t describe planned work as completed, and don’t share local databases, passwords, user-secrets or personal learner data. Once we agree owners, we can combine the sections, check them against the marking rubric, and rehearse one consistent demonstration.

## Suggested project documentation structure

1. **Cover and document control**
   - Project title, course, institution, team members, date, version and repository/commit reviewed.
   - Change history and who prepared/reviewed each section.

2. **Executive summary / abstract**
   - The workplace cybersecurity awareness problem.
   - What the application does, who it is for, the main technology and the principal outcome.
   - Keep this to a short summary of the final implementation, not the original proposal alone.

3. **Introduction and problem statement**
   - Why employee security behaviors matter.
   - The audience and the training gap the project addresses.
   - Project scope and context; distinguish assumptions from evidence.

4. **Aim, objectives and success criteria**
   - State measurable objectives (eight modules, saved learning progress, assessment, feedback, role-based management, etc.).
   - Map each objective to evidence that demonstrates completion.

5. **Requirements and scope**
   - Functional requirements: registration/sign-in, modules, section progress, assessments, feedback/history, management, schedules and notifications.
   - Non-functional requirements: usability, accessibility, privacy, security, maintainability and responsive layout.
   - In-scope features, out-of-scope items, assumptions and constraints.

6. **Background / related work**
   - Explain relevant cybersecurity-awareness guidance and learning/assessment principles.
   - Use authoritative sources already cited in the module content or project research.
   - Compare the application’s approach with relevant guidance; avoid unsupported claims that training alone prevents incidents.

7. **Methodology and project process**
   - How the team gathered requirements, divided work, developed, reviewed and integrated changes.
   - GitHub branches/pull requests and collaboration process, if evidenced.
   - Explain iterations and major decisions; include a timeline only if the team has records to support it.

8. **System design and architecture**
   - Diagram: browser/Razor Pages -> ASP.NET Core Identity and page handlers/API endpoints -> EF Core -> local SQLite.
   - Show where JSON module content/question banks are loaded and how progress/attempts are persisted.
   - Include key routes/components, role boundaries and data flow. Label diagram accurately; do not portray SQLite as a hosted production database.

9. **Database design**
   - Describe the local SQLite database and main entities/tables: Identity users/roles; section progress; enrollments; module owner assignments; schedules; assessment banks/questions/options; attempts and answers.
   - Include an ER diagram or schema summary derived from EF entities/migrations.
   - Explain primary/foreign keys, important uniqueness rules and how migrations/seeding work.
   - Use a sanitized schema or fake-data screenshot only. Never attach `cybertraining.db`, backups, WAL/SHM files, password hashes or real learner records.

10. **Implementation**
    - Technology stack: ASP.NET Core Razor Pages, .NET 10, EF Core, SQLite, Identity, HTML/CSS/JavaScript and JSON content.
    - Explain learner journey, assessment lifecycle and server-side score marking.
    - Explain Admin and ModuleOwner capabilities and learner data isolation.
    - Explain best-score display, 90% default pass threshold, retry-on-fail and lock-after-pass behavior.
    - Explain that answer feedback shows all options with correct/incorrect visual states and explanations.

11. **Security, privacy and ethical considerations**
    - Authentication, roles, server-side ownership checks, antiforgery checks, password policy/lockout and optional authenticator 2FA.
    - Data minimization, local test data and privacy-safe screenshots.
    - Discuss limitations honestly: this is a coursework prototype; production hosting, production email, operational backup and a full privacy/security review are not configured. 2FA is available but not mandatory.

12. **Testing and results**
    - Separate automated tests from manual/browser checks.
    - For each test: ID, setup, steps, expected result, actual result, pass/fail, evidence and tester.
    - Report exact build/test command and actual result only after running it; don’t claim the suite verifies every workflow.
    - Include representative positive, negative, authorization, persistence, retry/pass-boundary, notification and responsive/accessibility checks.

13. **Evaluation and discussion**
    - Evaluate each objective against test or demonstration evidence.
    - Discuss usability, strengths, issues found and their resolution or status.
    - Discuss what results do and do not demonstrate; avoid claiming an improvement in employee security without a measured user study.

14. **Limitations and future work**
    - State current boundaries: no named pathway editor or enrollment-removal workflow; schedules are reminders and do not gate attempts; notifications are generated on page visit without background/email/push delivery; no production hosting/email/operational backup/full security review.
    - Possible next steps should be labeled as future work, not completed features.

15. **Conclusion and recommendations**
    - Summarize delivery against scope and objectives, give the most important evidence, and identify realistic next steps.

16. **References and appendices**
    - Use the citation style required by COMP6900 and provide full references for external guidance, standards and sources.
    - Append approved diagrams, requirements traceability, test cases/results, sanitized screenshots and user/setup instructions as appropriate.

## Suggested presentation structure (about 10-12 slides)

1. **Title** - Project, team, course and date.
2. **Problem and audience** - Workplace awareness need and intended users.
3. **Aim and scope** - Objectives and what the project does/does not cover.
4. **Solution overview** - Eight modules, seven sections each, progress and assessment journey.
5. **Architecture** - Browser, Razor Pages, Identity, EF Core, SQLite and JSON content in one clear diagram.
6. **Database and data flow** - Key entities and how an answer goes from selection to server-side result; use sanitized/fake data.
7. **Key features** - Learner, Admin and ModuleOwner examples; mention 90% default and retry-until-pass/lock-after-pass accurately.
8. **Live demonstration** - Register/sign in with test account, complete sections, take assessment, show result feedback and history; optionally show manager enrollment/schedule/notification.
9. **Testing and evidence** - Automated test coverage plus manual scenarios actually run, with concise results/screenshots.
10. **Security and privacy** - Identity/authorization, server-side validation, antiforgery and handling of local learner data.
11. **Evaluation, limitations and future work** - Objective-by-objective evidence and honest prototype boundaries.
12. **Conclusion and questions** - Main takeaway and Q&A.

## Suggested speaker/demo flow

- Start with the user problem and show the module catalogue.
- Sign in as a disposable learner account; demonstrate saved section completion and assessment unlock only after all sections are complete.
- Submit a controlled practice assessment and show score, pass/retry behavior, feedback colors/explanations, best score and history. Use a fresh account/database for a pass-lock demonstration.
- If time allows, show Admin/ModuleOwner scope, module enrollment, schedule setup and the learner’s computed notification.
- Close with test evidence, limitations and next steps. Avoid exposing emails, secrets or database rows containing personal data.

## Evidence and accuracy checklist for authors

- Match claims to the reviewed branch/commit and code; include a source/evidence pointer for major claims.
- Distinguish: implemented, automated-tested, manually-tested, not tested, and future work.
- Use screenshots from the current version, with fake accounts and no personal data.
- Include the exact 90% default, retry and pass-lock rules; do not describe an obsolete threshold or old UI.
- Say notifications are calculated when the page is opened; do not imply emails/push are sent.
- Say the database is local SQLite; do not present it as a shared cloud database.
- Cite references consistently and check all figure/table numbers and slide titles.
- Assign an owner and reviewer to each report section and slide; one person should perform final consistency editing.
- Verify every final document against the official assignment brief/rubric before submission.
