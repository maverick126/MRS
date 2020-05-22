----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateHeader]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateHeader]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateHeader]
	@revisionId int
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @cmd				VARCHAR(MAX)
	DECLARE @crmDataSource		VARCHAR(255)
	DECLARE @currentpromotion	VARCHAR(500)
	DECLARE @salesacceptor		VARCHAR(100)
	DECLARE @draftperson		VARCHAR(100)
	DECLARE @colorconsultant	VARCHAR(100)
	DECLARE @salesestimator		VARCHAR(100)
	DECLARE @CSC				VARCHAR(100)
	DECLARE @estimateid INT, @promotionid INT, @currentrevisontype INT, @regionid INT, @brandid INT
	DECLARE @houseAndLandPackage VARCHAR(100), @homeproductid VARCHAR(50)
	DECLARE @totalprice DECIMAL(18,2), @updagradecost DECIMAL(18,2), @totalcost DECIMAL(18,2), @margin DECIMAL(18,2)
	DECLARE @homeprice DECIMAL(18,2), @depositdate DATETIME
	DECLARE @derivedhomepercentage DECIMAL(18,4), @deriveditempercentage DECIMAL(18,4), @targetmargin DECIMAL(18,4)
	DECLARE @homecost DECIMAL(18,2),@siteworkcost DECIMAL(18,2), @upgradecost DECIMAL(18,2)
	DECLARE @customerDocumentName VARCHAR(50), @customerDocumentDesc VARCHAR(50)
	
	SET @salesacceptor=''
	SET @draftperson=''
	SET @salesestimator=''
	SET @CSC=''	
	SET @colorconsultant=''
	
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND Active = 1)
	BEGIN
		DECLARE @documentType VARCHAR(50)
		SET @documentType = (SELECT TOP 1 DocumentType FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND Active = 1)
		IF @documentType = 'PC' OR @documentType = 'Variation'
			SET @customerDocumentName = (SELECT TOP 1 DocumentType + ' ' + CAST(DocumentNumber AS VARCHAR(10)) FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND Active = 1)
		ELSE IF @documentType = 'Contract'
			SET @customerDocumentName = 'HIA Contract'
		ELSE
			SET @customerDocumentName = ''
			
	    SET @customerDocumentDesc = (SELECT TOP 1 [Description] FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND Active = 1)
	END
	ELSE
	   BEGIN
	      SET @customerDocumentDesc =''
		  SET @customerDocumentName = ''
	   END
	
-- find the selected promotion	
	SELECT		@estimateid=fkidestimate,
	            @currentrevisontype=fkid_salesestimate_revisiontype,
				@promotionid=e.promotionid,
				@houseAndLandPackage =
				CASE WHEN e.fkidpackage IS NULL THEN  'No'
				ELSE 'Yes' END	,
				@regionid=e.RegionID,
				@homeproductid=h.ProductID,
				@brandid=h.BrandID,
				@homeprice=eh.HomePrice,
				@depositdate=dp.DepositDate
	FROM		tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN	estimate e	ON eh.fkidestimate=e.estimateid
	INNER JOIN  home h ON e.HomeID=h.HomeID
	INNER JOIN  DepositDetails dp ON e.fkidOpportunity=dp.fkidOpportunity
	WHERE		id_SalesEstimate_EstimateHeader=@revisionId
	
	EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
	EXEC sp_SalesEstimate_GetEstimateTotalCost @revisionId,@homecost OUTPUT,@upgradecost OUTPUT, @siteworkcost OUTPUT	
    
    SET @totalcost=CAST(ISNULL(@homecost,0)+ISNULL(@upgradecost,0)+ISNULL(@siteworkcost,0) AS DECIMAL(18,2))
	
	SELECT EstimateDetailsID, ProductID, Selected, PromotionProduct INTO #temped
	FROM	estimatedetails 
	WHERE	estimateid=@estimateid
	
	SELECT fkidEstimateDetails, Quantity, ItemPrice, IsSiteWork, IsPromotionProduct INTO #tempsed
	FROM	tbl_SalesEstimate_EstimateDetails 
	WHERE	fkid_SalesEstimate_EstimateHeader=@revisionId
	
	SELECT      @currentpromotion=promotionname
	FROM		#temped ed
	INNER JOIN	tblmultiplepromotion mp	ON ed.productid=mp.baseproductid
	WHERE		ed.selected=1 AND fkidpromotionid=@promotionid
	
	IF (@currentpromotion IS NULL) SET @currentpromotion='Not Selected'
-- end promotion
-- get sales acceptor /draft/ estimator

	   SELECT       @salesestimator=u.username
	   FROM			tbl_SalesEstimate_EstimateHeader eh
	   INNER JOIN	tbluser u		ON	eh.fkidowner=u.userid
	   WHERE		fkidEstimate=@estimateid AND fkid_SalesEstimate_RevisionType=4 AND fkid_salesestimate_status=2

	   SELECT       top 1 @draftperson=u.username
	   FROM			tbl_SalesEstimate_EstimateHeader eh
	   INNER JOIN	tbluser u		ON	eh.fkidowner=u.userid
	   WHERE		fkidestimate=@estimateid			AND
					fkid_salesestimate_revisiontype=3	AND
					fkid_salesestimate_status=2
	   ORDER BY		id_SalesEstimate_EstimateHeader	DESC
	   
	   SELECT       top 1 @colorconsultant=u.username
	   FROM			tbl_SalesEstimate_EstimateHeader eh
	   INNER JOIN	tbluser u		ON	eh.fkidowner=u.userid
	   WHERE		fkidestimate=@estimateid			AND
					fkid_salesestimate_revisiontype=6	AND
					fkid_salesestimate_status=2
	   ORDER BY		id_SalesEstimate_EstimateHeader	DESC
	   
 --   -- always find acceptor
       SELECT       top 1 @salesacceptor=u.username
       FROM			tbl_SalesEstimate_EstimateHeader eh
       INNER JOIN	tbluser u		ON	eh.fkidowner=u.userid
       WHERE		fkidestimate=@estimateid			AND
					fkid_salesestimate_revisiontype=2	AND
					fkid_salesestimate_status=2
	   ORDER BY		id_SalesEstimate_EstimateHeader	DESC
    -- end find acceptor

-- end of sales acceptor /draft/ estimator
	
-- get site work temp table

		SELECT SED.Quantity, SED.ItemPrice 
		INTO #sitework 
		FROM #tempsed SED
		WHERE SED.IsSiteWork=1 --Site Works Area	 
		
		SELECT DISTINCT
		A.name AS CustomerName, 
		E.BCCustomerID AS CustomerNumber, 
		E.BCContractNumber AS ContractNumber, 
		E.EstimateID AS EstimateNumber, 
		E.fkidopportunity,
		H.HomeName, 
		U.username AS SalesConsultantName, 
		EH.RevisionNumber,
		EH.fkid_SalesEstimate_RevisionType AS RevisionTypeId, 
		REV.Abbreviation AS RevisionTypeCode, 
		EH.CreatedOn,
		EH.fkid_SalesEstimate_Status AS StatusId,
		ST.Status AS StatusName,
		EH.fkidOwner AS OwnerId, 
		U2.username AS OwnerName,
		EH.id_SalesEstimate_EstimateHeader AS EstimateHeaderId, 
		EH.HomePrice,
		EH.HomePriceEffectiveDate AS EffectiveDate,
		EH.DueDate,
		EH.AppointmentDateTime,
		EH.Comments,
		EH.fkid_SalesEstimate_DifficultyRating AS DifficultyRatingId,
		b.brandname,
        r.regionname,
        h.brandid,
		CT.New_LotNumber AS LotNumber,
		CT.New_address_street AS StreetAddress,
		CT.New_address_suburb AS Suburb,
		CT.New_address_streetnumber AS StreetNumber,
		CT.New_address_postcode AS Postcode,
		CT.New_state AS State,
		ISNULL((SELECT SUM(SED.ItemPrice * SED.Quantity) FROM #tempsed SED INNER JOIN #temped ED ON SED.fkidEstimateDetails = EstimateDetailsID 
		WHERE SED.IsPromotionProduct = 0),0) AS UpgradeValue,

        @upgradecost AS UpgradeCost,
		
		ISNULL((SELECT SUM(SED.ItemPrice * SED.Quantity) FROM #tempsed SED INNER JOIN #temped ED ON SED.fkidEstimateDetails = EstimateDetailsID 
		WHERE SED.IsPromotionProduct = 1),0) AS PromotionValue,	

		ISNULL((SELECT SUM(ItemPrice * Quantity) FROM #sitework ),0) AS SiteWorkValue,
        @siteworkcost AS siteworkcost,
		@currentpromotion AS currentpromotion , 
		@salesacceptor AS SalesAcceptor , 
		@draftperson AS DraftPerson , 
		@salesestimator AS Salesestimator ,	
		@CSC AS CSC,
		@colorconsultant AS colorconsultant,
 		@houseAndLandPackage AS HouseAndLandPackage,
 		ISNULL(EH.ContractType,'PC') AS ContractType,
 		@customerDocumentName AS CustomerDocumentName,
 		@customerDocumentDesc AS customerDocumentDesc 
    INTO #final
	FROM tbl_SalesEstimate_EstimateHeader EH
	INNER JOIN Estimate E ON EH.fkidEstimate = E.EstimateID
	INNER JOIN tblUser U ON E.BCSalesConsultant = U.usercode
	INNER JOIN tblUser U2 ON EH.fkidOwner = U2.userid
	INNER JOIN tblUserSubRegionMapping M ON M.fkidUser = U.userid
	INNER JOIN Home H ON H.HomeID = E.HomeID
	INNER JOIN brand b ON h.brandid=b.brandid
	INNER JOIN region r ON e.regionid=r.regionid
	INNER JOIN tbl_SalesEstimate_RevisionType REV ON EH.fkid_SalesEstimate_RevisionType = REV.id_SalesEstimate_RevisionType
	INNER JOIN tbl_SalesEstimate_Status ST ON EH.fkid_SalesEstimate_Status = ST.id_SalesEstimate_Status
	LEFT JOIN syn_CRM_Account A ON E.fkidAccount = A.AccountId
	LEFT JOIN syn_CRM_Opportunity O ON E.fkidOpportunity = O.OpportunityId
	LEFT JOIN syn_CRM_New_Contract CT ON O.new_contractid = CT.new_contractid
	WHERE id_SalesEstimate_EstimateHeader = @revisionId	


    UPDATE #final
    SET UpgradeValue=UpgradeValue-SiteWorkValue
    
    SELECT *,
           CAST(HomePrice+UpgradeValue+SiteWorkValue AS DECIMAL(18,2) ) AS TotalPrice,
           @totalcost AS totalcost, 
           CAST(((HomePrice+UpgradeValue+SiteWorkValue)/1.1)-@totalcost AS DECIMAL(18,2) ) AS totalmargin,
           CAST(100*(((HomePrice+UpgradeValue+SiteWorkValue)/1.1)-@totalcost)/((HomePrice+UpgradeValue+SiteWorkValue)/1.1) AS DECIMAL(18,2)) AS margin,
           CAST(100*ISNULL(@targetmargin,0) AS DECIMAL(18,2) ) AS targetmargin,
           ISNULL(@homecost,0) AS homecost
    FROM #final
    
    DROP TABLE #temped
    DROP TABLE #tempsed
    DROP TABLE #sitework
    DROP TABLE #final
END
GO