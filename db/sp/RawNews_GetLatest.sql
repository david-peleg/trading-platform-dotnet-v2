IF OBJECT_ID('dbo.RawNews_GetLatest','P') IS NOT NULL DROP PROCEDURE dbo.RawNews_GetLatest;
GO
CREATE PROCEDURE dbo.RawNews_GetLatest
  @Symbol NVARCHAR(20) = NULL,
  @Take   INT = 50
AS
BEGIN
  SET NOCOUNT ON;
  SELECT TOP (@Take)
    Id,Ticker,Headline,Url,Source,PublishedAt,BodyHash,Lang,CreatedAt
  FROM dbo.RawNews
  WHERE (@Symbol IS NULL OR Ticker = @Symbol)
  ORDER BY PublishedAt DESC, Id DESC;
END
GO
