-- db/sp/Symbols_GetAll.sql
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.Symbols_GetAll
    @Exchange NVARCHAR(50) = NULL,
    @Take     INT          = 100
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Take)
        Symbol, Name, Exchange, Country, Sector, IsActive
    FROM dbo.Symbols
    WHERE (@Exchange IS NULL OR Exchange = @Exchange)
    ORDER BY Symbol;
END;
GO
