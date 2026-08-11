import { apiClient } from "./client";

export interface DashboardStatsDto {
  totalProducts: number;
  totalCustomers: number;
  pendingPurchaseRequests: number;
  productsAwaitingAttention: number;
  acquisitionBatchesInProgress: number;
}

export async function getDashboardStats(): Promise<DashboardStatsDto> {
  const { data } = await apiClient.get<DashboardStatsDto>("/admin/dashboard");
  return data;
}
