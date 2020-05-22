----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_RejectVariation]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_RejectVariation]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_RejectVariation]
	@estimateRevisionId INT, @userId INT
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND Active = 1)
	BEGIN
		UPDATE tbl_SalesEstimate_CustomerDocument SET Active = 0, ModifiedBy = @userId, ModifiedDate = GETDATE() 
		WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND Active = 1
	END
	
	SET NOCOUNT OFF;
END