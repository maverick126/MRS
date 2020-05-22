----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateCustomerDocumentDetails]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateCustomerDocumentDetails]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateCustomerDocumentDetails]
	@estimateRevisionId INT, 
	@active BIT,
	@sentDate DATE = NULL,
	@acceptedDate DATE = NULL,
	@documentNumber INT = NULL,
	@documentType VARCHAR(20),
	@extensionDays INT = NULL,
	@customerDocumentId INT,
	@userId INT ,
	@summary VARCHAR(40)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @customerDocumentId = 0
	BEGIN
		INSERT INTO tbl_SalesEstimate_CustomerDocument (fkid_SalesEstimate_EstimateHeader, SentDate, AcceptedDate, DocumentType,
			DocumentNumber, ExtensionDays, Active, CreatedDate, CreatedBy, [Description]) 
			VALUES (@estimateRevisionId, @sentDate, @acceptedDate, @documentType, @documentNumber, @extensionDays, @active, GETDATE(), @userId, @summary) 
 
		SET @customerDocumentId = SCOPE_IDENTITY()		
		
		IF (@sentDate IS NOT NULL)
		BEGIN
			IF (@documentType = 'PC')
			BEGIN
				EXEC sp_SalesEstimate_CreateEstimateEventRegister 'pcsent', @estimateRevisionId, @userId
			END
			ELSE IF (@documentType = 'Contract')
			BEGIN
				EXEC sp_SalesEstimate_CreateEstimateEventRegister 'contractsent', @estimateRevisionId, @userId
			END	
		END		
	END
	ELSE
	BEGIN
		IF (EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE id_SalesEstimate_CustomerDocument = @customerDocumentId AND SentDate IS NULL) AND @sentDate IS NOT NULL)
		BEGIN
			IF (@documentType = 'PC')
			BEGIN
				EXEC sp_SalesEstimate_CreateEstimateEventRegister 'pcsent', @estimateRevisionId, @userId
			END
			ELSE IF (@documentType = 'Contract')
			BEGIN
				EXEC sp_SalesEstimate_CreateEstimateEventRegister 'contractsent', @estimateRevisionId, @userId
			END		
		END
		
		UPDATE tbl_SalesEstimate_CustomerDocument SET 
		SentDate = @sentDate, 
		AcceptedDate = @acceptedDate, 
		DocumentType = @documentType,
		DocumentNumber = @documentNumber,
		ExtensionDays = @extensionDays,
		Active = @active,
		ModifiedBy = @userId,
		ModifiedDate = GETDATE(),
		[Description]=@summary
		WHERE id_SalesEstimate_CustomerDocument = @customerDocumentId
	END
	
	SELECT @customerDocumentId AS CustomerDocumentId
	
	SET NOCOUNT OFF;
END

