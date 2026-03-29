# React Permissions Lab

An exercise in calling a C# backend API from a React / TypeScript frontend.

## What is already built

| Component | Location |
|---|---|
| C# minimal API — `GET /api/permissions?role=user\|admin` | `backend/PermissionsApi/Program.cs` |
| Radio-button mode toggle (User / Admin) | `frontend/src/components/ModeToggle.tsx` |
| Secret admin panel component | `frontend/src/components/AdminPanel.tsx` |
| Service function stub & response type | `frontend/src/services/permissionsService.ts` |
| Main app shell with "Hello World" | `frontend/src/App.tsx` |

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
    │   ├── App.tsx
    │   ├── components/
    │   │   ├── AdminPanel.tsx
    │   │   └── ModeToggle.tsx
    │   └── services/
    │       └── permissionsService.ts
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

Verify it works:

```bash
curl "http://localhost:5081/api/permissions?role=user"
# → {"role":"user","hasAdminAccess":false}

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

## Exercises

There are three TODO exercises across two files. Complete them in order.

### Exercise 1 — Implement `fetchPermissions` (`permissionsService.ts`)

The function signature, return type, and API endpoint are provided. Make it call
the backend and return a typed response.

### Exercise 2 — Call the API on mode change (`App.tsx`)

When the user toggles the radio button, call your `fetchPermissions` function and
store the result in component state.

### Exercise 3 — Conditionally render the admin panel (`App.tsx`)

Use the stored API response to control the `visible` prop on `<AdminPanel>`.
Switching to Admin should show the panel; switching back to User should hide it.

---

## How to verify your solution

1. Start both the backend and frontend (see above).
2. Open http://localhost:5173 — you should see "Hello World" and the mode toggle.
3. Select **Admin** → the admin panel should appear below "Hello World".
4. Select **User** → the admin panel should disappear.
5. Open the browser Network tab and confirm a `GET /api/permissions?role=...`
   request fires each time you change mode.

---

## API reference

| Method | URL | Query param | Response |
|--------|-----|-------------|----------|
| GET | `/api/permissions` | `role=user` | `{ "role": "user", "hasAdminAccess": false }` |
| GET | `/api/permissions` | `role=admin` | `{ "role": "admin", "hasAdminAccess": true }` |

---

## Planned features (TODO)

### Step 1 — Add .NET Aspire DB backend

Integrate a hosted database into the project using [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) so the API has a persistent data store.

- [ ] Create an Aspire AppHost project at the solution root.
- [ ] Add a database resource (e.g. PostgreSQL or SQL Server) to the AppHost.
- [ ] Wire the connection string through to `PermissionsApi` via Aspire service defaults.
- [ ] Register a `DbContext` (e.g. `PermissionsDbContext`) in `Program.cs`.

### Step 2 — Define the database schema

Create the initial table to record each role selection.

```
Table: RoleSelections
┌─────────────┬──────────────────────┬──────────────────────────────────┐
│ Column      │ Type                 │ Notes                            │
├─────────────┼──────────────────────┼──────────────────────────────────┤
│ Id          │ INT IDENTITY PK      │ Auto-increment primary key       │
│ Role        │ NVARCHAR(50) NOT NULL│ The role that was requested      │
│ SelectedAt  │ DATETIMEOFFSET       │ UTC timestamp of the selection   │
│ UserId      │ NVARCHAR(100) NULL   │ NULL until multi-user is added   │
└─────────────┴──────────────────────┴──────────────────────────────────┘
```

- [ ] Add EF Core models and the initial migration (`dotnet ef migrations add InitialCreate`).
- [ ] Apply the migration on startup (or via a deploy step).

### Step 3 — Track the last selection

- [ ] Persist each role selection to `RoleSelections` inside the `GET /api/permissions` handler.
- [ ] Add a `GET /api/permissions/last` endpoint that returns the most recently persisted row so the frontend can restore the last selection on page load.

### TODO (future) — Multiple users

> **Note:** This step will require a **database migration** to add a `Users` table and a
> foreign key from `RoleSelections.UserId` to `Users.Id`.

- [ ] Add a `Users` table to the schema.
- [ ] Capture the authenticated user's identifier (from `HttpContext.User`) when persisting a selection.
- [ ] Write an EF Core migration: `dotnet ef migrations add AddUsersTable`.
- [ ] Update `GET /api/permissions/last` to be user-scoped.
