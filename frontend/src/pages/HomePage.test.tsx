import { beforeEach, describe, expect, it, vi } from "vitest";
import { screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { renderPage } from "../testUtils";
import { HomePage } from "./HomePage";
import { ProductStatus } from "../api/types";
import type { PagedResult } from "../api/types";
import type { ProductDto } from "../api/products";

vi.mock("../api/categories", () => ({ getCategories: vi.fn() }));
vi.mock("../api/vehicleBrands", () => ({ getVehicleBrands: vi.fn() }));
vi.mock("../api/vehicleModels", () => ({ getVehicleModels: vi.fn() }));
vi.mock("../api/products", () => ({ getProducts: vi.fn() }));

import { getCategories } from "../api/categories";
import { getVehicleBrands } from "../api/vehicleBrands";
import { getVehicleModels } from "../api/vehicleModels";
import { getProducts } from "../api/products";

function makeProduct(overrides: Partial<ProductDto> = {}): ProductDto {
  return {
    id: "p1",
    title: "Volkswagen Golf 6 (2008-2012)",
    categoryId: "c1",
    categoryName: "Kapı",
    sourceVehicleModelId: "m1",
    vehicleBrandId: "b1",
    vehicleBrandName: "Volkswagen",
    vehicleModelName: "Golf 6",
    startYear: 2008,
    endYear: 2012,
    variant: null,
    price: 1000,
    soldPrice: null,
    acquisitionCost: null,
    acquisitionSource: null,
    acquisitionBatchId: null,
    acquisitionBatchSource: null,
    effectiveAcquisitionCost: null,
    effectiveAcquisitionSource: null,
    color: 1,
    status: ProductStatus.Available,
    side: null,
    position: null,
    description: null,
    images: [],
    ...overrides,
  };
}

function pagedResult(items: ProductDto[]): PagedResult<ProductDto> {
  return { items, page: 1, pageSize: 20, totalCount: items.length, totalPages: 1 };
}

describe("HomePage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(getCategories).mockResolvedValue([
      { id: "c1", name: "Kapı" },
      { id: "c2", name: "Kaporta" },
    ]);
    vi.mocked(getVehicleBrands).mockResolvedValue([{ id: "b1", name: "Volkswagen" }]);
    vi.mocked(getVehicleModels).mockResolvedValue([
      { id: "m1", vehicleBrandId: "b1", name: "Golf 6", startYear: 2008, endYear: 2012, variant: null },
    ]);
  });

  it("renders fetched categories and brands as dropdowns, and products as cards", async () => {
    vi.mocked(getProducts).mockResolvedValue(pagedResult([makeProduct()]));
    renderPage(<HomePage />, { route: "/" });

    await waitFor(() => expect(screen.getByText("Volkswagen Golf 6 (2008-2012)")).toBeInTheDocument());

    const categorySelect = screen.getByDisplayValue("Tüm kategoriler").closest("select")!;
    expect(categorySelect).toHaveTextContent("Kapı");
    expect(categorySelect).toHaveTextContent("Kaporta");

    const brandSelect = screen.getByDisplayValue("Tüm markalar").closest("select")!;
    expect(brandSelect).toHaveTextContent("Volkswagen");
  });

  it("refetches with the chosen category when a category is selected", async () => {
    vi.mocked(getProducts).mockResolvedValue(pagedResult([makeProduct()]));
    const user = userEvent.setup();
    renderPage(<HomePage />, { route: "/" });

    await waitFor(() => expect(getProducts).toHaveBeenCalled());

    const categorySelect = screen.getByDisplayValue("Tüm kategoriler");
    await waitFor(() => expect(screen.getByRole("option", { name: "Kapı" })).toBeInTheDocument());
    await user.selectOptions(categorySelect, "Kapı");

    await waitFor(() =>
      expect(getProducts).toHaveBeenCalledWith(expect.objectContaining({ categoryId: "c1" })),
    );
  });

  it("shows an empty state when there are no matching products", async () => {
    vi.mocked(getProducts).mockResolvedValue(pagedResult([]));
    renderPage(<HomePage />, { route: "/" });

    expect(await screen.findByText("Bu kriterlere uygun ürün bulunamadı.")).toBeInTheDocument();
  });

  it("toggles the mobile filters panel open and closed", async () => {
    vi.mocked(getProducts).mockResolvedValue(pagedResult([makeProduct()]));
    const user = userEvent.setup();
    renderPage(<HomePage />, { route: "/" });

    await waitFor(() => expect(getProducts).toHaveBeenCalled());

    const toggle = screen.getByRole("button", { name: "Filtreler" });
    expect(toggle).toHaveAttribute("aria-expanded", "false");

    await user.click(toggle);
    expect(toggle).toHaveAttribute("aria-expanded", "true");

    await user.click(toggle);
    expect(toggle).toHaveAttribute("aria-expanded", "false");
  });
});
