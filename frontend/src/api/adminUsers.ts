import { apiClient } from "./client";

export interface AdminUserDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  emailConfirmed: boolean;
  isAdministrator: boolean;
  createdAt: string;
  favoriteCount: number;
  purchaseRequestCount: number;
}

export interface AdminUserListQuery {
  keyword?: string;
}

export async function getUsers(query: AdminUserListQuery = {}): Promise<AdminUserDto[]> {
  const { data } = await apiClient.get<AdminUserDto[]>("/admin/users", { params: query });
  return data;
}

export async function promoteUser(id: string): Promise<AdminUserDto> {
  const { data } = await apiClient.patch<AdminUserDto>(`/admin/users/${id}/promote`);
  return data;
}

export async function demoteUser(id: string): Promise<AdminUserDto> {
  const { data } = await apiClient.patch<AdminUserDto>(`/admin/users/${id}/demote`);
  return data;
}
