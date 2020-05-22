----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CopyDescriptionFromOtherHomeToNonstandardOption]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CopyDescriptionFromOtherHomeToNonstandardOption]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_CopyDescriptionFromOtherHomeToNonstandardOption]
    @regionid                   INT,
	@optionid					BIGINT,
	@revisionId					INT,
	@userId						INT
AS
BEGIN

    DECLARE @len	INT
    DECLARE @tempareaid	INT
    DECLARE @tempgroupid INT, @targetpagid INT 
	DECLARE @NonStandardAreaName VARCHAR(255), @NonStandardGroupName VARCHAR(255), @NonStandardProductName VARCHAR(300)
	DECLARE @revisionTypeId INT, @NonStandardAreaid INT,@NonStandardGroupid INT
    DECLARE @derivedhomepercentage DECIMAL(18,4), @deriveditempercentage DECIMAL(18,4), @targetmargin DECIMAL(18,4)
	DECLARE @estimateid INT, @tempestimatedetailsid	INT, @newID INT, @depositdate DATETIME
	DECLARE @brandid INT,  @Costprice DECIMAL(18,2), @sellprice DECIMAL(18,2), @productid VARCHAR(50), @storey INT
	DECLARE @areasortorder INT, @groupsortorder INT, @productsortorder INT , @areaid INT, @groupid INT
	DECLARE @productareagroupid BIGINT

	DECLARE @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT
	
	SELECT @productareagroupid = ProductAreaGroupId FROM HomeDisplayOption WHERE OptionID = @optionid
	
	SET @len=100
	SELECT @NonStandardAreaName = AreaName, 
	       @areaid=pag.areaid,
	       @NonStandardGroupName = GroupName,
	       @groupid=pag.groupid,
	       @productid=pt.ProductID,
	       @NonStandardProductName=pt.ProductName
	FROM ProductAreaGroup pag
	INNER JOIN Area a ON pag.AreaID=a.AreaID
	INNER JOIN [Group] g ON pag.GroupID=g.GroupID
	INNER JOIN product pt ON pag.ProductID=pt.ProductID
	WHERE productareagroupid = @productareagroupid

	--IF (EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @revisionId 
	--	AND fkidEstimateDetails = @EstimateDetailsId 
	--	AND fkid_NonStandardArea IS NOT NULL 
	--	AND fkid_NonStandardGroup IS NOT NULL))
	--BEGIN

	--	SELECT @NonStandardAreaName = Area.AreaName, @NonStandardGroupName = [Group].GroupName
	--	FROM tbl_SalesEstimate_EstimateDetails 
	--	INNER JOIN Area ON tbl_SalesEstimate_EstimateDetails.fkid_NonStandardArea = Area.AreaID
	--	INNER JOIN [Group] ON tbl_SalesEstimate_EstimateDetails.fkid_NonStandardGroup = [Group].GroupID
	--	WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND fkidEstimateDetails = @EstimateDetailsId
	--END
	
	SELECT 
	       @brandid=h.BrandID,
	       @estimateid=e.EstimateID,
	       @depositdate=dp.DepositDate,
	       @storey=h.Stories,
	       @revisionTypeId = eh.fkid_SalesEstimate_RevisionType
	FROM   tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
	INNER JOIN DepositDetails dp ON e.fkidOpportunity=dp.fkidOpportunity
	INNER JOIN Home h ON e.HomeID=h.HomeID
	WHERE eh.id_SalesEstimate_EstimateHeader=@revisionId
	
	--SELECT @productid=ProductID, 
	--       @sellprice=ISNULL(SellPrice,0),
	--       @areaid=areaid,
	--       @groupid=groupid
	--FROM EstimateDetails
	--WHERE EstimateDetailsID=@EstimateDetailsId
	
    SELECT te.*, ed.areaid, ed.groupid,ed.ProductID
    INTO   #tempMRSED
    FROM   tbl_SalesEstimate_EstimateDetails te
    INNER JOIN EstimateDetails ed ON te.fkidEstimateDetails=ed.EstimateDetailsID
    WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid
    
    SELECT * 
    INTO #temphdo
    FROM HomeDisplayOption
    WHERE OptionID=@optionid AND Active=1       
         
    SELECT TOP 1 @sellprice=PromotionPrice,
                 @Costprice=ISNULL(CostPrice,0)
    FROM Price 
    WHERE Regionid=@regionid AND
          ProductID=@productid AND
          Active=1 AND 
          EffectiveDate<=GETDATE()
    ORDER BY EffectiveDate desc
    
    SET @allowchangeprice=1
    SET @allowchangeqty=1
    SET @allowchangeDesc=1
    SET @allowchangePriceDisplayCode=1
    --EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId,@allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT 
    	
   IF (EXISTS(SELECT * FROM #tempMRSED WHERE areaid=@areaid))
      BEGIN
         SELECT TOP 1 @areasortorder=areasortorder FROM #tempMRSED WHERE areaid=@areaid
      END
   ELSE
      BEGIN
         SELECT @areasortorder=CASE WHEN @storey=1 THEN SortOrder ELSE SortOrderDouble END
         FROM Area 
         WHERE AreaID=@areaid
      END

   IF (EXISTS(SELECT * FROM #tempMRSED WHERE groupid=@groupid))
      BEGIN
         SELECT TOP 1 @groupsortorder=groupsortorder FROM #tempMRSED WHERE groupid=@groupid
      END
   ELSE
      BEGIN
         SELECT @groupsortorder=sortorder
         FROM [group]
         WHERE groupid=@groupid
      END                       

   IF (EXISTS(SELECT * FROM #tempMRSED WHERE productid=RTRIM(@productid)))
      BEGIN
         SELECT TOP 1 @productsortorder=productsortorder FROM #tempMRSED WHERE productid=RTRIM(@productid)
      END
   ELSE
      BEGIN
         SELECT @productsortorder=sortorder
         FROM [product]
         WHERE productid=RTRIM(@productid)
      END 
	

	
	EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
	
	IF(@Costprice=0)
	  BEGIN
	    SET @Costprice=CAST((@sellprice/1.1)*(1-@deriveditempercentage) AS DECIMAL(18,2))
	  END
	
	SELECT @estimateid=fkidestimate FROM tbl_SalesEstimate_EstimateHeader	WHERE id_SalesEstimate_EstimateHeader=@revisionId
   
	SELECT *	INTO #temped
	FROM		estimatedetails
	WHERE		estimateid=@estimateid
	-- get top non standard item which not exists in revision
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionType WHERE id_SalesEstimate_RevisionType = @revisiontypeid AND ExclusiveProductId IS NOT NULL)
	BEGIN
		SET @tempestimatedetailsid=
		(
				SELECT		top 1 ed.estimatedetailsid
				FROM		#temped ed
				INNER JOIN	productareagroup	pag	ON	ed.productareagroupid=pag.productareagroupid
				WHERE		pag.areaid=43		AND
							pag.groupid=24		AND
							ed.estimatedetailsid	NOT IN
								(SELECT fkidestimatedetails FROM tbl_SalesEstimate_EstimateDetails 
								WHERE fkid_salesestimate_estimateheader=@revisionId 
								AND fkidestimatedetails IS NOT NULL AND fkidestimatedetails>0) AND
							SUBSTRING(ed.ProductID,2,10) = 
								(SELECT TOP 1 SUBSTRING(ExclusiveProductId,2,10) FROM tbl_SalesEstimate_RevisionType 
								WHERE id_SalesEstimate_RevisionType = @revisiontypeid AND ExclusiveProductId IS NOT NULL)
							
				ORDER BY	pag.productid
		)
    END
    ELSE
    BEGIN
 		SET @tempestimatedetailsid=
		(
				SELECT		top 1 ed.estimatedetailsid
				FROM		#temped ed
				INNER JOIN	productareagroup	pag	ON	ed.productareagroupid=pag.productareagroupid
				WHERE		pag.areaid=43		AND
							pag.groupid=24		AND
							ed.estimatedetailsid	NOT IN
								(SELECT fkidestimatedetails FROM tbl_SalesEstimate_EstimateDetails 
								WHERE fkid_salesestimate_estimateheader=@revisionId 
								AND fkidestimatedetails IS NOT NULL AND fkidestimatedetails>0) AND
							SUBSTRING(ed.ProductID,2,10) NOT IN 
								(SELECT SUBSTRING(ExclusiveProductId,2,10) FROM tbl_SalesEstimate_RevisionType 
								WHERE ExclusiveProductId IS NOT NULL)
				ORDER BY	pag.productid
		)   
    END
    
    SELECT @targetpagid=ProductAreaGroupID, 	       
    @NonStandardProductName=ProductName
    FROM estimatedetails
    WHERE EstimateDetailsID=@tempestimatedetailsid 


					-- get the description of old item
					INSERT INTO tbl_SalesEstimate_EstimateDetails
							   ([fkid_SalesEstimate_EstimateHeader]
							   ,[fkidEstimateDetails]
							   ,[ItemPrice]
							   ,[Quantity]
							   ,[ProductDescription]
							   ,[ExtraDescription]
							   ,[InternalDescription]
							   ,[CreatedOn]
							   ,[CreatedBy]
							   ,[ModifiedOn]
							   ,[ModifiedBy]
							   ,[fkid_NonStandardArea]
							   ,[fkid_NonStandardGroup]
							   ,[additionalinfo]
							   ,[fkid_NonStandardPriceDisplayCode]
							   ,[DerivedCost]
							   ,[CostExcGST]
							   ,[AreaSortOrder]
							   ,[GroupSortOrder]
							   ,[ProductSortOrder]
							   ,[fkidArea]
							   ,[AreaName]
							   ,[fkidGroup]
							   ,[GroupName]
							   ,[fkidProductAreaGroup]
							   ,[ProductName]
							   ,[IsPromotionProduct]							   
							   
								)	
					
					SELECT 
								@revisionId,
								@tempestimatedetailsid,
								@sellprice,
								ed.quantity,
								pp.productdescription,
								ed.enterdesc,
								NULL,
								GETDATE(),
								@userId, 
								NULL,
								NULL,
								@areaid,
								@Groupid,
								'',
								10, --Default to NONE - NONE
								1,
								@Costprice,
                                @areasortorder,
								@groupsortorder,
								@productsortorder,
								43, --Non Standard Request Area
								@NonStandardAreaName,
								24, --Non Standard Group
								@NonStandardGroupName,
								@targetpagid,
								@NonStandardProductName,
								0		
					FROM		#temphdo ed
					INNER JOIN  ProductAreaGroup pag ON ed.ProductAreaGroupID=pag.ProductAreaGroupID
					INNER JOIN  product pp           ON pag.ProductID=pp.ProductID
					INNER JOIN  Area a               ON pag.AreaID=a.AreaID
					INNER JOIN  [group] g            ON pag.GroupID=g.GroupID					

					
					SET @newID=@@identity

					SELECT @tempareaid=fkid_NonStandardArea, @tempgroupid=fkid_NonStandardGroup FROM tbl_SalesEstimate_EstimateDetails WHERE id_SalesEstimate_EstimateDetails=@newID
					
					SELECT	
								'Non Standard Request' AS AreaName, --ed2.areaname		,
								'Non Standard' AS GroupName, --ed2.groupname		,
								43 AS AreaId,
								24 AS GroupId,
								ed.ProductID		,
								ed.ProductName+char(13)+'['+ed.ProductID+']' AS 	ProductName,
								ed2.ProductDescription,
								CASE  WHEN	LEN(ed2.ProductDescription)>@len
									  THEN	SUBSTRING(ed2.ProductDescription,1,@len)+' ...'
									  ELSE  ed2.ProductDescription
									  END
								AS ProductDescriptionShort,
								ed2.ExtraDescription AS EnterDesc,
								ed.UOM,
								ed2.Quantity,
							    CAST(ISNULL(itemPrice,0) AS DECIMAL(18,2)) AS sellprice,
								PromotionProduct,
								1 AS StandardOption,
								ed2.internalDescription,							
								EstimateDetailsId,
								@newID AS EstimateRevisionDetailsId,
								@NonStandardAreaName AS NonStandardAreaName,
								@NonStandardGroupName AS NonStandardGroupName,
								0 AS itemAccepted,
								ISNULL(@tempareaid,0) AS fkid_NonStandardArea,
								ISNULL(@tempgroupid,0) AS fkid_NonStandardGroup,
								ed2.additionalinfo,
								ISNULL(pdc.PriceDisplayCodeID, 10) AS PriceDisplayCodeID,
								ISNULL(pdc.PriceDisplayCode + ' - ' + pdc.PriceDisplayDesc,'NONE - NONE') AS PriceDisplayDesc,
								ed2.DerivedCost AS derivedcost,
							    CAST(ed2.CostExcGST AS DECIMAL(18,2)) AS costexcgst,
							    CASE WHEN ed2.ItemPrice>0
							         THEN CAST(100*((ed2.ItemPrice/1.1)-ed2.CostExcGST)/(ed2.ItemPrice/1.1) AS DECIMAL(18,2)) 
							         ELSE CASE WHEN ed2.ItemPrice=0
							                   THEN CAST(0 AS DECIMAL(18,2)) 
							                   ELSE CAST(-100 AS DECIMAL(18,2))
							              END
							    END AS margin,
								--1 AS ChangeQty,  
								--1 AS ChangePrice	
								@allowchangeqty AS ChangeQty,
								@allowchangeprice AS ChangePrice,
								@allowchangePriceDisplayCode AS changedisplaycode,
								@allowchangeDesc AS changeproductstandarddescription														    
					FROM		(SELECT * FROM estimatedetails WHERE estimatedetailsid=@tempestimatedetailsid)	ed
					INNER JOIN productareagroup pag ON ed.productareagroupid= pag.productareagroupid
					INNER JOIN product p ON pag.productid=p.productid					
					INNER JOIN  tbl_SalesEstimate_EstimateDetails ed2 ON ed.EstimateDetailsID=ed2.fkidEstimateDetails
					LEFT JOIN tblPriceDisplayCode pdc ON p.fkPriceDisplayCodeID = pdc.PriceDisplayCodeID
					WHERE		id_SalesEstimate_EstimateDetails=	@newID						

      IF(@newID>0)
         BEGIN
           INSERT INTO tbl_SalesEstimate_SourceOfNSRCopy
           SELECT @revisionId, 0, @productareagroupid, @newID, @targetpagid, 'OptionId='+CAST(@optionid AS VARCHAR), GETDATE(), @userId
         END

   DROP TABLE #tempMRSED
   DROP TABLE #temphdo
   DROP TABLE #temped

END

GO