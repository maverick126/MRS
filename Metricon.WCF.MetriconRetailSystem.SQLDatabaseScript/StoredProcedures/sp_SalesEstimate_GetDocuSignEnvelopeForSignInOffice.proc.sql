----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetDocuSignEnvelopeForSignInOffice]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopeForSignInOffice]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <30/11/2015>
-- Description:	<get envelope list for sign in office>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopeForSignInOffice]
@contractnumber     VARCHAR(50),
@doctype			VARCHAR(50),
@versiontype		VARCHAR(50),
@printtype			VARCHAR(50),
@versionnumber      VARCHAR(50)
AS
BEGIN

	SET NOCOUNT ON;

      DECLARE @sql VARCHAR(MAX)
      
      SET @sql='
      SELECT 
          e.BCContractNumber,
          er.DocumentType,
          er.ReferenceID,
          CASE WHEN er.PrintType IS NULL OR er.PrintType='''' THEN er.VersionType ELSE er.VersionType+'' (''+er.PrintType+'')'' END AS  VersionType,
          er.VersionNumber,
          er.PrintType,
          er.Application,
          ev.EnvelopeId,
          dc.documentname
      FROM   syn_DocuSign_envelopes ev
      INNER JOIN syn_DocuSign_Documents dc ON ev.id=dc.envelopeid 
      INNER JOIN syn_DocuSign_EnvelopeCustomerFieldsInRow er ON ev.id=er.fkidEnvelope
      LEFT JOIN Estimate e ON RTRIM(er.ReferenceID)=CAST(EstimateID AS VARCHAR)
      WHERE ev.LastStatusCode NOT IN (''Voided'',''Completed'') '
      
      IF(RTRIM(LTRIM(@contractnumber))<>'')
         SET @sql=@sql+ ' AND e.BCContractNumber='+@contractnumber
      
      IF(@doctype<>'ALL')
         SET @sql=@sql+ ' AND RTRIM(er.DocumentType)='''+REPLACE(@doctype, ' ','')+''''
         
      IF(@versiontype<>'ALL')
         SET @sql=@sql+ ' AND RTRIM(er.VersionType)='''+REPLACE(@versiontype, ' ','')+''''
         
      IF(@printtype<>'ALL')
         SET @sql=@sql+ ' AND RTRIM(er.PrintType)='''+REPLACE(@printtype, ' ','')+''''             
 
      IF(@versionnumber<>'')
         SET @sql=@sql+ ' AND RTRIM(er.VersionNumber)='''+ @versionnumber+'''' 
         
       -- select  @sql
      EXEC (@sql)
         
	SET NOCOUNT OFF;
END
GO