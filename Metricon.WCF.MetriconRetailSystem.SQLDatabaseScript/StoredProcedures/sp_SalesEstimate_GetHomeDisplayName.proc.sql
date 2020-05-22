IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetHomeDisplayName]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetHomeDisplayName]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetHomeDisplayName]
@revisionId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT HomeDisplayName FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId	

END