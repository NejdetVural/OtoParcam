import { apiClient } from "./client";
import type { PagedResult, ProductColor, ProductPosition, ProductSide, ProductStatus } from "./types";

export interface ProductImageDto {
  id: string;
  imageUrl: string;
  displayOrder: number;
}

export interface ProductDto {
  id: string;
  title: string;
  categoryId: string;
  categoryName: string;
  sourceVehicleModelId: string;
  vehicleBrandId: string;
  vehicleBrandName: string;
  vehicleModelName: string;
  startYear: number;
  endYear: number;
  variant: string | null;
  price: number | null;
  soldPrice: number | null;
  acquisitionCost: number | null;
  acquisitionSource: string | null;
  acquisitionBatchId: string | null;
  acquisitionBatchSource: string | null;
  effectiveAcquisitionCost: number | null;
  effectiveAcquisitionSource: string | null;
  color: ProductColor;
  status: ProductStatus;
  side: ProductSide | null;
  position: ProductPosition | null;
  description: string | null;
  images: ProductImageDto[];
}

export interface CompatibleVehicleModelDto {
  vehicleModelId: string;
  vehicleBrandId: string;
  vehicleBrandName: string;
  vehicleModelName: string;
  startYear: number;
  endYear: number;
  variant: string | null;
}

export interface ProductListQuery {
  categoryId?: string;
  vehicleBrandId?: string;
  vehicleModelId?: string;
  keyword?: string;
  color?: ProductColor;
  status?: ProductStatus;
  page?: number;
  sortBy?: "priceAsc" | "priceDesc";
}

export async function getProducts(query: ProductListQuery): Promise<PagedResult<ProductDto>> {
  const { data } = await apiClient.get<PagedResult<ProductDto>>("/products", { params: query });
  return data;
}

export async function getProductById(id: string): Promise<ProductDto> {
  const { data } = await apiClient.get<ProductDto>(`/products/${id}`);
  return data;
}

export async function getProductCompatibility(id: string): Promise<CompatibleVehicleModelDto[]> {
  const { data } = await apiClient.get<CompatibleVehicleModelDto[]>(`/products/${id}/compatibility`);
  return data;
}

export interface ProductRequest {
  categoryId: string;
  sourceVehicleModelId: string;
  price?: number | null;
  acquisitionCost?: number | null;
  acquisitionSource?: string | null;
  acquisitionBatchId?: string | null;
  color: ProductColor;
  side?: ProductSide | null;
  position?: ProductPosition | null;
  description?: string | null;
  status?: ProductStatus;
}

export async function createProduct(request: ProductRequest): Promise<ProductDto> {
  const { data } = await apiClient.post<ProductDto>("/products", request);
  return data;
}

export async function updateProduct(id: string, request: ProductRequest): Promise<void> {
  await apiClient.put(`/products/${id}`, request);
}

export async function hideProduct(id: string): Promise<void> {
  await apiClient.delete(`/products/${id}`);
}

export async function restoreProduct(id: string): Promise<void> {
  await apiClient.patch(`/products/${id}/restore`);
}

export async function markProductSold(id: string, soldPrice: number): Promise<ProductDto> {
  const { data } = await apiClient.patch<ProductDto>(`/products/${id}/sell`, { soldPrice });
  return data;
}

export async function addProductImage(id: string, file: File): Promise<ProductImageDto> {
  const formData = new FormData();
  formData.append("file", file);
  const { data } = await apiClient.post<ProductImageDto>(`/products/${id}/images`, formData);
  return data;
}

export async function deleteProductImage(id: string, imageId: string): Promise<void> {
  await apiClient.delete(`/products/${id}/images/${imageId}`);
}

export async function addProductCompatibility(id: string, vehicleModelId: string): Promise<CompatibleVehicleModelDto> {
  const { data } = await apiClient.post<CompatibleVehicleModelDto>(`/products/${id}/compatibility`, { vehicleModelId });
  return data;
}

export async function removeProductCompatibility(id: string, vehicleModelId: string): Promise<void> {
  await apiClient.delete(`/products/${id}/compatibility/${vehicleModelId}`);
}
