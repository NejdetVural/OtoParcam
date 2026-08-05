import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getFavorites } from "../api/favorites";
import { ProductCard } from "../components/product/ProductCard";
import { Spinner } from "../components/ui/Spinner";

export function FavoritesPage() {
  const favoritesQuery = useQuery({ queryKey: ["favorites"], queryFn: getFavorites });

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Favorilerim</h1>
        <p className="text-sm text-slate-500">Favorilerinize eklediğiniz ürünler.</p>
      </div>

      {favoritesQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {favoritesQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          Favoriler yüklenirken bir hata oluştu.
        </p>
      )}

      {favoritesQuery.data &&
        (favoritesQuery.data.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-300 bg-white p-12 text-center">
            <p className="text-sm text-slate-500">Henüz favori ürününüz yok.</p>
            <Link to="/" className="mt-3 inline-block text-sm font-medium text-slate-900 hover:underline">
              Ürünlere göz atın →
            </Link>
          </div>
        ) : (
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-[repeat(auto-fill,minmax(220px,1fr))]">
            {favoritesQuery.data.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        ))}
    </div>
  );
}
