import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import { getCategories } from "../api/categories";
import { getProducts } from "../api/products";
import { getVehicleBrands } from "../api/vehicleBrands";
import { getVehicleModels } from "../api/vehicleModels";
import { productColorLabels, ProductColor, ProductStatus } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { Roles } from "../auth/roles";
import { ProductCard } from "../components/product/ProductCard";
import { Spinner } from "../components/ui/Spinner";
import { Button } from "../components/ui/Button";
import { Switch } from "../components/ui/Switch";

const colorSwatchHex: Record<ProductColor, string> = {
  [ProductColor.Black]: "#1c1917",
  [ProductColor.White]: "#f8fafc",
  [ProductColor.Gray]: "#6b7280",
  [ProductColor.Silver]: "#cbd5e1",
  [ProductColor.Red]: "#dc2626",
  [ProductColor.Blue]: "#2563eb",
  [ProductColor.Green]: "#16a34a",
  [ProductColor.Yellow]: "#eab308",
  [ProductColor.Orange]: "#ea580c",
  [ProductColor.Brown]: "#78350f",
  [ProductColor.Beige]: "#e7d7b1",
  [ProductColor.Gold]: "#d4af37",
  [ProductColor.Bronze]: "#8c6239",
  [ProductColor.Purple]: "#7e22ce",
  [ProductColor.Pink]: "#ec4899",
  [ProductColor.Other]: "#94a3b8",
};

export function HomePage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const { user } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Administrator) ?? false;

  const categoryId = searchParams.get("categoryId") ?? undefined;
  const vehicleBrandId = searchParams.get("vehicleBrandId") ?? undefined;
  const vehicleModelId = searchParams.get("vehicleModelId") ?? undefined;
  const keyword = searchParams.get("keyword") ?? undefined;
  const color = searchParams.get("color") ? Number(searchParams.get("color")) : undefined;
  const sortBy = (searchParams.get("sortBy") as "priceAsc" | "priceDesc" | null) ?? undefined;
  const page = Number(searchParams.get("page") ?? "1");
  const onlyAvailable = isAdmin && searchParams.get("onlyAvailable") === "1";

  const [searchInput, setSearchInput] = useState(keyword ?? "");
  const [filtersOpen, setFiltersOpen] = useState(false);
  const activeFilterCount = [categoryId, vehicleBrandId, vehicleModelId, color].filter(Boolean).length;

  useEffect(() => {
    setSearchInput(keyword ?? "");
  }, [keyword]);

  useEffect(() => {
    const timeout = setTimeout(() => {
      if (searchInput !== (keyword ?? "")) {
        updateParam("keyword", searchInput);
      }
    }, 350);
    return () => clearTimeout(timeout);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchInput]);

  const categoriesQuery = useQuery({ queryKey: ["categories"], queryFn: getCategories });
  const vehicleBrandsQuery = useQuery({ queryKey: ["vehicle-brands"], queryFn: getVehicleBrands });
  const vehicleModelsQuery = useQuery({ queryKey: ["vehicle-models"], queryFn: getVehicleModels });
  const status = onlyAvailable ? ProductStatus.Available : undefined;
  const productsQuery = useQuery({
    queryKey: ["products", { categoryId, vehicleBrandId, vehicleModelId, keyword, color, status, sortBy, page }],
    queryFn: () => getProducts({ categoryId, vehicleBrandId, vehicleModelId, keyword, color, status, sortBy, page }),
  });

  const modelsForBrand = (vehicleModelsQuery.data ?? []).filter((m) => m.vehicleBrandId === vehicleBrandId);

  function updateParam(key: string, value: string) {
    const next = new URLSearchParams(searchParams);
    if (value) {
      next.set(key, value);
    } else {
      next.delete(key);
    }
    next.delete("page");
    setSearchParams(next);
  }

  function handleBrandChange(value: string) {
    const next = new URLSearchParams(searchParams);
    if (value) {
      next.set("vehicleBrandId", value);
    } else {
      next.delete("vehicleBrandId");
    }
    next.delete("vehicleModelId");
    next.delete("page");
    setSearchParams(next);
  }

  function goToPage(nextPage: number) {
    const next = new URLSearchParams(searchParams);
    next.set("page", String(nextPage));
    setSearchParams(next);
  }

  return (
    <div className="flex flex-col gap-5 md:flex-row md:items-start">
      <button
        type="button"
        onClick={() => setFiltersOpen((open) => !open)}
        aria-expanded={filtersOpen}
        className="flex items-center justify-between rounded-xl border border-slate-300 bg-white px-4 py-3 text-sm font-medium text-slate-900 shadow-sm md:hidden"
      >
        <span className="flex items-center gap-2">
          Filtreler
          {activeFilterCount > 0 && (
            <span className="flex h-5 w-5 items-center justify-center rounded-full bg-brand-800 text-xs text-white">
              {activeFilterCount}
            </span>
          )}
        </span>
        <svg
          viewBox="0 0 20 20"
          className={`h-4 w-4 text-slate-500 transition-transform ${filtersOpen ? "rotate-180" : ""}`}
          fill="none"
          stroke="currentColor"
          strokeWidth={2}
        >
          <path strokeLinecap="round" strokeLinejoin="round" d="M5 7.5l5 5 5-5" />
        </svg>
      </button>

      <aside
        className={`${filtersOpen ? "flex" : "hidden"} flex-col gap-0 overflow-hidden rounded-xl border border-slate-300 bg-white shadow-sm md:flex md:w-60 md:shrink-0`}
      >
        <div>
          <h3 className="bg-brand-800 px-3.5 py-2.5 text-xs font-bold uppercase tracking-wide text-white">Kategoriler</h3>
          <div className="p-3.5">
            <select
              value={categoryId ?? ""}
              onChange={(e) => updateParam("categoryId", e.target.value)}
              className="w-full rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              <option value="">Tüm kategoriler</option>
              {categoriesQuery.data?.map((category) => (
                <option key={category.id} value={category.id}>
                  {category.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="border-t border-slate-100 p-3.5">
          <span className="mb-2 block text-xs font-bold text-slate-600">Marka</span>
          <select
            value={vehicleBrandId ?? ""}
            onChange={(e) => handleBrandChange(e.target.value)}
            className="w-full rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
          >
            <option value="">Tüm markalar</option>
            {vehicleBrandsQuery.data?.map((brand) => (
              <option key={brand.id} value={brand.id}>
                {brand.name}
              </option>
            ))}
          </select>

          {vehicleBrandId && (
            <select
              value={vehicleModelId ?? ""}
              onChange={(e) => updateParam("vehicleModelId", e.target.value)}
              className="mt-2 w-full rounded-lg border border-slate-200 bg-white px-2.5 py-1.5 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              <option value="">Tüm modeller</option>
              {modelsForBrand.map((model) => (
                <option key={model.id} value={model.id}>
                  {model.name}
                  {model.variant ? ` (${model.variant})` : ""} — {model.startYear}-{model.endYear}
                </option>
              ))}
            </select>
          )}
        </div>

        <div className="border-t border-slate-100 p-3.5">
          <span className="mb-2 block text-xs font-bold text-slate-600">Renk</span>
          <div className="flex flex-wrap gap-2">
            <button
              type="button"
              onClick={() => updateParam("color", "")}
              aria-label="Tümü"
              title="Tümü"
              className={`flex h-6 w-6 items-center justify-center rounded-full border-2 text-slate-400 ${
                !color ? "border-brand-700 text-brand-700" : "border-slate-300"
              }`}
            >
              <svg viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="none" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" d="M5 5l14 14" />
              </svg>
            </button>
            {Object.entries(productColorLabels).map(([value, label]) => {
              const numericValue = Number(value) as ProductColor;
              return (
                <button
                  key={value}
                  type="button"
                  onClick={() => updateParam("color", value)}
                  aria-label={label}
                  title={label}
                  style={{ backgroundColor: colorSwatchHex[numericValue] }}
                  className={`h-6 w-6 rounded-full border-2 ${
                    color === numericValue ? "border-brand-700" : "border-slate-300"
                  }`}
                />
              );
            })}
          </div>
        </div>
      </aside>

      <div className="flex flex-1 flex-col gap-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h1 className="text-2xl font-semibold text-slate-900">Ürünler</h1>
            <p className="text-sm text-slate-500">Aracınıza uygun orijinal çıkma yedek parçaları bulun.</p>
          </div>
          <div className="flex items-center gap-3">
            {isAdmin && (
              <Switch
                checked={onlyAvailable}
                onChange={(checked) => updateParam("onlyAvailable", checked ? "1" : "")}
                label="Sadece Satıştakiler"
              />
            )}
            <input
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              placeholder="Anahtar kelime"
              className="w-40 rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400 sm:w-56"
            />
            <select
              value={sortBy ?? ""}
              onChange={(e) => updateParam("sortBy", e.target.value)}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            >
              <option value="">Varsayılan</option>
              <option value="priceAsc">Fiyat: Düşükten Yükseğe</option>
              <option value="priceDesc">Fiyat: Yüksekten Düşüğe</option>
            </select>
          </div>
        </div>

        {productsQuery.isLoading && (
          <div className="flex justify-center py-16">
            <Spinner />
          </div>
        )}

        {productsQuery.isError && (
          <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
            Ürünler yüklenirken bir hata oluştu.
          </p>
        )}

        {productsQuery.data && (
          <>
            {productsQuery.data.items.length === 0 ? (
              <p className="py-16 text-center text-sm text-slate-500">Bu kriterlere uygun ürün bulunamadı.</p>
            ) : (
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-[repeat(auto-fit,minmax(220px,1fr))]">
                {productsQuery.data.items.map((product) => (
                  <ProductCard key={product.id} product={product} />
                ))}
              </div>
            )}

            {productsQuery.data.totalPages > 1 && (
              <div className="flex items-center justify-center gap-3 pt-2">
                <Button variant="secondary" disabled={page <= 1} onClick={() => goToPage(page - 1)}>
                  Önceki
                </Button>
                <span className="text-sm text-slate-500">
                  Sayfa {productsQuery.data.page} / {productsQuery.data.totalPages}
                </span>
                <Button
                  variant="secondary"
                  disabled={page >= productsQuery.data.totalPages}
                  onClick={() => goToPage(page + 1)}
                >
                  Sonraki
                </Button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
