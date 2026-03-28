// Base URL for the C# backend API (matches the launchSettings.json http profile)
const API_BASE_URL = "http://localhost:5081";

export interface PermissionsResponse {
  role: string;
  hasAdminAccess: boolean;
}

/**
 * (Exercise 1): Implement this function.
 *
 * Goal: Call the backend permissions API and return the result.
 *
 * API endpoint:  GET ${API_BASE_URL}/api/permissions?role=<role>
 *
 * Acceptance criteria:
 *  - Makes an HTTP GET request to the endpoint above.
 *  - Returns a PermissionsResponse parsed from the JSON body.
 *  - Throws a descriptive Error when the request fails.
 */
export async function fetchPermissions(
  role: "user" | "admin"
): Promise<PermissionsResponse> {
  const response = await fetch(
    `${API_BASE_URL}/api/permissions?role=${encodeURIComponent(role)}`
  );

  if (!response.ok) {
    throw new Error(
      `Failed to fetch permissions for role '${role}': ${response.status} ${response.statusText}`
    );
  }

  return (await response.json()) as PermissionsResponse;
}
