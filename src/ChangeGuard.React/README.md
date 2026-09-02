# ChangeGuard React Frontend

This is the active ChangeGuard operator interface. It replaces the original Angular frontend while preserving every ASP.NET Core API contract and backend business rule.

## Local development

Start `ChangeGuard.Api` with the Visual Studio `https` profile, then run:

```powershell
cd .\src\ChangeGuard.React
npm.cmd install
npm.cmd run dev
```

Open `http://localhost:4200`.

The development API URL is configured in `.env.development`. If Visual Studio uses a different API port, update `VITE_API_BASE_URL` there and restart Vite.

## Verification

```powershell
npm.cmd run test:ci
npm.cmd run build
```

## Structure

- `src/core` contains system-wide models and services.
- `src/features/change-requests` contains the complete MVP feature.
- `src/shared/http` contains the typed Fetch wrapper and `ProblemDetails` handling.
- `src/test` contains shared Vitest setup.

Vite injects `VITE_*` values into browser-delivered code. Never place passwords, connection strings, signing keys, or client secrets in these files.
