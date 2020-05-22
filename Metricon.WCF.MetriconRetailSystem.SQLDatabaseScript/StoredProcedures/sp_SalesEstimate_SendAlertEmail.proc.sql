
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_SendAlertEmail]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_SendAlertEmail]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <25/02/2013>
-- Description:	<send email to user when estimate assign to user in MRS >
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_SendAlertEmail] 
@userId				INT, 
@ownerId			INT, 
@estimateRevisionId INT,
@type               VARCHAR(20)
AS
BEGIN
				DECLARE @to VARCHAR(100), @tableHTML1 VARCHAR(MAX)
				DECLARE @contract VARCHAR(10), @customercode VARCHAR(10)
				DECLARE @customername VARCHAR(100), @revisionNo INT, @lotaddress VARCHAR(200)
				DECLARE @sub VARCHAR(200), @lastcomment VARCHAR(1000)
				
				DECLARE @estimateId INT, @RevisionType VARCHAR(10), @revisionNoString VARCHAR(10), @lastrevisonOwner VARCHAR(100)
				
				SELECT @to=EmailAddress
				FROM   tbluser 
				WHERE  userid=@ownerId
				
				SELECT 
					@RevisionType = rt.Abbreviation,
					@estimateId = fkidEstimate,
					@revisionNo=RevisionNumber
				FROM tbl_SalesEstimate_EstimateHeader eh
				INNER JOIN tbl_SalesEstimate_RevisionType rt ON eh.fkid_SalesEstimate_RevisionType=rt.id_SalesEstimate_RevisionType
				WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId				      
				
				SELECT 
					@lastcomment=ISNULL(Comments, ''),
					@lastrevisonOwner=u.username
				FROM tbl_SalesEstimate_EstimateHeader eh
				INNER JOIN tbluser u ON eh.fkidOwner=u.userid 
				WHERE fkidEstimate = @estimateId AND RevisionNumber=@revisionNo-1					
							
				
				   				   
				SELECT		@customercode=BCCustomerID,
							@contract=BCContractNumber,
							@customername=o.name,
							@lotaddress=c.new_fullsiteaddress
							--@lotaddress=CASE WHEN c.new_lotnumber IS NULL OR c.new_lotnumber='' 
							--                 THEN CASE WHEN c.new_address_street IS NULL OR c.new_address_street=''
							--                           THEN
							--                                c.new_address_suburb+' '+c.new_address_postcode+','+c.new_state
							--                           ELSE
							--                                c.new_address_street+' '+c.new_address_suburb+' '+c.new_address_postcode+','+c.new_state 
							--                      END
							--                 ELSE CASE WHEN c.new_address_street IS NULL OR c.new_address_street=''
							--                           THEN 
							--                                'Lot '+c.new_lotnumber+' '+c.new_address_suburb+' '+c.new_address_postcode+','+c.new_state
							--                           ELSE
							--                                'Lot '+c.new_lotnumber+' '+c.new_address_street+' '+c.new_address_suburb+' '+c.new_address_postcode+','+c.new_state
							--                      END
							                 
							--            END
				FROM		Estimate e
				INNER JOIN  syn_crm_opportunity o		ON e.fkidOpportunity=o.opportunityid
				INNER JOIN  syn_crm_new_contract c		ON o.new_contractid=c.new_contractid
				WHERE  EstimateID=@estimateId
	            
	            SET @revisionNoString=CAST(@revisionNo AS VARCHAR)+'('+@RevisionType+')'
				SET @tableHTML1='<html><body><table width=600px border=0>'
				SET @tableHTML1=@tableHTML1+'<tr><td width=100px><b>Customer Name:</b></td><td width=20px>&nbsp;</td><td width=480px align=left>'+@customername+'</td></tr>'
				SET @tableHTML1=@tableHTML1+'<tr><td><b>Customer Code:</b></td><td>&nbsp;</td><td>'+@customercode+'</td></tr>'
				SET @tableHTML1=@tableHTML1+'<tr><td><b>Contract No:</b></td><td>&nbsp;</td><td>'+@contract+'</td></tr>'
				SET @tableHTML1=@tableHTML1+'<tr><td><b>Estimate No:</b></td><td>&nbsp;</td><td>'+CAST(@estimateId AS VARCHAR)+'</td></tr>'
				SET @tableHTML1=@tableHTML1+'<tr><td><b>Revision No:</b></td><td>&nbsp;</td><td>'+@revisionNoString+'</td></tr>'
				SET @tableHTML1=@tableHTML1+'<tr><td><b>Lot Address:</b></td><td>&nbsp;</td><td>'+@lotaddress+'</td></tr>'
				SET @tableHTML1=@tableHTML1+'<tr><td><b>Previous Comments:</b></td><td>&nbsp;</td><td>'+RTRIM(LTRIM(ISNULL(@lastcomment,'')))+'</td></tr>'
				SET @tableHTML1=@tableHTML1+'<tr><td><b>Previous Owner:</b></td><td>&nbsp;</td><td>'+RTRIM(LTRIM(ISNULL(@lastrevisonOwner,'')))+'</td></tr>'				
				SET @tableHTML1=@tableHTML1+'<tr><td><b>MRS URL:</b></td><td>&nbsp;</td><td><a href="http://metriconretail">Go to MRS</a></td></tr>'
				SET @tableHTML1=@tableHTML1+'</table></body></html>'

                IF (@type='REJECT')
                   BEGIN	            
						SET @sub='MRS - your estimate has been rejected. ' + @contract+'/'+CAST(@estimateId AS VARCHAR)+'/'+@revisionNoString
				   END
				ELSE
                   BEGIN	            
						SET @sub='MRS - New estimate has been assigned to you. ' + @contract+'/'+CAST(@estimateId AS VARCHAR)+'/'+@revisionNoString
				   END				   

				EXEC msdb.dbo.sp_send_dbmail
				@profile_name='MRSNotification',
				@recipients = @to,
				@subject = @sub,
				@body = @tableHTML1,
				@body_format = 'HTML';	  
END

GO