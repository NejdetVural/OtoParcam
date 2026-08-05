import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <div className="flex flex-col items-center gap-3 py-24 text-center">
      <h1 className="text-2xl font-semibold text-slate-900">Sayfa bulunamadı</h1>
      <p className="text-sm text-slate-500">Aradığınız sayfa mevcut değil.</p>
      <Link to="/" className="text-sm font-medium text-slate-900">
        Ana sayfaya dön
      </Link>
    </div>
  );
}
