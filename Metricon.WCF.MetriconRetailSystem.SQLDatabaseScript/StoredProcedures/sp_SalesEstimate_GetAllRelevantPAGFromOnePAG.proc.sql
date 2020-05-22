----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAllRelevantPAGFromOnePAG]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAllRelevantPAGFromOnePAG]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<AuFZ>
-- Create date: <21/11/2012>
-- Description:	<get all pags from one pag>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAllRelevantPAGFromOnePAG] 
@EstimateDetailsId			INT		,
--@StandardInclusionsId INT, OBSOLETED
@revisionid	INT
AS
BEGIN
		SET NOCOUNT ON;

		DECLARE @pagid INT, @estimateid INT , @brandid INT, @regionid INT, @groupid INT
		DECLARE @productid VARCHAR(25), @depositdate DATETIME
		DECLARE @derivedcost INT, @costexcgst DECIMAL(18,2), @sellprice  DECIMAL(18,2)
		DECLARE @derivedhomepercentage DECIMAL(18,4), @deriveditempercentage DECIMAL(18,4), @targetmargin DECIMAL(18,4)
		DECLARE @revisiontypeid INT, @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT
		
		SELECT 
		@revisiontypeid=eh.fkid_SalesEstimate_RevisionType
		FROM tbl_SalesEstimate_EstimateHeader eh
		INNER JOIN	estimate e	ON eh.fkidestimate=e.estimateid
		INNER JOIN	home h		ON e.homeid=h.homeid
		WHERE id_SalesEstimate_EstimateHeader=@revisionId
		
		EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId, @allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT  
		
		DECLARE @existingestimatedetails TABLE
		(
			estimatedetailsId BIGINT
		)

		DECLARE @EstimateDetails TABLE
		(
			EstimateDetailsId BIGINT,
			ProductDescription VARCHAR(MAX),
			AdditionalInfo VARCHAR(MAX),
			ExtraDescription VARCHAR(MAX),
			InternalDescription VARCHAR(MAX),
			StandardOption BIT,
			Quantity DECIMAL(18,2),
			HomeId BIGINT,
			HomeDisplayId BIGINT,
			ProductAreaGroupId BIGINT,
			Uom VARCHAR(50),
			ChangePrice BIT 
		)
		
		-- get all exists estimate details id
		INSERT INTO  @existingestimatedetails 
		SELECT ed.fkidEstimateDetails
		FROM tbl_SalesEstimate_EstimateDetails ed
		WHERE fkid_salesestimate_estimateheader=@revisionid
								
		-- get data
		SELECT   @estimateid=estimateid, 
		         @pagid=ed.productareagroupid, 
		         @productid=pag.productid, 
		         @sellprice=SellPrice,
		         @groupid=ed.groupid
		FROM estimatedetails ed
		INNER JOIN productareagroup pag on ed.productareagroupid=pag.productareagroupid
		WHERE	estimatedetailsid=  @EstimateDetailsId

		INSERT INTO @EstimateDetails 
		SELECT EstimateDetailsID,
		ProductDescription,
		AdditionalInfo,
		EnterDesc,
		InternalDescription,
		StandardOption,
		Quantity,
		HomeID,
		HomeDisplayID,
		ProductAreaGroupID,
		UOM, 
		ChangePrice
		FROM EstimateDetails
		WHERE EstimateID=@estimateid AND EstimateDetailsID NOT IN (SELECT * FROM @existingestimatedetails)	

		SELECT   pag.productareagroupid,pag.productid,pag.areaid,pag.groupid, a.areaname,g.groupname, ISNULL(pag.IsSiteWork, 0) AS IsSiteWork
		INTO		#temppag2
		FROM		productareagroup pag
		INNER JOIN area a on pag.areaid=a.areaid
		INNER JOIN [group] g on pag.groupid=g.groupid
		WHERE    productid=@productid --AND pag.active=1 AND a.active=1 AND g.active=1
-- get cost
        SELECT @regionid=e.RegionID,
               @brandid=h.BrandID,
               @depositdate=dp.DepositDate
        FROM Estimate e
        INNER JOIN DepositDetails dp ON e.fkidOpportunity=dp.fkidOpportunity
        INNER JOIN Home h ON e.HomeID=h.HomeID
        WHERE EstimateID=@estimateid
           
		DECLARE @priceDisplayCodeId INT, @priceDisplayDescription VARCHAR(50)
		 
		SELECT @priceDisplayCodeId=PriceDisplayCodeID, @priceDisplayDescription = PriceDisplayCode + ' - ' + PriceDisplayDesc
        FROM Product P INNER JOIN tblPriceDisplayCode PDC ON ISNULL(P.fkPriceDisplayCodeID, 10) = PDC.PriceDisplayCodeID 
        WHERE ProductID=@productid
        
        SELECT TOP 1 @costexcgst=ISNULL(CostPrice,0), @sellprice=ISNULL(PromotionPrice,0),@derivedcost=ISNULL(derivedcost,0)
        FROM Price
        WHERE ProductID=@productid AND RegionID=@regionid AND Active=1 AND EffectiveDate<=@depositdate
        ORDER BY EffectiveDate DESC
 
 	    EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT

        IF(@sellprice>0)
           BEGIN
				IF(@costexcgst=0 OR @costexcgst=@sellprice)
				   BEGIN
				      SET @derivedcost=1
					  SET @costexcgst=CAST((@sellprice/1.1)*(1-@deriveditempercentage) AS DECIMAL(18,2))
				   END
           END          
        ELSE
           BEGIN
               IF(@groupid in (286,189))
                  BEGIN
                    SET @derivedcost=1
                    SET @costexcgst=0
                  END
           END


-- end cost


		-- get final data
		SELECT       
		ed.estimatedetailsid, 
		0 AS idStandardInclusions,
		ed.StandardOption,
		CASE WHEN ed.HomeId IS NOT NULL 
	     THEN h.homename + ' Display at ' + d.suburb
	     ELSE '' 
		END AS DisplayAt,
		t2.*, 
		CASE WHEN ed.estimatedetailsid=@EstimateDetailsId THEN CAST('true' AS BIT) ELSE CAST('false' AS BIT) END AS selected,
		CAST(ISNULL(@costexcgst,0) AS Decimal(18,2)) AS costexcgst,
		ISNULL(@derivedcost,0) AS derivedcost,
		ed.ProductDescription,
		ed.AdditionalInfo,
		ed.ExtraDescription,
		ed.InternalDescription,
		CAST(ed.Quantity AS Decimal(18,2)) AS Quantity,
		CAST(@sellprice AS Decimal(18,2)) AS Price,
		ed.Uom,
		t2.IsSiteWork,
		@priceDisplayCodeId AS PriceDisplayCodeId,
		@priceDisplayDescription AS PriceDisplayDescription,
		
		CASE WHEN @costexcgst IS NOT NULL
		     THEN
					CASE WHEN @sellprice<>0 
						 THEN CAST(CAST(ISNULL((((@sellprice/1.1)-CAST(@costexcgst AS DECIMAL(18,2)))/(@sellprice/1.1))*100, 0)	 AS DECIMAL(18,2))  AS VARCHAR )
						 ELSE '' 
					END 
		     ELSE ''
		END
		AS margin,
		
		CASE WHEN @allowchangeqty=1 
			 THEN 1
			 ELSE	
				 CASE WHEN ed.Uom='NOTE'
				 THEN 0
				 ELSE 1   
				 END        
		END 		
		AS ChangeQty,

		CASE WHEN @allowchangeprice=1
		  THEN 1
		  ELSE			
				 CASE WHEN (ed.Uom='NOTE')
				 THEN 0
				 ELSE CASE WHEN t2.areaid=43
						   THEN 1
						   ELSE ed.ChangePrice
					  END
				 END
		END     
		AS ChangePrice,

		CASE WHEN @revisiontypeid IN (4,15, 19, 25) 
		     THEN @allowchangeDesc 
		     ELSE CASE WHEN t2.areaid=43
		               THEN 1
		               ELSE 0
		     END
		END
		AS changeproductstandarddescription,
		
		CASE WHEN @revisiontypeid IN (4,15, 19, 25) 
		     THEN @allowchangePriceDisplayCode 
		     ELSE 0
		END
		AS changedisplaycode
		FROM @EstimateDetails ed
		INNER JOIN  #temppag2 t2 ON ed.ProductAreaGroupId=t2.productareagroupid
		LEFT JOIN display d ON ed.HomeDisplayId=d.homeid
		LEFT JOIN home h ON d.homeid=h.homeid
		ORDER BY areaname, groupname
		
	
		
	SET NOCOUNT OFF;
END

GO
