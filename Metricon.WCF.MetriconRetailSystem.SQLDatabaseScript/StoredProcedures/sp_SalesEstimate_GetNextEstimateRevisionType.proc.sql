
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetNextEstimateRevisionType]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetNextEstimateRevisionType]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetNextEstimateRevisionType]
	@revisionId INT, 
	@statusId INT -- Status of the Sales Estimate i.e. Accepted = 2, Rejected = 3	
AS
BEGIN

	SET NOCOUNT ON;
	
	DECLARE @estimateId INT
	DECLARE @currentRevisionTypeId INT	
	DECLARE @ownerOfRevisionTypeId INT
	DECLARE @notes NVARCHAR(255)
	DECLARE @contractType NVARCHAR(10)
	DECLARE @contractNo INT

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT	
	
	DECLARE @brandId INT
	DECLARE @MRSGroupId INT
	
	DECLARE @router TABLE (
	id_SalesEstimate_RevisionTypeRouter int,
	fkid_SalesEstimate_RevisionType int,
	RejectedRevisionTypeId int,
	AcceptedRevisionTypeId int,
	FromRevisionTypeId int,
	CheckNSR bit,
	NSROnly bit,
	RejectedToOwnerOfRevisionTypeId int,
	AcceptedToOwnerOfRevisionTypeId int,
	CreateAcceptedRevisionAsAccepted bit,
	CheckColour bit,
	ColourOnly bit,
	MRSGroupId int,
	BrandId int,
	ContractType nvarchar(10),
	RevisionTypeExists int,
	Notes nvarchar(255),
	Active bit)
	
	DECLARE @result TABLE (
	NextRevisionTypeId INT,
	NewRevisionTypeName VARCHAR(255),
	OwnerOfRevisionTypeId INT, --Owner of what REVISION TYPE will be the owner of the next revision
	NextRevisionOwnerId INT, --the OWNER itself
	Notes VARCHAR(255))
	
	DECLARE @estimates TABLE (estimateId INT)
	
	-- Get Estimate ID from Estimate Header
	SELECT 
	@estimateId = fkidEstimate,
	@currentRevisionTypeId = fkid_SalesEstimate_RevisionType,
	@contractType = ContractType
	FROM tbl_SalesEstimate_EstimateHeader
	WHERE id_SalesEstimate_EstimateHeader = @revisionId
	
	SELECT @brandId = Home.BrandID, 
	@MRSGroupId = Region.MRSGroupID,
	@contractNo = Estimate.BCContractNumber
	FROM Estimate 
	INNER JOIN Home ON Estimate.HomeID = Home.HomeID
	INNER JOIN Region ON Estimate.RegionID = Region.RegionID 
	WHERE EstimateID = @estimateId
	
	INSERT INTO @estimates SELECT DISTINCT fkidEstimate FROM tbl_SalesEstimate_EstimateHeader SED INNER JOIN Estimate E
    ON SED.fkidEstimate = E.EstimateID WHERE BCContractNumber = @contractNo
	
	-- Work out the next Revision Type
	-- Accept or Reject
	IF @statusId = 2 OR @statusId = 3
	BEGIN
		-- Route by Brand and MRS Group 
		IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionTypeRouter
		WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId 
		AND Active = 1 AND BrandId = @brandId AND MRSGroupId = @MRSGroupId)
		BEGIN
			INSERT INTO @router 
			SELECT id_SalesEstimate_RevisionTypeRouter,
			fkid_SalesEstimate_RevisionType,
			RejectedRevisionTypeId,
			AcceptedRevisionTypeId,
			FromRevisionTypeId,
			CheckNSR,
			NSROnly,
			RejectedToOwnerOfRevisionTypeId,
			AcceptedToOwnerOfRevisionTypeId,
			CreateAcceptedRevisionAsAccepted,
			CheckColour,
			ColourOnly,
			MRSGroupId,
			BrandId,
			ContractType,
			RevisionTypeExists,
			CASE WHEN @statusId = 2 THEN Notes ELSE RejectionNotes END,
			Active 
			FROM tbl_SalesEstimate_RevisionTypeRouter
			WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId 
			AND Active = 1 AND BrandId = @brandId AND MRSGroupId = @MRSGroupId
		END
		-- Route by MRS Group only
		ELSE IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionTypeRouter
		WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId 
		AND Active = 1 AND MRSGroupId = @MRSGroupId AND BrandId = 0)
		BEGIN
			INSERT INTO @router 
			SELECT id_SalesEstimate_RevisionTypeRouter,
			fkid_SalesEstimate_RevisionType,
			RejectedRevisionTypeId,
			AcceptedRevisionTypeId,
			FromRevisionTypeId,
			CheckNSR,
			NSROnly,
			RejectedToOwnerOfRevisionTypeId,
			AcceptedToOwnerOfRevisionTypeId,
			CreateAcceptedRevisionAsAccepted,
			CheckColour,
			ColourOnly,
			MRSGroupId,
			BrandId,
			ContractType,
			RevisionTypeExists,			
			CASE WHEN @statusId = 2 THEN Notes ELSE RejectionNotes END,
			Active 
			FROM tbl_SalesEstimate_RevisionTypeRouter
			WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId 
			AND Active = 1 AND MRSGroupId = @MRSGroupId AND BrandId = 0
		END
		-- Default Routing
		ELSE
		BEGIN
			INSERT INTO @router 
			SELECT id_SalesEstimate_RevisionTypeRouter,
			fkid_SalesEstimate_RevisionType,
			RejectedRevisionTypeId,
			AcceptedRevisionTypeId,
			FromRevisionTypeId,
			CheckNSR,
			NSROnly,
			RejectedToOwnerOfRevisionTypeId,
			AcceptedToOwnerOfRevisionTypeId,
			CreateAcceptedRevisionAsAccepted,
			CheckColour,
			ColourOnly,
			MRSGroupId,
			BrandId,
			ContractType,
			RevisionTypeExists,			
			CASE WHEN @statusId = 2 THEN Notes ELSE RejectionNotes END,
			Active 
			FROM tbl_SalesEstimate_RevisionTypeRouter
			WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId 
			AND Active = 1 AND MRSGroupId = 0 AND BrandId = 0
		END

		--Remove Contract Type that is not applicable to this estimate 
		DELETE FROM @router WHERE ContractType <> 'ALL' AND ContractType <> @contractType

		--If RevisionTypeExists is specified
		IF EXISTS (SELECT * FROM @router WHERE RevisionTypeExists > 0)
		BEGIN
			--If revision exists, choose this path 
			IF EXISTS (SELECT Rtr.RevisionTypeExists FROM @router Rtr INNER JOIN tbl_SalesEstimate_EstimateHeader Hdr 
				ON Rtr.RevisionTypeExists = Hdr.fkid_SalesEstimate_RevisionType
				WHERE Hdr.fkidEstimate IN (SELECT estimateId FROM @estimates) AND Rtr.Active = 1)
			BEGIN	
				DELETE FROM @router WHERE RevisionTypeExists NOT IN 
				(SELECT Rtr.RevisionTypeExists FROM @router Rtr INNER JOIN tbl_SalesEstimate_EstimateHeader Hdr 
				ON Rtr.RevisionTypeExists = Hdr.fkid_SalesEstimate_RevisionType
				WHERE Hdr.fkidEstimate IN (SELECT estimateId FROM @estimates) AND Rtr.Active = 1)
			END
			ELSE --Revision does not exist, remove the routing path
				DELETE FROM @router WHERE RevisionTypeExists > 0
		END 

		DECLARE @CheckPreviousRevisionTypeCounter INT
		DECLARE @CheckNonStandardRequestCounter INT
		DECLARE @CheckColourCounter INT
		
		SELECT @CheckPreviousRevisionTypeCounter = COUNT(id_SalesEstimate_RevisionTypeRouter)
		FROM @router
		WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId AND FromRevisionTypeId > 0
		
		SELECT @CheckNonStandardRequestCounter = COUNT(id_SalesEstimate_RevisionTypeRouter)
		FROM @router
		WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId AND CheckNSR = 1	
		
		SELECT @CheckColourCounter = COUNT(id_SalesEstimate_RevisionTypeRouter)
		FROM @router
		WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId AND CheckColour = 1	

		DECLARE @previousRevisionTypeId INT
		DECLARE @previousRevisionId INT
			
		SELECT TOP 1 @previousRevisionTypeId = fkid_SalesEstimate_RevisionType, @previousRevisionId = id_SalesEstimate_EstimateHeader
		FROM tbl_SalesEstimate_EstimateHeader
		WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND id_SalesEstimate_EstimateHeader < @revisionId AND fkid_SalesEstimate_Status = 2
		ORDER BY id_SalesEstimate_EstimateHeader DESC
		
		IF (@CheckPreviousRevisionTypeCounter > 0)
		BEGIN
					
			DELETE FROM @router WHERE FromRevisionTypeId <> @previousRevisionTypeId AND FromRevisionTypeId > 0
			
		END
				

		IF (@CheckNonStandardRequestCounter > 0)
		BEGIN
			DECLARE @compareNSRWithRevisionTypeId INT
				
			IF @currentRevisionTypeId = 5 --CSC
			BEGIN
				--IF @previousRevisionTypeId = 23 --Studio M
				--BEGIN
				--	SET @compareNSRWithRevisionTypeId = 23 --Studio M
				--END
				--ELSE IF @previousRevisionTypeId = 5 --CSC
				--BEGIN
				--	SET @compareNSRWithRevisionTypeId = 5 --CSC
				--END
				--ELSE
				--BEGIN
				--	SET @compareNSRWithRevisionTypeId = 4 --Sales Estimating
				--END
				SET @compareNSRWithRevisionTypeId = @previousRevisionTypeId
			END
			ELSE IF @currentRevisionTypeId = 14 --Pre Site Variation CSC
			BEGIN		

				SET @compareNSRWithRevisionTypeId = (SELECT TOP 1 fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader < @revisionId AND fkid_SalesEstimate_Status = 2 AND fkidEstimate = @estimateId ORDER BY id_SalesEstimate_EstimateHeader DESC)
				
			END
			ELSE IF @currentRevisionTypeId = 18 --Building Variation BSC
			BEGIN
					
				SET @compareNSRWithRevisionTypeId = (SELECT TOP 1 fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader < @revisionId AND fkid_SalesEstimate_Status = 2 AND fkidEstimate = @estimateId ORDER BY id_SalesEstimate_EstimateHeader DESC)
			
			END
			ELSE IF @currentRevisionTypeId = 24 --Pre Sudio M Variation CSC
			BEGIN
			
				SET @compareNSRWithRevisionTypeId = (SELECT TOP 1 fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader < @revisionId AND fkid_SalesEstimate_Status = 2 AND fkidEstimate = @estimateId ORDER BY id_SalesEstimate_EstimateHeader DESC)
			
			END
			ELSE IF @currentRevisionTypeId = 23 --Studio M
			BEGIN
			
				IF EXISTS (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 6 AND fkidEstimate IN (SELECT estimateId FROM @estimates))
				BEGIN
					SET @compareNSRWithRevisionTypeId = 6 --Contract Draft
				END
				ELSE
				BEGIN 
					SET @compareNSRWithRevisionTypeId = 5 --CSC 
				END
			
			END
			ELSE
			BEGIN
				SET @ErrMsg = 'There is no Estimate Revision to compare Non Standard Request for Estimate ' + CONVERT(NVARCHAR(50), @estimateId) + '.'
				SET @ErrSeverity = 16
				RAISERROR(@ErrMsg, @ErrSeverity, 1)
			END
			
			DECLARE @compareNSRWithRevisionId INT
			SELECT TOP 1 @compareNSRWithRevisionId = id_SalesEstimate_EstimateHeader
			FROM tbl_SalesEstimate_EstimateHeader
			WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType = @compareNSRWithRevisionTypeId 
			AND fkid_SalesEstimate_Status = 2 --Accepted
			ORDER BY id_SalesEstimate_EstimateHeader DESC
			
				
			DECLARE @nonStandardRequestCount INT

            SELECT * INTO #tempprevisousrevision
            FROM tbl_SalesEstimate_EstimateDetails
            WHERE fkid_SalesEstimate_EstimateHeader = @compareNSRWithRevisionId	
			
			--Compare NSR to the previous revision to check whether new NSRs have been added  
			--SELECT @nonStandardRequestCount = COUNT(SED.id_SalesEstimate_EstimateDetails)
			--FROM tbl_SalesEstimate_EstimateDetails SED 
			--INNER JOIN EstimateDetails ED 
			--ON SED.fkidEstimateDetails = ED.EstimateDetailsID
			--WHERE SED.fkid_SalesEstimate_EstimateHeader = @revisionId 
			--AND ED.EstimateID = @estimateId  
			--AND ED.areaid = 43 AND NOT EXISTS 
			--(SELECT * FROM tbl_SalesEstimate_EstimateDetails 
			--WHERE fkid_SalesEstimate_EstimateHeader = @compareNSRWithRevisionId
			--AND fkidEstimateDetails = SED.fkidEstimateDetails)
			SELECT @nonStandardRequestCount = COUNT(SED.id_SalesEstimate_EstimateDetails)
			FROM tbl_SalesEstimate_EstimateDetails SED 
			INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = ED.EstimateDetailsID
			LEFT JOIN #tempprevisousrevision t ON SED.fkidEstimateDetails=t.fkidEstimateDetails
			WHERE SED.fkid_SalesEstimate_EstimateHeader = @revisionId 
			AND ED.EstimateID = @estimateId  
			AND ED.areaid = 43 AND t.fkidEstimateDetails IS NULL
			
			IF (@nonStandardRequestCount = 0)
			BEGIN
			
				--Check Removed Items table for NSRs that've been deleted and added back again (for different purposes)
				SELECT @nonStandardRequestCount = COUNT(SED.id_SalesEstimate_EstimateDetails)
				FROM tbl_SalesEstimate_EstimateDetails SED 
				INNER JOIN EstimateDetails ED 
				ON SED.fkidEstimateDetails = ED.EstimateDetailsID
				WHERE SED.fkid_SalesEstimate_EstimateHeader = @revisionId 
				AND ED.EstimateID = @estimateId  
				AND ED.areaid = 43 AND SED.fkidEstimateDetails IN 
				(SELECT fkidEstimateDetails FROM tbl_SalesEstimate_RemovedItems 
				WHERE fkidRevision = @revisionId 
				AND fkidEstimateDetails IS NOT NULL)
				
			END
			
			
			IF @nonStandardRequestCount > 0
			BEGIN
				DELETE FROM @router WHERE CheckNSR = 1 AND NSROnly = 0
			END
			ELSE
			BEGIN
				DELETE FROM @router WHERE CheckNSR = 1 AND NSROnly = 1
			END
		END
		
		IF (@CheckColourCounter > 0)
		BEGIN
			/*
			--Check what revision to compare whether new Studio M products have been added 
			DECLARE @compareColourWithRevisionId INT
			
			--IF BVAR exists 	
			IF EXISTS (SELECT id_SalesEstimate_EstimateHeader 
				FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc 
				ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader
				WHERE fkidEstimate = @estimateId 
				AND Hdr.fkid_SalesEstimate_RevisionType = 18 --BVAR-BSC
				AND Hdr.fkid_SalesEstimate_Status = 2 --Accepted
				AND Doc.Active = 1) 
			BEGIN
				SELECT TOP 1 @compareColourWithRevisionId = id_SalesEstimate_EstimateHeader 
				FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc 
				ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader
				WHERE fkidEstimate = @estimateId 
				AND Hdr.fkid_SalesEstimate_RevisionType = 18 --BVAR-BSC
				AND Hdr.fkid_SalesEstimate_Status = 2 --Accepted
				AND Doc.Active = 1
				ORDER BY Hdr.id_SalesEstimate_EstimateHeader DESC					
			END
			ELSE IF EXISTS (SELECT id_SalesEstimate_EstimateHeader 
				FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc 
				ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader
				WHERE fkidEstimate = @estimateId 
				AND Hdr.fkid_SalesEstimate_RevisionType = 14 --PVAR-CSC
				AND Hdr.fkid_SalesEstimate_Status = 2 --Accepted
				AND Doc.Active = 1) 
			BEGIN
				SELECT TOP 1 @compareColourWithRevisionId = id_SalesEstimate_EstimateHeader 
				FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc 
				ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader
				WHERE fkidEstimate = @estimateId 
				AND Hdr.fkid_SalesEstimate_RevisionType = 14 --PVAR-CSC
				AND Hdr.fkid_SalesEstimate_Status = 2 --Accepted
				AND Doc.Active = 1
				ORDER BY Hdr.id_SalesEstimate_EstimateHeader DESC					
			END			
			ELSE 
			BEGIN
				SELECT TOP 1 @compareColourWithRevisionId = id_SalesEstimate_EstimateHeader 
				FROM tbl_SalesEstimate_EstimateHeader
				WHERE fkidEstimate = @estimateId 
				AND fkid_SalesEstimate_RevisionType = 5 --CSC
				AND fkid_SalesEstimate_Status = 2 --Accepted
				ORDER BY id_SalesEstimate_EstimateHeader DESC
			END	
		
			DECLARE @checkColourCount INT
			
			SELECT fkidEstimateDetails INTO #colorTemp 
			FROM tbl_SalesEstimate_EstimateDetails 
			WHERE fkid_SalesEstimate_EstimateHeader = @compareColourWithRevisionId

			--Compare Studio M products to the previous revision to check whether new Studio M Products have been added  
			SELECT @checkColourCount = COUNT(SED.id_SalesEstimate_EstimateDetails)
			FROM tbl_SalesEstimate_EstimateDetails SED 
			
			INNER JOIN EstimateDetails ED 
			ON SED.fkidEstimateDetails = ED.EstimateDetailsID
			
			INNER JOIN Product P ON ED.ProductID = P.ProductID
			
			WHERE SED.fkid_SalesEstimate_EstimateHeader = @revisionId 
			AND ED.EstimateID = @estimateId 
			AND P.StudioMQAndA IS NOT NULL 
			AND P.isStudioMProduct = 1 
			AND SED.fkidEstimateDetails NOT IN (Select fkidEstimateDetails FROM #colorTemp)	
			*/
			
			--IF @checkColourCount = 0
			--BEGIN
			
			--	SELECT @checkColourCount = COUNT(SED.id_SalesEstimate_EstimateDetails)
			--	FROM tbl_SalesEstimate_EstimateDetails SED 
				
			--	INNER JOIN tblStandardInclusions SI
			--	ON SED.fkidStandardInclusions = SI.idStandardInclusions
				
			--	INNER JOIN ProductAreaGroup PAG
			--	ON PAG.ProductAreaGroupID = SI.ProductAreaGroupID
				
			--	INNER JOIN Product P
			--	ON P.ProductID = PAG.ProductID
				
			--	WHERE SED.fkid_SalesEstimate_EstimateHeader = @revisionId 
			--	AND P.StudioMQAndA IS NOT NULL 
			--	AND P.isStudioMProduct = 1 
			--	AND SED.fkidStandardInclusions NOT IN 
			--	(SELECT fkidStandardInclusions FROM tbl_SalesEstimate_EstimateDetails 
			--	WHERE fkid_SalesEstimate_EstimateHeader = @compareColourWithRevisionId
			--	AND fkidStandardInclusions IS NOT NULL)
			
			--END
			
			--If there is un-answered mandatory questions for Studio M products
			IF EXISTS (SELECT SED.id_SalesEstimate_EstimateDetails FROM tbl_SalesEstimate_EstimateDetails SED 
			INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = ED.EstimateDetailsID 
			INNER JOIN Product P ON P.ProductID = ED.ProductID
			WHERE SED.fkid_SalesEstimate_EstimateHeader = @revisionId AND
			ED.EstimateID = @estimateId AND
			P.isStudioMProduct = 1 AND
			CAST(P.StudioMQAndA AS VARCHAR(MAX)) LIKE '%mandatory="1"%' AND
			(SED.StudioMAttributes IS NULL OR CAST(SED.StudioMAttributes AS VARCHAR(MAX)) = ''))
			--If RSTM has been created
			AND EXISTS (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 6 AND fkidEstimate IN (SELECT estimateId FROM @estimates))
			BEGIN
				DELETE FROM @router WHERE CheckColour = 1 AND ColourOnly = 0
			END
			ELSE
			BEGIN
				DELETE FROM @router WHERE CheckColour = 1 AND ColourOnly = 1
			END
						
		END

		IF @statusId = 2 --Accepted
		BEGIN

			INSERT INTO @result 
			SELECT AcceptedRevisionTypeId, ISNULL(T.RevisionTypeName,'N/A'), ISNULL(AcceptedToOwnerOfRevisionTypeId,0), 0, Notes  
			FROM @router R LEFT JOIN tbl_SalesEstimate_RevisionType T ON R.AcceptedRevisionTypeId = T.id_SalesEstimate_RevisionType
			WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId
									
		END
		ELSE IF @statusId = 3 --Rejected
		BEGIN

			INSERT INTO @result 
			SELECT RejectedRevisionTypeId, ISNULL(T.RevisionTypeName,'N/A'), ISNULL(RejectedToOwnerOfRevisionTypeId,0), 0, Notes  
			FROM @router R LEFT JOIN tbl_SalesEstimate_RevisionType T ON R.RejectedRevisionTypeId = T.id_SalesEstimate_RevisionType
			WHERE fkid_SalesEstimate_RevisionType = @currentRevisionTypeId

		END
			
		-- Work out the next Owner (If user is not active, assign to the queue)
		UPDATE @result SET nextRevisionOwnerId = 
		ISNULL((SELECT TOP 1 fkidOwner FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbluser Usr ON Hdr.fkidOwner = Usr.userid
		WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = ownerOfRevisionTypeId AND Usr.active = 'Y'
		ORDER BY id_SalesEstimate_EstimateHeader DESC),0)

	END
	
	-- Re-activate
	IF @statusId = 1 -- WIP (Re-activate)
	BEGIN
		INSERT INTO @result 
		SELECT fkid_SalesEstimate_RevisionType, ISNULL(T.RevisionTypeName,'N/A'), 0,  fkidOwner, NULL
		FROM tbl_SalesEstimate_EstimateHeader H 
		INNER JOIN tbl_SalesEstimate_RevisionType T ON H.fkid_SalesEstimate_RevisionType = T.id_SalesEstimate_RevisionType
		WHERE id_SalesEstimate_EstimateHeader = @revisionId
	END

	--SELECT @nonStandardRequestCount AS NSRCount, @previousRevisionId AS prevRevId, @previousRevisionTypeId AS prevRevTypeId

	SELECT DISTINCT * FROM @result
	

	
	SET NOCOUNT OFF;
	
END

GO