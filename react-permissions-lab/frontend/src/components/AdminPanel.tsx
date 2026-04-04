import Button from "@mui/material/Button";
import { useState } from "react";
import {
  sendPowerOnCommand,
  type DevicePowerResponse,
} from "../services/deviceControlService";

interface AdminPanelProps {
  visible: boolean;
}

/**
 * A secret panel that should only be visible to admin users.
 * The `visible` prop is currently hardcoded in App.tsx — your task was to drive
 * it from the API response instead.
 */
export function AdminPanel({ visible }: AdminPanelProps) {
  const [isSending, setIsSending] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastResult, setLastResult] = useState<DevicePowerResponse | null>(null);

  if (!visible) return null;

  async function sendPowerOn() {
    setIsSending(true);
    setError(null);

    try {
      const result = await sendPowerOnCommand();
      setLastResult(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unknown command error");
    } finally {
      setIsSending(false);
    }
  }

  return (
    <div className="admin-panel">
      <h2>🔒 Admin Panel</h2>
      <p>Welcome, administrator! Here are your secret controls:</p>
      <ul>
        <li>Manage users</li>
        <li>View audit logs</li>
        <li>Configure system settings</li>
      </ul>

      <div style={{ display: "flex", gap: "0.75rem" }}>
        <Button
          variant="contained"
          id="power-on-button"
          onClick={sendPowerOn}
          disabled={isSending}
        >
          Power On
        </Button>
      </div>

      {/* TODO(power-off): Add a Power Off button with id="power-off-button" and wire it to sendPowerOffCommand(). */}
      {/* TODO(power-off): Mirror loading/error/result handling for the off flow once service + backend support are enabled. */}

      {isSending && <p>Sending device command...</p>}
      {error && <p style={{ color: "crimson" }}>Error: {error}</p>}
      {lastResult && (
        <p>
          Last command: {lastResult.receivedCommand}{" -> "}
          {lastResult.deviceCommand}
        </p>
      )}
    </div>
  );
}
