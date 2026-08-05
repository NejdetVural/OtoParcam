import { apiClient } from "./client";
import type { ProductDto } from "./products";

export async function getFavorites(): Promise<ProductDto[]> {
  const { data } = await apiClient.get<ProductDto[]>("/favorites");
  return data;
}

export async function addFavorite(productId: string): Promise<void> {
  await apiClient.post("/favorites", { productId });
}

export async function removeFavorite(productId: string): Promise<void> {
  await apiClient.delete(`/favorites/${productId}`);
}
