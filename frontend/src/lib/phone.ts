// Turkish mobile numbers: 10 digits (5XX XXX XXXX) after the country code.
export function normalizePhoneDigits(raw: string): string {
  let digits = raw.replace(/\D/g, "");
  if (digits.startsWith("90")) {
    digits = digits.slice(2);
  } else if (digits.startsWith("0")) {
    digits = digits.slice(1);
  }
  return digits.slice(0, 10);
}

export function formatPhoneNumber(raw: string): string {
  const digits = normalizePhoneDigits(raw);
  const parts = [digits.slice(0, 3), digits.slice(3, 6), digits.slice(6, 10)].filter(Boolean);
  return parts.length ? `+90 ${parts.join(" ")}` : "";
}

export function isEmailLike(value: string): boolean {
  return value.includes("@");
}
