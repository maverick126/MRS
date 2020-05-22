
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateEstimateComment]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateComment]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateComment]
	@estimateRevisionId int, 
	@comments varchar(max),
	@variationnumber    INT,
	@variationsummary   VARCHAR(40)	
AS
BEGIN

	UPDATE tbl_SalesEstimate_EstimateHeader
	SET Comments = @comments, 
	CommentModifiedDate = GETDATE()
	WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
	-- if accept a variation, then save the variation summary
	IF(@variationsummary<>'')
	   BEGIN
	      UPDATE tbl_SalesEstimate_CustomerDocument
	      SET [Description]=@variationsummary
	      WHERE fkid_SalesEstimate_EstimateHeader=@estimateRevisionId AND
	            DocumentType='Variation' AND
	            DocumentNumber=@variationnumber
	   END
	-- end of variation summary	

END

GO
