import { apiClient } from "./client";

export interface VehicleModelDto {
  id: string;
  vehicleBrandId: string;
  name: string;
  startYear: number;
  endYear: number;
  variant: string | null;
}

export interface VehicleModelRequest {
  vehicleBrandId: string;
  name: string;
  startYear: number;
  endYear: number;
  variant?: string | null;
}

export async function getVehicleModels(): Promise<VehicleModelDto[]> {
  const { data } = await apiClient.get<VehicleModelDto[]>("/vehicle-models");
  return data;
}

export async function createVehicleModel(request: VehicleModelRequest): Promise<VehicleModelDto> {
  const { data } = await apiClient.post<VehicleModelDto>("/vehicle-models", request);
  return data;
}

export async function updateVehicleModel(id: string, request: VehicleModelRequest): Promise<void> {
  await apiClient.put(`/vehicle-models/${id}`, request);
}

export async function deleteVehicleModel(id: string): Promise<void> {
  await apiClient.delete(`/vehicle-models/${id}`);
}
