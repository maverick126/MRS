 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UnlockEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UnlockEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <24/08/2012>
-- Description:	<unlock estimate hold by web app or ipad app>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_UnlockEstimate] 
@estimaterevisionid		INT	,
@type					INT  --1=IPAD 3= WEB, 2=open
AS
BEGIN

	SET NOCOUNT ON;
	     -- note we can't unlock IPAD hold estimate from web. vice vesa
	
         IF(EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader=@estimaterevisionid AND lockstatus=@type))
            BEGIN
               UPDATE tbl_SalesEstimate_EstimateHeader
               SET    lockstatus=2
               WHERE  id_SalesEstimate_EstimateHeader=@estimaterevisionid
            END
		

	SET NOCOUNT OFF;
END

GO
