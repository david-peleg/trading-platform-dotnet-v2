-- db/ddl/001_symbols_table.sql
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.Symbols', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Symbols
    (
        Symbol     NVARCHAR(20)  NOT NULL CONSTRAINT PK_Symbols PRIMARY KEY,
        Name       NVARCHAR(200) NULL,
        Exchange   NVARCHAR(50)  NULL,
        Country    NVARCHAR(50)  NULL,
        Sector     NVARCHAR(50)  NULL,
        IsActive   BIT           NOT NULL CONSTRAINT DF_Symbols_IsActive DEFAULT (1),
        CreatedAt  DATETIME2(7)  NOT NULL CONSTRAINT DF_Symbols_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt  DATETIME2(7)  NULL
    );
END
ELSE
BEGIN
    -- Patch columns if table already exists
    IF COL_LENGTH('dbo.Symbols','Name')      IS NULL ALTER TABLE dbo.Symbols ADD Name NVARCHAR(200) NULL;
    IF COL_LENGTH('dbo.Symbols','Exchange')  IS NULL ALTER TABLE dbo.Symbols ADD Exchange NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Symbols','Country')   IS NULL ALTER TABLE dbo.Symbols ADD Country NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Symbols','Sector')    IS NULL ALTER TABLE dbo.Symbols ADD Sector NVARCHAR(50) NULL;
    IF COL_LENGTH('dbo.Symbols','IsActive')  IS NULL ALTER TABLE dbo.Symbols ADD IsActive BIT NOT NULL CONSTRAINT DF_Symbols_IsActive DEFAULT (1);
    IF COL_LENGTH('dbo.Symbols','CreatedAt') IS NULL ALTER TABLE dbo.Symbols ADD CreatedAt DATETIME2(7) NOT NULL CONSTRAINT DF_Symbols_CreatedAt DEFAULT (SYSUTCDATETIME());
    IF COL_LENGTH('dbo.Symbols','UpdatedAt') IS NULL ALTER TABLE dbo.Symbols ADD UpdatedAt DATETIME2(7) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Symbols_Exchange' AND object_id = OBJECT_ID(N'dbo.Symbols'))
    CREATE INDEX IX_Symbols_Exchange ON dbo.Symbols(Exchange);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Symbols_Sector' AND object_id = OBJECT_ID(N'dbo.Symbols'))
    CREATE INDEX IX_Symbols_Sector ON dbo.Symbols(Sector);
GO
