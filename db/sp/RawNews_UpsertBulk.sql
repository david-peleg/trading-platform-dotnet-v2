IF OBJECT_ID('dbo.RawNews_UpsertBulk','P') IS NOT NULL DROP PROCEDURE dbo.RawNews_UpsertBulk;
GO
CREATE PROCEDURE dbo.RawNews_UpsertBulk
  @Rows dbo.RawNewsType READONLY
AS
BEGIN
  SET NOCOUNT ON;
  MERGE dbo.RawNews AS T
  USING @Rows AS S
    ON T.BodyHash = S.BodyHash
  WHEN MATCHED THEN
    UPDATE SET
      T.Ticker = COALESCE(T.Ticker, S.Ticker),
      T.Lang   = COALESCE(T.Lang,   S.Lang)
  WHEN NOT MATCHED BY TARGET THEN
    INSERT(Ticker,Headline,Url,Source,PublishedAt,BodyHash,Lang)
    VALUES(S.Ticker,S.Headline,S.Url,S.Source,S.PublishedAt,S.BodyHash,S.Lang);
END
GO
