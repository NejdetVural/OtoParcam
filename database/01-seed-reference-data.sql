-- OtoParcam — reference data seed (Categories, VehicleBrands, VehicleModels)
--
-- Prerequisite: migrations already applied (`dotnet ef database update`, see repo root
-- CLAUDE.md). This script does not create schema — EF Core migrations are the source of
-- truth for that (Code-First; see docs/Notes/ DECISION_LOG.md).
--
-- Idempotent: each row is guarded by its own unique-name check, so re-running this script
-- against a database that already has some/all of this data is safe and adds nothing extra.
--
-- Run with (Turkish characters require the UTF-8 codepage flag):
--   sqlcmd -S "(localdb)\mssqllocaldb" -d OtoParcamDb -i database\01-seed-reference-data.sql -f 65001

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();

-- ===================== Categories =====================

DECLARE @Categories TABLE (Name NVARCHAR(100));
INSERT INTO @Categories (Name) VALUES
    (N'Kaporta'), (N'Kapı'), (N'Tampon'), (N'Motor Parçaları'),
    (N'Aydınlatma'), (N'Aynalar'), (N'İç Aksam'), (N'Şanzıman'),
    (N'Süspansiyon'), (N'Elektrik/Elektronik');

INSERT INTO Categories (Id, Name, CreatedAt, UpdatedAt)
SELECT NEWID(), c.Name, @Now, @Now
FROM @Categories c
WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE Name = c.Name);

-- ===================== Vehicle Brands =====================

DECLARE @Brands TABLE (Name NVARCHAR(100));
INSERT INTO @Brands (Name) VALUES
    (N'Volkswagen'), (N'Renault'), (N'Fiat'), (N'Ford'), (N'BMW'), (N'Toyota');

INSERT INTO VehicleBrands (Id, Name, CreatedAt, UpdatedAt)
SELECT NEWID(), b.Name, @Now, @Now
FROM @Brands b
WHERE NOT EXISTS (SELECT 1 FROM VehicleBrands WHERE Name = b.Name);

-- ===================== Vehicle Models =====================
-- (BrandName, ModelName, StartYear, EndYear, Variant)

DECLARE @Models TABLE (BrandName NVARCHAR(100), ModelName NVARCHAR(100), StartYear SMALLINT, EndYear SMALLINT, Variant NVARCHAR(100));
INSERT INTO @Models (BrandName, ModelName, StartYear, EndYear, Variant) VALUES
    (N'Volkswagen', N'Golf 5',     2003, 2008, N'Golf 5'),
    (N'Volkswagen', N'Golf Plus',  2005, 2014, N'Golf Plus'),
    (N'Volkswagen', N'Golf 6',     2008, 2012, N'Golf 6'),
    (N'Volkswagen', N'Passat B6',  2005, 2010, N'B6'),
    (N'Renault',    N'Megane 3',   2008, 2016, N'Megane 3'),
    (N'Renault',    N'Clio 4',     2012, 2019, N'Clio 4'),
    (N'Fiat',       N'Doblo',      2010, 2022, NULL),
    (N'Fiat',       N'Egea',       2015, 2023, NULL),
    (N'Ford',       N'Focus 3',    2010, 2018, N'Focus Mk3'),
    (N'BMW',        N'3 Serisi',   2011, 2019, N'F30'),
    (N'Toyota',     N'Corolla',    2013, 2019, N'E170');

INSERT INTO VehicleModels (Id, VehicleBrandId, Name, StartYear, EndYear, Variant, CreatedAt, UpdatedAt)
SELECT NEWID(), vb.Id, m.ModelName, m.StartYear, m.EndYear, m.Variant, @Now, @Now
FROM @Models m
JOIN VehicleBrands vb ON vb.Name = m.BrandName
WHERE NOT EXISTS (
    SELECT 1 FROM VehicleModels vm
    WHERE vm.VehicleBrandId = vb.Id AND vm.Name = m.ModelName
);

PRINT 'Reference data seed complete.';
