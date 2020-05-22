----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_DeleteEstimateDetails]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_DeleteEstimateDetails]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_DeleteEstimateDetails]
	@revisionDetailsId int, 
	@userId int,
	@reason varchar(MAX),
	@reasonid int
AS
BEGIN
    DECLARE @len	INT
    SET @len=100

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT

	DECLARE @revisionId INT, @standardinclusionid INT, @pagid INT
	DECLARE @estimateDetailsId INT

	SELECT 
	        @revisionId = fkid_SalesEstimate_EstimateHeader,
	        @estimateDetailsId = fkidEstimateDetails,
	        @standardinclusionid=fkidstandardinclusions
	FROM	tbl_SalesEstimate_EstimateDetails
	WHERE	id_SalesEstimate_EstimateDetails = @revisionDetailsId 
	
	IF (@estimateDetailsId IS NULL OR @estimateDetailsId = 0) -- this is standard inclusion, find out pag from tblstandardinclusions table
	    BEGIN
	        SELECT @pagid=productareagroupid FROM tblstandardinclusions	WHERE idstandardinclusions=@standardinclusionid
	    END
	ELSE -- this is standard option, find out pag from estimatedetails table
	    BEGIN
	        SELECT @pagid=productareagroupid FROM estimatedetails WHERE estimatedetailsid=@estimateDetailsId
	    END
	    	
	BEGIN TRY

		BEGIN TRANSACTION	

			DELETE FROM tbl_SalesEstimate_EstimateDetails	
			WHERE id_SalesEstimate_EstimateDetails = @revisionDetailsId

			UPDATE tbl_SalesEstimate_EstimateHeader
			SET ModifiedBy = @userId, 
				ModifiedOn = GETDATE()
			WHERE id_SalesEstimate_EstimateHeader = @revisionId	
			
			-- log the reason and deleted item to new table
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
			@revisionId,	
			@estimateDetailsId, 
			@standardinclusionid, 
			@pagid, 
			GETDATE(), 
			@userId, 
			RTRIM(@reason),
            @reasonid
		COMMIT

	END TRY

	BEGIN CATCH

		IF @@TRANCOUNT > 0
			ROLLBACK
		
		-- Raise an error
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)

	END CATCH

	SELECT	
		areaname		,
		groupname		,
		ProductID		,
		ProductName+'['+ProductID+']' AS ProductName,
		ProductDescription,
	    CASE  WHEN	LEN(ProductDescription)>@len
	          THEN	SUBSTRING(ProductDescription,1,@len)+' ...'
	          ELSE  ProductDescription
	          END
	    AS ProductDescriptionShort,
		EnterDesc,
		UOM,
		Quantity,
		CAST(SellPrice AS DECIMAL(18,2)) AS SellPrice,
		PromotionProduct,
		StandardOption,
		EstimateDetailsId,
		0 AS ItemAccepted
		
		FROM estimatedetails		ed
		WHERE estimatedetailsid = @estimateDetailsId

END


GO