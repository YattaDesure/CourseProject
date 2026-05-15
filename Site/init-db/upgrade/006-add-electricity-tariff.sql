/*
  Seed tariff for electricity (kWh). RatePerSqm column stores RUB per 1 kWh.
*/
USE [Cursovaya];
GO

SET NOCOUNT ON;
GO

IF NOT EXISTS (
  SELECT 1 FROM dbo.ServiceTariffs
  WHERE ServiceCode = N'Electricity'
)
BEGIN
  INSERT INTO dbo.ServiceTariffs (ServiceCode, RatePerSqm, Currency, ActiveFromMonth, ActiveToMonth)
  VALUES (N'Electricity', 5.50, N'RUB', CONVERT(date, '2000-01-01'), NULL);
END
GO
