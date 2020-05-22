----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_salesestimate_DocuSignPushDocumentToTheProcessQueue]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_salesestimate_DocuSignPushDocumentToTheProcessQueue]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <09/09/2015>
-- Description:	<push document to the processing queue>
-- =============================================
ALTER PROCEDURE [dbo].[sp_salesestimate_DocuSignPushDocumentToTheProcessQueue]
@revisionid			INT,
@versiontype        VARCHAR(100),
@printtype			VARCHAR(100),
@userid             INT			,
@file               VARBINARY(MAX),
@pagenumber         INT,
@primarycontact     VARCHAR(200),
@primarycontactemail VARCHAR(200),
@routingorder       VARCHAR(50),
@documenttype       VARCHAR(100),
@sortorder          VARCHAR(50),
@emailsubject       VARCHAR(MAX),
@emailbody          VARCHAR(MAX)
AS
BEGIN
 
	SET NOCOUNT ON;

      DECLARE @estimateid VARCHAR(10), @revisonnumber VARCHAR(10) 
      
      SET @emailsubject=REPLACE(@emailsubject,',','&#44;')
      SET @emailbody=REPLACE(@emailbody,',','&#44;')
      
      SELECT @estimateid=estimateid, @revisonnumber=eh.RevisionNumber
      FROM tbl_SalesEstimate_EstimateHeader eh
      INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
      WHERE id_SalesEstimate_EstimateHeader=@revisionid

      IF(NOT EXISTS(SELECT * 
                FROM tbl_SalesEstimate_DocusignDocumentStatus 
                WHERE fkid_SalesEstimate_EstimateHeader=@revisionid 
                      AND VersionType=@versiontype 
                      AND printtype=@printtype
                      AND active=1
                      AND DocumentType=@documenttype
                )
        )
         BEGIN
              INSERT INTO tbl_SalesEstimate_DocusignDocumentStatus
              (
					[fkid_SalesEstimate_EstimateAction]
				   ,[fkid_SalesEstimate_EstimateHeader]
				   ,[fkidEstimate]
				   ,[ContractNumber]
				   ,[VersionType]
				   ,[PrintType]
				   ,[DocumentName]
				   ,[CreatedOn]
				   ,[CreatedBy]
				   ,[ModifiedOn]
				   ,[ModifiedBy]
				   ,[Processed]
				   ,[ProcessedOn]
				   ,[ErrorMessage]
				   ,[EnvelopeID]
				   ,[FailureCount]
				   ,[Active]   
				   ,[PDFFile] 
				   ,[SignaturePageNumber]
				   ,[RecipientName]
				   ,[RecipientEmail]  
				   ,[application] 
				   ,[RoutingOrder] 
				   ,[DocumentType]
				   ,[SortOrder]
				   ,[EmailSubject]
				   ,[EmailBody]
              )
			  SELECT 9,
					 @revisionid,
					 fkidEstimate,
					 e.BCContractNumber,
					 UPPER(@versiontype),
					 UPPER(@printtype),
					 'docusign_E'+@estimateid+'_'+@documenttype+'_V'+@revisonnumber+'.pdf',
					 GETDATE(),
					 @userid,
					 NULL,
					 NULL,
					 0,
					 NULL,
					 NULL,
					 NULL,
					 0,
					 1,
					 @file,
					 @pagenumber,
					 @primarycontact,
					 @primarycontactemail,
					 'MRS',
					 @routingorder,
					 @documenttype,
					 @sortorder,
					 @emailsubject,
					 @emailbody
			  FROM    tbl_SalesEstimate_EstimateHeader eh
			  INNER JOIN Estimate e ON eh.fkidEstimate=EstimateID
			  WHERE id_SalesEstimate_EstimateHeader=@revisionid         
         END
      ELSE
          BEGIN
              UPDATE tbl_SalesEstimate_DocusignDocumentStatus
				 SET [PDFFile]  =@file   ,
				     [SignaturePageNumber]  =@pagenumber,
				     [RecipientName]=@primarycontact,
				     [RecipientEmail]=@primarycontactemail,
				     [RoutingOrder]=@routingorder,
				     [sortorder]=@sortorder,
				     [EmailSubject]=@emailsubject,
				     [EmailBody]=@emailbody
				     
			  WHERE   fkid_SalesEstimate_EstimateHeader=@revisionid 
                      AND versiontype=@documenttype 
                      AND printtype=@printtype
                      AND active=1       
         END        


	SET NOCOUNT OFF;

END

GO
