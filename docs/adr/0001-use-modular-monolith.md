# ADR 0001: Use a Modular Monolith

## Status

Accepted on 2026-08-17.

## Context

ChangeGuard must support a traceable requirement-to-release workflow, including clarification, development information, QA evidence, approvals, SLA tracking, rollback readiness, and audit history.

The initial team is small. The product boundaries and scaling characteristics have not yet been validated through production usage. Introducing independently deployed microservices now would require additional deployment pipelines, distributed observability, network-failure handling, message consistency, versioned contracts, and operational ownership.

The solution still needs clear business boundaries and a path to evolve.

## Decision

Build the initial backend as a modular monolith using Clean Architecture principles.

- Deploy the backend initially as one application.
- Separate business capabilities using explicit module boundaries.
- Keep Domain independent of database, cloud, and user-interface technology.
- Put application use cases in Application.
- Implement persistence and external integrations in Infrastructure.
- Use interfaces and events where a meaningful boundary exists.
- Extract a module into a service only after evidence demonstrates a need.

## Reasons

- Lower initial operational complexity
- Faster delivery for a small team
- Easier local development and debugging
- Simpler transactions during early workflow development
- Lower infrastructure cost
- Clear business separation without premature distribution
- Ability to extract modules later

## Alternatives Considered

### Microservices from the Start

Not selected because current team size and requirements do not justify the cost of distributed deployment, consistency, diagnostics, and operations.

### Unstructured Monolith

Not selected because mixing business, persistence, HTTP, and integration logic would make future change and testing more difficult.

### Serverless Functions for Every Capability

Not selected because it would distribute an early, highly connected workflow before its boundaries and traffic patterns are understood.

## Positive Consequences

- One backend deployment initially
- Clear separation of business and technical concerns
- Easier refactoring while requirements evolve
- Simpler end-to-end tests
- Lower cloud and operational overhead

## Negative Consequences

- A defect can affect the single backend deployment.
- Modules share the same application process.
- Independent scaling is not initially available per module.
- Boundaries require code-review discipline because they are not enforced by network separation.

## Risks and Mitigations

| Risk | Mitigation |
|---|---|
| Modules become tightly coupled | Enforce dependency rules and review cross-module references |
| Business logic moves into controllers or Infrastructure | Keep controllers thin and test Domain/Application behaviour |
| The application grows difficult to deploy | Monitor build, deployment, resource use, and module ownership |
| A module requires independent scaling | Extract only that validated module behind a stable contract |

## Revisit Triggers

Review this decision if one or more of these conditions becomes material:

- A module requires independent scaling.
- Teams require independent release ownership.
- A capability needs stronger fault isolation.
- Deployment frequency differs significantly by module.
- A module requires a substantially different technology.
- Production evidence shows the modular monolith cannot meet a validated requirement.

## Related Documents

- `../product-requirements.md`
- `../architecture.md`
- `../day-01-technical-notes.md`
