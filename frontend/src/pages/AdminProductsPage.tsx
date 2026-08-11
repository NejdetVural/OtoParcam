import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link, useNavigate } from "react-router-dom";
import { getProducts, hideProduct, markProductSold, restoreProduct } from "../api/products";
import { extractErrorMessages } from "../api/errors";
import { productColorLabels, productPositionLabels, productSideLabels, productStatusLabels, ProductStatus } from "../api/types";
import type { ProductDto } from "../api/products";
import { Button } from "../components/ui/Button";
import { Badge } from "../components/ui/Badge";
import { Input } from "../components/ui/Input";
import { Modal } from "../components/ui/Modal";
import { Spinner } from "../components/ui/Spinner";
import { DropdownMenu } from "../components/ui/DropdownMenu";

function formatPrice(price: number | null): string {
  if (price === null) {
    return "Fiyat Belirtilmemiş";
  }
  return price.toLocaleString("tr-TR", { style: "currency", currency: "TRY", maximumFractionDigits: 0 });
}

function formatPartType(product: ProductDto): string | null {
  const parts = [
    product.side !== null ? productSideLabels[product.side] : null,
    product.position !== null ? productPositionLabels[product.position] : null,
  ].filter((part): part is string => part !== null);
  return parts.length > 0 ? parts.join(" ") : null;
}

function formatAcquisitionCost(product: ProductDto): string | null {
  if (product.acquisitionCost !== null) {
    return `Alış: ${formatPrice(product.acquisitionCost)}`;
  }
  if (product.effectiveAcquisitionCost !== null) {
    return `Alış: ~${formatPrice(product.effectiveAcquisitionCost)} (toplu alım)`;
  }
  return null;
}

const statusTone: Record<ProductStatus, "neutral" | "success" | "warning"> = {
  [ProductStatus.Available]: "success",
  [ProductStatus.Hidden]: "neutral",
  [ProductStatus.Sold]: "warning",
};

const statusFilterOptions: { label: string; value: ProductStatus | "all" }[] = [
  { label: "Tümü", value: "all" },
  { label: "Satışta", value: ProductStatus.Available },
  { label: "Gizli", value: ProductStatus.Hidden },
  { label: "Satıldı", value: ProductStatus.Sold },
];

export function AdminProductsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<ProductStatus | "all">("all");
  const [actionError, setActionError] = useState<string | null>(null);
  const [sellingProduct, setSellingProduct] = useState<ProductDto | null>(null);
  const [soldPriceInput, setSoldPriceInput] = useState("");
  const [sellError, setSellError] = useState<string | null>(null);

  const productsQuery = useQuery({
    queryKey: ["admin-products", page, statusFilter],
    queryFn: () => getProducts({ page, status: statusFilter === "all" ? undefined : statusFilter }),
  });

  const hideMutation = useMutation({
    mutationFn: (id: string) => hideProduct(id),
    onSuccess: () => {
      setActionError(null);
      queryClient.invalidateQueries({ queryKey: ["admin-products"] });
    },
    onError: () => setActionError("Ürün gizlenirken bir hata oluştu."),
  });

  const restoreMutation = useMutation({
    mutationFn: (id: string) => restoreProduct(id),
    onSuccess: () => {
      setActionError(null);
      queryClient.invalidateQueries({ queryKey: ["admin-products"] });
    },
    onError: (err) => setActionError(extractErrorMessages(err)[0]),
  });

  const sellMutation = useMutation({
    mutationFn: (soldPrice: number) => markProductSold(sellingProduct!.id, soldPrice),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["admin-products"] });
      setSellingProduct(null);
      setSellError(null);
    },
    onError: (err) => setSellError(extractErrorMessages(err)[0]),
  });

  function openSellModal(product: ProductDto) {
    setSellingProduct(product);
    setSoldPriceInput(product.price !== null ? String(product.price) : "");
    setSellError(null);
  }

  function handleConfirmSell() {
    const parsed = Number(soldPriceInput);
    if (!soldPriceInput.trim() || Number.isNaN(parsed) || parsed < 0) {
      setSellError("Geçerli bir satış fiyatı girin.");
      return;
    }
    sellMutation.mutate(parsed);
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">Ürün Yönetimi</h1>
          <p className="text-sm text-slate-500">Satıştaki, gizlenen ve satılan tüm parçaları yönetin.</p>
        </div>
        <Link to="/admin/urunler/yeni">
          <Button>Yeni Ürün Ekle</Button>
        </Link>
      </div>

      <div className="flex items-center gap-2">
        {statusFilterOptions.map((option) => (
          <button
            key={option.label}
            type="button"
            onClick={() => {
              setPage(1);
              setStatusFilter(option.value);
            }}
            className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
              statusFilter === option.value
                ? "bg-slate-900 text-white"
                : "bg-slate-100 text-slate-600 hover:bg-slate-200"
            }`}
          >
            {option.label}
          </button>
        ))}
      </div>

      {actionError && <p className="text-sm text-red-600">{actionError}</p>}

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
              <p className="text-sm text-slate-500">Bu filtreye uyan ürün yok.</p>
            </div>
          ) : (
            <div className="flex flex-col gap-2">
              {productsQuery.data.items.map((product) => (
                <div
                  key={product.id}
                  className="flex flex-wrap items-center justify-between gap-3 rounded-xl border border-slate-200 bg-white shadow-sm p-4"
                >
                  <Link to={`/admin/urunler/${product.id}/duzenle`} className="group">
                    <p className="text-sm font-medium text-slate-900 group-hover:underline">{product.title}</p>
                    <p className="text-xs text-slate-500">
                      {product.categoryName}
                      {formatPartType(product) && ` · ${formatPartType(product)}`} · {productColorLabels[product.color]} ·{" "}
                      {formatPrice(product.price)}
                    </p>
                    {formatAcquisitionCost(product) && (
                      <p className="text-xs text-slate-400">{formatAcquisitionCost(product)}</p>
                    )}
                    {product.status === ProductStatus.Sold && (
                      <p className="text-xs font-medium text-emerald-700">
                        Satış Fiyatı: {formatPrice(product.soldPrice)}
                      </p>
                    )}
                  </Link>
                  <div className="flex items-center gap-2">
                    <Badge tone={statusTone[product.status]}>{productStatusLabels[product.status]}</Badge>
                    <DropdownMenu
                      items={[
                        { label: "Düzenle", onClick: () => navigate(`/admin/urunler/${product.id}/duzenle`) },
                        ...(product.status !== ProductStatus.Sold
                          ? [{ label: "Satıldı Olarak İşaretle", onClick: () => openSellModal(product) }]
                          : []),
                        ...(product.status === ProductStatus.Hidden
                          ? [
                              {
                                label: "Görünür Yap",
                                onClick: () => restoreMutation.mutate(product.id),
                                disabled: restoreMutation.isPending,
                              },
                            ]
                          : product.status === ProductStatus.Available
                            ? [
                                {
                                  label: "Gizle",
                                  onClick: () => hideMutation.mutate(product.id),
                                  disabled: hideMutation.isPending,
                                  destructive: true,
                                  confirm: {
                                    message:
                                      "Bu ürünü gizlemek istediğinize emin misiniz? Müşteriler artık göremeyecek.",
                                    confirmLabel: "Evet, Gizle",
                                  },
                                },
                              ]
                            : []),
                      ]}
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

      {sellingProduct && (
        <Modal title="Satış Fiyatını Girin" onClose={() => setSellingProduct(null)}>
          <p className="text-xs text-slate-500">
            {sellingProduct.title} — sitedeki talep akışı dışında (elden/telefonla) yapılan satışlar için. Ürün Satıldı
            olarak işaretlenir ve girilen fiyat envanter/rapor hesaplarına yansır.
          </p>
          <Input
            label="Satış Fiyatı"
            type="number"
            min={0}
            step="0.01"
            autoFocus
            value={soldPriceInput}
            onChange={(e) => setSoldPriceInput(e.target.value)}
          />
          {sellError && <p className="text-sm text-red-600">{sellError}</p>}
          <div className="flex justify-end gap-2">
            <Button variant="secondary" onClick={() => setSellingProduct(null)}>
              Vazgeç
            </Button>
            <Button onClick={handleConfirmSell} disabled={sellMutation.isPending}>
              {sellMutation.isPending ? "Kaydediliyor…" : "Satıldı Olarak İşaretle"}
            </Button>
          </div>
        </Modal>
      )}
    </div>
  );
}
