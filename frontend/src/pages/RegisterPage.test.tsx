import { beforeEach, describe, expect, it, vi } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderPage } from "../testUtils";
import { RegisterPage } from "./RegisterPage";

vi.mock("../api/auth", () => ({
  login: vi.fn(),
  register: vi.fn(),
  resendConfirmationEmail: vi.fn(),
}));

import * as authApi from "../api/auth";

async function fillAndSubmit(user: ReturnType<typeof userEvent.setup>, email = "yeni@example.com") {
  await user.type(screen.getByLabelText("Ad"), "Ahmet");
  await user.type(screen.getByLabelText("Soyad"), "Yılmaz");
  await user.type(screen.getByLabelText("E-posta"), email);
  await user.type(screen.getByLabelText("Telefon"), "5551234567");
  await user.type(screen.getByLabelText("Şifre"), "Str0ng!Pass1");
  await user.click(screen.getByRole("checkbox"));
  await user.click(screen.getByRole("button", { name: "Kayıt Ol" }));
}

describe("RegisterPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("disables submit until the privacy policy checkbox is accepted", async () => {
    const user = userEvent.setup();
    renderPage(<RegisterPage />, { route: "/kayit" });

    expect(screen.getByRole("button", { name: "Kayıt Ol" })).toBeDisabled();

    await user.click(screen.getByRole("checkbox"));
    expect(screen.getByRole("button", { name: "Kayıt Ol" })).not.toBeDisabled();
  });

  it("registers and shows the success screen with a disabled, cooling-down resend button", async () => {
    vi.mocked(authApi.register).mockResolvedValue(undefined);
    const user = userEvent.setup();
    renderPage(<RegisterPage />, { route: "/kayit" });

    await fillAndSubmit(user);

    await waitFor(() => expect(screen.getByText("Kaydınız oluşturuldu")).toBeInTheDocument());
    expect(authApi.register).toHaveBeenCalledWith(
      expect.objectContaining({ firstName: "Ahmet", lastName: "Yılmaz", email: "yeni@example.com", privacyPolicyAccepted: true }),
    );

    const resendButton = screen.getByRole("button", { name: /Tekrar Gönder/ });
    expect(resendButton).toBeDisabled();
    expect(resendButton).toHaveTextContent("60s");
  });

  it("shows translated errors and stays on the form when registration fails", async () => {
    vi.mocked(authApi.register).mockRejectedValue({
      isAxiosError: true,
      response: { data: { errors: ["Phone number is already taken."] } },
    });
    const user = userEvent.setup();
    renderPage(<RegisterPage />, { route: "/kayit" });

    await fillAndSubmit(user);

    expect(await screen.findByText("Bu e-posta adresi veya telefon numarası zaten kayıtlı.")).toBeInTheDocument();
    expect(screen.queryByText("Kaydınız oluşturuldu")).not.toBeInTheDocument();
  });

  it("resends the confirmation email once the cooldown expires", async () => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
    vi.mocked(authApi.register).mockResolvedValue(undefined);
    vi.mocked(authApi.resendConfirmationEmail).mockResolvedValue(undefined);
    const user = userEvent.setup({ delay: null, advanceTimers: vi.advanceTimersByTime });
    renderPage(<RegisterPage />, { route: "/kayit" });

    await fillAndSubmit(user, "resend-test@example.com");
    await waitFor(() => expect(screen.getByText("Kaydınız oluşturuldu")).toBeInTheDocument());

    // Still cooling down right after the initial send — clicking must not trigger another call.
    await user.click(screen.getByRole("button", { name: /Tekrar Gönder/ }));
    expect(authApi.resendConfirmationEmail).not.toHaveBeenCalled();

    await vi.advanceTimersByTimeAsync(60_000);
    expect(screen.getByRole("button", { name: "Tekrar Gönder" })).not.toBeDisabled();

    await user.click(screen.getByRole("button", { name: "Tekrar Gönder" }));
    await waitFor(() => expect(authApi.resendConfirmationEmail).toHaveBeenCalledWith("resend-test@example.com"));

    vi.useRealTimers();
  });
});
