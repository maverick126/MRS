----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_ValidateDocuSignRecipientActionAndDocument]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_ValidateDocuSignRecipientActionAndDocument]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <06/12/2015>
-- Description:	<validate docusign stuff before send>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_ValidateDocuSignRecipientActionAndDocument] 
@estimateid				INT,
@versionnumber			INT,
@recipientname          VARCHAR(MAX),
@recipienttype          VARCHAR(MAX),
@recipientaction        VARCHAR(MAX)
AS
BEGIN
-- action 0-- no action, 1-- remote signer, 2-- sign in office, 3-- receive a copy
	SET NOCOUNT ON;

           DECLARE @stateid INT, @documenttype VARCHAR(20), @revisonid INT,@maxclient INT, @maxstaff INT, @minstaff INT
           DECLARE @tempTab TABLE (doctype varchar(20))
           DECLARE @clientsignercount INt, @staffsignercount INT, @errormessage VARCHAR(100)
           
           SELECT  @revisonid=id_SalesEstimate_EstimateHeader,
                   @stateid=h.fkStateID
           FROM    tbl_SalesEstimate_EstimateHeader eh
           INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
           INNER JOIN Home h ON e.HomeID=h.HomeID
           WHERE   fkidEstimate=@estimateid AND 
                   RevisionNumber=@versionnumber
                   
           INSERT INTO @tempTab
           EXEC dbo.sp_SalesEstimate_GetCustomerDocumentType @revisonid
           
           SELECT @documenttype=doctype 
           FROM @tempTab
           
           SELECT   IDENTITY(INT) AS tid, data AS recipientname 
           INTO		#tempname
           FROM		dbo.Splitfunction_string_to_table(@recipientname,',')    
           
           SELECT   IDENTITY(INT) AS tid, data AS recipienttype 
           INTO		#tempreptype
           FROM		dbo.Splitfunction_string_to_table(@recipienttype,',')    
           
           SELECT   IDENTITY(INT) AS tid, data AS  action 
           INTO		#tempaction
           FROM		dbo.Splitfunction_string_to_table(@recipientaction,',')   
             
       -- get max signer number for staff and client    
           IF(EXISTS(           SELECT * 
           FROM  tbl_SalesEstimate_DocusignDocumentValidation   
           WHERE fkidState=@stateid AND
                 Active=1 AND
                 RTRIM(DocumentType)=RTRIM(@documenttype)
                 )
                )
			   BEGIN 
				   SELECT @maxclient=MaxClientSignerNumber,
				          @maxstaff=MaxStaffSignerNumber
				   FROM  tbl_SalesEstimate_DocusignDocumentValidation   
				   WHERE fkidState=@stateid AND
						 Active=1 AND
						 RTRIM(DocumentType)=RTRIM(@documenttype)
			   END 
           ELSE IF(EXISTS( SELECT * 
           FROM  tbl_SalesEstimate_DocusignDocumentValidation   
           WHERE fkidState=@stateid AND
                 Active=1 AND
                 RTRIM(DocumentType)='ANY'
                 )
                )
			   BEGIN 
				   SELECT @maxclient=MaxClientSignerNumber,
				          @maxstaff=MaxStaffSignerNumber,
				          @minstaff=minstaffsignernumber
				   FROM  tbl_SalesEstimate_DocusignDocumentValidation   
				   WHERE fkidState=@stateid AND
						 Active=1 AND
						 RTRIM(DocumentType)='ANY'
			   END	
		-- validate
			   SELECT t1.*, t2.recipienttype, t3.action
			   INTO #tempfinal
			   FROM #tempname t1
			   INNER JOIN #tempreptype t2 ON t1.tid=t2.tid
			   INNER JOIN #tempaction t3 ON t1.tid=t3.tid
			   
			   SELECT @clientsignercount=COUNT(*)
			   FROM #tempfinal
			   WHERE recipienttype='customer' AND action='1'
			   
			   SELECT @staffsignercount=COUNT(*)
			   FROM #tempfinal
			   WHERE recipienttype='staff' AND action='1'		
			   
			   SET @errormessage='OK'
			   
			   IF(@clientsignercount>@maxclient)
			      BEGIN
			        IF(@errormessage='OK')
			   	   	   SET @errormessage='Max number of client signer is '+CAST(@maxclient AS VARCHAR)
			   	   	ELSE
			   	   	   SET @errormessage=@errormessage+' Max number of client signer is '+CAST(@maxclient AS VARCHAR)+'.'	
			   	  END	   
			   IF(@staffsignercount>@maxstaff OR @staffsignercount<@minstaff)
			     BEGIN
			        IF(@errormessage='OK')
			           SET @errormessage='Max number of Metricon staff signer is '+CAST(@maxstaff AS VARCHAR)+'. Min number of Metricon staff signer is '+CAST(@minstaff AS VARCHAR)+'.'
			        ELSE
			           SET @errormessage=@errormessage+' Max number of Metricon staff signer is '+CAST(@maxstaff AS VARCHAR)+'. Min number of Metricon staff signer is '+CAST(@minstaff AS VARCHAR)+'.'
			     END
			     
			  SELECT @errormessage AS validationMessage   
			     
			     
	SET NOCOUNT OFF;
END
GO
