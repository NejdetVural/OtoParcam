import { isAxiosError } from "axios";

const TRANSLATION_RULES: [RegExp, string][] = [
  [/already taken/i, "Bu e-posta adresi veya telefon numarası zaten kayıtlı."],
  [/non alphanumeric/i, "Şifre en az bir özel karakter içermelidir."],
  [/at least one digit/i, "Şifre en az bir rakam içermelidir."],
  [/lowercase/i, "Şifre en az bir küçük harf içermelidir."],
  [/uppercase/i, "Şifre en az bir büyük harf içermelidir."],
  [/at least 6 characters/i, "Şifre en az 6 karakter olmalıdır."],
  [/invalid credentials/i, "E-posta/telefon veya şifre hatalı."],
  [/not confirmed/i, "E-posta adresiniz henüz onaylanmadı."],
];

function translate(message: string): string {
  const rule = TRANSLATION_RULES.find(([pattern]) => pattern.test(message));
  return rule ? rule[1] : message;
}

export function extractErrorMessages(error: unknown): string[] {
  if (isAxiosError(error)) {
    const data = error.response?.data as { errors?: unknown; error?: unknown } | undefined;
    if (Array.isArray(data?.errors)) {
      return [...new Set(data.errors.map((message) => translate(String(message))))];
    }
    if (typeof data?.error === "string") {
      return [translate(data.error)];
    }
  }
  return ["Beklenmeyen bir hata oluştu. Lütfen tekrar deneyin."];
}
