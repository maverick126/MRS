----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_StudioM_IPAD_InsertItemToEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_StudioM_IPAD_InsertItemToEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_StudioM_IPAD_InsertItemToEstimate]
@estimaterevisonid			INT				,
@insertidstring				VARCHAR(MAX)	,
--@insertinclusionidstring	VARCHAR(MAX)	,
@insertpricestring			VARCHAR(MAX)	,
@insertqtystring			VARCHAR(MAX)	,
@insertinternaldescstring	VARCHAR(MAX)	,
@insertadditionalnotestring	VARCHAR(MAX)	,
@insertstudiomstring		VARCHAR(MAX)	,
@insertimageidstring		VARCHAR(MAX)	,
@sqsuserid					INT	
AS
BEGIN

	SET NOCOUNT ON;
      
       DECLARE @index INT, @total INT, @brandid INT, @tempestimateid INT, @temphomeid INT, @storey INT
       DECLARE @tempinsertid INT
       --DECLARE @tempinclusionid INT
       DECLARE @tempprice	 DECIMAL(18,2)
       DECLARE @tempqty		 DECIMAL(18,2)
       DECLARE @tempdesc	 VARCHAR(MAX)
       DECLARE @tempstudiom	 VARCHAR(MAX)
       DECLARE @tempaddinote VARCHAR(MAX)
       DECLARE @tempimageid INT
       DECLARE @areasortorder INT, @groupsortorder INT, @productsortorder INT , @areaid INT, @groupid INT, @productID VARCHAR(50)
       
       SELECT	@tempestimateid=fkidestimate 
       FROM		tbl_SalesEstimate_EstimateHeader 
       WHERE	id_SalesEstimate_EstimateHeader=@estimaterevisonid
       
       SELECT	@temphomeid=homeid 
       FROM		Estimate 
       WHERE	estimateid=@tempestimateid
        
       SELECT		@brandid=brandid , 
                    @storey=Stories
       FROM			home 
       WHERE		homeid =@temphomeid

       SET @index=1

       SELECT IDENTITY( INT ) AS tid, data  , 0 AS areasortorder, 0 AS groupsortorder, 0 AS productsortorder
       INTO #tempid  
       FROM dbo.Splitfunction_string_to_table(@insertidstring,',') order by outputid

       --SELECT IDENTITY( INT ) AS tid, data  
       --INTO #tempinclusionid  
       --FROM dbo.Splitfunction_string_to_table(@insertinclusionidstring,',')
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempprice  
       FROM dbo.Splitfunction_string_to_table(@insertpricestring,',') order by outputid       
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempqty  
       FROM dbo.Splitfunction_string_to_table(@insertqtystring,',') order by outputid 
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempdesc  
       FROM dbo.Splitfunction_string_to_table(@insertinternaldescstring,'^') order by outputid     
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempstudiom  
       FROM dbo.Splitfunction_string_to_table(@insertstudiomstring,'^') order by outputid       

       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempadditionalnote  
       FROM dbo.Splitfunction_string_to_table(@insertadditionalnotestring,'^') order by outputid   
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempimageid  
       FROM dbo.Splitfunction_string_to_table(@insertimageidstring,',') order by outputid          

-- get all estimate details includes the inclusion
	   SELECT		*
	   INTO			#ed
	   FROM			Estimatedetails ed
	   WHERE		estimateid =@tempestimateid
	   

-- loop to get item to insert
       SELECT @total=count(tid) FROM #tempid
       
       WHILE (@index<=@total)
           BEGIN
              SELECT @tempinsertid=data		FROM #tempid			WHERE tid=@index
              --SELECT @tempinclusionid=data	FROM #tempinclusionid	WHERE tid=@index
              SELECT @tempprice=data		FROM #tempprice			WHERE tid=@index
              SELECT @tempqty=data			FROM #tempqty			WHERE tid=@index
              SELECT @tempdesc=data			FROM #tempdesc			WHERE tid=@index
              SELECT @tempstudiom=data		FROM #tempstudiom		WHERE tid=@index
              SELECT @tempaddinote=data		FROM #tempadditionalnote WHERE tid=@index
              SELECT @tempimageid=data		FROM #tempimageid		WHERE tid=@index
  -- find the area group and product sortorder
			   SELECT 
                    @areaid=areaid,
                    @groupid=groupid,
                    @productID=ProductID 
               FROM  EstimateDetails
               WHERE EstimateDetailsID=@tempinsertid
               
				SELECT te.*, ed.areaid, ed.groupid,ed.ProductID
				INTO   #tempMRSED
				FROM   (SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE  fkid_SalesEstimate_EstimateHeader=@estimaterevisonid) te
				INNER JOIN EstimateDetails ed ON te.fkidEstimateDetails=ed.EstimateDetailsID
				
               IF (EXISTS(SELECT * FROM #tempMRSED WHERE areaid=@areaid))
                  BEGIN
                     SELECT TOP 1 @areasortorder=areasortorder FROM #tempMRSED WHERE areaid=@areaid
                  END
               ELSE
                  BEGIN
                     SELECT @areasortorder=CASE WHEN @storey=1 THEN SortOrder ELSE SortOrderDouble END
                     FROM Area 
                     WHERE AreaID=@areaid
                  END

               IF (EXISTS(SELECT * FROM #tempMRSED WHERE groupid=@groupid))
                  BEGIN
                     SELECT TOP 1 @groupsortorder=groupsortorder FROM #tempMRSED WHERE groupid=@groupid
                  END
               ELSE
                  BEGIN
                     SELECT @groupsortorder=sortorder
                     FROM [group]
                     WHERE groupid=@groupid
                  END                       

               IF (EXISTS(SELECT * FROM #tempMRSED WHERE productid=RTRIM(@productid)))
                  BEGIN
                     SELECT TOP 1 @productsortorder=productsortorder FROM #tempMRSED WHERE productid=RTRIM(@productid)
                  END
               ELSE
                  BEGIN
                     SELECT @productsortorder=sortorder
                     FROM [product]
                     WHERE productid=RTRIM(@productid)
                  END 							               
  -- end area group product sort order             
                       
              INSERT INTO	tbl_SalesEstimate_EstimateDetails
							([fkid_SalesEstimate_EstimateHeader]
						   ,[fkidEstimateDetails]
						   ,[fkidStandardInclusions]
						   ,[fkid_NonStandardArea]
						   ,[ItemPrice]
						   ,[Quantity]
						   ,[ProductDescription]
						   ,[ExtraDescription]
						   ,[InternalDescription]
						   ,[AdditionalInfo]
						   ,[CreatedOn]
						   ,[CreatedBy]
						   ,[ModifiedOn]
						   ,[ModifiedBy]
						   ,[ItemAccepted]
						   ,[SelectedImageID]
						   ,[StudioMAttributes]
						   ,[DerivedCost]
						   ,[CostExcGST]
						   ,[CostOverWriteBy]
						   ,[AreaSortOrder]
						   ,[GroupSortOrder]
						   ,[ProductSortOrder]
						   ,[fkidArea]
						   ,[AreaName]
						   ,[fkidGroup]
						   ,[GroupName]
						   ,[fkidProductAreaGroup]
						   ,[ProductName]
						   ,[IsPromotionProduct]						   
						   )            
              SELECT
						@estimaterevisonid	,
						@tempinsertid		,
						null				,
						null				,
						@tempprice			,
						@tempqty			,
						#ed.productdescription,
						#ed.enterdesc		,
						@tempdesc			,
						@tempaddinote		,
						GETDATE()			,
						@sqsuserid			,
						GETDATE()			,
						@sqsuserid			,
						null				,
						CASE WHEN @tempimageid = 0 THEN null ELSE @tempimageid END,
						@tempstudiom,
						NULL,
						NULL,
						NULL,
						@areasortorder,
						@groupsortorder,
						@productsortorder,
						areaid,
						areaname,
						groupid,
						groupname,
						productareagroupid,
						productname,
						promotionproduct		
              FROM      #ed
              WHERE		EstimateDetailsID=@tempinsertid	--AND fkidinclusion=@tempinclusionid
              --Make sure that the product has NOT been added
              AND EstimateDetailsID NOT IN (SELECT ISNULL(fkidEstimateDetails,0) FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisonid)
              
              --Remove product from removed products list (except NSR items because we want to keep track of all NSR inserts/deletes for Variation printing) 
              DELETE FROM tbl_SalesEstimate_RemovedItems WHERE fkidEstimateDetails = @tempinsertid AND fkidRevision = @estimaterevisonid AND 
				(NOT EXISTS (SELECT * FROM EstimateDetails WHERE EstimateDetailsID = fkidEstimateDetails AND areaid = 43))
              
              DROP TABLE #tempMRSED
                              
              SET @index=@index+1
           END 
		
		UPDATE tbl_SalesEstimate_EstimateHeader
		SET ModifiedBy = @sqsuserid, 
			ModifiedOn = GETDATE()
		WHERE id_SalesEstimate_EstimateHeader = @estimaterevisonid		
		
		DROP TABLE #tempid
		--DROP TABLE #tempinclusionid   
		DROP TABLE #tempprice
		DROP TABLE #tempqty
		DROP TABLE #tempdesc
		DROP TABLE #tempstudiom   

	SET NOCOUNT OFF;
END
GO