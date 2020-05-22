----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetSignerForEnvelope]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetSignerForEnvelope]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <30/11/2015>
-- Description:	<get signer for an evelope>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetSignerForEnvelope] 
@envelopeid		VARCHAR(50)
AS
BEGIN

	SET NOCOUNT ON;

        SELECT DocuSignId,MAX(id) as maxid
        INTO #tempRecipientStatuses
        FROM  syn_DocuSign_RecipientStatuses
        WHERE RecipientType<>'CertifiedDelivery'
        GROUP BY DocuSignId
        
        SELECT rc.UserName,
               rc.emailaddress,
               rc.LastStatusCode as status,
               rs.RoutingOrder,
               rc.DocuSignId
        FROM syn_DocuSign_Envelopes ev  
        INNER JOIN syn_DocuSign_EnvelopesRecipientsJoin er ON ev.Id=er.EnvelopeId
        INNER JOIN syn_DocuSign_Recipients rc ON er.RecipientId=rc.Id
        INNER JOIN syn_DocuSign_RecipientStatuses rs ON rc.DocuSignId=rs.DocuSignId
        INNER JOIN #tempRecipientStatuses tr ON rs.DocuSignId=tr.DocuSignId AND rs.Id=tr.maxid
        WHERE ev.EnvelopeId=@envelopeid
        ORDER BY RoutingOrder,UserName
       

	SET NOCOUNT OFF;
END


GO
