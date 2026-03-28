interface AdminPanelProps {
  visible: boolean;
}

/**
 * A secret panel that should only be visible to admin users.
 * The `visible` prop is currently hardcoded in App.tsx — your TODO is to drive
 * it from the API response instead.
 */
export function AdminPanel({ visible }: AdminPanelProps) {
  if (!visible) return null;

  return (
    <div className="admin-panel">
      <h2>🔒 Admin Panel</h2>
      <p>Welcome, administrator! Here are your secret controls:</p>
      <ul>
        <li>Manage users</li>
        <li>View audit logs</li>
        <li>Configure system settings</li>
      </ul>
    </div>
  );
}
