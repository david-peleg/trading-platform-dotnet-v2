IF NOT EXISTS (SELECT 1 FROM sys.types WHERE is_table_type = 1 AND name = 'AttributionType')
BEGIN
    CREATE TYPE dbo.AttributionType AS TABLE(
        ItemType   TINYINT       NOT NULL,
        ItemId     BIGINT        NOT NULL,
        Symbol     NVARCHAR(20)  NOT NULL,
        Direction  SMALLINT      NOT NULL,
        Confidence FLOAT         NOT NULL,
        HorizonD   SMALLINT      NOT NULL
    );
END
GO