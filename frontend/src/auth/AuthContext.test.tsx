import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { AuthProvider, useAuth } from "./AuthContext";
import { clearSession, getToken } from "./session";

const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

function base64UrlEncode(value: string): string {
  const base64 = btoa(unescape(encodeURIComponent(value)));
  return base64.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

function makeToken(payload: Record<string, unknown>): string {
  const header = base64UrlEncode(JSON.stringify({ alg: "HS256", typ: "JWT" }));
  const body = base64UrlEncode(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

const validToken = makeToken({
  sub: "1",
  email: "admin@example.com",
  exp: Math.floor(Date.now() / 1000) + 3600,
  [ROLE_CLAIM]: "Administrator",
});

vi.mock("../api/auth", () => ({
  login: vi.fn(),
  register: vi.fn(),
}));

import * as authApi from "../api/auth";

function TestConsumer() {
  const { user, isAuthenticated, login, logout } = useAuth();
  return (
    <div>
      <span data-testid="authed">{String(isAuthenticated)}</span>
      <span data-testid="email">{user?.email ?? ""}</span>
      <button onClick={() => login({ emailOrPhone: "admin@example.com", password: "x" })}>login</button>
      <button onClick={logout}>logout</button>
    </div>
  );
}

describe("AuthProvider", () => {
  beforeEach(() => {
    clearSession();
    vi.clearAllMocks();
  });

  it("starts unauthenticated when there is no stored token", () => {
    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );
    expect(screen.getByTestId("authed")).toHaveTextContent("false");
  });

  it("logs in, stores the token, and exposes the decoded user", async () => {
    vi.mocked(authApi.login).mockResolvedValue({ token: validToken, expiresAtUtc: "2099-01-01T00:00:00Z" });
    const user = userEvent.setup();

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await user.click(screen.getByText("login"));

    await waitFor(() => expect(screen.getByTestId("authed")).toHaveTextContent("true"));
    expect(screen.getByTestId("email")).toHaveTextContent("admin@example.com");
    expect(getToken()).toBe(validToken);
  });

  it("logs out and clears the stored token", async () => {
    vi.mocked(authApi.login).mockResolvedValue({ token: validToken, expiresAtUtc: "2099-01-01T00:00:00Z" });
    const user = userEvent.setup();

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    await user.click(screen.getByText("login"));
    await waitFor(() => expect(screen.getByTestId("authed")).toHaveTextContent("true"));

    await user.click(screen.getByText("logout"));

    expect(screen.getByTestId("authed")).toHaveTextContent("false");
    expect(getToken()).toBeNull();
  });

  it("ignores an expired token already in storage", () => {
    const expiredToken = makeToken({
      sub: "1",
      email: "admin@example.com",
      exp: Math.floor(Date.now() / 1000) - 60,
    });
    localStorage.setItem("otoparcam_token", expiredToken);

    render(
      <AuthProvider>
        <TestConsumer />
      </AuthProvider>,
    );

    expect(screen.getByTestId("authed")).toHaveTextContent("false");
    expect(getToken()).toBeNull();
  });
});
