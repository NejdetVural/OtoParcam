import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useSearchParams } from "react-router-dom";
import { getCategories } from "../api/categories";
import { getProducts } from "../api/products";
import { getVehicleBrands } from "../api/vehicleBrands";
import { getVehicleModels } from "../api/vehicleModels";
import { productColorLabels, ProductColor } from "../api/types";
import { ProductCard } from "../components/product/ProductCard";
import { Spinner } from "../components/ui/Spinner";
import { Button } from "../components/ui/Button";

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

function SidebarLink({ active, onClick, children }: { active: boolean; onClick: () => void; children: string }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`block w-full rounded-lg px-2.5 py-1.5 text-left text-sm transition-colors ${
        active ? "bg-brand-800 font-semibold text-white" : "text-slate-700 hover:bg-slate-100"
      }`}
    >
      {children}
    </button>
  );
}

export function HomePage() {
  const [searchParams, setSearchParams] = useSearchParams();

  const categoryId = searchParams.get("categoryId") ?? undefined;
  const vehicleBrandId = searchParams.get("vehicleBrandId") ?? undefined;
  const vehicleModelId = searchParams.get("vehicleModelId") ?? undefined;
  const keyword = searchParams.get("keyword") ?? undefined;
  const color = searchParams.get("color") ? Number(searchParams.get("color")) : undefined;
  const sortBy = (searchParams.get("sortBy") as "priceAsc" | "priceDesc" | null) ?? undefined;
  const page = Number(searchParams.get("page") ?? "1");

  const [searchInput, setSearchInput] = useState(keyword ?? "");

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
  const productsQuery = useQuery({
    queryKey: ["products", { categoryId, vehicleBrandId, vehicleModelId, keyword, color, sortBy, page }],
    queryFn: () => getProducts({ categoryId, vehicleBrandId, vehicleModelId, keyword, color, sortBy, page }),
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
      <aside className="flex flex-col gap-0 overflow-hidden rounded-xl border border-slate-300 bg-white shadow-sm md:w-60 md:shrink-0">
        <div>
          <h3 className="bg-brand-800 px-3.5 py-2.5 text-xs font-bold uppercase tracking-wide text-white">Kategoriler</h3>
          <div className="flex flex-col gap-0.5 p-1.5">
            <SidebarLink active={!categoryId} onClick={() => updateParam("categoryId", "")}>
              Tümü
            </SidebarLink>
            {categoriesQuery.data?.map((category) => (
              <SidebarLink key={category.id} active={categoryId === category.id} onClick={() => updateParam("categoryId", category.id)}>
                {category.name}
              </SidebarLink>
            ))}
          </div>
        </div>

        <div className="border-t border-slate-100 p-3.5">
          <span className="mb-2 block text-xs font-bold text-slate-600">Marka</span>
          <div className="flex flex-col gap-0.5">
            <SidebarLink active={!vehicleBrandId} onClick={() => handleBrandChange("")}>
              Tümü
            </SidebarLink>
            {vehicleBrandsQuery.data?.map((brand) => (
              <SidebarLink key={brand.id} active={vehicleBrandId === brand.id} onClick={() => handleBrandChange(brand.id)}>
                {brand.name}
              </SidebarLink>
            ))}
          </div>

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
          <div className="flex items-center gap-2">
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
              <div className="grid grid-cols-2 gap-4 sm:grid-cols-[repeat(auto-fill,minmax(220px,1fr))]">
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
