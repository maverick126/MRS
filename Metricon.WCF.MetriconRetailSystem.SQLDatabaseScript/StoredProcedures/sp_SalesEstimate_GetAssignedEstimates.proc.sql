
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAssignedEstimates]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAssignedEstimates]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAssignedEstimates]
	@revisionTypeId INT,
	@roleId INT, 
	@statusId INT, 
	@userId INT = 0, 
	@regionId INT = 0, 
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
	   
	DECLARE		@groupid INT
	
	SELECT		@groupid=r.MRSGroupID
	FROM		tblsubregion sb
	INNER JOIN  tblSubRegionPriceRegionMapping spm ON sb.idSubRegion=spm.fkidSubRegion
	INNER JOIN  Region r on spm.fkRegionID=r.RegionID
	WHERE		sb.idsubregion=@regionId	
	--SELECT		@groupid=groupid
	--FROM		tblsubregion
	--WHERE		idsubregion=@regionId

	--SELECT	idsubregion	
	--INTO	#temp
	--FROM	tblsubregion
	--WHERE	groupid=@groupid
	
SELECT MAX(EH.id_SalesEstimate_EstimateHeader) AS RevisionId
INTO #temp2
FROM tbl_SalesEstimate_EstimateHeader EH
	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN tblUser U 
	ON E.BCSalesConsultant = U.usercode

	--INNER JOIN tblUserSubRegionMapping M 
	--ON M.fkidUser = U.userid

	LEFT JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity
 
 	LEFT JOIN syn_CRM_New_Contract CT
	ON O.new_contractid = CT.new_contractid
 
	LEFT JOIN tbl_SalesEstimate_CustomerDocument CD ON CD.fkid_SalesEstimate_EstimateHeader = EH.id_SalesEstimate_EstimateHeader AND CD.Active = 1
 
	WHERE
	
	--M.fkidSubRegion IN (SELECT idsubregion FROM tblsubregion WHERE groupid = @groupid) 
	--e.RegionID IN (SELECT regionid FROM region WHERE MRSGroupID = @groupid)
	EH.fkid_salesestimate_MRSGroup=@groupid
	AND
	 
	((@revisionTypeId = 0 
	AND EH.fkid_SalesEstimate_RevisionType IN 
	(SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAccess WHERE fkidRole = @roleId)) OR 
	(@revisionTypeId > 0 AND EH.fkid_SalesEstimate_RevisionType = @revisionTypeId)) 
	
	AND 
	
	((@customerNumber IS NOT NULL AND E.BCCustomerID LIKE '%' + @customerNumber + '%') OR @customerNumber IS NULL)

	AND
	
	((@contractNumber IS NOT NULL AND E.BCContractNumber LIKE '%' + @contractNumber + '%') OR @contractNumber IS NULL)
	
	AND
	
	((@salesConsultantId > 0 AND U.userid = @salesConsultantId) OR @salesConsultantId <= 0)
	
	AND
	
	((@lotNumber IS NOT NULL AND CT.New_LotNumber LIKE '%' + @lotNumber + '%') OR @lotNumber IS NULL)
	
	AND
	
	((@streetName IS NOT NULL AND CT.New_address_street LIKE '%' + @streetName + '%') OR @streetName IS NULL)
	
	AND
	
	((@suburb IS NOT NULL AND CT.New_address_suburb LIKE '%' + @suburb + '%') OR @suburb IS NULL)
	
	
	AND 
	(
	((@statusId = 1 OR @statusId = 2 OR @statusId = 3) AND CT.statuscode <> 2  AND CT.statuscode <> 4) 
	
	OR
	
	(@statusId = 4 AND EH.fkid_SalesEstimate_Status = 1 AND CT.statuscode = 4)
	
	OR
	
	(@statusId = 5 AND EH.fkid_SalesEstimate_Status = 1 AND CT.statuscode = 2)
	)	
	
	GROUP BY EH.fkidEstimate, fkid_SalesEstimate_RevisionType, fkid_SalesEstimate_Status, CD.DocumentType 
	

	SELECT
	A.name AS CustomerName, 
	E.BCCustomerID AS CustomerNumber, 
	E.BCContractNumber AS ContractNumber, 
	E.EstimateID AS EstimateNumber,
	E.fkidopportunity,
	EH.RevisionNumber,
	EH.fkid_SalesEstimate_RevisionType AS RevisionTypeId,
	RT.Abbreviation AS RevisionTypeCode,
	H.HomeName, 
	U.username AS SalesConsultantName, 
	EH.CreatedOn, 
	U2.username AS OwnerName,
	EH.fkidOwner AS OwnerId,
	EH.DueDate,
	EH.AppointmentDateTime,
	D.DifficultyRatingName,
	EH.fkid_SalesEstimate_DifficultyRating AS DifficultyRatingId,
	EH.CommencedOn,
	EH.id_SalesEstimate_EstimateHeader AS EstimateHeaderId,
	EH.fkid_SalesEstimate_Status AS EstimateStatus,
	CT.statuscode AS ContractStatus,
	ISNULL(c.mobilephone,c.telephone2) AS phone,
	O.new_contractid AS ContractID,
	CASE WHEN rt.abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR','RSTM','CON-FIN','PVAR-COL','PVAR-DF','BVAR-BE')
	     THEN CAST(0 AS BIT) 
	     ELSE CAST(1 AS BIT) 
	END AS allowtoaddNSR,
	
	CASE WHEN rt.abbreviation IN ('STS','SE','PVAR-SE','BVAR-BE', 'PSTM-SE'	)
	     THEN CAST(1 AS BIT) 
	     ELSE CAST(0 AS BIT) 
	END AS AllowToAcceptItem,
	
	--CASE WHEN rt.abbreviation IN ('STS','SE','SC')
	--     THEN CAST(1 AS BIT) 
	--     ELSE CAST(0 AS BIT) 
	--END AS validateAccept,
	CAST(1 AS BIT) AS validateAccept,
	
	CASE WHEN rt.Abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR','CON-FIN','PVAR-CSC','PVAR-COL','BVAR-BSC','PSTM-CSC','BVAR-COL','STM')
	     THEN CAST(1 AS BIT) 
	     ELSE CASE WHEN rt.Abbreviation = 'CSC' AND EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = EH.fkidEstimate AND fkid_SalesEstimate_RevisionType = 23) 
	     THEN CAST(1 AS BIT)ELSE CAST(0 AS BIT) END 
	END AS AllowToViewStudioMTab,
	CASE WHEN rt.Abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR','STM')
	     THEN CAST(1 AS BIT) 
         ELSE CAST(0 AS BIT) 
	END AS AllowToViewStudioMDocuSign,		
						
	CASE WHEN rt.abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR','PVAR-COL','BVAR-COL','STM')
	     THEN CAST(1 AS BIT) 
	     ELSE CAST(0 AS BIT) 
	END AS validateStandardInclusion,
	
	CASE WHEN fkid_salesestimate_status in (2,3) OR rt.abbreviation IN ('DF','PVAR-DF','BVAR-DF','CON-FIN') 
	     THEN CAST(1 AS BIT) 
	     ELSE CAST(0 AS BIT) 
	END AS [readonly],	-- accepted or rejected estimates are readonly also drafting version
    ISNULL(eh.ContractType,'PC') AS ContractType,
    
    CASE WHEN DOC.DocumentType IS NULL THEN ''
    WHEN DOC.DocumentType = 'Contract' THEN 'HIA Contract'
    ELSE DOC.DocumentType + ' ' + CAST(DOC.DocumentNumber AS VARCHAR(10)) 
    END AS DocumentType,
    E.fkidAccount AS accountid,
    ms.mrsgroupname as MRSGroup
    
	FROM tbl_SalesEstimate_EstimateHeader EH

	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN tblUser U 
	ON E.BCSalesConsultant = U.usercode

	INNER JOIN tblUser U2 
	ON EH.fkidOwner = U2.userid

	INNER JOIN Home H 
	ON H.HomeID = E.HomeID
	
	LEFT JOIN tbl_SalesEstimate_DifficultyRating D 
	ON D.id_SalesEstimate_DifficultyRating = EH.fkid_SalesEstimate_DifficultyRating

	LEFT JOIN syn_Crm_Account A
	ON A.AccountId = E.fkidAccount

	LEFT JOIN syn_Crm_Contact c
	ON a.primarycontactid = c.contactid

	LEFT JOIN syn_Crm_Opportunity O
	ON O.OpportunityId = E.fkidOpportunity

	LEFT JOIN syn_Crm_New_Contract CT
	ON O.new_contractid = CT.new_contractid

	INNER JOIN tbl_SalesEstimate_RevisionType RT
	ON EH.fkid_SalesEstimate_RevisionType = RT.id_SalesEstimate_RevisionType
	
	LEFT JOIN tbl_SalesEstimate_CustomerDocument DOC
	ON EH.id_SalesEstimate_EstimateHeader = DOC.fkid_SalesEstimate_EstimateHeader AND DOC.Active = 1
	
	INNER JOIN tbl_SalesEstimate_MRSGroup ms
	ON EH.fkid_SalesEstimate_MRSGroup=ms.mrsgroupid
 
	WHERE EH.id_SalesEstimate_EstimateHeader IN (SELECT * FROM #temp2)
	
	AND
	
	((@userId > 0 AND (EH.fkidOwner = @userId OR RT.abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR'))) OR @userId = 0)
	
	AND 
	
	(((@statusId = 1 OR @statusId = 2 OR @statusId = 3) AND EH.fkid_SalesEstimate_Status = @statusId) OR (@statusId > 3 AND EH.fkid_SalesEstimate_Status = 1))
	
	ORDER BY ISNULL(EH.DueDate,'9999-01-01'), EH.CreatedOn DESC

END

GO