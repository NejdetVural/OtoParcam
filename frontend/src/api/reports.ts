import { apiClient } from "./client";

// Mirrors backend/src/OtoParcam.Application/Reports/ReportModels.cs's ReportPeriod.
// No JsonStringEnumConverter is registered in the API, but ASP.NET Core model-binds query-string enum
// values by name (case-insensitive), so these are sent as plain strings.
export enum ReportPeriod {
  AllTime = "AllTime",
  Daily = "Daily",
  Weekly = "Weekly",
  Monthly = "Monthly",
}

export const reportPeriodLabels: Record<ReportPeriod, string> = {
  [ReportPeriod.AllTime]: "Tümü",
  [ReportPeriod.Daily]: "Günlük",
  [ReportPeriod.Weekly]: "Haftalık",
  [ReportPeriod.Monthly]: "Aylık",
};

export async function downloadStatisticsReport(period: ReportPeriod): Promise<void> {
  const { data } = await apiClient.get("/admin/reports/statistics", {
    params: { period },
    responseType: "blob",
  });

  const url = URL.createObjectURL(new Blob([data], { type: "application/pdf" }));
  const link = document.createElement("a");
  link.href = url;
  link.download = `otoparcam-rapor-${new Date().toISOString().slice(0, 10)}.pdf`;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
