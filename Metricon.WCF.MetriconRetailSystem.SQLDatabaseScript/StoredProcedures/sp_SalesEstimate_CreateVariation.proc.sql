----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CreateVariation]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CreateVariation]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_CreateVariation]
	@estimateRevisionId INT, @revisionTypeId INT, @userId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @revisionNumber INT
	DECLARE @estimateId INT
	DECLARE @homePrice MONEY
	DECLARE @effectiveDate DATETIME
	DECLARE @previousRevisionHeaderId INT
	DECLARE @newEstimateRevisionId INT
    DECLARE @contractType VARCHAR(10)
    DECLARE @homeName VARCHAR(250)
    DECLARE @mrsgroupid INT
    
	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT

	SELECT
		@estimateId = fkidEstimate,
		@contractType= contracttype,
		@homePrice = HomePrice,
		@effectiveDate = HomePriceEffectiveDate,
		@homeName = HomeDisplayName,
		@mrsgroupid=fkid_salesestimate_MRSGroup
	FROM tbl_SalesEstimate_EstimateHeader
	WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
	-- Select the latest Revision of the Sales Estimate 
	SELECT TOP 1 @revisionNumber = RevisionNumber
	FROM tbl_SalesEstimate_EstimateHeader
	WHERE fkidEstimate = @estimateId
	ORDER BY RevisionNumber DESC	
	
	IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId
	AND fkid_SalesEstimate_RevisionType IN (14, 15, 16, 17, 18, 19, 20, 24, 25) 
	AND fkid_SalesEstimate_Status = 1) --In Progress
	BEGIN
		SET @ErrMsg = 'There is an existing In Progress Variation for Sales Estimate ' + CONVERT(NVARCHAR(50), @estimateId) + '.'
		SET @ErrSeverity = 16
		RAISERROR(@ErrMsg, @ErrSeverity, 1)	
	END
	
	IF @revisionTypeId = 14 --Pre Site Variation CSC
	BEGIN
		SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
		FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc
		ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader 
		WHERE fkidEstimate = @estimateId AND Doc.Active = 1 
		AND fkid_SalesEstimate_RevisionType = 14 --Pre Site Variation CSC
		AND fkid_SalesEstimate_Status = 2 --Accepted
		ORDER BY id_SalesEstimate_EstimateHeader DESC
		
		--IF Accepted variation is not found, use Final Contract as a based estimate 
		--IF @@ROWCOUNT = 0
		--BEGIN
		--	SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
		--	FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId 
		--	AND fkid_SalesEstimate_RevisionType = 13 --Final Contract
		--	AND fkid_SalesEstimate_Status = 2 --Accepted
		--	ORDER BY id_SalesEstimate_EstimateHeader DESC
		--END
		
		--IF Accepted variation is not found, use CSC as a based estimate 
		IF @@ROWCOUNT = 0
		BEGIN
			SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
			FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId 
			AND fkid_SalesEstimate_RevisionType = 5 --CSC
			AND fkid_SalesEstimate_Status = 2 --Accepted
			ORDER BY id_SalesEstimate_EstimateHeader DESC
		END
	END
	ELSE IF @revisionTypeId = 18 --Building Variation BSC
	BEGIN
		SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
		FROM tbl_SalesEstimate_EstimateHeader  Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc 
		ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader 
		WHERE fkidEstimate = @estimateId AND Doc.Active = 1
		AND fkid_SalesEstimate_RevisionType = 18 --Building Variation BSC
		AND fkid_SalesEstimate_Status = 2 --Accepted
		ORDER BY id_SalesEstimate_EstimateHeader DESC
		
		--IF Accepted Building Variation is not found, try to use Pre Site Variation as a based estimate
		IF @@ROWCOUNT = 0
		BEGIN
			SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
			FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc
			ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader 
			WHERE fkidEstimate = @estimateId AND Doc.Active = 1 
			AND fkid_SalesEstimate_RevisionType = 14 --Pre Site Variation CSC
			AND fkid_SalesEstimate_Status = 2 --Accepted
			ORDER BY id_SalesEstimate_EstimateHeader DESC
			
			--IF Accepted variation is not found, use CSC as a based estimate 
			IF @@ROWCOUNT = 0
			BEGIN
				SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
				FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId 
				AND fkid_SalesEstimate_RevisionType = 5 --CSC
				AND fkid_SalesEstimate_Status = 2 --Accepted
				ORDER BY id_SalesEstimate_EstimateHeader DESC
			END				
		END
		
	
	END
	ELSE IF @revisionTypeId = 24 --Pre Studio M Variation CSC
	BEGIN
		SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
		FROM tbl_SalesEstimate_EstimateHeader  Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc
		ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader 
		WHERE fkidEstimate = @estimateId AND Doc.Active = 1
		AND fkid_SalesEstimate_RevisionType = 24 --Pre Studio M Variation CSC
		AND fkid_SalesEstimate_Status = 2 --Accepted
		ORDER BY id_SalesEstimate_EstimateHeader DESC	
		
		--IF Accepted variation is not found, use CSC as a based estimate 
		IF @@ROWCOUNT = 0
		BEGIN
			SELECT TOP 1 @previousRevisionHeaderId = id_SalesEstimate_EstimateHeader 
			FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId 
			AND fkid_SalesEstimate_RevisionType = 5 --CSC
			AND fkid_SalesEstimate_Status = 2 --Accepted
			ORDER BY id_SalesEstimate_EstimateHeader DESC
		END	
				
	END
	ELSE IF @revisionTypeId = 23 --Skip Studio M Revisions
	BEGIN
		SET @previousRevisionHeaderId = @estimateRevisionId				
	END

	-- Increment Revision Number by 1		
	SET @revisionNumber = @revisionNumber + 1
	
	BEGIN TRY

		BEGIN TRANSACTION
	
			INSERT INTO tbl_SalesEstimate_EstimateHeader
			(fkidEstimate, 
			RevisionNumber, 
			fkid_SalesEstimate_RevisionType, 
			fkidOwner,
			fkid_SalesEstimate_Status, 
			HomePrice, 
			HomePriceEffectiveDate, 
			CreatedOn, 
			CreatedBy, 
			ModifiedOn, 
			ModifiedBy,
			ContractType,
			HomeDisplayName,
			fkid_salesestimate_MRSGroup)
			VALUES 
			(@estimateId, 
			@revisionNumber, 
			@revisionTypeId, 
			@userId, 
			1, 
			@homePrice, 
			@effectiveDate, 
			GETDATE(), 
			@userId, 
			GETDATE(), 
			@userId,
			@contractType,
			@homeName,
			@mrsgroupid)	

			SET @newEstimateRevisionId = SCOPE_IDENTITY()		

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
			fkidStandardInclusions,			
			AdditionalInfo,
			SelectedImageID,
		    fkid_NonStandardPriceDisplayCode,
			IsSitework,
			DerivedCost,
			CostExcGST,
			CostOverWriteBy,
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
			@newEstimateRevisionId AS fkid_SalesEstimate_EstimateHeader, 
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
			@userId, 
			fkid_NonStandardArea,
			fkid_NonStandardGroup, 
			fkidStandardInclusions,
			AdditionalInfo,
			SelectedImageID,
			fkid_NonStandardPriceDisplayCode,
			IsSitework,
			DerivedCost,
			CostExcGST,
			CostOverWriteBy,
			AreaSortOrder,
			GroupSortOrder,
			ProductSortOrder,
			ItemAccepted,
			SalesEstimatorAccepted,
			fkidArea,
			AreaName,
			fkidGroup,
			GroupName,
			fkidProductAreaGroup,
			ProductName,
			IsPromotionProduct	
			FROM tbl_SalesEstimate_EstimateDetails 
			WHERE fkid_SalesEstimate_EstimateHeader = @previousRevisionHeaderId
			
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
