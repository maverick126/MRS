/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the problem to drop the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAuditTrail]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAuditTrail]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAuditTrail]
	@estimateId int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @contractnumber VARCHAR(10), @oldregion VARCHAR(20), @newregion VARCHAR(20), @mindate DATETIME
	SELECT @contractnumber= BCContractNumber, @oldregion=mr.MRSGroupName
	FROM Estimate e
	INNER JOIN Region r ON e.RegionID=r.RegionID
	INNER JOIN tbl_SalesEstimate_MRSGroup mr ON r.MRSGroupID=mr.MRSGroupID
	WHERE EstimateID=@estimateId
	
    SELECT @newregion=mr.MRSGroupName
    FROM tbl_SalesEstimate_EstimateHeader eh
    INNER JOIN tbl_SalesEstimate_MRSGroup mr ON eh.fkid_salesestimate_MRSGroup=mr.MRSGroupID
    WHERE fkidEstimate=@estimateId AND fkid_SalesEstimate_RevisionType=2
-- this @mindate is used to identity which one is the first assign. 
--if there is a cross region assign, only this first one is show change region message
	SELECT @mindate=MIN(L.createdon)
	FROM syn_CRM_New_MRSLog L
    INNER JOIN syn_crm_new_contract cc  ON l.new_contractid=cc.new_contractid
	INNER JOIN syn_CRM_Contact C    	ON C.contactid = L.new_contactid
    WHERE cc.new_name = @contractnumber	
    GROUP BY cc.new_name
    
	SELECT  
	L.New_mrslogId AS LogId, 
	L.New_Action AS Action, 
	L.New_EstimateNumber AS EstimateNumber, 
	L.New_RevisionNumber AS RevisionNumber, 
	L.New_RevisionType AS RevisionType, 
	C.FullName AS [User],
	CASE WHEN L.New_RevisionNumber=2 AND L.New_Action LIKE '%assign%' AND @oldregion<>@newregion AND L.createdon=@mindate
	     THEN CASE WHEN L.New_ExtraDescription IS NULL
	               THEN 'Processing region changed from '+@oldregion+' to '+@newregion
	               ELSE L.New_ExtraDescription+CHAR(10)+'Processing region changed from '+@oldregion+' to '+@newregion
	           END
	     ELSE ISNULL(L.New_ExtraDescription,'') 
	END AS Description,
	L.CreatedOn AS LogTime

	FROM syn_CRM_New_MRSLog L
    INNER JOIN syn_crm_new_contract cc  ON l.new_contractid=cc.new_contractid
	INNER JOIN syn_CRM_Contact C    	ON C.contactid = L.new_contactid

    WHERE cc.new_name = @contractnumber
	--WHERE L.New_EstimateNumber = @estimateId

	ORDER BY L.CreatedOn

END
GO