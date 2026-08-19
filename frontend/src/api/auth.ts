import { apiClient } from "./client";

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  password: string;
  privacyPolicyAccepted: boolean;
}

export interface LoginRequest {
  emailOrPhone: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  expiresAtUtc: string;
}

export async function register(request: RegisterRequest): Promise<void> {
  await apiClient.post("/auth/register", request);
}

export async function confirmEmail(userId: string, token: string): Promise<void> {
  await apiClient.get("/auth/confirm-email", { params: { userId, token } });
}

export async function resendConfirmationEmail(email: string): Promise<void> {
  await apiClient.post("/auth/resend-confirmation", { email });
}

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>("/auth/login", request);
  return data;
}

export async function forgotPassword(email: string): Promise<void> {
  await apiClient.post("/auth/forgot-password", { email });
}

export interface ResetPasswordRequest {
  userId: string;
  token: string;
  newPassword: string;
}

export async function resetPassword(request: ResetPasswordRequest): Promise<void> {
  await apiClient.post("/auth/reset-password", request);
}
