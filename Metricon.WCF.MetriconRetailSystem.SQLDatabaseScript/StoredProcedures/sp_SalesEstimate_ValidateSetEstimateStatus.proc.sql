----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_ValidateSetEstimateStatus]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_ValidateSetEstimateStatus]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_ValidateSetEstimateStatus]
	@estimateRevisionId INT, @nextRevisionTypeId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @revisionTypeId INT
	DECLARE @estimateId INT
	DECLARE @contractType VARCHAR(20)
	DECLARE @errorMessage VARCHAR(500)
	
	SET @errorMessage = NULL
	
	SELECT @revisionTypeId = fkid_SalesEstimate_RevisionType, @contractType = ContractType, @estimateId = fkidEstimate 
	FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
	DECLARE @estimates TABLE (estimateId INT)
	
	INSERT INTO @estimates SELECT DISTINCT fkidEstimate FROM tbl_SalesEstimate_EstimateHeader SED INNER JOIN Estimate E
    ON SED.fkidEstimate = E.EstimateID WHERE BCContractNumber = (SELECT BCContractNumber FROM Estimate WHERE EstimateID = @estimateId)
	
	IF @nextRevisionTypeId = 0 --N/A
	BEGIN
		IF @revisionTypeId = 5 --CSC
		BEGIN
			
			IF @contractType = 'STC'
			BEGIN
				--Check if this revision is defined as a Contract
				IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Contract' AND Active = 1)
				BEGIN
					--Check if both SentDate and AcceptedDate are filled
					IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument 
					WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Contract' AND Active = 1 AND
					(SentDate IS NULL OR AcceptedDate IS NULL))
						SET @errorMessage = 'Please enter both Contract Sent Date and Accepted Date before accepting this revision'
				END
				--Only validate if Studio M revision doesn't exist 
				ELSE IF NOT EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 23)
					SET @errorMessage = 'Please define this revision as a Contract before accepting this revision'
			END
			ELSE IF @contractType = 'PC'
			BEGIN
				--Check if Studio M revision exists
				IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 23)
				BEGIN
					--Check if this revision is defined as a Contract
					IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Contract' AND Active = 1)
					BEGIN
						--Check if both SentDate and AcceptedDate are filled
						IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument 
						WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Contract' AND Active = 1 AND
						(SentDate IS NULL OR AcceptedDate IS NULL))
							SET @errorMessage = 'Please enter both Contract Sent Date and Accepted Date before accepting this revision'
					END
					ELSE
						SET @errorMessage = 'Please define this revision as a Contract before accepting this revision'					
				END
				ELSE --No Studio M revision
				BEGIN
					--Check if this revision is defined as a PC
					IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'PC' AND Active = 1)
					BEGIN
						--Check if both SentDate and AcceptedDate are filled
						IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument 
						WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'PC' AND Active = 1 AND SentDate IS NULL)
							SET @errorMessage = 'Please enter PC Sent Date before accepting this revision'
					END
				END
			END
		END
		ELSE IF (@revisionTypeId = 24 OR @revisionTypeId = 14 OR @revisionTypeId = 18) --PSTM-CSC OR PVAR-CSC OR BVAR-BSC
		BEGIN
				--Check if this revision is defined as a Variation
				IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Variation' AND Active = 1)
				BEGIN
					--Check if both SentDate and AcceptedDate are filled
					IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument 
					WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Variation' AND Active = 1 AND
					(SentDate IS NULL OR AcceptedDate IS NULL))
						SET @errorMessage = 'Please enter both Variation Sent Date and Accepted Date before accepting this revision'
				END
				ELSE
					SET @errorMessage = 'Please define this revision as a Variation before accepting this revision'		
		END
	END
	ELSE -- Move to the next revision that is not N/A
	BEGIN
		IF @revisionTypeId = 5 --CSC
		BEGIN
			--Check if this revision is defined as a Contract
			IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Contract' AND Active = 1)
				SET @errorMessage = 'This revision is set as Contract. Please remove Contract checkbox before accepting this revision'
			ELSE IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'PC' AND Active = 1)
				SET @errorMessage = 'This revision is set as PC. Please remove PC checkbox before accepting this revision'
		END
		ELSE IF (@revisionTypeId = 24 OR @revisionTypeId = 14 OR @revisionTypeId = 18) --PSTM-CSC OR PVAR-CSC OR BVAR-BSC
		BEGIN
			--Check if this revision is defined as a Variation
			IF EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND DocumentType = 'Variation' AND Active = 1)
				SET @errorMessage = 'This revision is set as Variation. Please remove Variation checkbox before accepting this revision'		
		END	
	END
	
	SELECT @errorMessage AS ErrorMessage
	
	SET NOCOUNT OFF;

END
GO