----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_StudioM_IPAD_GetProductsForEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_StudioM_IPAD_GetProductsForEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_StudioM_IPAD_GetProductsForEstimate] 
@estimaterevisionid int,
@selected int = 0,
@bytype int = 0,  --1 by category 2-- by area
@typeid int = 0   -- bytype=1 this is categoryid, bytype=2 this is areaid
AS
BEGIN

	SET NOCOUNT ON;
	
	DECLARE @sql VARCHAR(MAX), @sql2 VARCHAR(MAX)
    DECLARE @estimateid INT, @statusreason INT, @revisiontypeid INT   
    
	DECLARE @areatable TABLE
	(areaid INT)

	DECLARE @grouptable TABLE
	(groupid INT)
	
	DECLARE @tempTab TABLE
	(
	   id INT,
	   idtype VARCHAR(20)
	)	

    INSERT INTO @tempTab
    EXEC sp_SalesEstimate_GetExcludedAreaAndGroupForStudioMRevision @estimaterevisionid
		  		                 
    SELECT @estimateid=fkidestimate, @statusreason= sr.id_SalesEstimate_StatusReason, @revisiontypeid = h.fkid_SalesEstimate_RevisionType
    FROM tbl_SalesEstimate_EstimateHeader h
    INNER JOIN tbl_SalesEstimate_StatusReason sr ON h.fkid_SalesEstimate_RevisionType=sr.fkid_SalesEstimate_RevisionType and sr.fkid_SalesEstimate_Status=2
    WHERE id_SalesEstimate_EstimateHeader=@estimaterevisionid

	DECLARE @brand int, @storey int
	
	SELECT @brand = B.BrandID, @storey = B.Stories FROM dbo.Estimate A 
	INNER JOIN dbo.Home B ON A.HomeID = B.HomeID
	WHERE EstimateID = @estimateid

	DECLARE @stateid INT
	SELECT @stateid = fkStateID FROM tblRegionGroup rg 
	INNER JOIN tblPriceRegionGroupMapping rgm ON rg.idRegionGroup = rgm.fkidRegionGroup 
	INNER JOIN Region r ON rgm.fkRegionID = r.RegionID
	INNER JOIN Estimate e ON r.RegionID = e.RegionID WHERE e.EstimateID = @estimateid

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

		INSERT INTO @areatable (areaid) VALUES (43) --Add Non Standard Request Area

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
			WHERE  active=1				AND
			fkidarea IS NOT NULL		AND
			fkidarea<>0					AND
			excludedefinedareagroup=0   AND
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
			active=1					AND
			fkidgroup IS NOT NULL		AND
			fkidgroup<>0				AND
			excludedefinedareagroup=0	AND
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

	SELECT eds.*,CASE WHEN d.homeid IS NOT NULL 
		              THEN h.homename+' - Display at '+d.suburb
		              ELSE ''
		              END 
		         AS displayAt  
	into #ed from estimatedetails eds LEFT JOIN display d ON eds.homedisplayid=d.homeid
		  LEFT JOIN home h ON d.homeid=h.homeid WHERE estimateid = @estimateid and eds.active = 1

	SELECT * into #ed2 from tbl_SalesEstimate_EstimateDetails 
	WHERE fkid_SalesEstimate_EstimateHeader = @estimaterevisionid				  

	SELECT 
	id_SalesEstimate_EstimateDetails as estimaterevisiondetailsid,
	ed2.fkidestimatedetails, 
    ed2.fkidproductareagroup	as productareagroupid,
	0 as idstandardinclusions,
	ed.productid,
	ed2.productname,
	ed2.productdescription,
	ed2.fkidarea as areaid,
	ed2.areaname,
	ed2.fkidgroup as groupid,
	ed2.groupname,
	ed.displayAt,
	pc.productcategoryid,
	pc.productcategorydesc,
	ed.enterdesc,
	ed2.internaldescription,
	ed2.additionalinfo,
	ed.uom,
	1 AS selected,
	ed2.quantity,
	ed2.itemprice as sellprice,
	p.studiomqanda,
	ed2.studiomattributes,
	p.isstudiomproduct,
	pag.IsSiteWork,
	cast(ed.changeprice AS BIT) AS ItemAllowToChangePrice,
	cast(ed.changeqty AS BIT) AS ItemAllowToChangeQuantity,
	ISNULL(ed2.fkid_nonstandardarea,0) AS NonstandardAreaID,
	ISNULL(ed2.fkid_nonstandardgroup,0) AS NonstandardGroupID,
	ed2.selectedimageid,
	case when cast(p.studiomqanda as varchar(max)) like '%mandatory="1"%' then cast(1 as bit) else cast(0 as bit) end as StudioMAnswerMandatory,
	(SELECT COUNT(*) FROM tbl_StudioM_ProductImage WHERE fkidProduct = p.ProductID) as productimagecount,
	0 as retailclusterid,
	'' as retailclustername,
	--isnull(id_studiom_retailcluster,0) as retailclusterid,
	--isnull(retailclustername,'''') as retailclustername,
	isnull(g.StudioMSortOrder, 9999999) as StudioMSortOrder,
	case when @storey > 1 then a.SortOrderDouble else a.SortOrder end as AreaSortOrder,	
	0 as modifiedflag

	into #tempfinal
	from #ed2 ed2
	inner join #ed ed on ed2.fkidestimatedetails=ed.estimatedetailsid
	inner join product p on ed.productid=p.productid
	inner join [group] g on ed.groupid=g.groupid
	inner join area a on ed.AreaID = a.AreaID
	inner join ProductAreaGroup pag on pag.ProductAreaGroupID = ed.ProductAreaGroupID		  
	inner join tblproductcategory pc on p.fkproductcategoryid=productcategoryid
	--left join tbl_StudioM_RetailClusterGroupMapping rgm on ed.groupid=rgm.fkidgroup
	--left join tbl_StudioM_RetailCluster rc on rgm.fkidretailcluster=rc.id_studiom_retailcluster
	--WHERE rc.fkidState IS NULL --retail cluster may not be configured
	--OR rc.fkidState = @stateid

	UNION ALL

	SELECT
	0 as estimatedetailsid, 
	ed.estimatedetailsid as fkidestimatedetails,
    ed.productareagroupid,
	0 as idstandardinclusions,
	ed.productid,
	ed.productname,
	ed.productdescription,
	ed.areaid,
	ed.areaname,
	ed.groupid,
	ed.groupname,
	ed.displayAt,
	pc.productcategoryid,
	pc.productcategorydesc,
	ed.enterdesc,
	ed.internaldescription,
	ed.additionalinfo,
	ed.uom,
	0 AS selected,
	ed.quantity,
	ed.sellprice,
	p.studiomqanda,
	'' as studiomattributes,
	p.isstudiomproduct,
	pag.IsSiteWork,
	cast(ed.changeprice AS BIT) AS ItemAllowToChangePrice,
	cast(ed.changeqty AS BIT) AS ItemAllowToChangeQuantity,
	0 AS NonstandardAreaID,
	0 AS NonstandardGroupID,
	0,
	case when cast(p.studiomqanda as varchar(max)) like '%mandatory="1"%' then cast(1 as bit) else cast(0 as bit) end as StudioMAnswerMandatory,
	(SELECT COUNT(*) FROM tbl_StudioM_ProductImage WHERE fkidProduct = p.ProductID) as productimagecount,
	0 as retailclusterid,
	'' as retailclustername,
	--isnull(id_studiom_retailcluster,0) as retailclusterid,
	--isnull(retailclustername,'''') as retailclustername,
	isnull(g.StudioMSortOrder, 9999999) as StudioMSortOrder,
	case when @storey > 1 then a.SortOrderDouble else a.SortOrder end as AreaSortOrder,				      				      
	0 as modifiedflag				  
	from #ed ed
	inner join product p on ed.productid=p.productid
	inner join [group] g on ed.groupid=g.groupid
	inner join area a on ed.AreaID = a.AreaID
	inner join ProductAreaGroup pag on pag.ProductAreaGroupID = ed.ProductAreaGroupID
	inner join tblproductcategory pc on p.fkproductcategoryid=productcategoryid	
	--left join tbl_StudioM_RetailClusterGroupMapping rgm on ed.groupid=rgm.fkidgroup
	--left join tbl_StudioM_RetailCluster rc on rgm.fkidretailcluster=rc.id_studiom_retailcluster				  			  
	where ed.EstimateDetailsID not in (select fkidestimatedetails from #ed2 where fkidestimatedetails is not null)	
	AND (ed.areaid IN (SELECT areaid FROM @areatable) OR ed.groupid IN (SELECT groupid FROM @grouptable)) 
	--AND (rc.fkidState IS NULL --retail cluster may not be configured
	--OR rc.fkidState = @stateid)	  
	
	--Remove Products that should NOT be selectable in this revision
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionType WHERE id_SalesEstimate_RevisionType = @revisiontypeid AND ExclusiveProductId IS NOT NULL)
	BEGIN
		DELETE FROM #tempfinal WHERE areaid = 43 AND SUBSTRING(ProductID,2,10) NOT IN 
		(SELECT SUBSTRING(ExclusiveProductId,2,10) FROM tbl_SalesEstimate_RevisionType 
		WHERE id_SalesEstimate_RevisionType = @revisiontypeid 
		AND ExclusiveProductId IS NOT NULL) AND selected = 0	  
	END
	ELSE
	BEGIN
		DELETE FROM #tempfinal WHERE areaid = 43 AND SUBSTRING(ProductID,2,10) IN 
		(SELECT SUBSTRING(ExclusiveProductId,2,10) FROM tbl_SalesEstimate_RevisionType 
		WHERE ExclusiveProductId IS NOT NULL) AND selected = 0			  
	END 	
	
	UPDATE #tempfinal 
	SET 
	areaid = CASE WHEN NonstandardAreaID = 0 THEN #tempfinal.areaid ELSE NonstandardAreaID END, 
	groupid = CASE WHEN NonstandardGroupID = 0 THEN #tempfinal.groupid ELSE NonstandardGroupID END,
	AreaName = CASE WHEN NonstandardAreaID = 0 THEN
	(SELECT TOP 1 AreaName FROM Area WHERE AreaID = #tempfinal.areaid)
	else (SELECT TOP 1 AreaName FROM Area WHERE AreaID = NonstandardAreaID) END,
	GroupName = CASE WHEN NonstandardGroupID = 0 THEN
	(SELECT TOP 1 GroupName FROM [Group] WHERE GroupID = #tempfinal.groupid) 
	else (SELECT TOP 1 GroupName FROM [Group] WHERE GroupID = NonstandardGroupID) END,
	AreaSortOrder = CASE when @storey > 1 
	then (SELECT TOP 1 SortOrder FROM Area WHERE AreaID = CASE WHEN NonstandardAreaID = 0 THEN #tempfinal.areaid ELSE NonstandardAreaID END) 
	else (SELECT TOP 1 SortOrderDouble FROM Area WHERE AreaID = CASE WHEN NonstandardAreaID = 0 THEN #tempfinal.areaid ELSE NonstandardAreaID END) 
	END,				      				      
	StudioMSortOrder = ISNULL((SELECT TOP 1 ISNULL(StudioMSortOrder, 9999999) FROM [Group] WHERE GroupID = CASE WHEN NonstandardGroupID = 0 THEN #tempfinal.groupid ELSE NonstandardGroupID END),9999999),
	StudioMAnswerMandatory = 0,
	StudioMQAndA = '<Brands><Brand id="000" name="Metricon Homes Pty Ltd"><Questions><Question id="000" text="Studio M Clarification(Free Text)" type="Free Text" mandatory="0"><Answers><Answer id="0" text="Studio M Clarification" /></Answers></Question></Questions></Brand></Brands>'
	WHERE areaid = 43 --AND selected = 1
				  
	SELECT *, @estimateid AS estimateid, @statusreason AS statusreason FROM #tempfinal 
	WHERE (IsSiteWork = 0 OR selected = 1)
	ORDER BY selected DESC, StudioMSortOrder 
			  
	SET NOCOUNT OFF; 
END