----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CompareSaleEstimateDetails]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CompareSaleEstimateDetails]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_CompareSaleEstimateDetails]
	@estimateRevisionIdA int, @estimateRevisionIdB int
AS
BEGIN

	SET NOCOUNT ON;
    DECLARE @tempTab TABLE
    (
        edidA INT,
        pagidA INT,
        QuantityA DECIMAL(18,2),
        PriceA DECIMAL(18,2),
        UomA VARCHAR(20),
        ProductDescriptionA VARCHAR(MAX),
        ExtraDescriptionA VARCHAR(2000),
        InternalDescriptionA VARCHAR(2000),
        AdditionalInfoA VARCHAR(2000),
        StudioMAttributesA VARCHAR(MAX), 
        NonStandardAreaIdA INT,
        NonStandardGroupIdA INT,
        
        edidB	INT,
        pagidB INT,
        QuantityB DECIMAL(18,2),
        PriceB DECIMAL(18,2),
        UomB VARCHAR(20),
        ProductDescriptionB VARCHAR(MAX),
        ExtraDescriptionB VARCHAR(2000),
        InternalDescriptionB VARCHAR(2000), 
        AdditionalInfoB VARCHAR(2000),
        StudioMAttributesB VARCHAR(MAX),       
        NonStandardAreaIdB INT,
        NonStandardGroupIdB INT,
        
        ProductNameA VARCHAR(2000),
        ProductNameB VARCHAR(2000),
        ProductName VARCHAR(2000),
        AreaName VARCHAR(200),
        GroupName VARCHAR(200),
        ReasonA VARCHAR(2000),
        ReasonB VARCHAR(2000)
    )
    
	--Upgrade
    INSERT INTO @tempTab
	SELECT
	tablea.fkidEstimateDetails,
	TABLEA_pagid,
	TableA_Quantity AS QuantityA,  
	TableA_Price AS PriceA,
	TableA_UOM AS UomA,
    TableA_ProductDescription AS ProductDescriptionA,
	TableA_ExtraDescription AS ExtraDescriptionA,
	TableA_InternalDescription AS InternalDescriptionA,
	TableA_AdditionalInfo AS AdditionalInfoA,
	TableA_StudioMAttributes AS StudioMAttributesA,
	TableA_NonStandardArea AS NonStandardAreaIdA,
	TableA_NonStandardGroup AS NonStandardGroupIdA,


    tableb.fkidEstimateDetails,
    TABLEB_pagid,
	TableB_Quantity AS QuantityB,  
	TableB_Price AS PriceB,
	TableB_UOM AS UomB,
    TableB_ProductDescription AS ProductDescriptionB,
	TableB_ExtraDescription AS ExtraDescriptionB,
	TableB_InternalDescription AS InternalDescriptionB,
	TableB_AdditionalInfo AS AdditionalInfoB,
	TableB_StudioMAttributes AS StudioMAttributesB,	
	TableB_NonStandardArea AS NonStandardAreaIdB,
	TableB_NonStandardGroup AS NonStandardGroupIdB,

	CASE WHEN TableA_ProductName IS NULL THEN NULL
	ELSE TableA_ProductName + ' [' + TableA_ProductID + ']' END AS ProductNameA,

	CASE WHEN TableB_ProductName IS NULL THEN NULL
	ELSE TableB_ProductName + ' [' + TableB_ProductID + ']' END AS ProductNameB,

	CASE WHEN TableA_ProductName IS NULL THEN TableB_ProductName
	ELSE TableA_ProductName END AS ProductName,

	ISNULL(TableA_AreaName, TableB_AreaName) AS AreaName,
	ISNULL(TableA_GroupName, TableB_GroupName) AS GroupName,
	null,null	
	FROM

	(SELECT ED.Quantity AS TableA_Quantity, 
	ED.fkidproductareagroup AS TABLEA_pagid,
	ED.ItemPrice AS TableA_Price,
	ED.fkidEstimateDetails, 
	CASE WHEN A.AreaID IS NULL THEN D.AreaName ELSE A.AreaName END AS TableA_AreaName,
	CASE WHEN G.GroupID IS NULL THEN D.GroupName ELSE G.GroupName END AS TableA_GroupName,
	ED.ProductName AS TableA_ProductName,
	D.ProductID AS TableA_ProductID,
    ED.ProductDescription AS TableA_ProductDescription,
	ED.ExtraDescription AS TableA_ExtraDescription,
	ED.InternalDescription AS TableA_InternalDescription,
	ED.AdditionalInfo AS TableA_AdditionalInfo,
	ED.fkid_NonStandardArea AS TableA_NonStandardArea,
	ED.fkid_NonStandardGroup AS TableA_NonStandardGroup,
	CAST(ED.StudioMAttributes AS VARCHAR(MAX)) AS TableA_StudioMAttributes,
	D.UOM AS TableA_UOM

	FROM tbl_SalesEstimate_EstimateDetails ED
	INNER JOIN EstimateDetails D
	ON ED.fkidEstimateDetails = D.EstimateDetailsID

	LEFT JOIN Area A ON ED.fkid_NonStandardArea = A.AreaID
	LEFT JOIN [Group] G ON ED.fkid_NonStandardGroup = G.GroupID

	WHERE ED.fkid_SalesEstimate_EstimateHeader = @estimateRevisionIdA) TableA 

	FULL OUTER JOIN

	(SELECT ED.Quantity AS TableB_Quantity, 
	ED.fkidproductareagroup AS TABLEB_pagid,
	ED.ItemPrice AS TableB_Price,
	ED.fkidEstimateDetails,
	CASE WHEN A.AreaID IS NULL THEN D.AreaName ELSE A.AreaName END AS TableB_AreaName,
	CASE WHEN G.GroupID IS NULL THEN D.GroupName ELSE G.GroupName END AS TableB_GroupName,
	ED.ProductName AS TableB_ProductName,
	D.ProductID AS TableB_ProductID,
    ED.ProductDescription AS TableB_ProductDescription,
	ED.ExtraDescription AS TableB_ExtraDescription,
	ED.InternalDescription AS TableB_InternalDescription,
	ED.AdditionalInfo AS TableB_AdditionalInfo,
	ED.fkid_NonStandardArea AS TableB_NonStandardArea,
	ED.fkid_NonStandardGroup AS TableB_NonStandardGroup,	
	CAST(ED.StudioMAttributes AS VARCHAR(MAX)) AS TableB_StudioMAttributes,	
	D.UOM AS TableB_UOM

	FROM tbl_SalesEstimate_EstimateDetails ED
	INNER JOIN EstimateDetails D
	ON ED.fkidEstimateDetails = D.EstimateDetailsID
	
	LEFT JOIN Area A ON ED.fkid_NonStandardArea = A.AreaID
	LEFT JOIN [Group] G ON ED.fkid_NonStandardGroup = G.GroupID

	WHERE ED.fkid_SalesEstimate_EstimateHeader = @estimateRevisionIdB) TableB 

	ON TableA.TABLEA_pagid = TableB.TABLEB_pagid 
	AND TableA.TableA_AreaName = TableB.TableB_AreaName 
	AND TableA.TableA_GroupName = TableB.TableB_GroupName 

	ORDER BY AreaName, GroupName, ProductName

	-- get remove reason
	UPDATE @tempTab
	SET reasonA=r.reason
	FROM @tempTab t
	INNER JOIN tbl_salesestimate_removeditems r ON r.fkidrevision=@estimateRevisionIdA AND r.fkidestimatedetails=t.edidB 
	WHERE t.edidB<>0
	
	UPDATE @tempTab
	SET reasonA=r.reason
	FROM @tempTab t
	INNER JOIN tbl_salesestimate_removeditems r ON r.fkidrevision=@estimateRevisionIdA AND r.fkidproductareagroup=t.pagidB
	WHERE t.edidB=0	

	UPDATE @tempTab
	SET reasonB=r.reason
	FROM @tempTab t
	INNER JOIN tbl_salesestimate_removeditems r ON r.fkidrevision=@estimateRevisionIdB AND r.fkidestimatedetails=t.edidA 
	WHERE t.edidA<>0
	
	UPDATE @tempTab
	SET reasonB=r.reason
	FROM @tempTab t
	INNER JOIN tbl_salesestimate_removeditems r ON r.fkidrevision=@estimateRevisionIdB AND r.fkidproductareagroup=t.pagidA
	WHERE t.edidA=0		
	
	SELECT 
				QuantityA,
				PriceA,
				UomA,
				ProductDescriptionA,
				ExtraDescriptionA,
				InternalDescriptionA,
				AdditionalInfoA,
				StudioMAttributesA, 
				NonStandardAreaIdA,
				NonStandardGroupIdA,
		        
				QuantityB,
				PriceB,
				UomB,
				ProductDescriptionB,
				ExtraDescriptionB,
				InternalDescriptionB, 
				AdditionalInfoB,
				StudioMAttributesB,
				NonStandardAreaIdB,
				NonStandardGroupIdB,
		        
				ProductNameA,
				ProductNameB,
				ProductName,
				AreaName,
				GroupName,
				CASE WHEN ReasonA IS NOT NULL 
				     THEN ReasonA 
				     ELSE
				         CASE WHEN ReasonB IS NOT NULL THEN ReasonB ELSE '' END
				END AS reason
	FROM @tempTab

END

GO