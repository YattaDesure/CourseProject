/*
  Seed initial "previous" readings for ALL electric meters.
  - Inserts a reading for previous month for each meter if not already present.
  - Values are deterministic (based on meterId) and non-negative.
*/

USE [Cursovaya];
GO

SET NOCOUNT ON;
GO

DECLARE @PrevMonth DATE = DATEFROMPARTS(YEAR(DATEADD(MONTH, -1, GETUTCDATE())), MONTH(DATEADD(MONTH, -1, GETUTCDATE())), 1);

INSERT INTO dbo.ElectricMeterReadings (ElectricMeterId, ReadingMonth, ReadingValue)
SELECT
  m.ElectricMeterId,
  @PrevMonth,
  CAST((1000 + (m.ElectricMeterId * 37 % 9000)) AS DECIMAL(18,3)) AS ReadingValue
FROM dbo.ElectricMeters m
WHERE NOT EXISTS (
  SELECT 1
  FROM dbo.ElectricMeterReadings r
  WHERE r.ElectricMeterId = m.ElectricMeterId
    AND r.ReadingMonth = @PrevMonth
);

SELECT
  @PrevMonth AS SeedMonth,
  COUNT(*) AS InsertedCount
FROM dbo.ElectricMeters m
WHERE EXISTS (
  SELECT 1
  FROM dbo.ElectricMeterReadings r
  WHERE r.ElectricMeterId = m.ElectricMeterId
    AND r.ReadingMonth = @PrevMonth
);

