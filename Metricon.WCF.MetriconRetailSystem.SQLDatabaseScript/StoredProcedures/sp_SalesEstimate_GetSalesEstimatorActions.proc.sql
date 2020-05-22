----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetSalesEstimatorActions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetSalesEstimatorActions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetSalesEstimatorActions] 
	@estimateRevisionId INT,
	@userid INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @validToChangeFacade BIT
	DECLARE @validToChangeContractType BIT
	DECLARE @validToChangePriceEffectiveDate BIT
	
	DECLARE @estimateId INT
	DECLARE @contractType VARCHAR(10)
	DECLARE @revisionTypeId INT
	DECLARE @ownerId INT
	
	SELECT @estimateId = fkidEstimate, 
	       @contractType = ContractType, 
	       @revisionTypeId = fkid_SalesEstimate_RevisionType ,
	       @ownerId=fkidOwner
	FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
 
	IF (@revisionTypeId = 4 OR @revisionTypeId = 2) --Sales Estimator OR Sales Technical Support
			BEGIN
				IF (@contractType = 'PC')
				BEGIN
					--If Ready for Studio M revision exists, do not allow any actions 
					IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader 
						WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType = 6)

					BEGIN
						SET @validToChangeContractType = 0
						SET @validToChangePriceEffectiveDate = 0
					END
					ELSE
					BEGIN
						SET @validToChangeContractType = 1
						SET @validToChangePriceEffectiveDate = 1
					END
				END
				ELSE IF (@contractType = 'STC')
				BEGIN
					--If HIA Contract has been defined, do not allow any actions 
					IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc
						ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader
						WHERE fkidEstimate = @estimateId AND Doc.DocumentType = 'Contract' AND Doc.Active = 1)

					BEGIN
						SET @validToChangeContractType = 0
						SET @validToChangePriceEffectiveDate = 0
					END
					ELSE
					BEGIN
						SET @validToChangeContractType = 1
						SET @validToChangePriceEffectiveDate = 1
					END	
				END

		--		IF @revisionTypeId = 4 AND EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader 
		--			WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType = 23)
		--		BEGIN --Do not allow change of facade in SE revision after Studio M
		--			SET @validToChangeFacade = 0
		--		END
		--		ELSE
		--		BEGIN
					SET @validToChangeFacade = 1
		--		END
			END
			ELSE IF (@revisionTypeId = 15 OR @revisionTypeId = 25) --Pre Site Variation SE or Pre Studio M Variation SE
			BEGIN
				SET @validToChangeFacade = 1
				SET @validToChangeContractType = 0
				
				DECLARE @previousrevisionid INT
				EXEC sp_SalesEstimate_GetPreviousRevisonForVariation @estimateRevisionId, @previousrevisionid OUTPUT
				
				DECLARE @previousestimateid INT
				SELECT @previousestimateid = fkidEstimate FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @previousrevisionid
				IF @estimateid <> @previousestimateid
				BEGIN
					SET @validToChangePriceEffectiveDate = 1
				END
				ELSE
				BEGIN
					SET @validToChangePriceEffectiveDate = 0
				END
			END
 
	IF(@userid<>@ownerId)
	   BEGIN
	      SET @validToChangeFacade=CAST(0 AS BIT)
	      SET @validToChangeContractType=CAST(0 AS BIT)
	   END
	   
	SELECT @validToChangeFacade ValidToChangeFacade, 
	@validToChangeContractType ValidToChangeContractType,
	@validToChangePriceEffectiveDate ValidToChangePriceEffectiveDate
	
	SET NOCOUNT OFF;
END
GO