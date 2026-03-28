# Skill: Full-Stack Dev Loop

Use this when you need to run and debug both apps during an exercise.

## Checklist
1. Start backend with `http` profile at `http://localhost:5081`.
2. Start frontend Vite dev server at `http://localhost:5173`.
3. Toggle User/Admin and verify `GET /api/permissions?role=...` calls.
4. Confirm UI behavior matches backend response.

## Recommended scripts
- `scripts/start-backend-http.ps1`
- `scripts/start-frontend.ps1`

## Debug breakpoints
- Backend: `backend/PermissionsApi/Program.cs` inside `app.MapGet("/api/permissions", ...)`.
- Frontend: `frontend/src/services/permissionsService.ts` in `fetchPermissions`.

