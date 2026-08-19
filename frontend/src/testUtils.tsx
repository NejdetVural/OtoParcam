import type { ReactElement, ReactNode } from "react";
import { render } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import { setToken } from "./auth/session";

// Same claim-key shape as AuthService.GenerateJwt (backend) — see auth/jwt.ts. Kept here so any
// page test needing a logged-in user (customer or admin) can build one without duplicating this.
export const ROLE_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
const GIVEN_NAME_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/givenname";
const SURNAME_CLAIM = "http://schemas.microsoft.com/ws/2008/06/identity/claims/surname";

function base64UrlEncode(value: string): string {
  const base64 = btoa(unescape(encodeURIComponent(value)));
  return base64.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/, "");
}

export function makeToken(payload: Record<string, unknown>): string {
  const header = base64UrlEncode(JSON.stringify({ alg: "HS256", typ: "JWT" }));
  const body = base64UrlEncode(JSON.stringify(payload));
  return `${header}.${body}.signature`;
}

export function makeCustomerToken(overrides: Record<string, unknown> = {}): string {
  return makeToken({
    sub: "11111111-1111-1111-1111-111111111111",
    email: "customer@example.com",
    [GIVEN_NAME_CLAIM]: "Test",
    [SURNAME_CLAIM]: "Customer",
    exp: Math.floor(Date.now() / 1000) + 3600,
    [ROLE_CLAIM]: "Customer",
    ...overrides,
  });
}

export function makeAdminToken(overrides: Record<string, unknown> = {}): string {
  return makeToken({
    sub: "22222222-2222-2222-2222-222222222222",
    email: "admin@example.com",
    [GIVEN_NAME_CLAIM]: "Test",
    [SURNAME_CLAIM]: "Admin",
    exp: Math.floor(Date.now() / 1000) + 3600,
    [ROLE_CLAIM]: ["Administrator", "Customer"],
    ...overrides,
  });
}

export function createTestQueryClient(): QueryClient {
  return new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
}

interface RenderPageOptions {
  route?: string;
  token?: string;
}

// For pages that don't assert on navigating to a different route. Tests that need to observe
// "navigated to X after Y" should build their own small <Routes> wiring instead (see
// LoginPage.test.tsx / RegisterPage.test.tsx) since the target route's rendered content varies.
export function renderPage(ui: ReactElement, { route = "/", token }: RenderPageOptions = {}) {
  if (token) {
    setToken(token);
  }

  const queryClient = createTestQueryClient();

  function Wrapper({ children }: { children: ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <MemoryRouter initialEntries={[route]}>{children}</MemoryRouter>
        </AuthProvider>
      </QueryClientProvider>
    );
  }

  return render(ui, { wrapper: Wrapper });
}
