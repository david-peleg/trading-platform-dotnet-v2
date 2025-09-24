IF OBJECT_ID(N'dbo.Prices', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Prices
    (
        Id        BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Prices PRIMARY KEY,
        Symbol    NVARCHAR(20) NOT NULL,
        Dt        DATE NOT NULL,
        [Open]    DECIMAL(18,4) NULL,
        High      DECIMAL(18,4) NULL,
        Low       DECIMAL(18,4) NULL,
        [Close]   DECIMAL(18,4) NULL,
        Volume    BIGINT NULL,
        Source    NVARCHAR(50) NULL,
        CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Prices_CreatedAt DEFAULT (SYSUTCDATETIME())
    );

    ALTER TABLE dbo.Prices
      ADD CONSTRAINT UQ_Prices_Symbol_Dt UNIQUE (Symbol, Dt);

    CREATE INDEX IX_Prices_Symbol_Dt ON dbo.Prices(Symbol, Dt DESC);
END
