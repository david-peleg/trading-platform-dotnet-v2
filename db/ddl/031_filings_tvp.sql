-- TVP for bulk upsert
CREATE TYPE dbo.FilingRowType AS TABLE(
  Symbol NVARCHAR(20) NOT NULL,
  FilingType NVARCHAR(40) NOT NULL,
  PeriodStart DATE NULL,
  PeriodEnd   DATE NULL,
  Url NVARCHAR(400) NOT NULL,
  Source NVARCHAR(100) NOT NULL,
  PublishedAt DATETIME2 NOT NULL,
  DocHash BINARY(32) NOT NULL,
  Lang NVARCHAR(10) NULL
);
GO
