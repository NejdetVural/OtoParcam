import { describe, expect, it } from "vitest";
import { formatPhoneNumber, isEmailLike, normalizePhoneDigits } from "./phone";

describe("normalizePhoneDigits", () => {
  it("strips non-digit characters", () => {
    expect(normalizePhoneDigits("555 123 45 67")).toBe("5551234567");
  });

  it("strips a leading 90 country code", () => {
    expect(normalizePhoneDigits("905551234567")).toBe("5551234567");
  });

  it("strips a leading 0 trunk prefix", () => {
    expect(normalizePhoneDigits("05551234567")).toBe("5551234567");
  });

  it("truncates to 10 digits", () => {
    expect(normalizePhoneDigits("555123456789")).toBe("5551234567");
  });
});

describe("formatPhoneNumber", () => {
  it("formats a full number as +90 5XX XXX XXXX", () => {
    expect(formatPhoneNumber("5551234567")).toBe("+90 555 123 4567");
  });

  it("formats a partial number without padding missing groups", () => {
    expect(formatPhoneNumber("555")).toBe("+90 555");
  });

  it("returns an empty string for no digits", () => {
    expect(formatPhoneNumber("")).toBe("");
  });
});

describe("isEmailLike", () => {
  it("returns true when the value contains @", () => {
    expect(isEmailLike("ornek@eposta.com")).toBe(true);
  });

  it("returns false for a phone-shaped value", () => {
    expect(isEmailLike("5551234567")).toBe(false);
  });
});
