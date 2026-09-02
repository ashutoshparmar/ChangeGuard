# ChangeGuard MVP Acceptance Walkthrough

Use this walkthrough after opening the completed project on the Windows development machine. It verifies business behaviour, not only whether the code compiles.

## 1. Build the backend in Visual Studio

1. Open `ChangeGuard.slnx`.
2. Select **Build > Rebuild Solution**.
3. Open **Test Explorer** and run all tests.
4. Expected backend result: **17 tests passed**.
5. Start `ChangeGuard.Api` with the `https` launch profile.
6. Confirm the Output window says it is listening on `https://localhost:7110`.

Development startup applies the pending `CompleteChangeRequestWorkflow` migration automatically. Confirm:

- `https://localhost:7110/api/system/health` returns a Healthy response.
- `https://localhost:7110/health/live` returns Healthy.
- `https://localhost:7110/health/ready` returns Healthy and proves SQL Server is reachable.

If startup reports a migration error, stop and capture the full error before changing the database manually.

## 2. Start React

From a terminal in `src\ChangeGuard.React`:

```powershell
npm.cmd install
npm.cmd run test:ci
npm.cmd run dev
```

Expected frontend result: **7 tests passed**. Open `http://localhost:4200` and confirm the header shows **Healthy**.

## 3. Run the business mission

Create this request:

| Field | Value |
|---|---|
| Reference | `CG-501` |
| Title | `Prevent duplicate payment settlement` |
| Description | `Reject a repeated settlement identifier and preserve evidence for support.` |
| Priority | `Critical` |

Expected result: the request is stored as **Draft**, appears in Request Radar, and has a calculated SLA deadline.

Open `CG-501` and execute the following sequence:

| Step | Action | Expected result |
|---|---|---|
| 1 | Submit for review | Status becomes `Requirement Review` |
| 2 | Start development | Status becomes `In Development` |
| 3 | Save a rollback plan | Rollback gate becomes complete |
| 4 | Start QA testing | Status becomes `QA Testing` |
| 5 | Try Complete QA before evidence | API rejects the action; status remains `QA Testing` |
| 6 | Save QA evidence notes | Readiness becomes 100% and both gates are complete |
| 7 | Complete QA testing | Status becomes `Release Approval` |
| 8 | Approve release | Status becomes `Released` |
| 9 | Close request | Status becomes `Closed` |

The intentional failure in step 5 proves the rule is enforced on the server. It is not merely a disabled React button.

## 4. Verify auditability

Open the request and review its Audit Trail. It should contain creation, evidence, and workflow entries with actor, time, comment, previous status, and new status.

The request update and its audit entry are sent to EF Core and committed with one `SaveChangesAsync` call. This prevents a successful status update from being stored without its corresponding history under normal transaction handling.

## 5. Verify duplicate protection

Try to create `CG-501` again.

Expected result: the API returns HTTP **409 Conflict** and React displays the duplicate-reference message. The application performs an early existence check for usability, while the unique SQL index remains the concurrency-safe final defence.

## 6. Verify filters and errors

1. Search for `payment`.
2. Filter priority to `Critical`.
3. Filter status to `Closed`.
4. Clear all filters.
5. Stop the API and refresh React; confirm it displays an actionable connection error rather than silently showing empty data.

## 7. Production build gate

Run:

```powershell
cd C:\Users\Ashup\source\repos\ChangeGuard
dotnet build .\ChangeGuard.slnx --configuration Release
dotnet test .\ChangeGuard.slnx --configuration Release --no-build

cd .\src\ChangeGuard.React
npm.cmd run test:ci
npm.cmd run build
```

Do not merge or begin Azure deployment until all four commands succeed.
