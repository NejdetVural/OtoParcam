import { apiClient } from "./client";

export interface AcquisitionBatchDto {
  id: string;
  source: string;
  totalCost: number;
  purchaseDate: string;
  notes: string | null;
  closedAt: string | null;
  partCount: number;
  availableCount: number;
  soldCount: number;
  hiddenCount: number;
  estimatedCostPerPart: number | null;
  revenueSoFar: number;
  profitSoFar: number;
}

export interface AcquisitionBatchRequest {
  source: string;
  totalCost: number;
  purchaseDate: string;
  notes?: string | null;
}

export async function getAcquisitionBatches(): Promise<AcquisitionBatchDto[]> {
  const { data } = await apiClient.get<AcquisitionBatchDto[]>("/admin/acquisition-batches");
  return data;
}

export async function getAcquisitionBatchById(id: string): Promise<AcquisitionBatchDto> {
  const { data } = await apiClient.get<AcquisitionBatchDto>(`/admin/acquisition-batches/${id}`);
  return data;
}

export async function createAcquisitionBatch(request: AcquisitionBatchRequest): Promise<AcquisitionBatchDto> {
  const { data } = await apiClient.post<AcquisitionBatchDto>("/admin/acquisition-batches", request);
  return data;
}

export async function updateAcquisitionBatch(id: string, request: AcquisitionBatchRequest): Promise<AcquisitionBatchDto> {
  const { data } = await apiClient.put<AcquisitionBatchDto>(`/admin/acquisition-batches/${id}`, request);
  return data;
}

export async function deleteAcquisitionBatch(id: string): Promise<void> {
  await apiClient.delete(`/admin/acquisition-batches/${id}`);
}

export async function closeAcquisitionBatch(id: string): Promise<AcquisitionBatchDto> {
  const { data } = await apiClient.patch<AcquisitionBatchDto>(`/admin/acquisition-batches/${id}/close`);
  return data;
}

export async function reopenAcquisitionBatch(id: string): Promise<AcquisitionBatchDto> {
  const { data } = await apiClient.patch<AcquisitionBatchDto>(`/admin/acquisition-batches/${id}/reopen`);
  return data;
}
