IF OBJECT_ID('dbo.Attributions_GetBySymbol','P') IS NULL
  EXEC('CREATE PROCEDURE dbo.Attributions_GetBySymbol AS RETURN 0;');
GO

ALTER PROCEDURE dbo.Attributions_GetBySymbol
    @Symbol   NVARCHAR(20),
    @HorizonD SMALLINT = NULL,
    @Take     INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Take)
        Id, ItemType, ItemId, Symbol, Direction, Confidence, HorizonD, CreatedAt
    FROM dbo.Attributions WITH (READPAST)
    WHERE Symbol = @Symbol
      AND (@HorizonD IS NULL OR HorizonD = @HorizonD)
    ORDER BY CreatedAt DESC, Id DESC;
END
GO
