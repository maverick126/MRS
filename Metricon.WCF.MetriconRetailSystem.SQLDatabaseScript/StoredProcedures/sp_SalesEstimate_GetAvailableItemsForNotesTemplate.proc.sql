----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAvailableItemsForNotesTemplate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAvailableItemsForNotesTemplate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <10/09/2011>
-- Description:	<get all available options for notes template>
-- Notes: This were IDs on 01.05.2014 - 5 pm (a.areaid in (44,55,87,90,94,95,96,101))
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAvailableItemsForNotesTemplate]
@templateid		INT			,
@searchtext		VARCHAR(500)
AS
BEGIN
	SET NOCOUNT ON;
		-- the areas were in 44,55,85,86, 87,90,94,95,96,101,102,103,107,108,110
		-- from 06/01/2016 based on ticket req-1100, cahnge to not in 43
		DECLARE	@stateid	INT
		DECLARE	@sql		VARCHAR(MAX)
		
		SELECT		@stateid=rg.fkstateid 
		FROM		tbl_SalesEstimate_NotesTemplate nt
		INNER JOIN	tblsubregionpriceregionmapping spm			ON	nt.fkidsubregion=spm.fkidsubregion
		INNER JOIN	region r									ON	spm.fkregionid=r.regionid
		INNER JOIN	tblpriceregiongroupmapping rgm				ON	r.regionid=rgm.fkregionid
		INNER JOIN	tblregiongroup rg							ON	rgm.fkidregiongroup=rg.idregiongroup
		WHERE       nt.id_SalesEstimate_NotesTemplate=@templateid

		SELECT *	INTO	#temp
		FROM		tbl_SalesEstimate_NotesTemplateItems
		WHERE		fkid_SalesEstimate_NotesTemplate=@templateid	AND
		            active=1
		
        SET @sql=
		'
		SELECT 
					pag.productareagroupid,
					a.areaname,
					g.groupname,
					p.productid,
					p.productname+char(13)+''[''+p.productid+'']''	AS productname,
					p.productdescription
					
		FROM		Productareagroup pag
		INNER JOIN	product p		ON		pag.productid=p.productid
		INNER JOIN	area	a		ON		pag.areaid=a.areaid
		INNER JOIN	[group]	g		ON		pag.groupid=g.groupid
		LEFT JOIN	#temp	t		ON		pag.productareagroupid=t.fkidproductareagroup
		WHERE		fkidproductareagroup IS NULL		AND
					pag.active=1						AND
					a.active=1							AND
					p.active=1							AND
					g.active=1							AND
					p.fkstateid='+CAST(@stateid AS VARCHAR)+' AND
					(a.areaid not in (43) )
	   '
	   
	   IF (@searchtext<>'' AND @searchtext IS NOT NULL)
			SET	@sql=@sql+' AND (p.productname like ''%'+@searchtext+'%'' OR p.productdescription like ''%'+@searchtext+'%'' OR p.productID like ''%'+@searchtext+'%'')'
			
					
       SET @sql=@sql+' ORDER BY		a.areaname, g.groupname,p.Productid,p.productname '
       
       EXEC(@sql)
       
	SET NOCOUNT OFF;
END
GO