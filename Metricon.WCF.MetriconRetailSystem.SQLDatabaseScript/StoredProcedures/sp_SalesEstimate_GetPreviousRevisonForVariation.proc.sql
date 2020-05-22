----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetPreviousRevisonForVariation]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetPreviousRevisonForVariation]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <18/09/2014>
-- Description:	<get previous revison for a variation>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetPreviousRevisonForVariation]
@revisionid				INT	,
@previousRevisionID		INT OUTPUT
AS
BEGIN
 
	SET NOCOUNT ON;

      DECLARE @contracttype VARCHAR(10), @pre_studioM INT, @finalcontractcreated INT, @contractnumber INT
      DECLARE @studioM INT, @post_studioM INT , @maxrevisontype INT, @thisrevisontype INT
      DECLARE @estimates TABLE (estimateId INT)

      SELECT 
           @contracttype=ISNULL(ContractType,'PC'),					    				    
		   @contractnumber=BCContractNumber,
		   @thisrevisontype=fkid_SalesEstimate_RevisionType
      FROM tbl_SalesEstimate_EstimateHeader eh INNER JOIN Estimate e ON eh.fkidEstimate = e.EstimateID
      WHERE id_SalesEstimate_EstimateHeader=@revisionid
      
      INSERT INTO @estimates SELECT DISTINCT fkidEstimate FROM tbl_SalesEstimate_EstimateHeader SED INNER JOIN Estimate E
      ON SED.fkidEstimate = E.EstimateID WHERE BCContractNumber = @contractnumber
      
      --Check whether the variation is pre studio M, studio M or post studio M
      SELECT @maxrevisontype=MAX(fkid_SalesEstimate_RevisionType) 
      FROM tbl_SalesEstimate_EstimateHeader 
      WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND id_SalesEstimate_EstimateHeader<@revisionid AND fkid_SalesEstimate_RevisionType NOT IN (24,25,27) --exclude pre studio m
      
      IF(@maxrevisontype < 6)
           SET @pre_studioM = 1
      ELSE IF(@maxrevisontype >= 6 AND @thisrevisontype IN (7,8,9,10,11,12,21,22))
           SET @studioM = 1
      ELSE     	
           SET @post_studioM = 1

       IF(@pre_studioM = 1)
          BEGIN
				  IF EXISTS
				  (
					  SELECT * 
					  FROM tbl_SalesEstimate_CustomerDocument 
					  WHERE DocumentType IN('Contract','PC','VARIATION') AND fkid_SalesEstimate_EstimateHeader IN
					 (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
						AND id_SalesEstimate_EstimateHeader < @revisionid AND fkid_SalesEstimate_Status = 2) AND Active = 1
				  )      
					BEGIN
	 
					   SELECT @previousRevisionID=MAX(fkid_SalesEstimate_EstimateHeader)
					   FROM tbl_SalesEstimate_CustomerDocument
					   WHERE fkid_SalesEstimate_EstimateHeader IN (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates)
						AND id_SalesEstimate_EstimateHeader < @revisionid AND fkid_SalesEstimate_Status = 2) AND Active = 1
					END
				  ELSE
					 BEGIN
	 
						SELECT @previousRevisionID=MAX(id_SalesEstimate_EstimateHeader) 
						FROM  tbl_SalesEstimate_EstimateHeader
						WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND id_SalesEstimate_EstimateHeader<@revisionid
					 END
          END
       ELSE IF(@studioM=1)
          BEGIN
               SELECT @previousRevisionID=id_SalesEstimate_EstimateHeader
               FROM tbl_SalesEstimate_EstimateHeader 
               WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType=6 
               
          END
       ELSE IF(@post_studioM=1)
          BEGIN
				  IF EXISTS
				  (
					  SELECT * 
					  FROM tbl_SalesEstimate_CustomerDocument 
					  WHERE DocumentType IN ('Contract','VARIATION') AND fkid_SalesEstimate_EstimateHeader IN
					 (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates)
						AND id_SalesEstimate_EstimateHeader < @revisionid AND fkid_SalesEstimate_Status = 2) AND Active = 1
				  )      
					BEGIN
					   SELECT @previousRevisionID = MAX(fkid_SalesEstimate_EstimateHeader)
					   FROM tbl_SalesEstimate_CustomerDocument
					   WHERE fkid_SalesEstimate_EstimateHeader IN (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
					   AND id_SalesEstimate_EstimateHeader < @revisionid AND fkid_SalesEstimate_Status = 2) AND Active = 1
					END
				  ELSE IF EXISTS 
				  (
					SELECT * FROM tbl_SalesEstimate_EstimateHeader 
					WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 6 AND @thisrevisontype = 23
				  )
					 BEGIN
					   SELECT @previousRevisionID = id_SalesEstimate_EstimateHeader
					   FROM tbl_SalesEstimate_EstimateHeader 
					   WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 6 
					 END
				 ELSE
				 BEGIN
					   SELECT @previousRevisionID = id_SalesEstimate_EstimateHeader
					   FROM tbl_SalesEstimate_EstimateHeader 
					   WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) AND fkid_SalesEstimate_RevisionType = 23 				
				 END
          END
          
	SET NOCOUNT OFF;
END


