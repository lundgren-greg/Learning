# React Permissions Lab

An exercise in calling a C# backend API from a React / TypeScript frontend.

## What is already built

| Thing | Location | Status |
|---|---|---|
| C# minimal API with a `/api/permissions` endpoint | `backend/PermissionsApi/` | ✅ Done |
| Radio-button mode toggle (User / Admin) | `frontend/src/components/ModeToggle.tsx` | ✅ Done |
| `AdminPanel` component (secret panel) | `frontend/src/components/AdminPanel.tsx` | ✅ Done |
| "Hello World" display | `frontend/src/App.tsx` | ✅ Done |
| `fetchPermissions` service function | `frontend/src/services/permissionsService.ts` | ⬜ Stub — **your TODO** |
| Wire API call to mode change | `frontend/src/App.tsx` (TODO #1) | ⬜ **Your TODO** |
| Conditionally show Admin Panel | `frontend/src/App.tsx` (TODO #2) | ⬜ **Your TODO** |

---

## Project structure

```
react-permissions-lab/
├── backend/
│   └── PermissionsApi/        ← ASP.NET Core minimal API
│       ├── Program.cs
│       └── PermissionsApi.csproj
└── frontend/                  ← Vite + React + TypeScript
    ├── src/
    │   ├── App.tsx            ← Main app (TODO #1 and TODO #2 are here)
    │   ├── components/
    │   │   ├── AdminPanel.tsx
    │   │   └── ModeToggle.tsx
    │   └── services/
    │       └── permissionsService.ts   ← TODO: implement fetchPermissions
    └── package.json
```

---

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)

---

## Running the app

### 1 — Start the backend API

```bash
cd backend/PermissionsApi
dotnet run --launch-profile http
```

The API will be available at **http://localhost:5081**.

You can test it immediately with curl or a browser:

```bash
# User permissions
curl "http://localhost:5081/api/permissions?role=user"
# → {"role":"user","hasAdminAccess":false}

# Admin permissions
curl "http://localhost:5081/api/permissions?role=admin"
# → {"role":"admin","hasAdminAccess":true}
```

### 2 — Start the frontend

Open a **second** terminal:

```bash
cd frontend
npm install      # first time only
npm run dev
```

The app will open at **http://localhost:5173**.

---

## Your TODOs

Find **TODO #1** and **TODO #2** in `frontend/src/App.tsx`. Detailed instructions
are in comments directly above each TODO marker.

### TODO #1 — Call the API when the mode changes (`App.tsx`)

1. Add a `permissions` state variable.
2. Create a `handleModeChange` async function that calls `fetchPermissions` from
   `permissionsService.ts` and stores the result.
3. Wire `handleModeChange` to the `<ModeToggle>` component.

### TODO #2 — Conditionally render the Admin Panel (`App.tsx`)

Replace the hardcoded `false` in `<AdminPanel visible={false} />` with the
`hasAdminAccess` field from the permissions state you built in TODO #1.

### Implementing `fetchPermissions` (`permissionsService.ts`)

Open `frontend/src/services/permissionsService.ts`. The function signature and
expected return type are already defined. Follow the inline instructions to
replace the stub with a real `fetch` call to the backend.

---

## Quick API reference

| Method | URL | Query param | Response |
|--------|-----|-------------|----------|
| GET | `/api/permissions` | `role=user` | `{ "role": "user", "hasAdminAccess": false }` |
| GET | `/api/permissions` | `role=admin` | `{ "role": "admin", "hasAdminAccess": true }` |
