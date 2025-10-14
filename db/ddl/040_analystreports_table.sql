SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'dbo.AnalystReports', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AnalystReports
    (
        Id            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Symbol        NVARCHAR(20)  NOT NULL,
        Firm          NVARCHAR(100) NOT NULL,
        Title         NVARCHAR(300) NULL,
        Action        NVARCHAR(20)  NULL,
        RatingFrom    NVARCHAR(40)  NULL,
        RatingTo      NVARCHAR(40)  NULL,
        TargetPrice   DECIMAL(18,4) NULL,
        Currency      NVARCHAR(10)  NULL,
        Url           NVARCHAR(400) NOT NULL,
        Source        NVARCHAR(100) NOT NULL,
        PublishedAt   DATETIME2      NOT NULL,
        ReportHash    BINARY(32)     NOT NULL,
        Lang          NVARCHAR(10)   NULL,
        CreatedAt     DATETIME2       NOT NULL CONSTRAINT DF_AnalystReports_CreatedAt DEFAULT SYSUTCDATETIME()
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_AnalystReports_ReportHash' AND object_id = OBJECT_ID('dbo.AnalystReports'))
    CREATE UNIQUE INDEX UX_AnalystReports_ReportHash ON dbo.AnalystReports(ReportHash);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AnalystReports_Symbol_PublishedAt' AND object_id = OBJECT_ID('dbo.AnalystReports'))
    CREATE INDEX IX_AnalystReports_Symbol_PublishedAt ON dbo.AnalystReports(Symbol, PublishedAt DESC);
GO
