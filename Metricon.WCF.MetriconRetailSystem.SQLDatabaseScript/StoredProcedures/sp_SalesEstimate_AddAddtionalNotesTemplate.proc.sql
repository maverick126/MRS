----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_AddAddtionalNotesTemplate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_AddAddtionalNotesTemplate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <23/09/2011>
-- Description:	<add additonal notes template>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_AddAddtionalNotesTemplate] 
@revisionid			INT	,
@templatename		VARCHAR(500)	,
@userid				INT
AS
BEGIN

	SET NOCOUNT ON;
	
	DECLARE @estimateid	INT, @templateid INT, @regionid INT, @groupid INT, @priceregionid INT
	DECLARE @brandid	INT, @storey INT
	DECLARE @currentPrice TABLE
	(
	   productid VARCHAR(50),
	   promotionprice DECIMAL(18,2),
	   effectivedate DATETIME,
	   costprice DECIMAL(18,2),
	   derivedcost INT,
	   realcost INT
	)
	
	IF(PATINDEX('%(%',@templatename)>0)
	     SET @templatename=RTRIM(SUBSTRING(@templatename,1,PATINDEX('%(%',@templatename)-1))
	
	SELECT  @estimateid=fkidestimate,
	        @brandid=h.BrandID,
	        @storey=h.Stories
	FROm	tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
	INNER JOIN Home h ON e.HomeID=h.HomeID
	WHERE	id_SalesEstimate_EstimateHeader=@revisionid
	
	SELECT	 TOP  1	@groupid= sub.GroupID, @priceregionid=spm.fkRegionID
	FROM		tbl_SalesEstimate_Estimateheader eh
	INNER JOIN  tbluser u									ON	eh.fkidOwner=u.userid
	INNER JOIN  tblusersubregionmapping usm					ON  u.userid=usm.fkiduser
	INNER JOIN  tblSubRegion sub                            ON  usm.fkidSubRegion=sub.idSubRegion
	INNER JOIN  tblSubRegionPriceRegionMapping spm          ON  sub.idSubRegion=spm.fkidSubRegion
	WHERE		id_SalesEstimate_Estimateheader=@revisionid	
	
	SELECT idSubRegion INTO #subregion FROM tblSubRegion WHERE GroupID=@groupid

    INSERT INTO @currentPrice
    EXEC sp_SalesEstimate_GetItemCostPriceForEstimate @estimateid
       
  -- get price for items
		--SELECT		
  --                  pag.ProductID 
  --      INTO        #tempproduct
		--FROM        tbl_SalesEstimate_NotesTemplate nh                       		
		--INNER JOIN  tbl_SalesEstimate_NotesTemplateitems nd					ON  nh.id_SalesEstimate_NotesTemplate=nd.fkid_SalesEstimate_NotesTemplate
		--INNER JOIN	productareagroup pag									ON	nd.fkidproductareagroup=pag.productareagroupid
		--WHERE       nh.TemplateName=@templatename	
		
		--select PriceID,p.ProductID,SellPrice,EffectiveDate,CreatedDate,RegionID,PromotionPrice, ISNULL(p.CostPrice,0) AS  CostPrice
		--	into #tempPrice
		--	from price p 
		--	inner join #tempProduct tp on p.productid=tp.productid
		--where p.active=1 and regionid=@priceregionid and effectivedate<getdate()

		--select productid,max(effectivedate) as effectivedate into #price1
		--	from #tempPrice 
		--	group by productid
		--	having max(effectivedate)<getdate()

		--select p1.productid,p1.effectivedate,max(price.createddate) as createddate into #price2
		--	from #price1 p1 
		--	inner join #tempPrice price	on p1.productid=price.productid and p1.effectivedate=price.effectivedate
 	--		group by p1.productid, p1.effectivedate

		--select price.productid as productid, max(isnull(price.sellprice,0.0)) as sellprice, max(isnull(price.promotionprice,0.0)) as promotionprice,p2.effectivedate, CAST(MAX(Price.CostPrice) AS DECIMAL(18,2)) AS CostPrice
		--	into #currentPrice
		--	from #price2 p2 
		--	inner join #tempPrice price on p2.productid=price.productid
		--		and p2.effectivedate=price.effectivedate and p2.createddate=price.createddate
		--	group by price.productid,p2.effectivedate	  
  -- end price


	
	SELECT	@templateid=id_SalesEstimate_NotesTemplate 
	FROM	tbl_SalesEstimate_NotesTemplate
	WHERE	fkidSubRegion IN (SELECT idsubregion FROM #subregion)	AND		TemplateName=@templatename
	
	SELECT	ed.*,pag.IsSiteWork, pp.minibillStart, 	
	        CASE WHEN @storey=1
	             THEN a.SortOrder
	             ELSE a.SortOrderDouble
	        END AS areasortorder,
	        g.SortOrder AS groupsortorder,
	        pp.SortOrder AS productsortorder,
	        pp.fkPriceDisplayCodeID AS PriceDisplayCodeId
	INTO    #tempED	
	FROM	estimatedetails ed
	INNER JOIN ProductAreaGroup pag ON ed.ProductAreaGroupID=pag.ProductAreaGroupID
	INNER JOIN product pp			ON ed.ProductID=pp.ProductID
	INNER JOIN Area a				ON pag.AreaID=a.AreaID
	INNER JOIN [group] g			ON pag.GroupID=g.GroupID	
	WHERE	estimateid=@estimateid
	
	SELECT		ed1.*,ed1.fkidProductAreaGroup as productareagroupid	
	INTO		#tempED2	
	FROM		tbl_SalesEstimate_EstimateDetails ed1
	--INNER JOIN	#tempED ed2		ON	ed1.fkidestimatedetails=ed2.estimatedetailsid
	WHERE		fkid_SalesEstimate_EstimateHeader=@revisionid

	INSERT INTO tbl_SalesEstimate_EstimateDetails
			   ([fkid_SalesEstimate_EstimateHeader]
			   ,[fkidEstimateDetails]
			   ,[ItemPrice]
			   ,[Quantity]
			   ,[ProductDescription]
			   ,[ExtraDescription]
			   ,[InternalDescription]
			   ,[Additionalinfo]
			   ,[CreatedOn]
			   ,[CreatedBy]
			   ,[IsSiteWork]
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
			   ,[fkid_NonStandardPriceDisplayCode]
			   )
 
	SELECT      @revisionid,
	            ed.estimatedetailsid,
	            ISNULL(pr.promotionprice,0),
	            ISNULL(n.quantity,1),
	            ed.productdescription,
	            n.extradescription,
	            n.internaldescription,
	            n.AdditionalInfo,
	            GETDATE(),
	            @userid,
	            ed.issitework,
                pr.derivedcost,	            
			    ISNULL(pr.costprice,0),
			    ed.areasortorder,
			    ed.groupsortorder,
			    ed.productsortorder,
			    ed.areaid,
			    ed.areaname,
			    ed.groupid,
			    ed.groupname,
			    ed.productareagroupid,
			    ed.productname,
			    ed.promotionproduct,
			    ed.PriceDisplayCodeId
			    			
	FROM		tbl_SalesEstimate_NotesTemplateItems n
	INNER JOIN	#tempED	ed		ON n.fkidproductareagroup=ed.productareagroupid
	LEFT JOIN	#tempED2 ed2	ON n.fkidproductareagroup=ed2.productareagroupid
	INNER JOIN  @currentPrice pr ON ed.productid=pr.Productid
	WHERE		FKID_SalesEstimate_NotesTemplate = @templateid	
				AND
				ed2.productareagroupid IS NULL 
				AND 
				n.Active = 1

    DROP TABLE #subregion

    SET NOCOUNT OFF
END

GO