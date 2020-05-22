
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetQueuedEstimates]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetQueuedEstimates]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetQueuedEstimates]
	@revisionTypeId int, 
	@regionId int,
	@roleId int,
	@customerNumber nvarchar(50) = NULL, 
	@contractNumber nvarchar(50) = NULL, 
	@salesConsultantId INT = 0,
	@lotNumber nvarchar(50) = NULL, 
	@streetName nvarchar(50) = NULL, 
	@suburb nvarchar(50) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE		@groupid	INT
	--SELECT		@groupid=groupid
	--FROM		tblsubregion
	--WHERE		idsubregion=@regionId

	SELECT		@groupid=r.MRSGroupID
	FROM		tblsubregion sb
	INNER JOIN  tblSubRegionPriceRegionMapping spm ON sb.idSubRegion=spm.fkidSubRegion
	INNER JOIN  Region r on spm.fkRegionID=r.RegionID
	WHERE		sb.idsubregion=@regionId
		
	SELECT
	
	MAX(EH.id_SalesEstimate_EstimateHeader) AS RevisionId
	
	INTO #temp
	
	FROM tbl_SalesEstimate_Queue Q

	INNER JOIN Estimate E 
	ON Q.fkidEstimate = E.EstimateID

	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON EH.fkidEstimate = Q.fkidEstimate

	INNER JOIN tblUser U 
	ON E.BCSalesConsultant = U.usercode

	--INNER JOIN tblUserSubRegionMapping M 
	--ON M.fkidUser = U.userid

	LEFT JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity

	WHERE 
	
	--M.fkidSubRegion IN (SELECT idsubregion FROM tblsubregion WHERE groupid = @groupid) 
	--e.RegionID IN (SELECT regionid FROM region WHERE MRSGroupID = @groupid) 
	q.fkid_salesestimate_MRSGroup=@groupid
	AND
	 
	((@revisionTypeId = 0 
	AND Q.fkid_SalesEstimate_RevisionType IN 
	(SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAccess WHERE fkidRole = @roleId)) OR 
	(@revisionTypeId > 0 AND Q.fkid_SalesEstimate_RevisionType = @revisionTypeId)) 
	
	AND 
	
	((@customerNumber IS NOT NULL AND E.BCCustomerID LIKE '%' + @customerNumber + '%') OR @customerNumber IS NULL)

	AND
	
	((@contractNumber IS NOT NULL AND E.BCContractNumber LIKE '%' + @contractNumber + '%') OR @contractNumber IS NULL)
	
	AND
	
	((@salesConsultantId > 0 AND U.userid = @salesConsultantId) OR @salesConsultantId <= 0)
	
	AND
	
	((@lotNumber IS NOT NULL AND O.New_LotNumber LIKE '%' + @lotNumber + '%') OR @lotNumber IS NULL)
	
	AND
	
	((@streetName IS NOT NULL AND O.New_address_street LIKE '%' + @streetName + '%') OR @streetName IS NULL)
	
	AND
	
	((@suburb IS NOT NULL AND O.New_address_suburb LIKE '%' + @suburb + '%') OR @suburb IS NULL)
	
	GROUP BY EH.fkidEstimate



	SELECT
	A.name AS CustomerName, 
	E.BCCustomerID AS CustomerNumber, 
	E.BCContractNumber AS ContractNumber, 
	E.EstimateID AS EstimateNumber,
	EH.RevisionNumber,
	Q.fkid_SalesEstimate_RevisionType AS RevisionTypeId,
	RT.Abbreviation AS RevisionTypeCode, 
	H.HomeName, 
	U.username AS SalesConsultantName, 
	Q.CreatedOn,
	'-' AS OwnerName, 
	EH.fkidOwner AS OwnerId,
	Q.DueDate,
	D.DifficultyRatingName,
	Q.fkid_SalesEstimate_DifficultyRating AS DifficultyRatingId,
	CT.statuscode AS ContractStatus,
	Q.id_SalesEstimate_Queue AS QueueId,
	O.new_contractid AS contractID,
	EH.id_SalesEstimate_EstimateHeader AS PreviousRevisionId,	
	ISNULL(q.ContractType,'PC') AS ContractType
	FROM tbl_SalesEstimate_Queue Q

	INNER JOIN Estimate E 
	ON Q.fkidEstimate = E.EstimateID

	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON EH.fkidEstimate = Q.fkidEstimate

	INNER JOIN tblUser U 
	ON E.BCSalesConsultant = U.usercode

	INNER JOIN Home H 
	ON H.HomeID = E.HomeID

	LEFT JOIN tbl_SalesEstimate_DifficultyRating D 
	ON D.id_SalesEstimate_DifficultyRating = Q.fkid_SalesEstimate_DifficultyRating

	LEFT JOIN syn_CRM_Account A 
	ON A.AccountId = E.fkidAccount

	LEFT JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity

	LEFT JOIN syn_CRM_New_Contract CT
	ON O.new_contractid = CT.new_contractid 

	INNER JOIN tbl_SalesEstimate_RevisionType RT
	ON Q.fkid_SalesEstimate_RevisionType = RT.id_SalesEstimate_RevisionType

	WHERE EH.id_SalesEstimate_EstimateHeader IN (SELECT * FROM #temp) 
	
	ORDER BY ISNULL(Q.DueDate,'9999-01-01'), Q.CreatedOn DESC
	

END

GO