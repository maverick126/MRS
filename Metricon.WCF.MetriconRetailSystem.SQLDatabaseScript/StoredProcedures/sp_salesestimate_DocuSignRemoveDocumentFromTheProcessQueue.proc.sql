----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_salesestimate_DocuSignRemoveDocumentFromTheProcessQueue]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_salesestimate_DocuSignRemoveDocumentFromTheProcessQueue]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <05/11/2015>
-- Description:	<remove unprocessed request from queue by id>
-- =============================================
ALTER PROCEDURE [dbo].[sp_salesestimate_DocuSignRemoveDocumentFromTheProcessQueue]
@integrationid   INT
AS
BEGIN

	SET NOCOUNT ON;

      UPDATE tbl_SalesEstimate_DocusignDocumentStatus
      SET Active=0
      WHERE id_SalesEstimate_DocusignDocumentStatus=@integrationid
      

	SET NOCOUNT OFF;
END


GO
