----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_ValidateStudioMRevisions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_ValidateStudioMRevisions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_ValidateStudioMRevisions]
	@estimateRevisionId INT
AS
BEGIN
	-- Validate all Studio M revisions before merging into STM
	SET NOCOUNT ON;

	DECLARE @estimateId INT
	DECLARE @mrsGroupId INT

	-- Select the Base Revision of the Sales Estimate 
	SELECT @estimateId = SE.fkidEstimate, @mrsGroupId = R.MRSGroupID
	FROM tbl_SalesEstimate_EstimateHeader SE 
	INNER JOIN Estimate E ON SE.fkidEstimate = E.EstimateID 
	INNER JOIN Region R ON E.RegionID = R.RegionID
	WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
	SELECT AreaID INTO #StudioMAreas FROM (
	--Non Colour Selection

	SELECT DISTINCT fkidArea AS AreaID  FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidArea IS NOT NULL 
	AND 
	(
		fkid_SalesEstimate_RevisionType IN 
		(
			SELECT DISTINCT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId
		)
	)
	UNION
	--Colour Selection (WHEN Colour Selection Revision Exists)
	SELECT AreaID FROM [Area] WHERE 
	(
	AreaID NOT IN 
	(
		SELECT fkidArea FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidArea IS NOT NULL
	)
	OR AreaID IN 
	(
		SELECT fkidArea FROM tbl_SalesEstimate_RevisionTypeAreaGroup AG INNER JOIN tbl_SalesEstimate_RevisionType RT
		ON AG.fkid_SalesEstimate_RevisionType = RT.id_SalesEstimate_RevisionType 
		WHERE (ExcMRSGroupIDWhenSplit = CAST(@mrsGroupId AS VARCHAR(255))
		OR ExcMRSGroupIDWhenSplit LIKE '%'+CAST(@mrsGroupId AS VARCHAR(255))+',%'		
		OR ExcMRSGroupIDWhenSplit LIKE '%,'+CAST(@mrsGroupId AS VARCHAR(255))+',%'
		OR ExcMRSGroupIDWhenSplit LIKE '%,'+CAST(@mrsGroupid AS VARCHAR(255)))
		AND fkidArea IS NOT NULL
	)
	)
	AND EXISTS 
	(
		SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType IN 
		(
			SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE ExcludeDefinedAreaGroup = 1
		)
	)
	)StudioMAreas			


	SELECT GroupID INTO #StudioMGroups FROM (
	--Non Colour Selection
	SELECT DISTINCT fkidGroup AS GroupID  FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidGroup IS NOT NULL
	AND 
	(
		fkid_SalesEstimate_RevisionType IN 
		(
			SELECT DISTINCT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId
		)
	)

	UNION

	--Colour Selection (WHEN Colour Selection Revision Exists)
	SELECT GroupID FROM [Group] WHERE 
	(
	GroupID NOT IN 
	(
		SELECT fkidGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE Active = 1 AND fkidGroup IS NOT NULL
	)
	OR GroupID IN 
	(
		SELECT fkidGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup AG INNER JOIN tbl_SalesEstimate_RevisionType RT
		ON AG.fkid_SalesEstimate_RevisionType = RT.id_SalesEstimate_RevisionType 
		WHERE (ExcMRSGroupIDWhenSplit = CAST(@mrsGroupId AS VARCHAR(255))
		OR ExcMRSGroupIDWhenSplit LIKE '%'+CAST(@mrsGroupId AS VARCHAR(255))+',%'		
		OR ExcMRSGroupIDWhenSplit LIKE '%,'+CAST(@mrsGroupId AS VARCHAR(255))+',%'
		OR ExcMRSGroupIDWhenSplit LIKE '%,'+CAST(@mrsGroupid AS VARCHAR(255)))
		AND fkidGroup IS NOT NULL
	)
	)
	AND EXISTS 
	(
		SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType IN 
		(
			SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE ExcludeDefinedAreaGroup = 1
		)
	)	
	)StudioMGroups


	--SELECT Non-Studio M Standard Options or Studio M Standard Options but no Studio M Revision from the Ready for Studio M  
	SELECT 
	Product.ProductID,
	EstimateDetails.AreaName,
	EstimateDetails.GroupName
	
	INTO #TempProduct
	
	FROM tbl_SalesEstimate_EstimateDetails 

	INNER JOIN tbl_SalesEstimate_EstimateHeader 
	ON tbl_SalesEstimate_EstimateDetails.fkid_SalesEstimate_EstimateHeader = tbl_SalesEstimate_EstimateHeader.id_SalesEstimate_EstimateHeader

	INNER JOIN EstimateDetails 
	ON tbl_SalesEstimate_EstimateDetails.fkidEstimateDetails = EstimateDetails.EstimateDetailsID

	INNER JOIN Product 
	ON EstimateDetails.ProductID = Product.ProductID

	WHERE fkidEstimate = @estimateId 

	AND fkid_SalesEstimate_RevisionType = 6 --Ready for Studio M

	AND AreaId <> 43 --Non Standard Request

	AND 
	(
		Product.isStudioMProduct = 1 --Product is Studio M Product
		AND 
		(
			EstimateDetails.AreaId NOT IN --Area is NOT in Areas that are specified in existing Studio M Revisions
			( 
				SELECT AreaId FROM #StudioMAreas
			)
			OR
			EstimateDetails.GroupId NOT IN
			(
				SELECT GroupId FROM #StudioMGroups
			)				
		)
		AND
		CAST(Product.StudioMQAndA AS VARCHAR(MAX)) LIKE '%mandatory="1"%'
		
		AND 
		(tbl_SalesEstimate_EstimateDetails.StudioMAttributes IS NULL OR CAST(StudioMAttributes AS VARCHAR(MAX)) = '')	
	)

	SELECT * FROM #TempProduct

	DROP TABLE #StudioMAreas
	DROP TABLE #StudioMGroups
	DROP TABLE #TempProduct
	
	SET NOCOUNT OFF;

END

GO