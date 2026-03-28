type Mode = "user" | "admin";

interface ModeToggleProps {
  mode: Mode;
  onChange: (mode: Mode) => void;
}

/**
 * Radio-button toggle that lets you switch between User mode and Admin mode.
 * Selecting a mode should trigger an API call to retrieve the appropriate
 * permissions (see the in App.tsx).
 */
export function ModeToggle({ mode, onChange }: ModeToggleProps) {
  return (
    <fieldset className="mode-toggle">
      <legend>Select mode</legend>

      <label>
        <input
          type="radio"
          name="mode"
          value="user"
          checked={mode === "user"}
          onChange={() => onChange("user")}
        />
        User
      </label>

      <label>
        <input
          type="radio"
          name="mode"
          value="admin"
          checked={mode === "admin"}
          onChange={() => onChange("admin")}
        />
        Admin
      </label>
    </fieldset>
  );
}
