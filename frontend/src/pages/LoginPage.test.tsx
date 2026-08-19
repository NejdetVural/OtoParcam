import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { QueryClientProvider } from "@tanstack/react-query";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { AuthProvider } from "../auth/AuthContext";
import { createTestQueryClient, makeCustomerToken } from "../testUtils";
import { LoginPage } from "./LoginPage";

vi.mock("../api/auth", () => ({
  login: vi.fn(),
  register: vi.fn(),
}));

import * as authApi from "../api/auth";

function renderLoginPage() {
  const queryClient = createTestQueryClient();
  return render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <MemoryRouter initialEntries={["/giris"]}>
          <Routes>
            <Route path="/giris" element={<LoginPage />} />
            <Route path="/" element={<div>Ana Sayfa</div>} />
          </Routes>
        </MemoryRouter>
      </AuthProvider>
    </QueryClientProvider>,
  );
}

describe("LoginPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("logs in with the entered credentials and navigates home on success", async () => {
    vi.mocked(authApi.login).mockResolvedValue({ token: makeCustomerToken(), expiresAtUtc: "2099-01-01T00:00:00Z" });
    const user = userEvent.setup();
    renderLoginPage();

    await user.type(screen.getByLabelText("E-posta veya Telefon"), "customer@example.com");
    await user.type(screen.getByLabelText("Şifre"), "Str0ng!Pass1");
    await user.click(screen.getByRole("button", { name: "Giriş Yap" }));

    await waitFor(() => expect(screen.getByText("Ana Sayfa")).toBeInTheDocument());
    expect(authApi.login).toHaveBeenCalledWith({ emailOrPhone: "customer@example.com", password: "Str0ng!Pass1" });
  });

  it("normalizes a bare phone number before submitting", async () => {
    vi.mocked(authApi.login).mockResolvedValue({ token: makeCustomerToken(), expiresAtUtc: "2099-01-01T00:00:00Z" });
    const user = userEvent.setup();
    renderLoginPage();

    await user.type(screen.getByLabelText("E-posta veya Telefon"), "5551234567");
    await user.type(screen.getByLabelText("Şifre"), "Str0ng!Pass1");
    await user.click(screen.getByRole("button", { name: "Giriş Yap" }));

    await waitFor(() => expect(authApi.login).toHaveBeenCalled());
    expect(vi.mocked(authApi.login).mock.calls[0][0].emailOrPhone).toContain("555");
  });

  it("shows a translated error message and stays on the page when login fails", async () => {
    vi.mocked(authApi.login).mockRejectedValue({
      isAxiosError: true,
      response: { data: { errors: ["Invalid credentials."] } },
    });
    const user = userEvent.setup();
    renderLoginPage();

    await user.type(screen.getByLabelText("E-posta veya Telefon"), "customer@example.com");
    await user.type(screen.getByLabelText("Şifre"), "WrongPass1!");
    await user.click(screen.getByRole("button", { name: "Giriş Yap" }));

    expect(await screen.findByText("E-posta/telefon veya şifre hatalı.")).toBeInTheDocument();
    expect(screen.queryByText("Ana Sayfa")).not.toBeInTheDocument();
  });
});
