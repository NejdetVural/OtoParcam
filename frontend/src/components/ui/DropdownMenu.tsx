import { useEffect, useRef, useState } from "react";

export interface DropdownMenuItem {
  label: string;
  onClick: () => void;
  disabled?: boolean;
  destructive?: boolean;
  confirm?: { message: string; confirmLabel: string };
}

export function DropdownMenu({ items }: { items: DropdownMenuItem[] }) {
  const [open, setOpen] = useState(false);
  const [confirmingIndex, setConfirmingIndex] = useState<number | null>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  function close() {
    setOpen(false);
    setConfirmingIndex(null);
  }

  useEffect(() => {
    if (!open) return;

    function handlePointerDown(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        close();
      }
    }

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        close();
      }
    }

    document.addEventListener("mousedown", handlePointerDown);
    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("mousedown", handlePointerDown);
      document.removeEventListener("keydown", handleKeyDown);
    };
  }, [open]);

  const confirmingItem = confirmingIndex !== null ? items[confirmingIndex] : null;

  return (
    <div ref={containerRef} className="relative inline-block text-left">
      <button
        type="button"
        aria-haspopup="menu"
        aria-expanded={open}
        aria-label="Seçenekler"
        onClick={() => setOpen((o) => !o)}
        className="inline-flex h-8 w-8 items-center justify-center rounded-lg text-slate-500 hover:bg-slate-100 hover:text-slate-900"
      >
        <svg viewBox="0 0 20 20" className="h-5 w-5" fill="currentColor">
          <circle cx="10" cy="4" r="1.5" />
          <circle cx="10" cy="10" r="1.5" />
          <circle cx="10" cy="16" r="1.5" />
        </svg>
      </button>

      {open && (
        <div
          role="menu"
          className="absolute right-0 z-10 mt-1 w-56 rounded-lg border border-slate-200 bg-white p-1 shadow-lg"
        >
          {confirmingItem ? (
            <div className="flex flex-col gap-2 p-2">
              <p className="text-xs text-slate-600">{confirmingItem.confirm!.message}</p>
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setConfirmingIndex(null)}
                  className="rounded-lg px-2.5 py-1 text-xs font-medium text-slate-600 hover:bg-slate-100"
                >
                  Vazgeç
                </button>
                <button
                  type="button"
                  onClick={() => {
                    confirmingItem.onClick();
                    close();
                  }}
                  className="rounded-lg bg-red-600 px-2.5 py-1 text-xs font-medium text-white hover:bg-red-700"
                >
                  {confirmingItem.confirm!.confirmLabel}
                </button>
              </div>
            </div>
          ) : (
            items.map((item, index) => (
              <button
                key={item.label}
                type="button"
                role="menuitem"
                disabled={item.disabled}
                onClick={() => {
                  if (item.confirm) {
                    setConfirmingIndex(index);
                  } else {
                    item.onClick();
                    close();
                  }
                }}
                className={`block w-full rounded-md px-3 py-2 text-left text-sm transition-colors disabled:cursor-not-allowed disabled:text-slate-300 ${
                  item.destructive ? "text-red-600 hover:bg-red-50" : "text-slate-700 hover:bg-slate-100"
                }`}
              >
                {item.label}
              </button>
            ))
          )}
        </div>
      )}
    </div>
  );
}
