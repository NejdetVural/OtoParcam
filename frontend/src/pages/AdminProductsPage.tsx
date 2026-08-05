import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getProducts, hideProduct } from "../api/products";
import { productColorLabels, productStatusLabels } from "../api/types";
import { Button } from "../components/ui/Button";
import { ConfirmButton } from "../components/ui/ConfirmButton";
import { Badge } from "../components/ui/Badge";
import { Spinner } from "../components/ui/Spinner";

function formatPrice(price: number | null): string {
  if (price === null) {
    return "Fiyat Belirtilmemiş";
  }
  return price.toLocaleString("tr-TR", { style: "currency", currency: "TRY", maximumFractionDigits: 0 });
}

export function AdminProductsPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [hideError, setHideError] = useState<string | null>(null);

  const productsQuery = useQuery({
    queryKey: ["admin-products", page],
    queryFn: () => getProducts({ page }),
  });

  const hideMutation = useMutation({
    mutationFn: (id: string) => hideProduct(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["admin-products"] }),
    onError: () => setHideError("Ürün gizlenirken bir hata oluştu."),
  });

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">Ürünler</h1>
          <p className="text-sm text-slate-500">
            Satıştaki parçaları yönetin. Gizlenen veya satılan ürünler bu listede görünmez.
          </p>
        </div>
        <Link to="/admin/urunler/yeni">
          <Button>Yeni Ürün Ekle</Button>
        </Link>
      </div>

      {hideError && <p className="text-sm text-red-600">{hideError}</p>}

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
            <div className="rounded-xl border border-dashed border-slate-300 bg-white p-12 text-center">
              <p className="text-sm text-slate-500">Henüz satıştaki ürün yok.</p>
            </div>
          ) : (
            <div className="flex flex-col gap-2">
              {productsQuery.data.items.map((product) => (
                <div
                  key={product.id}
                  className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-slate-200 bg-white shadow-sm p-4"
                >
                  <div>
                    <p className="text-sm font-medium text-slate-900">{product.title}</p>
                    <p className="text-xs text-slate-500">
                      {product.categoryName} · {productColorLabels[product.color]} · {formatPrice(product.price)}
                    </p>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge>{productStatusLabels[product.status]}</Badge>
                    <Link to={`/admin/urunler/${product.id}/duzenle`}>
                      <Button variant="secondary">Düzenle</Button>
                    </Link>
                    <ConfirmButton
                      label="Gizle"
                      confirmLabel="Evet, Gizle"
                      message="Bu ürünü gizlemek istediğinize emin misiniz? Müşteriler artık göremeyecek."
                      disabled={hideMutation.isPending}
                      onConfirm={() => {
                        setHideError(null);
                        hideMutation.mutate(product.id);
                      }}
                    />
                  </div>
                </div>
              ))}
            </div>
          )}

          {productsQuery.data.totalPages > 1 && (
            <div className="flex items-center justify-center gap-3 pt-2">
              <Button variant="secondary" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                Önceki
              </Button>
              <span className="text-sm text-slate-500">
                Sayfa {productsQuery.data.page} / {productsQuery.data.totalPages}
              </span>
              <Button
                variant="secondary"
                disabled={page >= productsQuery.data.totalPages}
                onClick={() => setPage((p) => p + 1)}
              >
                Sonraki
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}
