----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetCustomerDocumentDetails]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetCustomerDocumentDetails]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetCustomerDocumentDetails]
	@estimateRevisionId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	
	DECLARE @documentType VARCHAR(20)
	
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId)
	BEGIN
		SELECT TOP 1 Active, SentDate, AcceptedDate, DocumentNumber, DocumentType, ExtensionDays, id_SalesEstimate_CustomerDocument AS CustomerDocumentId, [Description] as summary
		FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId
		ORDER BY id_SalesEstimate_CustomerDocument
	END
	ELSE
	BEGIN
		DECLARE @tempDocType TABLE (DocumentType VARCHAR(50))		
		INSERT INTO @tempDocType EXEC sp_SalesEstimate_GetCustomerDocumentType @estimateRevisionId
		
		SELECT TOP 1 @documentType = DocumentType FROM @tempDocType
		
		DECLARE @documentNumber INT
		DECLARE @estimateId INT
		DECLARE @contractNumber INT
		DECLARE @summary VARCHAR(40)
		
		SELECT @estimateId = fkidEstimate FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
		
		SELECT @contractNumber = BCContractNumber FROM Estimate WHERE EstimateID = @estimateId
		
		DECLARE @estimates TABLE ( estimateId INT )
		INSERT INTO @estimates SELECT EstimateID FROM Estimate WHERE BCContractNumber = @contractNumber
		
		IF (@documentType = 'Variation' OR @documentType = 'PC')
		BEGIN
			SELECT @documentNumber = MAX(Doc.DocumentNumber) 
			FROM tbl_SalesEstimate_CustomerDocument Doc INNER JOIN tbl_SalesEstimate_EstimateHeader Hdr
			ON Doc.fkid_SalesEstimate_EstimateHeader = Hdr.id_SalesEstimate_EstimateHeader
			WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND Doc.Active = 1 AND Doc.DocumentType = @documentType
			
			IF (@documentNumber IS NOT NULL)
				SET @documentNumber = @documentNumber + 1
			ELSE
				SET @documentNumber = 1
		END
		ELSE
			SET @documentNumber = 0
		
		
		SELECT 0 AS Active, NULL AS SentDate, NULL AS AcceptedDate, @documentNumber AS DocumentNumber, @documentType AS DocumentType, 0 AS ExtensionDays, '' AS summary,
		0 AS CustomerDocumentId 
	END
	SET NOCOUNT OFF;
END
