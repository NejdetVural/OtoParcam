import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import {
  cancelPurchaseRequest,
  confirmPurchaseRequest,
  getPurchaseRequests,
  purchaseRequestStatusLabels,
  rejectPurchaseRequest,
  PurchaseRequestStatus,
  type PurchaseRequestDto,
} from "../api/purchaseRequests";
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

  function invalidate() {
    queryClient.invalidateQueries({ queryKey: ["purchase-requests"] });
  }

  function handleError(err: unknown) {
    setError(extractErrorMessages(err)[0]);
  }

  const cancelMutation = useMutation({ mutationFn: () => cancelPurchaseRequest(request.id), onSuccess: invalidate, onError: handleError });
  const confirmMutation = useMutation({ mutationFn: () => confirmPurchaseRequest(request.id), onSuccess: invalidate, onError: handleError });
  const rejectMutation = useMutation({ mutationFn: () => rejectPurchaseRequest(request.id), onSuccess: invalidate, onError: handleError });

  const isBusy = cancelMutation.isPending || confirmMutation.isPending || rejectMutation.isPending;
  const total = request.items.reduce((sum, item) => sum + (item.negotiatedPrice ?? item.originalPrice ?? 0), 0);

  return (
    <div className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
      <div className="flex flex-wrap items-center justify-between gap-2">
        <div>
          <p className="text-xs text-slate-500">Talep No: {request.id.slice(0, 8)}</p>
          <p className="text-sm text-slate-500">{formatDate(request.createdAt)}</p>
        </div>
        <Badge tone={statusTone[request.status]}>{purchaseRequestStatusLabels[request.status]}</Badge>
      </div>

      <ul className="flex flex-col gap-2 border-t border-slate-100 pt-3">
        {request.items.map((item) => (
          <li key={item.productId} className="flex items-center justify-between gap-3 text-sm">
            <Link to={`/urunler/${item.productId}`} className="text-slate-800 hover:underline">
              {item.productTitle}
            </Link>
            <span className="text-slate-600">
              {item.negotiatedPrice !== null ? (
                <>
                  <span className="mr-1 text-slate-400 line-through">{formatPrice(item.originalPrice)}</span>
                  <span className="font-medium text-slate-900">{formatPrice(item.negotiatedPrice)}</span>
                </>
              ) : (
                formatPrice(item.originalPrice)
              )}
            </span>
          </li>
        ))}
      </ul>

      <div className="flex items-center justify-between border-t border-slate-100 pt-3">
        <span className="text-sm font-medium text-slate-700">Toplam</span>
        <span className="text-base font-semibold text-slate-900">{formatPrice(total)}</span>
      </div>

      {error && <p className="text-sm text-red-600">{error}</p>}

      {request.status === PurchaseRequestStatus.Pending && (
        <div className="flex justify-end">
          <Button variant="secondary" disabled={isBusy} onClick={() => cancelMutation.mutate()}>
            Talebi İptal Et
          </Button>
        </div>
      )}

      {request.status === PurchaseRequestStatus.WaitingForCustomerConfirmation && (
        <div className="flex flex-col gap-2">
          <p className="text-xs text-slate-500">
            Satıcı bu talep için fiyat teklif etti. Devam etmek için onaylayın ya da reddedin.
          </p>
          <div className="flex justify-end gap-2">
            <Button variant="secondary" disabled={isBusy} onClick={() => rejectMutation.mutate()}>
              Reddet
            </Button>
            <Button disabled={isBusy} onClick={() => confirmMutation.mutate()}>
              Onayla
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

export function PurchaseRequestsPage() {
  const requestsQuery = useQuery({ queryKey: ["purchase-requests"], queryFn: getPurchaseRequests });

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Taleplerim</h1>
        <p className="text-sm text-slate-500">Oluşturduğunuz satın alma taleplerinin durumu.</p>
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
            <p className="text-sm text-slate-500">Henüz bir satın alma talebiniz yok.</p>
            <Link to="/" className="mt-3 inline-block text-sm font-medium text-slate-900 hover:underline">
              Ürünlere göz atın →
            </Link>
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
