import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Link, NavLink, useLocation } from "react-router-dom";
import { useAuth } from "../../auth/AuthContext";
import { Roles } from "../../auth/roles";
import { getAllPurchaseRequests } from "../../api/adminPurchaseRequests";
import { getPurchaseRequests, PurchaseRequestStatus } from "../../api/purchaseRequests";
import { NotificationBubble } from "../ui/NotificationBubble";

const navLinkClass = ({ isActive }: { isActive: boolean }) =>
  `rounded-lg px-3 py-1.5 text-sm font-medium transition-colors ${
    isActive ? "bg-white/10 text-white" : "text-slate-400 hover:bg-white/5 hover:text-white"
  }`;

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
      { to: "/admin/urunler", label: "Ürün Yönetimi" },
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

  const customerRequestsQuery = useQuery({
    queryKey: ["purchase-requests"],
    queryFn: getPurchaseRequests,
    enabled: isAuthenticated && !isAdmin,
  });

  const adminRequestsQuery = useQuery({
    queryKey: ["admin-purchase-requests"],
    queryFn: getAllPurchaseRequests,
    enabled: isAdmin,
  });

  const badgeCounts: Record<string, number> = {
    "/taleplerim":
      customerRequestsQuery.data?.filter((r) => r.status === PurchaseRequestStatus.WaitingForCustomerConfirmation)
        .length ?? 0,
    "/admin/talepler":
      adminRequestsQuery.data?.filter((r) => r.status === PurchaseRequestStatus.Pending).length ?? 0,
  };

  return (
    <header className="sticky top-0 z-40 border-b border-white/[0.06] bg-brand-950/90 backdrop-blur-md">
      <div className="flex h-16 w-full items-center justify-between px-4 sm:px-6 lg:px-8 2xl:px-12">
        <Link to="/" className="flex items-center gap-2.5 text-lg font-semibold tracking-tight text-white">
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-white/10">
            <svg viewBox="0 0 24 24" className="h-4 w-4 text-brand-200" fill="none" stroke="currentColor" strokeWidth={1.8}>
              <circle cx="12" cy="12" r="7.5" />
              <circle cx="12" cy="12" r="2" fill="currentColor" stroke="none" />
              <path
                strokeLinecap="round"
                d="M12 3.5v3M12 17.5v3M3.5 12h3M17.5 12h3M6.3 6.3l2.1 2.1M15.6 15.6l2.1 2.1M6.3 17.7l2.1-2.1M15.6 8.4l2.1-2.1"
              />
            </svg>
          </span>
          OtoParcam
        </Link>

        <nav className="hidden items-center gap-3 sm:flex">
          {navItems.map((item) => (
            <NavLink key={item.to} to={item.to} end={item.end} className={navLinkClass}>
              <span className="inline-flex items-center gap-1.5">
                {item.label}
                <NotificationBubble count={badgeCounts[item.to] ?? 0} />
              </span>
            </NavLink>
          ))}
        </nav>

        <div className="flex items-center gap-3">
          {isAuthenticated ? (
            <button
              type="button"
              onClick={logout}
              className="inline-flex items-center justify-center rounded-lg border border-white/15 px-4 py-2 text-sm font-medium text-slate-200 transition-colors hover:border-white/30 hover:text-white"
            >
              Çıkış Yap
            </button>
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
                <span className="flex items-center justify-between">
                  {item.label}
                  <NotificationBubble count={badgeCounts[item.to] ?? 0} />
                </span>
              </NavLink>
            ))}
          </div>
        </nav>
      )}
    </header>
  );
}
