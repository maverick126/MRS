----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DocuSignIntegration_GetDocumentListToDownload]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_DocuSignIntegration_GetDocumentListToDownload]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <02/12/2015>
-- Description:	<get document list to download>
-- =============================================
ALTER PROCEDURE [dbo].[sp_DocuSignIntegration_GetDocumentListToDownload] 

AS
BEGIN

	SET NOCOUNT ON;
	   DECLARE @revisionid INT, @documenttype VARCHAR(50), @revisiontypeid INT, @customerdoctype VARCHAR(50),@dayslookback INT
	   DECLARE @total INT, @idx INT, @estimateid INT, @versionnumber INT, @opportunityid VARCHAR(50), @contractnumber VARCHAR(10)
	
	   SET @customerdoctype=''
	   SET @dayslookback=7
	   
	   SELECT ev.EnvelopeId, ev.Id, dc.DocumentName
	   INTO #temp
	   FROM syn_DocuSign_Envelopes ev
	   LEFT JOIN (SELECT * FROM tbl_SalesEstimate_DocusignDownLoadedEnvelope WHERE downloadsuccessful=1) dw ON ev.EnvelopeId=dw.EnvelopeId
	   INNER JOIN syn_DocuSign_Documents dc ON ev.Id=dc.EnvelopeId
	   WHERE dw.EnvelopeId IS NULl AND 
	         ev.LastStatusCode='completed' AND
	         ev.DateCompletedUTC>=GETDATE()-@dayslookback
	         
	   SELECT IDENTITY(INT) as tid,
	          tp.envelopeid,
	          ef.ReferenceID as estimateid,
	          ef.DocumentType,
	          ef.VersionNumber,
	          ef.PrintType,
	          ef.VersionType,
	          CAST('' AS VARCHAR(50)) AS category,
	          CAST('' AS VARCHAR(50)) AS opportunityid ,
	          CAST('' AS VARCHAR(10)) AS contractnumber,
	          tp.documentname
	   INTO #temp2
	   FROM #temp tp
	   INNER JOIN syn_DocuSign_EnvelopeCustomerFieldsInRow ef ON tp.Id = ef.fkidEnvelope 
	   WHERE ef.Application='MRS'
	   
	   SET @idx=1
	   SELECT @total=COUNT(*) FROM #temp2
	   WHILE(@idx<=@total)
	       BEGIN
               
				   SELECT @estimateid=estimateid,
				          @versionnumber=VersionNumber
				   FROM   #temp2 
				   WHERE  tid=@idx
				   
				   
				   SELECT @revisionid=id_SalesEstimate_EstimateHeader,
				          @revisiontypeid=fkid_SalesEstimate_RevisionType,
				          @opportunityid=e.fkidOpportunity,
				          @contractnumber=e.BCContractNumber
				   FROM tbl_SalesEstimate_EstimateHeader eh
				   INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
				   WHERE fkidEstimate=@estimateid AND  RevisionNumber=@versionnumber
				   
				   IF(@revisionid in (6,7,8,9,10,11,12))
					  SET @documenttype='Colour Schedule'
				   ELSE
					  SET @documenttype='MRS Estimate'

				   SELECT CAST('' AS VARCHAR(50)) AS doctype INTO #tempdoctype
				   DELETE FROM #tempdoctype
			       
				   INSERT INTO #tempdoctype
				   EXEC sp_SalesEstimate_GetCustomerDocumentType @revisionid
			       
				   IF(EXISTS(SELECT * FROM #tempdoctype))
					  BEGIN
						 SELECT @documenttype=doctype FROM #tempdoctype
					  END
                  
                  UPDATE #temp2
                  SET  category='Operations',
                       DocumentType=REPLACE(@documenttype,'PC','Preliminary Contract'),
                       opportunityid=@opportunityid,
                       contractnumber=@contractnumber
                  WHERE tid=@idx
                  
                  SET @idx=@idx+1
 
          END
          
          SELECT * FROM #temp2
          
	SET NOCOUNT OFF;
END

GO