SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
IF TYPE_ID(N'dbo.AnalystReportType') IS NOT NULL
    DROP TYPE dbo.AnalystReportType;
GO
CREATE TYPE dbo.AnalystReportType AS TABLE
(
    Symbol       NVARCHAR(20)  NOT NULL,
    Firm         NVARCHAR(100) NOT NULL,
    Title        NVARCHAR(300) NULL,
    Action       NVARCHAR(20)  NULL,
    RatingFrom   NVARCHAR(40)  NULL,
    RatingTo     NVARCHAR(40)  NULL,
    TargetPrice  DECIMAL(18,4) NULL,
    Currency     NVARCHAR(10)  NULL,
    Url          NVARCHAR(400) NOT NULL,
    Source       NVARCHAR(100) NOT NULL,
    PublishedAt  DATETIME2      NOT NULL,
    ReportHash   BINARY(32)     NOT NULL,
    Lang         NVARCHAR(10)   NULL
);
GO
