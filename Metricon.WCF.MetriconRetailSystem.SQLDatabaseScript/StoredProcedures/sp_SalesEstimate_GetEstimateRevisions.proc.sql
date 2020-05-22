
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateRevisions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateRevisions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO


ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateRevisions]
	@estimateId INT
AS
BEGIN
	SET NOCOUNT ON;
    DECLARE @contractnumber VARCHAR(10), @homeproductid VARCHAR(50)
    DECLARE @regionid INT, @brandid INT
    DECLARE @total INT, @index INT
	DECLARE @totalprice DECIMAL(18,2), @homecost DECIMAL(18,2),@totalcost DECIMAL(18,2), @margin DECIMAL(18,2)
	DECLARE @homeprice DECIMAL(18,2), @totalupgrade DECIMAL(18,2), @upgradecost DECIMAL(18,2), @depositdate DATETIME
	DECLARE @derivedhomepercentage DECIMAL(18,4), @deriveditempercentage DECIMAL(18,4), @targetmargin DECIMAL(18,4)
	
    SELECT @estimateId AS estimateid INTO #tempEST
    
    SELECT @contractnumber=e.BCContractNumber,
           @regionid=e.RegionID,
           @brandid=h.BrandID,
           @depositdate=dp.DepositDate
    FROM Estimate e
    INNER JOIN Home h ON e.HomeID=h.HomeID
    INNER JOIN DepositDetails dp ON e.fkidOpportunity=dp.fkidOpportunity
    where EstimateID=@estimateId
    
	
	EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
	 
    
    
    INSERT INTO #tempEST
    SELECT estimateid
    FROM Estimate 
    WHERE BCContractNumber=@contractnumber AND (CreatedBy='MRS Change Facade' OR (AcceptDate IS NOT NULL) ) AND EstimateID<>@estimateId

    

	SELECT 
	IDENTITY(INT) AS tid,	
	eh.fkidEstimate,
	EH.RevisionNumber,
	EH.CreatedOn, 
	U.username AS OwnerName,
	S.Status AS StatusName,
	CASE 
		WHEN EH.AppointmentDateTime IS NULL
		THEN EH.Comments
		ELSE 'Appointment Time: ' + CONVERT(varchar(10),EH.AppointmentDateTime,103)+' ' + 
        LTRIM(RIGHT(CONVERT(varchar,EH.AppointmentDateTime,0),7)) + CASE WHEN EH.Comments IS NULL THEN '' ELSE CHAR(10) + EH.Comments END
	END AS Comments,
	CAST(('('+REV.Abbreviation+')') AS VARCHAR(200)) AS RevisionTypeCode,
	EH.id_SalesEstimate_EstimateHeader AS EstimateHeaderID,
    CAST(0.0 AS DECIMAL(18,2)) AS ContractValue,
    CAST(0.0 AS DECIMAL(18,2)) AS TotalCost,
    CAST(0.0 AS DECIMAL(18,2)) AS Margin
    INTO #tempRevison
	FROM tbl_SalesEstimate_EstimateHeader EH

	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN tblUser U 
	ON EH.fkidOwner = U.userid

	INNER JOIN tbl_SalesEstimate_Status S 
	ON S.id_SalesEstimate_Status = EH.fkid_SalesEstimate_Status

	INNER JOIN tbl_SalesEstimate_RevisionType REV
	ON EH.fkid_SalesEstimate_RevisionType = REV.id_SalesEstimate_RevisionType

	WHERE EH.fkidEstimate  in (SELECT DISTINCT EstimateID FROM #tempEST)
	ORDER BY fkidEstimate, EH.RevisionNumber

-- get cost and margin
    DECLARE @temprevisionid INT, @tempestid INT
    IF(EXISTS(SELECT * FROM #tempRevison))
       BEGIN
         SET @index=1
         SELECT @total=COUNT(*) FROM #tempRevison
         WHILE (@index<=@total)
             BEGIN
                SELECT @tempestid=fkidestimate,
                       @temprevisionid=estimateheaderid
                FROM   #tempRevison
                WHERE  tid=@index
                -- get home price, upgrade price etc
                
                SELECT @homeprice=HomeSellPrice,
                       @homeproductid=h.ProductID,
                       @regionid=e.RegionID,
                       @brandid=h.BrandID
                FROM Estimate e
                INNER JOIN Home h ON e.HomeID=h.HomeID
                WHERE EstimateID= @tempestid
                
                SELECT TOP 1 @homecost=ISNULL(CostPrice,0)
                FROM Price
                WHERE ProductID=@homeproductid AND RegionID=@regionid AND Active=1 AND EffectiveDate<=@depositdate
                ORDER BY EffectiveDate DESC

                IF(@homecost=0)
                   BEGIN
                     SET @homecost=CAST((@homeprice/1.1)*(1-@derivedhomepercentage) AS DECIMAL(18,2))
                   END
                             
                SELECT @totalupgrade=CAST(ISNULL(SUM(sed.Quantity*sed.ItemPrice),0) AS DECIMAL(18,2))
                FROM tbl_SalesEstimate_EstimateDetails sed
                INNER JOIN EstimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID
                WHERE fkid_SalesEstimate_EstimateHeader=@temprevisionid  AND ed.StandardInclusion=0  AND ed.PromotionProduct=0

                SELECT 
                       @upgradecost=CAST(ISNULL(SUM(sed.Quantity*sed.CostExcGST),0) AS DECIMAL(18,2))
                FROM tbl_SalesEstimate_EstimateDetails sed
                INNER JOIN EstimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID
                WHERE fkid_SalesEstimate_EstimateHeader=@temprevisionid  AND ed.StandardInclusion=0 AND ed.PromotionProduct=0
                               
                SET @totalcost=@homecost+@upgradecost
                SET @totalprice=@homeprice+@totalupgrade
                IF(@totalprice<>0)
                   BEGIN
                      SET @margin=CAST(100*((@totalprice/1.1)-@totalcost)/(@totalprice/1.1) AS DECIMAL(18,2))
                   END
                ELSE
                   BEGIN
                      SET @margin=0
                   END
                UPDATE #tempRevison
                SET   ContractValue=@totalprice,
                      totalcost=@totalcost,
                      margin=@margin
                WHERE tid=@index

				--Update Customer Document Info
				IF EXISTS(SELECT * FROM #tempRevison Tmp INNER JOIN tbl_SalesEstimate_CustomerDocument Doc 
					ON Tmp.EstimateHeaderID = Doc.fkid_SalesEstimate_EstimateHeader
					WHERE Doc.fkid_SalesEstimate_EstimateHeader = @temprevisionid)
				BEGIN	
					DECLARE @contractType VARCHAR(50), @status INT, @sentDate VARCHAR(50), @acceptedDate VARCHAR(50), 
					@documentType VARCHAR(50), @documentNumber INT, @documentActive BIT
					
					SELECT @contractType = ContractType, @status = fkid_SalesEstimate_Status FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @temprevisionid
					
					SELECT @sentDate = CASE WHEN SentDate IS NULL THEN '-' ELSE CONVERT(VARCHAR(20), SentDate, 103) END, 
					@acceptedDate = CASE WHEN AcceptedDate IS NULL THEN '-' ELSE CONVERT(VARCHAR(20), AcceptedDate, 103) END, 
					@documentType = DocumentType, 
					@documentNumber = ISNULL(DocumentNumber,0),
					@documentActive = Active 
					FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @temprevisionid
					
					DECLARE @documentInfo VARCHAR(255)
					DECLARE @milestone VARCHAR(200)
					
					SET @documentInfo = ''
					SET @milestone = ''
					
					IF (@status = 3 AND @sentDate <> '') 
						SET @documentInfo = 'Sent: ' + @sentDate
					ELSE IF (@documentType = 'PC' AND @sentDate <> '' AND @documentActive = 0)
						SET @documentInfo = 'Sent: ' + @sentDate
					ELSE IF (@documentType = 'PC' AND @documentActive = 1)
						SET @documentInfo = 'PC ' + CONVERT(VARCHAR(5), @documentNumber) + CHAR(10) + 'Sent: ' + @sentDate
					ELSE IF (@documentType = 'Contract' AND @documentActive = 1)
					    BEGIN
						    SET @documentInfo = 'HIA Contract' + CHAR(10) + 'Sent: ' + @sentDate + CHAR(10) + 'Accepted: ' + @acceptedDate
						    SET @milestone='HIA Contract'
						END
					ELSE IF (@documentType = 'Variation' AND @documentActive = 1)
					    BEGIN
						  SET @documentInfo = 'Variation ' + CONVERT(VARCHAR(5), @documentNumber) + CHAR(10) + 'Sent: ' + @sentDate + CHAR(10) + 'Accepted: ' + @acceptedDate
						  SEt @milestone='Variation ' + CONVERT(VARCHAR(5), @documentNumber)
					    END
					IF (@documentInfo <> '')
					BEGIN
						UPDATE #tempRevison
						SET Comments = CASE WHEN Comments IS NULL THEN '' ELSE Comments + CHAR(10) END + @documentInfo,
						    RevisionTypeCode= CASE WHEN RTRIM(@milestone)<>'' 
						                           THEN RevisionTypeCode+' - '+RTRIM(@milestone)
						                           ELSE RevisionTypeCode
						                      END
						WHERE tid=@index					
					END			 
				END

                SET @index=@index+1
             END
       END
    
    SELECT * FROM #tempRevison
-- end cost and margin

    DROP TABLE #tempEST
    DROP TABLE #tempRevison
    

	SET NOCOUNT OFF;
END

GO