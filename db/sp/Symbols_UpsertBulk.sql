-- db/sp/Symbols_UpsertBulk.sql
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.Symbols_UpsertBulk
    @Rows dbo.SymbolType READONLY
AS
BEGIN
    SET NOCOUNT ON;

    MERGE dbo.Symbols AS T
    USING @Rows AS S
      ON T.Symbol = S.Symbol
    WHEN MATCHED THEN
        UPDATE SET
            T.Name      = S.Name,
            T.Exchange  = S.Exchange,
            T.Country   = S.Country,
            T.Sector    = S.Sector,
            T.IsActive  = ISNULL(S.IsActive, 1),
            T.UpdatedAt = SYSUTCDATETIME()
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (Symbol, Name, Exchange, Country, Sector, IsActive)
        VALUES (S.Symbol, S.Name, S.Exchange, S.Country, S.Sector, ISNULL(S.IsActive, 1));
END;
GO
