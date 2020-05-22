----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_MergeStudioMRevisions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_MergeStudioMRevisions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_MergeStudioMRevisions]
	@estimateRevisionId INT, 
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
	DECLARE @newEstimateRevisionId INT
	DECLARE @ownerId INT
	DECLARE @revisionTypeId INT
	DECLARE @contractType VARCHAR(10)
    DECLARE @homeName VARCHAR(250)	
	DECLARE @mrsgroupid INT
	
	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT
	
	SET @revisionTypeId = 23 -- Studio M Revision Type
	
	BEGIN TRY

		BEGIN TRANSACTION
		
			-- Select the Base Revision of the Sales Estimate 
			SELECT
				@estimateId = fkidEstimate,
				@homePrice = HomePrice,
				@effectiveDate = HomePriceEffectiveDate,
				@ownerId = fkidOwner,
				@contractType = ContractType,
				@homeName = HomeDisplayName	,
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
				@ownerId, 
				1,
				@homePrice, 
				@effectiveDate,
				GETDATE(), 
				@createdbyId, 
				GETDATE(), 
				@createdbyId,
				@contractType,
				@homeName,
				@mrsgroupid)	

				SET @newEstimateRevisionId = SCOPE_IDENTITY()


				DECLARE @temp TABLE
				(
				   id INT,
				   idtype VARCHAR(20)
				)
				INSERT INTO @temp
				EXEC sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision @estimateRevisionId

				SELECT AreaID INTO #StudioMAreas FROM (
				--Non Colour Selection
				SELECT DISTINCT fkidArea AS AreaID  FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidArea IS NOT NULL
				AND 
				(
					fkid_SalesEstimate_RevisionType IN 
					(
						SELECT DISTINCT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId
					)
				)
				UNION
				--Colour Selection (WHEN Colour Selection Revision Exists)
				SELECT AreaID FROM [Area] WHERE AreaID NOT IN 
				(
					SELECT fkidArea FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidArea IS NOT NULL 
					AND fkidArea IN (SELECT id FROM @temp WHERE idtype='AREA')
				)
				AND EXISTS 
				(
					SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType IN 
					(
						SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE ExcludeDefinedAreaGroup = 1
					)
				)				
				)StudioMAreas			

				SELECT GroupID INTO #StudioMGroups FROM (
				--Non Colour Selection
				SELECT DISTINCT fkidGroup AS GroupID  FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidGroup IS NOT NULL
				AND 
				(
					fkid_SalesEstimate_RevisionType IN 
					(
						SELECT DISTINCT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId
					)
				)

				UNION

				--Colour Selection (WHEN Colour Selection Revision Exists)
				SELECT GroupID FROM [Group] WHERE GroupID NOT IN 
				(
					SELECT fkidGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidGroup IS NOT NULL 
					AND fkidGroup IN (SELECT id FROM @temp WHERE idtype='GROUP')
				)
				AND EXISTS 
				(
					SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType IN 
					(
						SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE ExcludeDefinedAreaGroup = 1
					)
				)				
				)StudioMGroups
				

				--SELECT Products from ALL Studio M Revisions
				INSERT INTO tbl_SalesEstimate_EstimateDetails 
				   (
						fkid_SalesEstimate_EstimateHeader, 
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
						IsSitework,
						CostExcGST,
						CostOverWriteBy,
						DerivedCost,
						AreaSortOrder,
						GroupSortOrder,
						ProductSortOrder,
						ItemAccepted,
						SalesEstimatorAccepted	
					   ,[fkidArea]
					   ,[AreaName]
					   ,[fkidGroup]
					   ,[GroupName]
					   ,[fkidProductAreaGroup]
					   ,[ProductName]
					   ,[IsPromotionProduct]							
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
				tbl_SalesEstimate_EstimateDetails.IsSitework,
				tbl_SalesEstimate_EstimateDetails.CostExcGST,
				tbl_SalesEstimate_EstimateDetails.CostOverWriteBy,
				tbl_SalesEstimate_EstimateDetails.DerivedCost,
				tbl_SalesEstimate_EstimateDetails.AreaSortOrder,
				tbl_SalesEstimate_EstimateDetails.GroupSortOrder,
				tbl_SalesEstimate_EstimateDetails.ProductSortOrder,
				tbl_SalesEstimate_EstimateDetails.ItemAccepted,
				tbl_SalesEstimate_EstimateDetails.SalesEstimatorAccepted
			   ,tbl_SalesEstimate_EstimateDetails.[fkidArea]
			   ,tbl_SalesEstimate_EstimateDetails.[AreaName]
			   ,tbl_SalesEstimate_EstimateDetails.[fkidGroup]
			   ,tbl_SalesEstimate_EstimateDetails.[GroupName]
			   ,tbl_SalesEstimate_EstimateDetails.[fkidProductAreaGroup]
			   ,tbl_SalesEstimate_EstimateDetails.[ProductName]
			   ,tbl_SalesEstimate_EstimateDetails.[IsPromotionProduct]					
				FROM tbl_SalesEstimate_EstimateDetails 

				INNER JOIN tbl_SalesEstimate_EstimateHeader ON 
				tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = tbl_SalesEstimate_EstimateHeader.id_SalesEstimate_EstimateHeader

				WHERE fkidEstimate = @estimateId 
				AND fkid_SalesEstimate_RevisionType IN 
				(
					SELECT DISTINCT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAreagroup
				)



				--SELECT Non-Studio M Standard Options or Studio M Standard Options but no Studio M Revision from the Contract Draft  
				INSERT INTO tbl_SalesEstimate_EstimateDetails 
				   (
						fkid_SalesEstimate_EstimateHeader, 
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
						IsSitework,
						CostExcGST,
						CostOverWriteBy,
						DerivedCost,
						AreaSortOrder,
						GroupSortOrder,
						ProductSortOrder,
						ItemAccepted,
						SalesEstimatorAccepted
					   ,[fkidArea]
					   ,[AreaName]
					   ,[fkidGroup]
					   ,[GroupName]
					   ,[fkidProductAreaGroup]
					   ,[ProductName]
					   ,[IsPromotionProduct]								
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
				tbl_SalesEstimate_EstimateDetails.IsSitework,
				tbl_SalesEstimate_EstimateDetails.CostExcGST,
				tbl_SalesEstimate_EstimateDetails.CostOverWriteBy,
				tbl_SalesEstimate_EstimateDetails.DerivedCost,
				tbl_SalesEstimate_EstimateDetails.AreaSortOrder,
				tbl_SalesEstimate_EstimateDetails.GroupSortOrder,
				tbl_SalesEstimate_EstimateDetails.ProductSortOrder,	
				tbl_SalesEstimate_EstimateDetails.ItemAccepted,
				tbl_SalesEstimate_EstimateDetails.SalesEstimatorAccepted
			   ,tbl_SalesEstimate_EstimateDetails.[fkidArea]
			   ,tbl_SalesEstimate_EstimateDetails.[AreaName]
			   ,tbl_SalesEstimate_EstimateDetails.[fkidGroup]
			   ,tbl_SalesEstimate_EstimateDetails.[GroupName]
			   ,tbl_SalesEstimate_EstimateDetails.[fkidProductAreaGroup]
			   ,tbl_SalesEstimate_EstimateDetails.[ProductName]
			   ,tbl_SalesEstimate_EstimateDetails.[IsPromotionProduct]								
				FROM tbl_SalesEstimate_EstimateDetails 

				INNER JOIN tbl_SalesEstimate_EstimateHeader 
				ON tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = tbl_SalesEstimate_EstimateHeader.id_SalesEstimate_EstimateHeader

				INNER JOIN EstimateDetails 
				ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID

				INNER JOIN Product 
				ON EstimateDetails.ProductID = Product.ProductID

				WHERE fkidEstimate = @estimateId 

				AND fkid_SalesEstimate_RevisionType = 6 --Contract Draft
/* 2014-06-30 WTE Commented out as Non Standard Request items are now in Studio M
				AND AreaId <> 43 --Non Standard Request
*/
				AND 
				(
/* 2014-06-30 WTE Commented out as all products are now in Studio M (not just Studio M products)				
					Product.isStudioMProduct = 0 --Product is NOT Studio M Product
					OR 
					(
*/					
						EstimateDetails.AreaId NOT IN --Area is NOT in Areas that are specified in existing Studio M Revisions
						( 
							SELECT AreaId FROM #StudioMAreas
						)
						AND
						EstimateDetails.GroupId NOT IN
						(
							SELECT GroupId FROM #StudioMGroups
						)
--					)
				)


/* 2014-06-30 WTE Commented out as Non Standard Request items are now in Studio M

				--SELECT Non-Studio M Non Standard Request from the Contract Draft  
				
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
				AdditionalInfo)		
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
				tbl_SalesEstimate_EstimateDetails.AdditionalInfo
				
				FROM tbl_SalesEstimate_EstimateDetails 

				INNER JOIN tbl_SalesEstimate_EstimateHeader ON 
				tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = tbl_SalesEstimate_EstimateHeader.id_SalesEstimate_EstimateHeader

				INNER JOIN EstimateDetails 
				ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID

				WHERE fkidEstimate = @estimateId 

				AND fkid_SalesEstimate_RevisionType = 6 --Contract Draft

				AND AreaId = 43 --Non Standard Request

				AND 
				(
					tbl_SalesEstimate_EstimateDetails.fkid_NonStandardArea NOT IN --Area is NOT in Areas that are specified in existing Studio M Revisions
					( 
						SELECT AreaId FROM #StudioMAreas
					)   
					AND 
					tbl_SalesEstimate_EstimateDetails.fkid_NonStandardGroup NOT IN --Group is NOT in Groups that are specified in existing Studio M Revisions
					( 
						SELECT GroupId FROM #StudioMGroups
					)
				)
*/
				
/* 2014-06-30 WTE Commented out as we no longer use fkidStandardInclusions
		
				--SELECT Non-Studio M Standard Inclusions or Studio M Standard Inclusions but no Studio M Revision from the Contract Draft  
		
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
				AdditionalInfo)		
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
				tbl_SalesEstimate_EstimateDetails.AdditionalInfo
				
				FROM tbl_SalesEstimate_EstimateDetails 

				INNER JOIN tbl_SalesEstimate_EstimateHeader 
				ON tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = tbl_SalesEstimate_EstimateHeader.id_SalesEstimate_EstimateHeader

				INNER JOIN tblStandardInclusions 
				ON tbl_SalesEstimate_EstimateDetails.fkidStandardInclusions = tblStandardInclusions.idStandardInclusions

				INNER JOIN ProductAreaGroup 
				ON tblStandardInclusions.ProductAreaGroupID = ProductAreaGroup.ProductAreaGroupID

				INNER JOIN Product 
				ON ProductAreaGroup.ProductID = Product.ProductID

				WHERE fkidEstimate = @estimateId 

				AND fkid_SalesEstimate_RevisionType = 6 --Contract Draft

				AND 
				(
					Product.isStudioMProduct = 0 --Product is NOT Studio M Product
					OR 
					(
						ProductAreaGroup.AreaId NOT IN --Area is NOT in Areas that are specified in existing Studio M Revisions
						( 
							SELECT AreaId FROM #StudioMAreas
						)   
						AND 
						ProductAreaGroup.GroupId NOT IN --Group is NOT in Groups that are specified in existing Studio M Revisions
						( 
							SELECT GroupId FROM #StudioMGroups
						)
					)
				)
*/

				DROP TABLE #StudioMAreas
				DROP TABLE #StudioMGroups
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