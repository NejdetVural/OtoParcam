import { useState, type FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { extractErrorMessages } from "../api/errors";
import { formatPhoneNumber, isEmailLike } from "../lib/phone";
import { Button } from "../components/ui/Button";
import { Input } from "../components/ui/Input";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [emailOrPhone, setEmailOrPhone] = useState("");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setErrors([]);
    setIsSubmitting(true);
    try {
      const identifier = isEmailLike(emailOrPhone) ? emailOrPhone : formatPhoneNumber(emailOrPhone);
      await login({ emailOrPhone: identifier, password });
      navigate("/");
    } catch (error) {
      setErrors(extractErrorMessages(error));
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="mx-auto flex max-w-sm flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Giriş Yap</h1>
        <p className="text-sm text-slate-500">Hesabınıza erişmek için giriş yapın.</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-6">
        <Input
          label="E-posta veya Telefon"
          placeholder="ornek@eposta.com veya 5XX XXX XXXX"
          value={emailOrPhone}
          onChange={(e) => setEmailOrPhone(e.target.value)}
          required
        />
        <Input
          label="Şifre"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
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
          {isSubmitting ? "Giriş yapılıyor…" : "Giriş Yap"}
        </Button>
      </form>

      <p className="text-center text-sm text-slate-500">
        Hesabınız yok mu?{" "}
        <Link to="/kayit" className="font-medium text-slate-900">
          Kayıt olun
        </Link>
      </p>
    </div>
  );
}
