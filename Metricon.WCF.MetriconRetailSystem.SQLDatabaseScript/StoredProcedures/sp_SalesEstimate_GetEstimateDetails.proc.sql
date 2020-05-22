/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the problem to drop the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateDetails]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateDetails]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateDetails]
	@revisionId int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @len	INT, @brandid INT, @promotionid INT, @stateid INT, @contractNumber INT
    DECLARE @storey	INT, @revisiontypeid INT, @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT
    DECLARE @siteworkarea TABLE
    (productareagroupid INT)
    
    -- get sitework areaid this will determine which product is showing on "Site work" tab in the application
    --INSERT INTO @siteworkarea
    --SELECT productareagroupid
    --FROM   ProductAreaGroup pag   
    --INNER JOIN area a ON pag.AreaID=a.AreaID
    --INNER JOIN [Group] g ON pag.GroupID=g.GroupID
    --WHERE  a.issitework=1 AND g.IsSiteWork=1 AND pag.Active=1 AND a.Active=1 AND g.Active=1
    
    -- move sitework items to pag level
    INSERT INTO @siteworkarea
    SELECT productareagroupid
    FROM   ProductAreaGroup pag   
    WHERE  issitework=1 AND Active=1    
    
    -- get some variables
    
    SET @len=100
    
    SELECT  
		@storey=h.stories, 
		@brandid=h.brandid, 
		@stateid=h.fkStateID,
		@promotionid=e.PromotionID,
		@revisiontypeid=eh.fkid_SalesEstimate_RevisionType,
		@contractNumber=e.BCContractNumber
    FROM		tbl_SalesEstimate_EstimateHeader eh
    INNER JOIN	estimate e	ON eh.fkidestimate=e.estimateid
    INNER JOIN	home h		ON e.homeid=h.homeid
    WHERE id_SalesEstimate_EstimateHeader=@revisionId


-- populate allow changes
	EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId,@allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT  
-- end of allow changes

	SELECT 
	    @revisionId AS revisionId,
	    sed.id_SalesEstimate_EstimateDetails,
	    SED.fkidArea AS AreaId,
	    SED.fkidGroup AS GroupId,
	    CASE WHEN SED.fkidArea = 43 
	         THEN 'Non Standard Request'+char(13)+'['+SED.AreaName+' - '+ SED.GroupName+']'
	         ELSE SED.AreaName
	    END
	    AS AreaName,
	    CASE WHEN SED.fkidArea = 43 
	         THEN g.GroupName
	         ELSE SED.GroupName
	    END		
		AS GroupName, 
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
		sed.ExtraDescription AS enterdesc,
		SED.additionalinfo, 
		ISNULL(sed.InternalDescription,ed.InternalDescription) AS InternalDescription, 
		ed.UOM, 
		SED.Quantity, 
		CAST(ItemPrice AS DECIMAL(18,2)) AS ItemPrice,
		CAST(ItemPrice AS DECIMAL(18,2)) AS SellPrice,
		ISNULL(sED.IsPromotionProduct,0) as PromotionProduct, 
		ED.StandardOption, 
		id_SalesEstimate_EstimateDetails AS EstimateRevisionDetailsId, 
		ED.EstimateDetailsId,
		0 AS idstandardinclusions,
		SED.CreatedBy,
		A.AreaName AS NonStandardAreaName,
		grp.GroupName AS NonStandardGroupName,
		CASE WHEN A.AreaID IS NULL THEN
		CASE	WHEN @storey=1
				THEN a2.sortorder
				ELSE a2.sortorderdouble END
		ELSE
				CASE WHEN @storey=1
				THEN A.SortOrder
				ELSE A.SortOrderDouble END
		END	AS areaorder,
		CASE WHEN @storey=1
		THEN a2.SortOrder
		ELSE a2.SortOrderDouble END AS sortorder,		
		g.sortorder AS groupsortorder,		
		ISNULL(g.StudioMSortOrder, 9999999) AS StudioMSortOrder,	
		p.sortorder AS productsortorder,
		CASE WHEN @revisiontypeid=2
		     THEN ISNULL(sed.itemaccepted,0)
		     ELSE ISNULL(sed.SalesEstimatorAccepted,0)
		END AS itemaccepted,
		CASE WHEN ed.areaid<>43 AND ed.areaid<>1 --not nonstandard and surcharge
		     THEN 0
		     ELSE ISNULL(ISNULL(sed.fkid_NonStandardArea,ed.nonstandardcatid) ,0)
		END	AS nonstandardcategoryid,
		ISNULL(sed.fkid_NonStandardGroup,0) AS nonstandardgroupid,		
		ISNULL(p.isstudiomproduct,0) AS isstudiomproduct,
		0 AS imagecount,
		p.studiomqanda as studiomquestion,
		sed.studiomattributes AS studiomanswer,
		sed.studiomattributes,
		sed.SelectedImageID,
		ISNULL(SED.issitework,0) AS siteworkitem,
		--CASE WHEN sa.ProductAreaGroupID IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS siteworkitem,
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
		             ELSE 'There is no studio M questions.'
		        END	 		     
		END AS StudioMTooltips,
		ISNULL(SED.changed, CAST(0 AS BIT)) AS 	changed,
		ISNULL(SED.previouschanged, CAST(0 AS BIT)) AS 	previouschanged,
		CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
		     THEN CAST(1 AS BIT)
		     ELSE CAST(0 AS BIT)
		END AS qandamandatory,
		
		CASE WHEN @allowchangeqty=1 
			 THEN 1
			 ELSE	
				 CASE WHEN uo.code='NT'
				 THEN 0
				 ELSE 1   
				 END        
		END 		
		AS ChangeQty,

		CASE WHEN @allowchangeprice=1
		  THEN 1
		  ELSE			
				 CASE WHEN (uo.code='NT')
				 THEN 0
				 ELSE CASE WHEN ed.areaid=43
						   THEN 1
						   ELSE ED.ChangePrice
						   --ELSE CASE WHEN sa.productareagroupid IS NOT NULL -- that means site work item
						   --          THEN ED.ChangePrice -- site work items is able to change based on admin settings
						   --          ELSE 0
						   --     END
					  END
				 END
		END     
		AS ChangePrice,

		CASE WHEN @revisiontypeid IN (4,15, 19, 25) 
		     THEN @allowchangeDesc 
		     ELSE CASE WHEN ED.areaid=43
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
		
		CASE WHEN (mp.BaseProductID IS NOT NULL AND (@revisiontypeid NOT IN (2,4)))
		     THEN CAST(0 AS BIT)
		     ELSE CAST(1 AS BIT)
		END AS allowtoremove,
		CASE WHEN nsrpdc.PriceDisplayCodeID IS NULL
			THEN ISNULL(pdc.PriceDisplayCode + ' - ' + pdc.PriceDisplayDesc,'NONE - NONE') 
			ELSE ISNULL(nsrpdc.PriceDisplayCode + ' - ' + nsrpdc.PriceDisplayDesc,'NONE - NONE')
		END	AS PriceDisplayDesc,
        --ISNULL(nsrpdc.PriceDisplayCode + ' - ' + nsrpdc.PriceDisplayDesc,'NONE - NONE') AS PriceDisplayDesc,		
		--ISNULL(nsrpdc.PriceDisplayCodeID, 10) AS PriceDisplayCodeID,		
		CASE WHEN nsrpdc.PriceDisplayCodeID IS NULL
			THEN ISNULL(pdc.PriceDisplayCodeID, 10) 
			ELSE ISNULL(nsrpdc.PriceDisplayCodeID, 10)
		END AS PriceDisplayCodeID,
		CASE WHEN SED.CostExcGST IS NOT NULL
		     THEN CAST(ISNULL(CostExcGST,0) AS DECIMAL(18,2)) 
		     ELSE NULL 
		END AS CostExcGST,
		ISNULL(DerivedCost,0) AS DerivedCost,
		CASE WHEN ISNULL(DerivedCost,0)=1
		     THEN './images/link.png'
		     ELSE './images/spacer.gif'
		END AS DerivedCostIcon,
		CASE WHEN ISNULL(DerivedCost,0)=1
		     THEN 'Derived cost.'
		     ELSE ''
		END AS DerivedCostTooltips,
		--CASE WHEN ED.SellPrice<>0 
		--     THEN CASE WHEN ED.SellPrice<0 AND SED.CostExcGST=0
		--               THEN CAST(-100 AS DECIMAL(18,2))
		--               ELSE CAST(ISNULL((((ED.SellPrice/1.1)-SED.CostExcGST)/(ED.SellPrice/1.1))*100, 0)	 AS DECIMAL(18,2)) 
		--          END 
		--     ELSE 0 
		--END AS margin,
		CASE WHEN SED.CostExcGST IS NOT NULL
		     THEN
					CASE WHEN SED.ItemPrice<>0 
						 THEN CAST(CAST(ISNULL((((SED.ItemPrice/1.1)-CAST(SED.CostExcGST AS DECIMAL(18,2)))/(SED.ItemPrice/1.1))*100, 0)	 AS DECIMAL(18,2))  AS VARCHAR )
						 ELSE '' 
					END 
		     ELSE ''
		END
		AS margin,		
		ISNULL(SED.CostOverWriteBy,0) AS CostOverWriteBy,
		CASE WHEN sed.CostExcGST IS NULL 
		     THEN 0 
		     ELSE CASE WHEN SED.ItemPrice<0--(ED.areaid=52 OR ED.groupid IN (286,189)) AND SED.ItemPrice<0 AND SED.CostExcGST<0
		               THEN 0
		               ELSE 1
		          END
		END AS showmargin,
		CAST('' AS VARCHAR(100)) AS changetype,
		ED.ProductAreaGroupID,
		CASE WHEN mp.BaseProductID IS NOT NULL 
		     THEN CAST(1 AS BIT)
		     ELSE CAST(0 AS BIT)
		END AS ismasterpromotion
	INTO #temp	
	FROM tbl_SalesEstimate_EstimateDetails SED
	INNER JOIN EstimateDetails ED	ON SED.fkidEstimateDetails = ED.EstimateDetailsID
	INNER JOIN area a2				ON ed.areaid=a2.areaid
	LEFT OUTER JOIN Area A			ON A.AreaID = SED.fkid_NonStandardArea
	LEFT OUTER JOIN [Group] grp		ON grp.GroupID = SED.fkid_NonStandardGroup 
	LEFT JOIN [Group] g				ON ed.groupid=g.groupid
	LEFT JOIN [Product] p			ON ed.productid=p.productid 
	LEFT JOIN [tblUOM] uo           ON RTRIM(p.uom)=RTRIM(uo.code)  
	LEFT JOIN @siteworkarea sa		ON ed.ProductAreaGroupID=sa.ProductAreaGroupID
    LEFT JOIN display d ON ed.homedisplayid=d.homeid
    LEFT JOIN home h ON d.homeid=h.homeid
    LEFT JOIN tblMultiplePromotion mp ON ED.ProductID=mp.BaseProductID AND ED.PromotionProduct=0 and fkidPromotionID=@promotionid
    LEFT JOIN tblPriceDisplayCode pdc ON p.fkPriceDisplayCodeID = pdc.PriceDisplayCodeID
    LEFT JOIN tblPriceDisplayCode nsrpdc ON ISNULL(SED.fkid_NonStandardPriceDisplayCode,0) = nsrpdc.PriceDisplayCodeID
	WHERE fkid_SalesEstimate_EstimateHeader = @revisionId
 
    UPDATE #temp
    SET    groupsortorder=g1.SortOrder
    FROM #temp t1
    INNER JOIN [Group] g1 ON t1.nonstandardgroupname=g1.groupname
    WHERE AreaName like '%non standard%'
    
	--ORDER BY AreaName, GroupName, ProductName
	--ORDER BY areaorder, groupsortorder, productsortorder

	--get photo count
	SELECT DISTINCT productid INTO #temp2 FROM #temp
	
	SELECT 
				t.productid,
				COUNT(productid)  AS imagecount
    INTO		#imagecount
	FROM		#temp2 t
	INNER JOIN	tbl_StudioM_ProductImage pim ON t.productid=pim.fkidproduct
	GROUP BY t.productid
	

    UPDATE		#temp
    SET			imagecount=i.imagecount
    FROM		#temp t
    INNER JOIN  #imagecount i ON t.productid=i.productid
 
 -- find the changes
	DECLARE @estimates TABLE (estimateId INT)
	
	INSERT INTO @estimates SELECT DISTINCT fkidEstimate FROM tbl_SalesEstimate_EstimateHeader SED INNER JOIN Estimate E
    ON SED.fkidEstimate = E.EstimateID WHERE BCContractNumber = @contractNumber
	
    DECLARE @previousrevisionid INT
    --If the current revision is one of the Studio M revisions
    IF (@revisiontypeid IN (7,8,9,10,11,12,21,22))
    BEGIN
		SELECT @previousrevisionid = MAX(id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader < @revisionId
		AND fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 6
    END
    --If the current revision is STM revision
    ELSE IF (@revisiontypeid = 23)
    BEGIN
		--If RSTM revision exists
		IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader < @revisionId
		AND fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 6)
		BEGIN
			--Do not compare with any revisions
			SET @previousrevisionid = @revisionId
		END
		ELSE
		BEGIN
			--If Studio M was skipped, compare it to the previous revision
			SELECT @previousrevisionid = MAX(id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader 
			WHERE id_SalesEstimate_EstimateHeader < @revisionId AND fkidEstimate IN (SELECT estimateId FROM @estimates)			
		END
    END
    --If the current revision is PVAR-CSC, BVAR-BSC or PSTM-CSC
    ELSE IF (@revisiontypeid IN (14,18,24))
    BEGIN
		SELECT @previousrevisionid = MAX(id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader 
		WHERE id_SalesEstimate_EstimateHeader < @revisionId AND fkidEstimate IN (SELECT estimateId FROM @estimates)
		
		--If the previous revision is rejected
		IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @previousrevisionid AND fkid_SalesEstimate_Status = 3)
		BEGIN
			--Get previous revision from the last milestone
			EXEC sp_SalesEstimate_GetPreviousRevisonForVariation @revisionId,@previousrevisionid output
		END 
    END
    ELSE
    BEGIN
		SELECT @previousrevisionid = MAX(id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader 
		WHERE id_SalesEstimate_EstimateHeader < @revisionId AND fkidEstimate IN (SELECT estimateId FROM @estimates)
	END
	
    SELECT es.fkidProductAreaGroup as ProductAreaGroupID, HomeDisplayID,es.* 
    INTO   #tempprevioused
    FROM   tbl_SalesEstimate_EstimateDetails es
    INNER JOIN EstimateDetails ed on es.fkidEstimateDetails=ed.EstimateDetailsID
    WHERE es.fkid_SalesEstimate_EstimateHeader=@previousrevisionid
    
    
    UPDATE #temp
    SET    changetype='NEW'
    FROM   #temp t1
    LEFT JOIN #tempprevioused t2 ON t1.productareagroupid=t2.productareagroupid
    WHERE t2.productareagroupid IS NULL
    
    UPDATE #temp
    SET    changetype=changetype+'QTY'
    FROM   #temp t1
    INNER JOIN #tempprevioused t2 ON t1.productareagroupid=t2.productareagroupid
    WHERE  t1.quantity<>t2.quantity
 
    UPDATE #temp
    SET    changetype=CASE WHEN changetype='' THEN 'PRC' ELSE changetype+CHAR(10)+'PRC' END
    FROM   #temp t1
    INNER JOIN #tempprevioused t2 ON t1.productareagroupid=t2.productareagroupid
    WHERE  t1.itemprice<>t2.itemprice
    
    UPDATE #temp
    SET    changetype=CASE WHEN changetype='' THEN 'DESC' ELSE changetype+CHAR(10)+'DESC' END
    FROM   #temp t1
    INNER JOIN #tempprevioused t2 ON t1.productareagroupid=t2.fkidproductareagroup
    WHERE  (t1.productdescription<>t2.productdescription) OR
           (t1.extradescription<>t2.extradescription) OR 
           (t1.additionalinfo<>t2.additionalinfo) OR 
           (t1.internaldescription<>t2.internaldescription) 

    --UPDATE #temp
    --SET    changetype=CASE WHEN changetype<>'' 
    --                       THEN SUBSTRING(changetype,2,LEN(changetype))
    --                       ELSE ''
    --                  END
    --FROM   #temp 
    --WHERE  changetype<>'NEW'      
        
  -- final out put   
    SELECT 
        *,
        ISNULL(Quantity,1)*ISNULL(itemprice,0) AS totalprice 
        ,CASE WHEN CostExcGST IS NOT NULL
             THEN ISNULL(Quantity,1)*CostExcGST
             ELSE NULL
        END AS totalCostExcGST
    
    FROM #temp
    ORDER BY areaorder,  groupsortorder, productsortorder

END

GO
 