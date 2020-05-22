 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CheckEstimateLockStatus]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CheckEstimateLockStatus]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <21/08/2012>
-- Description:	<check estimate lock status. if it's locked by ipad user, return message, if it's open, then lock it for web app, return OK>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_CheckEstimateLockStatus] 
@estimaterevisionid		INT

AS
BEGIN

	SET NOCOUNT ON;
      DECLARE @status INT
      -- lock status 1-- current used by IPAD, 2-- open, 3-- currently used by web App
      
      SELECT @status=ISNULL(lockstatus,2)   
      FROM   tbl_SalesEstimate_EstimateHeader
      WHERE  id_SalesEstimate_EstimateHeader=@estimaterevisionid
      

	  IF (@status=1) 
	     SELECT @status AS [status], 'This estimate is currently in use by IPAD user.' AS [Message]
      ELSE IF(@status=2)
         BEGIN
            UPDATE tbl_SalesEstimate_EstimateHeader
            SET lockstatus=3
            WHERE id_SalesEstimate_EstimateHeader=@estimaterevisionid
            
            SELECT @status AS [status], 'OK' AS [Message]
         END
      ELSE   
         SELECT @status AS [status],'OK' AS [Message]
	SET NOCOUNT OFF;
	
END

GO