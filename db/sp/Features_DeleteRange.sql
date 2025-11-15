IF OBJECT_ID(N'dbo.Features_DeleteRange', N'P') IS NOT NULL
    DROP PROCEDURE dbo.Features_DeleteRange;
GO

CREATE PROCEDURE dbo.Features_DeleteRange
(
    @Symbol NVARCHAR(20),
    @From   DATE,
    @To     DATE,
    @Prefix NVARCHAR(20) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE f
    FROM dbo.Features AS f
    WHERE   f.Symbol = @Symbol
        AND f.Dt     >= @From
        AND f.Dt     <= @To
        AND (@Prefix IS NULL OR f.FeatureName LIKE @Prefix + N'%');
END;
GO
