CREATE OR ALTER PROCEDURE dbo.SP_Filings_UpsertBulk
  @Rows dbo.FilingRowType READONLY
AS
BEGIN
  SET NOCOUNT ON;

  DECLARE @ops TABLE([action] NVARCHAR(10));

  MERGE dbo.Filings AS T
  USING @Rows AS S
    ON T.DocHash = S.DocHash
  WHEN NOT MATCHED BY TARGET THEN
    INSERT (Symbol, FilingType, PeriodStart, PeriodEnd, Url, Source, PublishedAt, DocHash, Lang)
    VALUES (S.Symbol, S.FilingType, S.PeriodStart, S.PeriodEnd, S.Url, S.Source, S.PublishedAt, S.DocHash, S.Lang)
  OUTPUT $action INTO @ops;

  SELECT Inserted = COUNT(*) FROM @ops WHERE [action] = 'INSERT';
END
GO
