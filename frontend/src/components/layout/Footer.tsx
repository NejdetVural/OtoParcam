import { Link } from "react-router-dom";

export function Footer() {
  return (
    <footer className="border-t border-slate-200 bg-white">
      <div className="flex flex-col items-center justify-between gap-2 px-4 py-5 text-xs text-slate-500 sm:flex-row sm:px-6 lg:px-8 2xl:px-12">
        <span>© {new Date().getFullYear()} OtoParcam</span>
        <Link to="/gizlilik-politikasi" className="font-medium text-slate-600 hover:text-slate-900">
          Gizlilik Politikası
        </Link>
      </div>
    </footer>
  );
}
