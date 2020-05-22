----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_StudioM_IPAD_UpdateItemInEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_StudioM_IPAD_UpdateItemInEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_StudioM_IPAD_UpdateItemInEstimate]
@estimaterevisonid			INT				,
@updateidstring				VARCHAR(MAX)	,
@updatepricestring			VARCHAR(MAX)	,
@updateqtystring			VARCHAR(MAX)	,
@updateinternaldescstring	VARCHAR(MAX)	,
@updateadditionalnotestring	VARCHAR(MAX)	,
@updatestudiomstring		VARCHAR(MAX)	,
@updateimageidstring		VARCHAR(MAX)	,
@sqsuserid					INT	
AS
BEGIN

	SET NOCOUNT ON;
      
       DECLARE @index INT, @total INT, @brandid INT, @tempestimateid INT, @temphomeid INT, @storey INT
       DECLARE @tempupdateid INT
       DECLARE @tempprice	 DECIMAL(18,2)
       DECLARE @tempqty		 DECIMAL(18,2)
       DECLARE @tempdesc	 VARCHAR(MAX)
       DECLARE @tempaddinote VARCHAR(MAX)
       DECLARE @tempstudiom	 VARCHAR(MAX)
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

       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempid  
       FROM dbo.Splitfunction_string_to_table(@updateidstring,',') order by outputid
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempprice  
       FROM dbo.Splitfunction_string_to_table(@updatepricestring,',') order by outputid       
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempqty  
       FROM dbo.Splitfunction_string_to_table(@updateqtystring,',') order by outputid 
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempdesc  
       FROM dbo.Splitfunction_string_to_table(@updateinternaldescstring,'^') order by outputid     

       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempadditionalnote  
       FROM dbo.Splitfunction_string_to_table(@updateadditionalnotestring,'^') order by outputid
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempstudiom  
       FROM dbo.Splitfunction_string_to_table(@updatestudiomstring,'^') order by outputid       
       
       SELECT IDENTITY( INT ) AS tid, data  
       INTO #tempimageid  
       FROM dbo.Splitfunction_string_to_table(@updateimageidstring,',') order by outputid         
       
       --INSERT INTO temp_xml_eachfielddebug SELECT 'tempid', tid, data, GETDATE() FROM #tempid ORDER BY tid
       --INSERT INTO temp_xml_eachfielddebug SELECT 'tempprice', tid, data, GETDATE() FROM #tempprice ORDER BY tid 
       --INSERT INTO temp_xml_eachfielddebug SELECT 'tempqty', tid, data, GETDATE() FROM #tempqty ORDER BY tid 
       --INSERT INTO temp_xml_eachfielddebug SELECT 'tempdesc', tid, data, GETDATE() FROM #tempdesc ORDER BY tid 
       --INSERT INTO temp_xml_eachfielddebug SELECT 'tempadditionalnote', tid, data, GETDATE() FROM #tempadditionalnote ORDER BY tid 
       --INSERT INTO temp_xml_eachfielddebug SELECT 'tempstudiom', tid, data, GETDATE() FROM #tempstudiom ORDER BY tid 
       --INSERT INTO temp_xml_eachfielddebug SELECT 'tempimageid', tid, data, GETDATE() FROM #tempimageid ORDER BY tid  
       
       SELECT @total=count(tid) FROM #tempid
       
       WHILE (@index<=@total)
           BEGIN
              SELECT @tempupdateid=data FROM #tempid		WHERE tid=@index
              SELECT @tempprice=data	FROM #tempprice		WHERE tid=@index
              SELECT @tempqty=data		FROM #tempqty		WHERE tid=@index
              SELECT @tempdesc=data		FROM #tempdesc		WHERE tid=@index
              SELECT @tempaddinote=data	FROM #tempadditionalnote	WHERE tid=@index
              SELECT @tempstudiom=data	FROM #tempstudiom	WHERE tid=@index
              SELECT @tempimageid=data	FROM #tempimageid	WHERE tid=@index
			IF EXISTS (SELECT id_SalesEstimate_EstimateDetails FROM tbl_SalesEstimate_EstimateDetails WHERE fkidEstimateDetails = @tempupdateid)
			BEGIN
						--insert into temp_xml_insertupdate
						--select @tempupdateid,@tempstudiom			
			
              UPDATE	tbl_SalesEstimate_EstimateDetails
              SET
						itemprice			=	@tempprice	,
						quantity			=	@tempqty	,
						internaldescription	=	@tempdesc	,
						AdditionalInfo		=	@tempaddinote,
						studiomattributes	=	dbo.EscapeSpecialCharaceterWhenCreateXML(REPLACE(@tempstudiom,'&','&amp;')),
						SelectedImageID		=	@tempimageid,
						modifiedon			=	GETDATE()	,
						modifiedBy			=	@sqsuserid	
              WHERE		fkid_SalesEstimate_EstimateHeader = @estimaterevisonid AND
						fkidEstimateDetails = @tempupdateid
						

			END
			ELSE
			BEGIN
			
			
  -- find the area group and product sortorder
			   SELECT 
                    @areaid=areaid,
                    @groupid=groupid,
                    @productID=ProductID 
               FROM  EstimateDetails
               WHERE EstimateDetailsID=@tempupdateid
               
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
						EstimateDetailsID	,
						null				,
						null				,
						@tempprice			,
						@tempqty			,
						productdescription	,
						enterdesc			,
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
						AreaName,
						groupid,
						GroupName,
						ProductAreaGroupID,
						ProductName,
						PromotionProduct		
              FROM      EstimateDetails
              WHERE		EstimateDetailsID=@tempupdateid
              --Make sure that the product has NOT been added
              AND EstimateDetailsID NOT IN (SELECT ISNULL(fkidEstimateDetails,0) FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisonid)
              
              --Remove product from removed products list
              DELETE FROM tbl_SalesEstimate_RemovedItems WHERE fkidEstimateDetails = @tempupdateid AND fkidRevision = @estimaterevisonid AND 
				(NOT EXISTS (SELECT * FROM EstimateDetails WHERE EstimateDetailsID = fkidEstimateDetails AND areaid = 43))
              
			  DROP TABLE #tempMRSED
			END
              SET @index=@index+1
           END 
           
		UPDATE tbl_SalesEstimate_EstimateHeader
		SET ModifiedBy = @sqsuserid, 
			ModifiedOn = GETDATE()
		WHERE id_SalesEstimate_EstimateHeader = @estimaterevisonid
         						
		DROP TABLE #tempid   
		DROP TABLE #tempprice
		DROP TABLE #tempqty
		DROP TABLE #tempdesc
		DROP TABLE #tempstudiom   

	SET NOCOUNT OFF;
END


GO