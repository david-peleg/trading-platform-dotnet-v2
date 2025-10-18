IF OBJECT_ID('dbo.Attributions_InsertBulk','P') IS NULL
  EXEC('CREATE PROCEDURE dbo.Attributions_InsertBulk AS RETURN 0;');
GO

ALTER PROCEDURE dbo.Attributions_InsertBulk
    @Rows dbo.AttributionType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.Attributions AS T
    USING (SELECT ItemType, ItemId, Symbol, Direction, Confidence, HorizonD FROM @Rows) AS S
      ON  T.ItemType = S.ItemType
      AND T.ItemId   = S.ItemId
      AND T.Symbol   = S.Symbol
      AND T.HorizonD = S.HorizonD
    WHEN MATCHED THEN
        UPDATE SET
          Direction  = S.Direction,
          Confidence = S.Confidence
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (ItemType, ItemId, Symbol, Direction, Confidence, HorizonD)
        VALUES (S.ItemType, S.ItemId, S.Symbol, S.Direction, S.Confidence, S.HorizonD)
    OUTPUT $action AS MergeAction;
END
GO