 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CheckValidProductByRevision]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CheckValidProductByRevision]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO


ALTER PROCEDURE [dbo].[sp_SalesEstimate_CheckValidProductByRevision] 
@revisionId		INT, 
@productId VARCHAR(50)
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @revisionTypeId INT
	SELECT @revisionTypeId = fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId
	
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionType WHERE id_SalesEstimate_RevisionType = @revisionTypeId AND ExclusiveProductID IS NOT NULL)
	BEGIN
		IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionType 
			WHERE id_SalesEstimate_RevisionType = @revisionTypeId 
			AND SUBSTRING(ExclusiveProductId,2,10) = SUBSTRING(@productId,2,10))
			SELECT 1
		ELSE
			SELECT 0
	END
	ELSE
	BEGIN
		IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionType 
			WHERE SUBSTRING(ExclusiveProductId,2,10) = SUBSTRING(@productId,2,10))
			SELECT 0
		ELSE
			SELECT 1
	END

	SET NOCOUNT OFF;
END

GO