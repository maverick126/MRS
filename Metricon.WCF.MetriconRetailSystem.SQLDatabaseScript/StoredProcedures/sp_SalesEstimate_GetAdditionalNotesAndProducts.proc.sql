/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the problem to drop the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAdditionalNotesAndProducts]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAdditionalNotesAndProducts]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAdditionalNotesAndProducts] 
@revisionid		INT,
@userid			INT
AS
BEGIN
	SET NOCOUNT ON;

		DECLARE	@groupid	INT
		DECLARE	@estimateid	INT, @priceregionid INT, @revisiontypeid INT
		DECLARE @allowchangeprice INT, @allowchangeqty INT, @allowchangeDesc INT, @allowchangePriceDisplayCode INT

        EXEC [sp_SalesEstimate_GetPermissionForEditFields] @revisionId,@allowchangeprice OUTPUT,@allowchangeqty OUTPUT,@allowchangeDesc OUTPUT,@allowchangePriceDisplayCode OUTPUT
        -- get region id
        -- based on talk with vish on 20/03/2013, we show the template base on group level,that means everyone can see tempalte and add items from template to estimate if they are in same group
        -- for exmaple, SE mapped in metro north, will see all templates from metro south/north/central and can add item from them to his estimate.
		SELECT	 TOP  1	@groupid= sub.GroupID, @estimateid=estimateid, @priceregionid=spm.fkRegionID,
		             @revisiontypeid=eh.fkid_SalesEstimate_RevisionType
		FROM		tbl_SalesEstimate_Estimateheader eh
		INNER JOIN	estimate e									ON	eh.fkidestimate=e.estimateid
		INNER JOIN  tbluser u									ON	eh.fkidOwner=u.userid
		INNER JOIN  tblusersubregionmapping usm					ON  u.userid=usm.fkiduser
		INNER JOIN  tblSubRegion sub                            ON  usm.fkidSubRegion=sub.idSubRegion
		INNER JOIN  tblSubRegionPriceRegionMapping spm          ON  sub.idSubRegion=spm.fkidSubRegion
		WHERE		id_SalesEstimate_Estimateheader=@revisionid	
		
		SELECT idSubRegion, SubRegionName INTO #subregion FROM tblSubRegion WHERE GroupID=@groupid

		-- get all items of estimate
		SELECT		*		INTO #temped 
		FROM		estimatedetails
		WHERE		estimateid= @estimateid
		
		-- get all product for notes template and price
		SELECT		
                    pag.ProductID 
        INTO        #tempproduct
		FROM		tbl_SalesEstimate_NotesTemplateitems nd					
		INNER JOIN	productareagroup pag									ON	nd.fkidproductareagroup=pag.productareagroupid	
		
		select PriceID,p.ProductID,SellPrice,EffectiveDate,CreatedDate,RegionID,PromotionPrice , p.CostPrice
			into #tempPrice
			from price p 
			inner join #tempProduct tp on p.productid=tp.productid
		where p.active=1 and regionid=@priceregionid and effectivedate<getdate()

		select productid,max(effectivedate) as effectivedate into #price1
			from #tempPrice 
			group by productid
			having max(effectivedate)<getdate()

		select p1.productid,p1.effectivedate,max(price.createddate) as createddate into #price2
			from #price1 p1 
			inner join #tempPrice price	on p1.productid=price.productid and p1.effectivedate=price.effectivedate
 			group by p1.productid, p1.effectivedate

		select price.productid as productid, max(isnull(price.sellprice,0.0)) as sellprice, max(isnull(price.promotionprice,0.0)) as promotionprice,p2.effectivedate, MAX(ISNULL(Price.CostPrice,0)) AS CostPrice
			into #currentPrice
			from #price2 p2 
			inner join #tempPrice price on p2.productid=price.productid
				and p2.effectivedate=price.effectivedate and p2.createddate=price.createddate
			group by price.productid,p2.effectivedate		
		
		
		-- get note ttemplate and products

		SELECT		
					id_SalesEstimate_NotesTemplate AS templateid,
					templatename+' ('+sub.subregionname+')' AS templatename,
					a.areaname,
					g.groupname,
					p.productid,
					p.productname+char(13)+'['+p.productid+']' AS productname,
					p.productdescription,
					nd.ExtraDescription	AS enterdesc,
					nd.AdditionalInfo,
					nd.InternalDescription,
					1	AS Quantity,
					pr.promotionprice	AS sellprice,
					0	AS PromotionProduct,
					1	AS StandardOption,
					ISNULL(ed.estimatedetailsid,0) AS estimatedetailsid,
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
					ISNULL(pr.costprice,0) as costexcgst,
					CASE WHEN @allowchangeqty=1 AND @revisiontypeid IN (4,15, 19, 25) 
					     THEN 1
					     ELSE
							CASE WHEN p.uom='NT'
								 THEN 0
								 ELSE 1            
							END 	
						 END	
					AS ChangeQty,
					CASE WHEN @allowchangeprice=1 AND @revisiontypeid IN (4,15, 19, 25)
					     THEN 1
					     ELSE
							CASE WHEN (p.uom='NT')
								 THEN 0
								 ELSE CASE WHEN ed.areaid=43
										   THEN 1
										   ELSE ED.ChangePrice
									  END
						    END
					END     
					AS ChangePrice,	
					CASE WHEN @revisiontypeid IN (4,15, 19, 25) 
						 THEN @allowchangeDesc 
						 ELSE CASE WHEN pag.areaid=43
								   THEN 1
								   ELSE 0
						 END
					END
					AS changeproductstandarddescription,
					CASE WHEN @revisiontypeid IN (4,15, 19, 25) 
						 THEN @allowchangePriceDisplayCode 
						 ELSE 0
					END
					AS changedisplaycode													
					--@allowchangeqty AS ChangeQty,
					--@allowchangeprice AS ChangePrice,
					--@allowchangePriceDisplayCode AS changedisplaycode,
					--@allowchangeDesc AS changeproductstandarddescription					
		FROM		tbl_SalesEstimate_NotesTemplate nh
		LEFT JOIN	tbl_SalesEstimate_NotesTemplateitems nd					ON	nh.id_SalesEstimate_NotesTemplate=nd.fkid_SalesEstimate_NotesTemplate AND nd.active=1
		LEFT JOIN	#temped ed												ON	nd.fkidproductareagroup=ed.productareagroupid
		INNER JOIN	productareagroup pag									ON	nd.fkidproductareagroup=pag.productareagroupid
		INNER JOIN	area a													ON	pag.areaid=a.areaid
		INNER JOIN  [group] g												ON	pag.groupid=g.groupid
		INNER JOIN  product p												ON	pag.productid=p.productid
		INNER JOIN  #subregion sub                                          ON  nh.fkidSubRegion=sub.idsubregion
		INNER JOIN  #currentPrice pr                                        ON  pag.ProductID=pr.Productid
		WHERE		nh.active=1	AND (nh.fkidOwner = @userid OR nh.IsPrivate = 0)						
					
		ORDER BY	nh.templatename, a.areaname, g.groupname,p.productname	
			
        DROP TABLE #subregion
        
    SET NOCOUNT OFF
END

GO
