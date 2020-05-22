/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateVariationForEventIntegration]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateVariationForEventIntegration]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateVariationForEventIntegration]
@estimateRevisionId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @newRevisionTable TABLE (UpgradePrice DECIMAL, HomePrice DECIMAL) 
	DECLARE @oldRevisionTable TABLE (UpgradePrice DECIMAL, HomePrice DECIMAL)
	DECLARE @MRSGroupId INT, @estimateId INT
	
	INSERT INTO @newRevisionTable
	EXEC sp_SalesEstimate_GetEstimateAmount @estimateRevisionId
	
	DECLARE @previousrevisionid INT
	EXEC sp_SalesEstimate_GetPreviousRevisonForVariation @estimateRevisionId, @previousrevisionid OUTPUT
	
	INSERT INTO @oldRevisionTable
	EXEC sp_SalesEstimate_GetEstimateAmount @previousrevisionid
	
	SELECT @MRSGroupId = R.MRSGroupID, @estimateId = SEH.fkidEstimate FROM tbl_SalesEstimate_EstimateHeader SEH 
	INNER JOIN Estimate E ON SEH.fkidEstimate = E.EstimateID
	INNER JOIN Region R ON E.RegionID = R.RegionID 
	WHERE SEH.id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
	DECLARE @newPrice DECIMAL, @oldPrice DECIMAL, @variationAmount DECIMAL
	SELECT @newPrice = (UpgradePrice + HomePrice) FROM @newRevisionTable 
	SELECT @oldPrice = (UpgradePrice + HomePrice) FROM @oldRevisionTable
	SET @variationAmount = @newPrice - @oldPrice
	
	DECLARE @variationStartsAtRevisionId INT
	
	IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader > @previousrevisionid AND 
	id_SalesEstimate_EstimateHeader < @estimateRevisionId AND fkidEstimate = @estimateId AND fkid_SalesEstimate_Status = 3)
	BEGIN
		SET @variationStartsAtRevisionId = (SELECT MAX(id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader > @previousrevisionid AND 
	id_SalesEstimate_EstimateHeader < @estimateRevisionId AND fkidEstimate = @estimateId AND fkid_SalesEstimate_Status = 3)
	END
	ELSE
	BEGIN
		SET @variationStartsAtRevisionId = @previousrevisionid
	END
	
	DECLARE @salesEstimator VARCHAR(10)
	
	IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader > @variationStartsAtRevisionId AND 
	id_SalesEstimate_EstimateHeader < @estimateRevisionId AND fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType IN (15,19,25))
	BEGIN
		SET @salesEstimator = (SELECT TOP 1 U.usercode FROM tbl_SalesEstimate_EstimateHeader SEH INNER JOIN tbluser U ON SEH.fkidOwner = U.userid
		WHERE id_SalesEstimate_EstimateHeader > @variationStartsAtRevisionId AND id_SalesEstimate_EstimateHeader < @estimateRevisionId AND 
		fkidEstimate = @estimateId AND fkid_SalesEstimate_RevisionType IN (15,19,25) ORDER BY id_SalesEstimate_EstimateHeader DESC)
	END 
	ELSE
	BEGIN
		SET @salesEstimator = (SELECT U.usercode FROM tbl_SalesEstimate_EstimateHeader SEH INNER JOIN tbluser U ON SEH.fkidOwner = U.userid
		WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId)
	END
	
	SELECT 
	'MRS-' + Convert(VARCHAR(10),RevisionNumber) AS Reference,
	CASE WHEN (@MRSGroupId = 23 AND Doc.[Description] IS NOT NULL) THEN Doc.[Description] --Queensland
	ELSE
		CASE WHEN Hdr.fkid_SalesEstimate_RevisionType = 18 THEN 'Building Variation ' + Convert(VARCHAR(10),Doc.DocumentNumber)
		ELSE 'Variation ' + Convert(VARCHAR(10),Doc.DocumentNumber) END 
	END AS [Description],
	CASE WHEN Hdr.fkid_SalesEstimate_RevisionType = 18 THEN 'B'
	ELSE 'V' END AS VariationType,	
	@variationAmount AS VariationAmount,
	Doc.DocumentNumber AS DocumentNumber,
	@MRSGroupId AS MRSGroupId,
	@salesEstimator AS SalesEstimator
	FROM tbl_SalesEstimate_EstimateHeader Hdr INNER JOIN tbl_SalesEstimate_CustomerDocument Doc 
	ON Hdr.id_SalesEstimate_EstimateHeader = Doc.fkid_SalesEstimate_EstimateHeader AND Doc.Active = 1
	WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
END

GO
