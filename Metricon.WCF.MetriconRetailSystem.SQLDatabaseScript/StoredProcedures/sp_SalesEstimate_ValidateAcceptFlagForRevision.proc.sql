----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_ValidateAcceptFlagForRevision]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_ValidateAcceptFlagForRevision]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <26/11/2012>
-- Description:	<validate accept flag for revision>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_ValidateAcceptFlagForRevision] 
@estimaterevisionid		INT,
@userroleid				INT
AS
BEGIN

	SET NOCOUNT ON;
	
        DECLARE @estimateid INT, @revisiontypeid INT
        DECLARE @regionid INT, @brandid INT, @homeproductid VARCHAR(30)
		DECLARE @totalprice DECIMAL(18,2),  @totalcost DECIMAL(18,2), @margin DECIMAL(18,2), @targetmargin DECIMAL(18,4)
		DECLARE @homeprice DECIMAL(18,2),@totalupgrade DECIMAL(18,2)
		DECLARE @homecost DECIMAL(18,2), @upgradecost DECIMAL(18,2),@siteworkcost DECIMAL(18,2)
		DECLARE @derivedhomepercentage decimal(18,4),@deriveditempercentage decimal(18,4), @targetmargin2 DECIMAL(18,2)
	        
        DECLARE @finaltable TABLE
        (
           areaname			VARCHAR(100),
           groupname		VARCHAR(100),
           errormessage		VARCHAR(1000),
           reason			VARCHAR(1000),
           AllowGoAhead     BIT,
           ErrorIconToolTips VARCHAR(100),
           ErrorIconpath    VARCHAR(100),
           upgrade			VARCHAR(100),
           sortorder        INT
        )
        

        DECLARE @temptable TABLE
        (
           estimatedetailsid        INT,
           fkid_NonStandardArea     INT,
           fkid_NonStandardGroup    INT
        )
                
        SELECT		@estimateid=fkidestimate, @revisiontypeid=fkid_salesestimate_revisiontype
        FROM		tbl_salesestimate_estimateheader
        WHERE		id_salesestimate_estimateheader=@estimaterevisionid
        
        SELECT		* 
        INTO		#temped
        FROM		estimatedetails
        WHERE		estimateid=@estimateid
   
				   
        IF(@revisiontypeid in (4,15,19,25))  -- STS and SE version need check accept flag and nonstandard area group id
			BEGIN
			
-- get cost stuff
					EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
					EXEC [sp_SalesEstimate_GetEstimateTotalCost] @estimaterevisionid,@homecost output,@upgradecost output,@siteworkcost output
                  
						SELECT		@regionid=e.RegionID,
									@homeproductid=h.ProductID,
									@brandid=h.BrandID,
									@homeprice=eh.HomePrice
						FROM		tbl_SalesEstimate_EstimateHeader eh
						INNER JOIN	estimate e	ON eh.fkidestimate=e.estimateid
						INNER JOIN  home h ON e.HomeID=h.HomeID
						WHERE		id_SalesEstimate_EstimateHeader=@estimaterevisionid
						
						--SELECT TOP 1 @homecost=CostPrice
						--FROM Price
						--WHERE ProductID=@homeproductid AND RegionID=@regionid AND Active=1
						--ORDER BY EffectiveDate DESC

      --                  EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
		
						    
						--IF(@homecost=0)
						--   BEGIN
						--	 SET @homecost=CAST((@homeprice/1.1)*(1-@derivedhomepercentage) AS DECIMAL(18,2))
						--   END
						   
					 --  SELECT @upgradecost=SUM(sed.CostExcGST*sed.Quantity)
					 --  FROM tbl_SalesEstimate_EstimateDetails sed
					 --  INNER JOIN EstimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID
					 --  WHERE fkid_SalesEstimate_EstimateHeader=@estimaterevisionid AND ed.StandardInclusion=0 AND ed.PromotionProduct=0
					 
					   SET @totalcost=@homecost+@upgradecost+@siteworkcost

					   SELECT @totalupgrade=SUM(sed.ItemPrice*sed.Quantity)
					   FROM tbl_SalesEstimate_EstimateDetails sed
					   INNER JOIN EstimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID
					   WHERE fkid_SalesEstimate_EstimateHeader=@estimaterevisionid AND ed.PromotionProduct=0 
 				   				       
					 --  SET @totalcost=CAST(ISNULL(@homecost,0)+ISNULL(@upgradecost,0) AS DECIMAL(18,2))	
					   
					   SET @margin= CAST(100*(((@homeprice+@totalupgrade)/1.1)-@totalcost)/((@homeprice+@totalupgrade)/1.1) AS DECIMAL(18,2))
					   SET @targetmargin2=CAST(100*@targetmargin AS DECIMAL(18,2))				   					   

	-- end of cost		
 
						INSERT  INTO @temptable
						SELECT      fkidEstimateDetails, fkid_NonStandardArea, fkid_NonStandardGroup
						FROM		tbl_salesestimate_estimatedetails
						WHERE       fkid_salesestimate_estimateheader= @estimaterevisionid  AND ISNULL(SalesEstimatorAccepted,0)=0
						          
						INSERT INTO @temptable
						SELECT ed2.fkidEstimateDetails, fkid_NonStandardArea, fkid_NonStandardGroup
						FROM        tbl_salesestimate_estimatedetails ed2
						INNER JOIN  EstimateDetails ed ON ed2.fkidEstimateDetails=ed.EstimateDetailsID
						WHERE       fkid_salesestimate_estimateheader= @estimaterevisionid  AND ISNULL(SalesEstimatorAccepted,0)=1 AND ed.areaid=43 AND
						            (ed2.fkid_NonStandardArea IS NULL OR ed2.fkid_NonStandardArea=0 OR ed2.fkid_NonStandardGroup IS NULL OR ed2.fkid_NonStandardGroup=0) 
		                IF(@margin<@targetmargin2)
		                  BEGIN
		                     IF(@userroleid=5) -- if is estimate, the message is error message others are warning message
		                        BEGIN
									 INSERT INTO @finaltable	                     
								     SELECT '','','This estimate cannot be accepted as margin is lower then target margin. If you do wish to accept this estimate or wish to discuss reason for low margin please speak to estimating manager who can login to system and accept this estimate on your behalf.','Margin for this estimate is at  '+CAST(@margin AS VARCHAR)+'% which is lower then target margin '+CAST(@targetmargin2 AS VARCHAR)+'%.'
												,0,'Margin is below acceptable percentage for this estimate.','../images/exclamation.png','',3
							    END
							 ELSE
		                        BEGIN
									 INSERT INTO @finaltable	                     
								     SELECT '','','This estimate cannot be accepted as margin is lower then target margin. If you do wish to accept this estimate or wish to discuss reason for low margin please speak to estimating manager who can login to system and accept this estimate on your behalf.','Margin for this estimate is at  '+CAST(@margin AS VARCHAR)+'% which is lower then target margin '+CAST(@targetmargin2 AS VARCHAR)+'%.'
												,1,'Margin is below acceptable percentage for this estimate.','../images/exclamation.png','',3
							    END							 
		                  END
		                  
					                  
						SELECT      ed.*  INTO #tempMarginNull
						FROM		tbl_salesestimate_estimatedetails e
						INNER JOIN  #temped ed ON e.fkidestimatedetails=ed.estimatedetailsid
						INNER JOIN  product pt ON ed.productid=pt.ProductID
						WHERE       fkid_salesestimate_estimateheader= @estimaterevisionid  AND ed.areaid=43 AND ( e.CostExcGST IS NULL ) AND pt.UOM<>'NT'
						
						INSERT INTO @finaltable
						SELECT 
								a.areaname,
								g.groupname,
								'You have not given cost to this product ['+pag.productid+'].',	
								'All NSR must have cost before you accept the estimate.',
								0 AllowGoAhead,
								'Please cost each NSR items.' AS ErrorIconToolTips,
								'../images/exclamation.png' AS ErrorIconpath,
								'' AS upgrade,
								3														
						FROM 	#tempMarginNull t                 
						INNER JOIN  #temped ed ON t.estimatedetailsid=ed.estimatedetailsid
						INNER JOIN  productareagroup pag ON ed.productareagroupid=pag.productareagroupid
						INNER JOIN  area a ON pag.areaid=a.areaid
						INNER JOIN	[group] g ON pag.groupid=g.groupid
						
						
						-- sitework is null validation
						SELECT      ed.*  INTO #tempSiteworkNull
						FROM		tbl_salesestimate_estimatedetails e
						INNER JOIN  #temped ed ON e.fkidestimatedetails=ed.estimatedetailsid
						INNER JOIN  ProductAreaGroup pag on ed.ProductAreaGroupID=pag.ProductAreaGroupID AND pag.IsSiteWork=1
						WHERE       fkid_salesestimate_estimateheader= @estimaterevisionid  AND ( e.CostExcGST IS NULL )

						INSERT INTO @finaltable
						SELECT 
								a.areaname,
								g.groupname,
								'You have not given cost to this product ['+pag.productid+'].',	
								'All site works must have cost before you accept the estimate.',
								0 AllowGoAhead,
								'Please cost each sitework items.' AS ErrorIconToolTips,
								'../images/exclamation.png' AS ErrorIconpath,
								'' AS upgrade,
								3														
						FROM 	#tempSiteworkNull t                 
						INNER JOIN  #temped ed ON t.estimatedetailsid=ed.estimatedetailsid
						INNER JOIN  productareagroup pag ON ed.productareagroupid=pag.productareagroupid
						INNER JOIN  area a ON pag.areaid=a.areaid
						INNER JOIN	[group] g ON pag.groupid=g.groupid												
						-- end sitework validation
						
											                  
		          
				        INSERT INTO @finaltable
						SELECT 
									a.areaname,
									g.groupname,
									CASE WHEN a.areaname='Non Standard Request'
										 THEN
											CASE WHEN fkid_NonStandardArea IS NULL OR fkid_NonStandardArea=0
												 THEN 
														CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
															 THEN 'Product ['+pag.productid+'] is not yet accepted. Please select Area and Group for NSR.'
															 ELSE 'Product ['+pag.productid+'] is not yet accepted. Please select Area for NSR.'
														END
												 ELSE 
														CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
															 THEN 'Product ['+pag.productid+'] is not yet accepted. Please select Group for NSR.'
															 ELSE 'Product ['+pag.productid+'] is not yet accepted.'
														END					             
											END					     					        
										 ELSE 'Product ['+pag.productid+'] is not yet accepted.'
									END AS errormessage,

									CASE WHEN a.areaname='Non Standard Request'
										 THEN
											CASE WHEN fkid_NonStandardArea IS NULL OR fkid_NonStandardArea=0
												 THEN 
														CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
															 THEN 'Not accepted yet. Please select Area and Group for NSR.'
															 ELSE 'Not accepted yet. Please select Area for NSR.'
														END
												 ELSE 
														CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
															 THEN 'Not accepted yet. Please select Group for NSR.'
															 ELSE 'Not accepted yet'
														END					             
											END					          
										 ELSE 'Not accepted yet.'
									END AS reason,	
					
									CAST (0 AS BIT) AS AllowGoAhead,
									'This product is not yet accepted.' AS ErrorIconToolTips,
									'../images/exclamation.png' AS ErrorIconpath,
									'' AS upgrade,
									0
									
						FROM		@temptable t
						INNER JOIN  #temped ed ON t.estimatedetailsid=ed.estimatedetailsid
						INNER JOIN  productareagroup pag ON ed.productareagroupid=pag.productareagroupid
						INNER JOIN  area a ON pag.areaid=a.areaid
						INNER JOIN	[group] g ON pag.groupid=g.groupid
	
			END

	 --ELSE -- other role only check non standard request area group id
		--	BEGIN
			SELECT      ed.*, e.fkid_NonStandardArea,e.fkid_NonStandardGroup INTO #tempNON
			FROM		tbl_salesestimate_estimatedetails e
			INNER JOIN  #temped ed ON e.fkidestimatedetails=ed.estimatedetailsid
			WHERE       fkid_salesestimate_estimateheader= @estimaterevisionid  AND ed.areaid=43 AND (e.fkid_NonStandardArea IS NULL OR e.fkid_NonStandardArea=0 )

	        INSERT INTO @finaltable
			SELECT 
						a.areaname,
						g.groupname,

						CASE WHEN fkid_NonStandardArea IS NULL OR fkid_NonStandardArea=0
							 THEN 'Please select Area for NSR ['+pag.productid+'].'
									--CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
									--	 THEN 'Please select Area and Group for NSR ['+pag.productid+'].'
									--	 ELSE 'Please select Area for NSR ['+pag.productid+'].'
									--END
							 ELSE ''
									--CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
									--	 THEN 'Please select Group for NSR ['+pag.productid+'].'
									--	 ELSE ''
									--END					             
						END	 AS errormessage,


						CASE WHEN fkid_NonStandardArea IS NULL OR fkid_NonStandardArea=0
							 THEN 'Please select Area for NSR ['+pag.productid+'].'
									--CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
									--	 THEN 'Please select Area and Group for NSR ['+pag.productid+'].'
									--	 ELSE 'Please select Area for NSR ['+pag.productid+'].'
									--END
							 ELSE ''
									--CASE WHEN fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0
									--	 THEN 'Please select Group for NSR ['+pag.productid+'].'
									--	 ELSE ''
									--END				             
						END	 AS reason,	
		
						CAST (0 AS BIT) AS AllowGoAhead,
						'Please select Area and Group for NSR.' AS ErrorIconToolTips,
						'../images/exclamation.png' AS ErrorIconpath,
						'' AS upgrade,
						1
						
			FROM		#tempNON ed
			INNER JOIN  productareagroup pag ON ed.productareagroupid=pag.productareagroupid
			INNER JOIN  area a ON pag.areaid=a.areaid
			INNER JOIN	[group] g ON pag.groupid=g.groupid
			--END
			
			SELECT * FROM @finaltable
			ORDER BY sortorder, areaname, groupname

	SET NOCOUNT OFF;
END
GO
