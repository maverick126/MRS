
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetContractEventFromRevisonID]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetContractEventFromRevisonID]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <07/01/2016>
-- Description:	<get contract and event from revision>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetContractEventFromRevisonID] 
@revisionid INT
AS
BEGIN
 
	SET NOCOUNT ON;

      SELECT e.BCContractNumber AS contractnumber,
             rm.EventCode AS eventcode
      FROM tbl_SalesEstimate_EstimateHeader eh
      INNER JOIN tbl_SalesEstimate_MRSGroup mg ON eh.fkid_salesestimate_MRSGroup=mg.MRSGroupID
      INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
      INNER JOIN tbl_SalesEstimate_RevisionTypeBCEventMapping rm ON eh.fkid_SalesEstimate_RevisionType=rm.fkid_SalesEstimate_RevisionType AND mg.MRSGroupID=rm.fkidMRSGroup
      WHERE eh.id_SalesEstimate_EstimateHeader=@revisionid
      
	SET NOCOUNT OFF;
END

GO