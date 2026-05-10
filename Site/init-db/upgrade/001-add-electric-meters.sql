/*
  Idempotent DB upgrade script.
  Adds: ElectricMeters (1:1 with Apartment/StorageRoom/ParkingRoom), ElectricMeterReadings (monthly readings).
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

IF OBJECT_ID(N'dbo.ElectricMeters', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.ElectricMeters (
    ElectricMeterId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ElectricMeters PRIMARY KEY,
    SerialNumber NVARCHAR(64) NULL,

    ApartmentId INT NULL,
    StorageRoomId INT NULL,
    ParkingRoomId INT NULL,

    InstalledAt DATETIME2(0) NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ElectricMeters_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT CK_ElectricMeters_ExactlyOneObject CHECK (
      (CASE WHEN ApartmentId IS NULL THEN 0 ELSE 1 END) +
      (CASE WHEN StorageRoomId IS NULL THEN 0 ELSE 1 END) +
      (CASE WHEN ParkingRoomId IS NULL THEN 0 ELSE 1 END)
      = 1
    )
  );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ElectricMeters_ApartmentId' AND object_id = OBJECT_ID(N'dbo.ElectricMeters'))
BEGIN
  CREATE UNIQUE INDEX UX_ElectricMeters_ApartmentId ON dbo.ElectricMeters(ApartmentId) WHERE ApartmentId IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ElectricMeters_StorageRoomId' AND object_id = OBJECT_ID(N'dbo.ElectricMeters'))
BEGIN
  CREATE UNIQUE INDEX UX_ElectricMeters_StorageRoomId ON dbo.ElectricMeters(StorageRoomId) WHERE StorageRoomId IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_ElectricMeters_ParkingRoomId' AND object_id = OBJECT_ID(N'dbo.ElectricMeters'))
BEGIN
  CREATE UNIQUE INDEX UX_ElectricMeters_ParkingRoomId ON dbo.ElectricMeters(ParkingRoomId) WHERE ParkingRoomId IS NOT NULL;
END
GO

IF OBJECT_ID(N'dbo.ElectricMeterReadings', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.ElectricMeterReadings (
    ElectricMeterReadingId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ElectricMeterReadings PRIMARY KEY,
    ElectricMeterId INT NOT NULL,
    ReadingMonth DATE NOT NULL,
    ReadingValue DECIMAL(18,3) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_ElectricMeterReadings_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT FK_ElectricMeterReadings_ElectricMeters
      FOREIGN KEY (ElectricMeterId) REFERENCES dbo.ElectricMeters(ElectricMeterId) ON DELETE CASCADE,
    CONSTRAINT CK_ElectricMeterReadings_NonNegative CHECK (ReadingValue >= 0)
  );
END
GO

IF NOT EXISTS (
  SELECT 1
  FROM sys.indexes
  WHERE name = N'UX_ElectricMeterReadings_Meter_Month'
    AND object_id = OBJECT_ID(N'dbo.ElectricMeterReadings')
)
BEGIN
  CREATE UNIQUE INDEX UX_ElectricMeterReadings_Meter_Month
    ON dbo.ElectricMeterReadings(ElectricMeterId, ReadingMonth);
END
GO

IF NOT EXISTS (
  SELECT 1
  FROM sys.indexes
  WHERE name = N'IX_ElectricMeterReadings_Meter_MonthDesc'
    AND object_id = OBJECT_ID(N'dbo.ElectricMeterReadings')
)
BEGIN
  CREATE INDEX IX_ElectricMeterReadings_Meter_MonthDesc
    ON dbo.ElectricMeterReadings(ElectricMeterId, ReadingMonth DESC);
END
GO

/* FK constraints to existing object tables (create only if the tables exist) */
IF OBJECT_ID(N'dbo.Apartments', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ElectricMeters_Apartments')
BEGIN
  ALTER TABLE dbo.ElectricMeters WITH CHECK
  ADD CONSTRAINT FK_ElectricMeters_Apartments
    FOREIGN KEY (ApartmentId) REFERENCES dbo.Apartments(ApartmentId) ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.StorageRooms', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ElectricMeters_StorageRooms')
BEGIN
  ALTER TABLE dbo.ElectricMeters WITH CHECK
  ADD CONSTRAINT FK_ElectricMeters_StorageRooms
    FOREIGN KEY (StorageRoomId) REFERENCES dbo.StorageRooms(StorageRoomId) ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.ParkingRooms', N'U') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_ElectricMeters_ParkingRooms')
BEGIN
  ALTER TABLE dbo.ElectricMeters WITH CHECK
  ADD CONSTRAINT FK_ElectricMeters_ParkingRooms
    FOREIGN KEY (ParkingRoomId) REFERENCES dbo.ParkingRooms(ParkingRoomId) ON DELETE CASCADE;
END
GO

/* Backfill meters for existing objects (one meter per object) */
IF OBJECT_ID(N'dbo.Apartments', N'U') IS NOT NULL
BEGIN
  INSERT INTO dbo.ElectricMeters (ApartmentId)
  SELECT a.ApartmentId
  FROM dbo.Apartments a
  WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ElectricMeters m WHERE m.ApartmentId = a.ApartmentId
  );
END
GO

IF OBJECT_ID(N'dbo.StorageRooms', N'U') IS NOT NULL
BEGIN
  INSERT INTO dbo.ElectricMeters (StorageRoomId)
  SELECT s.StorageRoomId
  FROM dbo.StorageRooms s
  WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ElectricMeters m WHERE m.StorageRoomId = s.StorageRoomId
  );
END
GO

IF OBJECT_ID(N'dbo.ParkingRooms', N'U') IS NOT NULL
BEGIN
  INSERT INTO dbo.ElectricMeters (ParkingRoomId)
  SELECT p.ParkingRoomId
  FROM dbo.ParkingRooms p
  WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ElectricMeters m WHERE m.ParkingRoomId = p.ParkingRoomId
  );
END
GO

