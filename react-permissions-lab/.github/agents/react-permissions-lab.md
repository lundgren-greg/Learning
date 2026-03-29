---
description: Coding agent for the React Permissions Lab — a full-stack exercise connecting a React/TypeScript frontend to an ASP.NET Core backend.
---

# React Permissions Lab — Agent Context

## What this project is

A hands-on exercise in wiring a **React + TypeScript** frontend (Vite) to a **C# ASP.NET Core** minimal API. The user toggles between User and Admin roles; the frontend calls the backend and uses the response to show or hide a secret admin panel.

---

## Project layout

```
react-permissions-lab/
├── .github/
│   ├── agents/
│   │   └── react-permissions-lab.md   ← this file
│   └── copilot-instructions.md
├── .skills/
│   ├── fullstack-dev-loop.md
│   └── troubleshoot-api-connectivity.md
├── backend/
│   ├── PermissionsApi/
│   │   ├── Program.cs                 ← API logic (single file)
│   │   ├── PermissionsApi.csproj
│   │   ├── Properties/launchSettings.json
│   │   └── global.json
│   └── PermissionsApi.Tests/
│       ├── PermissionsEndpointTests.cs
│       └── PermissionsApi.Tests.csproj
└── frontend/
    ├── src/
    │   ├── App.tsx                    ← main component (exercises 2 & 3)
    │   ├── components/
    │   │   ├── AdminPanel.tsx
    │   │   └── ModeToggle.tsx
    │   └── services/
    │       └── permissionsService.ts  ← exercise 1: fetchPermissions
    └── package.json
```

---

## API contract

| Method | URL | Query param | Response |
|--------|-----|-------------|----------|
| GET | `/api/permissions` | `role=user` | `{ "role": "user", "hasAdminAccess": false }` |
| GET | `/api/permissions` | `role=admin` | `{ "role": "admin", "hasAdminAccess": true }` |

- Backend base URL: `http://localhost:5081`
- Frontend dev URL: `http://localhost:5173`
- CORS policy in `Program.cs` whitelists `http://localhost:5173`
- TypeScript interface: `PermissionsResponse` in `permissionsService.ts`

---

## Key files and their roles

| File | Purpose |
|------|---------|
| `backend/PermissionsApi/Program.cs` | Entire backend: CORS setup, single `MapGet` endpoint, `PermissionsResponse` record |
| `frontend/src/services/permissionsService.ts` | `fetchPermissions(role)` — the only place that calls the API |
| `frontend/src/App.tsx` | Root component; owns `mode` and `permission` state; passes props to `ModeToggle` and `AdminPanel` |
| `frontend/src/components/ModeToggle.tsx` | Radio-button toggle; emits selected mode to parent |
| `frontend/src/components/AdminPanel.tsx` | Hidden panel; rendered based on `visible` prop |

---

## Working rules

- Prefer small, focused changes tied to a single exercise or bug.
- Keep the API contract stable unless explicitly changing both frontend and backend together.
- Frontend API integration lives in `permissionsService.ts` — do not call `fetch` from components directly.
- Backend logic stays in `Program.cs`; keep it simple and readable.
- Do not change the default ports (`5081` backend, `5173` frontend) unless explicitly asked.
- Match existing code style: `const`/arrow functions in TypeScript, C# record types in the backend.

---

## Running the project locally

### Backend
```bash
cd backend/PermissionsApi
dotnet run --launch-profile http
# API available at http://localhost:5081
```

Verify with:
```bash
curl "http://localhost:5081/api/permissions?role=user"
# → {"role":"user","hasAdminAccess":false}

curl "http://localhost:5081/api/permissions?role=admin"
# → {"role":"admin","hasAdminAccess":true}
```

### Frontend
```bash
cd frontend
npm install      # first time only
npm run dev
# App available at http://localhost:5173
```

PowerShell convenience scripts are in `scripts/`:
- `scripts/start-backend-http.ps1`
- `scripts/start-frontend.ps1`

---

## Running tests

```bash
# Backend unit tests
cd backend
dotnet test
```

Frontend does not currently have automated tests; verify manually in the browser.

---

## Exercises (what students are implementing)

1. **`permissionsService.ts`** — implement `fetchPermissions` to call `GET /api/permissions?role=<role>` and return a typed `PermissionsResponse`.
2. **`App.tsx`** — call `fetchPermissions` when the mode toggle changes and store the result in state.
3. **`App.tsx`** — drive the `visible` prop of `<AdminPanel>` from `permission.hasAdminAccess`.

---

## Safety checks before finishing any change

1. Backend starts cleanly with `dotnet run --launch-profile http`.
2. Frontend starts cleanly with `npm run dev`.
3. Toggling User → Admin shows the admin panel and fires a `GET /api/permissions?role=admin` request.
4. Toggling Admin → User hides the admin panel and fires a `GET /api/permissions?role=user` request.
5. No unrelated files were modified.

---

## Skills reference

For repeatable workflows, see `.skills/`:
- **`fullstack-dev-loop.md`** — start, debug, and verify both apps together.
- **`troubleshoot-api-connectivity.md`** — diagnose frontend-to-backend connection issues.
