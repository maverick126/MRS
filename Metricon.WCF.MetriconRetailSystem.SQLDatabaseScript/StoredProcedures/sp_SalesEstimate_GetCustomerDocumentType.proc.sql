----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetCustomerDocumentType]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetCustomerDocumentType]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetCustomerDocumentType]
	@estimateRevisionId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @revisionTypeId INT
	DECLARE @estimateId INT
	DECLARE @contractType VARCHAR(20)
	DECLARE @documentType VARCHAR(20)
	
	SET @documentType = NULL
	
	SELECT @revisionTypeId = fkid_SalesEstimate_RevisionType, @contractType = ContractType, @estimateId = fkidEstimate
	FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
	DECLARE @contractNumber INT
	SELECT @contractNumber = BCContractNumber FROM Estimate WHERE EstimateID = @estimateId
	
	DECLARE @estimates TABLE ( estimateId INT )
	INSERT INTO @estimates SELECT EstimateID FROM Estimate WHERE BCContractNumber = @contractNumber
	
	IF @revisionTypeId = 5 --CSC
	BEGIN	
		IF @contractType = 'STC'
		BEGIN
			--Check if a Contract has already been defined
			IF NOT EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument Doc INNER JOIN tbl_SalesEstimate_EstimateHeader Hdr 
			ON Doc.fkid_SalesEstimate_EstimateHeader = Hdr.id_SalesEstimate_EstimateHeader 
			WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND id_SalesEstimate_EstimateHeader < @estimateRevisionId AND DocumentType = 'Contract' AND Active = 1)
				SET @documentType = 'Contract'
		END
		ELSE IF @contractType = 'PC'
		BEGIN
			--Check if Studio M revision exists
			IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType = 23)
				SET @documentType = 'Contract'	
			ELSE
				SET @documentType = 'PC'
		END
	END
	ELSE IF (@revisionTypeId = 24 OR @revisionTypeId = 14 OR @revisionTypeId = 18) --PSTM-CSC OR PVAR-CSC OR BVAR-BSC
	BEGIN
		SET @documentType = 'Variation'
	END

	SELECT @documentType AS DocumentType
	
	SET NOCOUNT OFF;

END