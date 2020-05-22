----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateHeaderForLogging]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateHeaderForLogging]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateHeaderForLogging]
	@revisionId int
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @variationnumber VARCHAR(20)
       
       
     IF(EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader=@revisionId AND DocumentType = 'Variation' AND Active = 1))
        BEGIN
            SELECT @variationnumber=CAST(DocumentNumber AS VARCHAR) 
			 FROM tbl_SalesEstimate_CustomerDocument 
			 WHERE fkid_SalesEstimate_EstimateHeader=@revisionId AND DocumentType = 'Variation' AND Active = 1
        END
     ELSE
        BEGIN
			SET @variationnumber='--'
        END
      
	SELECT 
	EH.RevisionNumber, 
	EH.fkidEstimate AS EstimateNumber,
	R.RevisionTypeName AS RevisionType,
	O.new_contractid AS ContractId,
    EH.createdon,
    null AS expirydate,
    @variationnumber AS VariationNumber
	FROM tbl_SalesEstimate_EstimateHeader EH

	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN tbl_SalesEstimate_RevisionType R 
	ON EH.fkid_SalesEstimate_RevisionType = R.id_SalesEstimate_RevisionType
	
	LEFT JOIN syn_CRM_Opportunity O ON E.fkidOpportunity = O.OpportunityId

	WHERE id_SalesEstimate_EstimateHeader = @revisionId

	SET NOCOUNT OFF;
END
GO