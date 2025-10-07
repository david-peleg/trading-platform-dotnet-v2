CREATE TABLE dbo.Filings(
  Id BIGINT IDENTITY(1,1) PRIMARY KEY,
  Symbol NVARCHAR(20) NOT NULL,
  FilingType NVARCHAR(40) NOT NULL,     -- 10-K / 10-Q / 20-F / ImmediateReport / ...
  PeriodStart DATE NULL,
  PeriodEnd   DATE NULL,
  Url NVARCHAR(400) NOT NULL,
  Source NVARCHAR(100) NOT NULL,        -- EDGAR / MAYA / ...
  PublishedAt DATETIME2 NOT NULL,
  DocHash BINARY(32) NOT NULL,          -- SHA256 uniqueness of a document
  Lang NVARCHAR(10) NULL,
  CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

CREATE UNIQUE INDEX UX_Filings_DocHash ON dbo.Filings(DocHash);
GO

CREATE INDEX IX_Filings_Symbol_PublishedAt ON dbo.Filings(Symbol, PublishedAt DESC);
GO
