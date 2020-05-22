IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetPromotionProductByMasterPromotionRevisionDetailsID]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetPromotionProductByMasterPromotionRevisionDetailsID]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetPromotionProductByMasterPromotionRevisionDetailsID]
@revisiondetailsid INT
AS
BEGIN

	SET NOCOUNT ON;
	
	   DECLARE @promotionid INT, @productid VARCHAR(50), @revisionid INT, @estimateid INT
       DECLARE @len INT, @storey INT,@revisiontypeid INT, @allowchangeprice INT,@allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT
	   
	   SET @len=100


	   SELECT 
	       @promotionid=e.PromotionID,
	       @productid=ed.ProductID,
	       @estimateid=e.EstimateID,
	       @storey=h.Stories,
	       @revisiontypeid=eh.fkid_SalesEstimate_RevisionType,
	       @revisionid=eh.id_SalesEstimate_EstimateHeader
	   FROM  (SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE id_SalesEstimate_EstimateDetails=@revisiondetailsid) sed
	   INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON sed.fkid_SalesEstimate_EstimateHeader=eh.id_SalesEstimate_EstimateHeader
	   INNER JOIN EstimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID 
	   INNER JOIN Estimate e ON ed.EstimateID=e.EstimateID
	   INNER JOIN Home h     ON e.HomeID=h.HomeID


-- populate allow changes
	   EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId,@allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT  
-- end of allow changes
	   	   
	   SELECT idMultiplePromotion 
	   INTO #tempmultiplepromotion
	   FROM tblMultiplePromotion
	   WHERE fkidPromotionID=@promotionid AND BaseProductID=@productid
	   
	   SELECT *
	   INTO   #originaltemped 
	   FROM   EstimateDetails
	   WHERE  EstimateID=@estimateid AND HomeDisplayID IS NULL
	   
	   SELECT *
	   INTO   #tempMRSED
	   FROM   tbl_SalesEstimate_EstimateDetails
	   WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid
	   
  	
       SELECT 
                sed.id_SalesEstimate_EstimateDetails,
                sed.fkidArea AS AreaId,
                sed.fkidGroup AS GroupId,
				sed.AreaName,
				sed.GroupName, 
				ed.ProductID, 
                sed.productname+char(13)+'['+ed.productid+']' AS productname,
				SED.ProductDescription, 
				CASE  WHEN	LEN(SED.ProductDescription)>@len
					  THEN	SUBSTRING(SED.ProductDescription,1,@len)+' ...'
					  ELSE  SED.ProductDescription
					  END
				AS ProductDescriptionShort,
				sed.ExtraDescription AS enterdesc,
				sed.ExtraDescription,
				SED.additionalinfo, 
				ISNULL(sed.InternalDescription,ed.InternalDescription) AS InternalDescription, 
				ed.UOM, 
				SED.Quantity, 
				CAST(ItemPrice AS DECIMAL(18,2)) AS sellPrice,
				CAST(ItemPrice AS DECIMAL(18,2)) AS ItemPrice,
				ISNULL(sed.IsPromotionProduct,0) as PromotionProduct, 
				ED.StandardOption, 
				ISNULL(sed.id_SalesEstimate_EstimateDetails,0) AS EstimateRevisionDetailsId, 
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
				CASE WHEN sed.fkidarea<>43 AND sed.fkidarea<>1 --not nonstandard and surcharge
					 THEN 0
					 ELSE ISNULL(ISNULL(sed.fkid_NonStandardArea,ed.nonstandardcatid) ,0)
				END	AS nonstandardcategoryid,
				ISNULL(sed.fkid_NonStandardGroup,0) AS nonstandardgroupid,		
				ISNULL(p.isstudiomproduct,0) AS isstudiomproduct,
				0 AS imagecount,
				p.studiomqanda as studiomquestion,
				sed.studiomattributes AS studiomanswer,
				sed.studiomattributes ,
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
				
				CAST(1 AS BIT) AS allowtoremove,
				--CASE WHEN nsrpdc.PriceDisplayCodeID IS NULL
				--	THEN ISNULL(pdc.PriceDisplayCode + ' - ' + pdc.PriceDisplayDesc,'NONE - NONE') 
				--	ELSE ISNULL(nsrpdc.PriceDisplayCode + ' - ' + nsrpdc.PriceDisplayDesc,'NONE - NONE')
				--END	AS PriceDisplayDesc,
				ISNULL(nsrpdc.PriceDisplayCode + ' - ' + nsrpdc.PriceDisplayDesc,'NONE - NONE') AS PriceDisplayDesc,		
				ISNULL(nsrpdc.PriceDisplayCodeID, 10) AS PriceDisplayCodeID,		
				--CASE WHEN nsrpdc.PriceDisplayCodeID IS NULL
				--	THEN ISNULL(pdc.PriceDisplayCodeID, 10) 
				--	ELSE ISNULL(nsrpdc.PriceDisplayCodeID, 10)
				--END AS PriceDisplayCodeID,
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
				CAST('NEW' AS VARCHAR(100)) AS changetype,
				ED.ProductAreaGroupID,
				0 AS ismasterpromotion,
				@revisionid as revisionid
	   INTO #tempfinal
       FROM   #tempmultiplepromotion tm
       INNER JOIN PromotionProduct pp	ON tm.idmultiplepromotion=pp.fkidMultiplePromotion
       INNER JOIN #originaltemped ed	ON pp.PagID=ed.productareagroupid 
       INNER JOIN #tempMRSED sed		ON ed.estimatedetailsid=sed.fkidestimatedetails
       INNER JOIN area a2				ON sed.fkidarea=a2.areaid
       LEFT OUTER JOIN Area a           ON A.AreaID = SED.fkid_NonStandardArea
       INNER JOIN [Group] g				ON sed.fkidGroup=g.GroupID
       LEFT OUTER JOIN [Group] grp		ON grp.GroupID = SED.fkid_NonStandardGroup
       INNER JOIN product p			    ON ed.ProductID=p.ProductID	
	   LEFT JOIN [tblUOM] uo            ON RTRIM(p.uom)=RTRIM(uo.code)  
	   LEFT JOIN tblPriceDisplayCode nsrpdc ON ISNULL(SED.fkid_NonStandardPriceDisplayCode,p.fkPriceDisplayCodeID) = nsrpdc.PriceDisplayCodeID
 -- get image count
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
-- final output		
		
		SELECT *
		FROM  #tempfinal
 
	SET NOCOUNT OFF;
END