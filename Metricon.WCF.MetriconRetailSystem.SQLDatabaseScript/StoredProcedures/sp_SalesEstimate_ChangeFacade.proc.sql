
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_ChangeFacade]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_ChangeFacade]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <13/02/2013>
-- Description:	<change facade>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_ChangeFacade] 
@revisionid			INT			,
@newfacadehomeid	INT			,
@date				VARCHAR(20)	,
@userid				INT
AS
BEGIN

	SET NOCOUNT ON;
	
	
	DECLARE @regionid INT, @stateid INT, @newestimateid INT, @packageid INT, @oldesimateid INT, @newrevisionid INT
	DECLARE @newfacadehomeprice DECIMAL(18,2)
	DECLARE @sessionid VARCHAR(50), @contractnumber VARCHAR(10), @customerid VARCHAR(10), @modifiedby VARCHAR(20)
	DECLARE @createdby VARCHAR(50), @state VARCHAR(10), @suburb VARCHAR(100)
	DECLARE @postcode VARCHAR(50), @accountid VARCHAR(50), @opportunityid VARCHAR(50), @contracttype VARCHAR(20)
	DECLARE @homepriceeffectivedate DATETIME
	
-- get necessary data to create new estimate	
	
	SET @modifiedby='MRS change facade'
	
	SELECT     
	           @regionid=e.regionid, 
	           @stateid=h.fkStateID,
	           @packageid=e.fkidpackage,
	           @sessionid=e.SessionId,
	           @contractnumber=e.BCContractNumber,
	           @customerid=e.BCCustomerID,
	           @createdby=e.BCSalesConsultant,
	           @state=sd.State,
	           @suburb=sd.Suburb,
	           @postcode=sd.PostCode,
	           @accountid=e.fkidAccount,
	           @opportunityid=e.fkidOpportunity,
	           @oldesimateid=e.EstimateID,
	           @contracttype=ISNULL(eh.ContractType,'PC')
	           
	FROM       tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN Estimate e		ON eh.fkidEstimate=e.EstimateID
	INNER JOIN Home h			ON e.HomeID=h.homeid
    INNER JOIN SiteDetails sd	ON e.fkidOpportunity=sd.fkidOpportunity
    WHERE eh.id_SalesEstimate_EstimateHeader=@revisionid

-- get old estimate details
    SELECT *
    INTO   #tempOldED
    FROM   EstimateDetails
    WHERE  EstimateID= @oldesimateid AND Selected=1   
  

-- create new estimate
BEGIN TRAN
 
    EXEC sp_InsertSelectedHomeTemplate  @newfacadehomeid,@regionid,@sessionid,@contractnumber,@customerid,@createdby,@state,@suburb,@postcode,@packageid,@accountid,@opportunityid,@newestimateid OUTPUT
 
-- update new estimate as change facade
    UPDATE Estimate
    SET    CreatedBy='MRS Change Facade', 
           fkidParentID=@oldesimateid,
           fkidParentDescID='CF',
           EstimateActualDate=GETDATE(),
           ExpiryDate=GETDATE()+15
    WHERE  EstimateID=@newestimateid
 -- get new estimate details and update new estimate details based on old estimate

    SELECT *
    INTO   #tempNewED
    FROM   EstimateDetails
    WHERE  EstimateID= @newestimateid
       
    UPDATE		#tempNewED
    SET			
			   [Quantity]=t2.[Quantity]
			  ,[SellPrice]=t2.[SellPrice]
			  ,[TotalPrice]=t2.[SellPrice]
			  ,[EnterDesc]=t2.[EnterDesc]
			  ,[PromotionProduct]=t2.[PromotionProduct]
			  ,[Selected]=t2.[Selected]
			  ,[nonstandardcatID]=t2.[nonstandardcatID]
			  ,[StandardPackageInclusion]=t2.[StandardPackageInclusion]
			  ,[InternalDescription]=t2.[InternalDescription]
			  ,[AdditionalInfo]=t2.[AdditionalInfo]   
			    
    FROM		#tempNewED t1
    INNER JOIN  #tempOldED t2	ON t1.productareagroupid=t2.productareagroupid AND t1.homedisplayid IS NULL AND t2.homedisplayid IS NULL
    
    -- update display home item
    UPDATE		#tempNewED
    SET			
			   [Quantity]=t2.[Quantity]
			  ,[SellPrice]=t2.[SellPrice]
			  ,[TotalPrice]=t2.[SellPrice]
			  ,[EnterDesc]=t2.[EnterDesc]
			  ,[PromotionProduct]=t2.[PromotionProduct]
			  ,[Selected]=t2.[Selected]
			  ,[nonstandardcatID]=t2.[nonstandardcatID]
			  ,[StandardPackageInclusion]=t2.[StandardPackageInclusion]
			  ,[InternalDescription]=t2.[InternalDescription]
			  ,[AdditionalInfo]=t2.[AdditionalInfo]   
			    
    FROM		#tempNewED t1
    INNER JOIN  #tempOldED t2	ON t1.productareagroupid=t2.productareagroupid AND t1.homedisplayid IS NOT NULL AND t2.homedisplayid IS NOT NULL AND  t1.homedisplayid=t2.homedisplayid   

    
    UPDATE		EstimateDetails
    SET
			   [Quantity]=t2.[Quantity]
			  ,[SellPrice]=t2.[SellPrice]
			  ,[TotalPrice]=t2.[SellPrice]
			  ,[EnterDesc]=t2.[EnterDesc]
			  ,[PromotionProduct]=t2.[PromotionProduct]
			  ,[Selected]=t2.[Selected]
			  ,[nonstandardcatID]=t2.[nonstandardcatID]
			  ,[StandardPackageInclusion]=t2.[StandardPackageInclusion]
			  ,[InternalDescription]=t2.[InternalDescription]
			  ,[AdditionalInfo]=t2.[AdditionalInfo]     
    FROM		EstimateDetails ed
    INNER JOIN  (SELECT * FROM #tempNewED WHERE selected=1) t2	ON ed.EstimateDetailsID=t2.EstimateDetailsID
     
-- copy MRS estimate and details

	SELECT TOP 1 @homepriceeffectivedate = EffectiveDate FROM Price 
	WHERE ProductID = (SELECT ProductID FROM Home WHERE HomeID = @newfacadehomeid) 
	AND EffectiveDate < GETDATE() 
	AND RegionID = @regionid ORDER BY EffectiveDate DESC

    SELECT      @newfacadehomeprice=HomeSellPrice
    FROM		Estimate
    WHERE       EstimateID=@newestimateid
 
    INSERT INTO		tbl_SalesEstimate_EstimateHeader
				(
					[fkidEstimate]
				   ,[RevisionNumber]
				   ,[fkid_SalesEstimate_RevisionType]
				   ,[fkidOwner]
				   ,[fkid_SalesEstimate_Status]
				   ,[fkid_SalesEstimate_StatusReason]
				   ,[fkid_SalesEstimate_DifficultyRating]
				   ,[HomePrice]
				   ,[HomePriceEffectiveDate]
				   ,[EffectiveDateChanged]
				   ,[EffectiveDateModifiedOn]
				   ,[EffectiveDateModifiedBy]
				   ,[Comments]
				   ,[CreatedOn]
				   ,[CreatedBy]
				   ,[ModifiedOn]
				   ,[ModifiedBy]
				   ,[CommencedOn]
				   ,[CommencedBy]
				   ,[DueDate]
				   ,[CommentModifiedDate]
				   ,[LockStatus]
				   ,[AppointmentDateTime]
				   ,[ContractType]
				   ,HomeDisplayName
				   ,fkid_SalesEstimate_MRSGroup
				   
				 )    
    SELECT
				    @newestimateid
				  ,1
				  ,[fkid_SalesEstimate_RevisionType]
				  ,[fkidOwner]
				  ,[fkid_SalesEstimate_Status]
				  ,[fkid_SalesEstimate_StatusReason]
				  ,[fkid_SalesEstimate_DifficultyRating]
				  ,@newfacadehomeprice
				  ,@homepriceeffectivedate
				  ,[EffectiveDateChanged]
				  ,[EffectiveDateModifiedOn]
				  ,[EffectiveDateModifiedBy]
				  ,[Comments]
				  ,GETDATE()
				  ,[CreatedBy]
				  ,[ModifiedOn]
				  ,[ModifiedBy]
				  ,[CommencedOn]
				  ,[CommencedBy]
				  ,[DueDate]
				  ,[CommentModifiedDate]
				  ,[LockStatus]
				  ,[AppointmentDateTime] 
				  ,@contracttype
				  ,HomeDisplayName
				  ,fkid_SalesEstimate_MRSGroup   
    FROM			tbl_SalesEstimate_EstimateHeader
    WHERE			id_SalesEstimate_EstimateHeader= @revisionid
    
    SELECT @newrevisionid=@@IDENTITY

 -- get selected item for new estimate details both displyitems or non display item
    SELECT         *
    INTO           #oldmrsestimatedetails
    FROM           tbl_SalesEstimate_EstimateDetails
    WHERE          fkid_SalesEstimate_EstimateHeader=@revisionid
     
 
    SELECT			ed1.*, ed3.estimatedetailsid AS newestimatedetailsid
    INTO			#tempED3
    FROM			#oldmrsestimatedetails ed1
    INNER JOIN      (SELECT EstimateDetailsID,ProductAreaGroupID,HomeDisplayID FROM EstimateDetails WHERE EstimateID = @oldesimateid) ed2 ON ed1.fkidEstimateDetails=ed2.EstimateDetailsID
    INNER JOIN      (SELECT * FROM #tempNewED) ed3 ON ed2.ProductAreaGroupID=ed3.productareagroupid AND ed2.HomeDisplayID IS NULL AND ed3.HomeDisplayID IS NULL
    
    SELECT			ed1.*, ed3.estimatedetailsid AS newestimatedetailsid
    INTO			#tempED3_1
    FROM			#oldmrsestimatedetails ed1
    INNER JOIN      (SELECT EstimateDetailsID,ProductAreaGroupID,HomeDisplayID FROM EstimateDetails WHERE EstimateID = @oldesimateid) ed2 ON ed1.fkidEstimateDetails=ed2.EstimateDetailsID
    INNER JOIN      (SELECT * FROM #tempNewED) ed3 ON ed2.ProductAreaGroupID=ed3.productareagroupid AND ed2.HomeDisplayID IS NOT NULL AND ed3.HomeDisplayID IS NOT NULL AND ed2.HomeDisplayID=ed3.HomeDisplayID    
    
    SELECT          *      
    INTO            #tempED3_2
    FROM            #tempED3
    
    UNION ALL
    
    SELECT          *
    FROM            #tempED3_1  


 --  create MRS estimate details
 
    INSERT INTO    tbl_SalesEstimate_EstimateDetails
			        (     
			            [fkid_SalesEstimate_EstimateHeader]
					   ,[fkidEstimateDetails]
					   ,[fkid_NonStandardArea]
					   ,[fkid_NonStandardGroup]
					   ,[ItemPrice]
					   ,[Quantity]
					   ,[ProductDescription]
					   ,[ExtraDescription]
					   ,[InternalDescription]
					   ,[CreatedOn]
					   ,[CreatedBy]
					   ,[ModifiedOn]
					   ,[ModifiedBy]
					   ,[ItemAccepted]
					   ,[StudioMAttributes]
					   ,[fkidStandardInclusions]
					   ,[AdditionalInfo]
					   ,[SelectedImageID]
					   ,[Changed]
					   ,[PreviousChanged]
					   ,[IsSiteWork]
					   ,DerivedCost
					   ,CostExcGST
					   ,CostOverWriteBy
					   ,AreaSortOrder
					   ,GroupSortOrder
					   ,ProductSortOrder
					   ,SalesEstimatorAccepted
					   ,fkid_NonStandardPriceDisplayCode
					   ,fkidArea
					   ,AreaName
					   ,fkidGroup
					   ,GroupName
					   ,fkidProductAreaGroup
					   ,ProductName
					   ,IsPromotionProduct
					 )    
    SELECT 
                   DISTINCT 
                   @newrevisionid,
                   t.newestimatedetailsid,
                   ed.fkid_NonStandardArea,
                   ed.fkid_NonStandardGroup,
                   ed.ItemPrice,
                   ed.Quantity,
                   ed.ProductDescription,
                   ed.ExtraDescription,
                   ed.InternalDescription,
                   GETDATE(),
                   ed.CreatedBy,
                   ed.ModifiedOn,
                   ed.ModifiedBy,
                   ed.ItemAccepted,
                   CASE WHEN ed.StudioMAttributes IS NULL THEN NULL ELSE  CAST(ed.StudioMAttributes AS VARCHAR(MAX)) END,
                   ed.fkidStandardInclusions,
                   ed.AdditionalInfo,
                   ed.SelectedImageID,
                   null,
                   null,
                   ISNULL(ed.IsSiteWork,0),
                   ed.DerivedCost,
                   ed.CostExcGST,
                   ed.CostOverWriteBy,
                   ed.AreaSortOrder,
                   ed.GroupSortOrder,
                   ed.ProductSortOrder,
                   ed.SalesEstimatorAccepted,
                   ed.fkid_NonStandardPriceDisplayCode
				   ,t.fkidArea
				   ,t.AreaName
				   ,t.fkidGroup
				   ,t.GroupName
				   ,t.fkidProductAreaGroup
				   ,t.ProductName
				   ,t.IsPromotionProduct                   
    FROM           tbl_SalesEstimate_EstimateDetails ed
    INNER JOIN     #tempED3_2 t			ON  ed.fkidEstimateDetails=t.fkidEstimateDetails
    WHERE          ed.fkid_SalesEstimate_EstimateHeader=@revisionid

	--If HIA Contract exists
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_CustomerDocument Doc 
		INNER JOIN tbl_SalesEstimate_EstimateHeader Hdr ON Doc.fkid_SalesEstimate_EstimateHeader = Hdr.id_SalesEstimate_EstimateHeader
		WHERE Hdr.fkidEstimate = @oldesimateid AND Hdr.fkid_SalesEstimate_Status = 2 AND Doc.Active = 1 AND Doc.DocumentType = 'Contract')
	BEGIN
		--Update Product Name of the new estimate to previous estimate if they are different
		UPDATE NEW SET NEW.ProductName = OLD.ProductName FROM EstimateDetails OLD INNER JOIN EstimateDetails NEW
		ON OLD.EstimateID = @oldesimateid AND NEW.EstimateID = @newestimateid AND OLD.ProductAreaGroupID = NEW.ProductAreaGroupID AND ISNULL(OLD.HomeDisplayID,0) = ISNULL(NEW.HomeDisplayID,0)
		INNER JOIN tbl_SalesEstimate_EstimateDetails NEW_ED ON NEW.EstimateDetailsID = NEW_ED.fkidEstimateDetails AND NEW_ED.fkid_SalesEstimate_EstimateHeader = @newrevisionid
		WHERE OLD.ProductName <> NEW.ProductName		
	END
	 

    
 -- if every thing is ok, deactivate the old MRS estimate   
    Update    tbl_SalesEstimate_EstimateHeader
    SET       fkid_SalesEstimate_Status=6  -- changed facade
    WHERE     id_SalesEstimate_EstimateHeader=  @revisionid
 -- log all the items not available in new estimate

    INSERT INTO    tbl_SalesEstimate_RemovedItems
			        (     
						[fkidRevision]
					   ,[fkidEstimateDetails]
					   ,[fkidStandardInclusions]
					   ,[fkidProductAreaGroup]
					   ,[RemovedDate]
					   ,[RemovedBy]
					   ,[Reason]
					 )    
    SELECT 
                   @newrevisionid,
                   ed.fkidEstimateDetails,
                   NULL,
                   ed2.ProductAreaGroupID,
                   GETDATE(),
                   @userid,
                   'Change facade-Not available in new facade.'
    FROM           tbl_SalesEstimate_EstimateDetails ed
    INNER JOIN     EstimateDetails ed2  ON  ed.fkidEstimateDetails=ed2.EstimateDetailsID
    LEFT JOIN      #tempED3_2 t			ON  ed.fkidEstimateDetails=t.fkidEstimateDetails
    WHERE          ed.fkid_SalesEstimate_EstimateHeader=@revisionid    AND
				   t.newestimatedetailsid IS NULL   
    
    --Integrate event to BC
    EXEC sp_SalesEstimate_CreateEstimateEventRegister 'changefacade', @newrevisionid, @userid
      

 IF (@@ERROR=0)
     COMMIT TRAN
 ELSE
     ROLLBACK
		
	SET NOCOUNT OFF;
END
GO