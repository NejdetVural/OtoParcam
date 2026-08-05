import { apiClient } from "./client";

export interface CategoryDto {
  id: string;
  name: string;
}

export interface CategoryRequest {
  name: string;
}

export async function getCategories(): Promise<CategoryDto[]> {
  const { data } = await apiClient.get<CategoryDto[]>("/categories");
  return data;
}

export async function createCategory(request: CategoryRequest): Promise<CategoryDto> {
  const { data } = await apiClient.post<CategoryDto>("/categories", request);
  return data;
}

export async function updateCategory(id: string, request: CategoryRequest): Promise<void> {
  await apiClient.put(`/categories/${id}`, request);
}

export async function deleteCategory(id: string): Promise<void> {
  await apiClient.delete(`/categories/${id}`);
}
