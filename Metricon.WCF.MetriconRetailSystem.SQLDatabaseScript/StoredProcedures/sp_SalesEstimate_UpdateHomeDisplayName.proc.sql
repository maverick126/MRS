IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateHomeDisplayName]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateHomeDisplayName]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateHomeDisplayName] 
@revisionid				INT,
@homename				VARCHAR(250) = NULL,
@userid					INT
AS
BEGIN

	SET NOCOUNT ON;

         UPDATE tbl_SalesEstimate_EstimateHeader
         SET    HomeDisplayName=@homename,
                ModifiedBy=@userid,
                ModifiedOn=GETDATE()
         WHERE  id_SalesEstimate_EstimateHeader=@revisionid

	SET NOCOUNT OFF;
END