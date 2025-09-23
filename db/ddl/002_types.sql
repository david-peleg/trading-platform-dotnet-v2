-- db/ddl/002_types.sql
IF TYPE_ID(N'dbo.SymbolType') IS NOT NULL
    DROP TYPE dbo.SymbolType;
GO

CREATE TYPE dbo.SymbolType AS TABLE
(
    Symbol   NVARCHAR(20)  NOT NULL,
    Name     NVARCHAR(200) NULL,
    Exchange NVARCHAR(50)  NULL,
    Country  NVARCHAR(50)  NULL,
    Sector   NVARCHAR(50)  NULL,
    IsActive BIT           NULL
);
GO
