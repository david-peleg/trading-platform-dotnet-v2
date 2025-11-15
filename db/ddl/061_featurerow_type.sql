IF TYPE_ID(N'dbo.FeatureType') IS NOT NULL
    DROP TYPE dbo.FeatureType;
GO

CREATE TYPE dbo.FeatureType AS TABLE(
    Symbol      NVARCHAR(20)  NOT NULL,
    Dt          DATE          NOT NULL,
    FeatureName NVARCHAR(80)  NOT NULL,
    Value       FLOAT         NOT NULL,
    Window      SMALLINT      NULL
);
GO
