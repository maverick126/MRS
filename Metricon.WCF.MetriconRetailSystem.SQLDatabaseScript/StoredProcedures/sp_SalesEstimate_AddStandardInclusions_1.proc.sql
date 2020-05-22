
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_AddStandardInclusions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_AddStandardInclusions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_AddStandardInclusions] 
	@previousEstimateRevisionId INT,
	@newEstimateRevisionId INT,
	@createdbyId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @regionGroupId INT
	DECLARE @brandId INT, @homeid INT, @estimateid INT
	DECLARE @homestories INT

	SELECT @regionGroupId = tblPriceRegionGroupMapping.fkidRegionGroup, 
	@brandId = Home.BrandID , 
	@homeid = Estimate.homeid, 
	@estimateid = Estimate.EstimateID,
	@homestories = Home.Stories
	FROM tbl_SalesEstimate_EstimateHeader  
	INNER JOIN Estimate ON tbl_SalesEstimate_EstimateHeader.fkidEstimate = Estimate.EstimateID
	INNER JOIN Home ON Estimate.HomeID = Home.HomeID
	INNER JOIN tblPriceRegionGroupMapping ON tblPriceRegionGroupMapping.fkRegionID = Estimate.RegionID
	WHERE id_SalesEstimate_EstimateHeader = @previousEstimateRevisionId 

	----Insert Standard Inclusions that have not been added to the Estimate
	--SELECT * INTO #temp FROM tblStandardInclusions 
	--WHERE BrandID = @brandId AND 
	--fkidRegionGroup = @regionGroupId AND 
	--idStandardInclusions NOT IN 
	--(SELECT ISNULL(fkidStandardInclusions,0) FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId)

	----Delete Standard Inclusions that have already been upgraded
	--DELETE FROM #temp WHERE ProductAreaGroupID IN (    
	----This SELECT returns all Standard Inclusion PAGs for a Brand that an Upgrade Option is already in Estimate Revision Details
	--SELECT PAG1.ProductAreaGroupID FROM tbl_StudioM_InclusionValidationRule 
	--	INNER JOIN Product P1 ON tbl_StudioM_InclusionValidationRule.fkidInclusionProduct = P1.ProductID
	--	INNER JOIN ProductAreaGroup PAG1 ON PAG1.ProductID = P1.ProductID
	--	INNER JOIN Product P2 ON tbl_StudioM_InclusionValidationRule.fkidUpgradeProduct = P2.ProductID
	--	INNER JOIN ProductAreaGroup PAG2 ON PAG2.ProductID = P2.ProductID 
	--	AND PAG1.AreaID = PAG2.AreaID AND PAG1.GroupID = PAG2.GroupID 
	--WHERE fkidBrand = @brandId AND 
	--PAG2.ProductAreaGroupID IN (
	----This SELECT returns all PAGs in Estimate Revision Details
	--SELECT ProductAreaGroupID FROM tbl_SalesEstimate_EstimateDetails INNER JOIN EstimateDetails 
	--ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID
	--WHERE tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId))
	
	---- delete all pags which not configured to the home in HDO
	
	--DELETE FROM #temp WHERE ProductAreaGroupid NOT IN
	--(
	--  select ProductAreaGroupID from HomeDisplayOption where HomeID=@homeid and HomeDisplayID is null and Active=1
	--)
	
	-- get SI from originate estimatedetails
	
	SELECT ed.*,pag.IsSiteWork INTO #temp 
	FROM     EstimateDetails ed
	INNER JOIN ProductAreaGroup pag ON ed.ProductAreaGroupID=pag.ProductAreaGroupID
	WHERE    EstimateID=@estimateid AND StandardInclusion=1 
	AND EstimateDetailsID NOT IN (SELECT fkidEstimateDetails FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId)
	
	--Minimum Areas for the home
	SELECT HomeStandardArea.AreaID INTO #tempHSA 
	FROM	 HomeStandardArea
	WHERE    HomeID=@homeid AND Active=1
	
	--Areas that have been added (SI)
	INSERT INTO #tempHSA SELECT DISTINCT areaid FROM tbl_SalesEstimate_EstimateDetails
	INNER JOIN EstimateDetails ON fkidEstimateDetails = EstimateDetailsID
	WHERE fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId AND areaid NOT IN (SELECT areaid FROM #tempHSA)

	--Areas that have been added (NSR)
	INSERT INTO #tempHSA SELECT DISTINCT fkid_NonStandardArea FROM tbl_SalesEstimate_EstimateDetails
	WHERE fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId 
	AND fkid_NonStandardArea IS NOT NULL
	AND fkid_NonStandardArea NOT IN (SELECT areaid FROM #tempHSA)

	--Related Areas
	INSERT INTO #tempHSA SELECT DISTINCT tbl_SalesEstimate_RelatedArea.AreaID FROM tbl_SalesEstimate_EstimateDetails
	INNER JOIN EstimateDetails ON fkidEstimateDetails = EstimateDetailsID
	INNER JOIN tbl_SalesEstimate_RelatedArea ON  EstimateDetails.ProductID = tbl_SalesEstimate_RelatedArea.ProductID
	WHERE fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId 
	AND tbl_SalesEstimate_RelatedArea.AreaID NOT IN (SELECT areaid FROM #tempHSA)
	AND tbl_SalesEstimate_RelatedArea.Active = 1
	AND tbl_SalesEstimate_RelatedArea.ValidateInStudioM=1
	
	-- excluded area by related area
    SELECT DISTINCT tbl_SalesEstimate_RelatedArea.AreaID 
    INTO #tempexcludedHSA
    FROM tbl_SalesEstimate_EstimateDetails
	INNER JOIN EstimateDetails ON fkidEstimateDetails = EstimateDetailsID
	INNER JOIN tbl_SalesEstimate_RelatedArea ON  EstimateDetails.ProductID = tbl_SalesEstimate_RelatedArea.ProductID
	WHERE fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId 
	AND tbl_SalesEstimate_RelatedArea.Active = 1
	AND tbl_SalesEstimate_RelatedArea.ValidateInStudioM=0
	
	DELETE FROM #tempHSA
	WHERE areaid IN (SELECT areaid FROM #tempexcludedHSA)

	
	SELECT t1.* INTO #tempSI
	FROM #temp t1
	INNER JOIN #tempHSA hsa ON t1.areaid=hsa.areaid

	----Delete Standard Inclusions that have already been upgraded
	DELETE FROM #tempSI WHERE ProductAreaGroupID IN (    
	--This SELECT returns all Standard Inclusion PAGs for a Brand that an Upgrade Option is already in Estimate Revision Details
	SELECT PAG1.ProductAreaGroupID FROM tbl_StudioM_InclusionValidationRule 
		INNER JOIN Product P1 ON tbl_StudioM_InclusionValidationRule.fkidInclusionProduct = P1.ProductID
		INNER JOIN ProductAreaGroup PAG1 ON PAG1.ProductID = P1.ProductID
		INNER JOIN Product P2 ON tbl_StudioM_InclusionValidationRule.fkidUpgradeProduct = P2.ProductID
		INNER JOIN ProductAreaGroup PAG2 ON PAG2.ProductID = P2.ProductID 
		AND PAG1.AreaID = PAG2.AreaID --AND PAG1.GroupID = PAG2.GroupID 
	WHERE fkidBrand = @brandId AND 
	PAG2.ProductAreaGroupID IN (
	--This SELECT returns all PAGs in Estimate Revision Details
	SELECT ProductAreaGroupID FROM tbl_SalesEstimate_EstimateDetails INNER JOIN EstimateDetails 
	ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID
	WHERE tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = @previousEstimateRevisionId))


	--INSERT Standard Inclusions that are left in #temp into Estimate Revision Details
	INSERT INTO tbl_SalesEstimate_EstimateDetails 
	(fkid_SalesEstimate_EstimateHeader, 
	fkidEstimateDetails,
	fkidStandardInclusions,
	ItemPrice, 
	Quantity, 
	ProductDescription, 
	InternalDescription,
	AdditionalInfo,	
	CreatedOn, 
	CreatedBy, 
	ModifiedOn, 
	ModifiedBy,
	IsSiteWork, 
	SelectedImageID,
	fkid_NonStandardPriceDisplayCode,
	DerivedCost,
	CostExcGST,
	CostOverWriteBy,	
	AreaSortOrder, 
	GroupSortOrder, 
	ProductSortOrder)
		
	SELECT @newEstimateRevisionId AS fkid_SalesEstimate_EstimateHeader,
	#tempSI.estimatedetailsid,
	0, 
	0, 
	#tempSI.Quantity, 
	#tempSI.ProductDescription,
	#tempSI.InternalDescription,
	#tempSI.AdditionalInfo,
	GETDATE(), 
	@createdbyId, 
	GETDATE(), 
	@createdbyId,
	pag.IsSiteWork,
	Null,
	NULL,
	1,
	0,
	NULL,
	CASE WHEN @homestories = 1 THEN a.sortorder ELSE a.sortorderdouble END,
	g.SortOrder,
	p.SortOrder	
	FROM #tempSI
	INNER JOIN Area a ON a.AreaID = #tempSI.areaid
	INNER JOIN [Group] g ON g.GroupID = #tempSI.groupid
	INNER JOIN Product p ON p.ProductID = #tempSI.ProductID
	INNER JOIN ProductAreaGroup pag ON pag.ProductAreaGroupID = #tempSI.ProductAreaGroupID
	--INNER JOIN Product ON ProductAreaGroup.ProductID = Product.ProductID

	DROP TABLE #temp
	DROP TABLE #tempHSA
	DROP TABLE #tempSI	

	SET NOCOUNT OFF;
END


GO