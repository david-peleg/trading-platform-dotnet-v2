IF OBJECT_ID('dbo.RawNews','U') IS NULL
BEGIN
  CREATE TABLE dbo.RawNews(
    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RawNews PRIMARY KEY,
    Ticker        NVARCHAR(20)  NULL,
    Headline      NVARCHAR(500) NOT NULL,
    Url           NVARCHAR(400) NOT NULL,
    Source        NVARCHAR(100) NOT NULL,
    PublishedAt   DATETIME2      NOT NULL,
    BodyHash      BINARY(32)     NOT NULL,
    Lang          NVARCHAR(10)   NULL,
    CreatedAt     DATETIME2       NOT NULL CONSTRAINT DF_RawNews_CreatedAt DEFAULT SYSUTCDATETIME()
  );
  CREATE UNIQUE INDEX UX_RawNews_BodyHash ON dbo.RawNews(BodyHash);
  CREATE INDEX IX_RawNews_Ticker_PublishedAt ON dbo.RawNews(Ticker, PublishedAt DESC);
END
