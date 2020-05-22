 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CreateEstimateFromOriginalEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CreateEstimateFromOriginalEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_CreateEstimateFromOriginalEstimate]
	-- Add the parameters for the stored procedure here
	@estimateId INT
AS
BEGIN
	DECLARE @revisionNumber INT
	DECLARE @homePrice MONEY
	DECLARE @effectiveDate DATETIME
	DECLARE @depositDate DATETIME
	DECLARE @revisionHeaderId INT
	DECLARE @previousRevisionHeaderId INT
	DECLARE @statusId INT
	DECLARE @revisionTypeId INT
	DECLARE @ownerId INT
	DECLARE @homeproductid VARCHAR(30)
	DECLARE @regionId INT
	DECLARE @brandid INT
	DECLARE @storey INT
	DECLARE @mrsgroupid INT
	DECLARE @contracttype VARCHAR(10)

	-- Error handling vars
	DECLARE @ErrMsg NVARCHAR(4000) 
	DECLARE @ErrSeverity INT
	
	DECLARE @currentPrice TABLE
	(
	   productid VARCHAR(50),
	   promotionprice DECIMAL(18,2),
	   effectivedate DATETIME,
	   costprice DECIMAL(18,2),
	   derivedcost INT,
	   realcost INT
	)

    INSERT INTO @currentPrice
    EXEC sp_SalesEstimate_GetItemCostPriceForEstimate @estimateid
    
	-- Select Sales Estimate from the original Sales Estimate table 
	SELECT 
		@homePrice = Est.HomeSellPrice,
		@depositDate = Dpst.DepositDate,
		@ownerId = Usr.userid,
		@regionId=est.regionid,
		@homeproductid=h1.ProductID,
		@brandid=h1.BrandID,
		@contracttype=ISNULL(cf.ExtRef,'PC'),
		@storey=h1.Stories,
		@mrsgroupid=r.MRSGroupID
	FROM Estimate Est         
	INNER JOIN DepositDetails Dpst ON Est.BCContractNumber = Dpst.BCContractNumber
	INNER JOIN tblSQSConfig cf ON cf.Code='SalesType' AND RTRIM(Dpst.DepositSaleType)=RTRIM(cf.CodeValue)
	INNER JOIN tbluser Usr ON Est.BCSalesConsultant = Usr.usercode 
	INNER JOIN Home h1 ON Est.HomeID=h1.HomeID
	INNER JOIN Region r ON Est.RegionID=r.RegionID
	WHERE Est.EstimateID = @estimateid AND cf.Text3 not like '%2'


	-- If the Sales Estimate exists
	IF @@ROWCOUNT > 0
	BEGIN
        SELECT TOP 1 @effectiveDate=effectiveDate
        FROM Price
        WHERE RegionID=@regionId AND ProductID=@homeproductid AND Active=1 AND EffectiveDate<=@depositDate
        ORDER BY EffectiveDate DESC

		-- Set Revision Number to 1
		SET @revisionNumber = 1
		
		-- Default Status to WIP
		SET @statusId = 1

		-- Default Revision Type to SC
		SET @revisionTypeId = 1

		INSERT INTO tbl_SalesEstimate_EstimateHeader
		(fkidEstimate, RevisionNumber, fkid_SalesEstimate_RevisionType, fkidOwner,
		fkid_SalesEstimate_Status, HomePrice, 
		HomePriceEffectiveDate, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, ContractType, fkid_salesestimate_MRSGroup)
		VALUES (@estimateId, @revisionNumber, @revisionTypeId, @ownerId, 
		@statusId, @homePrice, 
		@effectiveDate, GETDATE(), @ownerId, GETDATE(), @ownerId, @contracttype, @mrsgroupid)	

		SET @revisionHeaderId = @@IDENTITY

		INSERT INTO tbl_SalesEstimate_EstimateDetails 
		(fkid_SalesEstimate_EstimateHeader, fkidEstimateDetails,
		ItemPrice, Quantity, ProductDescription, ExtraDescription, InternalDescription,
		CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, fkid_NonStandardArea, AdditionalInfo, issitework,
		AreaSortOrder,GroupSortOrder,ProductSortOrder,DerivedCost, CostExcGST, CostOverWriteBy,
		fkidArea,AreaName,fkidGroup,GroupName,fkidProductAreaGroup,ProductName,IsPromotionProduct
		)
		
		SELECT @revisionHeaderId AS fkid_SalesEstimate_EstimateHeader, EstimateDetailsID,
		ed.SellPrice, Quantity, 
		CASE WHEN ed.areaid<>43 THEN ed.ProductDescription ELSE EnterDesc END, 
		CASE WHEN ed.areaid<>43 THEN EnterDesc ELSE '' END, 
		ed.InternalDescription,
		GETDATE(), @ownerId, GETDATE(), @ownerId, nonstandardcatID,ed.AdditionalInfo, ISNULL(pag.IsSiteWork,0),
		CASE WHEN ed.areaid<>43
		     THEN
				CASE WHEN @storey=1
					 THEN a.SortOrder
					 ELSE a.SortOrderDouble
			    END
			 ELSE
				CASE WHEN @storey=1
					 THEN a2.SortOrder
					 ELSE a2.SortOrderDouble
			    END			 
		END,
		g.SortOrder,
		pp.SortOrder,
		cp.derivedcost,
		CASE WHEN ed.areaid=43
		     THEN CASE WHEN ISNULL(cp.costprice,0)=0 
		               THEN NULL
		               ELSE cp.costprice
		          END
		     ELSE CASE WHEN pag.IsSiteWork=1 AND ed.ChangePrice=1
		               THEN CASE WHEN cp.costprice=0
		                         THEN NULL
		                         ELSE cp.costprice
		                    END
		               ELSE cp.costprice
		          END 
		END,
		null,
		ed.areaid,
		CASE WHEN a2.AreaName IS NULL THEN ed.AreaName ELSE a2.AreaName END,
		ed.groupid,
		ed.GroupName,
		ed.ProductAreaGroupID,
		ed.ProductName,
		ed.PromotionProduct
		FROM EstimateDetails ed
		INNER JOIN ProductAreaGroup pag ON ed.ProductAreaGroupID=pag.ProductAreaGroupID
		INNER JOIN product pp on ed.ProductID=pp.ProductID
		INNER JOIN Area a ON pag.AreaID=a.AreaID
		INNER JOIN [group] g ON pag.GroupID=g.GroupID
		INNER JOIN @currentPrice cp ON ed.ProductID=cp.productid
		LEFT JOIN  Area a2 ON ISNULL(ed.nonstandardcatID,0)=a2.AreaID
		WHERE EstimateID = @estimateId AND Selected = 1		

	END
	ELSE
	BEGIN

			SET @ErrMsg = 'The original Sales Estimate ' + CONVERT(NVARCHAR(50), @estimateId) + ' could not be found.'
			SET @ErrSeverity = 16
			RAISERROR(@ErrMsg, @ErrSeverity, 1)

	END	

END

GO