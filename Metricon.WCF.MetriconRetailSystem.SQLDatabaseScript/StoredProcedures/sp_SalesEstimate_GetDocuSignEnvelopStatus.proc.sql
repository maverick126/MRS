----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetDocuSignEnvelopStatus]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopStatus]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <09/09/2015,>
-- Description:	<get envelop status for a revision>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopStatus] 
@revisionid  INT
AS
BEGIN

	SET NOCOUNT ON;
	
	   DECLARE @maxprocessdate DATETIME,@estimateid INT, @documenttype VARCHAR(50), @documentnumber INT
	   DECLARE @final TABLE
	   (
	      tid           INT IDENTITY,
	      integrationid INT,
	      fkid_salesestimate_estimateheader INT,
	      EnvelopeID UNIQUEIDENTIFIER,
	      documenttype VARCHAR(50),
	      versiontype VARCHAR(50),
	      printtype VARCHAR(100),
	      createdDate DATETIME,
	      [status]  VARCHAR(50),
	      statusdate DATETIME,
	      RevisionNumber INT,
	      estimateid INT,
	      EnableSendViaDocuSign INT,
	      EnableSignInPerson INT,
	      EnableVoid INT,
	      accountid VARCHAR(50)
	   )
	   
	   SELECT @estimateid=fkidEstimate 
	   FROM tbl_SalesEstimate_EstimateHeader
	   WHERE id_SalesEstimate_EstimateHeader=@revisionid
	   
	   IF(EXISTS(SELECT * FROM tbl_SalesEstimate_CustomerDocument WHERE fkid_SalesEstimate_EstimateHeader=@revisionid))
	        BEGIN
	            SELECT TOP 1  @documenttype=DocumentType ,
	                          @documentnumber=DocumentNumber
	            FROM tbl_SalesEstimate_CustomerDocument 
	            WHERE fkid_SalesEstimate_EstimateHeader=@revisionid
	            ORDER BY id_SalesEstimate_CustomerDocument DESC
	        END
	   ELSE
	        BEGIN
	            SET @documenttype='PC'
	            SET @documentnumber=0
	        END	   	   

		 SELECT ReferenceID as estimateid,versiontype,printtype,versionnumber,MAX(createddate) AS maxdate, documenttype
		 INTO #tempgroup
		 FROM syn_DocuSign_EnvelopeCustomerFieldsInRow
		 WHERE [application]='MRS' AND  ReferenceID=CAST(@estimateid as VARCHAR)
		 GROUP BY  ReferenceID,documenttype,versiontype,printtype,versionnumber
 
		 INSERT INTO @final
		 SELECT ISNULL(es.id_SalesEstimate_DocusignDocumentStatus,0),
		        @revisionid,
		        ev.EnvelopeId,
		        ISNULL(ds.documenttype,@documenttype),
		        ds.versiontype,
		        ds.printtype,
		        ds.createddate,
		        ev.LastStatusCode,
              CASE WHEN ev.LastStatusCode='Sent' 
                   THEN DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,getutcdate(),GETDATE()),DateSentUTC)  
                   ELSE
                       CASE WHEN ev.LastStatusCode='Complete'
                            THEN DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,getutcdate(),GETDATE()),DateCompletedUTC) 
                            ELSE 
                               CASE WHEN ev.LastStatusCode='Delivered'
                                    THEN DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,getutcdate(),GETDATE()),DateDeliveredUTC)  
                                    ELSE
                                         CASE WHEN ev.LastStatusCode='Signed'
                                              THEN DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,getutcdate(),GETDATE()),DateSignedUTC) 
                                              ELSE 
                                                  CASE WHEN ev.LastStatusCode='Voided'
                                                       THEN DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,getutcdate(),GETDATE()),ISNULL(DateVoidedUTC,DateSentUTC))  
                                                       ELSE DATEADD(MILLISECOND,DATEDIFF(MILLISECOND,getutcdate(),GETDATE()),DateSentUTC) 
                                                  END
                                         END
                                         
                               END
                       END
              END 	,
              ds.versionnumber,
              @estimateid,	
              CASE WHEN ev.LastStatusCode='Sent' OR ev.LastStatusCode='Delivered' THEN 0 ELSE 1 END AS EnableSendViaDocuSign,
              CASE WHEN ev.LastStatusCode='Sent' OR ev.LastStatusCode='Delivered' THEN 0 ELSE 1 END AS EnableSignInPerson,
              CASE WHEN ev.LastStatusCode='Sent' OR ev.LastStatusCode='Delivered' THEN 1 ELSE 0 END AS EnableVoid ,
              e.fkidAccount                     
		 FROM syn_DocuSign_EnvelopeCustomerFieldsInRow ds
		 INNER JOIN #tempgroup tp ON ds.referenceid=tp.estimateid AND 
									 ds.printtype=tp.printtype AND 
									 ds.versiontype=tp.versiontype AND 
									 ds.versionnumber=tp.versionnumber AND
									 ds.createddate=tp.maxdate
		 INNER JOIN syn_DocuSign_Envelopes ev ON ds.fkidenvelope=ev.Id
		 LEFT JOIN tbl_SalesEstimate_DocusignDocumentStatus es ON ev.envelopeid=es.EnvelopeID 
		 INNER JOIN Estimate e ON ds.referenceid=e.estimateid 
		 
		 DROP TABLE #tempgroup	
	
        
-- get unprocessed records

       SELECT  
               id_SalesEstimate_DocusignDocumentStatus as integrationid,
               fkid_salesestimate_estimateheader,
               '00000000-0000-0000-0000-000000000000' as envelopeid,
               @documenttype as documenttype,
               es.VersionType,              
               printtype ,
               NULL as createddate,
               'Sent' as statuscode,
               es.CreatedOn,
               eh.RevisionNumber,
               es.fkidEstimate,
               0 as EnableSendViaDocuSign,
               0 as EnableSignInPerson,
               1 as EnableVoid,
               e.fkidAccount as accountid
       INTO #tempqueue
       FROM    tbl_SalesEstimate_DocusignDocumentStatus es
       INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON es.fkid_salesestimate_estimateheader=eh.id_SalesEstimate_EstimateHeader
       INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
       WHERE   fkid_salesestimate_estimateheader=@revisionid AND Processed=0 AND es.Active=1
  
       IF(EXISTS(SELECT * FROM #tempqueue))
          BEGIN
			   SELECT MAX(integrationid) AS maxid 
			   INTO #tempmaxid
			   FROM #tempqueue
			   GROUP BY fkid_salesestimate_estimateheader,documenttype,versiontype

			   INSERT INTO @final
			   SELECT * 
			   FROM #tempqueue
			   WHERE integrationid IN (SELECT maxid FROM #tempmaxid)
			   
			   DROP TABLE #tempqueue
		  END
       
  
       SELECT MAX(tid) as maxid
       INTO #tempid2 
       FROM @final
       GROUP BY fkid_salesestimate_estimateheader,documenttype, versiontype, printtype
       
       SELECT * , @documentnumber as documentnumber
       FROM @final
       WHERE tid in (SELECT maxid FROM #tempid2)
       
       
	SET NOCOUNT OFF;
	
END

GO
