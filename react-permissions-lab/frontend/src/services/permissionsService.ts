// Base URL for the C# backend API (matches the launchSettings.json http profile)
const API_BASE_URL = "http://localhost:5081";

export interface PermissionsResponse {
  role: string;
  hasAdminAccess: boolean;
}

/**
 * TODO (Exercise 1): Implement this function.
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
  // TODO: Replace this stub with a real API call.
  throw new Error(
    `fetchPermissions is not yet implemented. ` +
      `Expected to call ${API_BASE_URL}/api/permissions?role=${role}`
  );
}
