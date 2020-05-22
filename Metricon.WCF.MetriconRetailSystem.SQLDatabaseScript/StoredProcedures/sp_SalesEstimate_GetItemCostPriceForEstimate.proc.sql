
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetItemCostPriceForEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetItemCostPriceForEstimate]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetItemCostPriceForEstimate]
@estimateid		INT
AS
BEGIN

	SET NOCOUNT ON;
        DECLARE @depositDate DATETIME
        DECLARE @regionid INT, @brandid INT, @homeid INT
        DECLARE @derivedhomepercentage DECIMAL(18,4), @deriveditempercentage DECIMAL(18,4),  @targetmargin DECIMAL(18,4)
        --get deposit date
        
		SELECT 
			@depositDate = Dpst.DepositDate,
			@regionid=Est.RegionID,
			@brandid=BrandID,
			@homeid=est.homeid
		FROM Estimate Est   
		INNER JOIN Home h ON Est.HomeID=h.HomeID      
		INNER JOIN DepositDetails Dpst ON Est.BCContractNumber = Dpst.BCContractNumber
		WHERE Est.EstimateID = @estimateid 
		
		EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
		
   -- get all  productid
		SELECT * 
		INTO   #tempSelected
		FROM   EstimateDetails
		WHERE  EstimateID=@estimateId --AND Selected=1
		
		SELECT * 
		INTO #tempHDO
		FROM HomeDisplayOption 
		WHERE HomeID= @homeid AND Active=1 AND HomeDisplayID IS NULL
		
		SELECT DISTINCT productid, sellprice as tempprice, areaid
		INTO #tempProduct
		FROM #tempSelected
		
		-- get home product
		INSERT INTO #tempProduct
		SELECT h.ProductID, e.HomeSellPrice, 0
		FROM  Estimate e
		INNER JOIN Home h ON e.HomeID=h.HomeID
		WHERE e.EstimateID=@estimateid
		
		-- get new items in hdo
		INSERT INTO #tempProduct
		SELECT pag.ProductID,0,0
		FROM #temphdo tt
		INNER JOIN ProductAreaGroup pag on tt.productareagroupid=pag.ProductAreaGroupID
		LEFT JOIN #tempSelected ts ON tt.productareagroupid=ts.productareagroupid
		WHERE ts.productareagroupid IS NULL
	
	--  get all cost value for selected items
		select PriceID,p.ProductID,SellPrice,EffectiveDate,CreatedDate,RegionID,PromotionPrice, p.CostPrice ,p.derivedcost
		into #tempPrice
		from  #tempProduct pr
		inner join price p on pr.productid=p.productid 
		where active=1 and regionid=@regionid and effectivedate<=@depositDate
		
		insert into #tempPrice
		select PriceID,p.ProductID,SellPrice,EffectiveDate,CreatedDate,RegionID,PromotionPrice, p.CostPrice ,p.derivedcost
		from
		(select * from #tempProduct where productid not in (select distinct productid from #tempPrice)) pr
		inner join price p on pr.productid=p.productid 
		where active=1 and regionid=@regionid and effectivedate<=GETDATE()		 
 
		select productid,max(effectivedate) as effectivedate into #price1
			from #tempPrice 
			group by productid
			having max(effectivedate)<=@depositDate
			
			
		insert into #price1
		select productid,max(effectivedate) as effectivedate
		from #tempPrice where productid not in (select distinct productid from #price1)
		group by productid		
			
		select p1.productid,p1.effectivedate,max(price.createddate) as createddate into #price2
			from #price1 p1 
			inner join #tempPrice price	on p1.productid=price.productid and p1.effectivedate=price.effectivedate
			group by p1.productid, p1.effectivedate

		select price.productid as productid, max(isnull(price.promotionprice,0.0)) as promotionprice, p2.effectivedate,  max(isnull(price.CostPrice,0.0)) as CostPrice, ISNULL(Price.derivedcost,0) AS derivedcost, 0 as realcost
			into #currentPrice
			from #price2 p2 
			inner join #tempPrice price on p2.productid=price.productid
				and p2.effectivedate=price.effectivedate and p2.createddate=price.createddate
			group by price.productid,p2.effectivedate ,Price.derivedcost  
			
		-- get cost price					

		UPDATE  #currentPrice
		SET 
		    derivedcost=CASE WHEN t2.tempprice<0
		                     THEN pr.derivedcost
		                     ELSE CASE WHEN  t2.tempprice>0 AND ( ISNULL(pr.costprice,0)=0 OR pr.costprice=t2.tempprice)
		                               THEN  1
		                               ELSE  pr.derivedcost  
		                           END
		                END,		
		
		     realcost=  CASE WHEN t2.tempprice>0 AND ( ISNULL(pr.costprice,0)=0 OR pr.costprice=t2.tempprice)
		                     THEN 0
		                     ELSE 1
		                END,
		     costprice= 	 CASE WHEN t2.tempprice<0
		                      THEN pr.costprice 
		                      ELSE CASE WHEN t2.areaid<>43 AND t2.tempprice>0 AND ( ISNULL(pr.costprice,0)=0 OR pr.costprice=t2.tempprice)
		                                THEN CAST((t2.tempprice/1.1)*(1-@deriveditempercentage) AS DECIMAL(18,2))
		                                ELSE pr.costprice
		                           END
		                           
		END
		FROM  #currentPrice pr
		--INNER JOIN product pp ON pr.productid=pp.ProductID
		LEFT JOIN #tempProduct t2 ON pr.ProductID=t2.productid 
		   
		   
		SELECT * FROM #currentPrice -- where ProductID='NH-ELE-LIG-950' 

		    
	SET NOCOUNT OFF;
END
GO