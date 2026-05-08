/*
  Idempotent DB upgrade: News posts for dashboard.
  Table: dbo.NewsPosts
  Seed: 2 initial posts (if not already inserted).
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

IF OBJECT_ID(N'dbo.NewsPosts', N'U') IS NULL
BEGIN
  CREATE TABLE dbo.NewsPosts (
    NewsPostId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_NewsPosts PRIMARY KEY,
    Title NVARCHAR(200) NULL,
    Body NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_NewsPosts_CreatedAt DEFAULT (SYSUTCDATETIME()),
    IsPinned BIT NOT NULL CONSTRAINT DF_NewsPosts_IsPinned DEFAULT (0),
    IsPublished BIT NOT NULL CONSTRAINT DF_NewsPosts_IsPublished DEFAULT (1)
  );
END
GO

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes
  WHERE name = N'IX_NewsPosts_Published_CreatedAt'
    AND object_id = OBJECT_ID(N'dbo.NewsPosts')
)
BEGIN
  CREATE INDEX IX_NewsPosts_Published_CreatedAt
  ON dbo.NewsPosts(IsPublished, IsPinned, CreatedAt DESC);
END
GO

DECLARE @Seed1 NVARCHAR(MAX) = N'Доброе утро. Подпор на сетях канализации под 1 подъездом. «Слонов» вызвала, будут в течение часа. Проверьте кладовые 84,85,86.';
DECLARE @Seed2 NVARCHAR(MAX) = N'В связи с требованиями действующего жилищного законодательства и техническими ограничениями системы ГИС ЖКХ (которая не предусматривает проведение смешанных общих собраний), нам необходимо разделить вопросы по компетенции. В этой связи будут проведены два собрания:
1. Общее собрание собственников помещений
2. Общее собрание членов ТСН

Форма проведения: очно-заочная. Очная часть – формальная (для соблюдения процедуры).

Всем участникам будут выданы два разных бланка решений (по каждому собранию отдельно) после 16 мая.

Дополнительная информация (будет предоставлена позже в чат):
· отчёт о деятельности ТСН;
· смета доходов и расходов;
· ревизионный отчёт;
· документы по вопросам капитального ремонта.';

IF NOT EXISTS (SELECT 1 FROM dbo.NewsPosts WHERE Body = @Seed1)
BEGIN
  INSERT INTO dbo.NewsPosts (Title, Body, IsPinned, IsPublished)
  VALUES (N'Доброе утро', @Seed1, 1, 1);
END
IF NOT EXISTS (SELECT 1 FROM dbo.NewsPosts WHERE Body = @Seed2)
BEGIN
  INSERT INTO dbo.NewsPosts (Title, Body, IsPinned, IsPublished)
  VALUES (N'Общие собрания (май)', @Seed2, 0, 1);
END
GO

