import { beforeEach, describe, expect, it, vi } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderPage } from "../testUtils";
import { AdminAcquisitionBatchesPage } from "./AdminAcquisitionBatchesPage";
import type { AcquisitionBatchDto } from "../api/acquisitionBatches";

vi.mock("../api/acquisitionBatches", () => ({
  getAcquisitionBatches: vi.fn(),
  createAcquisitionBatch: vi.fn(),
  updateAcquisitionBatch: vi.fn(),
  deleteAcquisitionBatch: vi.fn(),
  closeAcquisitionBatch: vi.fn(),
  reopenAcquisitionBatch: vi.fn(),
}));

import * as batchesApi from "../api/acquisitionBatches";

function makeBatch(overrides: Partial<AcquisitionBatchDto> = {}): AcquisitionBatchDto {
  return {
    id: "batch-1",
    source: "Ovalı",
    totalCost: 1000,
    purchaseDate: "2026-08-01T00:00:00Z",
    notes: null,
    closedAt: null,
    partCount: 2,
    availableCount: 1,
    soldCount: 1,
    hiddenCount: 0,
    estimatedCostPerPart: 500,
    revenueSoFar: 600,
    profitSoFar: -400,
    ...overrides,
  };
}

describe("AdminAcquisitionBatchesPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders batches with an Açık badge and a working Alımı Bitir action", async () => {
    vi.mocked(batchesApi.getAcquisitionBatches).mockResolvedValue([makeBatch()]);
    vi.mocked(batchesApi.closeAcquisitionBatch).mockResolvedValue(makeBatch({ closedAt: "2026-08-18T00:00:00Z" }));
    const user = userEvent.setup();
    renderPage(<AdminAcquisitionBatchesPage />, { route: "/admin/toplu-alimlar" });

    await waitFor(() => expect(screen.getByRole("heading", { name: "Ovalı" })).toBeInTheDocument());
    expect(screen.getByText("Açık")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Alımı Bitir" }));
    await user.click(screen.getByRole("button", { name: "Evet, Bitir" }));

    await waitFor(() => expect(batchesApi.closeAcquisitionBatch).toHaveBeenCalledWith("batch-1"));
  });

  it("shows a Kapalı badge and a Yeniden Aç action for a closed batch", async () => {
    vi.mocked(batchesApi.getAcquisitionBatches).mockResolvedValue([makeBatch({ closedAt: "2026-08-15T00:00:00Z" })]);
    vi.mocked(batchesApi.reopenAcquisitionBatch).mockResolvedValue(makeBatch({ closedAt: null }));
    const user = userEvent.setup();
    renderPage(<AdminAcquisitionBatchesPage />, { route: "/admin/toplu-alimlar" });

    await waitFor(() => expect(screen.getByText("Kapalı")).toBeInTheDocument());
    expect(screen.queryByRole("button", { name: "Alımı Bitir" })).not.toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Yeniden Aç" }));

    await waitFor(() => expect(batchesApi.reopenAcquisitionBatch).toHaveBeenCalledWith("batch-1"));
  });

  it("creates a new batch from the form", async () => {
    vi.mocked(batchesApi.getAcquisitionBatches).mockResolvedValue([]);
    vi.mocked(batchesApi.createAcquisitionBatch).mockResolvedValue(makeBatch({ id: "batch-2", source: "Gümüş" }));
    const user = userEvent.setup();
    renderPage(<AdminAcquisitionBatchesPage />, { route: "/admin/toplu-alimlar" });

    await waitFor(() => expect(screen.getByText("Henüz toplu alım kaydı yok.")).toBeInTheDocument());

    await user.type(screen.getByLabelText("Kaynak"), "Gümüş");
    await user.type(screen.getByLabelText("Toplam Maliyet"), "1500");
    const dateInput = document.querySelector('input[type="date"]') as HTMLInputElement;
    await user.type(dateInput, "2026-08-18");
    await user.click(screen.getByRole("button", { name: "Ekle" }));

    await waitFor(() =>
      expect(batchesApi.createAcquisitionBatch).toHaveBeenCalledWith(
        expect.objectContaining({ source: "Gümüş", totalCost: 1500 }),
      ),
    );
  });

  it("shows the by-source summary table when batches share a source name", async () => {
    vi.mocked(batchesApi.getAcquisitionBatches).mockResolvedValue([
      makeBatch({ id: "b1", totalCost: 1000 }),
      makeBatch({ id: "b2", totalCost: 2000 }),
    ]);
    renderPage(<AdminAcquisitionBatchesPage />, { route: "/admin/toplu-alimlar" });

    await waitFor(() => expect(screen.getByText("Kaynağa Göre Özet")).toBeInTheDocument());
  });
});
