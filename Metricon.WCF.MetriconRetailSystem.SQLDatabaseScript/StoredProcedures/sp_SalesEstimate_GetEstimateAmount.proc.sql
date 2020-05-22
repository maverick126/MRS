IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateAmount]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateAmount]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateAmount]
@estimateRevisionId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @upgradePrice DECIMAL, @homePrice DECIMAL
	
	SELECT @upgradePrice = SUM(SED.Quantity * SED.ItemPrice) 
	FROM tbl_SalesEstimate_EstimateDetails SED
	--INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = ED.EstimateDetailsID
	WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND SED.IsPromotionProduct = 0

	SELECT @homePrice = HomePrice FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
		
	SELECT @upgradePrice AS UpgradePrice, @homePrice AS HomePrice
END