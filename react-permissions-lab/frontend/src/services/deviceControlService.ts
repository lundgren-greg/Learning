const API_BASE_URL = "http://localhost:5081";

export interface DevicePowerResponse {
  success: boolean;
  receivedCommand: string;
  deviceCommand: string;
}

export async function sendPowerOnCommand(): Promise<DevicePowerResponse> {
  const response = await fetch(`${API_BASE_URL}/api/device/power`, {
	method: "POST",
	headers: {
	  "Content-Type": "application/json",
	},
	body: JSON.stringify({ pwr: 1 }),
  });

  if (!response.ok) {
	const errorBody = await response.text();
	throw new Error(
	  `Device power command failed: ${response.status} ${response.statusText} ${errorBody}`
	);
  }

  return (await response.json()) as DevicePowerResponse;
}

// TODO(power-off): Add sendPowerOffCommand() that POSTs { pwr: 0 } to /api/device/power.


