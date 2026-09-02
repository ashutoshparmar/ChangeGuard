# ChangeGuard AI - Day 1 Technical Revision

## Document Purpose

Use this document for a 15-minute end-of-day revision and before interviews.

For every concept, remember four points:

1. What is it?
2. Where did we use it?
3. Why did we use it?
4. How would I explain it in an interview?

This document separates concepts actually implemented on Day 1 from concepts only reviewed or planned. Do not claim planned technology as completed experience.

## Day 1 Outcome

Day 1 established a working full-stack foundation:

```text
Angular component
-> Angular service
-> HttpClient
-> ASP.NET Core API
-> Typed JSON response
-> Observable
-> AsyncPipe
-> Browser UI
```

The flow was verified using unit tests, production builds, and a real browser-to-API check.

## Quick Concept Map

| Concept | Where used | Why used |
|---|---|---|
| Modular monolith | Backend architecture decision | Lower operational complexity for a small team while retaining module boundaries |
| Clean Architecture | Domain, Application, Infrastructure, and API projects | Protect business rules from framework and infrastructure dependencies |
| Vertical slice | System-health feature | Prove one capability through frontend and backend end to end |
| ASP.NET Core controller | `SystemController` | Expose the health operation through HTTP |
| Attribute routing | `api/system/health` | Give the endpoint a clear, stable URL |
| `ActionResult<T>` | Health action return type | Express both the response model and HTTP result |
| C# record | `SystemHealthResponse` | Represent an immutable response-shaped data contract concisely |
| CORS | `Program.cs` | Permit the Angular development origin to call the API |
| Angular standalone component | `AppComponent` | Build the UI without an NgModule |
| Angular service | `SystemHealthService` | Keep HTTP logic outside the visual component |
| Dependency injection | Angular `inject(...)` | Supply dependencies without direct construction and support testing |
| `HttpClient` | Health service | Send the typed GET request |
| Environment configuration | API base URL | Use different public configuration for development and production builds |
| TypeScript interface | Health response model | Type-check the frontend/backend contract |
| Observable | Health HTTP response | Represent and transform asynchronous response state |
| RxJS operators | `map`, `catchError`, `startWith` | Create loading, success, and error view states |
| `AsyncPipe` | Angular template | Subscribe to the Observable and manage the subscription lifecycle |
| `OnPush` | Root component | Reduce unnecessary change-detection work |
| Unit testing | xUnit and Vitest tests | Verify isolated behaviour quickly and repeatedly |
| HTTP test controller | Angular service test | Verify the request without calling a real API |
| Mock service | Angular component test | Test rendering independently from the network |
| Production build | .NET and Angular builds | Compile the complete application and catch integration errors |
| Git commit | First local commit | Create a traceable and recoverable project checkpoint |
| `.gitignore` | Repository root and Angular project | Exclude generated, large, and local-only files |
| `.editorconfig` | Repository root | Keep formatting and coding conventions consistent |

## 1. Modular Monolith

### What it is

A modular monolith is one deployable backend organised into clear business or technical modules.

### Where we used it

ChangeGuard currently has one backend solution with separate Domain, Application, Infrastructure, and API projects.

### Why we used it

- The initial team is small.
- There is no proven need for independent module deployment or scaling.
- It avoids early distributed-system complexity.
- Clear boundaries still allow later extraction when production evidence supports it.

### Interview answer

> We selected a modular monolith because the team is small and there is no proven need for independent deployment or scaling. It reduces infrastructure and operational complexity while Clean Architecture keeps the code modular. If a module later requires independent scaling, ownership, or fault isolation, we can extract it behind a stable contract.

### Remember

```text
One deployment does not have to mean one unstructured codebase.
```

## 2. Clean Architecture

### What it is

Clean Architecture organises dependencies so core business rules do not depend on UI, database, cloud, or framework details.

### Where we used it

- `ChangeGuard.Domain`: business concepts and rules
- `ChangeGuard.Application`: use cases and orchestration
- `ChangeGuard.Infrastructure`: persistence and external integrations
- `ChangeGuard.Api`: HTTP entry point and dependency composition
- `ChangeGuard.Web`: Angular frontend communicating through HTTP contracts

### Why we used it

- Protect business logic from technical changes
- Improve testing
- Make responsibilities clear
- Reduce direct coupling
- Support future replacement of infrastructure implementations

### Dependency rule

```text
Dependencies point toward business rules.
```

Angular must never connect directly to SQL Server.

## 3. Vertical Slice

### What it is

A vertical slice is one small capability implemented through every layer necessary to deliver and verify it.

### Where we used it

The system-health slice crosses:

- Angular component
- Angular service
- TypeScript contract
- Environment configuration
- HTTP
- CORS
- ASP.NET Core routing
- Controller
- C# response contract
- Tests and builds

### Why we used it

It proves the basic full-stack wiring early instead of building many disconnected layers before receiving feedback.

### Interview answer

> The first vertical slice calls the health endpoint from Angular, routes the request to the ASP.NET Core controller, returns a typed JSON response, and displays it through an Observable and AsyncPipe. Unit tests, production builds, and a browser test verified the slice.

## 4. ASP.NET Core Controller and Routing

### What they are

A controller handles HTTP requests. Attribute routing maps an HTTP method and URL to an action.

### Where we used them

`SystemController` exposes:

```text
GET /api/system/health
```

Important attributes:

- `[ApiController]`: enables API-controller conventions
- `[Route("api/system")]`: establishes the controller route
- `[HttpGet("health")]`: maps the GET action
- `[AllowAnonymous]`: permits the health request without authentication

### Why we used them

- Clear HTTP contract
- Conventional REST-style endpoint
- Easy manual and automated verification
- Future compatibility with monitoring systems

## 5. ActionResult<T> and Typed Response

### What it is

`ActionResult<T>` allows an action to return a typed success body or another HTTP result.

### Where we used it

```csharp
public ActionResult<SystemHealthResponse> GetHealth()
```

The action returns HTTP 200 with `SystemHealthResponse`.

### Why we used it

- Makes the success contract explicit
- Supports HTTP status results
- Improves OpenAPI metadata and testability
- Provides compile-time type checking

## 6. C# Record

### What it is

A record is a concise reference type designed primarily for data-oriented models and value-based equality.

### Where we used it

`SystemHealthResponse` contains status, service, version, and UTC timestamp.

### Why we used it

The response is a small data contract that should be created and returned rather than mutated throughout the application.

## 7. CORS

### What it is

CORS is a browser-enforced policy controlling whether code from one origin may call a resource at another origin.

An origin consists of:

```text
scheme + host + port
```

### Where we used it

Angular runs at `http://localhost:4200`, and the API runs at `http://localhost:5080`. The API allows the Angular development origin.

### Why we used it

Without the permitted-origin policy, the browser would block the Angular application from reading the API response.

### Interview distinction

```text
CORS = Which browser origin may call?
Authentication = Who are you?
Authorization = What may you do?
```

CORS is not an authentication mechanism.

## 8. Angular Standalone Component

### What it is

A standalone Angular component declares its own imports and does not require an NgModule.

### Where we used it

`AppComponent` is bootstrapped directly from `main.ts`.

### Why we used it

- Current Angular architecture
- Less module boilerplate
- Dependencies are visible at the component level

## 9. Angular Service and Dependency Injection

### What they are

A service holds reusable non-visual logic. Dependency injection supplies that service to a consumer.

### Where we used them

`SystemHealthService` contains the HTTP call. `AppComponent` receives it using `inject(SystemHealthService)`.

### Why we used them

- Keeps network logic out of the component
- Reduces direct coupling
- Makes the component easier to test with a mock service
- Supports reuse

### Remember

An Angular service does not always call a server; it can also manage state, logging, transformation, or other reusable logic.

## 10. HttpClient and TypeScript Interface

### What they are

`HttpClient` sends HTTP requests. A TypeScript interface describes the expected shape of data at compile time.

### Where we used them

```typescript
this.http.get<SystemHealthResponse>(this.endpoint)
```

### Why we used them

- Typed frontend/backend contract
- Better editor support
- Safer property access and refactoring
- Easier testing

TypeScript types disappear at runtime, so production systems may still require runtime validation for untrusted data.

## 11. Angular Environment Configuration

### What it is

Angular build configurations can replace environment files for different build targets.

### Where we used it

The development file contains:

```typescript
apiBaseUrl: 'http://localhost:5080/api'
```

### Why we used it

The HTTP service does not need a hard-coded development URL inside its implementation.

### Security rule

Angular environment values are included in browser-delivered code.

```text
If the browser receives it, the user can inspect it.
```

Never store passwords, connection strings, private API keys, or client secrets there.

## 12. Observable and RxJS Operators

### What they are

An Observable represents values delivered over time. Angular `HttpClient` returns Observables.

### Where we used them

The health Observable is transformed into view state with:

- `startWith`: initial loading state
- `map`: success state
- `catchError`: user-visible error state

### Why we used them

- Represent asynchronous HTTP state
- Keep loading, success, and error handling in one stream
- Support declarative template consumption

### Execution rule

An Angular HTTP Observable begins its request when subscribed. Independent subscriptions normally create independent requests unless sharing is introduced deliberately.

## 13. AsyncPipe and OnPush

### AsyncPipe

`AsyncPipe` subscribes to the Observable in the template and manages unsubscription when appropriate.

### OnPush

`OnPush` reduces unnecessary change-detection checks when state changes are handled predictably.

### Why we used them together

Observable emissions consumed by `AsyncPipe` notify the component when new view state is available, while `OnPush` keeps change detection efficient.

## 14. Unit Tests

### Backend test

The xUnit test directly invokes `SystemController.GetHealth()` and verifies:

- HTTP 200
- Response type
- Status
- Service name
- Version
- Timestamp range

Why: verify controller behaviour quickly without starting a real HTTP server.

Limitation: it does not test routing, middleware, serialization, or CORS.

### Angular service test

The service test uses `HttpTestingController` to verify the GET request and return a controlled response.

Why: test HTTP-service behaviour without a real backend.

### Angular component test

The component test supplies a mock health service and verifies rendering.

Why: test UI behaviour independently from the network.

## 15. Unit Test vs Build vs End-to-End Check

| Verification | Purpose | Day 1 example |
|---|---|---|
| Unit test | Verify an isolated unit | Controller, Angular service, and component tests |
| Build | Compile the complete application | Detected stale `App` import in `main.ts` |
| End-to-end development check | Verify real components together | Browser displayed the live API health response |

Important lesson:

```text
Passing unit tests does not prove that the complete application builds or integrates.
```

## 16. Git Concepts

### Working tree

Files currently present in the local repository folder.

### Staging area

The exact snapshot prepared for the next commit with `git add`.

### Commit

A permanent local repository checkpoint containing author, timestamp, message, and content changes.

### Where we used them

The first commit was created on `main`:

```text
feat: establish ChangeGuard full-stack foundation
```

### Why we used them

- Traceability
- Safe recovery
- Professional history
- Future collaboration and CI/CD

A local commit is not automatically uploaded to GitHub.

## 17. Gitignore and EditorConfig

### `.gitignore`

Prevents generated or local-only content such as `node_modules`, `dist`, `bin`, and `obj` from being committed.

### `.editorconfig`

Defines formatting and code-style conventions shared by supported editors and tooling.

### Why they matter

They keep the repository small, reproducible, and consistent.

## 18. Important Day 1 Troubleshooting Lessons

### Unit tests passed but Angular build failed

Cause: focused tests did not compile the complete `main.ts` entry point, which still imported `App` after the class was renamed to `AppComponent`.

Lesson: run unit tests and full builds.

### Backend test project reported zero tests

Cause: the xUnit project existed but contained no `[Fact]` method.

Lesson: a test project is only infrastructure until real tests are discovered and executed.

### npm PowerShell scripts were blocked

Cause: PowerShell execution policy blocked `npm.ps1` and `npx.ps1`.

Solution used: `npm.cmd` and `npx.cmd`.

Lesson: use the safe executable wrapper rather than weakening system-wide execution policy unnecessarily.

### Git whitespace check failed

Cause: generated `.editorconfig` lines contained trailing spaces and an extra final blank line.

Lesson: run `git diff --cached --check` before committing.

## 19. Reviewed but Not Yet Implemented

These concepts were discussed or are planned, but Day 1 did not implement them:

| Concept | Status |
|---|---|
| EF Core and `AsNoTracking` | Reviewed; persistence not implemented |
| `IQueryable<T>` database translation | Reviewed; database query not implemented |
| Clustered and nonclustered indexes | Reviewed; SQL schema not implemented |
| Authentication and authorization | Reviewed; production security not implemented |
| Retry and circuit breaker | Reviewed; resilience policy not implemented |
| Idempotency | Reviewed; write endpoint not implemented |
| Docker image | Docker installed; project image not built |
| GitHub Actions | Planned |
| Azure resources | Planned |
| AI risk analysis | Planned |

Interview rule:

> I understand these concepts and have planned where they fit. I will describe them as implemented in ChangeGuard only after I build and verify them.

## 20. Fifteen-Minute EOD Revision Routine

### Minutes 1-3: Explain the business problem

Explain the old process, risks, ChangeGuard workflow, and expected value.

### Minutes 4-7: Draw the runtime flow from memory

```text
Component -> Service -> HTTP -> Controller -> JSON -> Observable -> UI
```

### Minutes 8-11: Explain three design decisions

- Modular monolith
- Clean Architecture
- Typed frontend/backend contract

### Minutes 12-14: Explain testing

- What each test verifies
- What it does not verify
- Why the full build was still necessary

### Minute 15: State completed versus planned work

This protects interview credibility.

## 21. Interview Self-Check

Answer without opening the document:

1. Why did we choose a modular monolith?
2. What is the dependency direction in Clean Architecture?
3. What is a vertical slice?
4. Describe the complete health-request flow.
5. Why is `ActionResult<T>` useful?
6. Why did we use a record for the response?
7. What is an origin in CORS?
8. How is CORS different from authentication and authorization?
9. Why is HTTP logic in an Angular service?
10. When does an Angular HTTP Observable start?
11. What do `startWith`, `map`, and `catchError` do here?
12. Why use `AsyncPipe`?
13. What did each unit test verify?
14. Why did the tests pass while the production build failed?
15. What is the difference between staging and committing in Git?
16. Which technologies are planned but not yet implemented?

## 22. Ninety-Second Project Explanation

> ChangeGuard AI is a requirement-to-release risk intelligence platform intended to make software changes traceable and safe. I began by documenting the business problem, stakeholders, requirements, acceptance criteria, risks, and constraints. Because the initial team is small and there is no proven need for independent service deployment, I selected a modular monolith using Clean Architecture. On Day 1, I created the .NET 10 solution and Angular 22 frontend, implemented a typed health endpoint, configured restricted development CORS, and connected Angular through HttpClient and an Observable-based view state. I added xUnit and Angular tests, ran complete production builds, and verified the real browser-to-API flow. Database persistence, security, Docker packaging, CI/CD, Azure deployment, and AI analysis are planned future slices and are not yet presented as completed.
