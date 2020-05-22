
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_AcceptOriginalEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_AcceptOriginalEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_AcceptOriginalEstimate]
	-- Add the parameters for the stored procedure here
	@estimateId INT
AS
BEGIN
	DECLARE @revisionId INT
    DECLARE @ownerId INT

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT

	BEGIN TRY

		BEGIN TRANSACTION
			
			-- Update Accept Estimate
			UPDATE Estimate SET
			FKEstimatestatusid = 3, 
			AcceptDate = getdate()
			WHERE EstimateID = @estimateId

			EXEC sp_SalesEstimate_CreateEstimateFromOriginalEstimate @estimateId

			SELECT TOP 1 @revisionId = id_SalesEstimate_EstimateHeader, @ownerId = fkidOwner 
			FROM tbl_SalesEstimate_EstimateHeader
			WHERE fkidEstimate = @estimateId
			ORDER BY CreatedOn DESC

			EXEC sp_SalesEstimate_CompleteEstimateRevision @revisionId, @ownerId, 2, 7, 2, 0 -- Accept with no issues

		COMMIT

	END TRY

	BEGIN CATCH

		IF @@TRANCOUNT > 0
			ROLLBACK
		
		-- Raise an error
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)

	END CATCH

	SELECT @revisionId AS estimateHeaderId

END

GO