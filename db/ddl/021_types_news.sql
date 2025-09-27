IF TYPE_ID('dbo.RawNewsType') IS NULL
  CREATE TYPE dbo.RawNewsType AS TABLE(
    Ticker      NVARCHAR(20)   NULL,
    Headline    NVARCHAR(500)  NOT NULL,
    Url         NVARCHAR(400)  NOT NULL,
    Source      NVARCHAR(100)  NOT NULL,
    PublishedAt DATETIME2       NOT NULL,
    BodyHash    BINARY(32)      NOT NULL,
    Lang        NVARCHAR(10)    NULL
  );
