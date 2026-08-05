import { apiClient } from "./client";

export enum PurchaseRequestStatus {
  Pending = 1,
  WaitingForCustomerConfirmation = 2,
  Approved = 3,
  Rejected = 4,
  Cancelled = 5,
}

export const purchaseRequestStatusLabels: Record<PurchaseRequestStatus, string> = {
  [PurchaseRequestStatus.Pending]: "Beklemede",
  [PurchaseRequestStatus.WaitingForCustomerConfirmation]: "Onayınız Bekleniyor",
  [PurchaseRequestStatus.Approved]: "Onaylandı",
  [PurchaseRequestStatus.Rejected]: "Reddedildi",
  [PurchaseRequestStatus.Cancelled]: "İptal Edildi",
};

export interface PurchaseRequestItemDto {
  productId: string;
  productTitle: string;
  originalPrice: number | null;
  negotiatedPrice: number | null;
}

export interface PurchaseRequestDto {
  id: string;
  applicationUserId: string;
  customerName: string;
  customerEmail: string;
  status: PurchaseRequestStatus;
  createdAt: string;
  items: PurchaseRequestItemDto[];
}

export async function getPurchaseRequests(): Promise<PurchaseRequestDto[]> {
  const { data } = await apiClient.get<PurchaseRequestDto[]>("/purchase-requests");
  return data;
}

export async function createPurchaseRequest(productIds: string[]): Promise<PurchaseRequestDto> {
  const { data } = await apiClient.post<PurchaseRequestDto>("/purchase-requests", { productIds });
  return data;
}

export async function cancelPurchaseRequest(id: string): Promise<PurchaseRequestDto> {
  const { data } = await apiClient.patch<PurchaseRequestDto>(`/purchase-requests/${id}/cancel`);
  return data;
}

export async function confirmPurchaseRequest(id: string): Promise<PurchaseRequestDto> {
  const { data } = await apiClient.patch<PurchaseRequestDto>(`/purchase-requests/${id}/confirm`);
  return data;
}

export async function rejectPurchaseRequest(id: string): Promise<PurchaseRequestDto> {
  const { data } = await apiClient.patch<PurchaseRequestDto>(`/purchase-requests/${id}/reject`);
  return data;
}
