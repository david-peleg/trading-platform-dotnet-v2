SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID(N'dbo.AnalystReports_GetBySymbol', N'P') IS NOT NULL
    DROP PROCEDURE dbo.AnalystReports_GetBySymbol;
GO
CREATE PROCEDURE dbo.AnalystReports_GetBySymbol
    @Symbol NVARCHAR(20),
    @Take   INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Take)
        Id, Symbol, Firm, Title, Action, RatingFrom, RatingTo, TargetPrice, Currency,
        Url, Source, PublishedAt, ReportHash, Lang, CreatedAt
    FROM dbo.AnalystReports
    WHERE Symbol = @Symbol
    ORDER BY PublishedAt DESC, Id DESC;
END
GO
