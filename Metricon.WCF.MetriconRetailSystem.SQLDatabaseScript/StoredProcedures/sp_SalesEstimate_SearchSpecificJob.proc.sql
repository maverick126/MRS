
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_SearchSpecificJob]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_SearchSpecificJob]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <21/12/2015>
-- Description:	<search for a specific contract>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_SearchSpecificJob]
@customernumber    VARCHAR(50),
@contractnumber    VARCHAR(50)
AS
BEGIN
 
	SET NOCOUNT ON;

	    DECLARE @tempTab TABLE
	    (
	       tid INT IDENTITY, 
	       customername VARCHAR(100),
	       CustomerNumber VARCHAR(10),
	       ContractNumber VARCHAR(10),
	       EstimateNumber VARCHAR(10),
	       HomeName VARCHAR(100),
	       RevisionNumber VARCHAR(100),
	       SalesConsultantName VARCHAR(50),
	       OwnerName VARCHAR(50),
	       Region VARCHAR(50),
	       Location VARCHAR(100),
	       acceptdate DATETIME,
	       RecordId INT,
	       RevisionTypeID INT,
	       RevisionTypeCode VARCHAR(50),
	       PreviousRevisionId INT
	    )
	    
	    DECLARE @sql VARCHAR(MAX)
	    
	    SET @sql='SELECT  a.name,
	                      e.bccustomerid,
	                      e.bccontractnumber,
	                      e.estimateid,
	                      h.homename,
	                      0,
	                      u.username,
	                      '''',
	                      '''',
	                      '''',
	                      acceptdate,
	                      0,
	                      0,
	                      '''',
	                      0
	    FROM estimate e
	    LEFT JOIN syn_crm_account a ON e.fkidaccount=a.accountid 
        INNER JOIN home h ON e.homeid=h.homeid	
        INNER JOIN tbluser u ON e.bcsalesconsultant=u.usercode
	    WHERE estimateid>0 '
	    
	    IF(@customernumber<>'') SET @sql=@sql+' AND bccustomerid=' +@customernumber
	    IF(@contractnumber<>'') SET @sql=@sql+' AND bccontractnumber=' +@contractnumber

	    INSERT INTO @tempTab
	    EXEC(@sql)

        IF(EXISTS(SELECT * FROM @tempTab WHERE acceptdate IS NOT NULL))
           BEGIN
 				  SELECT fkidEstimate,MAX(eh.RevisionNumber) AS MAXrevision, MAX(eh.id_SalesEstimate_EstimateHeader) AS MAXrevisionId
 				  INTO #temp
 				  FROM @tempTab tt
				  INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON tt.EstimateNumber=eh.fkidEstimate
				  WHERE fkid_SalesEstimate_RevisionType>1 AND fkid_SalesEstimate_Status <> 3
				  GROUP BY fkidEstimate
                   
                  SELECT tt.fkidEstimate
                  INTO #tempdelete
                  FROM #temp tt
                  INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON tt.fkidEstimate=eh.fkidEstimate AND tt.MAXrevision=eh.RevisionNumber
                  WHERE eh.fkid_SalesEstimate_Status=6 --OR eh.fkid_SalesEstimate_Status=3
                  
                  DELETE FROM #temp
                  WHERE fkidestimate in ( SELECT fkidEstimate FROM #tempdelete)
                                   
				  IF(Exists(
				       SELECT * FROM tbl_SalesEstimate_Queue q
				       INNER JOIN @tempTab tt ON q.fkidEstimate=tt.EstimateNumber 
				    ))
						  BEGIN
							   UPDATE @tempTab
							   SET OwnerName='',
								   RevisionNumber=CAST((SELECT MAX(RevisionNumber) FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = q.fkidEstimate) + 1 AS VARCHAR) +' ('+ rt.Abbreviation +')', 
							       Location='Unassigned: available in '+rt.Abbreviation +' new queue.', 
							       Region=mp.MRSGroupName,
							       RecordId=q.id_SalesEstimate_Queue,
							       RevisionTypeID=q.fkid_SalesEstimate_RevisionType,
							       RevisionTypeCode=rt.Abbreviation,
							       PreviousRevisionId=(SELECT MAX(id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = q.fkidEstimate)
							   FROM @tempTab tt
							   INNER JOIN tbl_SalesEstimate_Queue q ON q.fkidEstimate=tt.EstimateNumber
							   INNER JOIN tbl_SalesEstimate_RevisionType rt ON q.fkid_SalesEstimate_RevisionType=rt.id_SalesEstimate_RevisionType 	
							   INNER JOIN tbl_SalesEstimate_MRSGroup mp ON q.fkid_SalesEstimate_MRSGroup=mp.MRSGroupID			      
						      
						      SELECT * FROM @tempTab
						      
						  END
				ELSE IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader eh
				INNER JOIN #temp tt on eh.id_SalesEstimate_EstimateHeader = tt.MAXrevisionid 
				INNER JOIN tbl_SalesEstimate_CustomerDocument cd ON cd.fkid_SalesEstimate_EstimateHeader = eh.id_SalesEstimate_EstimateHeader 
				WHERE eh.fkid_SalesEstimate_Status = 2)
				BEGIN
								  UPDATE @tempTab
								  SET RevisionNumber=CAST(eh.RevisionNumber AS VARCHAR) +' ('+ rt.Abbreviation +')',
									  OwnerName=u.username,
									  Region=mp.MRSGroupName,
									  Location=u.username+'''s accepted estimate',
									  RecordId=eh.id_SalesEstimate_EstimateHeader,
									  RevisionTypeID=eh.fkid_SalesEstimate_RevisionType,
									  RevisionTypeCode=rt.Abbreviation
								  FROM @tempTab tt 
								  INNER JOIN #temp t1 ON tt.EstimateNumber=t1.fkidEstimate
								  INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON t1.fkidEstimate=eh.fkidEstimate AND maxrevision=eh.RevisionNumber
								  INNER JOIN tbl_SalesEstimate_RevisionType rt on eh.fkid_SalesEstimate_RevisionType=rt.id_SalesEstimate_RevisionType
								  INNER JOIN tbluser u ON eh.fkidOwner=u.userid
								  INNER JOIN tbl_SalesEstimate_MRSGroup mp ON eh.fkid_SalesEstimate_MRSGroup=mp.MRSGroupID
					
				END		  
				ELSE IF EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateHeader eh
				INNER JOIN #temp tt on eh.id_SalesEstimate_EstimateHeader = tt.MAXrevisionid  
				WHERE fkid_SalesEstimate_Status = 2 )
				BEGIN
					DECLARE @revisionId INT
					DECLARE @previousrevisionid INT
					DECLARE @revisionTypeId INT
					
					SET @revisionId = (SELECT MAX(MAXrevisionId) FROM #temp)
					
					SET @revisionTypeId = (SELECT fkid_SalesEstimate_RevisionType FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId)
					
					IF @revisionTypeId = 5
					BEGIN
						SET @previousrevisionid = @revisionId
					END
					ELSE
					BEGIN
						EXEC sp_SalesEstimate_GetPreviousRevisonForVariation @revisionId, @previousrevisionid OUTPUT
					END
				
					UPDATE @tempTab
					SET RevisionNumber=CAST(eh.RevisionNumber AS VARCHAR) +' ('+ rt.Abbreviation +')',
					OwnerName=u.username,
					Region=mp.MRSGroupName,
					Location=u.username+'''s accepted estimate',
					RecordId=eh.id_SalesEstimate_EstimateHeader,
					RevisionTypeID=eh.fkid_SalesEstimate_RevisionType,
					RevisionTypeCode=rt.Abbreviation
					FROM @tempTab tt 
					INNER JOIN #temp t1 ON tt.EstimateNumber=t1.fkidEstimate
					INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON t1.fkidEstimate=eh.fkidEstimate AND @previousrevisionid=eh.id_SalesEstimate_EstimateHeader
					INNER JOIN tbl_SalesEstimate_RevisionType rt on eh.fkid_SalesEstimate_RevisionType=rt.id_SalesEstimate_RevisionType
					INNER JOIN tbluser u ON eh.fkidOwner=u.userid
					INNER JOIN tbl_SalesEstimate_MRSGroup mp ON eh.fkid_SalesEstimate_MRSGroup=mp.MRSGroupID
					
				END						  
				ELSE
					BEGIN
		          
						  UPDATE @tempTab
						  SET RevisionNumber=CAST(eh.RevisionNumber AS VARCHAR) +' ('+ rt.Abbreviation +')',
							  OwnerName=u.username,
							  Region=mp.MRSGroupName,
							  Location=u.username+'''s workspace',
							  RecordId=eh.id_SalesEstimate_EstimateHeader,
							  RevisionTypeID=eh.fkid_SalesEstimate_RevisionType,
							  RevisionTypeCode=rt.Abbreviation
						  FROM @tempTab tt 
						  INNER JOIN #temp t1 ON tt.EstimateNumber=t1.fkidEstimate
						  INNER JOIN tbl_SalesEstimate_EstimateHeader eh ON t1.fkidEstimate=eh.fkidEstimate AND maxrevision=eh.RevisionNumber
						  INNER JOIN tbl_SalesEstimate_RevisionType rt on eh.fkid_SalesEstimate_RevisionType=rt.id_SalesEstimate_RevisionType
						  INNER JOIN tbluser u ON eh.fkidOwner=u.userid
						  INNER JOIN tbl_SalesEstimate_MRSGroup mp ON eh.fkid_SalesEstimate_MRSGroup=mp.MRSGroupID
					END
				END
        ELSE
        BEGIN
            DELETE FROM @tempTab
            WHERE tid>1
                
            UPDATE @tempTab
            SET OwnerName='',
            Location='SQS'
        END
           
		DELETE FROM @tempTab
		WHERE EstimateNumber NOT IN
		(SELECT fkidEstimate FROM #temp)
	    
	    SELECT * FROM @tempTab

	SET NOCOUNT OFF;
END

GO