IF OBJECT_ID('dbo.Attributions_GetForItem','P') IS NULL
  EXEC('CREATE PROCEDURE dbo.Attributions_GetForItem AS RETURN 0;');
GO

ALTER PROCEDURE dbo.Attributions_GetForItem
    @ItemType TINYINT,
    @ItemId   BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, ItemType, ItemId, Symbol, Direction, Confidence, HorizonD, CreatedAt
    FROM dbo.Attributions WITH (READPAST)
    WHERE ItemType = @ItemType AND ItemId = @ItemId
    ORDER BY CreatedAt DESC, Id DESC;
END
GO
