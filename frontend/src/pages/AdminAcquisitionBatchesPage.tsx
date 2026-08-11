import { useMemo, useState, type FormEvent } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  createAcquisitionBatch,
  deleteAcquisitionBatch,
  getAcquisitionBatches,
  updateAcquisitionBatch,
  type AcquisitionBatchDto,
  type AcquisitionBatchRequest,
} from "../api/acquisitionBatches";
import { extractErrorMessages } from "../api/errors";
import { Button } from "../components/ui/Button";
import { ConfirmButton } from "../components/ui/ConfirmButton";
import { Input } from "../components/ui/Input";
import { Spinner } from "../components/ui/Spinner";

const currencyFormatter = new Intl.NumberFormat("tr-TR", { maximumFractionDigits: 0 });

function formatCurrency(value: number | null): string {
  return value === null ? "—" : `${currencyFormatter.format(value)} ₺`;
}

function toDateInputValue(iso: string): string {
  return iso.slice(0, 10);
}

type FormState = {
  source: string;
  totalCost: string;
  purchaseDate: string;
  notes: string;
};

function toRequest(form: FormState): AcquisitionBatchRequest {
  return {
    source: form.source.trim(),
    totalCost: Number(form.totalCost),
    purchaseDate: form.purchaseDate,
    notes: form.notes.trim() ? form.notes.trim() : null,
  };
}

function BatchStat({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex flex-col">
      <span className="text-xs text-slate-500">{label}</span>
      <span className="text-sm font-medium text-slate-900">{value}</span>
    </div>
  );
}

function BatchCard({ batch }: { batch: AcquisitionBatchDto }) {
  const queryClient = useQueryClient();
  const [editing, setEditing] = useState(false);
  const [form, setForm] = useState<FormState>({
    source: batch.source,
    totalCost: String(batch.totalCost),
    purchaseDate: toDateInputValue(batch.purchaseDate),
    notes: batch.notes ?? "",
  });
  const [error, setError] = useState<string | null>(null);

  const updateMutation = useMutation({
    mutationFn: () => updateAcquisitionBatch(batch.id, toRequest(form)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["acquisition-batches"] });
      setEditing(false);
      setError(null);
    },
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  const deleteMutation = useMutation({
    mutationFn: () => deleteAcquisitionBatch(batch.id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["acquisition-batches"] }),
    onError: (err) => setError(extractErrorMessages(err)[0]),
  });

  if (editing) {
    return (
      <li className="flex flex-col gap-3 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <Input
            label="Kaynak"
            value={form.source}
            onChange={(e) => setForm((p) => ({ ...p, source: e.target.value }))}
            className="sm:col-span-2"
          />
          <Input
            label="Toplam Maliyet"
            type="number"
            min={0}
            step="0.01"
            value={form.totalCost}
            onChange={(e) => setForm((p) => ({ ...p, totalCost: e.target.value }))}
          />
          <Input
            label="Alım Tarihi"
            type="date"
            value={form.purchaseDate}
            onChange={(e) => setForm((p) => ({ ...p, purchaseDate: e.target.value }))}
          />
          <label className="flex flex-col gap-1.5 text-sm sm:col-span-2">
            <span className="font-medium text-slate-700">Notlar (opsiyonel)</span>
            <textarea
              value={form.notes}
              onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))}
              rows={2}
              maxLength={2000}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            />
          </label>
        </div>
        <div className="flex items-center gap-2">
          <Button
            onClick={() => updateMutation.mutate()}
            disabled={updateMutation.isPending || !form.source.trim() || !form.totalCost.trim() || !form.purchaseDate}
          >
            Kaydet
          </Button>
          <Button
            variant="secondary"
            onClick={() => {
              setEditing(false);
              setError(null);
              setForm({
                source: batch.source,
                totalCost: String(batch.totalCost),
                purchaseDate: toDateInputValue(batch.purchaseDate),
                notes: batch.notes ?? "",
              });
            }}
          >
            Vazgeç
          </Button>
        </div>
        {error && <p className="text-sm text-red-600">{error}</p>}
      </li>
    );
  }

  return (
    <li className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h3 className="text-sm font-medium text-slate-900">{batch.source}</h3>
          <p className="text-xs text-slate-500">
            {new Date(batch.purchaseDate).toLocaleDateString("tr-TR")} — Toplam {formatCurrency(batch.totalCost)}
          </p>
          {batch.notes && <p className="mt-1 text-xs text-slate-500">{batch.notes}</p>}
        </div>
        <div className="flex items-center gap-2">
          <Button variant="ghost" onClick={() => setEditing(true)}>
            Düzenle
          </Button>
          <ConfirmButton
            label="Sil"
            confirmLabel="Evet, Sil"
            message={`"${batch.source}" alımını silmek istediğinize emin misiniz?`}
            onConfirm={() => deleteMutation.mutate()}
            disabled={deleteMutation.isPending}
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-3 border-t border-slate-100 pt-4 sm:grid-cols-4">
        <BatchStat label="Parça Sayısı" value={String(batch.partCount)} />
        <BatchStat label="Satışta / Satıldı / Gizli" value={`${batch.availableCount} / ${batch.soldCount} / ${batch.hiddenCount}`} />
        <BatchStat label="Parça Başı Maliyet (tahmini)" value={formatCurrency(batch.estimatedCostPerPart)} />
        <BatchStat label="Gelir (şimdiye kadar)" value={formatCurrency(batch.revenueSoFar)} />
      </div>
      <p className={`text-sm font-medium ${batch.profitSoFar >= 0 ? "text-emerald-600" : "text-slate-500"}`}>
        {batch.profitSoFar >= 0
          ? `Kar (şimdiye kadar): ${formatCurrency(batch.profitSoFar)}`
          : `Henüz maliyeti karşılamadı: ${formatCurrency(batch.profitSoFar)}`}
      </p>
      {error && <p className="text-sm text-red-600">{error}</p>}
    </li>
  );
}

interface SourceSummaryRow {
  source: string;
  batchCount: number;
  totalCost: number;
  partCount: number;
  revenueSoFar: number;
  profitSoFar: number;
}

function SourceSummary({ batches }: { batches: AcquisitionBatchDto[] }) {
  const groups = useMemo(() => {
    const map = new Map<string, SourceSummaryRow>();
    for (const batch of batches) {
      const row =
        map.get(batch.source) ??
        ({ source: batch.source, batchCount: 0, totalCost: 0, partCount: 0, revenueSoFar: 0, profitSoFar: 0 } satisfies SourceSummaryRow);
      row.batchCount += 1;
      row.totalCost += batch.totalCost;
      row.partCount += batch.partCount;
      row.revenueSoFar += batch.revenueSoFar;
      row.profitSoFar += batch.profitSoFar;
      map.set(batch.source, row);
    }
    return [...map.values()].sort((a, b) => b.totalCost - a.totalCost);
  }, [batches]);

  if (groups.length === 0) {
    return null;
  }

  return (
    <div className="rounded-xl border border-slate-200 bg-white shadow-sm p-5">
      <h2 className="text-sm font-medium text-slate-900">Kaynağa Göre Özet</h2>
      <p className="mb-3 text-xs text-slate-500">
        Aynı kaynak adıyla yapılan her toplu alım ayrı bir kayıt olarak kalır (örn. "Ovalı"'ya yapılan her ziyaret kendi
        maliyeti ve parçalarıyla ayrı bir alımdır) — burada sadece aynı kaynak adına ait alımların toplamı gösteriliyor,
        şirketler arası kar marjı karşılaştırması için.
      </p>
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="text-left text-xs text-slate-500">
              <th className="pb-2 pr-4 font-medium">Kaynak</th>
              <th className="pb-2 pr-4 text-right font-medium">Alım Sayısı</th>
              <th className="pb-2 pr-4 text-right font-medium">Toplam Maliyet</th>
              <th className="pb-2 pr-4 text-right font-medium">Toplam Parça</th>
              <th className="pb-2 pr-4 text-right font-medium">Toplam Gelir</th>
              <th className="pb-2 text-right font-medium">Toplam Kar/Zarar</th>
            </tr>
          </thead>
          <tbody>
            {groups.map((g) => (
              <tr key={g.source} className="border-t border-slate-100">
                <td className="py-2 pr-4 font-medium text-slate-900">{g.source}</td>
                <td className="py-2 pr-4 text-right text-slate-700">{g.batchCount}</td>
                <td className="py-2 pr-4 text-right text-slate-700">{formatCurrency(g.totalCost)}</td>
                <td className="py-2 pr-4 text-right text-slate-700">{g.partCount}</td>
                <td className="py-2 pr-4 text-right text-slate-700">{formatCurrency(g.revenueSoFar)}</td>
                <td className={`py-2 text-right font-medium ${g.profitSoFar >= 0 ? "text-emerald-600" : "text-slate-500"}`}>
                  {formatCurrency(g.profitSoFar)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export function AdminAcquisitionBatchesPage() {
  const queryClient = useQueryClient();
  const batchesQuery = useQuery({ queryKey: ["acquisition-batches"], queryFn: getAcquisitionBatches });

  const [form, setForm] = useState<FormState>({ source: "", totalCost: "", purchaseDate: "", notes: "" });
  const [createError, setCreateError] = useState<string | null>(null);

  const createMutation = useMutation({
    mutationFn: () => createAcquisitionBatch(toRequest(form)),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["acquisition-batches"] });
      setForm({ source: "", totalCost: "", purchaseDate: "", notes: "" });
    },
    onError: (err) => setCreateError(extractErrorMessages(err)[0]),
  });

  function handleCreate(e: FormEvent) {
    e.preventDefault();
    setCreateError(null);
    createMutation.mutate();
  }

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-slate-900">Toplu Alımlar</h1>
        <p className="text-sm text-slate-500">
          Birlikte satın alınan (örn. sigortadan hasarlı bir araç) ama tek tek satılan parçalar için tek bir toplu maliyet
          kaydı tutun. Parçaları bu alıma bağlamak için ürün formunda "Toplu Alım" seçin — parça başı maliyet, alıma
          bağlı toplam parça sayısına göre otomatik bölüştürülür.
        </p>
      </div>

      <form onSubmit={handleCreate} className="flex flex-col gap-4 rounded-xl border border-slate-200 bg-white shadow-sm p-5">
        <h2 className="text-sm font-medium text-slate-900">Yeni Toplu Alım</h2>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <Input
            label="Kaynak"
            value={form.source}
            onChange={(e) => setForm((p) => ({ ...p, source: e.target.value }))}
            placeholder="Örn. Ford Focus - sigorta hasarlı, Ankara"
            className="sm:col-span-2"
            required
          />
          <Input
            label="Toplam Maliyet"
            type="number"
            min={0}
            step="0.01"
            value={form.totalCost}
            onChange={(e) => setForm((p) => ({ ...p, totalCost: e.target.value }))}
            required
          />
          <Input
            label="Alım Tarihi"
            type="date"
            value={form.purchaseDate}
            onChange={(e) => setForm((p) => ({ ...p, purchaseDate: e.target.value }))}
            required
          />
          <label className="flex flex-col gap-1.5 text-sm sm:col-span-2">
            <span className="font-medium text-slate-700">Notlar (opsiyonel)</span>
            <textarea
              value={form.notes}
              onChange={(e) => setForm((p) => ({ ...p, notes: e.target.value }))}
              rows={2}
              maxLength={2000}
              className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-sm focus:border-slate-400 focus:outline-none focus:ring-1 focus:ring-slate-400"
            />
          </label>
        </div>
        <Button
          type="submit"
          className="self-start"
          disabled={createMutation.isPending || !form.source.trim() || !form.totalCost.trim() || !form.purchaseDate}
        >
          {createMutation.isPending ? "Ekleniyor…" : "Ekle"}
        </Button>
        {createError && <p className="text-sm text-red-600">{createError}</p>}
      </form>

      {batchesQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {batchesQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          Toplu alımlar yüklenirken bir hata oluştu.
        </p>
      )}

      {batchesQuery.data && batchesQuery.data.length === 0 && (
        <p className="rounded-lg border border-slate-200 bg-slate-50 p-4 text-sm text-slate-500">
          Henüz toplu alım kaydı yok.
        </p>
      )}

      {batchesQuery.data && batchesQuery.data.length > 0 && (
        <>
          <SourceSummary batches={batchesQuery.data} />
          <ul className="flex flex-col gap-3">
            {batchesQuery.data.map((batch) => (
              <BatchCard key={batch.id} batch={batch} />
            ))}
          </ul>
        </>
      )}
    </div>
  );
}
