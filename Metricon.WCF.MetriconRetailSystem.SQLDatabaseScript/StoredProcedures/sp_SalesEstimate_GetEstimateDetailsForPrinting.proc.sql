----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateDetailsForPrinting]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateDetailsForPrinting]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER Procedure [dbo].[sp_SalesEstimate_GetEstimateDetailsForPrinting]
	@revisionId int,
	@printtype  varchar(50),
	@includestd VARCHAR(10)
as
BEGIN

	SET NOCOUNT ON;

-- get site work temp table
        DECLARE @docutype VARCHAR(50)
		declare @tt table (doctype varchar(20))
		insert into @tt
		exec sp_SalesEstimate_GetCustomerDocumentType 25152
		
        SELECT @docutype=doctype FROM @tt
 
		  
		--SELECT SED.Quantity, SED.ItemPrice
		--INTO #sitework 
		--FROM tbl_SalesEstimate_EstimateDetails SED 
		--INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID
		--INNER JOIN ProductAreaGroup pag ON ED.ProductAreaGroupID=pag.ProductAreaGroupID
		--WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND PromotionProduct = 0 AND pag.IsSiteWork=1

		--UNION ALL
        DECLARE @totalsitecost DECIMAL(18,2), @totalsurcharge DECIMAL(18,2), @siteworksurcharge DECIMAL(18,2),@nonsiteworksurcharge DECIMAL(18,2)
        DECLARE @totalprovisionalsum DECIMAL(18,2), @stateid INT
        
        SELECT @stateid=h.fkStateID FROM tbl_SalesEstimate_EstimateHeader eh
        inner join Estimate e on eh.fkidEstimate=e.EstimateID
        inner join Home h on e.HomeID=h.homeid
        WHERE eh.id_SalesEstimate_EstimateHeader=@revisionId

        IF(@stateid<>2)
            BEGIN
              SET @totalprovisionalsum=0
            END
        ELSE
            BEGIN
            
				SELECT @totalprovisionalsum= ISNULL(SUM(ISNULL(SED.ItemPrice,0) * SED.Quantity),0) 
				FROM tbl_SalesEstimate_EstimateDetails SED 
				INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
				WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND areaid =108
				
            END



		SELECT SED.Quantity,SED.ItemPrice 
		INTO #sitework
		FROM tbl_SalesEstimate_EstimateDetails SED 
		WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND IsSiteWork=1
		
		
		SELECT @totalsitecost=ISNULL(SUM(Quantity*ISNULL(ItemPrice,0)),0)
		FROM #sitework
		--INNER JOIN Area a ON SED.fkid_NonStandardArea=a.AreaID
		--WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND a.AreaID=55
	 
        SELECT @totalsurcharge= ISNULL(SUM(ISNULL(SED.ItemPrice,0) * SED.Quantity),0) 
        FROM tbl_SalesEstimate_EstimateDetails SED 
        INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
	    WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND IsPromotionProduct = 0 AND areaid = 1
	    
        SELECT @siteworksurcharge= ISNULL(SUM(ISNULL(SED.ItemPrice,0) * SED.Quantity),0) 
        FROM tbl_SalesEstimate_EstimateDetails SED 
        INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
	    WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND IsPromotionProduct = 0 AND areaid = 1	AND sed.IsSiteWork=1
	    
        SELECT @nonsiteworksurcharge= ISNULL(SUM(ISNULL(SED.ItemPrice,0) * SED.Quantity) ,0)
        FROM tbl_SalesEstimate_EstimateDetails SED 
        INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
	    WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND IsPromotionProduct = 0 AND areaid = 1	AND sed.IsSiteWork=0	        
	    	
-- end site work temp table	

    IF (@printtype<>'changesonly')
    BEGIN
			SELECT
			E.EstimateID, 
			E.fkidAccount AS AccountId,
			E.fkidOpportunity AS OpportunityId,
			E.RegionID,
			S.StateAbbreviation AS [State], 
			s.stateid,
			CASE WHEN EH.HomeDisplayName IS NULL THEN H.HomeName ELSE EH.HomeDisplayName END AS HomeName,			
			EH.RevisionNumber,
			R.RevisionTypeName AS RevisionType, 
			--U.username AS OwnerName,
			(CASE WHEN 0 = CHARINDEX(' ',U.username) 
			THEN 
			U.username
			ELSE
			SUBSTRING(U.username,1,CHARINDEX(' ', U.username) -1) 
			END) + ' (' + U.usercode + ')' AS OwnerName,
			U.username AS OwnerFullName,			
			EH.HomePrice,
			EH.HomePriceEffectiveDate AS EffectiveDate,
			E.fkidpackage AS PackageId,
			EH.fkid_SalesEstimate_Status AS StatusId,
			EH.fkid_SalesEstimate_RevisionType AS RevisionTypeId,
            r.Abbreviation AS briefRevisonType, 
            
			ISNULL((SELECT SUM(SED.ItemPrice * SED.Quantity) FROM tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
			WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND sed.IsPromotionProduct = 0),0)-@totalsitecost-@totalprovisionalsum AS UpgradeValue,

			ISNULL((SELECT SUM(SED.ItemPrice * SED.Quantity) FROM tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
			WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND sed.IsPromotionProduct = 1),0) AS PromotionValue,	

            @totalsitecost AS SiteWorkValue,

			@totalsurcharge AS Surcharge,
			@siteworksurcharge AS siteworksurcharge,
			@nonsiteworksurcharge AS nonsiteworksurcharge,
            @totalprovisionalsum AS provisionalsums,
			ISNULL((SELECT L.Sellprice FROM Estimate E
			INNER JOIN tblPackage P ON P.idPackage = E.fkidPackage
			INNER JOIN tblLand L ON L.idLand = P.fkidLand
			INNER JOIN tbl_SalesEstimate_EstimateHeader H ON H.fkidEstimate = E.EstimateId
			WHERE H.id_SalesEstimate_EstimateHeader = @revisionId),0) AS LandPrice,
            eh.AppointmentDateTime,
            @docutype AS documenttype
            
            
			FROM tbl_SalesEstimate_EstimateHeader EH

			INNER JOIN Estimate E 
			ON EH.fkidEstimate = E.EstimateID

			INNER JOIN tblUser U 
			ON EH.fkidOwner = U.userid

			INNER JOIN [State] S
			ON U.fkStateID = S.StateID
			
			INNER JOIN Home H
			ON E.HomeID = H.HomeID
			
			INNER JOIN tbl_SalesEstimate_RevisionType R 
			ON EH.fkid_SalesEstimate_RevisionType = R.id_SalesEstimate_RevisionType

			WHERE id_SalesEstimate_EstimateHeader = @revisionId
	END
	ELSE
    BEGIN
            declare @total decimal(18,2)
			declare @tempTab table 
			(
						optionid int,
						fkidestimatedetails int,
						homeid int, 
						orginalarea varchar(100), 
						groupname varchar(100), 
						productid varchar(100),
						productname varchar(max),
						productdescription varchar(max),			
						nonstandardcatID int,
						nonstandardgroupID int,
						quantity decimal(18,2),
						sellprice decimal(18,2),
						totalprice decimal(18,2),
						productareagroupid int,
						standardinclusion int,
						standardoption int,
						addextradesc int,
						enterdesc varchar(max),
						internaldesc varchar(max),
						additionalinfo varchar(max),
						--sortorder int,
						--gsortorder int,
						--psortorder int,
						uom varchar(200),
						areaid int,
						groupid int,
						promotionproduct int,
						catorder int, 
						areaname varchar(100),
						displayAt varchar(100),
						icon varchar(100),
						StandardPackageInclusion int, 
						gorder int,
						porder int,
						ItemAccepted int,
						isstudiomproduct int,
						studiomattributes varchar(max)	,
						selectedimageid int,
						changed varchar(100),
						fkidStandardInclusions int,
						productpricedisplaycode int,
						nonstandardpricedisplaycode int,
						printprice varchar(100),
						img image
			)
			insert into @tempTab
			exec sp_SalesEstimate_GetEstimateDetailsForPrinting @revisionId,@printtype,1
            
			select @total=SUM(totalprice) 
			from @tempTab
			--where changed<>'DELETED'    
    
    
			SELECT
			E.EstimateID, 
			E.fkidAccount AS AccountId,
			E.fkidOpportunity AS OpportunityId,
			E.RegionID,
			S.StateAbbreviation AS [State], 
			s.StateID,
			CASE WHEN EH.HomeDisplayName IS NULL THEN H.HomeName ELSE EH.HomeDisplayName END AS HomeName,
			EH.RevisionNumber,
			R.RevisionTypeName AS RevisionType,  
			--U.username AS OwnerName,
			(CASE WHEN 0 = CHARINDEX(' ',U.username) 
			THEN 
			U.username
			ELSE
			SUBSTRING(U.username,1,CHARINDEX(' ', U.username) -1) 
			END) + ' (' + U.usercode + ')' AS OwnerName,
			U.username AS OwnerFullName,	
			EH.HomePrice,
			EH.HomePriceEffectiveDate AS EffectiveDate,
			E.fkidpackage AS PackageId,
			EH.fkid_SalesEstimate_Status AS StatusId,
			EH.fkid_SalesEstimate_RevisionType AS RevisionTypeId,
            r.Abbreviation AS briefRevisonType,
            @total-@totalsitecost-@totalprovisionalsum AS UpgradeValue,
            0 AS PromotionValue,
            0 AS SiteWorkValue,
            0 AS Surcharge,
			0 AS siteworksurcharge,
			0 AS nonsiteworksurcharge,
			@totalprovisionalsum AS provisionalsums,  
			@docutype AS documenttype,          
			--ISNULL((SELECT SUM(SED.ItemPrice * SED.Quantity) FROM tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
			--WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND PromotionProduct = 1 AND Changed=1),0) AS PromotionValue,	

			--ISNULL((SELECT SUM(SED.ItemPrice * SED.Quantity) FROM tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
			--WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND PromotionProduct = 0 AND areaid = 55 AND Changed=1),0) AS SiteWorkValue,

			--ISNULL((SELECT SUM(SED.ItemPrice * SED.Quantity) FROM tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = EstimateDetailsID 
			--WHERE fkid_SalesEstimate_EstimateHeader = @revisionId AND PromotionProduct = 0 AND areaid = 1 AND Changed=1),0) AS Surcharge,

			ISNULL((SELECT L.Sellprice 
			
			FROM Estimate E
			INNER JOIN tblPackage P ON P.idPackage = E.fkidPackage
			INNER JOIN tblLand L ON L.idLand = P.fkidLand
			INNER JOIN tbl_SalesEstimate_EstimateHeader H ON H.fkidEstimate = E.EstimateId
			WHERE H.id_SalesEstimate_EstimateHeader = @revisionId),0) AS LandPrice,
			eh.AppointmentDateTime

			FROM tbl_SalesEstimate_EstimateHeader EH

			INNER JOIN Estimate E 
			ON EH.fkidEstimate = E.EstimateID

			INNER JOIN tblUser U 
			ON EH.fkidOwner = U.userid

			INNER JOIN [State] S
			ON U.fkStateID = S.StateID
			
			INNER JOIN Home H
			ON E.HomeID = H.HomeID

			INNER JOIN tbl_SalesEstimate_RevisionType R 
			ON EH.fkid_SalesEstimate_RevisionType = R.id_SalesEstimate_RevisionType

			WHERE id_SalesEstimate_EstimateHeader = @revisionId
	END	

END

GO
