
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateTotalCost]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateTotalCost]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateTotalCost]
@revisionId				INT,
@homecost               DECIMAL(18,4) OUTPUT,
@upgradecost            DECIMAL(18,4) OUTPUT,
@siteworkcost           DECIMAL(18,4) OUTPUT
AS
BEGIN

	SET NOCOUNT ON;
	
	   --DECLARE  @homecost DECIMAL(18,2), @updagradecost DECIMAL(18,2),@siteworkcost DECIMAL(18,2)
	   DECLARE @homeprice DECIMAL(18,2)
       DECLARE @estimateid INT, @promotionid INT, @currentrevisontype INT, @regionid INT, @brandid INT
       DECLARE @homeproductid VARCHAR(50), @depositdate DATETIME
       DECLARE @derivedhomepercentage DECIMAL(18,4), @deriveditempercentage DECIMAL(18,4), @targetmargin DECIMAL(18,4)
       
		DECLARE @estimateDetails TABLE ( 
		EstimateDetailsID INT,
		StandardInclusion BIT,
		PromotionProduct BIT,
		GroupId INT,
		AreaId INT )
             
		INSERT INTO @estimateDetails 
		SELECT EstimateDetailsID, StandardInclusion, PromotionProduct, GroupId, AreaId  
		FROM EstimateDetails WHERE EstimateID = (SELECT fkidEstimate FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId)

		DECLARE @mrsEstimateDetails TABLE (
		fkidEstimateDetails INT,
		IsSiteWork BIT,
		fkid_NonStandardGroup INT,
		CostExcGST MONEY,
		Quantity DECIMAL )

		INSERT INTO @mrsEstimateDetails
		SELECT fkidEstimateDetails, IsSiteWork, fkid_NonStandardGroup, CostExcGST, Quantity
		FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @revisionId
       
-- get upgrade cost
       SELECT sed.CostExcGST,sed.Quantity
       INTO #tempupgrade
       FROM @mrsEstimateDetails sed
       INNER JOIN @estimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID
       WHERE ed.StandardInclusion=0 AND ed.PromotionProduct=0 AND (sed.IsSiteWork=0 or sed.IsSiteWork IS NULL)
             AND ((ed.groupid<>286 AND ed.groupid<>189) and ed.areaid<>43)  AND sed.CostExcGST IS NOT NULL
             
       INSERT INTO #tempupgrade
       SELECT sed.CostExcGST,sed.Quantity
       FROM @mrsEstimateDetails sed
       INNER JOIN @estimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID
       WHERE ed.StandardInclusion=0 AND ed.PromotionProduct=0 AND (sed.IsSiteWork=0 or sed.IsSiteWork IS NULL)
             AND (sed.fkid_NonStandardGroup not IN (286,189) and ed.areaid=43)  
             AND sed.CostExcGST IS NOT NULL  

       SELECT @upgradecost=SUM(CostExcGST*Quantity)
       FROM #tempupgrade 

-- end upgrade cost
-- get home cost

		SELECT		@estimateid=fkidestimate,
					@regionid=e.RegionID,
					@homeproductid=h.ProductID,
					@brandid=h.BrandID,
					@homeprice=eh.HomePrice,
					@depositdate=dp.DepositDate
		FROM		tbl_SalesEstimate_EstimateHeader eh
		INNER JOIN	estimate e	ON eh.fkidestimate=e.estimateid
		INNER JOIN  home h ON e.HomeID=h.HomeID
		INNER JOIN  DepositDetails dp ON e.fkidOpportunity=dp.fkidOpportunity
		WHERE		id_SalesEstimate_EstimateHeader=@revisionId
	
		SELECT TOP 1 @homecost=ISNULL(CostPrice,0)
		FROM Price
		WHERE ProductID=@homeproductid AND RegionID=@regionid AND EffectiveDate<=@depositdate
		ORDER BY EffectiveDate DESC, CreatedDate DESC

		EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
			

		IF(@homecost=0 OR @homecost>=@homeprice OR @homecost IS NULL)
		   BEGIN
			 SET @homecost=CAST((@homeprice/1.1)*(1-@derivedhomepercentage) AS DECIMAL(18,2))
		   END
-- end home cost
-- get sitework cost
		SELECT @siteworkcost= ISNULL((SELECT SUM(ISNULL(SED.CostExcGST,0) * SED.Quantity) 
		FROM @mrsEstimateDetails SED 
		INNER JOIN @estimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
		WHERE  sed.CostExcGST IS NOT NULL AND SED.IsSiteWork = 1 AND ED.PromotionProduct=0),0)  	
-- end sitework cost

       -- SELECT @homecost as homecost, @updagradecost as upgradecost , @siteworkcost AS siteworkcost
       
       DROP TABLE #tempupgrade
     
	SET NOCOUNT OFF;
END
GO

