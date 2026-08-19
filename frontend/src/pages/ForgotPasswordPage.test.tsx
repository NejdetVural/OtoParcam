import { beforeEach, describe, expect, it, vi } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderPage } from "../testUtils";
import { ForgotPasswordPage } from "./ForgotPasswordPage";

vi.mock("../api/auth", () => ({
  forgotPassword: vi.fn(),
}));

import * as authApi from "../api/auth";

describe("ForgotPasswordPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("submits the email and shows the success screen with a cooling-down resend button", async () => {
    vi.mocked(authApi.forgotPassword).mockResolvedValue(undefined);
    const user = userEvent.setup();
    renderPage(<ForgotPasswordPage />, { route: "/sifremi-unuttum" });

    await user.type(screen.getByLabelText("E-posta"), "customer@example.com");
    await user.click(screen.getByRole("button", { name: "Sıfırlama Bağlantısı Gönder" }));

    await waitFor(() => expect(screen.getByText("Talebiniz alındı")).toBeInTheDocument());
    expect(authApi.forgotPassword).toHaveBeenCalledWith("customer@example.com");

    const resendButton = screen.getByRole("button", { name: /Tekrar Gönder/ });
    expect(resendButton).toBeDisabled();
  });

  it("does not reveal whether the account exists — same success screen regardless", async () => {
    // Backend always no-ops silently for an unknown email (BR-81, avoids account enumeration);
    // forgotPassword() never rejects for that case, so the UI has nothing special to branch on.
    vi.mocked(authApi.forgotPassword).mockResolvedValue(undefined);
    const user = userEvent.setup();
    renderPage(<ForgotPasswordPage />, { route: "/sifremi-unuttum" });

    await user.type(screen.getByLabelText("E-posta"), "nobody@example.com");
    await user.click(screen.getByRole("button", { name: "Sıfırlama Bağlantısı Gönder" }));

    expect(await screen.findByText("Talebiniz alındı")).toBeInTheDocument();
  });

  it("shows an error and stays on the form when the request itself fails", async () => {
    vi.mocked(authApi.forgotPassword).mockRejectedValue(new Error("network down"));
    const user = userEvent.setup();
    renderPage(<ForgotPasswordPage />, { route: "/sifremi-unuttum" });

    await user.type(screen.getByLabelText("E-posta"), "customer@example.com");
    await user.click(screen.getByRole("button", { name: "Sıfırlama Bağlantısı Gönder" }));

    expect(await screen.findByText("Beklenmeyen bir hata oluştu. Lütfen tekrar deneyin.")).toBeInTheDocument();
    expect(screen.queryByText("Talebiniz alındı")).not.toBeInTheDocument();
  });
});
