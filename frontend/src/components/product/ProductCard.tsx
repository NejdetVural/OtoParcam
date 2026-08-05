import { Link } from "react-router-dom";
import type { ProductDto } from "../../api/products";
import { resolveImageUrl } from "../../lib/images";
import { FavoriteButton } from "./FavoriteButton";

function formatPrice(price: number | null): string {
  if (price === null) {
    return "Fiyat İçin Arayın";
  }
  return price.toLocaleString("tr-TR", { style: "currency", currency: "TRY", maximumFractionDigits: 0 });
}

export function ProductCard({ product }: { product: ProductDto }) {
  const cover = product.images[0]?.imageUrl;

  return (
    <Link
      to={`/urunler/${product.id}`}
      className="group flex flex-col overflow-hidden rounded-xl border-2 border-slate-300 bg-white shadow-sm transition-all hover:border-brand-400 hover:shadow-md"
    >
      <div className="relative aspect-[4/3] w-full bg-slate-100">
        {cover ? (
          <img src={resolveImageUrl(cover)} alt={product.title} className="h-full w-full object-cover" />
        ) : (
          <div className="flex h-full w-full items-center justify-center text-sm text-slate-400">Görsel yok</div>
        )}
        <FavoriteButton productId={product.id} className="absolute right-2 top-2 shadow-sm" />
      </div>
      <div className="flex flex-1 flex-col gap-1.5 p-3">
        <span className="inline-flex w-fit items-center rounded-full bg-brand-800 px-2.5 py-0.5 text-xs font-semibold text-white">
          {product.categoryName}
        </span>
        <h3 className="text-sm font-medium text-slate-900 group-hover:text-brand-700">{product.title}</h3>
        <span className="mt-auto pt-2 text-sm font-semibold text-slate-900">{formatPrice(product.price)}</span>
      </div>
    </Link>
  );
}
