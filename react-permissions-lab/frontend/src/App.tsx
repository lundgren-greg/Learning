import {useCallback, useState} from "react";
import { AdminPanel } from "./components/AdminPanel";
import { ModeToggle } from "./components/ModeToggle";
import {fetchPermissions, type PermissionsResponse} from "./services/permissionsService";
import "./App.css";

type Mode = "user" | "admin";

function App() {
  const [mode, setMode] = useState<Mode>("user");
  const [permission, setPermission] = useState<PermissionsResponse>();

  //  (Exercise 2): When the user switches the radio button, call
  //   fetchPermissions with the selected role and store the API response
  //   in component state.
  //
  // Acceptance criteria:
  //  - Selecting "Admin" calls the API with role "admin".
  //  - Selecting "User" calls the API with role "user".
  //  - The API response is available to the rest of the component.
const radioButtonSwitched = useCallback(async (nextMode: Mode) => {
  try{

  let result:PermissionsResponse  =  await fetchPermissions(nextMode);
console.log(result);
console.log("Greg")
    // if successful then update
  setPermission(result);
    setMode(nextMode)
  }
  catch(err) {
    console.log(`Error fetching permissions: ${err}, defaulting to user`);
    setPermission({role: "user", hasAdminAccess: false});
    setMode("user");
  }
},[]);


  // TO DO (Exercise 3): Show the AdminPanel only when the API says the
  //   user has admin access. Right now it is hardcoded to visible={false}.
  //
  // Acceptance criteria:
  //  - Switching to "Admin" shows the secret admin panel.
  //  - Switching back to "User" hides it.
  //  - The visibility is driven by the API response, not the local mode state.

  return (
    <div className="app">
      <h1>Permissions Demo</h1>

      <ModeToggle mode={mode} onChange={radioButtonSwitched} />

      <div className="content">
        <p className="hello-world">Hello World</p>

        <AdminPanel visible={permission?.hasAdminAccess ?? false}/>
      </div>
    </div>
  );
}

// Keep the import used so TypeScript/ESLint don't flag it as unused while the
// exercises are still pending.
void fetchPermissions;

export default App;
