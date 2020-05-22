----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetDocuSignEnvelopHistoyByRevision]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopHistoyByRevision]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopHistoyByRevision] 
@revisionid				INT,
@versiontype            VARCHAR(50),
@printtype              VARCHAR(50)
AS
BEGIN

	SET NOCOUNT ON;
       DECLARE @completeddate DATETIME, @develierddate DATETIME, @envelopeid VARCHAR(50), @estimateid VARCHAR(10), @versionnumber VARCHAR(10)
       DECLARE @total INT, @idx INT, @envelopestatus VARCHAR(50), @lastupdatedate DATETIME, @voidreason VARCHAR(MAX)

       SELECT CAST('' AS VARCHAR(50)) AS UserName,
              CAST('' AS VARCHAR(100)) AS ActionStatus,
              GETDATE() AS [ActionTime],
              CAST('Sent' AS VARCHAR(MAX)) AS  LastStatusCode,
              CAST('' AS VARCHAR(50)) AS envelopeid
       INTO   #temp
       DELETE FROM #temp
       
       IF(@versiontype LIKE '%Customer%')
          SET @versiontype=SUBSTRING(@versiontype,1,PATINDEX('% %',@versiontype))
       
       SELECT @estimateid=fkidEstimate,
              @versionnumber=RevisionNumber
       FROM tbl_SalesEstimate_EstimateHeader
       WHERE id_SalesEstimate_EstimateHeader=@revisionid
 
       SELECT IDENTITY(INT) AS tid, en.EnvelopeId
       INTO #tempEn 
       FROM  dbo.syn_DocuSign_EnvelopeCustomerFieldsInRow er
       INNER JOIN dbo.syn_DocuSign_Envelopes en ON er.fkidEnvelope=en.Id
       WHERE er.ReferenceID=@estimateid AND 
             er.VersionNumber=@versionnumber AND 
             er.PrintType=@printtype AND
             er.VersionType=@versiontype


        SET @idx=1
        SELECT @total=COUNT(*)
        FROM #tempEn
        
        WHILE (@idx<=@total)
             BEGIN
                    SELECT @envelopeid=envelopeid
                    FROM #tempEn
                    WHERE tid=@idx
                    
                    INSERT INTO #temp
					SELECT rc.UserName+' ('+ rs.recipienttype+')' as UserName,
						   rs.ActionStatus,
						   CASE WHEN rs.ActionStatus='Sent'
								THEN rs.DateSentUTC
								ELSE CASE WHEN rs.ActionStatus='Delivered'
										  THEN rs.DateDeliveredUTC
										  ELSE CASE WHEN rs.ActionStatus='Completed'
													THEN rs.DateSignedUTC
													ELSE CASE WHEN rs.ActionStatus='Voided' OR rs.ActionStatus='Declined'
															  THEN rs.DateDeclinedUTC
															  ELSE DateSentInitialUTC
														 END
											   END
									 END
			                              
						   END AS [ActionTime],
						   CAST('Sent' AS VARCHAR(30)) AS  LastStatusCode,
					       @envelopeid
					FROM syn_DocuSign_Envelopes ev  
					INNER JOIN syn_DocuSign_EnvelopesRecipientsJoin er ON ev.Id=er.EnvelopeId
					INNER JOIN syn_DocuSign_Recipients rc ON er.RecipientId=rc.Id
					INNER JOIN syn_DocuSign_RecipientStatuses rs ON rc.DocuSignId=rs.DocuSignId
					WHERE ev.EnvelopeId=@envelopeid
					
					SELECT @envelopestatus=LastStatusCode,
					       @lastupdatedate=DateLastTimestampUTC,
					       @voidreason=CASE WHEN VoidReason IS NULL OR VoidReason=''
					                        THEN ''
					                        ELSE VoidReason
					                   END 
					FROM  [vm-sqlmel01].[DocuSignInsightDb].[dbo].Envelopes
					WHERE EnvelopeId=@envelopeid
			 
			        IF(@envelopestatus='Voided')
			           BEGIN
			              INSERT INTO #temp
			              SELECT '','',@lastupdatedate,@envelopestatus+char(13)+char(10)+@voidreason,@envelopeid 
			           END
			 
			 
					SELECT @completeddate= dbo.FormatDateTime(DateCompletedUTC,'YYYY-MM-DD')+' '+dbo.FormatDateTime(DateCompletedUTC,'HH:MM:SS 24'),
						   @develierddate= dbo.FormatDateTime(DateDeliveredUTC,'YYYY-MM-DD')+' '+dbo.FormatDateTime(DateDeliveredUTC,'HH:MM:SS 24')
					FROM  syn_DocuSign_Envelopes
					WHERE EnvelopeId=@envelopeid      
			        
					UPDATE #temp
					SET LastStatusCode='Completed'
					WHERE actiontime> @completeddate

					UPDATE #temp
					SET LastStatusCode='Delivered'
					WHERE actiontime<=@completeddate AND actiontime> @develierddate     

              SET @idx=@idx+1
              
        END



        SELECT * 
        FROM #temp
        ORDER BY [ActionTime]
      
	SET NOCOUNT OFF;
	
END

GO