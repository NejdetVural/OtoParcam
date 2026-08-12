import { describe, expect, it } from "vitest";
import { extractErrorMessages } from "./errors";

function axiosErrorWith(data: unknown) {
  return {
    isAxiosError: true,
    response: { data },
  };
}

describe("extractErrorMessages", () => {
  it("translates known Identity error messages to Turkish", () => {
    const error = axiosErrorWith({ errors: ["Passwords must have at least one uppercase ('A'-'Z')."] });
    expect(extractErrorMessages(error)).toEqual(["Şifre en az bir büyük harf içermelidir."]);
  });

  it("passes through unrecognized messages unchanged", () => {
    const error = axiosErrorWith({ errors: ["Some unmapped backend error."] });
    expect(extractErrorMessages(error)).toEqual(["Some unmapped backend error."]);
  });

  it("deduplicates repeated messages", () => {
    const error = axiosErrorWith({ errors: ["not confirmed", "not confirmed"] });
    expect(extractErrorMessages(error)).toHaveLength(1);
  });

  it("reads a single 'error' string field (Conflict-style responses)", () => {
    const error = axiosErrorWith({ error: "last remaining administrator" });
    expect(extractErrorMessages(error)).toEqual(["Son yönetici hesabının yetkisi kaldırılamaz."]);
  });

  it("falls back to a generic message for non-axios errors", () => {
    expect(extractErrorMessages(new Error("network down"))).toEqual([
      "Beklenmeyen bir hata oluştu. Lütfen tekrar deneyin.",
    ]);
  });
});
