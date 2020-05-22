
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateCount]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateCount]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateCount]
	@userId INT,
	@roleId INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

DECLARE @QueueCount INT
DECLARE @WIPCount INT
DECLARE @AcceptedCount INT
DECLARE @RejectedCount INT
DECLARE @OnHoldCount INT
DECLARE @CancelledCount INT
DECLARE @AppointmentCount INT
DECLARE @IsManager bit

SELECT @IsManager = IsManager from tblRole WHERE IdRole = @roleId

DECLARE @groupid INT
SELECT @groupid = r.MRSGroupID
FROM (SELECT TOP 1 fkidSubRegion FROM tblUserSubRegionMapping WHERE fkidUser = @userId) ur
INNER JOIN tblSubRegionPriceRegionMapping spm ON ur.fkidSubRegion=spm.fkidSubRegion
INNER JOIN Region r ON spm.fkRegionID=r.RegionID


SELECT regionid	
INTO #temp
FROM region
WHERE MRSGroupID = @groupid

SELECT @QueueCount = COUNT (*) FROM tbl_SalesEstimate_Queue Q
	INNER JOIN Estimate E 
	ON Q.fkidEstimate = E.EstimateID

	INNER JOIN tblUser U 
	ON E.BCSalesConsultant = U.usercode

	--INNER JOIN tblUserSubRegionMapping M 
	--ON M.fkidUser = U.userid 

	WHERE --M.fkidSubRegion IN (SELECT idsubregion FROM #temp) AND 
	E.RegionID in (SELECT RegionID FROM #temp) AND
	Q.fkid_SalesEstimate_RevisionType IN 
	(SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAccess
	WHERE fkidRole = @roleId)


SELECT MAX(RevisionNumber) AS RevisionNumber, fkidEstimate, fkid_SalesEstimate_RevisionType
INTO #temp2
FROM tbl_SalesEstimate_EstimateHeader EH
	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN tblUser U 
	ON E.BCSalesConsultant = U.usercode

	INNER JOIN tblUser U2 
	ON EH.fkidOwner = U2.userid

	INNER JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity

	INNER JOIN syn_CRM_New_Contract CT
	ON O.new_contractid = CT.new_contractid 

	--INNER JOIN tblUserSubRegionMapping M 
	--ON M.fkidUser = U.userid

	INNER JOIN tbl_SalesEstimate_RevisionType RT
	ON EH.fkid_SalesEstimate_RevisionType = RT.id_SalesEstimate_RevisionType
 
	WHERE fkid_SalesEstimate_RevisionType IN 
	(SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_RevisionTypeAccess WHERE fkidRole = @roleId)
    AND EH.fkid_salesestimate_MRSGroup in (SELECT RegionID FROM #temp)
	--AND E.RegionID in (SELECT RegionID FROM #temp)--M.fkidSubRegion IN (SELECT idsubregion FROM #temp)
		
	GROUP BY fkidEstimate, fkid_SalesEstimate_RevisionType

SELECT @AppointmentCount = COUNT (*) FROM #temp2 t 
	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON t.RevisionNumber = EH.RevisionNumber AND t.fkidEstimate = EH.fkidEstimate 

	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity

	INNER JOIN syn_CRM_New_Contract CT
	ON O.new_contractid = CT.new_contractid 

	WHERE EH.fkid_SalesEstimate_Status = 1 -- Work In Progress
	AND CT.statuscode <> 2 -- NOT Cancelled
	AND CT.statuscode <> 4 -- NOT On Hold
	AND ((EH.fkidOwner = @userId AND @IsManager = 0) OR @IsManager = 1)
	AND EH.AppointmentDateTime >= CONVERT(VARCHAR(10), getdate(), 120)
	AND  EH.AppointmentDateTime < CONVERT(VARCHAR(10), getdate() + 1, 120)
	AND EH.fkid_salesestimate_MRSGroup in (SELECT RegionID FROM #temp)
	--AND E.RegionID in (SELECT RegionID FROM #temp)
	--AND C.cle_event = 2800 -- Specification Selection
	--AND C.cle_regdate = CONVERT(VARCHAR(10), getdate(), 120) -- today appointments

SELECT @WIPCount = COUNT (*) FROM #temp2 t 
	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON t.RevisionNumber = EH.RevisionNumber AND t.fkidEstimate = EH.fkidEstimate 

	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity

	INNER JOIN syn_CRM_New_Contract CT
	ON O.new_contractid = CT.new_contractid 

	WHERE EH.fkid_SalesEstimate_Status = 1 -- Work In Progress
	AND CT.statuscode <> 2 -- NOT Cancelled
	AND CT.statuscode <> 4 -- NOT On Hold
	AND ((EH.fkidOwner = @userId AND @IsManager = 0) OR @IsManager = 1)
	AND EH.fkid_salesestimate_MRSGroup in (SELECT RegionID FROM #temp)
	--AND E.RegionID in (SELECT RegionID FROM #temp)
		
SELECT @AcceptedCount = COUNT (*) FROM #temp2 t 
	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON t.RevisionNumber = EH.RevisionNumber AND t.fkidEstimate = EH.fkidEstimate 
	WHERE EH.fkid_SalesEstimate_Status = 2 -- Accepted
	AND ((EH.fkidOwner = @userId AND @IsManager = 0) OR @IsManager = 1)

SELECT @RejectedCount = COUNT (*) FROM #temp2 t 
	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON t.RevisionNumber = EH.RevisionNumber AND t.fkidEstimate = EH.fkidEstimate 
	WHERE EH.fkid_SalesEstimate_Status = 3 -- Rejected
	AND ((EH.fkidOwner = @userId AND @IsManager = 0) OR @IsManager = 1)

SELECT @OnHoldCount = COUNT (*) FROM #temp2 t 
	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON t.RevisionNumber = EH.RevisionNumber AND t.fkidEstimate = EH.fkidEstimate 

	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity

	INNER JOIN syn_CRM_New_Contract CT
	ON O.new_contractid = CT.new_contractid 

	WHERE EH.fkid_SalesEstimate_Status = 1 -- Work In Progress
	AND CT.statuscode = 4  -- On Hold
	AND ((EH.fkidOwner = @userId AND @IsManager = 0) OR @IsManager = 1)
	AND EH.fkid_salesestimate_MRSGroup in (SELECT RegionID FROM #temp)
	--AND E.RegionID in (SELECT RegionID FROM #temp)


SELECT @CancelledCount = COUNT (*) FROM #temp2 t 
	INNER JOIN tbl_SalesEstimate_EstimateHeader EH
	ON t.RevisionNumber = EH.RevisionNumber AND t.fkidEstimate = EH.fkidEstimate 

	INNER JOIN Estimate E 
	ON EH.fkidEstimate = E.EstimateID

	INNER JOIN syn_CRM_Opportunity O 
	ON O.OpportunityId = E.fkidOpportunity

	INNER JOIN syn_CRM_New_Contract CT
	ON O.new_contractid = CT.new_contractid 

	WHERE EH.fkid_SalesEstimate_Status = 1 -- Work In Progress
	AND CT.statuscode = 2 -- Cancelled
	AND ((EH.fkidOwner = @userId AND @IsManager = 0) OR @IsManager = 1)
	AND EH.fkid_salesestimate_MRSGroup in (SELECT RegionID FROM #temp)
	--AND E.RegionID in (SELECT RegionID FROM #temp)

SELECT @QueueCount AS QueueCount, 
@WIPCount AS WIPCount, 
@AcceptedCount AS AcceptedCount, 
@RejectedCount AS RejectedCount,
@OnHoldCount AS OnHoldCount,
@CancelledCount AS CancelledCount,
@AppointmentCount AS AppointmentCount

END

GO