# ChangeGuard AI

[![ChangeGuard CI](https://github.com/ashutoshparmar/ChangeGuard/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/ashutoshparmar/ChangeGuard/actions/workflows/ci.yml)


ChangeGuard is a requirement-to-release control centre for making software changes clear, traceable, and safe. It replaces informal email-and-meeting hand-offs with a visible workflow, mandatory release evidence, SLA signals, and an immutable audit trail.

The current risk score is intentionally rule-based. Azure AI integration remains a later enhancement and must not be presented as implemented.

## Completed MVP

- Create a validated change request as a draft
- Enforce a controlled status workflow in the domain layer
- Search and filter requests by text, priority, and status
- View request details, SLA deadline, and release-readiness score
- Record a rollback plan during development or QA
- Record QA evidence only during QA testing
- Block QA completion until both mandatory artifacts exist
- Approve, reject, release, and close requests through explicit actions
- Store every creation, evidence update, and transition in an audit trail
- Display operational dashboard metrics and recent activity
- Return consistent `ProblemDetails` errors for validation, conflicts, and missing data
- Persist data in SQL Server through EF Core migrations
- Expose liveness, database-readiness, and system-health endpoints
- Propagate an `X-Correlation-ID` for troubleshooting
- Run backend and frontend validation in GitHub Actions
- Run the complete stack with Docker Compose

## Workflow

```text
Draft
  -> Requirement Review
  -> In Development
  -> QA Testing
  -> Release Approval
  -> Released
  -> Closed
```

A request may be rejected before release. QA testing cannot be completed without QA evidence and a rollback plan. These rules are enforced by the API domain model, not only by React.

## Technology Stack

| Area | Technology |
|---|---|
| Backend | .NET 10, ASP.NET Core, C# |
| Frontend | React 19, TypeScript 7, Vite 8, CSS |
| Database | SQL Server, Entity Framework Core 10 |
| Tests | xUnit, React Testing Library, Vitest |
| Delivery | Docker, Nginx, Docker Compose, GitHub Actions |
| Architecture | Modular monolith with Clean Architecture boundaries |

## Architecture

```text
React Web -> ASP.NET Core API -> Application use cases -> Domain rules
                                      |
                                      v
                         Repository abstraction
                                      |
                                      v
                         EF Core / SQL Server
```

- `ChangeGuard.Domain` owns workflow, readiness, and SLA business rules.
- `ChangeGuard.Application` owns use cases, response mapping, and persistence abstractions.
- `ChangeGuard.Infrastructure` implements EF Core persistence, migrations, and database health.
- `ChangeGuard.Api` owns HTTP contracts, validation, error handling, CORS, and dependency composition.
- `ChangeGuard.React` owns the active React operator experience and communicates only through HTTP.
- `ChangeGuard.Web` retains the former Angular client temporarily as a rollback and learning reference.

See [architecture.md](docs/architecture.md) and [ADR 0001](docs/adr/0001-use-modular-monolith.md).

## Run with Visual Studio and LocalDB

Prerequisites: Visual Studio with ASP.NET development tools, .NET 10 SDK, Node.js 24, and SQL Server LocalDB.

1. Open `ChangeGuard.slnx` in Visual Studio.
2. Set `ChangeGuard.Api` as the startup project.
3. Select the `https` launch profile and start the API.
4. Development startup applies pending EF Core migrations to `ChangeGuardDb` automatically.
5. Verify `https://localhost:7110/api/system/health` and `https://localhost:7110/health/ready`.
6. Open a terminal in `src\ChangeGuard.React` and run:

```powershell
npm.cmd install
npm.cmd run dev
```

7. Open `http://localhost:4200` and create a request such as `CG-501`.

The React development environment targets `https://localhost:7110/api`. If Visual Studio starts the API on another port, update `VITE_API_BASE_URL` in `src/ChangeGuard.React/.env.development` and restart Vite.

### Apply migrations manually

Automatic migration is enabled only in `appsettings.Development.json`. To inspect and apply it yourself:

```powershell
dotnet ef migrations list `
  --project .\src\ChangeGuard.Infrastructure `
  --startup-project .\src\ChangeGuard.Api

dotnet ef database update `
  --project .\src\ChangeGuard.Infrastructure `
  --startup-project .\src\ChangeGuard.Api `
  -- --environment Development
```

## Run the Full Stack with Docker

From the repository root:

```powershell
Copy-Item .\.env.example .\.env
notepad .\.env
docker compose up --build
```

Replace the example SQL password before starting. Then open:

- Web: `http://localhost:8080`
- API: `http://localhost:8081`
- Readiness: `http://localhost:8081/health/ready`

Stop containers with `docker compose down`. Add `--volumes` only when you intentionally want to delete the local SQL data volume.

## API Surface

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/change-requests` | Create a draft |
| `GET` | `/api/change-requests` | Search and filter |
| `GET` | `/api/change-requests/dashboard` | Operational metrics |
| `GET` | `/api/change-requests/{reference}` | Complete details |
| `POST` | `/api/change-requests/{reference}/workflow` | Apply a controlled transition |
| `PUT` | `/api/change-requests/{reference}/release-artifacts` | Record QA evidence or rollback plan |
| `GET` | `/api/change-requests/{reference}/release-readiness` | Evaluate the release gate |
| `GET` | `/api/change-requests/{reference}/audit` | Retrieve audit history |
| `GET` | `/api/system/health` | Typed application health response |
| `GET` | `/health/live` | Process liveness |
| `GET` | `/health/ready` | SQL database readiness |

Request examples are available in `src/ChangeGuard.Api/ChangeGuard.Api.http`.

## Build and Test

Backend, from the repository root:

```powershell
dotnet build .\ChangeGuard.slnx
dotnet test .\ChangeGuard.slnx --no-build
```

Frontend:

```powershell
cd .\src\ChangeGuard.React
npm.cmd run test:ci
npm.cmd run build
```

The CI definition is in `.github/workflows/ci.yml` and runs both pipelines independently.

## Configuration and Security

- LocalDB configuration is limited to `appsettings.Development.json`.
- Production supplies `ConnectionStrings__ChangeGuardDatabase` through the environment or a secret store.
- Vite environment files never contain credentials because `VITE_*` values are delivered to the browser.
- The database unique index remains the final protection against concurrent duplicate reference numbers.
- API errors expose a trace identifier but hide unexpected exception details.
- CORS permits selected browser origins; it is not authentication.

Authentication and role-based authorization are intentionally not yet active. The next cloud phase will integrate Microsoft Entra ID and Azure-managed secret storage manually so those concepts can be learned and explained, not merely copied.

## Azure Readiness

The application is prepared for a manual Azure learning phase through container images, environment-based configuration, health probes, SQL retry, structured logs, migrations, and CI validation. No Azure resources have been provisioned and no deployment is claimed yet.

The manual phase will cover resource group, Azure SQL, container registry, Container Apps, managed identity, Key Vault, Application Insights, deployment validation, and cost cleanup.

## Documentation

- [Documentation index](docs/README.md)
- [Product requirements](docs/product-requirements.md)
- [Architecture](docs/architecture.md)
- [Angular-to-React migration](docs/react-migration.md)
- [ADR 0001: modular monolith](docs/adr/0001-use-modular-monolith.md)
- Day 1–3 technical notes and interview-revision Word documents in `docs/`

## Current Boundaries

- Microsoft Entra authentication and role authorization are not implemented.
- Azure resources and CI/CD deployment are not configured.
- File uploads use text evidence references rather than Blob Storage.
- Notifications and Service Bus events are not implemented.
- AI-generated requirement or risk analysis is not implemented.

These are deliberate next-phase learning items, not claims hidden behind the name “AI.”
