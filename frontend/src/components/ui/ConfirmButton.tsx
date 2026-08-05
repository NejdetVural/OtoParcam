import { useState } from "react";
import { Button } from "./Button";

interface ConfirmButtonProps {
  label: string;
  confirmLabel: string;
  message: string;
  onConfirm: () => void;
  disabled?: boolean;
  triggerVariant?: "ghost" | "secondary";
}

export function ConfirmButton({
  label,
  confirmLabel,
  message,
  onConfirm,
  disabled = false,
  triggerVariant = "ghost",
}: ConfirmButtonProps) {
  const [confirming, setConfirming] = useState(false);

  if (confirming) {
    return (
      <div className="flex flex-wrap items-center gap-2">
        <span className="text-xs text-slate-500">{message}</span>
        <Button
          variant="secondary"
          className="border-red-200 text-red-600 hover:border-red-300"
          disabled={disabled}
          onClick={() => {
            setConfirming(false);
            onConfirm();
          }}
        >
          {confirmLabel}
        </Button>
        <Button variant="ghost" onClick={() => setConfirming(false)}>
          Vazgeç
        </Button>
      </div>
    );
  }

  return (
    <Button variant={triggerVariant} disabled={disabled} onClick={() => setConfirming(true)}>
      {label}
    </Button>
  );
}
