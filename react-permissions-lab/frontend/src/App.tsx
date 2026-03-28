import { useState } from "react";
import { AdminPanel } from "./components/AdminPanel";
import { ModeToggle } from "./components/ModeToggle";
import { fetchPermissions } from "./services/permissionsService";
import "./App.css";

type Mode = "user" | "admin";

function App() {
  const [mode, setMode] = useState<Mode>("user");

  // TODO #1 — Call the API when the mode changes.
  //
  // Currently the mode is stored in local state but no API call is made.
  // Your task:
  //   1. Add a `permissions` state variable to hold the `PermissionsResponse`
  //      returned by the API (import the type from ./services/permissionsService).
  //   2. Create a `handleModeChange` function that:
  //        a. Updates the `mode` state.
  //        b. Calls `fetchPermissions(newMode)` from permissionsService.ts.
  //        c. Stores the result in the `permissions` state.
  //        d. Handles any errors (e.g., show an error message in the UI).
  //   3. Pass `handleModeChange` to <ModeToggle onChange={...}> instead of the
  //      plain `setMode` that is currently wired up.
  //
  // Hint: because fetchPermissions is async, use async/await inside the handler
  // or chain .then()/.catch() on the returned Promise.

  // TODO #2 — Conditionally render the AdminPanel based on API permissions.
  //
  // Right now the AdminPanel is never shown (visible={false}).
  // Once you complete TODO #1 you will have a `permissions` object.
  // Replace `false` below with `permissions?.hasAdminAccess ?? false` so
  // that the panel appears only when the API says the user is an admin.

  return (
    <div className="app">
      <h1>Permissions Demo</h1>

      <ModeToggle mode={mode} onChange={setMode} />

      <div className="content">
        <p className="hello-world">Hello World</p>

        {/* TODO #2: Replace `false` with the hasAdminAccess value from the API */}
        <AdminPanel visible={false} />
      </div>
    </div>
  );
}

// Keep the import used so TypeScript/ESLint don't flag it as unused while the
// TODOs are still pending.
void fetchPermissions;

export default App;
