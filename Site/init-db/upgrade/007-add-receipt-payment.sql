/*
  Статус оплаты квитанции (без реальной интеграции с банком).
*/
USE [Cursovaya];
GO

SET NOCOUNT ON;
GO

IF COL_LENGTH('dbo.Receipts', 'PaymentStatus') IS NULL
BEGIN
  ALTER TABLE dbo.Receipts ADD PaymentStatus NVARCHAR(32) NOT NULL
    CONSTRAINT DF_Receipts_PaymentStatus DEFAULT (N'Unpaid');
END
GO

IF COL_LENGTH('dbo.Receipts', 'PaymentDueDate') IS NULL
BEGIN
  ALTER TABLE dbo.Receipts ADD PaymentDueDate DATE NULL;
END
GO

IF COL_LENGTH('dbo.Receipts', 'PaidAt') IS NULL
BEGIN
  ALTER TABLE dbo.Receipts ADD PaidAt DATETIME2(0) NULL;
END
GO

IF COL_LENGTH('dbo.Receipts', 'PaymentReference') IS NULL
BEGIN
  ALTER TABLE dbo.Receipts ADD PaymentReference NVARCHAR(64) NULL;
END
GO

-- Существующие квитанции: срок оплаты — 25-е число месяца начисления
UPDATE dbo.Receipts
SET PaymentStatus = N'Unpaid',
    PaymentDueDate = DATEFROMPARTS(YEAR(BillingMonth), MONTH(BillingMonth), 25),
    PaymentReference = N'GQ-' + CAST(ReceiptId AS NVARCHAR(12)) + N'-' + FORMAT(BillingMonth, 'yyyyMM')
WHERE PaymentReference IS NULL;
GO
