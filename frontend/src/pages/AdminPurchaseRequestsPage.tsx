import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getAllPurchaseRequests, updateNegotiation } from "../api/adminPurchaseRequests";
import { purchaseRequestStatusLabels, PurchaseRequestStatus, type PurchaseRequestDto } from "../api/purchaseRequests";
import { extractErrorMessages } from "../api/errors";
import { Badge } from "../components/ui/Badge";
import { Button } from "../components/ui/Button";
import { Spinner } from "../components/ui/Spinner";

function formatPrice(price: number | null): string {
  if (price === null) {
    return "Fiyat Belirtilmemiş";
  }
  return price.toLocaleString("tr-TR", { style: "currency", currency: "TRY", maximumFractionDigits: 0 });
}

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString("tr-TR", { year: "numeric", month: "long", day: "numeric" });
}

const statusTone: Record<PurchaseRequestStatus, "neutral" | "success" | "warning"> = {
  [PurchaseRequestStatus.Pending]: "neutral",
  [PurchaseRequestStatus.WaitingForCustomerConfirmation]: "warning",
  [PurchaseRequestStatus.Approved]: "success",
  [PurchaseRequestStatus.Rejected]: "neutral",
  [PurchaseRequestStatus.Cancelled]: "neutral",
};

function RequestCard({ request }: { request: PurchaseRequestDto }) {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string | null>(null);
  const [prices, setPrices] = useState<Record<string, string>>(() =>
    Object.fromEntries(
      request.items.map((item) => [item.productId, String(item.negotiatedPrice ?? item.originalPrice ?? 0)]),
    ),
  );

  const canNegotiate =
    request.status === PurchaseRequestStatus.Pending ||
    request.status === PurchaseRequestStatus.WaitingForCustomerConfirmation;

  const negotiationMutation = useMutation({
    mutationFn: () =>
      updateNegotiation(
        request.id,
        request.items.map((item) => ({
          productId: item.productId,
          negotiatedPrice: Number(prices[item.productId]),
        })),
      ),
    onSuccess: () => {
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["admin-purchase-requests"] });
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const hasInvalidPrice = request.items.some((item) => {
    const value = Number(prices[item.productId]);
    return Number.isNaN(value) || value < 0;
  });

  return (
    <div className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <p className="text-sm font-medium text-slate-900">{request.customerName}</p>
          <p className="text-xs text-slate-500">{request.customerEmail}</p>
          <p className="mt-1 text-xs text-slate-400">
            Talep No: {request.id.slice(0, 8)} — {formatDate(request.createdAt)}
          </p>
        </div>
        <Badge tone={statusTone[request.status]}>{purchaseRequestStatusLabels[request.status]}</Badge>
      </div>

      <ul className="flex flex-col gap-3 border-t border-slate-100 pt-3">
        {request.items.map((item) => (
          <li key={item.productId} className="flex flex-wrap items-center justify-between gap-3 text-sm">
            <Link to={`/urunler/${item.productId}`} className="text-slate-800 hover:underline">
              {item.productTitle}
            </Link>
            <div className="flex items-center gap-2">
              <span className="text-xs text-slate-400">Liste: {formatPrice(item.originalPrice)}</span>
              {canNegotiate ? (
                <input
                  type="number"
                  min={0}
                  step="0.01"
                  value={prices[item.productId]}
                  onChange={(e) => setPrices((p) => ({ ...p, [item.productId]: e.target.value }))}
                  className="w-28 rounded-lg border border-slate-200 bg-white px-2 py-1 text-right text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
                />
              ) : (
                <span className="font-medium text-slate-900">{formatPrice(item.negotiatedPrice)}</span>
              )}
            </div>
          </li>
        ))}
      </ul>

      {error && <p className="text-sm text-red-600">{error}</p>}

      {canNegotiate && (
        <div className="flex items-center justify-between gap-3 border-t border-slate-100 pt-3">
          <p className="text-xs text-slate-500">
            {request.status === PurchaseRequestStatus.WaitingForCustomerConfirmation
              ? "Müşterinin onayı bekleniyor. Teklifi güncelleyebilirsiniz."
              : "Fiyatları belirleyip teklif gönderin."}
          </p>
          <Button disabled={negotiationMutation.isPending || hasInvalidPrice} onClick={() => negotiationMutation.mutate()}>
            {negotiationMutation.isPending ? "Gönderiliyor…" : "Teklifi Gönder"}
          </Button>
        </div>
      )}
    </div>
  );
}

export function AdminPurchaseRequestsPage() {
  const requestsQuery = useQuery({ queryKey: ["admin-purchase-requests"], queryFn: getAllPurchaseRequests });

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Talep Yönetimi</h1>
        <p className="text-sm text-slate-500">Tüm müşteri satın alma taleplerini görüntüleyin ve fiyat teklifi gönderin.</p>
      </div>

      {requestsQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {requestsQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          Talepler yüklenirken bir hata oluştu.
        </p>
      )}

      {requestsQuery.data &&
        (requestsQuery.data.length === 0 ? (
          <div className="rounded-xl border border-dashed border-slate-300 bg-white p-12 text-center">
            <p className="text-sm text-slate-500">Henüz bir satın alma talebi yok.</p>
          </div>
        ) : (
          <div className="flex flex-col gap-4">
            {[...requestsQuery.data]
              .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
              .map((request) => (
                <RequestCard key={request.id} request={request} />
              ))}
          </div>
        ))}
    </div>
  );
}
