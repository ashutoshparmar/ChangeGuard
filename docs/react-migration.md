# Angular-to-React Migration

## Decision

The active browser client is now `src/ChangeGuard.React`, built with React, TypeScript, and Vite. The ASP.NET Core API, Application layer, Domain model, Infrastructure layer, EF Core migrations, SQL schema, and HTTP contracts were not changed by this migration.

The former `src/ChangeGuard.Web` Angular application remains temporarily as a rollback and learning reference. Docker Compose and GitHub Actions now build the React application.

## Why the change is low risk

The frontend already communicated with the backend only through documented REST contracts. React consumes the same routes and JSON models:

| Capability | API route | React implementation |
|---|---|---|
| Health | `GET /api/system/health` | `getSystemHealth` |
| Search | `GET /api/change-requests` | `changeRequestService.search` |
| Dashboard | `GET /api/change-requests/dashboard` | `getDashboard` |
| Details | `GET /api/change-requests/{reference}` | `getByReferenceNumber` |
| Create | `POST /api/change-requests` | `create` |
| Workflow | `POST /api/change-requests/{reference}/workflow` | `applyWorkflowAction` |
| Evidence | `PUT /api/change-requests/{reference}/release-artifacts` | `recordReleaseArtifacts` |
| Audit | `GET /api/change-requests/{reference}/audit` | `getAudit` |

## Concept mapping

| Angular concept | React equivalent used here |
|---|---|
| Standalone component | Function component |
| Signals / component fields | `useState` |
| `ngOnInit` | `useEffect` |
| `HttpClient` service | Typed service using Fetch |
| RxJS `forkJoin` | `Promise.all` |
| Reactive FormGroup | Controlled form state and validation |
| Angular environment replacement | Vite mode-specific `.env` files |
| Angular TestBed | React Testing Library |
| `HttpTestingController` | Mocked Fetch request assertions |

## Validation gates

- TypeScript strict compilation
- React Testing Library and Vitest tests
- Vite production build
- Existing backend build and xUnit tests
- Browser-to-API acceptance walkthrough
- Docker Compose build and smoke test

## Rollback

Until the team accepts the React client, rollback requires changing the `web` Dockerfile path and frontend CI working directory back to `src/ChangeGuard.Web`. No database rollback or API deployment change is required.

After React is accepted and deployed successfully, the Angular directory can be removed in a separate, explicit cleanup commit.
