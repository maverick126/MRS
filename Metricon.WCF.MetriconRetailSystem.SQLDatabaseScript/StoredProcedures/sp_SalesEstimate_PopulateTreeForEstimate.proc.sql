/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the problem to drop the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_PopulateTreeForEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_PopulateTreeForEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER Procedure [dbo].[sp_SalesEstimate_PopulateTreeForEstimate]
	@revisionid int
AS
BEGIN

	SET NOCOUNT ON;


		  DECLARE @estimateid INT, @brandid INT, @regiongroupid INT, @revisiontypeid INT, @priceregionid INT
		  DECLARE @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT

		  DECLARE @areatable TABLE
		  (areaid INT)
		  
		  DECLARE @grouptable TABLE
		  (groupid INT)
		  
		DECLARE @currentPrice TABLE
		(
		   productid VARCHAR(50),
		   promotionprice DECIMAL(18,2),
		   effectivedate DATETIME,
		   costprice DECIMAL(18,2),
		   derivedcost INT,
		   realcost INT
		)		  
		DECLARE @tempTab TABLE
		(
		   id INT,
		   idtype VARCHAR(20)
		)		
	  
		  SELECT	@estimateid=fkidestimate , @revisiontypeid=fkid_salesestimate_revisiontype
		  FROM		tbl_SalesEstimate_EstimateHeader 
		  WHERE		id_SalesEstimate_EstimateHeader=@revisionid
		  
		  SELECT @brandid=h.brandid, @regiongroupid=rg.fkidregiongroup, @priceregionid=e.RegionID
		  FROM estimate e
		  INNER JOIN home h ON e.homeid=h.homeid
		  INNER JOIN tblpriceregiongroupmapping rg ON e.regionid=rg.fkregionid
		  WHERE estimateid=@estimateid
		  
		  INSERT INTO @currentPrice
		  EXEC sp_SalesEstimate_GetItemCostPriceForEstimate @estimateid		  
		  
		  EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId,@allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT 

		  INSERT INTO @tempTab
		  EXEC sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision @revisionid
  
		  IF (@revisiontypeid in (8,9,10,11,12,21,22))
			BEGIN
				  INSERT INTO @areatable
				  SELECT fkidarea
				  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
				  WHERE  fkid_salesestimate_revisiontype=@revisiontypeid	AND 
						 active=1											AND
						 fkidarea IS NOT NULL								AND
						 fkidarea<>0										AND
						 excludedefinedareagroup=0
				
				  INSERT INTO @areatable (areaid) VALUES (43) --Add Non Standard Request Area
						 
				  INSERT INTO @grouptable
				  SELECT fkidgroup
				  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
				  WHERE  fkid_salesestimate_revisiontype=@revisiontypeid	AND 
						 active=1											AND
						 fkidgroup IS NOT NULL								AND
						 fkidgroup<>0										AND
						 excludedefinedareagroup=0						 
		    END
		  ELSE IF (@revisiontypeid=7)
		    BEGIN
		 
				  INSERT INTO @areatable
				  SELECT areaid
				  FROM	 area
				  WHERE  active=1 AND areaid NOT IN   
				         (
							  SELECT fkidarea 
							  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
							  WHERE  active=1											AND
									 fkidarea IS NOT NULL								AND
									 fkidarea<>0										AND
									 excludedefinedareagroup=0                          AND
									 fkidArea IN (SELECT id FROM @tempTab WHERE idtype='AREA')
						 )
						 
						 
				  INSERT INTO @grouptable
				  SELECT groupid
				  FROM   [group]
				  WHERE   active=1 AND groupid NOT IN
				        (
				              SELECT fkidgroup
							  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
							  WHERE  
									 active=1											AND
									 fkidgroup IS NOT NULL								AND
									 fkidgroup<>0										AND
									 excludedefinedareagroup=0		                    AND
									 fkidGroup IN (SELECT id FROM @tempTab WHERE idtype='GROUP')
						 )	  
						 

		    END
		  ELSE -- normal version get all area group
			BEGIN
				  INSERT INTO @areatable
				  SELECT areaid
				  FROM	 area
				  WHERE  active=1  
			END	  

  		  SELECT * INTO #temped2 
		  FROM estimatedetails 
		  WHERE estimateid=@estimateid 
		  
		  --Remove Products that should NOT be selectable in this revision
		  IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionType WHERE id_SalesEstimate_RevisionType = @revisiontypeid AND ExclusiveProductId IS NOT NULL)
		  BEGIN
				DELETE FROM #temped2 WHERE areaid = 43 AND SUBSTRING(ProductID,2,10) NOT IN 
				(SELECT SUBSTRING(ExclusiveProductId,2,10) FROM tbl_SalesEstimate_RevisionType 
				WHERE id_SalesEstimate_RevisionType = @revisiontypeid AND ExclusiveProductId IS NOT NULL)	  
		  END
		  ELSE
		  BEGIN
				DELETE FROM #temped2 WHERE areaid = 43 AND SUBSTRING(ProductID,2,10) IN 
				(SELECT SUBSTRING(ExclusiveProductId,2,10) FROM tbl_SalesEstimate_RevisionType 
				WHERE ExclusiveProductId IS NOT NULL)			  
		  END 
		  		 
		  SELECT  t2.*,
		         CASE WHEN d.homeid IS NOT NULL 
		              THEN h.homename+' - Display at '+d.suburb
		              ELSE ''
		              END 
		         AS displayAt 
		  INTO #temped
		  FROM #temped2 t2
		  LEFT JOIN display d ON t2.homedisplayid=d.homeid
		  LEFT JOIN home h ON d.homeid=h.homeid
		  WHERE areaid IN (SELECT areaid FROM @areatable) OR groupid IN (SELECT groupid FROM @grouptable)		  
  
		  -- update nonstandard option make sure always got qty=1 and price is 0 and extra desc is subject to builder acceptence. because this is for new options
		  UPDATE #temped
		  set     quantity=1, sellprice=0, totalprice=0, enterdesc='Subject to builder acceptence.'
		  WHERE  areaid=43 

-- get standard options
		  SELECT 
			  ed.areaid,ed.areaname,ed.sortorder,ed.groupname,ed.groupid,ed.productid, 
			  CASE WHEN ed.displayAt<>''
			       THEN ed.productname+'['+ed.productid+']'+char(13)+ed.displayAt
			       ELSE ed.productname+'['+ed.productid+']' 
			  END
			  AS productname, 
			  CAST(isnull(quantity,1) AS DECIMAL(18,2)) AS quantity,
			  CAST(isNULL(ed.sellprice,0) AS DECIMAL(18,2)) AS sellprice, 
			  selected, estimatedetailsid, 0 AS idStandardInclusions, promotionproduct,standardoption,
			  ed.productdescription, enterdesc ,ed.additionalinfo,ed.internaldescription,
			  --CASE WHEN ed.areaid in (43,55)-- non standard and site work
			  --     THEN 0				
			  --     ELSE 1
			  --END	AS itemAccepted,
			  0 AS itemAccepted,
			  ISNULL(p.isstudiomproduct,0) AS isstudiomproduct, pag.productareagroupid,
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
		--ed.ChangeQty,
		--ed.ChangePrice,
		CASE WHEN @allowchangeqty=1 AND @revisiontypeid IN (4,15, 19, 25)
			 THEN 1
			 ELSE	
				 CASE WHEN uo.code='NT'
				 THEN 0
				 ELSE 1   
				 END        
		END 		
		AS ChangeQty,

		CASE WHEN @allowchangeprice=1 AND @revisiontypeid IN (4,15, 19, 25)
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
			 ELSE CASE WHEN pag.areaid=43
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
		--@allowchangePriceDisplayCode AS changedisplaycode,
		--@allowchangeDesc AS changeproductstandarddescription,		
        pr.derivedcost,
		ISNULL(pr.costprice,0)	AS 	 costexcgst								  
		  INTO	#tempoption
		  FROM #temped ed
		  INNER JOIN productareagroup pag		ON ed.productareagroupid= pag.productareagroupid
		  INNER JOIN product p					ON pag.productid=p.productid
		  LEFT JOIN [tblUOM] uo					ON RTRIM(p.uom)=RTRIM(uo.code)
		  INNER JOIN [group] g					ON pag.groupid=g.groupid
		  INNER JOIN @currentPrice pr           ON p.ProductID=pr.productid	
		  LEFT JOIN (SELECT	fkidestimatedetails FROM	tbl_SalesEstimate_EstimateDetails WHERE	fkid_SalesEstimate_EstimateHeader=@revisionid  ) a ON ed.estimatedetailsid=a.fkidestimatedetails
		  WHERE		a.fkidestimatedetails IS NULL

		  ORDER BY areaname, groupname, productname

	  
---- final out put

          SELECT * 
          INTO #tempfinal 
          FROM #tempoption
          /*
          UNION
          SELECT * FROM #tempinclison */
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
    
    /*
    SELECT f.*,CASE WHEN sa.productareagroupid IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS siteworkitem 
    FROM #tempfinal f
    LEFT JOIN #siteworkarea sa ON f.productareagroupid=Sa.productareagroupid
    */
    
    SELECT f.*,PAG.IsSiteWork AS siteworkitem 
    FROM #tempfinal f
    INNER JOIN ProductAreaGroup PAG ON f.productareagroupid=PAG.productareagroupid
    --WHERE f.ProductID='sg-bas-was-010'
    ORDER BY areaname, groupname, productname
    
    --where isstudiomproduct=1
    --ORDER BY areaorder, groupsortorder, productsortorder	       
    DROP TABLE #temped
    DROP TABLE #temped2
    DROP TABLE #tempfinal
    DROP TABLE #imagecount
    DROP TABLE #temp2
    DROP TABLE #tempoption
    /*DROP TABLE #tempinclison*/
 
	 SET NOCOUNT OFF;

END
GO
 