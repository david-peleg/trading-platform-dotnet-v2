IF TYPE_ID(N'dbo.PriceDailyType') IS NULL
    CREATE TYPE dbo.PriceDailyType AS TABLE
    (
        Symbol NVARCHAR(20) NOT NULL,
        Dt     DATE NOT NULL,
        [Open] DECIMAL(18,4) NULL,
        High   DECIMAL(18,4) NULL,
        Low    DECIMAL(18,4) NULL,
        [Close] DECIMAL(18,4) NULL,
        Volume BIGINT NULL,
        Source NVARCHAR(50) NULL
    );
