import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { resendConfirmationEmail } from "../api/auth";
import { extractErrorMessages } from "../api/errors";
import { formatPhoneNumber, normalizePhoneDigits } from "../lib/phone";
import { useCountdown } from "../lib/useCountdown";
import { Button } from "../components/ui/Button";
import { Input } from "../components/ui/Input";

const RESEND_COOLDOWN_SECONDS = 60;

export function RegisterPage() {
  const { register } = useAuth();
  const [form, setForm] = useState({
    firstName: "",
    lastName: "",
    email: "",
    phoneDigits: "",
    password: "",
  });
  const [privacyPolicyAccepted, setPrivacyPolicyAccepted] = useState(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDone, setIsDone] = useState(false);
  const { remaining: resendCooldown, start: startResendCooldown } = useCountdown();
  const [isResending, setIsResending] = useState(false);
  const [resendMessage, setResendMessage] = useState<string | null>(null);

  function updateField(field: "firstName" | "lastName" | "email" | "password", value: string) {
    setForm((prev) => ({ ...prev, [field]: value }));
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setIsSubmitting(true);
    try {
      await register({
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        phoneNumber: formatPhoneNumber(form.phoneDigits),
        password: form.password,
        privacyPolicyAccepted,
      });
      setIsDone(true);
      startResendCooldown(RESEND_COOLDOWN_SECONDS);
    } catch (error) {
      setErrors(extractErrorMessages(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function handleResend() {
    if (resendCooldown > 0 || isResending) {
      return;
    }
    setIsResending(true);
    setResendMessage(null);
    try {
      await resendConfirmationEmail(form.email);
      setResendMessage("Onay bağlantısı yeniden gönderildi.");
      startResendCooldown(RESEND_COOLDOWN_SECONDS);
    } catch (error) {
      setResendMessage(extractErrorMessages(error)[0] ?? "Gönderilemedi, tekrar deneyin.");
    } finally {
      setIsResending(false);
    }
  }

  if (isDone) {
    return (
      <div className="mx-auto max-w-sm rounded-xl border border-slate-200 bg-white shadow-sm p-6 text-center">
        <h1 className="text-lg font-semibold text-slate-900">Kaydınız oluşturuldu</h1>
        <p className="mt-2 text-sm text-slate-600">
          Hesabınız oluşturuldu ancak giriş yapabilmeniz için e-posta onayı gerekiyor. Size gönderdiğimiz
          onay bağlantısına tıklayarak hesabınızı aktifleştirebilirsiniz.
        </p>
        <p className="mt-2 text-xs text-slate-500">
          E-postayı bulamıyor musunuz? Spam/gereksiz klasörünü kontrol edin.
        </p>
        <Button
          variant="secondary"
          className="mt-4 w-full"
          disabled={resendCooldown > 0 || isResending}
          onClick={handleResend}
        >
          {isResending
            ? "Gönderiliyor…"
            : resendCooldown > 0
              ? `Tekrar Gönder (${resendCooldown}s)`
              : "Tekrar Gönder"}
        </Button>
        {resendMessage && <p className="mt-2 text-xs text-slate-500">{resendMessage}</p>}
        <Link to="/giris" className="mt-4 inline-block text-sm font-medium text-slate-900">
          Giriş sayfasına dön
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-sm flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Kayıt Ol</h1>
        <p className="text-sm text-slate-500">Yeni bir müşteri hesabı oluşturun.</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-6">
        <Input label="Ad" value={form.firstName} onChange={(e) => updateField("firstName", e.target.value)} required />
        <Input label="Soyad" value={form.lastName} onChange={(e) => updateField("lastName", e.target.value)} required />
        <Input
          label="E-posta"
          type="email"
          value={form.email}
          onChange={(e) => updateField("email", e.target.value)}
          required
        />
        <Input
          label="Telefon"
          type="tel"
          placeholder="5XX XXX XXXX"
          value={formatPhoneNumber(form.phoneDigits)}
          onChange={(e) => setForm((prev) => ({ ...prev, phoneDigits: normalizePhoneDigits(e.target.value) }))}
          required
        />
        <p className="-mt-3 text-xs text-slate-500">Örn: +90 555 123 4567 — başında 0 olmadan girebilirsiniz.</p>
        <Input
          label="Şifre"
          type="password"
          value={form.password}
          onChange={(e) => updateField("password", e.target.value)}
          minLength={6}
          required
        />
        <p className="-mt-3 text-xs text-slate-500">
          En az 6 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.
        </p>
        <label className="flex items-start gap-2 text-sm text-slate-600">
          <input
            type="checkbox"
            checked={privacyPolicyAccepted}
            onChange={(e) => setPrivacyPolicyAccepted(e.target.checked)}
            required
            className="mt-0.5 h-4 w-4 rounded border-slate-300 text-slate-900 focus:ring-slate-400"
          />
          <span>
            <Link to="/gizlilik-politikasi" target="_blank" className="font-medium text-slate-900 underline">
              Gizlilik Politikasını
            </Link>{" "}
            okudum ve kabul ediyorum.
          </span>
        </label>
        {errors.length > 0 && (
          <ul className="flex flex-col gap-1 rounded-lg border border-red-100 bg-red-50 p-3 text-sm text-red-700">
            {errors.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        )}
        <Button type="submit" disabled={isSubmitting || !privacyPolicyAccepted}>
          {isSubmitting ? "Kayıt oluşturuluyor…" : "Kayıt Ol"}
        </Button>
      </form>

      <p className="text-center text-sm text-slate-500">
        Zaten hesabınız var mı?{" "}
        <Link to="/giris" className="font-medium text-slate-900">
          Giriş yapın
        </Link>
      </p>
    </div>
  );
}
