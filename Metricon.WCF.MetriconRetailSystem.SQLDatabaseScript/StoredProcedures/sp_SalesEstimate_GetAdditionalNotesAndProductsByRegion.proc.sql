
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAdditionalNotesAndProductsByRegion]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAdditionalNotesAndProductsByRegion]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <21/09/2011>
-- Description:	<get additional notes and products>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAdditionalNotesAndProductsByRegion] 
@templatename		VARCHAR(200)=NULL,
@subregionid		INT=0,
@userid             INT,
@active				INT   -- 2--all, 1 active, 0 -- inactive
AS
BEGIN
	SET NOCOUNT ON;

        DECLARE	@sql VARCHAR(MAX)
        DECLARE @regionid INT, @groupid INT, @usersubregion INT
        
        SELECT  @usersubregion=us.fkidSubRegion
        FROM    tbluser u
        INNER JOIN tblUserSubRegionMapping us ON u.userid=us.fkiduser
        WHERE   u.userid=@userid
        
        
        SELECT  @groupid=GroupID
        FROM    tblSubRegion sr
        WHERE   sr.idSubRegion=@usersubregion
        
        SELECT idsubregion
        INTO   #tempsubregion
        FROM   tblSubRegion
        WHERE  GroupID=@groupid
        
        SELECT DISTINCT fkRegionID
        INTO   #temppriceregion
        FROM   tblSubRegionPriceRegionMapping 
        WHERE  fkidSubRegion IN (SELECT idsubregion FROM #tempsubregion)

		-- get note ttemplate and products


		SELECT		
                    pag.ProductID 
        INTO        #tempproduct
		FROM        tbl_SalesEstimate_NotesTemplate nh                       		
		INNER JOIN  tbl_SalesEstimate_NotesTemplateitems nd					ON  nh.id_SalesEstimate_NotesTemplate=nd.fkid_SalesEstimate_NotesTemplate
		INNER JOIN	productareagroup pag									ON	nd.fkidproductareagroup=pag.productareagroupid	
		
		select PriceID,p.ProductID,SellPrice,EffectiveDate,CreatedDate,RegionID,PromotionPrice , CostPrice
			into #tempPrice
			from price p 
			inner join #tempProduct tp on p.productid=tp.productid
		where p.active=1 and regionid in (SELECT fkRegionID FROM #temppriceregion) and effectivedate<getdate()

		select productid,max(effectivedate) as effectivedate, regionid into #price1
			from #tempPrice 
			group by regionid,productid
			having max(effectivedate)<getdate()

		select p1.productid,p1.effectivedate,max(price.createddate) as createddate, p1.regionid into #price2
			from #price1 p1 
			inner join #tempPrice price	on p1.productid=price.productid and p1.effectivedate=price.effectivedate and p1.regionid=Price.regionid
 			group by p1.regionid,p1.productid, p1.effectivedate

		select price.regionid, price.productid as productid, max(isnull(price.sellprice,0.0)) as sellprice, max(isnull(price.promotionprice,0.0)) as promotionprice,p2.effectivedate, MAX(ISNULL(Price.CostPrice,0)) AS CostPrice
			into #currentPrice
			from #price2 p2 
			inner join #tempPrice price on p2.productid=price.productid
				and p2.effectivedate=price.effectivedate and p2.createddate=price.createddate and p2.RegionID=price.regionid
			group by price.regionid,price.productid,p2.effectivedate	


        SET @sql=
        '       
		SELECT		
					id_SalesEstimate_NotesTemplate AS templateid,
					templatename,
					u.username AS ownername,
					sb.subregionname as regionname,
					a.areaid AS areaid,
					g.groupid AS groupid,
					isNULL(a.areaname,'''') AS areaname,
					ISNULL(g.groupname,'''') AS groupname,
					ISNULL(p.productid, '''') AS productid,
					CASE WHEN p.productid IS NOT NULL THEN
							p.productname+char(13)+''[''+p.productid+'']'' 
						 ELSE ''''
						 END
				    AS productname,
					ISNULL(p.productdescription,'''') AS productdescription,
					ISNULL(nd.extradescription,'''')	AS enterdesc,
					ISNULL(nd.internaldescription,'''')	AS internaldesc,
					ISNULL(nd.additionalinfo,'''') AS additionalinfo,
					ISNULL(nd.quantity,1)	AS Quantity,
					CAST(ISNULL(pr.promotionprice,0) AS DECIMAL(18,2))	AS sellprice,
					0	AS PromotionProduct,
					1	AS StandardOption,
					ISNULL(nd.fkidproductareagroup,0) AS productareagroupid,
					nh.active,
					case when ISNULL(pr.costprice,0)=0 
					     then 1  
					     else case when ISNULL(pr.costprice,0)>0 and p.minibillstart=1 
					               then 1
					               else case when ISNULL(pr.costprice,0)>0 and p.minibillstart=0
					                         then 0
					                         else 0
					                    end
					               end
					end
					as derivedcost,
					0 as costexcgst,
					CASE WHEN p.uom=''NT''
						 THEN 0
						 ELSE 1            
					END 		
					AS ChangeQty,
					CASE WHEN (p.uom=''NT'')
						 THEN 0
						 ELSE CASE WHEN pag.areaid=43
								   THEN 1
								   ELSE 0
							  END
					END     
					AS ChangePrice,
					0 AS changedisplaycode,
					0 AS changeproductstandarddescription,	
					nh.IsPrivate
		FROM		tbl_SalesEstimate_NotesTemplate nh
		INNER JOIN	tbluser u												ON	nh.fkidOwner=u.userid 
		LEFT JOIN	tblsubregion sb											ON  nh.fkidsubregion=sb.idsubregion
		LEFT JOIN   tblSubRegionPriceRegionMapping spm                      ON  sb.idsubregion=spm.fkidsubregion
		LEFT JOIN	tbl_SalesEstimate_NotesTemplateitems nd					ON	nh.id_SalesEstimate_NotesTemplate=nd.fkid_SalesEstimate_NotesTemplate AND nd.active=1 
		LEFT JOIN	productareagroup pag									ON	nd.fkidproductareagroup=pag.productareagroupid
		LEFT JOIN	area a													ON	pag.areaid=a.areaid
		LEFT JOIN  [group] g												ON	pag.groupid=g.groupid
		LEFT JOIN  product p												ON	pag.productid=p.productid
		LEFT JOIN #currentPrice pr                                          ON  pag.productid=pr.productid and pr.regionid=spm.fkregionid
		WHERE		nh.id_SalesEstimate_NotesTemplate > 0 AND (nh.fkidOwner = ' + CAST(@userid AS VARCHAR) + ' OR nh.IsPrivate = 0)'
		
		IF (@subregionid <>0)
			SET	@sql=@sql+' AND	nh.fkidsubregion='+CAST(@subregionid AS VARCHAR)
	    ELSE 
	        SET @sql=@sql+' AND	nh.fkidsubregion in (select idsubregion FROM #tempsubregion)'
			
		IF (@active <>2)
			SET	@sql=@sql+' AND	nh.active='+CAST(@active AS VARCHAR)			
			
		IF (@templatename IS NOT NULL AND RTRIM(@templatename)<>'')
			SET @sql=@sql+'	AND templatename like ''%'+@templatename+'%'''
					
		SET @sql=@sql + ' ORDER BY	nh.templatename, a.areaname, g.groupname,p.productname	'

        EXEC(@sql)
        
        DROP TABLE #tempsubregion
        DROP TABLE #tempPrice
        DROP TABLE #price1
        DROP TABLE #price2
        DROP TABLE #currentPrice

    SET NOCOUNT OFF
END

GO