 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_SetDefaultAnswerForEstimateRevision]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_SetDefaultAnswerForEstimateRevision]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_SetDefaultAnswerForEstimateRevision]
@idstring				VARCHAR(MAX)	,
@studiomstring			VARCHAR(MAX)	,
@usercode				INT	
AS
BEGIN

	SET NOCOUNT ON;
      
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempid  
       FROM dbo.Splitfunction_string_to_table(@idstring,',') order by outputid       
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempstudiom  
       FROM dbo.Splitfunction_string_to_table(@studiomstring,'^') order by outputid 
 
       UPDATE tbl_SalesEstimate_EstimateDetails
       SET    StudioMAttributes=ts.data
       FROM   tbl_SalesEstimate_EstimateDetails ed
       INNER JOIN #tempid ti ON ed.id_SalesEstimate_EstimateDetails=ti.data
       INNER JOIN #tempstudiom ts ON ti.tid=ts.tid
       
		
		DROP TABLE #tempid
		DROP TABLE #tempstudiom   

	SET NOCOUNT OFF;
END

GO
