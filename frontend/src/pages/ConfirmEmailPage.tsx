import { useEffect, useRef, useState, type FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { confirmEmail, resendConfirmationEmail } from "../api/auth";
import { extractErrorMessages } from "../api/errors";
import { useCountdown } from "../lib/useCountdown";
import { Button } from "../components/ui/Button";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";

const RESEND_COOLDOWN_SECONDS = 60;

type Status = "loading" | "success" | "error" | "invalid-link";

export function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();
  const userId = searchParams.get("userId");
  const token = searchParams.get("token");

  const [status, setStatus] = useState<Status>(userId && token ? "loading" : "invalid-link");
  const [errors, setErrors] = useState<string[]>([]);
  const hasRun = useRef(false);

  const [resendEmail, setResendEmail] = useState("");
  const { remaining: resendCooldown, start: startResendCooldown } = useCountdown();
  const [isResending, setIsResending] = useState(false);
  const [resendMessage, setResendMessage] = useState<string | null>(null);

  async function handleResend(e: FormEvent) {
    e.preventDefault();
    if (resendCooldown > 0 || isResending) {
      return;
    }
    setIsResending(true);
    setResendMessage(null);
    try {
      await resendConfirmationEmail(resendEmail);
      setResendMessage("Bu e-posta adresine kayıtlı bir hesap varsa, yeni bir onay bağlantısı gönderildi.");
      startResendCooldown(RESEND_COOLDOWN_SECONDS);
    } catch (error) {
      setResendMessage(extractErrorMessages(error)[0] ?? "Gönderilemedi, tekrar deneyin.");
    } finally {
      setIsResending(false);
    }
  }

  useEffect(() => {
    if (!userId || !token || hasRun.current) {
      return;
    }
    hasRun.current = true;

    confirmEmail(userId, token)
      .then(() => setStatus("success"))
      .catch((error) => {
        setErrors(extractErrorMessages(error));
        setStatus("error");
      });
  }, [userId, token]);

  if (status === "invalid-link") {
    return (
      <div className="mx-auto max-w-sm rounded-xl border border-slate-200 bg-white shadow-sm p-6 text-center">
        <h1 className="text-lg font-semibold text-slate-900">Geçersiz bağlantı</h1>
        <p className="mt-2 text-sm text-slate-600">
          Bu e-posta onay bağlantısı eksik veya hatalı. Kayıt sırasında gönderilen e-postadaki bağlantıyı
          kullandığınızdan emin olun.
        </p>
        <Link to="/giris" className="mt-4 inline-block text-sm font-medium text-slate-900">
          Giriş sayfasına dön
        </Link>
      </div>
    );
  }

  if (status === "loading") {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  if (status === "error") {
    return (
      <div className="mx-auto max-w-sm rounded-xl border border-slate-200 bg-white shadow-sm p-6 text-center">
        <h1 className="text-lg font-semibold text-slate-900">Onaylanamadı</h1>
        <p className="mt-2 text-sm text-slate-600">
          E-posta adresiniz onaylanamadı. Bağlantının süresi dolmuş ya da daha önce kullanılmış olabilir.
        </p>
        {errors.length > 0 && (
          <ul className="mt-3 flex flex-col gap-1 rounded-lg border border-red-100 bg-red-50 p-3 text-left text-sm text-red-700">
            {errors.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        )}
        <form onSubmit={handleResend} className="mt-4 flex flex-col gap-2 text-left">
          <Input
            label="E-posta"
            type="email"
            value={resendEmail}
            onChange={(e) => setResendEmail(e.target.value)}
            required
          />
          <Button type="submit" variant="secondary" disabled={resendCooldown > 0 || isResending}>
            {isResending
              ? "Gönderiliyor…"
              : resendCooldown > 0
                ? `Yeni Bağlantı Gönder (${resendCooldown}s)`
                : "Yeni Bağlantı Gönder"}
          </Button>
          {resendMessage && <p className="text-xs text-slate-500">{resendMessage}</p>}
        </form>
        <Link to="/giris" className="mt-4 inline-block text-sm font-medium text-slate-900">
          Giriş sayfasına dön
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-sm rounded-xl border border-slate-200 bg-white shadow-sm p-6 text-center">
      <h1 className="text-lg font-semibold text-slate-900">E-posta adresiniz onaylandı</h1>
      <p className="mt-2 text-sm text-slate-600">Artık hesabınızla giriş yapabilirsiniz.</p>
      <Link to="/giris" className="mt-4 inline-block text-sm font-medium text-slate-900">
        Giriş yap
      </Link>
    </div>
  );
}
