----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateItemAcceptance]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateItemAcceptance]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <20/09/2012>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateItemAcceptance] 
@revisionestimatedetailsid			INT,
@accepted							INT,
@userid								INT
AS
BEGIN
	SET NOCOUNT ON;
	    DECLARE @revisontypeid INT
	    
	    
	    SELECT @revisontypeid=eh.fkid_SalesEstimate_RevisionType 
	    FROM
	    (SELECT * FROM tbl_salesestimate_estimatedetails WHERE id_SalesEstimate_EstimateDetails=@revisionestimatedetailsid) a
	    INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON a.fkid_SalesEstimate_EstimateHeader=eh.id_SalesEstimate_EstimateHeader
	
	    IF(@revisontypeid=2)
	        BEGIN
				UPDATE		tbl_salesestimate_estimatedetails
				SET			itemaccepted=@accepted,
							modifiedon=GETDATE(),
							modifiedBy=@userid,
							changed=1
				WHERE		id_salesestimate_estimatedetails=@revisionestimatedetailsid
		    END
	    ELSE
	        BEGIN
				UPDATE		tbl_salesestimate_estimatedetails
				SET			SalesEstimatorAccepted=@accepted,
							modifiedon=GETDATE(),
							modifiedBy=@userid,
							changed=1
				WHERE		id_salesestimate_estimatedetails=@revisionestimatedetailsid
		    END	    
    
	SET NOCOUNT OFF;
END

GO