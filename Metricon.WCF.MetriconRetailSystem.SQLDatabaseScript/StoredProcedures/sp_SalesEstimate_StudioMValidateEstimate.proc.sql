
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_StudioMValidateEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_StudioMValidateEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <23/08/2012>
-- Description:	<validate estimate SI/SO>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_StudioMValidateEstimate] 
@estimaterevisionid		INT
AS
BEGIN

	SET NOCOUNT ON;
      -- common variables
		DECLARE @brandid INT, @regiongroupid INT, @idx INT, @total INT, @revisiontypeid INT, @homeid INT, @tempestimatedetailsid INT, @temppagid INT
		DECLARE @tempsiproduct VARCHAR(50), @tempsiproductname VARCHAR(MAX), @tempsiareaname VARCHAR(200), @tempsigroupname VARCHAR(200) 
		DECLARE @tempsoproduct VARCHAR(50), @tempsoproductname VARCHAR(MAX), @tempsoareaname VARCHAR(200)

		DECLARE @tempoldsiproduct VARCHAR(50), @tempoldsiproductname VARCHAR(MAX), @tempoldsiareaname VARCHAR(200), @tempoldsigroupname VARCHAR(200) 
		DECLARE @tempoldsoproduct VARCHAR(50), @tempoldsoproductname VARCHAR(MAX), @tempoldsoareaname VARCHAR(200)
				
		DECLARE @resultTable TABLE
		(
		    siestimatedetailsid INT,
		    sipagid         INT,
		    siproductid		VARCHAR(30),
		    areaname		VARCHAR(200),
		    groupname		VARCHAR(300),
		    errormessage	VARCHAR(MAX),
		    reason			VARCHAR(MAX),
		    upgrade			VARCHAR(MAX),
		    sortorder		INT,
		    allowgoahead    INT
		)
		

		
	  DECLARE @areatable TABLE
	  (areaid INT)
	  
	  DECLARE @grouptable TABLE
	  (groupid INT)
	  
	  
	  DECLARE @estimateId INT
	  
	    SELECT      @brandid=h.brandid, 
	                @regiongroupid=fkidregiongroup,
	                @revisiontypeid=fkid_salesestimate_revisiontype,
	                @homeid=e.HomeID,
	                @estimateId=e.estimateid
	    FROM        tbl_SalesEstimate_EstimateHeader eh
	    INNER JOIN  estimate e  ON eh.fkidestimate=e.estimateid
	    INNER JOIN  home h		ON e.homeid=h.homeid
	    INNER JOIN  tblpriceregiongroupmapping pm ON e.regionid=pm.fkregionid
	    WHERE		id_SalesEstimate_EstimateHeader=@estimaterevisionid  	  


-- get excluded area and group (i.e. merge pav to color)
		DECLARE @tempTab TABLE
		(
		   id INT,
		   idtype VARCHAR(20)
		)
		  INSERT INTO @tempTab
		  EXEC sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision @estimaterevisionid
		  
 			
-- end get excluded area and group (i.e. merge pav to color)				

	 
	  
		  IF (@revisiontypeid in (8,9,10,11,12,21,22))
			BEGIN
				  INSERT INTO @areatable
				  SELECT fkidarea
				  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
				  WHERE  fkid_salesestimate_revisiontype=@revisiontypeid	AND 
						 active=1											AND
						 fkidarea IS NOT NULL								AND
						 fkidarea<>0										AND
						 excludedefinedareagroup=0
						 
				  INSERT INTO @grouptable
				  SELECT fkidgroup
				  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
				  WHERE  fkid_salesestimate_revisiontype=@revisiontypeid	AND 
						 active=1											AND
						 fkidgroup IS NOT NULL								AND
						 fkidgroup<>0										AND
						 excludedefinedareagroup=0						 
		    END
		  ELSE IF (@revisiontypeid=7)
		    BEGIN
		 
				  INSERT INTO @areatable
				  SELECT areaid
				  FROM	 area
				  WHERE  active=1 AND areaid NOT IN   
				         (
							  SELECT fkidarea 
							  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
							  WHERE  active=1											AND
									 fkidarea IS NOT NULL								AND
									 fkidarea<>0										AND
									 excludedefinedareagroup=0                          AND
									 fkidArea IN (SELECT id FROM @tempTab WHERE idtype='AREA')
						 )
						 
						 
				  INSERT INTO @grouptable
				  SELECT groupid
				  FROM   [group]
				  WHERE   active=1 AND groupid NOT IN
				        (
				              SELECT fkidgroup
							  FROM   tbl_SalesEstimate_RevisionTypeAreaGroup
							  WHERE  
									 active=1											AND
									 fkidgroup IS NOT NULL								AND
									 fkidgroup<>0										AND
									 excludedefinedareagroup=0		                    AND
									 fkidGroup IN (SELECT id FROM @tempTab WHERE idtype='GROUP')
						 )	  
		    END
		  ELSE -- normal version get all area group
			BEGIN
				  INSERT INTO @areatable
				  SELECT areaid
				  FROM	 area
				  WHERE  active=1  
			END	
	
     --Areas that should have Standard Inclusions inserted 
	 DECLARE @requiredarea TABLE
	 (areaid INT)
 
 	 DECLARE @excludedrequiredarea TABLE
	 (
	    areaid INT,
	    productid VARCHAR(50)
	 )
	     
     --Minimum Areas for the home
     INSERT INTO @requiredarea 
     SELECT AreaID FROM HomeStandardArea WHERE HomeID=@homeid AND Active=1 

     --Areas that were upgraded in the estimate
     INSERT INTO @requiredarea 
     SELECT SED.fkidArea as areaid FROM tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = ED.EstimateDetailsID WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisionid  
     
     --Related Areas of the products that were selected in the estimate (structural products)
     INSERT INTO @requiredarea 
     SELECT RA.AreaID FROM tbl_SalesEstimate_EstimateDetails SED 
          INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = ED.EstimateDetailsID
          INNER JOIN tbl_SalesEstimate_RelatedArea RA ON ED.ProductID = RA.ProductID AND RA.Active = 1 AND RA.ValidateInStudioM=1
          WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisionid
          
     INSERT INTO @excludedrequiredarea 
     SELECT DISTINCT RA.AreaID, RA.ProductID 
     FROM tbl_SalesEstimate_EstimateDetails SED 
      INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = ED.EstimateDetailsID
      INNER JOIN tbl_SalesEstimate_RelatedArea RA ON ED.ProductID = RA.ProductID AND RA.Active = 1 AND RA.ValidateInStudioM=0
      WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisionid          
	
	 --Areas of the selected NSR
     INSERT INTO @requiredarea 
     SELECT fkid_NonStandardArea FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisionid AND fkid_NonStandardArea IS NOT NULL
    
    -- exclude the groups which linked to excluded area
-- Commented 17/10/2014 WTE
--    SELECT distinct pag.GroupID
--    INTO #tempexcludedgroup
--    FROM @excludedrequiredarea ea
--    INNER JOIN ProductAreaGroup pag on ea.areaid=pag.AreaID
--    WHERE pag.Active=1
    
    
     DELETE FROM @requiredarea
     WHERE areaid IN (SELECT areaid FROM @excludedrequiredarea)
       	  
          DELETE FROM @areatable
          WHERE  areaid NOT IN
          (SELECT areaid FROM @requiredarea)

-- Commented 17/10/2014 WTE     
--          DELETE FROM @grouptable
--          WHERE  groupid IN
--          (SELECT groupid FROM #tempexcludedgroup)     



     -- get originate estimatedetails
        SELECT		ed.*
        INTO		#originateED
        FROM        tbl_SalesEstimate_EstimateHeader eh
        INNER JOIN	estimatedetails ed				ON	eh.fkidestimate=ed.estimateid 
        WHERE       id_SalesEstimate_EstimateHeader=@estimaterevisionid
        		
         
      -- get selected estimate details
        SELECT		ed.id_SalesEstimate_EstimateDetails ,ed.fkidestimatedetails, fkid_nonstandardarea, itemprice, ed.quantity, ed.productdescription, extradescription,ed.internaldescription
					itemaccept,CAST(studiomattributes AS VARCHAR(MAX)) AS studiomattributes, fkidstandardinclusions, 
					ed.fkidProductAreaGroup as productareagroupid, ed.fkidArea as areaid, ed2.productid, ed.fkidGroup as groupid
        INTO		#temped
        FROM		tbl_SalesEstimate_EstimateDetails ed
        INNER JOIN	estimatedetails ed2				ON ed.fkidestimatedetails=ed2.estimatedetailsid
        WHERE		fkid_SalesEstimate_EstimateHeader=@estimaterevisionid AND (ed.fkidarea IN (SELECT areaid FROM @areatable) OR ed.fkidgroup IN (SELECT groupid FROM @grouptable))

 	  -- get all SI
        SELECT      *
        INTO		#tempSI
        FROM		#originateED 
        WHERE       StandardInclusion=1	  		       
    
      --get existing and nonexists standard inclusion in temp table
        SELECT		si.*
        INTO		#nonexistSI
        FROM		#tempSI si
        LEFT JOIN	(SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader=@estimaterevisionid) ed		ON	si.estimatedetailsid=ed.fkidEstimateDetails
        WHERE		ed.fkid_SalesEstimate_EstimateHeader IS NULL

	    SELECT      si.*
	    INTO		#existingSI
	    FROM		#tempSI si
	    INNER JOIN  (SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader=@estimaterevisionid) ed ON si.estimatedetailsid=ed.fkidEstimateDetails				
	    WHERE       si.areaid IN (SELECT areaid FROM @areatable) OR si.groupid IN (SELECT groupid FROM @grouptable)

 -- check if all standard inclusions are in estimate
 -- validate the Standard Inclusions that have already been upgraded 
        SELECT		IDENTITY(INT) AS t_id, ed.*, p.productname, esi.productid as siproductid, esi.productname as siproductname, esi.areaname as siareaname, esi.groupname as sigroupname
        INTO        #upgradeError
        FROM		#existingSI esi
        INNER JOIN  tbl_StudioM_InclusionValidationRule vr		ON	esi.productid=vr.fkidinclusionproduct	AND vr.fkidbrand=@brandid	AND vr.Active=1
        INNER JOIN  productareagroup pag						ON	vr.fkidupgradeproduct=pag.productid		AND esi.areaid=pag.areaid	--AND esi.groupid=pag.groupid
        INNER JOIN	product p									ON	pag.productid=p.productid
        INNER JOIN  #temped ed									ON  pag.productareagroupid=ed.productareagroupid
        ORDER BY	siareaname, sigroupname,productid


    -- loop to generate error message for existing SI
        SET @idx=1
        SELECT @total=COUNT(*) FROM #upgradeError
        DECLARE @previousSI VARCHAR(30), @possibleupgrade VARCHAR(MAX)
        --SET @previousSI=''
        
        WHILE (@idx<=@total)
			BEGIN
			  SELECT
			                @tempestimatedetailsid=fkidestimatedetails,
			                @temppagid=productareagroupid,
							@tempsiproduct=siproductid,
							@tempsiproductname=siproductname,
							@tempsiareaname=siareaname,
							@tempsigroupname =sigroupname,
							@tempsoproduct=productid,
							@tempsoproductname=productname						
			  FROM			#upgradeError
			  WHERE			t_id=@idx
			  
			  SET @possibleupgrade=@tempsoproduct+' - '+@tempsoproductname

					

			  IF (EXISTS(SELECT * FROM @resultTable WHERE siproductid=@tempsiproduct AND sortorder=5))
			    BEGIN
			          UPDATE @resultTable
			          SET   upgrade=upgrade+CHAR(10)+@possibleupgrade
			          WHERE siproductid=@tempsiproduct AND sortorder=1
			    END
			  ELSE
			    BEGIN
					  INSERT INTO	@resultTable
					  SELECT		@tempestimatedetailsid,@temppagid,@tempsiproduct,@tempsiareaname,@tempsigroupname, 'Standard Inclusion - '+@tempsiproduct+' - '+@tempsiproductname+' should be removed.','Upgrade option is selected.', @possibleupgrade, 5, 1
				END	
				
			  SET @idx=@idx+1
			
			END

 -- check if all standard inclusions are not in estimate
-- validate Standard Inclusions that are missing from the estimate
        SELECT		ed.productid,ed.productname, esi.estimatedetailsid as siestimatedetailsid, esi.ProductAreaGroupID as sipagid, esi.productid as siproductid, esi.productname as siproductname, esi.areaname as siareaname,esi.groupname as sigroupname, ed2.fkidestimatedetails as newidd
        INTO        #inclusionError_temp
        FROM		#nonexistSI esi
        INNER JOIN @areatable a ON esi.areaid = a.areaid 
        LEFT JOIN   tbl_StudioM_InclusionValidationRule vr		ON	esi.productid=vr.fkidinclusionproduct	AND vr.fkidbrand=@brandid	AND vr.Active=1
        LEFT JOIN   productareagroup pag						ON	vr.fkidupgradeproduct=pag.productid		AND esi.areaid=pag.areaid	--AND esi.groupid=pag.groupid
        LEFT JOIN	product p									ON	pag.productid=p.productid
        LEFT JOIN  #originateED ed								ON  pag.productareagroupid=ed.productareagroupid and ed.homedisplayid is null
        LEFT JOIN   #temped ed2									ON  ed.estimatedetailsid=ed2.fkidestimatedetails
        WHERE		/*ed2.fkidestimatedetails IS NULL AND --commented 6/11/2013 by WTE */ ed.groupid IN (SELECT groupid FROM @grouptable)  AND  ed.estimatedetailsid is not null
        ORDER BY	siareaname, sigroupname,productid

        SELECT siproductid INTO #tempNoNeedSI FROM #inclusionError_temp WHERE newidd IS NOT NULL
        DELETE FROM #inclusionError_temp WHERE siproductid IN (SELECT siproductid FROM #tempNoNeedSI)
   
        SELECT IDENTITY(INT) AS t_id,* INTO #inclusionError FROM #inclusionError_temp
   -- loop to generate error message for existing SI

        SET @idx=1
        SELECT @total=COUNT(*) FROM #inclusionError
        
        WHILE (@idx<=@total)
			BEGIN
			  SELECT
			                @tempestimatedetailsid=siestimatedetailsid,
			                @temppagid=sipagid,
							@tempsiproduct=siproductid,
							@tempsiproductname=siproductname,
							@tempsiareaname=siareaname,
							@tempsoproduct=productid,
							@tempsigroupname =sigroupname,
							@tempsoproductname=productname					
			  FROM			#inclusionError
			  WHERE			t_id=@idx
			  
			  SET @possibleupgrade=@tempsoproduct+' - '+@tempsoproductname
			  
			  IF (EXISTS(SELECT * FROM @resultTable WHERE siproductid=@tempsiproduct AND sortorder=0))
			    BEGIN
			          UPDATE @resultTable
			          SET   upgrade=upgrade+CHAR(10)+@possibleupgrade
			          WHERE siproductid=@tempsiproduct AND sortorder=0
			    END
			  ELSE
			    BEGIN
					  INSERT INTO	@resultTable
					  SELECT		@tempestimatedetailsid,@temppagid,@tempsiproduct,@tempsiareaname,@tempsigroupname, 'Standard Inclusion - '+@tempsiproduct+' - '+@tempsiproductname+' should be added to estimate.','No upgrade option is selected.', @possibleupgrade, 0,CASE WHEN @revisiontypeid IN (14,18) THEN 1 ELSE 0 END  -- if revision is PVAR-CSC or BVAR-BSC should be allow to go ahead
				END	
			  
			  SET @idx=@idx+1
			
			END

  -- check all studio M products have the answer of the question
        INSERT INTO	@resultTable
        SELECT			
                        ed.fkidestimatedetails,ed.productareagroupid,ed.productid, a.areaname, g.groupname, 'Studio M product error ['+ed.productid+'] '+p.ProductName, 'Please answer Studio M questions.','', 1,CASE WHEN @revisiontypeid IN (14,18) THEN 1 ELSE 0 END
        FROM			#temped ed
        INNER JOIN		product p ON ed.productid=p.productid
        INNER JOIN		area a	  ON ed.areaid=a.areaid
        INNER JOIN      [group] g ON ed.groupid=g.groupid
        WHERE			p.isstudiomproduct=1		AND 
						((ed.StudioMAttributes IS NULL OR CAST(ed.StudioMAttributes AS VARCHAR(MAX))='') AND 
						((p.studiomqanda IS NOT NULL OR (CAST(p.studiomqanda AS VARCHAR(MAX))<>'')) AND CAST(p.studiomqanda AS VARCHAR(MAX)) LIKE '%mandatory="1"%' ))
		ORDER BY		ed.productid

-- get deleted SI items which no need to validate
        SELECT ri.fkidProductAreaGroup
        INTO #tempnovalidation 
        FROM tbl_SalesEstimate_RemovedItems ri
        INNER JOIN tbl_SalesEstimate_PredefinedDeletionReason rr ON ri.fkid_SalesEstimate_PredefinedDeletionReason=rr.idSalesEstimate_PredefinedDeletionReason
        WHERE rr.ValidatingInStudioM=0 and ri.fkidRevision=@estimaterevisionid
 -- remove these si from the validation error table       
        DELETE FROM @resultTable
        WHERE sipagid in (SELECT fkidProductAreaGroup FROM #tempnovalidation)


-- validate related area/ exclude area items, this is for warning message
          INSERT INTO	@resultTable
          SELECT ed.estimatedetailsid,ed.productareagroupid,ed.productid, ed.areaname, ed.groupname, 'SO related area warning: ['+RTRIM(ed.productid)+'] '+ed.ProductName, 'The upgrade product ['+ RTRIM(RA.ProductID)+'] has been selected. This SI item should be removed.',ra.productid, 5,1
          FROM tbl_SalesEstimate_EstimateDetails SED 
          INNER JOIN EstimateDetails ED ON SED.fkidEstimateDetails = ED.EstimateDetailsID
          INNER JOIN @excludedrequiredarea ra ON ED.areaid=ra.areaid
          WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisionid 

-- final result
		DECLARE @contractNumber INT
		SELECT @contractNumber = BCContractNumber FROM Estimate WHERE EstimateID = @estimateId
	
		DECLARE @estimates TABLE ( estimateId INT )
		INSERT INTO @estimates SELECT EstimateID FROM Estimate WHERE BCContractNumber = @contractNumber		

		-- check if RSTM exists
		IF NOT EXISTS (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 6 AND fkidEstimate IN (SELECT estimateId FROM @estimates))
		BEGIN
			
			DELETE FROM @resultTable

  		END -- end If RSTM revision exists
  			
	    SELECT * FROM @resultTable 
	    --WHERE sortorder=5
	    ORDER BY sortorder, areaname, groupname, siproductid
	    
	    DROP TABLE #temped
	    DROP TABLE #tempSI
	    DROP TABLE #originateED
	    DROP TABLE #nonexistSI
	    DROP TABLE #existingSI
	    DROP TABLE #inclusionError
	    DROP TABLE #upgradeError

	SET NOCOUNT OFF;
	
END


GO