----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_DocuSignIntegration_UpdateDownLoadList]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_DocuSignIntegration_UpdateDownLoadList]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[sp_DocuSignIntegration_UpdateDownLoadList] 
@envelopeidstring		VARCHAR(MAX),
@errormessage			VARCHAR(MAX)
AS
BEGIN

	SET NOCOUNT ON;
       
           SELECT   IDENTITY(INT) AS tid, data AS envelopeid 
           INTO		#tempid
           FROM		dbo.Splitfunction_string_to_table(@envelopeidstring,',')         

           SELECT   IDENTITY(INT) AS tid, data AS errormessage 
           INTO		#tempmessage
           FROM		dbo.split_string_to_table_by_multiple_characters(@errormessage,'~!~')        
      
      -- insert new envelope
      
           INSERT INTO tbl_SalesEstimate_DocusignDownLoadedEnvelope
           SELECT 
                 t1.envelopeid,
                 t2.errormessage,
                 CASE WHEN RTRIM(t2.errormessage)='OK'
                      THEN 1
                      ELSE 0
                 END
           FROM  #tempid t1 
           INNER JOIN #tempmessage t2 ON t1.tid=t2.tid  
           LEFT JOIN tbl_SalesEstimate_DocusignDownLoadedEnvelope dw ON t1.envelopeid=dw.envelopeid
           WHERE dw.envelopeid IS NULL       

       -- update old envelope
           UPDATE tbl_SalesEstimate_DocusignDownLoadedEnvelope
              SET errormessage=t2.errormessage,
                  downloadsuccessful= CASE WHEN RTRIM(t2.errormessage)='OK'
										   THEN 1
										   ELSE 0
									  END
           FROM tbl_SalesEstimate_DocusignDownLoadedEnvelope dw
           INNER JOIN #tempid t1 ON t1.envelopeid=dw.envelopeid
           INNER JOIN #tempmessage t2 ON t1.tid=t2.tid


	SET NOCOUNT OFF;
END

GO