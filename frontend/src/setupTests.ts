import "@testing-library/jest-dom/vitest";
import { afterEach, vi } from "vitest";
import { cleanup } from "@testing-library/react";

// jsdom 30 defers localStorage/sessionStorage to Node's own (experimental, opt-in via
// --localstorage-file) Web Storage implementation rather than providing its own — without that
// flag, window.localStorage is present but its value is undefined. Rather than depend on an
// experimental Node flag, stub in a minimal in-memory Storage so app code using localStorage
// (e.g. src/auth/session.ts) works the same under test as it does in a real browser.
class MemoryStorage implements Storage {
  private store = new Map<string, string>();

  get length(): number {
    return this.store.size;
  }

  clear(): void {
    this.store.clear();
  }

  getItem(key: string): string | null {
    return this.store.has(key) ? this.store.get(key)! : null;
  }

  key(index: number): string | null {
    return Array.from(this.store.keys())[index] ?? null;
  }

  removeItem(key: string): void {
    this.store.delete(key);
  }

  setItem(key: string, value: string): void {
    this.store.set(key, String(value));
  }
}

vi.stubGlobal("localStorage", new MemoryStorage());
vi.stubGlobal("sessionStorage", new MemoryStorage());

afterEach(() => {
  cleanup();
});
