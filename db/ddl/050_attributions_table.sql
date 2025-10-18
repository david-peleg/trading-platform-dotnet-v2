IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Attributions' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
  CREATE TABLE dbo.Attributions(
      Id         BIGINT IDENTITY(1,1) PRIMARY KEY,
      ItemType   TINYINT       NOT NULL,   -- 1=News, 2=Filing, 3=Analyst
      ItemId     BIGINT        NOT NULL,
      Symbol     NVARCHAR(20)  NOT NULL,
      Direction  SMALLINT      NOT NULL,   -- -1/0/+1
      Confidence FLOAT         NOT NULL,   -- 0..1
      HorizonD   SMALLINT      NOT NULL,   -- 1/7/30/60
      CreatedAt  DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
  );
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Attr_Symbol' AND object_id=OBJECT_ID('dbo.Attributions'))
  CREATE INDEX IX_Attr_Symbol ON dbo.Attributions(Symbol, CreatedAt DESC);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='UX_Attr_Item' AND object_id=OBJECT_ID('dbo.Attributions'))
  CREATE UNIQUE INDEX UX_Attr_Item ON dbo.Attributions(ItemType, ItemId, Symbol, HorizonD);
