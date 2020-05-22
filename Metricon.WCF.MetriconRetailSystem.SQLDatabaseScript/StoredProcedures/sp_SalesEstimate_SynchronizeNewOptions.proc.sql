----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_SynchronizeNewOptions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_SynchronizeNewOptions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_SynchronizeNewOptions]
	@revisionid INT
AS

	Declare 
	    @SelectedHomeName varchar(250),
	    @estimateid int,
		@Displaycounter int,
		@BrandID int,
		@Stories int,
		@promotionID int,
		@Homeprice money,
		@BaseProductID varchar(250),
		@Deposited bit,
		@homeid int,
		@regionid int,
		@promotiontype int,
		@modifiedEstimate int,
		@estimateDate smalldatetime,
		@stateId int,
		@suburbid int,
	    @homeproductid VARCHAR(50) 
	
BEGIN
	SET NOCOUNT ON;
	set @promotionID = 0
	set @modifiedEstimate = 0
	DECLARE @derivedhomepercentage decimal(18,4),@deriveditempercentage decimal(18,4),@targetmargin decimal(18,4)
	DECLARE @previousrevisionid INT, @ismilestone INT
	
	SET @ismilestone=0


	-- get estimateid
	SELECT	@estimateid=fkidestimate
	FROM	tbl_SalesEstimate_Estimateheader
	WHERE	id_SalesEstimate_Estimateheader=@revisionid

    EXEC sp_SalesEstimate_GetDerivedPercentageForEstimate @estimateid,@derivedhomepercentage OUTPUT, @deriveditempercentage OUTPUT, @targetmargin OUTPUT
		
	-- get suburb id from sitedetails
	SELECT		TOP 1 @suburbid=idauspost
	FROM		estimate e
	INNER JOIN	sitedetails sd		ON	e.fkidopportunity=sd.fkidopportunity
	INNER JOIN  DepositDetails dp   ON  e.fkidOpportunity=dp.fkidOpportunity
	INNER JOIN	tblauspost ap		ON	sd.suburb=ap.suburb	AND	sd.postcode=ap.postcode	AND	sd.state=ap.state
	WHERE		e.estimateid=@estimateid


	select @homeid = e.homeid, @regionid = regionid, @estimateDate = EstimateDate , @homeproductid=h.ProductID, @BrandID=h.BrandID
	from estimate e
	inner join home h on e.HomeID=h.HomeID
	where estimateid = @estimateid

-- get new items without price
	
	--Get all items from SQS Estimate Details
	SELECT * INTO #newED
	FROM estimatedetails 
	WHERE estimateid=@estimateid

	--Get all items from Home Display Option table 
	SELECT * INTO #newHDO 
	FROM homedisplayoption
	WHERE homeid=@homeid AND active=1

	--Get selected items in current MRS revision
    SELECT med.*, ed.ProductID, 0 AS needupdate  
    INTO    #tempMRSED
    FROM    tbl_SalesEstimate_EstimateDetails med
    INNER JOIN EstimateDetails ed ON med.fkidEstimateDetails=ed.EstimateDetailsID
    WHERE   fkid_SalesEstimate_EstimateHeader=@revisionid	
 

	--Get items that are in Home Display Option but NOT in SQS Estimate Details (based on PAG ID and Home Display ID)
	SELECT distinct h.productareagroupid INTO #product
	FROM #newHDO h
	INNER JOIN ProductAreaGroup pag ON h.productareagroupid=pag.ProductAreaGroupID
	LEFT JOIN #newED e	ON h.productareagroupid = e.productareagroupid AND ISNULL(h.HomeDisplayId, 0) = ISNULL(e.HomeDisplayId, 0)
	WHERE e.estimatedetailsid IS NULL AND pag.Active = 1 

-- get previous estimate and check if it's accepted mile stone version 	

    SELECT 0 AS estimatedetailid INTO #temppreviousmilestoneestimate
    
    --Get a milestone revision (could be previous revision, PC, Contract, Variation or RSTM)
	EXEC sp_SalesEstimate_GetPreviousRevisonForVariation @revisionid, @previousrevisionid OUTPUT
	

	--If a milestone revision exists
	IF(EXISTS(
				SELECT * 
				FROM tbl_SalesEstimate_Estimateheader eh WHERE eh.fkid_SalesEstimate_Status=2 AND eh.id_SalesEstimate_EstimateHeader = @previousrevisionid
	       )
	  )
	  BEGIN
	       
		 --Get selected items (exclude NSR items) in milestone MRS revision
         SELECT ed.* 
         INTO  #selectedpreviousED
         FROM  (SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE  fkid_SalesEstimate_EstimateHeader=@previousrevisionid AND ISNULL(fkid_NonStandardArea,0)=0 ) e1
         INNER JOIN EstimateDetails ed ON e1.fkidEstimateDetails=ed.EstimateDetailsID

		 --Get selected items (exclude NSR items) in current MRS revision 
         SELECT ed.* 
         INTO  #selectedcurrentED
         FROM  (SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE  fkid_SalesEstimate_EstimateHeader=@revisionid AND ISNULL(fkid_NonStandardArea,0)=0 ) e1
         INNER JOIN EstimateDetails ed ON e1.fkidEstimateDetails=ed.EstimateDetailsID
         
         --Get items that are in BOTH milestone and current MRS revision (exclude NSR items)    
	     INSERT INTO #temppreviousmilestoneestimate
	     SELECT ed2.EstimateDetailsid
	     FROM   #selectedpreviousED ed1
	     INNER JOIN #selectedcurrentED ed2 ON ed1.productareagroupid=ed2.productareagroupid and ISNULL(ed1.homedisplayid,0)=ISNULL(ed2.homedisplayid,0)

	  END
	
	  DELETE FROM #temppreviousmilestoneestimate WHERE estimatedetailid=0
	  

-- end previous estimate

	--If there are new items in HDO table or there are items in current MRS revision
	IF (EXISTS(SELECT * FROM #product) OR EXISTS(SELECT * FROM #tempMRSED))
	BEGIN

				-- includes home products
                select ProductID,ProductName,ProductDescription,UOM,ModifiedDate
                into #tempProduct_21
                FROM product where ProductID=@homeproductid

                -- includes products in current MRS revision 
				insert into #tempProduct_21
                select DISTINCT p.ProductID,ed.ProductName,p.ProductDescription,UOM,ModifiedDate
                FROM   #tempMRSED	ed			   
			    INNER JOIN product p ON ed.ProductID=p.ProductID 

				-- includes new products in HDO table 
                insert into #tempProduct_21
				select distinct p2.ProductID,p2.ProductName,ProductDescription,UOM,p2.ModifiedDate
				from #product p
				inner join productareagroup pag on p.productareagroupid=pag.productareagroupid
				inner join product p2 on pag.productid=p2.productid
								
				-- remove possible duplication			   
				SELECT DISTINCT *
				INTO #tempProduct
				FROM #tempProduct_21		   
				
				--Get state of the estimate		   
				Select @stateId = rg.fkStateID
						from tblPriceRegionGroupMapping m 
						inner join tblRegionGroup rg on m.fkidRegionGroup=rg.idRegionGroup
					where m.fkRegionID = @regionid
			
				--Setup temp price table
			   SELECT CAST('' AS VARCHAR(50)) AS productid,
			          CAST(0  AS DECIMAL(18,4)) AS promotionprice,
			          GETDATE() AS effectivedate,
			          CAST(0 AS DECIMAL(18,4)) AS costprice,
			          0 AS derivedcost,
			          0 AS realcost
			   INTO #currentPrice
			   
			   DELETE FROM #currentPrice
			   
			   --Get cost and price of each products in SQS Estimate Details table
			   INSERT INTO #currentPrice
			   EXEC sp_SalesEstimate_GetItemCostPriceForEstimate @estimateid


-- This table for only vanila home not for display
				--Get all items in Home Display Option table for this home (only main home) 
				Select OptionID,HomeID,ProductAreaGroupID,StandardOption,GeneralOption,StandardInclusion
						,Quantity,Active,HomeDisplayID,ChangeQty,AddExtraDesc,EnterDesc,ChangePrice
					into #tempHdo 
					from homedisplayoption hdo where active=1 and  homeid = @homeid and hdo.homedisplayid is null
					
				--Get Home ID
				select HomeID, ProductID, HomeName, ParentHomeID, BrandID, Stories into #tempHome
					from home where active=1 and homeid is not null and homeid = @homeid

				--Get all PAG in SQS Estimate Details and HDO table
				select distinct pag.* into #tempPag
					from productareagroup pag 
					where pag.active=1 and (ProductAreaGroupID in (select productareagroupid from #newED) or ProductAreaGroupID in (select productareagroupid from #newHDO))
	
--step1: create temp tables for finding all new PAG
				--Get all active items in SQS Estimate Details table 
				select estimatedetailsid,estimateid,sellprice,quantity,totalprice
					,productareagroupid,productid,homedisplayid,areaid,groupid,Promotionproduct,selected,active
					,areaname,sortorder,groupname,productname,productdescription, 0 as IsModified
					into #tempEd from estimatedetails
					where estimateid = @estimateid and active = 1 --and (standardpackageinclusion = 1 or standardpackageinclusion is null)

				--step2: Get the HomeName,Brand,stories,homeprice for the HomeID
				Select @SelectedHomeName = h.HomeName,
				@BrandID = h.brandid,
				@Stories=h.stories,
				@Homeprice=price.promotionprice
				from #tempHome h 
				inner join #currentPrice price on h.productid = price.productid 

				--step2.1: get the homeid for the displayhome that match this home 
				select distinct(home.homeid) into #displayhomeid 
				from home, display
				where  
					home.parenthomeid is not null and home.homeid = display.homeid and home.active=1 and home.homename like 
					(rtrim(substring(@selectedhomename,1,isnull(charindex(' ', @selectedhomename, 1),0))) + ' '+
					substring(ltrim(substring(@selectedhomename,len(rtrim(substring(@selectedhomename,1,isnull(charindex(' ', @selectedhomename, 1),0))))+1,
					len(@selectedhomename))),1,isnull(charindex(' ',ltrim(substring(@selectedhomename,
					len(rtrim(substring(@selectedhomename,1,isnull(charindex(' ', @selectedhomename, 1),0))))+1,
					len(@selectedhomename))), 1),0))) + '%'
			
				-- Step 2.2: Check if there are display home for the selected
				select @displaycounter = count(homeid) from #displayhomeid 

				--step2.3: get the previous promotionid
				select @promotionID = e.promotionid , @promotiontype = promotiontypeid, @BaseProductID = p.baseproductid
					from estimate e inner join promotion p on e.promotionid = p.promotionid
					where e.estimateid = @estimateid

                select distinct ParentHomeID into #tempparenthomeid from Home where HomeID in (select distinct(homeid) from #displayhomeid)
			
				if @Displaycounter > 0
				begin
					--Get Display Home Options from HDO table
					Select OptionID,HomeID,ProductAreaGroupID,StandardOption,GeneralOption,StandardInclusion
							,Quantity,Active,HomeDisplayID,ChangeQty,AddExtraDesc,EnterDesc,ChangePrice
						into #tempHdoDisplay
						from homedisplayoption hdo where homeid in (select parenthomeid from #tempparenthomeid) and active=1 and  hdo.homedisplayid in (select distinct(homeid) from #displayhomeid)
				End

				-- Step 6: Finding all the new PAGs for vanilla home
				select distinct  pag.productid, pag.areaid, pag.groupid, thdo.*
					into #newpagtable
					from #tempHdo thdo
					left join #tempEd on thdo.productareagroupid = #tempEd.productareagroupid and #tempEd.HomeDisplayID is null
					inner join #tempPag pag on thdo.productareagroupid = pag.productareagroupid
					inner join #tempProduct p on p.productID=pag.productID
					inner join area a on a.areaID=pag.areaID
					inner join [group] g on g.groupID=pag.groupID
					where #tempEd.productareagroupid is null and a.active = 1 and g.active = 1

				--	go through all the new pags and insert into the estimatedetails table
				if @@rowcount > 0	-- If any new pag
				Begin
					SET NOCOUNT ON;
	 
					-- Step 6.1: No Display for the base Home
					-- Enter the details in the estimatedetails
					-- Print 'EstimateDetails - Insert - **STARTED** - Base Home'
					insert into [dbo].[estimatedetails]
					   ([estimateid],[optionid],[homeid],[homedisplayid],[productareagroupid],[areaname],[groupname]
					   ,[productid],[areaid],[groupid],[productname],[productdescription],[quantity],[sellprice],
						[totalprice],[standardinclusion]
					   ,[standardoption],[generaloption],[addextradesc],[enterdesc],[changeqty],[changeprice]
					   ,[uom],[sortorder],[active],[promotionproduct],[selected],[pricelocked])
					select 
						 @estimateid,np.optionid,np.homeid,np.homedisplayid,np.productareagroupid,a.areaname,g.groupname,
						 pag.productid, pag.areaid,pag.groupid,p.productname,p.productdescription, np.quantity,price.promotionprice,
						 np.quantity * price.promotionprice,np.standardinclusion,
						 np.standardoption, np.generaloption,np.addextradesc,np.enterdesc,np.changeqty,np.changeprice,
						 p.uom,a.sortorder,np.active,0,0 as selected,0
					from 
						#newpagtable np 
						inner join #tempPag pag on np.productareagroupid = pag.productareagroupid
						inner join area a on a.areaid = pag.areaid 
						inner join #tempProduct p on p.productid = pag.productid
						inner join [group] g on g.groupid = pag.groupid 
						inner join #currentPrice price on price.productid = p.productid
					where 
						a.active=1 and g.active=1 and np.active = 1 and np.homedisplayid is null


					-- Print 'EstimateDetails - Insert - **COMPLETED**'

					-- Step 6.2: for Display home
					if @Displaycounter >0
					begin
						-- Finding all the new PAGs for display homes
						select productareagroupid, homedisplayid into #displayED from #tempEd where homedisplayid is not null

						select pag.productid, pag.areaid, pag.groupid, a.areaname,a.sortorder,g.groupname
								,p.productname,p.productdescription,p.uom,thdo.*
							into #displaynewpagtable
							from #tempHdoDisplay thdo
							left join #displayED ded on thdo.productareagroupid = ded.productareagroupid and thdo.homedisplayid=ded.homedisplayid
							inner join #tempPag pag on thdo.productareagroupid = pag.productareagroupid
							inner join #tempProduct p on p.productID=pag.productID
							inner join area a on a.areaID=pag.areaID
							inner join [group] g on g.groupID=pag.groupID
							where ded.productareagroupid is null and a.active = 1 and g.active = 1

						if @@rowcount > 0
						Begin
							insert into [dbo].[estimatedetails]
								([estimateid],[optionid],[homeid],[homedisplayid],[productareagroupid],[areaname],[groupname],
								[productid],[areaid],[groupid],[productname],[productdescription],[quantity],[sellprice],
								[totalprice],[standardinclusion],[standardoption],[generaloption],[addextradesc],[enterdesc],
								[changeqty],[changeprice],[uom],[sortorder],[active],[promotionproduct],[selected],[pricelocked])
						   Select
								 @estimateid,np.optionid,np.homeid,np.homedisplayid,np.productareagroupid,np.areaname,np.groupname,
								 np.productid,np.areaid,np.groupid,np.productname,np.productdescription,np.quantity,price.promotionprice,
								 np.quantity * price.promotionprice,np.standardinclusion,
								 np.standardoption,np.generaloption,np.addextradesc,np.enterdesc,np.changeqty,np.changeprice,
								 np.uom,np.sortorder,np.active,0,0 as selected,0
						   from
								#displaynewpagtable np 
								inner join #currentPrice price on price.productid = np.productid

						End   
						drop table #tempHdoDisplay
						drop table #displayED

						-- Print 'EstimateDetails - Insert - **COMPLETED** - Base Home + Display'
					end -- End if @Displaycounter =0

				end -- End if new pag

-- update standard desc for originate estimate

    DECLARE @opendesc INT
    SET @opendesc=0
    
    IF (EXISTS(SELECT * FROM tbl_SalesEstimate_OpenEditModule WHERE fkidState=@stateId AND Active=1))
        BEGIN
			SELECT @opendesc= OpenProductStandardDescription 
			FROM tbl_SalesEstimate_OpenEditModule 
			WHERE fkidState=@stateId AND Active=1
        END

     IF(@opendesc=1)
        BEGIN
             --Get items that are in SQS Estimate but not in current MRS revision and not selected in SQS estimate
             SELECT ed.*, 0 AS needsync
             INTO #tempEDNeedSyncDesc 
             FROM #tempED ed
             LEFT JOIN  #tempMRSED ed2 ON ed.estimatedetailsid=ed2.fkidestimatedetails
             WHERE ed2.fkidestimatedetails IS NULL AND ed.selected=0
             
			 --Set needsync = 1 when product description has changed in SQS Admin 
             UPDATE #tempEDNeedSyncDesc
             SET  ProductDescription=p.ProductDescription,
                  needsync=1
             FROM #tempEDNeedSyncDesc ed
             INNER JOIN product p ON ed.productid=p.ProductID
             WHERE ed.ProductDescription<>p.ProductDescription
       
           --Update item description in SQS Estimate Details if the item is not in current MRS revision 
           --and not selected in SQS and not in milestone revision  
           IF (EXISTS(SELECT * FROM #tempEDNeedSyncDesc))
			BEGIN
			   UPDATE		EstimateDetails
			   SET          ProductDescription=ed2.ProductDescription				                
			   FROM         EstimateDetails ed
			   INNER JOIN   #tempEDNeedSyncDesc ed2 ON ed.EstimateDetailsID=ed2.EstimateDetailsID
			   WHERE ed.EstimateDetailsID NOT IN (Select EstimateDetailid FROM #temppreviousmilestoneestimate) AND ed2.needsync=1
			END   
	     END
	 ELSE
	     BEGIN          
-- end standard desc for originate estimate
-- update standard description/product name for MRS selected item
 -- this part was comment on 12/02/2015, details to check REQ-890

			   --Update selected items (non NSR) in current MRS revision if the description was updated in SQS Admin 
               UPDATE       #tempMRSED
               SET          ProductDescription=p.ProductDescription, 
                            needupdate=1
               FROM			#tempMRSED t1
               INNER JOIN	EstimateDetails ed	ON t1.fkidestimatedetails=ed.EstimateDetailsID
               INNER JOIN   product p			ON ed.ProductID=p.ProductID 
               WHERE        (t1.ProductDescription<>p.ProductDescription) AND ed.areaid<>43

               DELETE FROM  #tempMRSED WHERE needupdate=0
             
			   --Update product description in current MRS revision but not in a milestone revision
               IF (EXISTS(SELECT * FROM #tempMRSED))
				BEGIN
				   UPDATE		tbl_SalesEstimate_EstimateDetails
				   SET          ProductDescription=ed2.ProductDescription				                
				   FROM         tbl_SalesEstimate_EstimateDetails ed
				   INNER JOIN   #tempMRSED ed2 ON ed.id_SalesEstimate_EstimateDetails=ed2.id_SalesEstimate_EstimateDetails
				   WHERE ed.fkidEstimateDetails NOT IN (Select EstimateDetailid FROM #temppreviousmilestoneestimate)
				END
 
          END   
             
-- update all desc for non select item for originating estimate
               --Get items that are in SQS Estimate but not in current MRS revision and not selected in SQS estimate
  
               SELECT		ed.*, 0 AS needupdate 
               INTO			#tempNonSelected
               FROM			#newED ed
               LEFT JOIN	#tempMRSED ed2 ON ed.estimatedetailsid=ed2.fkidestimatedetails
               WHERE        ed2.fkidestimatedetails IS NULL AND ed.selected=0
               

			   --Update product description, internal description and additional notes if they have been updated in SQS Admin
               UPDATE       #tempNonSelected
               SET          ProductDescription=p.ProductDescription, 
                            InternalDescription=p.InternalDescription,
                            AdditionalInfo=p.AdditionalInfo,
                            needupdate=1
               FROM			#tempNonSelected t1
               INNER JOIN	EstimateDetails ed	ON t1.estimatedetailsid=ed.EstimateDetailsID
               INNER JOIN   product p			ON ed.ProductID=p.ProductID 
               WHERE        ISNULL(t1.ProductDescription,'')<>ISNULL(p.ProductDescription,'') OR
                            ISNULL(t1.AdditionalInfo,'')<>ISNULL(p.AdditionalInfo,'') OR
                            ISNULL(t1.InternalDescription,'')<>ISNULL(p.InternalDescription,'')
   
               --Update extra description if it's been updated in SQS Admin
               UPDATE       #tempNonSelected
               SET          enterdesc=hdo.EnterDesc, 
                            needupdate=1
               FROM			#tempNonSelected t1
               INNER JOIN	HomeDisplayOption hdo	ON t1.optionid=hdo.OptionID
               WHERE        ISNULL(t1.enterdesc,'')<>ISNULL(hdo.EnterDesc,'')
          
                
               DELETE FROM  #tempNonSelected WHERE needupdate=0
        
				--Update descriptions of items in SQS Estimate Details when the item is not in current MRS revision 
				--and not selected in SQS estimate and not in milestone revision
                IF (EXISTS(SELECT * FROM #tempNonSelected))
				BEGIN
				   UPDATE		EstimateDetails
				   SET          ProductDescription=ed2.ProductDescription,
				                InternalDescription=ed2.InternalDescription,
				                AdditionalInfo=ed2.AdditionalInfo,
				                EnterDesc=ed2.EnterDesc
				   FROM         EstimateDetails ed
				   INNER JOIN   #tempNonSelected ed2 ON ed.EstimateDetailsID=ed2.EstimateDetailsid
				   WHERE ed.EstimateDetailsID NOT IN (Select EstimateDetailid FROM #temppreviousmilestoneestimate)
				END              
       
				drop table #DisplayHomeid
				--drop table #tempPrice
				--drop table #price1
				--drop table #price2
				--drop table #currentPrice
				drop table #tempHdo
				drop table #tempProduct
				drop table #tempHome
				drop table #tempPag
				drop table #tempEd
				drop table #tempNonSelected
				drop table #tempMRSED

       END

	--Get all items from SQS Estimate Details after new items added
	SELECT * INTO #EstimateDetailsAfterSync
	FROM estimatedetails ed
	WHERE ed.estimateid=@estimateid

-- sync promotion product flag


 			SELECT      *
			INTO        #tempPP 
			FROM		PromotionProduct
			WHERE       fkidMultiplePromotion IN (SELECT mp.idMultiplePromotion
			FROM		#EstimateDetailsAfterSync ed
			INNER JOIN  tblMultiplePromotion mp ON ed.productid=mp.BaseProductID AND ed.selected=1 AND mp.fkidPromotionID=@promotionid)	
 
		    
			SELECT      ed.estimatedetailsid
			INTO        #tempedid
			FROM        #tempPP tp
			INNER JOIN  #EstimateDetailsAfterSync  ed  ON tp.pagid=ed.productareagroupid AND ed.promotionproduct=0 AND ed.HomeDisplayID IS NULL      
 
		    
			UPDATE EstimateDetails
			SET PromotionProduct=1
			WHERE EstimateID=@estimateid AND 
			      EstimateDetailsID IN (SELECT EstimateDetailsID FROM #tempedid) AND
			      EstimateDetailsID NOT IN (Select EstimateDetailid FROM #temppreviousmilestoneestimate)
	
-- sync standardoption/standardinclusion flag and area/group, product name

			UPDATE #newED
			SET    standardinclusion=hdo.standardinclusion,
			       standardoption=hdo.standardoption,
			       changeprice=HDO.changeprice
			FROM   #newED ed
			INNER JOIN #newHDO hdo ON ed.optionid=hdo.optionid


-- sync area gooup , merged above code to one.

			UPDATE #newED
			SET    productname=pp.ProductName,
			       areaid=pag.AreaID,
			       areaname=a.areaname,
			       groupid=pag.GroupID,
			       groupname=g.GroupName					        
			FROM   #newED ed
			INNER JOIN ProductAreaGroup pag ON ed.productareagroupid=pag.ProductAreaGroupID 
			INNER JOIN product pp ON pag.ProductID=pp.productid
			INNER JOIN Area a ON pag.AreaID=a.areaid
			INNER JOIN [Group] g ON pag.GroupID=g.groupid			
			WHERE ed.AreaID<>pag.AreaID OR ed.GroupID<>pag.groupid OR ed.productname<>pp.ProductName OR  ed.areaname<>a.AreaName OR ed.groupname<>g.GroupName		
		
			
			UPDATE EstimateDetails
			SET    standardinclusion=ed2.standardinclusion,
			       standardoption=ed2.standardoption,
			       areaid=ed2.AreaID,
			       areaname=ed2.areaname,
			       groupid=ed2.GroupID,
			       groupname=ed2.GroupName,
			       ProductName=ed2.productname,
			       ChangePrice=ed2.changeprice
			FROM   EstimateDetails ed
			INNER JOIN #newED ed2 ON ed.EstimateDetailsID=ed2.EstimateDetailsID	AND
			ed.EstimateDetailsID NOT IN (Select EstimateDetailid FROM #temppreviousmilestoneestimate)
	
			UPDATE tbl_SalesEstimate_EstimateDetails
			SET	fkidArea=ed2.AreaID,
			    AreaName=ed2.areaname,
			    fkidGroup=ed2.GroupID,
			    GroupName=ed2.GroupName,
			    ProductName=ed2.productname
			FROM tbl_SalesEstimate_EstimateDetails SED
			INNER JOIN #newED ed2 ON SED.fkidEstimateDetails = ed2.EstimateDetailsID AND
			SED.fkid_SalesEstimate_EstimateHeader = @revisionid AND 
			ed2.EstimateDetailsID NOT IN (Select EstimateDetailid FROM #temppreviousmilestoneestimate)
			AND SED.fkidArea <> 43
			
			UPDATE tbl_SalesEstimate_EstimateDetails
			SET	AreaName=A.AreaName,
			    GroupName=G.GroupName
			FROM tbl_SalesEstimate_EstimateDetails SED
			INNER JOIN #newED ed2 ON SED.fkidEstimateDetails = ed2.EstimateDetailsID AND
			SED.fkid_SalesEstimate_EstimateHeader = @revisionid AND SED.fkidArea = 43
			INNER JOIN Area A ON ISNULL(SED.fkid_NonStandardArea,0) = A.AreaID
			INNER JOIN [Group] G ON ISNULL(SED.fkid_NonStandardGroup,0) = G.GroupID

-- sync derivedcost flag and cost from product & price table to MRS estimate details table
               SELECT sed.* 
               INTO    #tempMRSEDCost
               FROM    tbl_SalesEstimate_EstimateDetails sed
               INNER JOIN EstimateDetails ed ON sed.fkidEstimateDetails=ed.EstimateDetailsID
               WHERE   fkid_SalesEstimate_EstimateHeader=@revisionid AND 
                       CostOverWriteBy IS NULL 
                       AND ed.areaid<>43
                       --AND  ((fkid_NonStandardArea IS NULL OR fkid_NonStandardArea=0) AND (fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0))
	
               IF(EXISTS(SELECT * FROM #tempMRSEDCost))
                  BEGIN
					   UPDATE #tempMRSEDCost 
					   SET  derivedcost= cp.derivedcost,
					        costexcGST= CASE WHEN t1.itemprice<0
		                                     THEN cp.costprice 
		                                     ELSE CASE WHEN  ISNULL(cp.costprice,0)=0 OR cp.costprice=t1.itemprice
		                                               THEN CASE WHEN pag.IsSiteWork=1 AND ed.ChangePrice=1 
		                                                         THEN NULL
		                                                         ELSE CAST((t1.itemprice/1.1)*(1-@deriveditempercentage) AS DECIMAL(18,2))
		                                                    END 
		                                               ELSE CASE WHEN (pag.IsSiteWork=1 AND ed.ChangePrice=1 AND cp.realcost=0) 
		                                                         THEN NULL 
		                                                         ELSE cp.costprice
		                                                    END 
		                                          END
		                                 END
					                         
					   FROM #tempMRSEDCost t1
					   INNER JOIN EstimateDetails ed ON t1.fkidestimatedetails=ed.EstimateDetailsID
					   INNER JOIN ProductAreaGroup pag ON ed.ProductAreaGroupID=pag.ProductAreaGroupID
					   INNER JOIN product p ON ed.ProductID=p.ProductID 
					   INNER JOIN #currentPrice cp ON p.ProductID=cp.productid

                       UPDATE tbl_SalesEstimate_EstimateDetails 
                       SET CostExcGST=t1.costexcGST,
                           DerivedCost=t1.derivedcost
                       FROM tbl_SalesEstimate_EstimateDetails ed
                       INNER JOIN #tempMRSEDCost t1 ON ed.id_SalesEstimate_EstimateDetails=t1.id_SalesEstimate_EstimateDetails
                       WHERE ed.fkid_SalesEstimate_EstimateHeader=@revisionid AND  
                             ed.fkidEstimateDetails NOT IN (Select EstimateDetailid FROM #temppreviousmilestoneestimate)
                             
             
                       
				  END

-- end sync derivedcost & cost				

			drop table #tempPP
			drop table #newED
			drop table #newHDO
			drop table #tempedid
			drop table #temppreviousmilestoneestimate
			drop table #currentPrice
			drop table #EstimateDetailsAfterSync
			drop table #tempEDNeedSyncDesc

	SET NOCOUNT OFF;
END


GO