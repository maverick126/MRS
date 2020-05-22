/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the problem to drop the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <19/11/2014>
-- Description:	<get exclude area and group for studio M>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision]
@estimateRevisionId INT
AS
BEGIN

	SET NOCOUNT ON;
	        DECLARE @mrsgroupid INT
	        DECLARE @returntable TABLE
	        (
	           returnid INT,
	           returnidtype VARCHAR(20)
	        )

			SELECT
				@mrsgroupid=r.MRSGroupID
			FROM tbl_SalesEstimate_EstimateHeader eh
			INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
			INNER JOIN Region r ON e.RegionID=r.RegionID
			WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
			
			-- get exclude areaid and groupid
                SELECT fkidArea, fkid_SalesEstimate_RevisionType 
                INTO #excludearea
                FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
				WHERE fkidArea IS NOT NULL			

                SELECT fkidGroup, fkid_SalesEstimate_RevisionType 
                INTO #excludegroup
                FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
				WHERE fkidGroup IS NOT NULL		
			-- get what mrs group and revison type should be merge to STM-COL	
				SELECT IDENTITY(INT) AS tid, ExcMRSGroupIDWhenSplit ,CAST(id_SalesEstimate_RevisionType AS VARCHAR) id_SalesEstimate_RevisionType
				INTO #tempExcMRSGroup
				FROM tbl_SalesEstimate_RevisionType
				WHERE ExcMRSGroupIDWhenSplit IS NOT NULL AND RTRIM(ExcMRSGroupIDWhenSplit)<>''
				
				DECLARE @idx INT, @total INT, @tempstring VARCHAR(200), @temprevisiontypeid INT
				SET @idx=1
				SELECT @total=COUNT(*) FROM #tempExcMRSGroup
				
				WHILE (@idx<=@total)
                    BEGIN	
                            SELECT @tempstring=ExcMRSGroupIDWhenSplit ,
                                   @temprevisiontypeid=id_SalesEstimate_RevisionType
                            FROM #tempExcMRSGroup 
                            WHERE tid=@idx		
							
							IF(@tempstring<>'')
								BEGIN
									SELECT data AS MRSGroupID
									INTO #tempMRSGroup 
									FROM dbo.Splitfunction_string_to_table(@tempstring,',')
							        
									IF(EXISTS(SELECT * FROM #tempMRSGroup WHERE MRSGroupID=@mrsgroupid))
										BEGIN
											DELETE FROM #excludearea WHERE fkid_SalesEstimate_RevisionType=@temprevisiontypeid
											DELETE FROM #excludegroup WHERE fkid_SalesEstimate_RevisionType=@temprevisiontypeid
										END
								END
								
						  SET @idx=@idx+1
						  DROP TABLE #tempMRSGroup
				    END
				-- build final return table
				    
				 INSERT INTO  @returntable
				 SELECT  fkidArea, 'AREA'
				 FROM #excludearea
				 
				 INSERT INTO  @returntable
				 SELECT  fkidgroup, 'GROUP'
				 FROM #excludegroup	
				 
				 
				 SELECT * FROM 	@returntable		 
				    
    	SET NOCOUNT OFF; 
END

GO