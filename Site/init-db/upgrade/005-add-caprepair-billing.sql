/*
  Idempotent DB upgrade: CapRepair billing (tariffs + receipts).
  Adds:
    - dbo.ServiceTariffs (rate per m2, effective months)
    - dbo.Receipts (per resident per billing month)
    - dbo.ReceiptLines (caprepair lines by object)
*/

USE [Cursovaya];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET ANSI_PADDING ON;
GO
SET ANSI_WARNINGS ON;
GO
SET CONCAT_NULL_YIELDS_NULL ON;
GO
SET ARITHABORT ON;
GO
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.ServiceTariffs', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.ServiceTariffs (
    ServiceTariffId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ServiceTariffs PRIMARY KEY,
    ServiceCode NVARCHAR(64) NOT NULL,
    RatePerSqm DECIMAL(18,2) NOT NULL,
    Currency NVARCHAR(8) NOT NULL CONSTRAINT DF_ServiceTariffs_Currency DEFAULT (N'RUB'),
    ActiveFromMonth DATE NOT NULL,
    ActiveToMonth DATE NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ServiceTariffs_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT CK_ServiceTariffs_RateNonNegative CHECK (RatePerSqm >= 0),
    CONSTRAINT CK_ServiceTariffs_ActiveRange CHECK (ActiveToMonth IS NULL OR ActiveToMonth >= ActiveFromMonth)
  );
END
GO

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = N'IX_ServiceTariffs_Code_From'
    AND object_id = OBJECT_ID(N'dbo.ServiceTariffs')
)
BEGIN
  CREATE INDEX IX_ServiceTariffs_Code_From
    ON dbo.ServiceTariffs(ServiceCode, ActiveFromMonth DESC);
END
GO

IF OBJECT_ID(N'dbo.Receipts', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.Receipts (
    ReceiptId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Receipts PRIMARY KEY,
    ResidentId INT NOT NULL,
    BillingMonth DATE NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL CONSTRAINT DF_Receipts_TotalAmount DEFAULT (0),
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Receipts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_Receipts_Residents
      FOREIGN KEY (ResidentId) REFERENCES dbo.Residents(ResidentId) ON DELETE CASCADE
  );
END
GO

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = N'UX_Receipts_Resident_Month'
    AND object_id = OBJECT_ID(N'dbo.Receipts')
)
BEGIN
  CREATE UNIQUE INDEX UX_Receipts_Resident_Month
    ON dbo.Receipts(ResidentId, BillingMonth);
END
GO

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = N'IX_Receipts_Resident_MonthDesc'
    AND object_id = OBJECT_ID(N'dbo.Receipts')
)
BEGIN
  CREATE INDEX IX_Receipts_Resident_MonthDesc
    ON dbo.Receipts(ResidentId, BillingMonth DESC);
END
GO

IF OBJECT_ID(N'dbo.ReceiptLines', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.ReceiptLines (
    ReceiptLineId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ReceiptLines PRIMARY KEY,
    ReceiptId INT NOT NULL,
    ServiceCode NVARCHAR(64) NOT NULL,
    ObjectType NVARCHAR(32) NOT NULL,
    ObjectId INT NOT NULL,
    AreaSqm DECIMAL(18,2) NOT NULL,
    RatePerSqm DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ReceiptLines_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT FK_ReceiptLines_Receipts
      FOREIGN KEY (ReceiptId) REFERENCES dbo.Receipts(ReceiptId) ON DELETE CASCADE,
    CONSTRAINT CK_ReceiptLines_AreaNonNegative CHECK (AreaSqm >= 0),
    CONSTRAINT CK_ReceiptLines_RateNonNegative CHECK (RatePerSqm >= 0),
    CONSTRAINT CK_ReceiptLines_AmountNonNegative CHECK (Amount >= 0)
  );
END
GO

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = N'IX_ReceiptLines_Receipt'
    AND object_id = OBJECT_ID(N'dbo.ReceiptLines')
)
BEGIN
  CREATE INDEX IX_ReceiptLines_Receipt
    ON dbo.ReceiptLines(ReceiptId);
END
GO

/* Seed default CapRepair tariff 33 RUB/m2 */
IF NOT EXISTS (
  SELECT 1
  FROM dbo.ServiceTariffs
  WHERE ServiceCode = N'CapRepair'
    AND RatePerSqm = 33.00
    AND ActiveToMonth IS NULL
)
BEGIN
  INSERT INTO dbo.ServiceTariffs (ServiceCode, RatePerSqm, Currency, ActiveFromMonth, ActiveToMonth)
  VALUES (N'CapRepair', 33.00, N'RUB', CONVERT(date, '2000-01-01'), NULL);
END
GO

