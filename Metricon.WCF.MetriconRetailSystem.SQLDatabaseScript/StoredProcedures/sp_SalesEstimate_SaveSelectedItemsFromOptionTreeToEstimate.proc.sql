----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_SaveSelectedItemsFromOptionTreeToEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_SaveSelectedItemsFromOptionTreeToEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_SaveSelectedItemsFromOptionTreeToEstimate] 
@estimatedetailsidstring	VARCHAR(MAX),
@standardinclusionidstring	VARCHAR(1000),
@revisionid					INT,
@studiomanswer				VARCHAR(MAX),
@derivedcoststring			VARCHAR(MAX),
@costexcgststring			VARCHAR(MAX),
@quantitystring				VARCHAR(MAX),
@pricestring				VARCHAR(MAX),
@isacceptedstring			VARCHAR(MAX), 
@areaidstring				VARCHAR(MAX),
@groupidstring				VARCHAR(MAX),
@pricedisplaycodestring		VARCHAR(MAX),
@issiteworkstring			VARCHAR(MAX),
@productdescriptionstring	VARCHAR(MAX),
@additionalnotestring		VARCHAR(MAX),
@extradescriptionstring		VARCHAR(MAX), 
@internaldescriptionstring	VARCHAR(MAX),  
@userid						INT,
@action						VARCHAR(30)   -- action: optiontree or studom_answer. optiontree means add item from tree, insert in table; studiom_answer means answer studiom question, update existing
AS
BEGIN
	SET NOCOUNT ON;
	
	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000)
	DECLARE @ErrSeverity INT
    
    DECLARE @revisiontypeid INT , @areasortorder INT, @groupsortorder INT, @productsortorder INT , @areaid INT, @groupid INT, @productID VARCHAR(50) 
	DECLARE @estimateid INT, @brandid INT, @regiongroupid INT	, @storey INT, @promotionid INT		
	DECLARE @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT, @stateid INT
	
	SELECT 0 AS fkidmultiplepromotion INTO #tempmultiplepromotion
				
	SELECT	@estimateid=fkidestimate, @revisiontypeid=fkid_salesestimate_revisiontype
	FROM tbl_SalesEstimate_EstimateHeader 
	WHERE id_SalesEstimate_EstimateHeader=@revisionid

	SELECT @brandid=h.brandid, 
	       @regiongroupid=rg.fkidregiongroup,
	       @storey=h.Stories,
	       @stateid=h.fkStateID,
	       @promotionid=PromotionID
	FROM estimate e
	INNER JOIN home h ON e.homeid=h.homeid
	INNER JOIN tblpriceregiongroupmapping rg ON e.regionid=rg.fkregionid
	WHERE estimateid=@estimateid

    SELECT EstimateDetailsID,
		ProductAreaGroupID,
		AreaName,
		GroupName,
		AreaID,
		GroupID,
		ProductName,
		ProductDescription,
		Quantity,
		SellPrice,
		EnterDesc,
		AdditionalInfo
    INTO #originaltemped
    FROM EstimateDetails
    WHERE estimateid=@estimateid AND HomeDisplayID IS NULL
 
	CREATE NONCLUSTERED INDEX idx_1 ON #originaltemped  (ProductAreaGroupID)
 
 -- populate allow changes
    EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId,@allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT  
-- end of allow changes

   
	SELECT  @revisiontypeid=fkid_SalesEstimate_RevisionType
    FROM    tbl_SalesEstimate_EstimateHeader
    WHERE   id_SalesEstimate_EstimateHeader=@revisionid
    
    DECLARE @mainRevisionId INT --Revision that defines Area, Group, Product Sort Order and selected Promotion Packs 
    SET @mainRevisionId = @revisionid
    
    IF (@revisiontypeid IN (7,8,9,10,11,12,21,22)) --Studio M revisions
    BEGIN
		SET @mainRevisionId = (SELECT TOP 1 id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 6 AND fkidEstimate = @estimateid)
    END
    
    SELECT te.*, ed.areaid, ed.groupid,ed.ProductID
    INTO   #tempMRSED
    FROM   tbl_SalesEstimate_EstimateDetails te
    INNER JOIN EstimateDetails ed ON te.fkidEstimateDetails=ed.EstimateDetailsID
    WHERE  fkid_SalesEstimate_EstimateHeader=@mainRevisionId
    
    DECLARE @tempQuantity TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempPrice TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempAccepted TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempAreaId TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempGroupId TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempPriceDisplayCodeId TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempSiteWork TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempProductDescription TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempAdditionalNotes TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempExtraDescription TABLE (id INT, data VARCHAR(MAX))
    DECLARE @tempInternalDescription TABLE (id INT, data VARCHAR(MAX))
    DECLARE @allSelectedPromotions TABLE (multiplePromotionId INT)
    DECLARE @allPromotionProducts TABLE (estimateDetailsId BIGINT)
 
 	BEGIN TRY
		BEGIN TRANSACTION	   
			IF @estimatedetailsidstring <> ''
			BEGIN
				SELECT	data , IDENTITY(INT) AS t1_id , 0 AS areasortorder, 0 AS groupsortorder, 0 AS productsortorder, 0 AS promotionproduct
				INTO	#temp
				FROM	dbo.split_string_to_table_by_multiple_characters(@estimatedetailsidstring,'#$#!')
				
				SELECT	data , IDENTITY(INT) AS t2_id 
				INTO	#tempderiveflag
				FROM	dbo.split_string_to_table_by_multiple_characters(@derivedcoststring,'#$#!')
				
				SELECT	data , IDENTITY(INT) AS t3_id 
				INTO	#tempcost
				FROM	dbo.split_string_to_table_by_multiple_characters(@costexcgststring,'#$#!')
				
				INSERT INTO	@tempQuantity SELECT outputid, data
				FROM	dbo.split_string_to_table_by_multiple_characters(@quantitystring,'#$#!')
											
				INSERT INTO	@tempPrice SELECT outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@pricestring,'#$#!')
				
				INSERT INTO	@tempAccepted SELECT outputid, data
				FROM	dbo.split_string_to_table_by_multiple_characters(@isacceptedstring,'#$#!')
				
				INSERT INTO	@tempAreaId SELECT outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@areaidstring,'#$#!')
				
				INSERT INTO	@tempGroupId SELECT	outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@groupidstring,'#$#!')
				
				INSERT INTO	@tempPriceDisplayCodeId SELECT outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@pricedisplaycodestring,'#$#!')
				 
				INSERT INTO	@tempSiteWork SELECT outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@issiteworkstring,'#$#!')	
				
				INSERT INTO	@tempProductDescription SELECT outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@productdescriptionstring,'#$#!')
				 
				INSERT INTO	@tempAdditionalNotes SELECT	outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@additionalnotestring,'#$#!')
				
				INSERT INTO	@tempExtraDescription SELECT outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@extradescriptionstring,'#$#!')
				
				INSERT INTO	@tempInternalDescription SELECT	outputid, data 
				FROM	dbo.split_string_to_table_by_multiple_characters(@internaldescriptionstring,'#$#!')																						

	-- get sortorder for details. if the area/group exists in the current MRS estimatedetails, use the sortorder from MRS estimate, if not exists then use the area/group/product sort order
                DECLARE @idx INT, @total INT
                SELECT @total=COUNT(*) FROM #temp
                SET @idx=1
				WHILE @idx<=@total
					BEGIN
                       SELECT 
                            @areaid = CAST(a.data AS INT),
                            @groupid= CAST(g.data AS INT),
                            @productID=ProductID 
                       FROM #temp t1
                       INNER JOIN EstimateDetails ed ON t1.data=ed.EstimateDetailsID
                       INNER JOIN @tempAreaId a ON t1.t1_id = a.id
                       INNER JOIN @tempGroupId g ON t1.t1_id = g.id
                       WHERE t1.t1_id=@idx
                       
                       IF(EXISTS(SELECT * FROM tblMultiplePromotion WHERE fkidPromotionID=@promotionid AND BaseProductID=@productID))
                          BEGIN
                             INSERT INTO #tempmultiplepromotion
                             SELECT idMultiplePromotion 
                             FROM tblMultiplePromotion 
                             WHERE fkidPromotionID=@promotionid AND BaseProductID=@productID
                          END
                       
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
                       
                       UPDATE #temp
                       SET areasortorder=CASE WHEN @areaid<> 43 THEN ISNULL(@areasortorder,0) ELSE 0 END,
                           groupsortorder=CASE WHEN @areaid<> 43 THEN ISNULL(@groupsortorder,0) ELSE 0 END,
                           productsortorder=ISNULL(@productsortorder,0)
                       WHERE t1_id=@idx
                       
                       SET @idx=@idx+1
					END
                 
					DELETE FROM #tempmultiplepromotion -- remove the dummy record
					WHERE fkidmultiplepromotion=0
                
					--Update Promotion Product Flag
					INSERT INTO @allSelectedPromotions SELECT fkidmultiplepromotion FROM #tempmultiplepromotion

					INSERT INTO @allSelectedPromotions
					SELECT idMultiplePromotion 
					FROM tblMultiplePromotion 
					WHERE fkidPromotionID=@promotionid AND BaseProductID IN (SELECT ProductID FROM #tempMRSED)

					INSERT INTO @allPromotionProducts
					SELECT EstimateDetailsId FROM #originaltemped A 
					INNER JOIN PromotionProduct B ON A.ProductAreaGroupId = B.PagID
					WHERE B.fkidMultiplePromotion IN (SELECT multiplePromotionId FROM @allSelectedPromotions)
					
					IF(EXISTS(SELECT * FROM @allPromotionProducts)) 
					BEGIN
						UPDATE #temp SET promotionproduct = 1 WHERE CAST(data AS BIGINT) IN (SELECT estimateDetailsId FROM @allPromotionProducts)
					END

	-- end of sortorder
	
				IF (@action='OPTIONTREE')
					BEGIN
						INSERT INTO tbl_SalesEstimate_EstimateDetails 
						(
							fkid_SalesEstimate_EstimateHeader, 
							fkidEstimateDetails, 
							ItemPrice, 
							Quantity,
							ProductDescription, 
							ExtraDescription,
							AdditionalInfo,
							InternalDescription, 
							CreatedOn, 
							CreatedBy, 
							ModifiedOn, 
							ModifiedBy, 
							studiomattributes,
							ItemAccepted,
							changed,
							isSitework,
							DerivedCost,
							CostExcGST,
							SalesEstimatorAccepted,
							AreaSortOrder,
							GroupSortOrder,
							ProductSortOrder
						   ,[fkidArea]
						   ,[AreaName]
						   ,[fkidGroup]
						   ,[GroupName]
						   ,fkid_NonStandardArea
						   ,fkid_NonStandardGroup
						   ,[fkidProductAreaGroup]
						   ,[ProductName]
						   ,[IsPromotionProduct]
						   ,fkid_NonStandardPriceDisplayCode							
						)
						SELECT DISTINCT
							@revisionId, 
							EstimateDetailsID, 
							CAST(tPrice.data AS DECIMAL(18,2)),
							CAST(tQty.data AS DECIMAL(18,2)),
							tProdDesc.data, 
							tExtraDesc.data,
							tAddNote.data,
							tInDesc.data, 
							GETDATE(), 
							@userId, 
							GETDATE(), 
							@userId, 
							@studiomanswer,
							CASE WHEN tAccepted.data='1' AND @revisiontypeid = 2 THEN 1 ELSE 0 END, --STS Accepted
							1,
							CAST(tSiteWork.data AS BIT),
							t2.data,
							CAST(t3.data AS DECIMAL(18,2)),
							CASE WHEN tAccepted.data='1' AND @revisiontypeid <> 2 THEN 1 ELSE 0 END, --SE Accepted
							t1.areasortorder,
							t1.groupsortorder,
							t1.productsortorder,
							ed.areaid,
							CASE WHEN ed.areaid=43 THEN A.AreaName ELSE ed.AreaName END,
							ed.groupid,
							CASE WHEN ed.areaid=43 THEN G.GroupName ELSE ed.GroupName END,
							CASE WHEN ed.areaid=43 THEN A.AreaID ELSE NULL END,
							CASE WHEN ed.areaid=43 THEN G.GroupID ELSE NULL END,
							ed.ProductAreaGroupID,
							ed.ProductName,
							t1.PromotionProduct,
							CAST(tPriceDisplay.data AS INT)
						FROM EstimateDetails ed
						INNER JOIN ProductAreaGroup pag ON ed.ProductAreaGroupID=pag.ProductAreaGroupID 
						INNER JOIN #temp t1 ON ed.EstimateDetailsID=t1.data
						INNER JOIN #tempderiveflag t2 ON t1.t1_id=t2.t2_id
						LEFT JOIN #tempcost t3 ON t1.t1_id=t3.t3_id
						INNER JOIN @tempQuantity tQty ON t1.t1_id = tQty.id
						INNER JOIN @tempPrice tPrice ON t1.t1_id = tPrice.id
						INNER JOIN @tempAccepted tAccepted ON t1.t1_id = tAccepted.id
						INNER JOIN @tempAreaId tArea ON t1.t1_id = tArea.id
						INNER JOIN @tempGroupId tGroup ON t1.t1_id = tGroup.id
						INNER JOIN @tempPriceDisplayCodeId tPriceDisplay ON t1.t1_id = tPriceDisplay.id
						INNER JOIN @tempSiteWork tSiteWork ON t1.t1_id = tSiteWork.id
						INNER JOIN @tempProductDescription tProdDesc ON t1.t1_id = tProdDesc.id
						INNER JOIN @tempAdditionalNotes tAddNote ON t1.t1_id = tAddNote.id
						INNER JOIN @tempExtraDescription tExtraDesc ON t1.t1_id = tExtraDesc.id
						INNER JOIN @tempInternalDescription tInDesc ON t1.t1_id = tInDesc.id
						INNER JOIN Area A ON A.AreaID = CAST(tArea.data AS INT)
						INNER JOIN [Group] G ON G.GroupID = CAST(tGroup.data AS INT)
						--WHERE EstimateDetailsID IN (SELECT data FROM #temp)

                        IF(EXISTS(SELECT * FROM #tempmultiplepromotion)) -- if inserted item is a master promotion item, then bring in all promotion products
                            BEGIN
                               SELECT AreaSortOrder, fkidArea 
                               INTO   #areasortorder
                               FROM   tbl_SalesEstimate_EstimateDetails
                               WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid
                               GROUP BY fkidArea,AreaSortOrder
                               
                               SELECT GroupSortOrder, fkidGroup 
                               INTO   #groupsortorder
                               FROM   tbl_SalesEstimate_EstimateDetails
                               WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid
                               GROUP BY fkidGroup ,GroupSortOrder                        
                           
                               SELECT 
                                    @revisionid as revisionid,
                                    ed.estimatedetailsid,
                                    ed.sellprice,
                                    ed.quantity,
                                    ed.ProductDescription,
                                    ed.enterdesc,
                                    GETDATE() as createdon,
                                    @userid as createdby,
                                    NULL as modifiedon,
                                    NULL as modifiedby,
                                    0 as fkidStandardInclusions,
                                    ed.additionalinfo,
                                    '' as studiomattributes,
                                    0 as accepted,
                                    1 as changed,
                                    pag.IsSiteWork,
                                    0 as DerivedCost ,
                                    0 as CostExcGST,
                                    0 as SalesEstimatorAccepted,
                                    CASE WHEN @storey=1 THEN a.SortOrder ELSE a.SortOrderDouble END as areasortorder,
                                    g.SortOrder as groupsortorder,
                                    pt.SortOrder as productsortorder,
                                    ed.areaid,
                                    ed.areaname,
                                    ed.groupid,
                                    ed.groupname,
                                    ed.productareagroupid,
                                    ed.productname,
                                    1 as promotionproduct
                               INTO   #insertpromotionproducts
                               FROM   #tempmultiplepromotion tm
                               INNER JOIN PromotionProduct pp ON tm.fkidmultiplepromotion=pp.fkidMultiplePromotion
                               INNER JOIN #originaltemped ed ON pp.PagID=ed.productareagroupid 
                               INNER JOIN ProductAreaGroup pag ON pp.PagID=pag.productareagroupid
                               INNER JOIN Area a ON pag.AreaID=a.AreaID
                               INNER JOIN [Group] g ON pag.GroupID=g.GroupID
                               INNER JOIN product pt on pag.ProductID=pt.ProductID
                               
                               UPDATE #insertpromotionproducts
                               SET    areasortorder=ao.AreaSortOrder
                               FROM   #insertpromotionproducts so
                               INNER JOIN #areasortorder ao ON so.areaid=ao.fkidarea
                               
                               UPDATE #insertpromotionproducts
                               SET    groupsortorder=gd.GroupSortOrder
                               FROM   #insertpromotionproducts so
                               INNER JOIN #groupsortorder gd ON so.groupid=gd.fkidgroup  
                               
                               UPDATE t1 
                               SET t1.IsPromotionProduct = 1
                               FROM tbl_SalesEstimate_EstimateDetails t1 
                               INNER JOIN #insertpromotionproducts t2 
                               ON t1.fkidEstimateDetails = t2.estimatedetailsid 
                               WHERE t1.fkid_SalesEstimate_EstimateHeader = @revisionid
                               
                               INSERT INTO tbl_SalesEstimate_EstimateDetails 
										(
											fkid_SalesEstimate_EstimateHeader, 
											fkidEstimateDetails, 
											ItemPrice, 
											Quantity,
											ProductDescription, 
											ExtraDescription, 
											CreatedOn, 
											CreatedBy, 
											ModifiedOn, 
											ModifiedBy, 
											fkidStandardInclusions, 
											additionalinfo,
											studiomattributes,
											ItemAccepted,
											changed,
											isSitework,
											DerivedCost,
											CostExcGST,
											SalesEstimatorAccepted,
											AreaSortOrder,
											GroupSortOrder,
											ProductSortOrder
										   ,[fkidArea]
										   ,[AreaName]
										   ,[fkidGroup]
										   ,[GroupName]
										   ,[fkidProductAreaGroup]
										   ,[ProductName]
										   ,[IsPromotionProduct]							
										)    
							   SELECT so.* FROM  #insertpromotionproducts so
							   LEFT JOIN   (  SELECT * FROM   tbl_SalesEstimate_EstimateDetails WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid)  ed ON so.estimatedetailsid=ed.fkidEstimateDetails
							   WHERE ed.fkidEstimateDetails IS NULL                   
                               
							   DELETE FROM tbl_SalesEstimate_RemovedItems WHERE fkidRevision = @revisionid AND fkidEstimateDetails IN (SELECT estimatedetailsid FROM #insertpromotionproducts)                               
                               
                            END


						-- SELECT id_SalesEstimate_EstimateDetails, fkidEstimateDetails, 0 AS fkidStandardInclusions FROM tbl_SalesEstimate_EstimateDetails WHERE fkidEstimateDetails IN (SELECT data FROM #temp) AND fkid_SalesEstimate_EstimateHeader=@revisionid
						

SELECT ed.*, ed2.DerivedCost, ed2.CostExcGST, ed2.id_SalesEstimate_EstimateDetails INTO #temped2 
FROM estimatedetails ed WITH (NOLOCK)
INNER JOIN tbl_SalesEstimate_EstimateDetails ed2 WITH (NOLOCK) ON ed.EstimateDetailsID=ed2.fkidEstimateDetails
WHERE estimateid=@estimateid and EstimateDetailsID IN (SELECT data FROM #temp) 

SELECT DISTINCT  t2.*,
     CASE WHEN d.homeid IS NOT NULL 
          THEN h.homename+' - Display at '+d.suburb
          ELSE ''
          END 
     AS displayAt 
INTO #temped
FROM #temped2 t2
LEFT JOIN display d ON t2.homedisplayid=d.homeid
LEFT JOIN home h ON d.homeid=h.homeid	  

-- update nonstandard option make sure always got qty=1 and price is 0 and extra desc is subject to builder acceptence. because this is for new options
UPDATE #temped
SET quantity=1, sellprice=0, totalprice=0, enterdesc='Subject to builder acceptence.',DerivedCost=0,CostExcGST=null
WHERE  areaid=43

SELECT DISTINCT
    @revisionid AS revisionid, 
    @userid AS CreatedBy,
	ed.areaid,
	CASE WHEN a.fkidArea = 43 
         THEN 'Non Standard Request'+char(13)+'['+a.AreaName+' - '+ a.GroupName+']'
         ELSE a.AreaName
    END
    AS AreaName,
	ed.sortorder,
    CASE WHEN a.fkidArea = 43 
         THEN ed.GroupName
         ELSE a.GroupName
    END		
	AS GroupName,
	ed.groupid,
	ed.productid, 
	CASE WHEN ed.displayAt<>''
	   THEN ed.productname+'['+ed.productid+']'+char(13)+ed.displayAt
	   ELSE ed.productname+'['+ed.productid+']' 
	END
	AS productname, 
	CAST(isnull(a.Quantity,1) AS DECIMAL(18,2)) AS quantity,
	CAST(isNULL(a.ItemPrice,0) AS DECIMAL(18,2)) AS sellprice, 
	selected, 
	estimatedetailsid, 
	0 AS idStandardInclusions, 
	a.IsPromotionProduct AS promotionproduct,
	standardoption,
	a.productdescription, 
	a.ExtraDescription AS enterdesc,
	a.AdditionalInfo,
	a.InternalDescription,
	CASE WHEN @revisiontypeid = 2 THEN a.ItemAccepted ELSE a.SalesEstimatorAccepted END AS itemAccepted,
	ISNULL(p.isstudiomproduct,0) AS isstudiomproduct, 
	pag.productareagroupid,
	CAST(p.studiomqanda AS VARCHAR(MAX)) AS studiomquestion,0 AS imagecount,
	p.uom,
	ISNULL(g.StudioMSortOrder, 9999999) AS StudioMSortOrder,
	CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
		 THEN CAST(1 AS BIT)
		 ELSE CAST(0 AS BIT)
	END AS qandamandatory,
	'./images/upgrade.png' AS SOSI,
	'Upgrade Option.' AS SOSIToolTips,
	CASE WHEN p.isstudiomproduct=1 AND p.studiomqanda IS NOT NULL AND CAST(p.studiomqanda AS VARCHAR(MAX))<>''
     THEN
		CASE WHEN a.studiomattributes IS NOT NULL AND CAST(a.studiomattributes AS VARCHAR(MAX))<>''
		    THEN './images/green_box.png'  
		ELSE    
			CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
			THEN './images/color_swatch.png'
			ELSE './images/color_swatch_gray.png'
			END		              
        END
     ELSE 
        CASE WHEN p.isstudiomproduct=0
             THEN ''
             ELSE './images/green_box.png'
        END	        
	END AS StudioMIcon,
	CASE WHEN p.isstudiomproduct=1 AND p.studiomqanda IS NOT NULL AND CAST(p.studiomqanda AS VARCHAR(MAX))<>''
     THEN 
        CASE WHEN a.studiomattributes IS NOT NULL AND CAST(a.studiomattributes AS VARCHAR(MAX))<>''
			THEN 'Studio M Product. Question answered.'
		ELSE 
			CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
			THEN 'Studio M Product. Question not answered yet.'
			ELSE 'Studio M Product. Answers are not mandatory.'
			END
		END	 
     ELSE 
        CASE WHEN p.isstudiomproduct=0
             THEN ''
             ELSE 'Studio M Product. Question answered.'
        END	 		     
	END AS StudioMTooltips,
	
	CASE WHEN @allowchangeqty=1 
		 THEN 1
		 ELSE	
			 CASE WHEN (um.[Description]='NOTE')
			 THEN 0
			 ELSE 1   
			 END        
	END 		
	AS ChangeQty,

	CASE WHEN @allowchangeprice=1
	  THEN 1
	  ELSE			
			 CASE WHEN (um.[Description]='NOTE')
			 THEN 0
			 ELSE CASE WHEN a.fkidArea=43
					   THEN 1
					   ELSE ed.ChangePrice
				  END
			 END
	END     
	AS ChangePrice,

	CASE WHEN @revisiontypeid IN (4,15, 19, 25) 
	     THEN @allowchangeDesc 
	     ELSE CASE WHEN a.fkidArea=43
	               THEN 1
	               ELSE 0
	     END
	END
	AS changeproductstandarddescription,
	
	CASE WHEN @revisiontypeid IN (4,15, 19, 25) 
	     THEN @allowchangePriceDisplayCode 
	     ELSE 0
	END
	AS changedisplaycode,
	
	a.id_SalesEstimate_EstimateDetails,
	CAST(a.StudioMAttributes AS VARCHAR(MAX)) AS StudioMAttributes,
	ISNULL(pdc.PriceDisplayCodeID, 10) AS PriceDisplayCodeID,
	ISNULL(pdc.PriceDisplayCode + ' - ' + pdc.PriceDisplayDesc,'NONE - NONE') AS PriceDisplayDesc,
	a.IsSiteWork AS siteworkitem,
	ed.derivedcost,
	CASE WHEN ISNULL(ed.derivedcost,0)=1
	     THEN './images/link.png'
	     ELSE './images/spacer.gif'
	END AS DerivedCostIcon,
	CASE WHEN ISNULL(ed.derivedcost,0)=1
	     THEN 'Derived cost.'
	     ELSE ''
	END AS DerivedCostTooltips,	
	CAST(a.CostExcGST AS DECIMAL(18,2)) AS costexcgst,
	CASE WHEN a.ItemPrice<>0
	     THEN CAST(100*((a.ItemPrice/1.1)-a.CostExcGST)/(a.ItemPrice/1.1) AS DECIMAL(18,2)) 
	     ELSE 0
	END AS margin,
	CAST('NEW' AS VARCHAR(100)) AS changetype,	
	CASE WHEN mp.BaseProductID IS NOT NULL 
	     THEN CAST(1 AS BIT)
	     ELSE CAST(0 AS BIT)
	END AS ismasterpromotion,
	ISNULL(a.fkid_NonStandardArea, 0) AS NonstandardCategoryID,
	ISNULL(a.fkid_NonStandardGroup, 0) AS NonstandardGroupID,
	CASE WHEN (mp.BaseProductID IS NOT NULL AND (@revisiontypeid NOT IN (2,4)))
		THEN CAST(0 AS BIT)
		ELSE CAST(1 AS BIT)
	END AS allowtoremove,
	CAST(0 AS BIT) AS PreviousChanged,
	CAST(0 AS BIT) AS changed,
	NULL AS selectedimageid			  
INTO #tempoption
FROM #temped ed
INNER JOIN productareagroup pag	ON ed.productareagroupid= pag.productareagroupid
INNER JOIN product p ON pag.productid=p.productid
INNER JOIN tblUOM um ON p.UOM=um.Code
INNER JOIN [group] g ON pag.groupid=g.groupid
INNER JOIN (SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader=@revisionid) a ON ed.estimatedetailsid=a.fkidEstimateDetails
LEFT JOIN tblPriceDisplayCode pdc ON a.fkid_NonStandardPriceDisplayCode = pdc.PriceDisplayCodeID
LEFT JOIN tblMultiplePromotion mp ON ED.ProductID=mp.BaseProductID AND ED.PromotionProduct=0 and fkidPromotionID=@promotionid
    
ORDER BY areaname, groupname, productname


SELECT * 
INTO #tempfinal 
FROM #tempoption
ORDER BY 	areaname, groupname, productname

SELECT DISTINCT productid INTO #temp2 FROM #tempfinal

SELECT 
			t.productid,
			COUNT(productid)  AS imagecount
INTO		#imagecount
FROM		#temp2 t
INNER JOIN	tbl_StudioM_ProductImage pim ON t.productid=pim.fkidproduct
GROUP BY t.productid    


UPDATE		#tempfinal
SET			imagecount=i.imagecount
FROM		#tempfinal t
INNER JOIN  #imagecount i ON t.productid=i.productid							
						
SELECT f.*  
FROM #tempfinal f
INNER JOIN ProductAreaGroup PAG ON f.productareagroupid=PAG.productareagroupid						
				
DROP TABLE #temped
DROP TABLE #temped2
DROP TABLE #tempfinal
DROP TABLE #imagecount
DROP TABLE #temp2
DROP TABLE #tempoption
DROP TABLE #tempMRSED			

						--Do not remove NSR from the removed item log as a new NSR may not be the same as the deleted one
						DELETE FROM tbl_SalesEstimate_RemovedItems WHERE fkidRevision = @revisionid AND fkidEstimateDetails IN (SELECT data FROM #temp)
						AND (NOT EXISTS (SELECT * FROM EstimateDetails WHERE EstimateDetailsID = fkidEstimateDetails AND areaid = 43))
						
		           END
		        ELSE IF (@action='STUDIOM_ANSWER')
		           BEGIN
		                UPDATE tbl_SalesEstimate_EstimateDetails
		                SET    StudioMAttributes=@studiomanswer
                        WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid 
                               AND fkidEstimateDetails IN (SELECT data FROM #temp)            
		           END
				
				DROP TABLE #temp
			END

			
			UPDATE tbl_SalesEstimate_EstimateHeader
			SET ModifiedBy = @userId, ModifiedOn = GETDATE()
			WHERE id_SalesEstimate_EstimateHeader = @revisionId
			
			COMMIT

	END TRY

	BEGIN CATCH

		IF @@TRANCOUNT > 0
			ROLLBACK
		
		-- Raise an error
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)

	END CATCH

	SET NOCOUNT OFF;
END


GO