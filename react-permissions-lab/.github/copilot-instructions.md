# Copilot Instructions for React Permissions Lab

## Goal
Use this repository to learn full-stack wiring between a React frontend and an ASP.NET Core backend.

## Project map
- Frontend: `frontend/` (Vite + React + TypeScript)
- Backend: `backend/PermissionsApi/` (ASP.NET Core minimal API)
- API endpoint: `GET /api/permissions?role=user|admin`

## Working rules
- Prefer small, focused changes tied to a single exercise or bug.
- Keep API contract stable unless explicitly changing both frontend and backend.
- For frontend calls, use `frontend/src/services/permissionsService.ts` as the integration point.
- For backend behavior, keep logic in `backend/PermissionsApi/Program.cs` simple and readable.
- Preserve `http://localhost:5081` backend and `http://localhost:5173` frontend defaults unless asked otherwise.

## Local run and debug
- Backend in Rider: run `backend/PermissionsApi` with launch profile `http`.
- Frontend in VS Code: run from `frontend/` using `npm run dev` or `pnpm run dev`.
- Validate integration by switching modes and confirming API calls in browser DevTools.

## Safety checks before finishing a change
1. Backend still starts with launch profile `http`.
2. Frontend still starts with Vite dev server.
3. Toggling User/Admin reflects API response behavior.
4. No unrelated files were modified.

