----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DocuSignIntegration_UpdateMRSIntegrationStatus]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_DocuSignIntegration_UpdateMRSIntegrationStatus]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<fz>
-- Create date: <04/11/2015>
-- Description:	<update docusign integration status>
-- =============================================
ALTER PROCEDURE [dbo].[sp_DocuSignIntegration_UpdateMRSIntegrationStatus] 
@idstring				VARCHAR(MAX),
@messagestring			VARCHAR(MAX),
@envelopeidstring       VARCHAR(MAX)
AS
BEGIN
 
	SET NOCOUNT ON;

           SELECT   IDENTITY(INT) AS tid, data AS integrationid 
           INTO		#tempid
           FROM		dbo.Splitfunction_string_to_table(@idstring,',')         

           SELECT   IDENTITY(INT) AS tid, data AS errormessage 
           INTO		#tempmessage
           FROM		dbo.split_string_to_table_by_multiple_characters(@messagestring,'~!~') 

           SELECT   IDENTITY(INT) AS tid, data AS envelopeid 
           INTO		#tempenvelopeid
           FROM		dbo.Splitfunction_string_to_table(@envelopeidstring,',')              
           
           UPDATE tbl_SalesEstimate_DocusignDocumentStatus
               SET Processed=CASE WHEN RTRIM(t2.errormessage)='OK' THEN 1 ELSE 0 END,
                   ProcessedOn=GETDATE(),
                   ErrorMessage=t2.errormessage,
                   EnvelopeID=t3.envelopeid,
                   FailureCount=CASE WHEN RTRIM(t2.errormessage)='OK' THEN FailureCount ELSE FailureCount+1 END,
                   Active=CASE WHEN RTRIM(t2.errormessage)='OK' THEN 0 ELSE 1 END
           FROM   tbl_SalesEstimate_DocusignDocumentStatus ds
           INNER JOIN #tempid t1 ON ds.id_SalesEstimate_DocusignDocumentStatus=t1.integrationid
           INNER JOIN #tempmessage t2 ON t1.tid=t2.tid
           INNER JOIN #tempenvelopeid t3 ON t1.tid=t3.tid
              

	SET NOCOUNT OFF;
END

GO