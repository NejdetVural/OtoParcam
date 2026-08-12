import { Outlet } from "react-router-dom";
import { Header } from "./Header";
import { Footer } from "./Footer";

export function Layout() {
  return (
    <div className="page-texture flex min-h-screen flex-col">
      <Header />
      <main className="w-full flex-1 px-4 py-8 sm:px-6 lg:px-8 2xl:px-12">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
}
