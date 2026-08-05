import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import * as authApi from "../api/auth";
import { decodeUser, isTokenExpired, type CurrentUser } from "./jwt";
import { clearSession, getToken, setToken } from "./session";

interface AuthContextValue {
  user: CurrentUser | null;
  isAuthenticated: boolean;
  login: (request: authApi.LoginRequest) => Promise<void>;
  register: (request: authApi.RegisterRequest) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

function loadUserFromStorage(): CurrentUser | null {
  const token = getToken();
  if (!token || isTokenExpired(token)) {
    clearSession();
    return null;
  }
  return decodeUser(token);
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(loadUserFromStorage);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isAuthenticated: user !== null,
      async login(request) {
        const { token } = await authApi.login(request);
        setToken(token);
        setUser(decodeUser(token));
      },
      async register(request) {
        await authApi.register(request);
      },
      logout() {
        clearSession();
        setUser(null);
      },
    }),
    [user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
}
