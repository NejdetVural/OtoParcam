import { describe, expect, it } from "vitest";
import { decodeUser, isTokenExpired } from "./jwt";

// Mirrors the raw ClaimTypes URIs AuthService.GenerateJwt embeds in the token (see jwt.ts).
const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const GIVEN_NAME_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/givenname";
const SURNAME_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/surname";

function base64UrlEncode(value: string): string {
  const base64 = btoa(unescape(encodeURIComponent(value)));
  return base64.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function makeToken(payload: Record<string, unknown>): string {
  const header = base64UrlEncode(JSON.stringify({ alg: "HS256", typ: "JWT" }));
  const body = base64UrlEncode(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

describe("decodeUser", () => {
  it("maps the backend's raw claim-type URIs to a CurrentUser", () => {
    const token = makeToken({
      sub: "11111111-1111-1111-1111-111111111111",
      email: "test@example.com",
      [GIVEN_NAME_CLAIM]: "Ada",
      [SURNAME_CLAIM]: "Lovelace",
      [ROLE_CLAIM]: "Administrator",
    });

    expect(decodeUser(token)).toEqual({
      id: "11111111-1111-1111-1111-111111111111",
      email: "test@example.com",
      firstName: "Ada",
      lastName: "Lovelace",
      roles: ["Administrator"],
    });
  });

  it("normalizes a single role string into an array", () => {
    const token = makeToken({ sub: "1", email: "a@b.com", [ROLE_CLAIM]: "Customer" });
    expect(decodeUser(token).roles).toEqual(["Customer"]);
  });

  it("keeps multiple roles as an array", () => {
    const token = makeToken({ sub: "1", email: "a@b.com", [ROLE_CLAIM]: ["Customer", "Administrator"] });
    expect(decodeUser(token).roles).toEqual(["Customer", "Administrator"]);
  });

  it("defaults to an empty roles array and blank name fields when claims are missing", () => {
    const token = makeToken({ sub: "1", email: "a@b.com" });
    const user = decodeUser(token);
    expect(user.roles).toEqual([]);
    expect(user.firstName).toBe("");
    expect(user.lastName).toBe("");
  });
});

describe("isTokenExpired", () => {
  it("returns true for a token whose exp is in the past", () => {
    const token = makeToken({ exp: Math.floor(Date.now() / 1000) - 60 });
    expect(isTokenExpired(token)).toBe(true);
  });

  it("returns false for a token whose exp is in the future", () => {
    const token = makeToken({ exp: Math.floor(Date.now() / 1000) + 3600 });
    expect(isTokenExpired(token)).toBe(false);
  });
});
