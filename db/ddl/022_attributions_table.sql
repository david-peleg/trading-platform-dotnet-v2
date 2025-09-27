IF OBJECT_ID('dbo.Attributions','U') IS NULL
BEGIN
  CREATE TABLE dbo.Attributions(
    Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Attributions PRIMARY KEY,
    ItemType   TINYINT       NOT NULL,   -- 1=News,2=Filing,3=Analyst
    ItemId     BIGINT        NOT NULL,
    Symbol     NVARCHAR(20)  NOT NULL,
    Direction  SMALLINT      NOT NULL,   -- -1,0,+1
    Confidence FLOAT         NOT NULL,   -- 0..1
    HorizonD   SMALLINT      NOT NULL,   -- days
    CreatedAt  DATETIME2     NOT NULL CONSTRAINT DF_Attrib_CreatedAt DEFAULT SYSUTCDATETIME()
  );
  CREATE INDEX IX_Attrib_Symbol_Item ON dbo.Attributions(Symbol, ItemType, ItemId);
END
