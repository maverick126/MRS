----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CreateStudioMEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CreateStudioMEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_CreateStudioMEstimate]
	@estimateRevisionId INT, 
	@ownerId INT, 
	@appointmentDate DATETIME, 
	@revisionTypeId INT, 
	@createdbyId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @revisionNumber INT
	DECLARE @homePrice MONEY
	DECLARE @effectiveDate DATETIME
	DECLARE @estimateId INT
	DECLARE @mrsgroupid INT
	DECLARE @newEstimateRevisionId INT
	DECLARE @contractType VARCHAR(10)
	DECLARE @tempmrsgroupid VARCHAR(200)
	DECLARE @homeName VARCHAR(250)

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT
	
	BEGIN TRY

		BEGIN TRANSACTION
		
			-- Select the Base Revision of the Sales Estimate 
			SELECT
				@estimateId = fkidEstimate,
				@contractType=contractType,
				@homePrice = HomePrice,
				@effectiveDate = HomePriceEffectiveDate,
				@homeName = HomeDisplayName	,
				@mrsgroupid =fkid_salesestimate_MRSGroup
			FROM tbl_SalesEstimate_EstimateHeader eh
			INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
			INNER JOIN Region r ON e.RegionID=r.RegionID
			WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
-- get excluded area and group
			DECLARE @temp TABLE
			(
			   id INT,
			   idtype VARCHAR(20)
			)
			INSERT INTO @temp
			EXEC sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision @estimateRevisionId
			
			SELECT id AS fkidarea
			INTO #excludearea
			FROM @temp 
			WHERE idtype='AREA'
			
			SELECT id AS fkidgroup
			INTO #excludegroup
			FROM @temp 
			WHERE idtype='GROUP'	

 -- end get excluded area and group  
	  
			-- If the Sales Estimate Revision exists
			IF (@mrsgroupid IS NOT NULL AND @mrsgroupid>0)
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
				HomePrice, 
				AppointmentDateTime,
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
				@ownerId, 
				1, 
				@homePrice, 
				@appointmentDate, 
				@effectiveDate, 
				GETDATE(), 
				@createdbyId, 
				GETDATE(), 
				@createdbyId,
				@contractType,
				@homeName,
				@mrsgroupid)	

				SET @newEstimateRevisionId = @@IDENTITY

				IF EXISTS (SELECT id_SalesEstimate_RevisionTypeAreaGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
				WHERE fkid_SalesEstimate_RevisionType = @revisionTypeId AND ExcludeDefinedAreaGroup = 1)
				BEGIN
				
					-- Standard Options
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
					@newEstimateRevisionId, 
					fkidEstimateDetails, 
					ItemPrice, 
					tbl_SalesEstimate_EstimateDetails.Quantity, 
					tbl_SalesEstimate_EstimateDetails.ProductDescription, 
					tbl_SalesEstimate_EstimateDetails.ExtraDescription, 
					tbl_SalesEstimate_EstimateDetails.InternalDescription, 
					CAST(StudioMAttributes AS VARCHAR(MAX)),
					getdate(),
					@createdbyId,
					getdate(),
					@createdbyId,
					fkid_NonStandardArea, 
					fkid_NonStandardGroup,					
					fkidStandardInclusions,
					tbl_SalesEstimate_EstimateDetails.AdditionalInfo ,			
					tbl_SalesEstimate_EstimateDetails.SelectedImageID,
					tbl_SalesEstimate_EstimateDetails.fkid_NonStandardPriceDisplayCode,
					tbl_SalesEstimate_EstimateDetails.IsSitework,
					tbl_SalesEstimate_EstimateDetails.DerivedCost,
					tbl_SalesEstimate_EstimateDetails.CostExcGST,
					tbl_SalesEstimate_EstimateDetails.CostOverWriteBy,
					tbl_SalesEstimate_EstimateDetails.AreaSortOrder,
					tbl_SalesEstimate_EstimateDetails.GroupSortOrder,
					tbl_SalesEstimate_EstimateDetails.ProductSortOrder,					
					tbl_SalesEstimate_EstimateDetails.ItemAccepted,
					tbl_SalesEstimate_EstimateDetails.SalesEstimatorAccepted	
				   ,tbl_SalesEstimate_EstimateDetails.fkidArea
				   ,tbl_SalesEstimate_EstimateDetails.AreaName
				   ,tbl_SalesEstimate_EstimateDetails.fkidGroup
				   ,tbl_SalesEstimate_EstimateDetails.GroupName
				   ,tbl_SalesEstimate_EstimateDetails.fkidProductAreaGroup
				   ,tbl_SalesEstimate_EstimateDetails.ProductName
				   ,tbl_SalesEstimate_EstimateDetails.IsPromotionProduct											
					FROM tbl_SalesEstimate_EstimateDetails
					 
					INNER JOIN EstimateDetails ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID 
					
					INNER JOIN Product ON EstimateDetails.ProductID = Product.ProductID
					
					WHERE tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = @estimateRevisionId
					
					AND (EstimateDetails.AreaId NOT IN (SELECT fkidArea FROM #excludearea)
					
					AND EstimateDetails.GroupId NOT IN (SELECT fkidGroup FROM #excludegroup)) 
					
					AND EstimateDetails.AreaId <> 43 -- Non Standard Request
					
					--AND Product.isStudioMProduct = 1
					
								
					--Non Standard Request
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
					@newEstimateRevisionId, 
					fkidEstimateDetails, 
					ItemPrice, 
					tbl_SalesEstimate_EstimateDetails.Quantity, 
					tbl_SalesEstimate_EstimateDetails.ProductDescription, 
					tbl_SalesEstimate_EstimateDetails.ExtraDescription, 
					tbl_SalesEstimate_EstimateDetails.InternalDescription, 
					CAST(StudioMAttributes AS VARCHAR(MAX)),
					getdate(),
					@createdbyId,
					getdate(),
					@createdbyId,
					fkid_NonStandardArea, 
					fkid_NonStandardGroup,					
					fkidStandardInclusions,
					tbl_SalesEstimate_EstimateDetails.AdditionalInfo,
					tbl_SalesEstimate_EstimateDetails.SelectedImageID,
					tbl_SalesEstimate_EstimateDetails.fkid_NonStandardPriceDisplayCode,
					tbl_SalesEstimate_EstimateDetails.IsSitework,
					tbl_SalesEstimate_EstimateDetails.DerivedCost,
					tbl_SalesEstimate_EstimateDetails.CostExcGST,
					tbl_SalesEstimate_EstimateDetails.CostOverWriteBy,
					tbl_SalesEstimate_EstimateDetails.AreaSortOrder,
					tbl_SalesEstimate_EstimateDetails.GroupSortOrder,
					tbl_SalesEstimate_EstimateDetails.ProductSortOrder,
					tbl_SalesEstimate_EstimateDetails.ItemAccepted,
					tbl_SalesEstimate_EstimateDetails.SalesEstimatorAccepted	
				   ,tbl_SalesEstimate_EstimateDetails.fkidArea
				   ,tbl_SalesEstimate_EstimateDetails.AreaName
				   ,tbl_SalesEstimate_EstimateDetails.fkidGroup
				   ,tbl_SalesEstimate_EstimateDetails.GroupName
				   ,tbl_SalesEstimate_EstimateDetails.fkidProductAreaGroup
				   ,tbl_SalesEstimate_EstimateDetails.ProductName
				   ,tbl_SalesEstimate_EstimateDetails.IsPromotionProduct										 
					FROM tbl_SalesEstimate_EstimateDetails 
					
					INNER JOIN EstimateDetails ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID  
					
					INNER JOIN Product ON EstimateDetails.ProductID = Product.ProductID
					
					WHERE tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = @estimateRevisionId
					
					AND (tbl_SalesEstimate_EstimateDetails.fkid_NonStandardArea NOT IN (SELECT fkidArea FROM #excludearea)
					
					AND tbl_SalesEstimate_EstimateDetails.fkid_NonStandardGroup NOT IN (SELECT fkidGroup FROM #excludegroup) )
					
					AND EstimateDetails.AreaId = 43 -- Non Standard Request	
					
					--AND Product.isStudioMProduct = 1
					

				END
				ELSE
				BEGIN
					
					-- Standard Options
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
					@newEstimateRevisionId, 
					fkidEstimateDetails, 
					ItemPrice, 
					tbl_SalesEstimate_EstimateDetails.Quantity, 
					tbl_SalesEstimate_EstimateDetails.ProductDescription, 
					tbl_SalesEstimate_EstimateDetails.ExtraDescription, 
					tbl_SalesEstimate_EstimateDetails.InternalDescription, 
					CAST(StudioMAttributes AS VARCHAR(MAX)),
					getdate(),
					@createdbyId,
					getdate(),
					@createdbyId,
					fkid_NonStandardArea, 
					fkid_NonStandardGroup,					
					fkidStandardInclusions,
					tbl_SalesEstimate_EstimateDetails.AdditionalInfo , 
					tbl_SalesEstimate_EstimateDetails.SelectedImageID,
					tbl_SalesEstimate_EstimateDetails.fkid_NonStandardPriceDisplayCode,
					tbl_SalesEstimate_EstimateDetails.IsSitework,
					tbl_SalesEstimate_EstimateDetails.DerivedCost,
					tbl_SalesEstimate_EstimateDetails.CostExcGST,
					tbl_SalesEstimate_EstimateDetails.CostOverWriteBy,
					tbl_SalesEstimate_EstimateDetails.AreaSortOrder,
					tbl_SalesEstimate_EstimateDetails.GroupSortOrder,
					tbl_SalesEstimate_EstimateDetails.ProductSortOrder,
					tbl_SalesEstimate_EstimateDetails.ItemAccepted,
					tbl_SalesEstimate_EstimateDetails.SalesEstimatorAccepted
				   ,tbl_SalesEstimate_EstimateDetails.fkidArea
				   ,tbl_SalesEstimate_EstimateDetails.AreaName
				   ,tbl_SalesEstimate_EstimateDetails.fkidGroup
				   ,tbl_SalesEstimate_EstimateDetails.GroupName
				   ,tbl_SalesEstimate_EstimateDetails.fkidProductAreaGroup
				   ,tbl_SalesEstimate_EstimateDetails.ProductName
				   ,tbl_SalesEstimate_EstimateDetails.IsPromotionProduct										
					FROM tbl_SalesEstimate_EstimateDetails
					 
					INNER JOIN EstimateDetails ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID 
					
					INNER JOIN Product ON EstimateDetails.ProductID = Product.ProductID
					
					WHERE tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = @estimateRevisionId
					
					AND (EstimateDetails.AreaId IN (SELECT fkidArea FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
					WHERE fkidArea IS NOT NULL AND fkid_SalesEstimate_RevisionType = @revisionTypeId)
					
					OR EstimateDetails.GroupId IN (SELECT fkidGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
					WHERE fkidGroup IS NOT NULL AND fkid_SalesEstimate_RevisionType = @revisionTypeId))
					
					AND EstimateDetails.AreaId <> 43 -- Non Standard Request
					
					--AND Product.isStudioMProduct = 1
					
					
					
					--Non Standard Request
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
					@newEstimateRevisionId, 
					fkidEstimateDetails, 
					ItemPrice, 
					tbl_SalesEstimate_EstimateDetails.Quantity, 
					tbl_SalesEstimate_EstimateDetails.ProductDescription, 
					tbl_SalesEstimate_EstimateDetails.ExtraDescription, 
					tbl_SalesEstimate_EstimateDetails.InternalDescription, 
					CAST(StudioMAttributes AS VARCHAR(MAX)),
					getdate(),
					@createdbyId,
					getdate(),
					@createdbyId,
					fkid_NonStandardArea, 
					fkid_NonStandardGroup,					
					fkidStandardInclusions,
					tbl_SalesEstimate_EstimateDetails.AdditionalInfo, 
					tbl_SalesEstimate_EstimateDetails.SelectedImageID,
					tbl_SalesEstimate_EstimateDetails.fkid_NonStandardPriceDisplayCode,
					tbl_SalesEstimate_EstimateDetails.IsSitework,
					tbl_SalesEstimate_EstimateDetails.DerivedCost,
					tbl_SalesEstimate_EstimateDetails.CostExcGST,
					tbl_SalesEstimate_EstimateDetails.CostOverWriteBy,
					tbl_SalesEstimate_EstimateDetails.AreaSortOrder,
					tbl_SalesEstimate_EstimateDetails.GroupSortOrder,
					tbl_SalesEstimate_EstimateDetails.ProductSortOrder,
					tbl_SalesEstimate_EstimateDetails.ItemAccepted,
					tbl_SalesEstimate_EstimateDetails.SalesEstimatorAccepted	
				   ,tbl_SalesEstimate_EstimateDetails.fkidArea
				   ,tbl_SalesEstimate_EstimateDetails.AreaName
				   ,tbl_SalesEstimate_EstimateDetails.fkidGroup
				   ,tbl_SalesEstimate_EstimateDetails.GroupName
				   ,tbl_SalesEstimate_EstimateDetails.fkidProductAreaGroup
				   ,tbl_SalesEstimate_EstimateDetails.ProductName
				   ,tbl_SalesEstimate_EstimateDetails.IsPromotionProduct										
					FROM tbl_SalesEstimate_EstimateDetails 
					
					INNER JOIN EstimateDetails ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID  
					
					INNER JOIN Product ON EstimateDetails.ProductID = Product.ProductID
					
					WHERE tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = @estimateRevisionId
					
					AND (tbl_SalesEstimate_EstimateDetails.fkid_NonStandardArea IN (SELECT fkidArea FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
					WHERE fkidArea IS NOT NULL AND fkid_SalesEstimate_RevisionType = @revisionTypeId)
					
					OR tbl_SalesEstimate_EstimateDetails.fkid_NonStandardGroup IN (SELECT fkidGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
					WHERE fkidGroup IS NOT NULL AND fkid_SalesEstimate_RevisionType = @revisionTypeId)) 
					
					AND EstimateDetails.AreaId = 43 -- Non Standard Request	
					
					--AND Product.isStudioMProduct = 1
					

				END	
                SELECT @newEstimateRevisionId AS newEstimateRevisionId
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