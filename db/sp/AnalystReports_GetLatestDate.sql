SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID(N'dbo.AnalystReports_GetLatestDate', N'P') IS NOT NULL
    DROP PROCEDURE dbo.AnalystReports_GetLatestDate;
GO
CREATE PROCEDURE dbo.AnalystReports_GetLatestDate
    @Symbol NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Symbol IS NULL
        SELECT MAX(PublishedAt) AS LatestDate FROM dbo.AnalystReports;
    ELSE
        SELECT MAX(PublishedAt) AS LatestDate FROM dbo.AnalystReports WHERE Symbol = @Symbol;
END
GO
