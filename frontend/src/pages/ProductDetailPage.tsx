import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate, useParams } from "react-router-dom";
import { getProductById, getProductCompatibility, hideProduct, restoreProduct } from "../api/products";
import { createPurchaseRequest } from "../api/purchaseRequests";
import { extractErrorMessages } from "../api/errors";
import { productColorLabels, productPositionLabels, productSideLabels, productStatusLabels, ProductStatus } from "../api/types";
import { useAuth } from "../auth/AuthContext";
import { Roles } from "../auth/roles";
import { resolveImageUrl } from "../lib/images";
import { Spinner } from "../components/ui/Spinner";
import { Badge } from "../components/ui/Badge";
import { Button } from "../components/ui/Button";
import { DropdownMenu } from "../components/ui/DropdownMenu";
import { FavoriteButton } from "../components/product/FavoriteButton";

function formatPrice(price: number | null): string {
  if (price === null) {
    return "Fiyat İçin Arayın";
  }
  return price.toLocaleString("tr-TR", { style: "currency", currency: "TRY", maximumFractionDigits: 0 });
}

export function ProductDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [activeImage, setActiveImage] = useState(0);
  const [requestError, setRequestError] = useState<string | null>(null);
  const { isAuthenticated, user } = useAuth();
  const isAdmin = user?.roles.includes(Roles.Administrator) ?? false;
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const purchaseRequestMutation = useMutation({
    mutationFn: () => createPurchaseRequest([id!]),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["purchase-requests"] });
      navigate("/taleplerim");
    },
    onError: (error) => setRequestError(extractErrorMessages(error)[0]),
  });

  const hideMutation = useMutation({
    mutationFn: () => hideProduct(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product", id] });
      queryClient.invalidateQueries({ queryKey: ["admin-products"] });
    },
  });

  const restoreMutation = useMutation({
    mutationFn: () => restoreProduct(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["product", id] });
      queryClient.invalidateQueries({ queryKey: ["admin-products"] });
    },
  });

  const productQuery = useQuery({
    queryKey: ["product", id],
    queryFn: () => getProductById(id!),
    enabled: Boolean(id),
  });

  const compatibilityQuery = useQuery({
    queryKey: ["product-compatibility", id],
    queryFn: () => getProductCompatibility(id!),
    enabled: Boolean(id),
  });

  if (productQuery.isLoading) {
    return (
      <div className="flex justify-center py-16">
        <Spinner />
      </div>
    );
  }

  if (productQuery.isError || !productQuery.data) {
    return <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">Ürün bulunamadı.</p>;
  }

  const product = productQuery.data;
  const images = product.images;
  const cover = images[activeImage]?.imageUrl;

  return (
    <div className="flex flex-col gap-6">
      <Link to="/" className="text-sm text-slate-500 hover:text-slate-900">
        ← Ürünlere dön
      </Link>

      <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
        <div className="flex flex-col gap-3">
          <div className="aspect-square w-full overflow-hidden rounded-xl border border-slate-200 bg-slate-100">
            {cover ? (
              <img src={resolveImageUrl(cover)} alt={product.title} className="h-full w-full object-cover" />
            ) : (
              <div className="flex h-full w-full items-center justify-center text-sm text-slate-400">Görsel yok</div>
            )}
          </div>
          {images.length > 1 && (
            <div className="flex gap-2">
              {images.map((image, index) => (
                <button
                  key={image.id}
                  onClick={() => setActiveImage(index)}
                  className={`h-16 w-16 overflow-hidden rounded-lg border ${
                    index === activeImage ? "border-slate-900" : "border-slate-200"
                  }`}
                >
                  <img src={resolveImageUrl(image.imageUrl)} alt="" className="h-full w-full object-cover" />
                </button>
              ))}
            </div>
          )}
        </div>

        <div className="flex flex-col gap-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <span className="text-xs font-medium text-slate-500">{product.categoryName}</span>
              <h1 className="text-2xl font-semibold text-slate-900">{product.title}</h1>
            </div>
            {isAdmin ? (
              <DropdownMenu
                items={[
                  { label: "Düzenle", onClick: () => navigate(`/admin/urunler/${product.id}/duzenle`) },
                  ...(product.status === ProductStatus.Hidden
                    ? [
                        {
                          label: "Görünür Yap",
                          onClick: () => restoreMutation.mutate(),
                          disabled: restoreMutation.isPending,
                        },
                      ]
                    : product.status === ProductStatus.Available
                      ? [
                          {
                            label: "Gizle",
                            onClick: () => hideMutation.mutate(),
                            disabled: hideMutation.isPending,
                            destructive: true,
                            confirm: {
                              message: "Bu ürünü gizlemek istediğinize emin misiniz? Müşteriler artık göremeyecek.",
                              confirmLabel: "Evet, Gizle",
                            },
                          },
                        ]
                      : []),
                ]}
              />
            ) : (
              <FavoriteButton productId={product.id} />
            )}
          </div>

          <span className="text-2xl font-semibold text-slate-900">{formatPrice(product.price)}</span>

          <div className="flex flex-wrap gap-2">
            <Badge>{productColorLabels[product.color]}</Badge>
            {product.side !== null && <Badge>{productSideLabels[product.side]}</Badge>}
            {product.position !== null && <Badge>{productPositionLabels[product.position]}</Badge>}
            {product.status !== ProductStatus.Available && <Badge tone="warning">{productStatusLabels[product.status]}</Badge>}
          </div>

          {product.description && <p className="text-sm leading-relaxed text-slate-600">{product.description}</p>}

          <div className="flex flex-col gap-2">
            {isAdmin ? null : product.status === ProductStatus.Available ? (
              isAuthenticated ? (
                <Button
                  disabled={purchaseRequestMutation.isPending}
                  onClick={() => {
                    setRequestError(null);
                    purchaseRequestMutation.mutate();
                  }}
                >
                  {purchaseRequestMutation.isPending ? "Gönderiliyor…" : "Satın Alma Talebi Oluştur"}
                </Button>
              ) : (
                <Link
                  to="/giris"
                  className="inline-flex items-center justify-center rounded-lg bg-brand-800 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-brand-700"
                >
                  Talep oluşturmak için giriş yapın
                </Link>
              )
            ) : (
              <p className="text-sm text-slate-500">
                Bu ürün için satın alma talebi oluşturulamaz ({productStatusLabels[product.status]}).
              </p>
            )}
            {requestError && <p className="text-sm text-red-600">{requestError}</p>}
          </div>

          <div className="rounded-xl border border-slate-200 bg-white shadow-sm p-4">
            <h2 className="text-sm font-medium text-slate-900">Uyumlu Araçlar</h2>
            <p className="mt-1 text-xs text-slate-500">
              Bu parça, aşağıda listelenen araç modelleriyle uyumludur. Uyumluluk bilgisi ayrıca listelenmiştir.
            </p>
            {compatibilityQuery.data && compatibilityQuery.data.length > 0 ? (
              <ul className="mt-3 flex flex-col gap-1.5 text-sm text-slate-700">
                {compatibilityQuery.data.map((vehicle) => (
                  <li key={vehicle.vehicleModelId}>
                    {vehicle.vehicleBrandName} {vehicle.vehicleModelName}
                    {vehicle.variant ? ` (${vehicle.variant})` : ""} — {vehicle.startYear}-{vehicle.endYear}
                  </li>
                ))}
              </ul>
            ) : (
              <p className="mt-3 text-sm text-slate-500">Uyumluluk bilgisi girilmemiş.</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
