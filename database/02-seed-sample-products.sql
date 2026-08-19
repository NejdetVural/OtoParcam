-- OtoParcam — sample product catalog seed (AcquisitionBatch, Products, ProductImages,
-- ProductCompatibility)
--
-- Prerequisite: 01-seed-reference-data.sql already run against this database (this script
-- looks up Category/VehicleBrand/VehicleModel rows by name rather than duplicating them).
--
-- Idempotent: the whole script is a no-op if the marker AcquisitionBatch row already exists,
-- so it's safe to run more than once. It is NOT safe to run against a database that already
-- has unrelated real product data with the same category/vehicle names if you don't want the
-- extra sample rows — this is meant for a fresh local/dev database.
--
-- Image URLs point at placehold.co (external placeholder host), matching the pattern already
-- used for seed data elsewhere in this project — see frontend-skeleton-scaffold notes on
-- resolveImageUrl() passing absolute URLs through unchanged.
--
-- Run with (Turkish characters require the UTF-8 codepage flag):
--   sqlcmd -S "(localdb)\mssqllocaldb" -d OtoParcamDb -i database\02-seed-sample-products.sql -f 65001

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @BatchSource NVARCHAR(500) = N'Sigorta Hasarlı Volkswagen Golf 6 (2026)';

IF EXISTS (SELECT 1 FROM AcquisitionBatches WHERE Source = @BatchSource)
BEGIN
    PRINT 'Sample product seed already applied — skipping.';
    RETURN;
END

IF NOT EXISTS (
    SELECT 1 FROM VehicleModels vm JOIN VehicleBrands vb ON vb.Id = vm.VehicleBrandId
    WHERE vb.Name = N'Volkswagen' AND vm.Name = N'Golf 6'
)
BEGIN
    RAISERROR(N'Reference data not found — run 01-seed-reference-data.sql first.', 16, 1);
    RETURN;
END

-- ===================== Acquisition batch =====================
-- One lump-sum insurance-total-loss purchase; three Golf 6 products below are linked to it
-- and split its TotalCost evenly (see EffectiveAcquisitionCost / DECISION-007).

DECLARE @BatchId UNIQUEIDENTIFIER = NEWID();

INSERT INTO AcquisitionBatches (Id, Source, TotalCost, PurchaseDate, Notes, CreatedAt, UpdatedAt)
VALUES (
    @BatchId, @BatchSource, 45000.00, '2026-06-15',
    N'Sigorta eksperi hasar tespiti sonrası pert araç olarak satın alındı; kaporta ve iç aksam parçalanarak stoklandı.',
    @Now, @Now
);

-- ===================== Products =====================
-- Color: Black=1 White=2 Gray=3 Silver=4 Red=5 Blue=6 Green=7 Yellow=8 Orange=9 Brown=10
--        Beige=11 Gold=12 Bronze=13 Purple=14 Pink=15 Other=16
-- Status: Available=1 Sold=2 Hidden=3
-- Side: Left=1 Right=2 | Position: Front=1 Rear=2

DECLARE @Products TABLE (
    ProductKey NVARCHAR(10) PRIMARY KEY,
    Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    CategoryName NVARCHAR(100) NOT NULL,
    BrandName NVARCHAR(100) NOT NULL,
    ModelName NVARCHAR(100) NOT NULL,
    Price DECIMAL(10,2) NULL,
    SoldPrice DECIMAL(10,2) NULL,
    SoldAt DATETIME2 NULL,
    AcquisitionCost DECIMAL(10,2) NULL,
    AcquisitionSource NVARCHAR(500) NULL,
    InAcquisitionBatch BIT NOT NULL DEFAULT 0,
    Color INT NOT NULL,
    Status INT NOT NULL,
    Side INT NULL,
    Position INT NULL,
    Description NVARCHAR(2000) NOT NULL
);

INSERT INTO @Products
    (ProductKey, CategoryName, BrandName, ModelName, Price, SoldPrice, SoldAt, AcquisitionCost, AcquisitionSource, InAcquisitionBatch, Color, Status, Side, Position, Description)
VALUES
    (N'P1', N'Kaporta', N'Volkswagen', N'Golf 5', 650.00, NULL, NULL, NULL, NULL, 0,
        2, 1, 1, 1, N'Orijinal sol ön çamurluk, küçük bir çizik dışında hasarsız. Boyasız.'),
    (N'P2', N'Kapı', N'Volkswagen', N'Golf 6', 1450.00, NULL, NULL, NULL, NULL, 1,
        1, 1, 2, 1, N'Sağ ön kapı, iç trim ve cam mekanizmasıyla birlikte, tam çalışır durumda.'),
    (N'P3', N'Tampon', N'Renault', N'Megane 3', 900.00, NULL, NULL, 500.00, N'Yedek parçacı - Mersin', 0,
        3, 1, NULL, 2, N'Arka tampon, alt kısımda hafif bir çatlak mevcut, sensör delikleri boş.'),
    (N'P4', N'Aydınlatma', N'Ford', N'Focus 3', 300.00, NULL, NULL, NULL, NULL, 0,
        16, 1, 1, 2, N'Sol arka stop lambası, LED, kırık yok, bağlantı soketi sağlam.'),
    (N'P5', N'Motor Parçaları', N'BMW', N'3 Serisi', NULL, NULL, NULL, NULL, NULL, 0,
        16, 1, NULL, NULL, N'Turbo motor, hafif yağ sızıntısı mevcut, çalışır vaziyette test edilmiştir. Fiyat için arayınız.'),
    (N'P6', N'Aynalar', N'Toyota', N'Corolla', 350.00, 320.00, '2026-07-20', NULL, NULL, 0,
        4, 2, 2, NULL, N'Sağ dikiz aynası, elektrikli katlanır, çizik yok.'),
    (N'P7', N'İç Aksam', N'Fiat', N'Egea', 700.00, NULL, NULL, NULL, NULL, 0,
        1, 1, 1, 1, N'Sol ön koltuk, kumaş döşeme, yırtık yok, mekanizması sorunsuz çalışıyor.'),
    (N'P8', N'Şanzıman', N'Fiat', N'Doblo', 4500.00, NULL, NULL, 3000.00, N'Hurda araçtan sökme', 0,
        16, 3, NULL, NULL, N'Manuel şanzıman, 120.000 km, sökülmeden önce vites geçişleri düzgündü.'),
    (N'P9', N'Süspansiyon', N'Renault', N'Clio 4', 550.00, NULL, NULL, NULL, NULL, 0,
        16, 1, NULL, 1, N'Ön amortisör takımı (sağ+sol), yaylarıyla birlikte, sızıntı yok.'),
    (N'P10', N'Aydınlatma', N'Volkswagen', N'Golf 6', 480.00, NULL, NULL, NULL, NULL, 1,
        16, 1, 1, 1, N'Sol ön far, xenon, çizik ve buğulanma yok.'),
    (N'P11', N'Aydınlatma', N'Volkswagen', N'Golf 6', 480.00, NULL, NULL, NULL, NULL, 1,
        16, 1, 2, 1, N'Sağ ön far, xenon, çizik ve buğulanma yok.');

INSERT INTO Products
    (Id, CategoryId, SourceVehicleModelId, Price, SoldPrice, SoldAt, AcquisitionCost, AcquisitionSource, AcquisitionBatchId, Color, Status, Side, Position, Description, CreatedAt, UpdatedAt)
SELECT
    p.Id, c.Id, vm.Id, p.Price, p.SoldPrice, p.SoldAt, p.AcquisitionCost, p.AcquisitionSource,
    CASE WHEN p.InAcquisitionBatch = 1 THEN @BatchId ELSE NULL END,
    p.Color, p.Status, p.Side, p.Position, p.Description, @Now, @Now
FROM @Products p
JOIN Categories c ON c.Name = p.CategoryName
JOIN VehicleBrands vb ON vb.Name = p.BrandName
JOIN VehicleModels vm ON vm.VehicleBrandId = vb.Id AND vm.Name = p.ModelName;

-- ===================== Product images =====================
-- DisplayOrder starts at 1 (matches ProductService.AddProductImageAsync's numbering).

DECLARE @Images TABLE (ProductKey NVARCHAR(10), DisplayOrder SMALLINT, ImageUrl NVARCHAR(500));
INSERT INTO @Images (ProductKey, DisplayOrder, ImageUrl) VALUES
    (N'P1', 1, N'https://placehold.co/800x600?text=Sol+On+Camurluk'),
    (N'P1', 2, N'https://placehold.co/800x600?text=Camurluk+Detay'),
    (N'P2', 1, N'https://placehold.co/800x600?text=Sag+On+Kapi'),
    (N'P2', 2, N'https://placehold.co/800x600?text=Kapi+Ic+Trim'),
    (N'P3', 1, N'https://placehold.co/800x600?text=Arka+Tampon'),
    (N'P4', 1, N'https://placehold.co/800x600?text=Sol+Arka+Stop'),
    (N'P5', 1, N'https://placehold.co/800x600?text=Turbo+Motor'),
    (N'P6', 1, N'https://placehold.co/800x600?text=Sag+Dikiz+Aynasi'),
    (N'P7', 1, N'https://placehold.co/800x600?text=On+Koltuk'),
    (N'P8', 1, N'https://placehold.co/800x600?text=Manuel+Sanziman'),
    (N'P9', 1, N'https://placehold.co/800x600?text=On+Amortisor'),
    (N'P10', 1, N'https://placehold.co/800x600?text=Sol+On+Far'),
    (N'P11', 1, N'https://placehold.co/800x600?text=Sag+On+Far');

INSERT INTO ProductImages (Id, ProductId, ImageUrl, DisplayOrder, CreatedAt, UpdatedAt)
SELECT NEWID(), p.Id, i.ImageUrl, i.DisplayOrder, @Now, @Now
FROM @Images i
JOIN @Products p ON p.ProductKey = i.ProductKey;

-- ===================== Product compatibility =====================
-- P1 (sourced from a Golf 5) also fits the Golf Plus, which shares the Golf 5/PQ35 platform.

INSERT INTO ProductCompatibilities (Id, ProductId, VehicleModelId, CreatedAt, UpdatedAt)
SELECT NEWID(), p.Id, vm.Id, @Now, @Now
FROM @Products p
JOIN VehicleBrands vb ON vb.Name = N'Volkswagen'
JOIN VehicleModels vm ON vm.VehicleBrandId = vb.Id AND vm.Name = N'Golf Plus'
WHERE p.ProductKey = N'P1';

PRINT 'Sample product catalog seed complete.';
