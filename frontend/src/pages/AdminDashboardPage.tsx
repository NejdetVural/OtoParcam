import { useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { getDashboardStats } from "../api/dashboard";
import { downloadStatisticsReport, reportPeriodLabels, ReportPeriod } from "../api/reports";
import { Button } from "../components/ui/Button";
import { Spinner } from "../components/ui/Spinner";

function StatTile({ label, value, hint, to }: { label: string; value: number; hint?: string; to?: string }) {
  const content = (
    <>
      <span className="text-sm text-slate-500">{label}</span>
      <span className="text-3xl font-semibold text-slate-900">{value}</span>
      {hint && <span className="text-xs text-slate-400">{hint}</span>}
    </>
  );

  if (to) {
    return (
      <Link
        to={to}
        className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
      >
        {content}
      </Link>
    );
  }

  return <div className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white shadow-sm p-5">{content}</div>;
}

export function AdminDashboardPage() {
  const statsQuery = useQuery({ queryKey: ["admin-dashboard-stats"], queryFn: getDashboardStats });
  const [reportError, setReportError] = useState<string | null>(null);
  const [reportPeriod, setReportPeriod] = useState<ReportPeriod>(ReportPeriod.AllTime);

  const reportMutation = useMutation({
    mutationFn: () => downloadStatisticsReport(reportPeriod),
    onError: () => setReportError("Rapor oluşturulurken bir hata oluştu."),
    onSuccess: () => setReportError(null),
  });

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900">Yönetim Paneli</h1>
          <p className="text-sm text-slate-500">Genel durum özeti.</p>
        </div>
        <div className="flex flex-col items-end gap-2">
          <div className="flex items-center gap-1">
            {Object.values(ReportPeriod).map((value) => (
              <button
                key={value}
                type="button"
                onClick={() => setReportPeriod(value)}
                className={`rounded-full px-3 py-1 text-xs font-medium transition-colors ${
                  reportPeriod === value
                    ? "bg-slate-900 text-white"
                    : "bg-slate-100 text-slate-600 hover:bg-slate-200"
                }`}
              >
                {reportPeriodLabels[value]}
              </button>
            ))}
          </div>
          <Button variant="secondary" disabled={reportMutation.isPending} onClick={() => reportMutation.mutate()}>
            {reportMutation.isPending ? "Oluşturuluyor…" : "İstatistik Raporu İndir (PDF)"}
          </Button>
          {reportError && <span className="text-xs text-red-600">{reportError}</span>}
        </div>
      </div>

      {statsQuery.isLoading && (
        <div className="flex justify-center py-16">
          <Spinner />
        </div>
      )}

      {statsQuery.isError && (
        <p className="rounded-lg border border-red-100 bg-red-50 p-4 text-sm text-red-700">
          İstatistikler yüklenirken bir hata oluştu.
        </p>
      )}

      {statsQuery.data && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatTile label="Toplam Ürün" value={statsQuery.data.totalProducts} to="/admin/urunler" />
          <StatTile label="Toplam Müşteri" value={statsQuery.data.totalCustomers} />
          <StatTile label="Bekleyen Talepler" value={statsQuery.data.pendingPurchaseRequests} to="/admin/talepler" />
          <StatTile
            label="İlgi Bekleyen Ürünler"
            value={statsQuery.data.productsAwaitingAttention}
            hint="Görseli veya uyumluluk bilgisi olmayan satıştaki ürünler"
            to="/admin/urunler"
          />
          <StatTile
            label="Süren Toplu Alımlar"
            value={statsQuery.data.acquisitionBatchesInProgress}
            hint="Satıştaki parçası kalan toplu alımlar"
            to="/admin/toplu-alimlar"
          />
        </div>
      )}

      <div>
        <h2 className="mb-3 text-sm font-medium text-slate-700">Katalog Yönetimi</h2>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <Link
            to="/admin/urunler"
            className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
          >
            <span className="text-sm font-medium text-slate-900">Ürün Yönetimi</span>
            <span className="text-xs text-slate-500">Parça ekle, düzenle, gizle</span>
          </Link>
          <Link
            to="/admin/kategoriler"
            className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
          >
            <span className="text-sm font-medium text-slate-900">Kategoriler</span>
            <span className="text-xs text-slate-500">Ürün kategorilerini yönet</span>
          </Link>
          <Link
            to="/admin/markalar"
            className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
          >
            <span className="text-sm font-medium text-slate-900">Araç Markaları</span>
            <span className="text-xs text-slate-500">Marka ekle, düzenle, sil</span>
          </Link>
          <Link
            to="/admin/modeller"
            className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
          >
            <span className="text-sm font-medium text-slate-900">Araç Modelleri</span>
            <span className="text-xs text-slate-500">Model ekle, düzenle, sil</span>
          </Link>
          <Link
            to="/admin/toplu-alimlar"
            className="flex flex-col gap-1 rounded-xl border border-slate-200 bg-white shadow-sm p-5 transition-shadow hover:shadow-md"
          >
            <span className="text-sm font-medium text-slate-900">Toplu Alımlar</span>
            <span className="text-xs text-slate-500">Sigorta/hasarlı araç lotu maliyeti gir</span>
          </Link>
        </div>
      </div>
    </div>
  );
}
