CREATE OR ALTER PROCEDURE dbo.Prices_UpsertDaily
    @Rows dbo.PriceDailyType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.Prices AS T
    USING @Rows AS S
      ON (T.Symbol = S.Symbol AND T.Dt = S.Dt)
    WHEN MATCHED THEN
        UPDATE SET
            T.[Open]  = S.[Open],
            T.High    = S.High,
            T.Low     = S.Low,
            T.[Close] = S.[Close],
            T.Volume  = S.Volume,
            T.Source  = S.Source
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Symbol, Dt, [Open], High, Low, [Close], Volume, Source)
        VALUES (S.Symbol, S.Dt, S.[Open], S.High, S.Low, S.[Close], S.Volume, S.Source);
END
