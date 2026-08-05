import { useEffect, useState } from "react";
import { Link, NavLink, useLocation } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { Roles } from "../../auth/roles";
import { Button } from "../ui/Button";

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `text-sm font-medium transition-colors ${isActive ? "text-white" : "text-slate-400 hover:text-white"}`;

const mobileNavLinkClass = ({ isActive }: { isActive: boolean }) =>
  `block rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
    isActive ? "bg-brand-800 text-white" : "text-slate-300 hover:bg-brand-800/60 hover:text-white"
  }`;

interface NavItem {
  to: string;
  label: string;
  end?: boolean;
}

function getNavItems(isAuthenticated: boolean, isAdmin: boolean): NavItem[] {
  const items: NavItem[] = [{ to: "/", label: "Ürünler", end: true }];

  if (isAuthenticated && !isAdmin) {
    items.push({ to: "/favoriler", label: "Favoriler" }, { to: "/taleplerim", label: "Taleplerim" });
  }

  if (isAdmin) {
    items.push(
      { to: "/admin", label: "Yönetim", end: true },
      { to: "/admin/urunler", label: "Ürünler" },
      { to: "/admin/talepler", label: "Talep Yönetimi" },
    );
  }

  if (isAuthenticated) {
    items.push({ to: "/profilim", label: "Profilim" });
  }

  return items;
}

export function Header() {
  const { isAuthenticated, user, logout } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Administrator) ?? false;
  const [menuOpen, setMenuOpen] = useState(false);
  const location = useLocation();

  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname]);

  const navItems = getNavItems(isAuthenticated, isAdmin);

  return (
    <header className="border-b border-brand-800 bg-brand-950">
      <div className="flex h-16 w-full items-center justify-between px-4 sm:px-6 lg:px-8 2xl:px-12">
        <Link to="/" className="text-lg font-semibold tracking-tight text-white">
          OtoParcam
        </Link>

        <nav className="hidden items-center gap-6 sm:flex">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.end} className={navLinkClass}>
              {item.label}
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-3">
          {isAuthenticated ? (
            <Button variant="secondary" onClick={logout}>
              Çıkış Yap
            </Button>
          ) : (
            <>
              <Link to="/giris" className="text-sm font-medium text-slate-300 hover:text-white">
                Giriş Yap
              </Link>
              <Link
                to="/kayit"
                className="inline-flex items-center justify-center rounded-lg bg-white px-4 py-2 text-sm font-medium text-brand-900 transition-colors hover:bg-slate-100"
              >
                Kayıt Ol
              </Link>
            </>
          )}

          <button
            type="button"
            onClick={() => setMenuOpen((open) => !open)}
            aria-label={menuOpen ? "Menüyü kapat" : "Menüyü aç"}
            aria-expanded={menuOpen}
            className="inline-flex h-9 w-9 items-center justify-center rounded-lg border border-brand-700 text-slate-300 hover:text-white sm:hidden"
          >
            {menuOpen ? (
              <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth={1.8}>
                <path strokeLinecap="round" d="M6 6l12 12M18 6L6 18" />
              </svg>
            ) : (
              <svg viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth={1.8}>
                <path strokeLinecap="round" d="M4 7h16M4 12h16M4 17h16" />
              </svg>
            )}
          </button>
        </div>
      </div>

      {menuOpen && (
        <nav className="border-t border-brand-800 px-4 py-3 sm:hidden">
          <div className="flex flex-col gap-1">
            {navItems.map((item) => (
              <NavLink key={item.to} to={item.to} end={item.end} className={mobileNavLinkClass}>
                {item.label}
              </NavLink>
            ))}
          </div>
        </nav>
      )}
    </header>
  );
}
