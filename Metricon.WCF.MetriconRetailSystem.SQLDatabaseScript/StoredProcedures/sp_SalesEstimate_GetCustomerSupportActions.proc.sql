----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetCustomerSupportActions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetCustomerSupportActions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetCustomerSupportActions] 
	@estimateRevisionId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @validToCreatePreStudioMVariation BIT
	DECLARE @validToCreateContractDraft BIT	
	DECLARE @validToCreateFinalContract BIT	
	DECLARE @validToCreateCustomerSupport BIT
	DECLARE @validToCreatePreSiteVariation BIT
	DECLARE @validToCreateBuildingVariation BIT	
	
	DECLARE @estimateId INT, @maxrevisionid INT
	DECLARE @contractType VARCHAR(10)
	
	SELECT @estimateId = fkidEstimate, @contractType = ContractType FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId

	DECLARE @contractNumber INT
	SELECT @contractNumber = BCContractNumber FROM Estimate WHERE EstimateID = @estimateId
	
	DECLARE @estimates TABLE ( estimateId INT )
	INSERT INTO @estimates SELECT EstimateID FROM Estimate WHERE BCContractNumber = @contractNumber
	
	--Get the latest revision
	SELECT @maxrevisionid=MAX(id_SalesEstimate_EstimateHeader) 
	FROM tbl_SalesEstimate_EstimateHeader 
	WHERE fkidEstimate IN (SELECT estimateId FROM @estimates)

	-- If the latest revision was rejected
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @maxrevisionid AND fkid_SalesEstimate_Status = 3)
	BEGIN
		DECLARE @previousrevisionid INT
		EXEC sp_SalesEstimate_GetPreviousRevisonForVariation @maxrevisionid, @previousrevisionid OUTPUT
		
		SET @maxrevisionid = @previousrevisionid
	END

	--If there is any In Progress or Queue revisions, do not allow any actions	   
	IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_Status = 1 AND fkidEstimate IN (SELECT estimateId FROM @estimates)
				UNION SELECT id_SalesEstimate_Queue FROM tbl_SalesEstimate_Queue WHERE fkidEstimate IN (SELECT estimateId FROM @estimates)) 
	BEGIN
		SET @validToCreateContractDraft = 0
		SET @validToCreateFinalContract = 0
		SET @validToCreatePreStudioMVariation = 0
		SET @validToCreateCustomerSupport = 0
		SET @validToCreatePreSiteVariation = 0
		SET @validToCreateBuildingVariation = 0
	END
	ELSE
	BEGIN
		--If Ready for Studio M revision already exists, do not allow it to be created again
		IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType IN (6, 23) AND fkidEstimate IN (SELECT estimateId FROM @estimates))
		OR @maxrevisionid <> @estimateRevisionId
		BEGIN
		SET @validToCreateContractDraft = 0
		END
		ELSE
		BEGIN
			--If a Pre Studio M revision is In Progress, do not allow Ready for Studio M
			IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType IN (24, 25) --Pre Studio M Variation
			AND fkid_SalesEstimate_Status = 1 AND fkidEstimate IN (SELECT estimateId FROM @estimates)
			UNION SELECT id_SalesEstimate_Queue FROM tbl_SalesEstimate_Queue WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 25)
			BEGIN
				SET @validToCreateContractDraft = 0
			END
			ELSE
			BEGIN
				SET @validToCreateContractDraft = 1
			END
		END		

		--Final Contract is no longer required
		SET @validToCreateFinalContract = 0


		--If Ready for Studio M exists, do not allow Pre Studio M Variation
		IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType IN (6,23) AND fkidEstimate IN (SELECT estimateId FROM @estimates))
		OR @maxrevisionid <> @estimateRevisionId
		BEGIN
			SET @validToCreatePreStudioMVariation = 0
		END
		ELSE
		BEGIN
			--Only allow Pre Studio M Variation in STC process
			IF @contractType = 'STC'
				SET @validToCreatePreStudioMVariation = 1
			ELSE
				SET @validToCreatePreStudioMVariation = 0
		END	


		--If Ready for Studio M exists, do not allow CSC
		IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType IN (6,23) AND fkidEstimate IN (SELECT estimateId FROM @estimates))
		OR @maxrevisionid <> @estimateRevisionId
		BEGIN
			SET @validToCreateCustomerSupport = 0
		END
		ELSE
		BEGIN
			IF @contractType = 'PC'
				SET @validToCreateCustomerSupport = 1
			ELSE
				SET @validToCreateCustomerSupport = 0
		END	
		
		--If Studio M exists
		IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 23 AND fkidEstimate IN (SELECT estimateId FROM @estimates))
		BEGIN
			IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
				AND fkid_SalesEstimate_RevisionType IN (14, 15, 16, 17)
				AND fkid_SalesEstimate_Status = 1) --In Progress
				OR @maxrevisionid <> @estimateRevisionId
			BEGIN
				SET @validToCreatePreSiteVariation = 0 
			END
			ELSE
			BEGIN
				IF EXISTS (SELECT id_SalesEstimate_Queue FROM tbl_SalesEstimate_Queue WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
					AND fkid_SalesEstimate_RevisionType IN (14, 15, 16, 17)) 
				BEGIN
					SET @validToCreatePreSiteVariation = 0
				END
				ELSE
				BEGIN
					SET @validToCreatePreSiteVariation = 1
				END
			END
			
			IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
				AND fkid_SalesEstimate_RevisionType IN (18, 19, 20)
				AND fkid_SalesEstimate_Status = 1) --In Progress
				OR @maxrevisionid <> @estimateRevisionId
			BEGIN
				SET @validToCreateBuildingVariation = 0
			END
			ELSE
			BEGIN
				IF EXISTS (SELECT id_SalesEstimate_Queue FROM tbl_SalesEstimate_Queue WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
					AND fkid_SalesEstimate_RevisionType IN (18, 19, 20)) 
				BEGIN
					SET @validToCreateBuildingVariation = 0
				END
				ELSE
				BEGIN
					SET @validToCreateBuildingVariation = 1
				END
			END
			
		END
		ELSE
		BEGIN
			SET @validToCreatePreSiteVariation = 0
			SET @validToCreateBuildingVariation = 0
		END	

	END	
		
	SELECT @validToCreateFinalContract ValidToCreateFinalContract, 
	@validToCreateContractDraft ValidToCreateContractDraft,
	@validToCreatePreStudioMVariation ValidToCreatePreStudioMVariation,
	@validToCreateCustomerSupport ValidToCreateCustomerSupport,
	@validToCreatePreSiteVariation ValidToCreatePreSiteVariation,
	@validToCreateBuildingVariation ValidToCreateBuildingVariation
	
	SET NOCOUNT OFF;
END
GO