 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CreateEstimateFromPreviousRevision]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CreateEstimateFromPreviousRevision]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_CreateEstimateFromPreviousRevision]
	@estimateId INT, @ownerId INT, @revisionTypeId INT, @createdbyId INT, @statusId INT,
	@queueId INT = 0, @estimateRevisionId INT = 0 OUTPUT
AS
BEGIN
	DECLARE @revisionNumber INT
	DECLARE @homePrice MONEY
	DECLARE @effectiveDate DATETIME
	DECLARE @dueDate DATETIME
	DECLARE @previousRevisionHeaderId INT
	--DECLARE @statusId INT
	DECLARE @previousRevisionTypeId INT
	DECLARE @difficultyRatingId INT
	DECLARE @previousRevisionNumber INT
	DECLARE @contracttype VARCHAR(10)
	DECLARE @homeName VARCHAR(250)
	DECLARE @mrsgroupid INT

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT, @previousUserid INT

	-- Select the latest Revision of the Sales Estimate 
	
    IF(@revisionTypeId=2)
    BEGIN  -- this means this from first queue, get mrs group from user not from previous revision
       SELECT @mrsgroupid=r.MRSGroupID
       FROM   tblUserSubRegionMapping usm
       INNER JOIN tblSubRegionPriceRegionMapping spm ON usm.fkidSubRegion=spm.fkidSubRegion
       INNER JOIN Region r ON spm.fkRegionID=r.RegionID
       WHERE usm.fkidUser=@ownerId
    END	
    ELSE
    BEGIN
		SELECT TOP 1 
			@mrsgroupid=fkid_salesestimate_MRSGroup		
		FROM tbl_SalesEstimate_EstimateHeader
		WHERE fkidEstimate = @estimateId
		ORDER BY RevisionNumber DESC    
    END
	
	SELECT TOP 1 
		@homePrice = HomePrice,
		@effectiveDate = HomePriceEffectiveDate,
		@previousRevisionNumber = RevisionNumber,
		@dueDate = NULL, --Reset Due Date
		@previousRevisionHeaderId = id_SalesEstimate_EstimateHeader,
		@previousRevisionTypeId = fkid_SalesEstimate_RevisionType,
		@difficultyRatingId = fkid_SalesEstimate_DifficultyRating,
		@previousUserid=fkidowner,
		@contracttype=ISNULL(ContractType,'PC'),
		@homeName = HomeDisplayName
	FROM tbl_SalesEstimate_EstimateHeader
	WHERE fkidEstimate = @estimateId
	ORDER BY RevisionNumber DESC



	-- If the Sales Estimate Revision exists
	IF @@ROWCOUNT > 0
	BEGIN

		-- If it's created from the Queue
		IF @queueId > 0
		BEGIN
			
			-- Get Due Date and Difficulty Rating from the Queue
			SELECT 
			@dueDate = DueDate,
			@difficultyRatingId = fkid_SalesEstimate_DifficultyRating,
			@contracttype=ISNULL(ContractType,'PC')
			FROM tbl_SalesEstimate_Queue
			WHERE id_SalesEstimate_Queue = @queueId

		END
		-- If the estimate moves from one revision type to another
		ELSE IF @revisionTypeId <> @previousRevisionTypeId 
		BEGIN
			SET @difficultyRatingId = NULL
		END

        IF(NOT (@revisionTypeId=@previousRevisionTypeId AND @previousUserid=@ownerId)    )
             BEGIN
		-- Increment Revision Number by 1		
							SET @revisionNumber = @previousRevisionNumber + 1
							
							INSERT INTO tbl_SalesEstimate_EstimateHeader
							(fkidEstimate, 
							RevisionNumber, 
							fkid_SalesEstimate_RevisionType, 
							fkidOwner,
							fkid_SalesEstimate_Status, 
							HomePrice, 
							DueDate, 
							fkid_SalesEstimate_DifficultyRating,
							HomePriceEffectiveDate, 
							CreatedOn, 
							CreatedBy, 
							ModifiedOn, 
							ModifiedBy,
							ContractType,
							HomeDisplayName,
							fkid_salesestimate_MRSGroup
							)
							VALUES 
							(@estimateId, 
							@revisionNumber, 
							@revisionTypeId, 
							@ownerId, 
							@statusId, 
							@homePrice, 
							@dueDate, 
							@difficultyRatingId, 
							@effectiveDate, 
							GETDATE(), 
							@createdbyId, 
							GETDATE(), 
							@createdbyId,
							@contracttype,
							@homeName,
							@mrsgroupid)	

							SET @estimateRevisionId = @@IDENTITY

							INSERT INTO tbl_SalesEstimate_EstimateDetails 
							(fkid_SalesEstimate_EstimateHeader, 
							fkidEstimateDetails,
							ItemPrice, 
							Quantity, 
							ProductDescription, 
							ExtraDescription, 
							InternalDescription,
							StudioMAttributes,
							CreatedOn, 
							CreatedBy,
							ModifiedOn, 
							ModifiedBy,
							fkid_NonStandardArea,
							fkid_NonStandardGroup, 
							fkid_NonStandardPriceDisplayCode,
							fkidStandardInclusions,
							AdditionalInfo,
							SelectedImageID,
							IsSitework,
							CostExcGST,
							CostOverWriteBy,
							DerivedCost,
							AreaSortOrder,
							GroupSortOrder,
							ProductSortOrder,
							ItemAccepted,
							SalesEstimatorAccepted	
						   ,fkidArea
						   ,AreaName
						   ,fkidGroup
						   ,GroupName
						   ,fkidProductAreaGroup
						   ,ProductName
						   ,IsPromotionProduct								
							)
							SELECT 
							@estimateRevisionId AS fkid_SalesEstimate_EstimateHeader, 
							fkidEstimateDetails,
							ItemPrice, 
							Quantity, 
							ProductDescription, 
							ExtraDescription, 
							InternalDescription,
							StudioMAttributes,
							CreatedOn, 
							CreatedBy, 
							GETDATE(), 
							@createdbyId, 
							fkid_NonStandardArea, 
							fkid_NonStandardGroup, 
							fkid_NonStandardPriceDisplayCode,
							fkidStandardInclusions,		
							AdditionalInfo,
							SelectedImageID,
							IsSitework,
							CostExcGST,
							CostOverWriteBy,
							DerivedCost,
							AreaSortOrder,
							GroupSortOrder,
							ProductSortOrder,
							ItemAccepted,
							SalesEstimatorAccepted
						   ,fkidArea
						   ,AreaName
						   ,fkidGroup
						   ,GroupName
						   ,fkidProductAreaGroup
						   ,ProductName
						   ,IsPromotionProduct									
							FROM tbl_SalesEstimate_EstimateDetails 
							WHERE fkid_SalesEstimate_EstimateHeader = @previousRevisionHeaderId			

							-- rejected estimate should bring itemaccepted flag to SC version
							DECLARE @previousrevisionstatus INT, @previousrevisiontype INT
							SELECT 
									@previousrevisionstatus= fkid_SalesEstimate_Status,
									@previousrevisiontype=fkid_SalesEstimate_RevisionType
							FROM tbl_SalesEstimate_EstimateHeader 
							WHERE id_SalesEstimate_EstimateHeader=@previousRevisionHeaderId

							IF (@previousrevisionstatus=3 AND @previousrevisiontype=2) -- rejected by STS back to SC
								BEGIN
									SELECT ItemAccepted, InternalDescription, AdditionalInfo, fkidEstimateDetails INTO #temp1  
									FROM tbl_SalesEstimate_EstimateDetails 
									WHERE fkid_SalesEstimate_EstimateHeader = @previousRevisionHeaderId
							   
									UPDATE tbl_SalesEstimate_EstimateDetails
									SET ItemAccepted=ISNULL(ed2.ItemAccepted,0),
										InternalDescription=ed2.InternalDescription,
										AdditionalInfo=ed2.AdditionalInfo,
										previouschanged=1
									FROM   tbl_SalesEstimate_EstimateDetails ed1
									INNER JOIN #temp1 ed2
									ON ed1.fkidEstimateDetails=ed2.fkidEstimateDetails 
									AND ed1.fkid_SalesEstimate_EstimateHeader=@estimateRevisionId
									
									DROP TABLE #temp1
								END
					        
							ELSE IF (@previousrevisionstatus=2 AND @previousrevisiontype=1) -- SC accepted to STS
								BEGIN
									SELECT ItemAccepted, fkidEstimateDetails INTO #temp2  
									FROM tbl_SalesEstimate_EstimateDetails 
									WHERE fkid_SalesEstimate_EstimateHeader = @previousRevisionHeaderId AND ItemAccepted=1 AND (changed IS NULL or changed=0)
									
									UPDATE tbl_SalesEstimate_EstimateDetails
									SET ItemAccepted=ISNULL(ed2.ItemAccepted,0)
									FROM tbl_SalesEstimate_EstimateDetails ed1
									INNER JOIN #temp2 ed2
									ON ed1.fkidEstimateDetails=ed2.fkidEstimateDetails 
									AND ed1.fkid_SalesEstimate_EstimateHeader=@estimateRevisionId  
									
									DROP TABLE #temp2        
							   END
					           
							IF (@previousrevisionstatus<>3 AND @previousrevisiontype<>2)
							BEGIN

							  SELECT Changed, fkidEstimateDetails INTO #temp3  
							  FROM tbl_SalesEstimate_EstimateDetails 
						      WHERE fkid_SalesEstimate_EstimateHeader = @previousRevisionHeaderId
								
							  UPDATE tbl_SalesEstimate_EstimateDetails
							  SET     previouschanged=ed2.changed
							  FROM   tbl_SalesEstimate_EstimateDetails ed1
							  INNER JOIN #temp3 ed2 ON ed1.fkidEstimateDetails=ed2.fkidEstimateDetails 
							  AND ed1.fkid_SalesEstimate_EstimateHeader=@estimateRevisionId 
							  
							  DROP TABLE #temp3         
							END
							--Add Standard Inclusions after Sales Consultant
							--IF @previousRevisionNumber = 1
							--BEGIN
							--	EXEC sp_SalesEstimate_AddStandardInclusions
							--	@previousRevisionHeaderId, @estimateRevisionId, @createdbyId
							--END
			 END
        ELSE SET @estimateRevisionId=@previousRevisionHeaderId
	END
	ELSE
	BEGIN

			SET @ErrMsg = 'The previous revision of Sales Estimate ' + CONVERT(NVARCHAR(50), @estimateId) + ' could not be found.'
			SET @ErrSeverity = 16
			RAISERROR(@ErrMsg, @ErrSeverity, 1)

	END	

END


GO