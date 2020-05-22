----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateEstimateStatus]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateStatus]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateStatus]
	@revisionId int, @statusId int, @statusReasonId int, @userId int
AS
BEGIN
	
	IF @statusReasonId = 0
	BEGIN
		
		SET @statusReasonId = NULL

	END

	UPDATE tbl_SalesEstimate_EstimateHeader 
	SET 
	fkid_SalesEstimate_Status = @statusId, 
	fkid_SalesEstimate_StatusReason = @statusReasonId,
	ModifiedOn = GETDATE(), 
	ModifiedBy = @userId
	WHERE id_SalesEstimate_EstimateHeader = @revisionId
	
	
	IF(@statusId=2 AND EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader=@revisionId AND Active=1))
	   BEGIN  
	      -- if it's hia contract or variation and SE revision exists, update acceptance checkbox for SA and SE
			IF (EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 4 AND fkidEstimate 
			IN (SELECT fkidEstimate FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId)))
			BEGIN
				UPDATE tbl_SalesEstimate_EstimateDetails
				SET ItemAccepted=1, SalesEstimatorAccepted=1
				WHERE fkid_SalesEstimate_EstimateHeader=@revisionId
			END
			ELSE
			BEGIN
				UPDATE tbl_SalesEstimate_EstimateDetails
				SET ItemAccepted=1
				WHERE fkid_SalesEstimate_EstimateHeader=@revisionId			
			END
	   END

END

GO
