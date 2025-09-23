-- db/sp/Symbols_Search.sql
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO
CREATE OR ALTER PROCEDURE dbo.Symbols_Search
    @Query NVARCHAR(100),
    @Take  INT = 50
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @pattern NVARCHAR(210);
    -- Escape specials for LIKE: %, _, [, \  | use ESCAPE '\'
    SET @pattern = N'%' +
        REPLACE(REPLACE(REPLACE(REPLACE(@Query, N'\', N'\\'), N'%', N'\%'), N'_', N'\_'), N'[', N'\[')
        + N'%';

    SELECT TOP (@Take)
        Symbol, Name, Exchange, Country, Sector, IsActive
    FROM dbo.Symbols
    WHERE Symbol LIKE @pattern ESCAPE N'\' OR Name LIKE @pattern ESCAPE N'\'
    ORDER BY Symbol;
END;
GO
