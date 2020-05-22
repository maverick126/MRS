IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetExistingPromotionProductByMasterPromotionRevisionDetailsID]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetExistingPromotionProductByMasterPromotionRevisionDetailsID]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetExistingPromotionProductByMasterPromotionRevisionDetailsID]
@revisiondetailsid INT
AS
BEGIN

	SET NOCOUNT ON;
	
	   DECLARE @promotionid INT, @productid VARCHAR(50), @revisionid INT, @estimateid INT, @selectedpromo VARCHAR(200)
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
	   	     	
	   	   
	   SELECT idMultiplePromotion,PromotionName 
	   INTO #tempmultiplepromotion
	   FROM tblMultiplePromotion
	   WHERE fkidPromotionID=@promotionid AND BaseProductID=@productid
	   
	   SELECT @selectedpromo=PromotionName
	   FROM #tempmultiplepromotion
	   
	   SELECT *
	   INTO   #originaltemped 
	   FROM   EstimateDetails
	   WHERE  EstimateID=@estimateid AND HomeDisplayID IS NULL
	   
	   SELECT *
	   INTO   #tempMRSED
	   FROM   tbl_SalesEstimate_EstimateDetails
	   WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid
	   
  	
       SELECT 
                sed.fkidproductareagroup,
                sed.fkidestimatedetails,
				sED.AreaName,
				sed.GroupName, 
                sed.productname+char(13)+'['+ed.productid+']' AS productname,
				SED.ProductDescription, 
				CASE  WHEN	LEN(SED.ProductDescription)>@len
					  THEN	SUBSTRING(SED.ProductDescription,1,@len)+' ...'
					  ELSE  SED.ProductDescription
					  END
				AS ProductDescriptionShort,
				SED.Quantity, 
				CAST(ItemPrice AS DECIMAL(18,2)) AS ItemPrice,
				ISNULL(sED.IsPromotionProduct,0) as PromotionProduct, 
				ISNULL(sed.id_SalesEstimate_EstimateDetails,0) AS EstimateRevisionDetailsId, 
				ISNULL(SED.issitework,0) AS siteworkitem,
                0 AS isinmultiplepromotion,
                CAST(@selectedpromo AS VARCHAR(2000)) AS multiplepromotionname,
                CAST('../images/spacer.gif' AS VARCHAR(100)) AS iconimage,
                sed.areasortorder,
                sed.groupsortorder,
                sed.productsortorder 
	   INTO #tempfinal
       FROM   #tempmultiplepromotion tm
       INNER JOIN PromotionProduct pp	ON tm.idmultiplepromotion=pp.fkidMultiplePromotion
       INNER JOIN #originaltemped ed	ON pp.PagID=ed.productareagroupid 
       INNER JOIN #tempMRSED sed		ON ed.estimatedetailsid=sed.fkidestimatedetails
       INNER JOIN area a2				ON sed.fkidarea=a2.areaid
       INNER JOIN [Group] g				ON sed.fkidGroup=g.GroupID
 
	   IF(EXISTS
	       (
	         -- area 52 and group 251 means promotion-promotionpack
	          SELECT *
	          FROM tbl_SalesEstimate_EstimateDetails
	          WHERE fkid_SalesEstimate_EstimateHeader=@revisionid AND 
	                fkidArea=52 AND 
	                fkidGroup=251 AND 
	                id_SalesEstimate_EstimateDetails<>@revisiondetailsid
	       )
	     )
	     BEGIN
	          SELECT mp.PromotionName, pp.PagID, ed3.estimatedetailsid
	          INTO #tempotherpromotion
	          FROM #tempMRSED ed
	          INNER JOIN #originaltemped ed2 ON ed.fkidEstimateDetails=ed2.EstimateDetailsID
	          INNER JOIN tblMultiplePromotion mp ON ed2.ProductID=mp.BaseProductID AND mp.fkidPromotionID=@promotionid
	          INNER JOIN PromotionProduct pp ON mp.idMultiplePromotion=pp.fkidMultiplePromotion
	          INNER JOIN #originaltemped ed3 ON pp.PagID=ed3.productareagroupid
	          WHERE fkidArea=52 AND fkidGroup=251 AND id_SalesEstimate_EstimateDetails<>@revisiondetailsid	 
       
	          UPDATE  #tempfinal
	          SET     isinmultiplepromotion=1,
	                  multiplepromotionname=multiplepromotionname+CHAR(13)+opp.PromotionName,
	                  iconimage='../images/arrow_divide.png'
	          FROM    #tempfinal fin
	          INNER JOIN #tempotherpromotion opp ON fin.fkidestimatedetails=opp.estimatedetailsid      
	     END
	
       SELECT * 
       FROM #tempfinal
       ORDER BY areasortorder, groupsortorder,productsortorder
 
 
       DROP TABLE #tempfinal
       --DROP TABLE #tempotherpromotion
       DROP TABLE #originaltemped
       DROP TABLE #tempMRSED
       DROP TABLE #tempmultiplepromotion
       
       
	SET NOCOUNT OFF;
END
