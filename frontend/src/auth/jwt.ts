import { jwtDecode } from "jwt-decode";

// Claim keys as written by AuthService.GenerateJwt (backend/src/OtoParcam.Infrastructure/Services/AuthService.cs) —
// raw System.Security.Claims.ClaimTypes URIs, since the token is built without any inbound claim-type mapping.
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const GIVEN_NAME_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/givenname";
const SURNAME_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/surname";

interface RawTokenPayload {
  sub: string;
  email: string;
  [GIVEN_NAME_CLAIM]?: string;
  [SURNAME_CLAIM]?: string;
  [ROLE_CLAIM]?: string | string[];
}

export interface CurrentUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: string[];
}

export function decodeUser(token: string): CurrentUser {
  const payload = jwtDecode<RawTokenPayload>(token);
  const roles = payload[ROLE_CLAIM] ?? [];

  return {
    id: payload.sub,
    email: payload.email,
    firstName: payload[GIVEN_NAME_CLAIM] ?? "",
    lastName: payload[SURNAME_CLAIM] ?? "",
    roles: Array.isArray(roles) ? roles : [roles],
  };
}

export function isTokenExpired(token: string): boolean {
  const { exp } = jwtDecode<{ exp: number }>(token);
  return Date.now() >= exp * 1000;
}
