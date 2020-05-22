
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_PopulateBcEventProcessingQueue]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_PopulateBcEventProcessingQueue]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_PopulateBcEventProcessingQueue]
@eventRegisterId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	BEGIN TRY

		BEGIN TRANSACTION
			
			DECLARE @rows INT
		
			INSERT INTO tbl_SalesEstimate_BcEventProcessingQueue (fkid_SalesEstimate_EventRegister, fkid_SalesEstimate_BcEvent)
			SELECT id_SalesEstimate_EventRegister AS EventRegisterId,
			BE.id_SalesEstimate_BcEvent AS BcEventId
			FROM tbl_SalesEstimate_EventRegister SER 
			INNER JOIN tbl_SalesEstimate_EstimateHeader SEH ON SER.fkid_SalesEstimate_EstimateHeader = SEH.id_SalesEstimate_EstimateHeader
			INNER JOIN tbl_SalesEstimate_EstimateEvent EE ON EE.id_SalesEstimate_EstimateEvent = SER.fkid_SalesEstimate_EstimateEvent
			INNER JOIN tbl_SalesEstimate_EstimateAction EA ON EE.fkid_SalesEstimate_EstimateAction = EA.id_SalesEstimate_EstimateAction
			INNER JOIN tbl_SalesEstimate_RevisionType RT ON RT.id_SalesEstimate_RevisionType = SEH.fkid_SalesEstimate_RevisionType
			INNER JOIN tbl_SalesEstimate_BcEvent BE ON BE.fkid_SalesEstimate_EstimateEvent = EE.id_SalesEstimate_EstimateEvent AND BE.fkidMRSGroup = SEH.fkid_SalesEstimate_MRSGroup
			INNER JOIN tbl_SalesEstimate_BcAction BA ON BE.fkid_SalesEstimate_BcAction = BA.id_SalesEstimate_BcAction
			WHERE SER.Active = 1 AND BA.Active = 1 AND BE.Active = 1 AND SER.id_SalesEstimate_EventRegister = @eventRegisterId 
			ORDER BY BE.Sequence, BE.id_SalesEstimate_BcEvent
			
			SET @rows = @@ROWCOUNT
			
			IF @rows > 0
			BEGIN
				UPDATE tbl_SalesEstimate_EventRegister SET Active = 0, Processed = 1, LastErrorMessage = '', ProcessedOn = GETDATE() WHERE id_SalesEstimate_EventRegister = @eventRegisterId
			END
			ELSE
			BEGIN
				UPDATE tbl_SalesEstimate_EventRegister SET Active = 0, LastErrorMessage = 'No Action Required', ProcessedOn = GETDATE() WHERE id_SalesEstimate_EventRegister = @eventRegisterId
			END
		COMMIT

	END TRY

	BEGIN CATCH

		IF @@TRANCOUNT > 0
			ROLLBACK
		
		UPDATE tbl_SalesEstimate_EventRegister SET 
			LastErrorMessage = LEFT(ERROR_MESSAGE(),250) , 
			ProcessedOn = GETDATE(), 
			FailureCount = FailureCount + 1, 
			Active = CASE WHEN FailureCount >=2  THEN 0 ELSE 1 END  
			WHERE id_SalesEstimate_EventRegister = @eventRegisterId

	END CATCH

END

GO
