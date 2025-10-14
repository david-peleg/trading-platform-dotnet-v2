SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID(N'dbo.AnalystReports_UpsertBulk', N'P') IS NOT NULL
    DROP PROCEDURE dbo.AnalystReports_UpsertBulk;
GO
CREATE PROCEDURE dbo.AnalystReports_UpsertBulk
    @Rows dbo.AnalystReportType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.AnalystReports WITH (HOLDLOCK) AS T
    USING @Rows AS S
      ON T.ReportHash = S.ReportHash
    WHEN MATCHED THEN
        UPDATE SET
            T.Symbol      = S.Symbol,
            T.Firm        = S.Firm,
            T.Title       = S.Title,
            T.Action      = S.Action,
            T.RatingFrom  = S.RatingFrom,
            T.RatingTo    = S.RatingTo,
            T.TargetPrice = S.TargetPrice,
            T.Currency    = S.Currency,
            T.Url         = S.Url,
            T.Source      = S.Source,
            T.PublishedAt = S.PublishedAt,
            T.Lang        = S.Lang
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Symbol, Firm, Title, Action, RatingFrom, RatingTo, TargetPrice, Currency, Url, Source, PublishedAt, ReportHash, Lang)
        VALUES (S.Symbol, S.Firm, S.Title, S.Action, S.RatingFrom, S.RatingTo, S.TargetPrice, S.Currency, S.Url, S.Source, S.PublishedAt, S.ReportHash, S.Lang);
END
GO
