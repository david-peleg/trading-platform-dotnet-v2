IF OBJECT_ID('dbo.RawNews_GetLatestDate','P') IS NOT NULL DROP PROCEDURE dbo.RawNews_GetLatestDate;
GO
CREATE PROCEDURE dbo.RawNews_GetLatestDate
  @Source NVARCHAR(100) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  SELECT MAX(PublishedAt) AS LatestPublishedAt
  FROM dbo.RawNews
  WHERE (@Source IS NULL OR Source = @Source);
END
GO
