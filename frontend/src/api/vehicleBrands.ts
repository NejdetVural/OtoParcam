import { apiClient } from "./client";

export interface VehicleBrandDto {
  id: string;
  name: string;
}

export interface VehicleBrandRequest {
  name: string;
}

export async function getVehicleBrands(): Promise<VehicleBrandDto[]> {
  const { data } = await apiClient.get<VehicleBrandDto[]>("/vehicle-brands");
  return data;
}

export async function createVehicleBrand(request: VehicleBrandRequest): Promise<VehicleBrandDto> {
  const { data } = await apiClient.post<VehicleBrandDto>("/vehicle-brands", request);
  return data;
}

export async function updateVehicleBrand(id: string, request: VehicleBrandRequest): Promise<void> {
  await apiClient.put(`/vehicle-brands/${id}`, request);
}

export async function deleteVehicleBrand(id: string): Promise<void> {
  await apiClient.delete(`/vehicle-brands/${id}`);
}
