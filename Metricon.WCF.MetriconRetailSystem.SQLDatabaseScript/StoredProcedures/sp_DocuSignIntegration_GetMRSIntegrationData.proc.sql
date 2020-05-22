----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DocuSignIntegration_GetMRSIntegrationData]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_DocuSignIntegration_GetMRSIntegrationData]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <04/11/2015>
-- Description:	<get docusign integration data>
-- =============================================
ALTER PROCEDURE [dbo].[sp_DocuSignIntegration_GetMRSIntegrationData]

AS
BEGIN

	SET NOCOUNT ON;

       SELECT ds.*, eh.RevisionNumber
       FROM tbl_SalesEstimate_DocusignDocumentStatus ds
       INNER JOIN Estimate e ON ds.fkidEstimate=e.EstimateID
       INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON ds.fkid_SalesEstimate_EstimateHeader=eh.id_SalesEstimate_EstimateHeader
       WHERE ds.Active=1 AND
             PDFFile IS NOT NULL AND
             SignaturePageNumber >0 AND
             Processed=0 

 	SET NOCOUNT OFF;
END

GO