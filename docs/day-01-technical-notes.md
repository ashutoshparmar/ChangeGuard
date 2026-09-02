# Day 1 Technical and Interview Notes

## 1. Day 1 Objective

The objective was to establish a production-style foundation and verify the first end-to-end vertical slice between Angular and ASP.NET Core.

Verified flow:

```text
Angular component
-> Angular service
-> HttpClient
-> ASP.NET Core endpoint
-> Typed JSON response
-> Observable
-> AsyncPipe
-> Browser status
```

## 2. Vertical Slice

A vertical slice is one small feature implemented through every layer required to deliver visible value.

The Day 1 slice included:

- Angular UI
- Angular HTTP service
- TypeScript response contract
- Environment-based API URL
- ASP.NET Core routing and controller
- Typed backend response
- CORS configuration
- Unit-test verification
- Production builds
- Real browser-to-API verification

It did not include database persistence, security, the core workflow, AI, Docker packaging, CI/CD, or Azure deployment.

Memory statement:

```text
Horizontal work completes one layer.
Vertical-slice work proves one capability end to end.
```

## 3. Requirement Vocabulary

| Term | Meaning | Memory aid |
|---|---|---|
| Functional requirement | What the system must do | Capability |
| Non-functional requirement | How well the system must operate | Quality |
| Business rule | An organisational policy or condition | Policy |
| Acceptance criteria | Testable conditions for accepting a requirement | Proof |
| Assumption | Believed true but not fully confirmed | Believed |
| Dependency | Something needed before another activity can complete | Needed |
| Risk | An uncertain event that may cause harm | Might happen |
| Constraint | A limitation within which the solution must operate | Limited by |

Detailed product examples are maintained in `product-requirements.md` rather than duplicated here.

## 4. Dependency Injection Lifetimes

### Transient

A new instance is supplied each time the dependency is resolved from the container.

Use for lightweight, stateless services when sharing an instance is unnecessary.

### Scoped

One instance is supplied within one dependency-injection scope. In ASP.NET Core, one HTTP request normally creates one scope.

Entity Framework Core `DbContext` is commonly scoped.

### Singleton

One instance is shared for the application lifetime.

Singleton services must be thread-safe when used concurrently. A singleton must not directly capture a scoped service.

Memory aid:

```text
Transient = Every resolution
Scoped = One request
Singleton = Application lifetime
```

## 5. Async and Await

`async` and `await` allow code to wait for asynchronous work without synchronously blocking a request thread.

Typical I/O operations include:

- Database calls
- HTTP calls
- File operations
- Cloud storage
- Message brokers

When an incomplete I/O task is awaited, control returns to the caller. `async` does not automatically create a new thread.

Avoid blocking asynchronous work with `.Result` or `.Wait()`. Use asynchronous calls through the complete request path when possible.

## 6. IEnumerable and IQueryable

### IEnumerable<T>

Represents a sequence enumerated by .NET. LINQ applied after materialisation operates on in-memory objects.

### IQueryable<T>

Builds an expression tree that a provider such as Entity Framework Core may translate into SQL.

```csharp
var users = await dbContext.Users
    .Where(user => user.IsActive)
    .Select(user => new { user.Id, user.Name })
    .ToListAsync();
```

Here, supported filtering and projection are translated for database execution. `ToListAsync` executes and materialises the query.

Avoid loading a complete table before applying filters that the database can perform.

## 7. AsNoTracking

EF Core tracks entities by default so that changes can later be detected and saved.

For read-only queries:

```csharp
var requests = await dbContext.ChangeRequests
    .AsNoTracking()
    .ToListAsync();
```

Benefits can include lower tracking overhead and memory use. Do not use it blindly when the same entities must be updated through normal change tracking.

Memory aid:

```text
Read only = Consider AsNoTracking
Update required = Tracking may be needed
```

## 8. HTTP 401 and 403

### 401 Unauthorized

Authentication did not succeed. The token may be missing, invalid, or expired.

### 403 Forbidden

Authentication succeeded, but the identity lacks permission for the requested action.

Memory aid:

```text
401 = Who are you?
403 = I know you, but you cannot do this.
```

## 9. Clustered and Nonclustered Indexes

### Clustered index

The leaf level contains the table's data rows. A table can have only one clustered index.

### Nonclustered index

It is a separate index structure whose leaf level contains indexed values, a row locator, and optional included columns. A table can have multiple nonclustered indexes.

Indexes can improve reads but increase storage and write-maintenance cost.

Memory aid:

```text
Clustered leaf = Data rows
Nonclustered leaf = Keys and row locator
```

## 10. Retry and Circuit Breaker

### Retry

Repeats an operation when a failure is likely to be transient. Production retries normally use a limit, backoff, jitter, and timeouts.

Do not retry permanent validation or business failures.

### Circuit breaker

Temporarily blocks calls to a repeatedly failing dependency.

- Closed: calls are allowed.
- Open: calls fail quickly.
- Half-open: a limited call tests recovery.

Memory aid:

```text
Retry = Try a transient failure again
Circuit breaker = Stop repeatedly calling an unhealthy dependency
```

## 11. Idempotency

An operation is idempotent when repeating the same request has the same intended business effect as executing it once.

An application-level "check then insert" is not sufficient under concurrency because two requests may pass the check simultaneously.

Protections can include:

- Database unique constraints
- Idempotency keys
- Transactions
- Concurrency control
- Processed-message records

## 12. Angular Component

A component controls part of the user interface. It combines TypeScript behaviour, a template, styles, and Angular metadata.

The Day 1 root component builds a health view state and exposes it to the template.

## 13. Angular Service

A service contains reusable logic that does not belong directly in the visual component.

Services can support HTTP communication, authentication, state, logging, or data transformation. A service does not always call a server.

`SystemHealthService` owns the health-endpoint call so the component does not construct or send HTTP requests itself.

## 14. Angular Dependency Injection

Angular creates and supplies registered dependencies.

```typescript
private readonly systemHealthService =
  inject(SystemHealthService);
```

This reduces direct coupling and enables test replacement.

## 15. Observable

An Observable represents values over time. Angular `HttpClient` returns Observables.

For an HTTP Observable:

- Subscription starts the request.
- Each independent subscription normally creates another request.
- It emits a response or error and then completes.
- RxJS operators can transform and handle its state.

## 16. AsyncPipe

`AsyncPipe` subscribes to an Observable in the template and manages unsubscription when appropriate.

It reduces manual subscription lifecycle code.

## 17. OnPush Change Detection

`OnPush` reduces unnecessary Angular change-detection work when state is managed predictably.

It is a performance and design choice, not a replacement for correct state management.

## 18. Angular Environment Files

The development environment stores a public API base URL:

```typescript
apiBaseUrl: 'http://localhost:5080/api'
```

Angular environment files are included in browser-delivered code. They must not contain passwords, private API keys, connection strings, signing keys, or client secrets.

Memory aid:

```text
If the browser receives it, the user can inspect it.
```

## 19. CORS

CORS means Cross-Origin Resource Sharing.

The Angular origin is `http://localhost:4200`, while the API origin is `http://localhost:5080`. The API permits the development Angular origin.

CORS is a browser-origin policy. It does not establish identity or permissions.

```text
CORS = Browser origin permission
Authentication = Identity
Authorization = Permission
```

## 20. Typed API Contract

The API returns status, service name, version, and UTC timestamp. Angular defines a matching TypeScript interface.

Typed contracts provide:

- Compile-time checking
- Better editor support
- Clear frontend/backend communication
- Safer refactoring
- Easier tests

## 21. Testing Lessons

### Unit test

Tests a focused class or component while replacing external dependencies. The Angular HTTP test uses a testing controller rather than a real API.

### Build verification

Compiles the complete application. This detected the `App` versus `AppComponent` mismatch in `main.ts` even though focused tests passed.

### End-to-end development verification

Runs the real Angular application and real API together. Seeing the live `Healthy` response confirmed their integration.

Memory aid:

```text
Passing unit tests does not guarantee that the complete application builds or integrates.
```

## 22. Interview Explanation

> I started ChangeGuard AI as a production-style requirement-to-release risk platform. I first performed business discovery and identified problems such as incomplete requirements, missing QA evidence, SLA breaches, and absent rollback plans. Because the target client has a small team, I selected a modular monolith with Clean Architecture instead of introducing premature microservices complexity. I created separate Domain, Application, Infrastructure, and API projects using .NET 10, together with an Angular 22 frontend. I implemented a typed health endpoint, configured a restricted development CORS policy, and created an Angular service using HttpClient and Observables. I added frontend unit tests, verified production builds, and completed an end-to-end browser test. Future phases will add the core workflow, SQL Server persistence, security, Docker, CI/CD, and Azure deployment.

## 23. Client Status Update

> The initial solution foundation has been completed. We documented the core business problem, stakeholders, requirements, and architectural constraints. The .NET API and Angular application are running locally, and end-to-end communication has been verified through the system-health endpoint. Frontend unit tests and production builds are passing. The next step is to implement the core change-request domain model and workflow while maintaining test coverage and traceability.

## 24. Self-Check Questions

1. What is a vertical slice?
2. What complete flow did the Day 1 vertical slice verify?
3. Why did we select a modular monolith?
4. What belongs in the Domain project?
5. Why must Angular not connect directly to SQL Server?
6. What is the difference between a functional requirement and a business rule?
7. What is the difference between acceptance criteria and a test case?
8. What is the difference between Transient, Scoped, and Singleton?
9. Does `async` automatically create a new thread?
10. When should `AsNoTracking` be considered?
11. What is the difference between HTTP 401 and 403?
12. Why can a unit test pass while the full application build fails?
13. What does CORS control?
14. What is an idempotent operation?
15. What is the difference between retry and circuit breaker?
16. Why must secrets never be stored in Angular environment files?

## 25. Answer to the Vertical-Slice Question

The Day 1 vertical slice verified the complete flow from the Angular component to an injected Angular service, through `HttpClient` to the ASP.NET Core health endpoint, and back as a typed JSON response delivered through an Observable and rendered by `AsyncPipe` in the browser.
