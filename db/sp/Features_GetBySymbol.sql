IF OBJECT_ID(N'dbo.Features_GetBySymbol', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Features_GetBySymbol;
GO

CREATE PROCEDURE dbo.Features_GetBySymbol
(
    @Symbol NVARCHAR(20),
    @From   DATE,
    @To     DATE,
    @Prefix NVARCHAR(20) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        f.Id,
        f.Symbol,
        f.Dt,
        f.FeatureName,
        f.Value,
        f.Window,
        f.CreatedAt
    FROM dbo.Features AS f
    WHERE   f.Symbol = @Symbol
        AND f.Dt     >= @From
        AND f.Dt     <= @To
        AND (@Prefix IS NULL OR f.FeatureName LIKE @Prefix + N'%')
    ORDER BY
        f.Dt,
        f.FeatureName,
        f.Window;
END;
GO
