import axios from "axios";
import { getToken, clearSession } from "../auth/session";

const apiBaseUrl: string = import.meta.env.VITE_API_BASE_URL;

// Origin only (no /api/v1 suffix) — used to resolve server-relative URLs like uploaded product images.
export const apiOrigin = new URL(apiBaseUrl).origin;

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
});

apiClient.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      clearSession();
    }
    return Promise.reject(error);
  },
);
