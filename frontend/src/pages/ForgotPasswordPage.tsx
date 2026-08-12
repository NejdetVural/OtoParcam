import { useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { forgotPassword } from "../api/auth";
import { extractErrorMessages } from "../api/errors";
import { Button } from "../components/ui/Button";
import { Input } from "../components/ui/Input";

export function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [errors, setErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDone, setIsDone] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setIsSubmitting(true);
    try {
      await forgotPassword(email);
      setIsDone(true);
    } catch (error) {
      setErrors(extractErrorMessages(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  if (isDone) {
    return (
      <div className="mx-auto max-w-sm rounded-xl border border-slate-200 bg-white shadow-sm p-6 text-center">
        <h1 className="text-lg font-semibold text-slate-900">Talebiniz alındı</h1>
        <p className="mt-2 text-sm text-slate-600">
          Bu e-posta adresine kayıtlı bir hesap varsa, şifre sıfırlama bağlantısı gönderildi. Bu geliştirme
          ortamında bağlantı e-posta ile gönderilmiyor — sunucu konsol loglarından alınmalıdır.
        </p>
        <Link to="/giris" className="mt-4 inline-block text-sm font-medium text-slate-900">
          Giriş sayfasına dön
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-sm flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Şifremi Unuttum</h1>
        <p className="text-sm text-slate-500">
          Hesabınıza kayıtlı e-posta adresini girin, size şifre sıfırlama talimatları gönderelim.
        </p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-6">
        <Input
          label="E-posta"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        {errors.length > 0 && (
          <ul className="flex flex-col gap-1 rounded-lg border border-red-100 bg-red-50 p-3 text-sm text-red-700">
            {errors.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        )}
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Gönderiliyor…" : "Sıfırlama Bağlantısı Gönder"}
        </Button>
      </form>

      <p className="text-center text-sm text-slate-500">
        Şifrenizi hatırladınız mı?{" "}
        <Link to="/giris" className="font-medium text-slate-900">
          Giriş yapın
        </Link>
      </p>
    </div>
  );
}
