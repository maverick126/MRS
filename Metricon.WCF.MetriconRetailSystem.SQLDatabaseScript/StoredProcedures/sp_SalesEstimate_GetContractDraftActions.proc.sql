----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetContractDraftActions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetContractDraftActions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetContractDraftActions]
	@estimateRevisionId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @colourSelectionExists BIT
	DECLARE @electricalSelectionExists BIT
	DECLARE @pavingSelectionExists BIT
	DECLARE @tileSelectionExists BIT
	DECLARE @deckingSelectionExists BIT
	DECLARE @carpetSelectionExists BIT
	DECLARE @curtainSelectionExists BIT
	DECLARE @floorSelectionExists BIT	
	DECLARE @studioMExists BIT
	
	DECLARE @colourSelectionStatus INT
	DECLARE @electricalSelectionStatus INT
	DECLARE @pavingSelectionStatus INT
	DECLARE @tileSelectionStatus INT
	DECLARE @deckingSelectionStatus INT
	DECLARE @carpetSelectionStatus INT	
	DECLARE @curtainSelectionStatus INT
	DECLARE @floorSelectionStatus INT
		
	DECLARE @colourSelectionInProgress BIT
	DECLARE @electricalSelectionInProgress BIT
	DECLARE @pavingSelectionInProgress BIT
	DECLARE @tileSelectionInProgress BIT
	DECLARE @deckingSelectionInProgress BIT
	DECLARE @carpetSelectionInProgress BIT
	DECLARE @curtainSelectionInProgress BIT
	DECLARE @floorSelectionInProgress BIT	
	
	DECLARE @estimateId INT, @currentMRSGroupID INT
	
	SELECT @estimateId = fkidEstimate ,
	       @currentMRSGroupID=r.MRSGroupID
	FROM tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
	INNER JOIN Region r ON e.RegionID=r.RegionID
	WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId

	SELECT TOP 1 @colourSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 7 AND fkidEstimate = @estimateId
	IF @@ROWCOUNT > 0
	BEGIN
		SET @colourSelectionExists = 1
		IF @colourSelectionStatus = 1
		BEGIN
			SET @colourSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @colourSelectionInProgress = 0
		END
	END
	ELSE
	BEGIN
		SET @colourSelectionExists = 0
		SET @colourSelectionInProgress = 0
	END

	SELECT TOP 1 @electricalSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 8 AND fkidEstimate = @estimateId
	IF @@ROWCOUNT > 0
	BEGIN
		SET @electricalSelectionExists = 1
		IF @electricalSelectionStatus = 1
		BEGIN
			SET @electricalSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @electricalSelectionInProgress = 0
		END
	END
	ELSE
	BEGIN
		SET @electricalSelectionExists = 0
		SET @electricalSelectionInProgress = 0
	END

	SELECT TOP 1 @pavingSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 9 AND fkidEstimate = @estimateId
	IF  @@ROWCOUNT > 0
	BEGIN
		SET @pavingSelectionExists = 1
		IF @pavingSelectionStatus = 1
		BEGIN
			SET @pavingSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @pavingSelectionInProgress = 0
		END		
	END
	ELSE
	BEGIN
		SET @pavingSelectionExists = 0
		SET @pavingSelectionInProgress = 0
	END

	SELECT TOP 1 @tileSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 10 AND fkidEstimate = @estimateId
	IF  @@ROWCOUNT > 0
	BEGIN
		SET @tileSelectionExists = 1
		IF @tileSelectionStatus = 1
		BEGIN
			SET @tileSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @tileSelectionInProgress = 0
		END			
	END
	ELSE
	BEGIN
		SET @tileSelectionExists = 0
		SET @tileSelectionInProgress = 0
	END

	SELECT TOP 1 @deckingSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 11 AND fkidEstimate = @estimateId
	IF  @@ROWCOUNT > 0
	BEGIN
		SET @deckingSelectionExists = 1
		IF @deckingSelectionStatus = 1
		BEGIN
			SET @deckingSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @deckingSelectionInProgress = 0
		END		
	END
	ELSE
	BEGIN
		SET @deckingSelectionExists = 0
		SET @deckingSelectionInProgress = 0
	END

	SELECT TOP 1 @carpetSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 12 AND fkidEstimate = @estimateId
	IF  @@ROWCOUNT > 0
	BEGIN
		SET @carpetSelectionExists = 1
		IF @carpetSelectionStatus = 1
		BEGIN
			SET @carpetSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @carpetSelectionInProgress = 0
		END		
	END
	ELSE
	BEGIN
		SET @carpetSelectionExists = 0
		SET @carpetSelectionInProgress = 0
	END

	SELECT TOP 1 @curtainSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 21 AND fkidEstimate = @estimateId
	IF  @@ROWCOUNT > 0
	BEGIN
		SET @curtainSelectionExists = 1
		IF @curtainSelectionStatus = 1
		BEGIN
			SET @curtainSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @curtainSelectionInProgress = 0
		END		
	END
	ELSE
	BEGIN
		SET @curtainSelectionExists = 0
		SET @curtainSelectionInProgress = 0
	END
	
	SELECT TOP 1 @floorSelectionStatus = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 22 AND fkidEstimate = @estimateId
	IF  @@ROWCOUNT > 0
	BEGIN
		SET @floorSelectionExists = 1
		IF @floorSelectionStatus = 1
		BEGIN
			SET @floorSelectionInProgress = 1
		END
		ELSE
		BEGIN
			SET @floorSelectionInProgress = 0
		END		
	END
	ELSE
	BEGIN
		SET @floorSelectionExists = 0
		SET @floorSelectionInProgress = 0
	END	

	IF EXISTS (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 23 AND fkidEstimate = @estimateId)
	BEGIN
		SET @studioMExists = 1
	END
	ELSE
	BEGIN
		SET @studioMExists = 0
	END

-- get merge stuff (i.e QLD paving merge to color)

    SELECT IDENTITY(INT) AS tid,
           CAST(id_SalesEstimate_RevisionType AS VARCHAR) AS id_SalesEstimate_RevisionType, 
           ExcMRSGroupIDWhenSplit 
    INTO #temptab
    FROM tbl_SalesEstimate_RevisionType
    WHERE ExcMRSGroupIDWhenSplit IS NOT NULL AND LTRIM(RTRIM(ExcMRSGroupIDWhenSplit))<>'' AND Active=1

    DECLARE @idx INT, @total INT, @temprevisontypeid INT
    DECLARE @tempstring VARCHAR(200)

    SELECT @total=COUNT(*) FROM #temptab
    SET @idx=1
-- end of merge stuff

    WHILE(@idx<=@total)
       BEGIN
           SELECT @tempstring=ExcMRSGroupIDWhenSplit,
                  @temprevisontypeid= id_SalesEstimate_RevisionType
           FROM #temptab WHERE tid=@idx
           
		   SELECT data AS MRSGroupID
		   INTO #tempMRSGroup 
		   FROM dbo.Splitfunction_string_to_table(@tempstring,',')           
           
           IF(EXISTS(SELECT MRSGroupID FROM #tempMRSGroup WHERE MRSGroupID=@currentMRSGroupID))
               BEGIN
                  IF (@temprevisontypeid=8)
                      BEGIN
                         SET @electricalSelectionExists = 1
                      END
                  ELSE IF(@temprevisontypeid=9)
                      BEGIN
                         SET @pavingSelectionExists = 1
                      END  
                  ELSE IF(@temprevisontypeid=10)
                      BEGIN
                         SET @tileSelectionExists = 1
                      END 
                  ELSE IF(@temprevisontypeid=11)
                      BEGIN
                         SET @deckingSelectionExists = 1
                      END 
                  ELSE IF(@temprevisontypeid=12)
                      BEGIN
                         SET @carpetSelectionExists = 1
                      END  
                  ELSE IF(@temprevisontypeid=21)
                      BEGIN
                         SET @curtainSelectionExists = 1
                      END 
                  ELSE IF(@temprevisontypeid=22)
                      BEGIN
                         SET @floorSelectionExists = 1
                      END  
                                                                                                                                              
               END
           
           SET @idx=@idx+1
           DROP TABLE #tempMRSGroup
       END


	SELECT @colourSelectionExists ColourSelectionExists, 
	@electricalSelectionExists ElectricalSelectionExists, 
	@pavingSelectionExists PavingSelectionExists, 
	@tileSelectionExists TileSelectionExists, 
	@deckingSelectionExists DeckingSelectionExists, 
	@carpetSelectionExists CarpetSelectionExists, 
	@curtainSelectionExists CurtainSelectionExists, 
	@floorSelectionExists FloorSelectionExists, 
	@studioMExists StudioMExists,
	@colourSelectionInProgress ColourSelectionInProgress, 
	@electricalSelectionInProgress ElectricalSelectionInProgress, 
	@pavingSelectionInProgress PavingSelectionInProgress, 
	@tileSelectionInProgress TileSelectionInProgress, 
	@deckingSelectionInProgress DeckingSelectionInProgress, 
	@carpetSelectionInProgress CarpetSelectionInProgress,
	@curtainSelectionInProgress CurtainSelectionInProgress,
	@floorSelectionInProgress FloorSelectionInProgress

END


GO