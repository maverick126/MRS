----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_InsertEstimateDetails]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_InsertEstimateDetails]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_InsertEstimateDetails]
	@revisionId int, 
	@estimateDetailsId int, 
	@pagId int,
	@userId int, 
	@estimateRevisionDetailsId int OUTPUT
AS
BEGIN

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000), @addinfo VARCHAR(2000) 
	DECLARE @ErrSeverity INT
    DECLARE @storey INT
    DECLARE @qty DECIMAL(18,2)
    DECLARE @productdesc VARCHAR(MAX), @productid VARCHAR(50)
    DECLARE @issitework INT, @areasortorder INT, @groupsortorder INT, @productsortorder INT, @areaid INT, @groupid INT
    DECLARE @cost DECIMAL(18,2), @retailprice DECIMAL(18,2),@derivedpercentage DECIMAL(18,4)
    DECLARE @effectivedate DATETIME

 
    SELECT      @storey=h.Stories
    FROM        tbl_SalesEstimate_EstimateHeader eh
    INNER JOIN  estimate e  ON eh.fkidestimate=e.estimateid
    --INNER JOIN  DepositDetails dp ON e.fkidOpportunity=dp.fkidOpportunity
    INNER JOIN  home h		ON e.homeid=h.homeid
    --INNER JOIN  tblpriceregiongroupmapping pm ON e.regionid=pm.fkregionid
    WHERE		id_SalesEstimate_EstimateHeader=@revisionId
	
	/*        
    IF (@estimateDetailsId=0)-- this is standard inclusion
    BEGIN
	    
		SELECT
					@standardinclusionid=st.idstandardinclusions,
					@qty=st.quantity,
					@productdesc=p.productdescription,
					@addinfo=p.additionalinfo
		FROM		(SELECT * FROM tblStandardInclusions WHERE brandid= @brandid AND fkidregiongroup=@regiongroupid AND ProductAreaGroupID=@pagId) st
		INNER JOIN  productareagroup pag				ON st.productareagroupid=pag.productareagroupid 
		INNER JOIN  product p							ON pag.productid=p.productid 
		
		SELECT @brandid AS BRDID, @regiongroupid AS RGNID, @standardInclusionId AS STDINCID
		
    END
	*/
	
	BEGIN TRY
		BEGIN TRANSACTION	
            IF (@estimateDetailsId<>0) -- standard option
            BEGIN
                -- get sort order
                SELECT ed1.*, 
                       CASE WHEN ed1.fkidarea<>43 THEN ed1.fkidarea ELSE ed1.fkid_NonStandardArea END AS areaid, 
                       CASE WHEN ed1.fkidarea<>43 THEN ed1.fkidgroup ELSE ed1.fkid_NonStandardGroup END AS groupid,
                       ed.ProductID
                INTO #tempMRSED
                FROM   (SELECT * FROM tbl_SalesEstimate_EstimateDetails  WHERE fkid_SalesEstimate_EstimateHeader=@revisionId) ed1
                INNER JOIN EstimateDetails ed ON ed1.fkidEstimateDetails=ed.EstimateDetailsID
               
                
                SELECT 
                     @issitework=pag.IsSiteWork,
                     @productid=ed.ProductID,
                     @areaid=ed.areaid,
                     @groupid=ed.groupid,
                     @areasortorder=CASE WHEN @storey=1 THEN a.SortOrder ELSE a.SortOrderDouble END,
                     @groupsortorder=g.SortOrder,
                     @productsortorder=pp.SortOrder
                FROM
                (SELECT * FROM   EstimateDetails WHERE  EstimateDetailsID=@estimateDetailsId) ed
                INNER JOIN ProductAreaGroup pag ON ed.ProductAreaGroupID=pag.ProductAreaGroupID
                INNER JOIN product pp ON ed.ProductID=pp.ProductID
                INNER JOIN Area a ON ed.areaid=a.AreaID
                INNER JOIN [Group] g ON ed.groupid=g.GroupID
                
                IF (EXISTS(SELECT * FROM #tempMRSED WHERE areaid=@areaid))
                   BEGIN
                      SELECT TOP 1 @areasortorder=areasortorder FROM  #tempMRSED WHERE areaid=@areaid
                   END

                IF (EXISTS(SELECT * FROM #tempMRSED WHERE groupid=@groupid))
                   BEGIN
                      SELECT TOP 1 @groupsortorder=groupsortorder FROM  #tempMRSED WHERE groupid=@groupid
                   END                

               IF (EXISTS(SELECT * FROM #tempMRSED WHERE productid=@productid))
                   BEGIN
                      SELECT TOP 1 @productsortorder=productsortorder FROM  #tempMRSED WHERE productid=@productid
                   END                  
                -- end sort order
                

				INSERT INTO tbl_SalesEstimate_EstimateDetails 
				(fkid_SalesEstimate_EstimateHeader, fkidEstimateDetails,ItemPrice, Quantity,
				ProductDescription, ExtraDescription, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, fkidStandardInclusions, additionalinfo, changed,
				IsSiteWork, DerivedCost, CostExcGST, AreaSortOrder, GroupSortOrder, ProductSortOrder,
				fkidArea,AreaName,fkidGroup, GroupName, fkidProductAreaGroup,ProductName,IsPromotionProduct
				)
				SELECT @revisionId, EstimateDetailsID, 				
				CASE WHEN areaid=43 THEN 0 ELSE SellPrice END AS SellPrice, 
				CASE WHEN areaid=43 THEN 1 ELSE Quantity END AS Quantity,
				ProductDescription, EnterDesc, GETDATE(), @userId, GETDATE(), @userId, null, additionalinfo, 1,
				@issitework,0,0, @areasortorder, @groupsortorder, @productsortorder,
				areaid,AreaName,groupid,GroupName,ProductAreaGroupID,ProductName,PromotionProduct
				FROM EstimateDetails
				WHERE EstimateDetailsID = @estimateDetailsId
				
				--Do not remove NSR from the removed item log as a new NSR may not be the same as the deleted one
				DELETE FROM tbl_SalesEstimate_RemovedItems WHERE fkidEstimateDetails = @estimateDetailsId AND fkidRevision = @revisionId AND 
				(NOT EXISTS (SELECT * FROM EstimateDetails WHERE EstimateDetailsID = fkidEstimateDetails AND areaid = 43))

			END
			--ELSE -- standard inclusion
   --         BEGIN

			--	INSERT INTO tbl_SalesEstimate_EstimateDetails 
			--	(fkid_SalesEstimate_EstimateHeader, fkidEstimateDetails, ItemPrice, Quantity,
			--	ProductDescription, ExtraDescription, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, fkidStandardInclusions,additionalinfo, changed,
			--	IsSiteWork, DerivedCost, CostExcGST, AreaSortOrder, GroupSortOrder, ProductSortOrder)
			--	SELECT @revisionId, 0, 0, @qty,@productdesc, '', GETDATE(), @userId, GETDATE(), @userId, @standardinclusionid,@addinfo,1,
			--	@issitework,0,0, @areasortorder, @groupsortorder, @productsortorder

			--	DELETE FROM tbl_SalesEstimate_RemovedItems WHERE fkidStandardInclusions = @standardinclusionid AND fkidRevision = @revisionId
			
			--	SELECT @standardinclusionid AS STDINCID, @revisionId AS REVID
			
			--END			

			SELECT @estimateRevisionDetailsId = @@IDENTITY

			UPDATE tbl_SalesEstimate_EstimateHeader
			SET ModifiedBy = @userId, 
				ModifiedOn = GETDATE()
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

END
