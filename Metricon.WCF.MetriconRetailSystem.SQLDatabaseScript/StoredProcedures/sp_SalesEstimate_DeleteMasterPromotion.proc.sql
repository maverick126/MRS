IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_DeleteMasterPromotion]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_DeleteMasterPromotion]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_DeleteMasterPromotion]
@masterpromotionitemid			INT,
@selectedpromotionitemids       VARCHAR(MAX),
@userid                         INT
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @len	INT, @brandid INT, @promotionid INT, @stateid INT, @revisionid INT, @productid VARCHAR(50)
    DECLARE @storey	INT, @multiplepromotionid INT
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT

	SELECT	data AS revisiondetailsid
	INTO	#tempdetails
	FROM	dbo.Splitfunction_string_to_table(@selectedpromotionitemids,',')
	
	
	SELECT @masterpromotionitemid AS revisiondetailsid 
	INTO   #tempalldeleted
	
	INSERT INTO #tempalldeleted
	SELECT revisiondetailsid
	FROM   #tempdetails

	
    -- get promotion products. Note:@selectedpromotionitemids is the selected remove items of this promo
    -- it may not be all the promo items. so we need get all promo items to compare:
    -- if still exists some promo items, after remove master promo, they should mark BOT as promotion items.
    
    SELECT @revisionid=fkid_SalesEstimate_EstimateHeader,
           @productid=ed2.ProductID,
           @promotionid=e.PromotionID
    FROM tbl_SalesEstimate_EstimateDetails ed
    INNER JOIN EstimateDetails ed2 ON ed.fkidEstimateDetails=ed2.EstimateDetailsID
    INNER JOIN	estimate e	ON ed2.estimateid=e.estimateid
    WHERE id_SalesEstimate_EstimateDetails in (@masterpromotionitemid)
    

    SELECT @multiplepromotionid =idMultiplePromotion
    FROM   tblMultiplePromotion
    WHERE  fkidPromotionID=@promotionid AND BaseProductID=@productid
    
    SELECT *
    INTO   #temppromotionproducts
    FROM   PromotionProduct
    WHERE  fkidMultiplePromotion=@multiplepromotionid
    
    -- get unselected promotion products

    SELECT ed1.*,ed2.ProductID
    INTO #tempMRSED
    FROM  (SELECT * FROM  tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader=@revisionid) ed1
    INNER JOIN EstimateDetails ed2 ON ed1.fkidestimatedetails=ed2.EstimateDetailsID

    
    SELECT      ed.id_SalesEstimate_EstimateDetails,fkidproductareagroup
    INTO        #unselectedpromotionitems
    FROM		#tempMRSED  ed
    INNER JOIN  #temppromotionproducts pp ON ed.fkidproductareagroup=pp.pagid
    WHERE       ed.id_SalesEstimate_EstimateDetails NOT IN (SELECT revisiondetailsid FROM #tempdetails)   

   -- get other master promo which are not removed one
   
   SELECT  ed.*,mp.idMultiplePromotion
   INTO    #otherselectedmasterpromo
   FROM    #tempMRSED ed
   INNER JOIN tblMultiplePromotion mp ON fkidPromotionID=@promotionid AND ed.productid=mp.BaseProductID AND ed.id_SalesEstimate_EstimateDetails<>@masterpromotionitemid
   

   SELECT DISTINCT pagid
   INTO   #otherpromotionproducts 
   FROM   PromotionProduct pp
   INNER JOIN #otherselectedmasterpromo op ON pp.fkidMultiplePromotion=Op.idMultiplePromotion


	BEGIN TRY

		BEGIN TRANSACTION	    
        -- insert items to removed item table
				INSERT INTO tbl_salesestimate_removeditems
				([fkidRevision]
			   ,[fkidEstimateDetails]
			   ,[fkidStandardInclusions]
			   ,[fkidProductAreaGroup]
			   ,[RemovedDate]
			   ,[RemovedBy]
			   ,[Reason]
			   ,[fkid_SalesEstimate_PredefinedDeletionReason])	
			   SELECT
				 @revisionid,
				 fkidEstimateDetails,
				 fkidStandardInclusions,
				 fkidProductAreaGroup,
				 GETDATE(),
				 @userId,
				 '',
				 4
			   FROM #tempalldeleted ded
			   INNER JOIN #tempMRSED ed ON ded.revisiondetailsid=ed.id_SalesEstimate_EstimateDetails

       -- remove items from estimate
               DELETE FROM tbl_SalesEstimate_EstimateDetails
               WHERE  id_SalesEstimate_EstimateDetails IN
               (SELECT revisiondetailsid FROM #tempalldeleted) 

       -- update unselected items to NOT as promo item if not exists in other selected promotion estimate
               UPDATE tbl_SalesEstimate_EstimateDetails
               SET    IsPromotionProduct=0
               FROM   tbl_SalesEstimate_EstimateDetails ed
               INNER JOIN  #unselectedpromotionitems ded ON ed.id_SalesEstimate_EstimateDetails= ded.id_SalesEstimate_EstimateDetails   
               LEFT JOIN   #otherpromotionproducts opi ON ed.fkidProductAreaGroup=opi.pagid
               WHERE opi.pagid IS NULL       
              
  
		COMMIT

	END TRY

	BEGIN CATCH

		IF @@TRANCOUNT > 0
			ROLLBACK
		
		-- Raise an error
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)

	END CATCH				 

    
END