IF OBJECT_ID(N'dbo.Features_UpsertBulk', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Features_UpsertBulk;
GO

CREATE PROCEDURE dbo.Features_UpsertBulk
(
    @Rows dbo.FeatureType READONLY
)
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.Features AS target
    USING @Rows AS src
        ON  target.Symbol      = src.Symbol
        AND target.Dt          = src.Dt
        AND target.FeatureName = src.FeatureName
        AND (
                (target.Window IS NULL AND src.Window IS NULL)
             OR (target.Window = src.Window)
            )
    WHEN MATCHED THEN
        UPDATE SET
            Value = src.Value
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Symbol, Dt, FeatureName, Value, Window)
        VALUES (src.Symbol, src.Dt, src.FeatureName, src.Value, src.Window);
END;
GO
