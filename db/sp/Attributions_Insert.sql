IF OBJECT_ID('dbo.Attributions_Insert','P') IS NOT NULL DROP PROCEDURE dbo.Attributions_Insert;
GO
CREATE PROCEDURE dbo.Attributions_Insert
  @ItemType   TINYINT,
  @ItemId     BIGINT,
  @Symbol     NVARCHAR(20),
  @Direction  SMALLINT,
  @Confidence FLOAT,
  @HorizonD   SMALLINT
AS
BEGIN
  SET NOCOUNT ON;
  INSERT dbo.Attributions(ItemType,ItemId,Symbol,Direction,Confidence,HorizonD)
  VALUES(@ItemType,@ItemId,@Symbol,@Direction,@Confidence,@HorizonD);
END
GO
