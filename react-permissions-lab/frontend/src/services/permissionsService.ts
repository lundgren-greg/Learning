// Base URL for the C# backend API (matches the launchSettings.json http profile)
const API_BASE_URL = "http://localhost:5081";

export interface PermissionsResponse {
  role: string;
  hasAdminAccess: boolean;
}

/**
 * TODO: Call the backend API to retrieve the permissions for the given role.
 *
 * Steps to implement:
 *  1. Use the `fetch` API (or a library like axios) to make a GET request to:
 *       `${API_BASE_URL}/api/permissions?role=<role>`
 *  2. Parse the JSON response body into a `PermissionsResponse` object.
 *  3. Return the parsed object.
 *  4. Handle network/HTTP errors appropriately (e.g., throw an Error with a
 *     descriptive message so the caller can display feedback to the user).
 *
 * Example fetch call to get you started:
 *   const response = await fetch(`${API_BASE_URL}/api/permissions?role=${role}`);
 *   if (!response.ok) throw new Error(`API error: ${response.status}`);
 *   return response.json() as Promise<PermissionsResponse>;
 */
export async function fetchPermissions(
  role: "user" | "admin"
): Promise<PermissionsResponse> {
  // TODO: Replace this stub with a real API call (see instructions above).
  throw new Error(
    `fetchPermissions is not yet implemented. ` +
      `Expected to call ${API_BASE_URL}/api/permissions?role=${role}`
  );
}
