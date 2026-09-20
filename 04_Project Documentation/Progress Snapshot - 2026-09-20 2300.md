# Project Progress Snapshot

**Project:** Cybersecurity Education and Awareness Training Application  
**Course:** COMP6900 Computing Project, University of Newcastle  
**Recorded:** 20 September 2026, 11:00 pm (Australia/Sydney)

## Milestone summary

The project has moved from content preparation into Phase 2 application development. All planned module and question-bank documents are present, the ASP.NET Core application foundation builds successfully, all module content is available in the frontend, and Module 1 now provides the first working learning-progress slice.

## Completed content work

- Eight employee-focused cybersecurity module documents.
- Three question-bank documents for each module.
- Ten planned questions in each bank.
- 240 planned questions overall.
- Correct answer, explanation, business impact and reference planned for every question.
- Module-specific references, including references embedded in Module 1 and separate documents for Modules 2–8.
- AI Literacy and AI Security content limited to education and risk awareness.
- No AI-powered application functionality implemented.

## Completed application work

- .NET 10 solution created.
- ASP.NET Core Razor Pages web project created.
- Entity Framework Core configured.
- SQLite configured for local development.
- Initial module, question-bank, question and answer-option models created.
- Initial seed data created for the application prototype.
- Home, dashboard, module catalogue, module, assessment prototype, results prototype and privacy pages created.
- Full learning content for all eight modules imported into structured web data.
- Responsive styling applied.

## Module 1 design milestone

Module 1 — Cybersecurity Fundamentals is the current final design candidate pending team review.

Completed Module 1 features:

- Seven guided learning sections.
- Sticky section navigation on larger screens.
- Responsive navigation and content layout for smaller screens.
- Key-term, business-impact and security-checklist tables.
- Explicit learner-controlled section completion.
- Progress calculated from confirmed sections.
- Progress shown consistently on the module page, catalogue and dashboard.
- Completion status retained after refresh.
- Backend SQLite record containing learner key, module ID, section key and completion timestamp.
- Anonymous learner cookie until authentication is available.
- Browser-storage fallback for temporary resilience.
- Undo support for an incorrectly completed section.
- Assessment clearly marked as a future integration step.

## Verification completed

- .NET build completed with zero warnings and zero errors after the Module 1 redesign.
- Module catalogue returned HTTP 200.
- Module 1 returned HTTP 200.
- Module 2 preview remained accessible and returned HTTP 200.
- Dashboard returned HTTP 200.
- Progress complete, refresh, catalogue synchronization and undo behaviour were checked.
- Progress endpoint create, read and remove behaviour was checked.
- Local database and build output remain excluded from Git.

## Current limitations

- Authentication and role-based access are not implemented.
- The anonymous learner key is temporary and must be connected to the authenticated user.
- The Word question banks have not yet been imported into the database.
- Assessment submission, marking and feedback remain prototypes.
- Attempts, selected banks, answers, scores and results are not yet recorded.
- Repeated attempts do not yet select a different bank.
- Modules 2–8 contain web content but do not yet use the approved Module 1 guided-learning design.
- Module Owner and Administrator interfaces are not implemented.
- Production security, privacy, migration, testing and deployment work remains outstanding.

## Team review gate

The team should review Module 1 before the pattern is copied to Modules 2–8. Review should cover:

- Learning-section structure.
- Employee readability.
- Progress-completion rule.
- Frontend appearance and mobile layout.
- Database progress record.
- Authentication integration approach.
- Assessment entry criteria.

## Next planned vertical slice

After team approval:

```text
Login → Dashboard → Module 1 → Question Bank → Assessment → Score → Saved Result → Retake
```

The next implementation priorities are authentication, Module 1 question-bank import, assessment persistence, scoring, results and repeated-attempt bank selection.

## Git evidence at this milestone

- `24d47e1` — Load full module content into frontend.
- `1ff3566` — Redesign Module 1 with tracked learning progress.

This snapshot records the state of the project at 11:00 pm and should be updated after team review and the next integration milestone.
