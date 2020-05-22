 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateEstimateDetails]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateDetails]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateEstimateDetails]
	@revisionDetailsId int, 
	@price money, 
	@quantity decimal(18,2), 
	@productDescription varchar(max)=NULL,
	@extraDescription varchar(max)=NULL, 
	@internalDescription varchar(max)=NULL,
	@studioManswer varchar(max)=NULL, 
	@userId int,
	@itemaccepted int,
	@categoryid int,
	@groupid int,
	@pricedisplayid int,
	@applyanswertoallgroup int,
	@additionalnotes varchar(max)=NULL,
	@selectedimageid varchar(10),
	@issiteworkitem int,
	@cost VARCHAR(10)
AS
BEGIN

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT, @oldcost VARCHAR(10)

	DECLARE @revisionId INT, @pagid INT, @estimatedetailsid INT, @standardinclusionid INT, @estimateid INT, @revisionTypeId INT
    DECLARE @productid VARCHAR(30)
    DECLARE @oldqty DECIMAL(18,2), @oldprice  DECIMAL(18,2), @oldadddesc VARCHAR(MAX), @oldextradesc VARCHAR(MAX)
    DECLARE @oldNSRareaid INT, @oldNSRgroupid INT, @storey INT
    DECLARE @areasortorder INT, @groupsortorder INT, @productsortorder INT , @existsareaid INT, @existsgroupid INT, @existsproductID VARCHAR(50)
    DECLARE @areaname VARCHAR(100), @groupname VARCHAR(100)

	SELECT @revisionId = fkid_SalesEstimate_EstimateHeader,
	       @estimatedetailsid=fkidestimatedetails,
	       @standardinclusionid= fkidstandardinclusions,
	       @oldadddesc=ISNULL(AdditionalInfo,''),
	       @oldextradesc=ISNULL(ExtraDescription,''),
	       @oldNSRareaid=ISNULL(fkid_NonStandardArea,0),
	       @oldNSRgroupid=ISNULL(fkid_NonStandardGroup,0),
	       @oldprice=ISNULL(ItemPrice,0),
	       @oldqty=Quantity
	FROM tbl_SalesEstimate_EstimateDetails
	WHERE id_SalesEstimate_EstimateDetails = @revisionDetailsId 
	
	SELECT @estimateid=fkidestimate,
	       @revisionTypeId=fkid_SalesEstimate_RevisionType,
	       @storey=h.Stories
	FROM   tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
	INNER JOIN Home h ON e.HomeID=h.HomeID
	WHERE  id_SalesEstimate_EstimateHeader=@revisionId
	
	SELECT @oldcost=ISNULL(CAST(CostExcGST AS VARCHAR),'')
	FROM   tbl_SalesEstimate_EstimateDetails
	WHERE  id_SalesEstimate_EstimateDetails=@revisionDetailsId
 
	SELECT * INTO #ed
	FROM     estimatedetails
	WHERE    estimateid=@estimateid
-- sortorder 
    IF((ISNULL(@oldNSRareaid,0)<> @categoryid OR ISNULL(@oldNSRgroupid,0)<>@groupid))--only when NSR area and group changes
    BEGIN
			SELECT te.*, ed.areaid, ed.groupid,ed.ProductID
			INTO   #tempMRSED
			FROM   tbl_SalesEstimate_EstimateDetails te
			INNER JOIN EstimateDetails ed ON te.fkidEstimateDetails=ed.EstimateDetailsID
			WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid

			   IF (EXISTS(SELECT * FROM #tempMRSED WHERE areaid=@categoryid))
				  BEGIN
					 SELECT TOP 1 @areasortorder=areasortorder FROM #tempMRSED WHERE areaid=@categoryid
				  END
			   ELSE
				  BEGIN
					 SELECT @areasortorder=CASE WHEN @storey=1 THEN SortOrder ELSE SortOrderDouble END
					 FROM Area 
					 WHERE AreaID=@categoryid
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
     END
  -- end sortorder                        


	IF @categoryid = 0
	BEGIN
		SET @categoryid = NULL
	END
	ELSE
	BEGIN
		SET @areaname = (SELECT TOP 1 AreaName FROM Area WHERE AreaID = @categoryid)
	END
	
	IF @groupid = 0
	BEGIN
		SET @groupid = NULL
	END
	ELSE
	BEGIN
		SET @groupname = (SELECT TOP 1 GroupName FROM [Group] WHERE GroupID = @groupid)
	END

	IF @pricedisplayid = 0
	BEGIN
		SET @pricedisplayid = NULL
	END

	BEGIN TRY

		BEGIN TRANSACTION	

			UPDATE tbl_SalesEstimate_EstimateDetails
			SET ItemPrice = @price, 
				Quantity = @quantity,
				ProductDescription = @productDescription, 
				ExtraDescription = LTRIM(RTRIM(@extraDescription)),
				InternalDescription = LTRIM(RTRIM(@internalDescription)),
				additionalinfo= LTRIM(RTRIM(@additionalnotes)),
				--ItemAccepted=@itemaccepted,
				fkid_NonStandardArea=@categoryid,
				fkid_NonStandardGroup=@groupid,
				fkid_NonStandardPriceDisplayCode = @pricedisplayid,
	            studiomattributes=RTRIM(LTRIM(@studioManswer)),			
				ModifiedBy = @userId, 
				ModifiedOn = GETDATE(),
				SelectedImageID=@selectedimageid,
				changed=1,
				IsSiteWork=@issiteworkitem,
				AreaName = CASE WHEN (@categoryid IS NULL) THEN AreaName ELSE @areaname END,
				GroupName = CASE WHEN @groupid IS NULL THEN GroupName ELSE @groupname END
			WHERE id_SalesEstimate_EstimateDetails = @revisionDetailsId
-- update cost if cost change and remove derived cost flag

 
            IF(RTRIM(@cost)<>'')
               BEGIN
					IF(@oldcost<>@cost)
					   BEGIN
							UPDATE tbl_SalesEstimate_EstimateDetails
							SET  CostExcGST=@cost,
								 DerivedCost=0,
								 CostOverWriteBy=@userId
							WHERE id_SalesEstimate_EstimateDetails = @revisionDetailsId                 
					   END
               END
            ELSE
               BEGIN
					UPDATE tbl_SalesEstimate_EstimateDetails
					SET  CostExcGST=NULL,
						 DerivedCost=1,
						 CostOverWriteBy=NULL
					WHERE id_SalesEstimate_EstimateDetails = @revisionDetailsId                 
               END             
			UPDATE tbl_SalesEstimate_EstimateHeader
			SET ModifiedBy = @userId, 
				ModifiedOn = GETDATE()
			WHERE id_SalesEstimate_EstimateHeader = @revisionId	
			
			-- update studio m answers to all group 
			-- get pag first
			IF (@applyanswertoallgroup=1)
			   BEGIN
			       IF (@estimatedetailsid>0) -- this means it is standardoption
			           BEGIN
						   SELECT @pagid= productareagroupid
						   FROM	 estimatedetails
						   WHERE estimatedetailsid=@estimatedetailsid
			           END
			       ELSE
			           BEGIN
						   SELECT @pagid= productareagroupid
						   FROM	 tblstandardinclusions
						   WHERE idstandardinclusions=@standardinclusionid
			           END		
			           
						-- get all group/product
			            DECLARE @groupid2 INT
						SELECT @groupid2=groupid, @productid=productid FROM productareagroup WHERE productareagroupid=@pagid
						SELECT productareagroupid INTO #temppag
						FROM   productareagroup
						WHERE  groupid=@groupid2 AND productid=@productid AND active=1
			            
						-- update fileds for standard options
						UPDATE		tbl_SalesEstimate_EstimateDetails
						SET			studiomAttributes=	@studioManswer,
						            changed=1
						FROM		tbl_SalesEstimate_EstimateDetails sed
						INNER JOIN	#ed ed			ON	sed.fkidestimatedetails=ed.estimatedetailsid
						INNER JOIN  #temppag pag	ON	ed.productareagroupid=pag.productareagroupid
						WHERE fkid_SalesEstimate_EstimateHeader = @revisionId
						
						-- update fileds for standard inclusions
						--UPDATE		tbl_SalesEstimate_EstimateDetails
						--SET			studiomAttributes=	@studioManswer,
						--            changed=1
						--FROM		tbl_SalesEstimate_EstimateDetails sed
						--INNER JOIN	tblstandardinclusions si			ON	sed.fkidstandardinclusions=si.idstandardinclusions
						--INNER JOIN  #temppag pag	ON	si.productareagroupid=pag.productareagroupid						
						--WHERE fkid_SalesEstimate_EstimateHeader = @revisionId	           	       
			       
			   END		

   -- if qty price, additional notes or extra desc change update itemaccepted/salesestimatoracceptor column
               IF(@revisionTypeId=2)
                  BEGIN
 							UPDATE tbl_SalesEstimate_EstimateDetails
							SET ItemAccepted=@itemaccepted
							WHERE  id_SalesEstimate_EstimateDetails = @revisionDetailsId                    
                  END
               ELSE IF (@revisionTypeId IN (4,19,15,25))
                  BEGIN
 							UPDATE tbl_SalesEstimate_EstimateDetails
							SET SalesEstimatorAccepted=@itemaccepted
							WHERE  id_SalesEstimate_EstimateDetails = @revisionDetailsId                    
                  END               
               ELSE
                  BEGIN
						IF(LTRIM(RTRIM(@oldadddesc))<>RTRIM(LTRIM(ISNULL(@additionalnotes,''))) OR
						   LTRIM(RTRIM(@oldextradesc))<>RTRIM(LTRIM(ISNULL(@extraDescription,''))) OR
						   @oldNSRareaid<>@categoryid OR
						   @oldNSRgroupid<>@groupid OR
						   @oldprice<>@price OR
						   @oldqty<>@quantity
						   )       
						BEGIN
 							UPDATE tbl_SalesEstimate_EstimateDetails
							SET ItemAccepted=0,
								SalesEstimatorAccepted=0
							WHERE  id_SalesEstimate_EstimateDetails = @revisionDetailsId           
						END
                 END
              IF((ISNULL(@oldNSRareaid,0)<> @categoryid OR ISNULL(@oldNSRgroupid,0)<>@groupid))-- if NSR area changed, update areasortorder 
                 BEGIN
						UPDATE tbl_SalesEstimate_EstimateDetails
						SET AreaSortOrder=@areasortorder,
							GroupSortOrder=@groupsortorder
						WHERE  id_SalesEstimate_EstimateDetails = @revisionDetailsId  
                 END
		COMMIT

	END TRY

	BEGIN CATCH

		IF @@TRANCOUNT > 0
			ROLLBACK
		
		-- Raise an error
		SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY()
		RAISERROR(@ErrMsg, @ErrSeverity, 1)

	END CATCH
	
	DROP TABLE #ed

END
GO