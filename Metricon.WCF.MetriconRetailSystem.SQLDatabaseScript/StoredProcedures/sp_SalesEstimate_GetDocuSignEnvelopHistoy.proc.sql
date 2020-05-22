----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetDocuSignEnvelopHistoy]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopHistoy]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetDocuSignEnvelopHistoy] 
@envelopeid  VARCHAR(50)
AS
BEGIN

	SET NOCOUNT ON;

        DECLARE @completeddate DATETIME, @develierddate DATETIME
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
               CAST('Sent' AS VARCHAR(30)) AS  LastStatusCode
        INTO #temp
        FROM syn_DocuSign_Envelopes ev  
        INNER JOIN syn_DocuSign_EnvelopesRecipientsJoin er ON ev.Id=er.EnvelopeId
        INNER JOIN syn_DocuSign_Recipients rc ON er.RecipientId=rc.Id
        INNER JOIN syn_DocuSign_RecipientStatuses rs ON rc.DocuSignId=rs.DocuSignId
        WHERE ev.EnvelopeId=@envelopeid
 
		SELECT @completeddate= dbo.FormatDateTime(DateCompletedUTC,'YYYY-MM-DD')+' '+dbo.FormatDateTime(DateCompletedUTC,'HH:MM:SS 24'),
		       @develierddate= dbo.FormatDateTime(DateDeliveredUTC,'YYYY-MM-DD')+' '+dbo.FormatDateTime(DateDeliveredUTC,'HH:MM:SS 24')
		FROM  [vm-sqlmel01].[DocuSignInsightDb].[dbo].envelopes
		WHERE EnvelopeId=@envelopeid      
        
        UPDATE #temp
        SET LastStatusCode='Completed'
        WHERE actiontime> @completeddate

        UPDATE #temp
        SET LastStatusCode='Delivered'
        WHERE actiontime<=@completeddate AND actiontime> @develierddate     

        SELECT * 
        FROM #temp
        ORDER BY [ActionTime]
      
	SET NOCOUNT OFF;
	
END

GO