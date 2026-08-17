import { beforeEach, describe, expect, it } from "vitest";
import { clearSession, getToken, setToken } from "./session";

describe("session", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it("returns null when no token is stored", () => {
    expect(getToken()).toBeNull();
  });

  it("stores and retrieves a token", () => {
    setToken("abc.def.ghi");
    expect(getToken()).toBe("abc.def.ghi");
  });

  it("clears the stored token", () => {
    setToken("abc.def.ghi");
    clearSession();
    expect(getToken()).toBeNull();
  });

  it("overwrites a previously stored token", () => {
    setToken("first.token.value");
    setToken("second.token.value");
    expect(getToken()).toBe("second.token.value");
  });
});
