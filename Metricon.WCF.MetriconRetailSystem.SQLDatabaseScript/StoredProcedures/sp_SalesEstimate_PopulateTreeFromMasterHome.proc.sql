----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_PopulateTreeFromMasterHome]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_PopulateTreeFromMasterHome]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER Procedure [dbo].[sp_SalesEstimate_PopulateTreeFromMasterHome]
	@regionid INT,
	@homeid   INT
AS
BEGIN

	SET NOCOUNT ON;

		DECLARE @estimateid INT, @brandid INT, @regiongroupid INT, @revisiontypeid INT, @priceregionid INT, @promotionid INT
		DECLARE @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT, @parenthomeid INT

		DECLARE @areatable TABLE
		(areaid INT)

		DECLARE @grouptable TABLE
		(groupid INT)

		SELECT @brandid=Brandid, @parenthomeid=ISNULL(ParentHomeID,0)
		FROM Home
		WHERE HomeID=@homeid

		SELECT @regiongroupid=fkidregiongroup 
		FROM tblpriceregiongroupmapping
		WHERE fkRegionID=@regionid

		INSERT INTO @areatable
		SELECT areaid
		FROM	 area
		WHERE  active=1
		  
		DECLARE @temphdo TABLE (
			HomeID BIGINT,
			ProductAreaGroupID BIGINT,
			[StandardOption] BIT,
			[GeneralOption] BIT
			,[StandardInclusion] BIT
			,[Quantity] DECIMAL(18,3)
			,[CreatedDate] DATETIME
			,[CreatedBy] VARCHAR(250)
			,[ModifiedDate] DATETIME
			,[ModifiedBy] VARCHAR(250)
			,[Active] BIT
			,[HomeDisplayID] BIGINT
			,[ChangeQty] BIT
			,[AddExtraDesc] BIT
			,[EnterDesc] VARCHAR(250)
			,[ChangePrice] BIT
			,[InternalDescription] VARCHAR(MAX)
			,[OptionID] BIGINT
			,[DisplayAt] VARCHAR(100)
		)	  
		  
		  INSERT INTO @temphdo
		  SELECT [HomeID]
				  ,[ProductAreaGroupID]
				  ,[StandardOption]
				  ,[GeneralOption]
				  ,[StandardInclusion]
				  ,[Quantity]
				  ,[CreatedDate]
				  ,[CreatedBy]
				  ,[ModifiedDate]
				  ,[ModifiedBy]
				  ,[Active]
				  ,[HomeDisplayID]
				  ,[ChangeQty]
				  ,[AddExtraDesc]
				  ,[EnterDesc]
				  ,[ChangePrice]
				  ,[InternalDescription]
				  ,[OptionID]
				  , CAST('' AS vARCHAR(100)) AS displayat
		  FROM HomeDisplayOption WITH (NOLOCK)
		  WHERE HomeID=@homeid AND Active=1
		  
		  IF(@parenthomeid>0)
		     BEGIN
		          INSERT INTO @temphdo
				  SELECT [HomeID]
					  ,[ProductAreaGroupID]
					  ,[StandardOption]
					  ,[GeneralOption]
					  ,[StandardInclusion]
					  ,[Quantity]
					  ,[CreatedDate]
					  ,[CreatedBy]
					  ,[ModifiedDate]
					  ,[ModifiedBy]
					  ,[Active]
					  ,[HomeDisplayID]
					  ,[ChangeQty]
					  ,[AddExtraDesc]
					  ,[EnterDesc]
					  ,[ChangePrice]
					  ,[InternalDescription]
					  ,[OptionID]
					  , CAST('' AS vARCHAR(100)) AS displayat
				  FROM HomeDisplayOption WITH (NOLOCK)
				  WHERE HomeID=@parenthomeid AND HomeDisplayID= @homeid AND Active=1		     
		     END
	  
		  UPDATE @temphdo
		  SET displayat=h.HomeName++' - Display at '+d.suburb
		  FROM @temphdo hdo
		  INNER JOIN Display d ON HDO.homedisplayid= d.HomeID
		  INNER JOIN Home h ON d.homeid=h.homeid
		  WHERE HDO.homedisplayid IS NOT NULL
		  
		  SELECT DISTINCT ProductID
		  INTO #tempProduct
		  FROM @temphdo hh
		  INNER JOIN ProductAreaGroup pag ON hh.ProductAreaGroupID=pag.ProductAreaGroupID
-- get price

		select PriceID,p.ProductID,SellPrice,EffectiveDate,CreatedDate,RegionID,PromotionPrice , CostPrice, DerivedCost
			into #tempPrice
			from price p 
			inner join #tempProduct tp on p.productid=tp.productid
		where p.active=1 and regionid=@regionid and effectivedate<getdate()

		select productid,max(effectivedate) as effectivedate into #price1
			from #tempPrice 
			group by productid
			having max(effectivedate)<getdate()

		select p1.productid,p1.effectivedate,max(price.createddate) as createddate into #price2
			from #price1 p1 
			inner join #tempPrice price	on p1.productid=price.productid and p1.effectivedate=price.effectivedate
 			group by p1.productid, p1.effectivedate

		select price.productid as productid, 
		       max(isnull(price.costprice,0.0)) as costprice, 
		       max(isnull(price.promotionprice,0.0)) as promotionprice,
		       ISNULL(DerivedCost,0) AS DerivedCost, 
		       0 AS realcost,
		       p2.effectivedate
			into #currentPrice
			from #price2 p2 
			inner join #tempPrice price on p2.productid=price.productid
				and p2.effectivedate=price.effectivedate and p2.createddate=price.createddate
			group by price.productid,p2.effectivedate, price.DerivedCost
			
			
-- end get price
		  


-- get standard options
		  SELECT 0 as id_SalesEstimate_EstimateDetails,
			  pag.areaid,a.areaname,a.sortorder,g.groupname,g.groupid,p.productid, 
			  CASE WHEN ed.displayAt<>''
			       THEN p.productname+'['+p.productid+']'+char(13)+ed.displayAt
			       ELSE p.productname+'['+p.productid+']' 
			  END
			  AS productname, 
			  CAST(isnull(quantity,1) AS DECIMAL(18,2)) AS quantity,
			  CAST(isNULL(pr.promotionprice,0) AS DECIMAL(18,2)) AS sellprice, 
			  0 as selected, 
			  ed.OptionID AS estimatedetailsid, 0 AS idStandardInclusions, 0 AS promotionproduct,
			   1 AS standardoption,
			  p.productdescription, ed.enterdesc ,p.additionalinfo,p.internaldescription,
			  0 AS itemAccepted,
			  ISNULL(p.isstudiomproduct,0) AS isstudiomproduct, pag.productareagroupid,
			  CAST(p.studiomqanda AS VARCHAR(MAX)) AS studiomquestion,0 AS imagecount,
			  p.uom,
			  ISNULL(g.StudioMSortOrder, 9999999) AS StudioMSortOrder,
			  '' AS StudioMAttributes,
			CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
				 THEN CAST(1 AS BIT)
				 ELSE CAST(0 AS BIT)
			END AS qandamandatory,
			0 AS NonstandardCategoryID,
			0 AS nonstandardgroupid,
			1 as allowtoremove,
		'./images/upgrade.png' AS SOSI,
		'Upgrade Option.' AS SOSIToolTips,
		CASE WHEN p.isstudiomproduct=1 AND p.studiomqanda IS NOT NULL AND CAST(p.studiomqanda AS VARCHAR(MAX))<>''
		     THEN 
                   CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
                        THEN './images/color_swatch.png'
                        ELSE './images/color_swatch_gray.png'
                   END		              
		     ELSE 
		        CASE WHEN p.isstudiomproduct=0
		             THEN ''
		             ELSE './images/green_box.png'
		        END	        
		END AS StudioMIcon,
		CASE WHEN p.isstudiomproduct=1 AND p.studiomqanda IS NOT NULL AND CAST(p.studiomqanda AS VARCHAR(MAX))<>''
		     THEN 
               CASE WHEN CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
                    THEN 'Studio M Product. Question not answered yet.'
                    ELSE 'Studio M Product. Answers are not mandatory.'
               END	 
		     ELSE 
		        CASE WHEN p.isstudiomproduct=0
		             THEN ''
		             ELSE 'There is no studio M questions.'
		        END	 		     
		END AS StudioMTooltips,
        1 AS ChangeQty,
		1 AS ChangePrice,
		1 AS changedisplaycode,
		1 AS changeproductstandarddescription,	
		CAST(0 AS BIT) AS PreviousChanged,
		CAST(0 AS BIT) AS changed,	
		0 AS selectedimageid,
        pr.derivedcost,
		ISNULL(pr.costprice,0)	AS 	 costexcgst,
	    pd.PriceDisplayCodeID,
		pd.PriceDisplayDesc	,
		0 AS CreatedBy,
		0 AS revisionId	,
		'' AS DerivedCostIcon,
		'' AS DerivedCostTooltips	,
		0 AS margin,
		'' AS changetype,
		ed.homeid,
		ISNULL(ed.homedisplayid,0) AS homedisplayid
							  
		  INTO	#tempoption
		  FROM @temphdo ed
		  INNER JOIN productareagroup pag		ON ed.productareagroupid= pag.productareagroupid
		  INNER JOIN Area a                     ON pag.AreaID=a.AreaID
		  INNER JOIN product p					ON pag.productid=p.productid
		  LEFT JOIN [tblUOM] uo					ON RTRIM(p.uom)=RTRIM(uo.code)
		  INNER JOIN [group] g					ON pag.groupid=g.groupid
		  INNER JOIN #currentPrice pr           ON p.ProductID=pr.productid
          INNER JOIN tblPriceDisplayCode pd     ON p.fkPriceDisplayCodeID=pd.PriceDisplayCodeID
		  ORDER BY areaname, groupname, productname

	  
---- final out put

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
    

    
    SELECT f.*,PAG.IsSiteWork AS siteworkitem, 
           CASE WHEN mp.idMultiplePromotion IS NULL 
                THEN 0
                ELSE 1
           END AS ismasterpromotion
    FROM #tempfinal f
    INNER JOIN ProductAreaGroup PAG ON f.productareagroupid=PAG.productareagroupid
    LEFT JOIN  tblMultiplePromotion mp ON f.productid=mp.BaseProductID AND mp.fkidPromotionID=@promotionid --AND mp.active=1
    ORDER BY areaname, groupname, productname
       

    DROP TABLE #tempfinal
    DROP TABLE #imagecount
    DROP TABLE #temp2
    DROP TABLE #tempoption
    DROP TABLE #currentprice

 
	 SET NOCOUNT OFF;

END

GO