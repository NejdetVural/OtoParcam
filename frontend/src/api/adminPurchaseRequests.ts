import { apiClient } from "./client";
import type { PurchaseRequestDto } from "./purchaseRequests";

export interface NegotiationItemRequest {
  productId: string;
  negotiatedPrice: number;
}

export async function getAllPurchaseRequests(): Promise<PurchaseRequestDto[]> {
  const { data } = await apiClient.get<PurchaseRequestDto[]>("/admin/purchase-requests");
  return data;
}

export async function updateNegotiation(id: string, items: NegotiationItemRequest[]): Promise<PurchaseRequestDto> {
  const { data } = await apiClient.patch<PurchaseRequestDto>(`/admin/purchase-requests/${id}/negotiation`, { items });
  return data;
}
