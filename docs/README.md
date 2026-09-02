# ChangeGuard Documentation

This folder contains the product, architecture, decision, learning, and daily revision records for ChangeGuard AI.

## Document Map

| File | Purpose | When to update |
|---|---|---|
| `product-requirements.md` | Business problem, impact, stakeholders, requirements, rules, acceptance criteria, risks, and scope | When the client clarifies or changes a requirement |
| `architecture.md` | System structure, responsibilities, dependency rules, request flow, and technical boundaries | When architecture or integration design changes |
| `mvp-acceptance-walkthrough.md` | Visual Studio, API, React, workflow, audit, duplicate, and build acceptance checks | Run before merging or starting Azure deployment |
| `react-migration.md` | React migration boundary, Angular-to-React concept mapping, validation, and rollback | Review when explaining or changing the frontend migration |
| `day-01-technical-notes.md` | Expanded Day 1 learning and interview notes | When a Day 1 explanation is corrected or expanded |
| `day-01-technical-notes.docx` | Professionally formatted Word version of the detailed Day 1 technical notes | Use for complete interview preparation |
| `day-01-technical-revision.docx` | Concise EOD revision covering what, where, why, trade-offs and interview explanations | Review for 15 minutes at the end of Day 1 and before interviews |
| `day-02-technical-notes.docx` | Domain model, readiness rules, application boundary, Angular feature, and tests | Review before domain and Clean Architecture interviews |
| `day-02-technical-revision.docx` | Concise Day 2 interview revision | Review with Day 2 practice questions |
| `day-03-technical-notes.docx` | EF Core, SQL Server, migrations, repository, and persistence concepts | Review before data-access interviews |
| `day-03-technical-revision.docx` | Concise Day 3 interview revision | Review with EF Core and SQL questions |
| `daily-technical-documentation-template.docx` | Reusable Word template for future daily technical documentation | Copy and complete at the end of every development day |
| `day-01-technical-revision.md` | Structured EOD revision: what, where used, why used, interview answer, and memory aid | Use for daily revision; update only when Day 1 work changes |
| `daily-technical-documentation-template.md` | Mandatory template for Day 2 onward | Copy at the end of each day and replace the placeholders with verified work |
| `adr/0001-use-modular-monolith.md` | Permanent record of why a modular monolith was selected | When recording a superseding decision or important consequence |

## Documentation Rules

The Day 1–3 documents intentionally describe the Angular implementation used during those learning days. Use `react-migration.md` and the root README for the current React architecture.

1. Product documents explain **what and why**.
2. Architecture documents explain **how the system is organised**.
3. ADRs explain **why an important technical decision was made**.
4. Technical notes explain **what the developer learned and can explain in an interview**.
5. Daily revision documents explain **what was used, where it was used, and why it was selected**.
6. Planned functionality must not be described as completed functionality.
7. Requirements should be measurable and testable whenever possible.
8. When a client decision changes, record the date and reason.
9. Every development day ends with an updated revision document and verified Git commit.
