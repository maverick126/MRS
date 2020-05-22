----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_StudioM_IPAD_RemoveItemFromEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_StudioM_IPAD_RemoveItemFromEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_StudioM_IPAD_RemoveItemFromEstimate]
@estimaterevisonid		INT		,
@removeidstring			VARCHAR(MAX),
@removereasonstring		VARCHAR(MAX),
@removereasonid			VARCHAR(MAX),
@sqsuserid				INT
AS
BEGIN

	SET NOCOUNT ON;

       SELECT IDENTITY( INT ) AS tempid, data  
       INTO #temp  
       FROM dbo.Splitfunction_string_to_table(@removeidstring,',') order by outputid 
       
       SELECT IDENTITY( INT ) AS tempid2, data  
       INTO #temp2  
       FROM dbo.Splitfunction_string_to_table(@removereasonstring,'~') order by outputid 

       SELECT IDENTITY( INT ) AS tempid3, data  
       INTO #temp3  
       FROM dbo.Splitfunction_string_to_table(@removereasonid,',') order by outputid 


       -- for remove standard options
       SELECT 
						@estimaterevisonid as estimaterevisonid,
						ed2.EstimateDetailsID,
						NULL as fkidstandardinclusions,
						ed2.ProductAreaGroupID,
						GETDATE() as removedate,
						@sqsuserid as removedby,
						t2.data as reason,
						CASE WHEN t3.data = '' THEN NULL ELSE t3.data END as reasonid
       INTO				#tempSO
       FROM				#temp t
       INNER JOIN		#temp2 t2 ON t.tempid=t2.tempid2
       INNER JOIN		#temp3 t3 ON t.tempid=t3.tempid3
       INNER JOIN       EstimateDetails ed2						ON t.data=ed2.EstimateDetailsID
 
 
 
       DELETE FROM		tbl_SalesEstimate_EstimateDetails 
       WHERE			fkid_SalesEstimate_EstimateHeader=@estimaterevisonid AND
						fkidEstimateDetails IN
						(SELECT data FROM #temp)
						
						
	 -- insert deleted item to the table
	   IF @@ERROR=0
	   BEGIN
	      INSERT INTO tbl_SalesEstimate_RemovedItems
	      (
				[fkidRevision]
			   ,[fkidEstimateDetails]
			   ,[fkidStandardInclusions]
			   ,[fkidProductAreaGroup]
			   ,[RemovedDate]
			   ,[RemovedBy]
			   ,[Reason]
			   ,[fkid_SalesEstimate_PredefinedDeletionReason]	      
	      )
	      SELECT * FROM #tempSO 
	      --WHERE EstimateDetailsID NOT IN (SELECT ISNULL(fkidEstimateDetails,0) FROM tbl_SalesEstimate_RemovedItems WHERE fkidRevision = @estimaterevisonid)
	      	      
	   END
						

		UPDATE tbl_SalesEstimate_EstimateHeader
		SET ModifiedBy = @sqsuserid, 
			ModifiedOn = GETDATE()
		WHERE id_SalesEstimate_EstimateHeader = @estimaterevisonid
						
	   DROP TABLE #temp 
	   DROP TABLE #tempSO   

	SET NOCOUNT OFF;
END

GO