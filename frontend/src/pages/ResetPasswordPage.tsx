import { useState, type FormEvent } from "react";
import { Link, useSearchParams } from "react-router-dom";
import { resetPassword } from "../api/auth";
import { extractErrorMessages } from "../api/errors";
import { Button } from "../components/ui/Button";
import { Input } from "../components/ui/Input";

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const userId = searchParams.get("userId");
  const token = searchParams.get("token");

  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [errors, setErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isDone, setIsDone] = useState(false);

  if (!userId || !token) {
    return (
      <div className="mx-auto max-w-sm rounded-xl border border-slate-200 bg-white shadow-sm p-6 text-center">
        <h1 className="text-lg font-semibold text-slate-900">Geçersiz bağlantı</h1>
        <p className="mt-2 text-sm text-slate-600">
          Bu şifre sıfırlama bağlantısı eksik veya hatalı. Lütfen yeni bir sıfırlama talebi oluşturun.
        </p>
        <Link to="/sifremi-unuttum" className="mt-4 inline-block text-sm font-medium text-slate-900">
          Şifremi unuttum sayfasına dön
        </Link>
      </div>
    );
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    if (newPassword !== confirmPassword) {
      setErrors(["Şifreler eşleşmiyor."]);
      return;
    }
    setIsSubmitting(true);
    try {
      await resetPassword({ userId: userId!, token: token!, newPassword });
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
        <h1 className="text-lg font-semibold text-slate-900">Şifreniz güncellendi</h1>
        <p className="mt-2 text-sm text-slate-600">Yeni şifrenizle giriş yapabilirsiniz.</p>
        <Link to="/giris" className="mt-4 inline-block text-sm font-medium text-slate-900">
          Giriş sayfasına dön
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-sm flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Yeni Şifre Belirle</h1>
        <p className="text-sm text-slate-500">Hesabınız için yeni bir şifre girin.</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-6">
        <Input
          label="Yeni Şifre"
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          minLength={6}
          required
        />
        <p className="-mt-3 text-xs text-slate-500">
          En az 6 karakter, bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir.
        </p>
        <Input
          label="Yeni Şifre (Tekrar)"
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          minLength={6}
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
          {isSubmitting ? "Kaydediliyor…" : "Şifreyi Güncelle"}
        </Button>
      </form>
    </div>
  );
}
