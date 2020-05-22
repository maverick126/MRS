----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetItemRemoveReason]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetItemRemoveReason]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <09/09/2014>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetItemRemoveReason]

AS
BEGIN
 
	SET NOCOUNT ON;

      SELECT idsalesestimate_predefinedDeletionReason AS reasonid,
             deletionreason AS deletionreason,
             SortOrder AS sortorder 
	  INTO #temp
      FROM   tbl_salesestimate_predefinedDeletionReason
      WHERE  active=1 ORDER BY SortOrder
      
      SELECT * FROM #temp
      ORDER BY sortorder, deletionreason

	SET NOCOUNT OFF;
END



GO