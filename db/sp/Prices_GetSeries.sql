CREATE OR ALTER PROCEDURE dbo.Prices_GetSeries
    @Symbol NVARCHAR(20),
    @From   DATE,
    @To     DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        Symbol, Dt, [Open], High, Low, [Close], Volume, Source
    FROM dbo.Prices
    WHERE Symbol = @Symbol
      AND Dt BETWEEN @From AND @To
    ORDER BY Dt ASC;
END
