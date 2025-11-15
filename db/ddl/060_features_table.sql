CREATE TABLE dbo.Features(
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    Symbol NVARCHAR(20) NOT NULL,
    Dt DATE NOT NULL,
    FeatureName NVARCHAR(80) NOT NULL,   -- MA_20, MA_50, RSI_14, VOL_20, SENT_D1
    Value FLOAT NOT NULL,
    Window SMALLINT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX UX_Features_Key 
    ON dbo.Features(Symbol, Dt, FeatureName, Window);
GO

CREATE INDEX IX_Features_Symbol_Dt 
    ON dbo.Features(Symbol, Dt DESC);
GO
