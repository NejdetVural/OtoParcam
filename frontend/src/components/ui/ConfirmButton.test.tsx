import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ConfirmButton } from "./ConfirmButton";

describe("ConfirmButton", () => {
  it("shows only the trigger button initially", () => {
    render(<ConfirmButton label="Sil" confirmLabel="Evet, Sil" message="Emin misiniz?" onConfirm={vi.fn()} />);
    expect(screen.getByRole("button", { name: "Sil" })).toBeInTheDocument();
    expect(screen.queryByText("Emin misiniz?")).not.toBeInTheDocument();
  });

  it("reveals the confirmation message and actions after clicking the trigger", async () => {
    const user = userEvent.setup();
    render(<ConfirmButton label="Sil" confirmLabel="Evet, Sil" message="Emin misiniz?" onConfirm={vi.fn()} />);

    await user.click(screen.getByRole("button", { name: "Sil" }));

    expect(screen.getByText("Emin misiniz?")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Evet, Sil" })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Vazgeç" })).toBeInTheDocument();
  });

  it("calls onConfirm and collapses back to the trigger after confirming", async () => {
    const onConfirm = vi.fn();
    const user = userEvent.setup();
    render(<ConfirmButton label="Sil" confirmLabel="Evet, Sil" message="Emin misiniz?" onConfirm={onConfirm} />);

    await user.click(screen.getByRole("button", { name: "Sil" }));
    await user.click(screen.getByRole("button", { name: "Evet, Sil" }));

    expect(onConfirm).toHaveBeenCalledTimes(1);
    expect(screen.getByRole("button", { name: "Sil" })).toBeInTheDocument();
    expect(screen.queryByText("Emin misiniz?")).not.toBeInTheDocument();
  });

  it("cancels without calling onConfirm when Vazgeç is clicked", async () => {
    const onConfirm = vi.fn();
    const user = userEvent.setup();
    render(<ConfirmButton label="Sil" confirmLabel="Evet, Sil" message="Emin misiniz?" onConfirm={onConfirm} />);

    await user.click(screen.getByRole("button", { name: "Sil" }));
    await user.click(screen.getByRole("button", { name: "Vazgeç" }));

    expect(onConfirm).not.toHaveBeenCalled();
    expect(screen.getByRole("button", { name: "Sil" })).toBeInTheDocument();
  });

  it("disables the trigger button when disabled is set", () => {
    render(
      <ConfirmButton
        label="Sil"
        confirmLabel="Evet, Sil"
        message="Emin misiniz?"
        onConfirm={vi.fn()}
        disabled
      />,
    );
    expect(screen.getByRole("button", { name: "Sil" })).toBeDisabled();
  });
});
