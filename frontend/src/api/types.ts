export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// Mirrors backend/src/OtoParcam.Domain/Enums/ProductColor.cs.
// No JsonStringEnumConverter is registered in the API, so these serialize as numbers.
export enum ProductColor {
  Black = 1,
  White = 2,
  Gray = 3,
  Silver = 4,
  Red = 5,
  Blue = 6,
  Green = 7,
  Yellow = 8,
  Orange = 9,
  Brown = 10,
  Beige = 11,
  Gold = 12,
  Bronze = 13,
  Purple = 14,
  Pink = 15,
  Other = 16,
}

export const productColorLabels: Record<ProductColor, string> = {
  [ProductColor.Black]: "Siyah",
  [ProductColor.White]: "Beyaz",
  [ProductColor.Gray]: "Gri",
  [ProductColor.Silver]: "Gümüş",
  [ProductColor.Red]: "Kırmızı",
  [ProductColor.Blue]: "Mavi",
  [ProductColor.Green]: "Yeşil",
  [ProductColor.Yellow]: "Sarı",
  [ProductColor.Orange]: "Turuncu",
  [ProductColor.Brown]: "Kahverengi",
  [ProductColor.Beige]: "Bej",
  [ProductColor.Gold]: "Altın",
  [ProductColor.Bronze]: "Bronz",
  [ProductColor.Purple]: "Mor",
  [ProductColor.Pink]: "Pembe",
  [ProductColor.Other]: "Diğer",
};

// Mirrors backend/src/OtoParcam.Domain/Enums/ProductStatus.cs.
export enum ProductStatus {
  Available = 1,
  Sold = 2,
  Hidden = 3,
}

export const productStatusLabels: Record<ProductStatus, string> = {
  [ProductStatus.Available]: "Satışta",
  [ProductStatus.Sold]: "Satıldı",
  [ProductStatus.Hidden]: "Gizli",
};

// Mirrors backend/src/OtoParcam.Domain/Enums/ProductSide.cs. Nullable — not every category has a side (e.g. Hood).
export enum ProductSide {
  Left = 1,
  Right = 2,
}

export const productSideLabels: Record<ProductSide, string> = {
  [ProductSide.Left]: "Sol",
  [ProductSide.Right]: "Sağ",
};

// Mirrors backend/src/OtoParcam.Domain/Enums/ProductPosition.cs. Nullable — not every category has a position (e.g. Hood).
export enum ProductPosition {
  Front = 1,
  Rear = 2,
}

export const productPositionLabels: Record<ProductPosition, string> = {
  [ProductPosition.Front]: "Ön",
  [ProductPosition.Rear]: "Arka",
};
