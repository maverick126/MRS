----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CreateContractDraft]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CreateContractDraft]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_CreateContractDraft] 
	@estimateRevisionId INT,
	@createdbyId INT
	--@appointment DateTime
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @revisionNumber INT
	DECLARE @homePrice MONEY
	DECLARE @effectiveDate DATETIME
	DECLARE @estimateId INT
	DECLARE @newEstimateRevisionId INT
	DECLARE @ownerId INT
	DECLARE @revisionTypeId INT
	DECLARE @contractType VARCHAR(10)
	DECLARE @homeName VARCHAR(250)
	DECLARE @mrsgroupid INT
	
	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT

	SET @revisionTypeId = 6 -- Contract Draft
	
	BEGIN TRY

		BEGIN TRANSACTION
		
			-- Select the Base Revision of the Sales Estimate 
			SELECT
				@estimateId = fkidEstimate,
				@contractType=ContractType,
				@homePrice = HomePrice,
				@effectiveDate = HomePriceEffectiveDate,
				@ownerId = fkidOwner,
				@homeName = HomeDisplayName,
				@mrsgroupid=fkid_salesestimate_MRSGroup
			FROM tbl_SalesEstimate_EstimateHeader
			WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
			
			-- If the Sales Estimate Revision exists
			IF @@ROWCOUNT > 0
			BEGIN

				-- Select the latest Revision of the Sales Estimate 
				SELECT TOP 1
					@revisionNumber = RevisionNumber
				FROM tbl_SalesEstimate_EstimateHeader
				WHERE fkidEstimate = @estimateId
				ORDER BY RevisionNumber DESC

				-- Increment Revision Number by 1		
				SET @revisionNumber = @revisionNumber + 1
				
				INSERT INTO tbl_SalesEstimate_EstimateHeader
				(fkidEstimate, 
				RevisionNumber, 
				fkid_SalesEstimate_RevisionType, 
				fkidOwner, 
				fkid_SalesEstimate_Status,
				fkid_SalesEstimate_StatusReason, 
				HomePrice,
				HomePriceEffectiveDate,
				--AppointmentDateTime,
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
				@ownerId, 
				2,
				47, 
				@homePrice, 
				@effectiveDate, 
				--@appointment,
				GETDATE(), 
				@createdbyId, 
				GETDATE(), 
				@createdbyId,
				@contractType, 
				@homeName,
				@mrsgroupid)	

				SET @newEstimateRevisionId = @@IDENTITY

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
				IsSiteWork,
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
				@createdbyId, 
				fkid_NonStandardArea, 
				fkid_NonStandardGroup,
				fkidStandardInclusions,
				AdditionalInfo,
				SelectedImageID,
				fkid_NonStandardPriceDisplayCode,
				IsSiteWork,
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
				FROM tbl_SalesEstimate_EstimateDetails 
				WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId			
						
				--Add Standard Inclusions
				EXEC sp_SalesEstimate_AddStandardInclusions
				@estimateRevisionId, @newEstimateRevisionId, @createdbyId

			END
			ELSE
			BEGIN

					SET @ErrMsg = 'The Sales Estimate Revision Id ' + CONVERT(NVARCHAR(50), @estimateRevisionId) + ' could not be found.'
					SET @ErrSeverity = 16
					RAISERROR(@ErrMsg, @ErrSeverity, 1)

			END		
	
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

