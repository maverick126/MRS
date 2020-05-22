----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CompleteEstimateRevision]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CompleteEstimateRevision]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_CompleteEstimateRevision]
	@revisionId INT, 
	@userId INT,  -- User who completed the Sales Estimate
	@statusId INT, -- Status of the Sales Estimate i.e. Accepted, Rejected
	@statusReasonId INT, --- i.e. Accepted with Minor Issues
	@nextRevisionTypeId INT,
	@nextRevisionOwnerId INT
AS
BEGIN
	DECLARE @estimateId INT

--	DECLARE @nextRevisionTypeId INT  -- 0 to finalise the Sales Estimate
--	DECLARE @nextRevisionOwnerId INT -- 0 to put the Sales Estimate in the Queue
	DECLARE @revisionNumber INT
	DECLARE @type VARCHAR(20)
	DECLARE @contracttype VARCHAR(10)
	DECLARE @mrsgroupid INT

	DECLARE @nextRevisionStatusId INT
	SET @nextRevisionStatusId = 1 -- WIP
	
	IF (@statusId=3)
	    SET @type='REJECT'
	ELSE
		SET @type='ASSIGN'
	
	DECLARE @queueId INT
	SET @queueId = 0 -- Not from Queue
	
	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT

	DECLARE @existingStatusId INT 
	SELECT @existingStatusId = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId
	IF (@existingStatusId = @statusId)--If the estimate has already been updated
	BEGIN
		RETURN 
	END

	-- Get Estimate ID from Estimate Header
	SELECT 
	@estimateId = fkidEstimate,
	@revisionNumber = RevisionNumber,
	@contracttype=contracttype,
	@mrsgroupid=fkid_salesestimate_MRSGroup
	FROM tbl_SalesEstimate_EstimateHeader
	WHERE id_SalesEstimate_EstimateHeader = @revisionId

--	EXEC sp_SalesEstimate_GetNextEstimateRevisionType @revisionId, @statusId, @nextRevisionTypeId OUTPUT, @nextRevisionOwnerId OUTPUT	

	BEGIN TRY

		BEGIN TRANSACTION
			
			-- Update Sales Estimate Revision Status
			EXEC sp_SalesEstimate_UpdateEstimateStatus @revisionId, @statusId, @statusReasonId, @userId
			--select 111,@nextRevisionOwnerId,@estimateId as estimateid,@nextRevisionTypeId as revisiontype,@contracttype as ctype
			
			IF @statusId = 2 --Accepted
			BEGIN
				--Integrate Event to BC
				EXEC sp_SalesEstimate_CreateEstimateEventRegister 'accept', @revisionId, @userId
			END
			
			-- If Next Revision Type is specified
			IF @nextRevisionTypeId > 0
			BEGIN

				-- If new Owner is not specified
				IF @nextRevisionOwnerId = 0
				BEGIN
					
					-- Add Sales Estimate to the Queue
					IF(NOT EXISTS(SELECT * FROM tbl_SalesEstimate_Queue WHERE fkidEstimate=@estimateId AND fkid_SalesEstimate_RevisionType=@nextRevisionTypeId))
					   BEGIN
							INSERT INTO tbl_SalesEstimate_Queue 
							(fkidEstimate, fkid_SalesEstimate_RevisionType, 
							CreatedOn, ContractType, fkid_salesestimate_MRSGroup)--, DueDate)	
							VALUES (@estimateId, @nextRevisionTypeId, 
							getdate(), @contracttype, @mrsgroupid)--, @duedate)
					   END					
 
				END

				ELSE
				BEGIN
					
					DECLARE @nextEstimateRevisionId INT
					
					-- Create and assign new Sales Estimate Revision
					EXEC sp_SalesEstimate_CreateEstimateFromPreviousRevision 
					@estimateId, @nextRevisionOwnerId, @nextRevisionTypeId, @userId, @nextRevisionStatusId , @queueId, @nextEstimateRevisionId OUTPUT
					
					--Commented 19/08/2014 No longer required as Standard Inclusions are inserted in Ready for Studio M revision
					--IF @revisionNumber = 1
					--BEGIN
					--	EXEC sp_SalesEstimate_AddStandardInclusions
					--	@revisionId, @nextEstimateRevisionId, @userId
					--END
					
				END

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


	IF (@@ERROR=0)
	   BEGIN
	     IF (@userId<>@nextRevisionOwnerId AND @nextRevisionOwnerId>0 AND @nextEstimateRevisionId>0) -- not assign to me need email
	        BEGIN
            	 EXEC sp_SalesEstimate_SendAlertEmail  @userId,  @nextRevisionOwnerId, @nextEstimateRevisionId, @type
		    END
	   END
END
GO