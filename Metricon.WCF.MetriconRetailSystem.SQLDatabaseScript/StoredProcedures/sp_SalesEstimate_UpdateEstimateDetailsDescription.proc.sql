IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateEstimateDetailsDescription]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateDetailsDescription]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateDetailsDescription]
	@revisionDetailsId INT, 
	@productDescription VARCHAR(MAX)='',
	@additionalnotes VARCHAR(MAX)='',
	@extraDescription VARCHAR(MAX)='', 
	@userId INT
AS
BEGIN

	DECLARE @oldProductDescription VARCHAR(MAX), 
		@oldAdditionalnotes VARCHAR(MAX), 
		@oldExtraDescription VARCHAR(MAX)

	SELECT @oldProductDescription = ISNULL(ProductDescription,''),
	       @oldAdditionalnotes = ISNULL(AdditionalInfo,''),
	       @oldExtraDescription = ISNULL(ExtraDescription,'')	
	FROM tbl_SalesEstimate_EstimateDetails
	WHERE id_SalesEstimate_EstimateDetails = @revisionDetailsId 
	
	IF ((RTRIM(@oldProductDescription) <> RTRIM(@productDescription)) OR
		(RTRIM(@oldAdditionalnotes) <> RTRIM(@additionalnotes)) OR
		(RTRIM(@oldExtraDescription) <> RTRIM(@extraDescription)))
	BEGIN
		UPDATE tbl_SalesEstimate_EstimateDetails
		SET ProductDescription = RTRIM(@productDescription),
		AdditionalInfo = RTRIM(@additionalnotes),
		ExtraDescription = RTRIM(@extraDescription),
		ModifiedOn = GETDATE(),
		ModifiedBy = @userId
		WHERE id_SalesEstimate_EstimateDetails = @revisionDetailsId
	END
END