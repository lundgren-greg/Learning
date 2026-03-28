# Skill: Troubleshoot API Connectivity

Use this when frontend calls do not reach backend or fail unexpectedly.

## Quick checks
1. Backend is running with launch profile `http` on `http://localhost:5081`.
2. Frontend is running on `http://localhost:5173`.
3. Backend CORS policy in `backend/PermissionsApi/Program.cs` still includes `http://localhost:5173`.
4. Frontend API base URL points to `http://localhost:5081`.

## Fast diagnostics
- Open browser DevTools Network tab and inspect `GET /api/permissions`.
- Test backend directly in a terminal:
  - `curl "http://localhost:5081/api/permissions?role=user"`
  - `curl "http://localhost:5081/api/permissions?role=admin"`
- If requests fail, restart backend and re-check selected launch profile.

