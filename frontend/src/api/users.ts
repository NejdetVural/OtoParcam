import { apiClient } from "./client";

export interface UserProfileDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
}

export interface UpdateUserProfileRequest {
  firstName: string;
  lastName: string;
}

export async function getMyProfile(): Promise<UserProfileDto> {
  const { data } = await apiClient.get<UserProfileDto>("/users/me");
  return data;
}

export async function updateMyProfile(request: UpdateUserProfileRequest): Promise<UserProfileDto> {
  const { data } = await apiClient.put<UserProfileDto>("/users/me", request);
  return data;
}
