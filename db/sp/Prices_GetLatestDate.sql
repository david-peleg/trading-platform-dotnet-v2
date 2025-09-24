CREATE OR ALTER PROCEDURE dbo.Prices_GetLatestDate
    @Symbol NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT MAX(Dt) AS Dt
    FROM dbo.Prices
    WHERE Symbol = @Symbol;
END
