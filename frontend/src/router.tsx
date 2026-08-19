import { createBrowserRouter } from "react-router-dom";
import { Layout } from "./components/layout/Layout";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { Roles } from "./auth/roles";
import { HomePage } from "./pages/HomePage";
import { ProductDetailPage } from "./pages/ProductDetailPage";
import { LoginPage } from "./pages/LoginPage";
import { RegisterPage } from "./pages/RegisterPage";
import { ForgotPasswordPage } from "./pages/ForgotPasswordPage";
import { ResetPasswordPage } from "./pages/ResetPasswordPage";
import { ConfirmEmailPage } from "./pages/ConfirmEmailPage";
import { ProfilePage } from "./pages/ProfilePage";
import { FavoritesPage } from "./pages/FavoritesPage";
import { PurchaseRequestsPage } from "./pages/PurchaseRequestsPage";
import { AdminDashboardPage } from "./pages/AdminDashboardPage";
import { AdminPurchaseRequestsPage } from "./pages/AdminPurchaseRequestsPage";
import { AdminProductsPage } from "./pages/AdminProductsPage";
import { AdminProductFormPage } from "./pages/AdminProductFormPage";
import { AdminCategoriesPage } from "./pages/AdminCategoriesPage";
import { AdminVehicleBrandsPage } from "./pages/AdminVehicleBrandsPage";
import { AdminVehicleModelsPage } from "./pages/AdminVehicleModelsPage";
import { AdminAcquisitionBatchesPage } from "./pages/AdminAcquisitionBatchesPage";
import { AdminUsersPage } from "./pages/AdminUsersPage";
import { PrivacyPolicyPage } from "./pages/PrivacyPolicyPage";
import { NotFoundPage } from "./pages/NotFoundPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Layout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: "urunler/:id", element: <ProductDetailPage /> },
      { path: "giris", element: <LoginPage /> },
      { path: "kayit", element: <RegisterPage /> },
      { path: "sifremi-unuttum", element: <ForgotPasswordPage /> },
      { path: "sifre-sifirla", element: <ResetPasswordPage /> },
      { path: "onay", element: <ConfirmEmailPage /> },
      { path: "gizlilik-politikasi", element: <PrivacyPolicyPage /> },
      {
        element: <ProtectedRoute requiredRole={Roles.Customer} />,
        children: [
          { path: "favoriler", element: <FavoritesPage /> },
          { path: "taleplerim", element: <PurchaseRequestsPage /> },
        ],
      },
      {
        element: <ProtectedRoute />,
        children: [{ path: "profilim", element: <ProfilePage /> }],
      },
      {
        element: <ProtectedRoute requiredRole={Roles.Administrator} />,
        children: [
          { path: "admin", element: <AdminDashboardPage /> },
          { path: "admin/talepler", element: <AdminPurchaseRequestsPage /> },
          { path: "admin/urunler", element: <AdminProductsPage /> },
          { path: "admin/urunler/yeni", element: <AdminProductFormPage /> },
          { path: "admin/urunler/:id/duzenle", element: <AdminProductFormPage /> },
          { path: "admin/kategoriler", element: <AdminCategoriesPage /> },
          { path: "admin/markalar", element: <AdminVehicleBrandsPage /> },
          { path: "admin/modeller", element: <AdminVehicleModelsPage /> },
          { path: "admin/toplu-alimlar", element: <AdminAcquisitionBatchesPage /> },
          { path: "admin/musteriler", element: <AdminUsersPage /> },
        ],
      },
      { path: "*", element: <NotFoundPage /> },
    ],
  },
]);
