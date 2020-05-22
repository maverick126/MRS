----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetPagByID]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetPagByID]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetPagByID]
	@revisionId int		,
	@estimatedetailsid int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @len	INT, @promotionid INT, @derivedcost INT, @costprice DECIMAL(18,2), @regionid INT, @brandid INT
	DECLARE @productid VARCHAR(50), @sellprice DECIMAL(18,2), @depositdate DATETIME, @estimateid INT, @revisontypeid INT
	DECLARE @derivedhomepercentage DECIMAL(18,4), @deriveditempercentage DECIMAL(18,4), @targetmargin DECIMAL(18,4)
	DECLARE @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT
	
	SET @len=100
   
    SELECT      @estimateid=e.EstimateID,
		        @promotionid=e.PromotionID,
		        @regionid=e.RegionID,
		        @brandid=h.BrandID,
		        @revisontypeid=eh.fkid_SalesEstimate_RevisionType,
		        @depositdate=dp.DepositDate
    FROM		tbl_SalesEstimate_EstimateHeader eh
    INNER JOIN	estimate e	ON eh.fkidestimate=e.estimateid
    INNER JOIN  Home h ON e.HomeID=h.homeid
    INNER JOIN  DepositDetails dp ON e.fkidOpportunity=dp.fkidOpportunity
    WHERE id_SalesEstimate_EstimateHeader=@revisionId   
 
    EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
     
    EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId,@allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT 
      
    SELECT @productid=productid, 
           @sellprice=SellPrice
    FROM   EstimateDetails
    WHERE  EstimateDetailsID=@estimatedetailsid
    
    SELECT 
         @derivedcost=minibillStart
    FROM product
    WHERE ProductID=@productid
    
    SELECT TOP 1 @costprice= ISNULL(CostPrice,0)
    FROM Price
    WHERE ProductID=@productid AND RegionID=@regionid AND active=1 AND EffectiveDate<=@depositdate
    ORDER BY EffectiveDate DESC
    
    IF(@costprice=0)
      BEGIN
         SET @derivedcost=1
         IF(@sellprice>=0)
            BEGIN
                 SET @costprice=CAST((@sellprice/1.1)*(1-@deriveditempercentage) AS DECIMAL(18,2))
            END
         ELSE
             BEGIN
                 SET @costprice=0
            END        
      END   
    
	SELECT 
	    sED.AreaName,
		sed.GroupName,
		SED.fkidArea AS AreaId,
		SED.fkidGroup AS GroupId, 
		ed.ProductID, 
		CASE WHEN d.homeid IS NOT NULL 
		     THEN sed.productname+char(13)+'['+ed.productid+']'+char(13)+h.homename+' - Display at '+d.suburb
		     ELSE sed.productname+char(13)+'['+ed.productid+']' 
		END AS productname,
		SED.ProductDescription, 
	    CASE  WHEN	LEN(SED.ProductDescription)>@len
	          THEN	SUBSTRING(SED.ProductDescription,1,@len)+' ...'
	          ELSE  SED.ProductDescription
	          END
	    AS ProductDescriptionShort,
		sed.ExtraDescription,
		SED.additionalinfo, 
		ISNULL(sed.InternalDescription,ed.InternalDescription) AS InternalDescription, 
		ed.UOM, 
		SED.Quantity, 
		CAST(ItemPrice AS DECIMAL(18,2)) AS ItemPrice,
		sED.IsPromotionProduct as PromotionProduct, 
		ED.StandardOption, 
		id_SalesEstimate_EstimateDetails AS EstimateRevisionDetailsId, 
		ED.EstimateDetailsId,
		0 AS idstandardinclusions,
		SED.CreatedBy,
		A.AreaName AS NonStandardAreaName,
		0	AS areaorder,
		g.sortorder AS groupsortorder,
		g.StudioMSortOrder AS studiomsortorder,		
		p.sortorder AS productsortorder,
		CASE WHEN @revisontypeid=2
		     THEN ISNULL(sed.itemaccepted,0)
		     ELSE ISNULL(sed.SalesEstimatorAccepted,0)
		END	AS itemaccepted,
		CASE WHEN sed.fkidArea<>43 AND sed.fkidarea<>1 --not nonstandard and surcharge
		     THEN 0
		     ELSE ISNULL(ISNULL(sed.fkid_NonStandardArea,ed.nonstandardcatid) ,0)
		END	AS nonstandardcategoryid,
		ISNULL(sed.fkid_NonStandardGroup,0) AS nonstandardgroupid,		
		ISNULL(p.isstudiomproduct,0) AS isstudiomproduct,
		0 AS imagecount,
		p.studiomqanda as studiomquestion,
		sed.studiomattributes AS studiomanswer,
		sed.SelectedImageID,
		CAST(0 AS BIT) AS siteworkitem,
		'./images/upgrade.png' AS SOSI,
		'Upgrade Option.' AS SOSIToolTips,
		CASE WHEN p.isstudiomproduct=1 AND p.studiomqanda IS NOT NULL AND CAST(p.studiomqanda AS VARCHAR(MAX))<>''
		     THEN 
		         CASE WHEN sed.studiomattributes IS NOT NULL AND CAST(sed.studiomattributes AS VARCHAR(MAX))<>''
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
		         CASE WHEN sed.studiomattributes IS NOT NULL AND CAST(sed.studiomattributes AS VARCHAR(MAX))<>''
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
		CAST(0 AS BIT) AS 	changed,
		CAST(0 AS BIT) AS 	previouschanged,
		CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
		     THEN CAST(1 AS BIT)
		     ELSE CAST(0 AS BIT)
		END AS qandamandatory,
		--ED.ChangeQty,
		--ED.ChangePrice,
		@allowchangeqty AS ChangeQty,
		@allowchangeprice AS ChangePrice,
		@allowchangePriceDisplayCode AS changedisplaycode,
		@allowchangeDesc AS changeproductstandarddescription,
		CASE WHEN mp.BaseProductID IS NOT NULL 
		     THEN CAST(0 AS BIT)
		     ELSE CAST(1 AS BIT)
		END AS allowtoremove,
		@derivedcost AS derivedcost,
		@costprice AS costexcgst
	FROM tbl_SalesEstimate_EstimateDetails SED
	INNER JOIN EstimateDetails ED	ON SED.fkidEstimateDetails = ED.EstimateDetailsID
	INNER JOIN area a2				ON ed.areaid=a2.areaid
	LEFT OUTER JOIN Area A			ON A.AreaID = SED.fkid_NonStandardArea
	LEFT JOIN [Group] g				ON ed.groupid=g.groupid
	LEFT JOIN [Product] p			ON ed.productid=p.productid   
    LEFT JOIN display d ON ed.homedisplayid=d.homeid
    LEFT JOIN home h ON d.homeid=h.homeid
    LEFT JOIN tblMultiplePromotion mp ON ED.ProductID=mp.BaseProductID AND ED.PromotionProduct=0 and fkidPromotionID=@promotionid
	WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND ED.EstimateDetailsID=@estimatedetailsid
	


END

GO