
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetMRSDisclaimer]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetMRSDisclaimer]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <20/11/2012>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetMRSDisclaimer]
@revisionid				INT	,
@state					VARCHAR(10),
@printtype				VARCHAR(20)

AS
BEGIN

	SET NOCOUNT ON;

      DECLARE @codevalue VARCHAR(50)
      DECLARE @ContractType VARCHAR(10),@buildingstate VARCHAR(10), @agreement VARCHAR(MAX)
      DECLARE @brandid INT, @contractnumber INT, @regionid INT,@pricestate VARCHAR(10)
      DECLARE @estimates TABLE (estimateId INT)
      
      SELECT @ContractType= ContractType , 
             @brandid=h.BrandID, 
             @contractnumber=e.BCContractNumber,
             @regionid=e.RegionID,
             @buildingstate=sd.State,
             @pricestate=s.StateAbbreviation
      FROM tbl_SalesEstimate_EstimateHeader eh
      INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
      INNER JOIN tblPriceRegionGroupMapping prm ON e.RegionID=prm.fkRegionID
      INNER JOIN tblRegionGroup rg ON prm.fkidRegionGroup=idRegionGroup
      INNER JOIN State s ON rg.fkStateID=s.StateID
      INNER JOIN Home h ON e.HomeID=h.HomeID
      INNER JOIN SiteDetails sd ON e.fkidOpportunity=sd.fkidOpportunity
      WHERE id_SalesEstimate_EstimateHeader=@revisionid
      
      INSERT INTO @estimates 
      SELECT DISTINCT fkidEstimate 
      FROM tbl_SalesEstimate_EstimateHeader SED 
      INNER JOIN Estimate E  ON SED.fkidEstimate = E.EstimateID 
      WHERE BCContractNumber = @contractnumber      
 
	  IF (@printtype = 'changeonly')
	     SET @codevalue = 'Variation'
      ELSE IF (RTRIM(@ContractType)='PC')
         SET @codevalue='Preliminary Contract'
      ELSE
         SET @codevalue='Contract'
         
      IF (@brandid NOT IN (47, 58, 71))
         SET @brandid=0

      -- check if the revision already has been set as HIA contract
      -- if HIA cotnract has been set, then regardless what is the contract type in revision, always use contract to get disclaimer    
	  IF EXISTS
	  (
		  SELECT * 
		  FROM tbl_SalesEstimate_CustomerDocument 
		  WHERE DocumentType IN('Contract') AND fkid_SalesEstimate_EstimateHeader IN
		 (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
			AND id_SalesEstimate_EstimateHeader < @revisionid AND fkid_SalesEstimate_Status = 2) AND Active = 1
	  ) OR EXISTS
	  (
		  SELECT * 
		  FROM tbl_SalesEstimate_CustomerDocument 
		  WHERE DocumentType IN('Contract') AND fkid_SalesEstimate_EstimateHeader IN
		 (SELECT id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate IN (SELECT estimateId FROM @estimates) 
			AND id_SalesEstimate_EstimateHeader = @revisionid ) AND Active = 1
	  )
	  BEGIN
	      IF(@printtype<>'changeonly')
	          SET @codevalue='Contract'
	  END
	         
      IF(EXISTS
        (
           SELECT *
           FROM tblsqsconfig
           WHERE Active=1 AND 
                 code='MRSEstimateAgreement' AND 
                 text3=@state AND 
                 codevalue=@codevalue AND 
                 (Text1=CAST(@brandid AS VARCHAR) or Text1 IS NULL) AND
                 RTRIM(ExtRef)=CAST(@regionid AS VARCHAR)
         )
        )
			  BEGIN
				   SELECT @agreement=REPLACE(text2, '$printdatetoken$', CAST(DAY(GETDATE()) AS VARCHAR(2)) + ' ' + DATENAME(MM, GETDATE()) + ' ' + CAST(YEAR(GETDATE()) AS VARCHAR(4)))
				   FROM tblsqsconfig
				   WHERE Active=1 AND 
						 code='MRSEstimateAgreement' AND 
						 text3=@state AND 
						 codevalue=@codevalue AND 
						 (Text1=CAST(@brandid AS VARCHAR) or Text1 IS NULL) AND
						 RTRIM(ExtRef)=CAST(@regionid AS VARCHAR)         
			  END
      ELSE
			 BEGIN   
				  SELECT @agreement=REPLACE(text2, '$printdatetoken$', CAST(DAY(GETDATE()) AS VARCHAR(2)) + ' ' + DATENAME(MM, GETDATE()) + ' ' + CAST(YEAR(GETDATE()) AS VARCHAR(4))) 
				  FROM tblsqsconfig
				  WHERE Active=1 AND code='MRSEstimateAgreement' AND text3=@state AND codevalue=@codevalue AND (Text1=CAST(@brandid AS VARCHAR) or Text1 IS NULL) AND (RTRIM(ExtRef)='0' OR ExtRef IS NULL)
			 END
			 
	  IF(@pricestate=@buildingstate)
	     BEGIN
	         SELECT @agreement AS agreement
	     END
	  ELSE
	     BEGIN
	        IF(@pricestate='VIC' AND @buildingstate='NSW')
	            BEGIN
	               SELECT REPLACE(@agreement,'Victorian State','New South Wales State') AS agreement
	            END
	        ELSE
				 BEGIN
					 SELECT @agreement AS agreement
				 END	            
	     END
	     	
	SET NOCOUNT ON;
END
GO