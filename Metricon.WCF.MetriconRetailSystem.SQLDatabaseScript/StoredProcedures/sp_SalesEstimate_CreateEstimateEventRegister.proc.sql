IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CreateEstimateEventRegister]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CreateEstimateEventRegister]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_CreateEstimateEventRegister]
	@action VARCHAR(50), @revisionId INT, @userId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @actionId INT
	DECLARE @actionTable TABLE (actionId INT)

	DECLARE @revisionTypeId INT, @contractType VARCHAR(10), @estimateId INT, @contractNumber INT
	
	SELECT @revisionTypeId = fkid_SalesEstimate_RevisionType, @contractType = ContractType, 
		@estimateId = fkidEstimate, @contractNumber = BCContractNumber 
	FROM tbl_SalesEstimate_EstimateHeader INNER JOIN Estimate ON fkidEstimate = EstimateID 
	WHERE id_SalesEstimate_EstimateHeader = @revisionId
	
	IF @action = 'accept'
	BEGIN
		DECLARE @documentType VARCHAR(10)
		IF @revisionTypeId = 4 --Sales Estimating
		BEGIN
			INSERT INTO @actionTable VALUES (1) --Accept
			IF (@contractType = 'STC')
			BEGIN
				INSERT INTO @actionTable VALUES (3) --Accept SE STC
			END
			ELSE IF (@contractType = 'PC')
			BEGIN
				--If StudioM revision exists
				IF EXISTS (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 23 
					AND fkidEstimate IN (SELECT EstimateID FROM Estimate WHERE BCContractNumber = @contractNumber)
					AND id_SalesEstimate_EstimateHeader < @revisionId)
				BEGIN
					INSERT INTO @actionTable VALUES (3) --Accept SE STC
				END
				ELSE
				BEGIN
					INSERT INTO @actionTable VALUES (2) --Accept SE PC
				END
			END		
		END
		ELSE IF @revisionTypeId = 5 --Customer Support Coordinator
		BEGIN
			SELECT @documentType = DocumentType FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND Active = 1
			IF @documentType = 'PC'
			BEGIN
				INSERT INTO @actionTable VALUES (7) --Accept CSC PC
			END
			ELSE IF @documentType = 'Contract'
			BEGIN
				INSERT INTO @actionTable VALUES (6) --Accept CSC Contract
			END
			ELSE
			BEGIN
				RETURN
			END
		END
		ELSE IF @revisionTypeId = 3 --Drafting
		BEGIN
			INSERT INTO @actionTable VALUES (1) --Accept
			IF (@contractType = 'STC')
			BEGIN
				INSERT INTO @actionTable VALUES (13) --Accept DF STC
			END
			ELSE IF (@contractType = 'PC')
			BEGIN
				--If StudioM revision exists
				IF EXISTS (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 23 
					AND fkidEstimate IN (SELECT EstimateID FROM Estimate WHERE BCContractNumber = @contractNumber)
					AND id_SalesEstimate_EstimateHeader < @revisionId)
				BEGIN
					INSERT INTO @actionTable VALUES (13) --Accept DF STC
				END
				ELSE
				BEGIN
					INSERT INTO @actionTable VALUES (12) --Accept DF PC
				END
			END		
		END				
		ELSE IF (@revisionTypeId = 14 OR @revisionTypeId = 18 OR @revisionTypeId = 24) --PVAR-CSC, BVAR-BSC, PSTM-CSC respectively
		BEGIN
			SELECT @documentType = DocumentType FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND Active = 1
			IF @documentType = 'Variation'
			BEGIN
				INSERT INTO @actionTable VALUES (8) --Accept CSC Variation
			END
			ELSE
			BEGIN
				RETURN
			END
		END
		ELSE
		BEGIN
			INSERT INTO @actionTable VALUES (1) --Accept
		END
	END	
	ELSE IF @action = 'changefacade'
	BEGIN
		INSERT INTO @actionTable VALUES (4) --Change of Facade
	END
	ELSE IF @action = 'pcsent'
	BEGIN
		INSERT INTO @actionTable VALUES (15) --PC Sent
	END
	ELSE IF @action = 'contractsent'
	BEGIN
		INSERT INTO @actionTable VALUES (14) --Contract Sent
	END			
	ELSE
	BEGIN
		RETURN
	END
	DECLARE aCursor CURSOR FOR
	SELECT actionId FROM @actionTable
	
	OPEN aCursor
	FETCH NEXT FROM aCursor INTO @actionId
	
	WHILE @@FETCH_STATUS = 0
	BEGIN
	
		DECLARE @Evt INT
		SELECT @Evt = id_SalesEstimate_EstimateEvent FROM tbl_SalesEstimate_EstimateEvent EE WHERE EE.fkid_SalesEstimate_EstimateAction = @actionId
		AND EE.Active = 1 AND (EE.fkid_SalesEstimate_RevisionType = 
		(SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId) 
		OR EE.fkid_SalesEstimate_RevisionType = 0)

		--IF Action exists for this Revision Type
		IF @Evt IS NOT NULL 
		BEGIN
			--If this Action is allowed to have multiple rows from the same contract (do not allow mutiple rows from the same revision)
			IF (EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateAction WHERE id_SalesEstimate_EstimateAction = @actionId AND AllowMultipleRows = 1 AND Active = 1) 
			AND 
			NOT EXISTS (SELECT * FROM tbl_SalesEstimate_EventRegister
			WHERE fkid_SalesEstimate_EstimateEvent = @Evt AND fkid_SalesEstimate_EstimateHeader = @revisionId))
			
			OR 
			--If this Action does not exist for this contract number
			NOT EXISTS (SELECT * FROM tbl_SalesEstimate_EventRegister ER 
			INNER JOIN tbl_SalesEstimate_EstimateHeader EH 
			ON ER.fkid_SalesEstimate_EstimateHeader = EH.id_SalesEstimate_EstimateHeader
			WHERE ER.fkid_SalesEstimate_EstimateEvent = @Evt AND EH.fkidEstimate IN 
			(SELECT EstimateID FROM Estimate WHERE BCContractNumber = @contractNumber))
			BEGIN
				INSERT INTO tbl_SalesEstimate_EventRegister (fkid_SalesEstimate_EstimateEvent, fkid_SalesEstimate_EstimateHeader, CreatedBy)
				VALUES (@Evt, @revisionId, @userId)
			END
		END
		
		FETCH NEXT FROM aCursor INTO @actionId
	END
	
	CLOSE aCursor
	DEALLOCATE aCursor
END