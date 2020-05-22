----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetEstimateDetailsForVariation]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetEstimateDetailsForVariation]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER Procedure [dbo].[sp_SalesEstimate_GetEstimateDetailsForVariation]
	@revisionId int
as
BEGIN
	SET NOCOUNT ON

    DECLARE @estimateid INT
    DECLARE @homeid INT
    DECLARE @regiongroupid INT
	DECLARE @brandid INT
	DECLARE @homestories INT
	DECLARE @promotionid INT
	DECLARE @promotiontype INT
	DECLARE @revisiontypeid INT
	DECLARE @mrsgroupid INT
	DECLARE @contractno INT
    DECLARE @icon VARCHAR(20)


	SELECT @estimateid = fkidEstimate ,
	       @regiongroupid=rm.fkidregiongroup, 
	       @homeid=e.homeid, 
	       @revisiontypeid=fkid_SalesEstimate_RevisionType,
	       @mrsgroupid=r.MRSGroupID,
	       @contractno=e.BCContractNumber
	FROM tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN estimate e on eh.fkidestimate=e.estimateid
	INNER JOIN Region r on e.RegionID=r.RegionID
	INNER JOIN tblpriceregiongroupmapping rm on e.regionid=rm.fkregionid 
	WHERE id_SalesEstimate_EstimateHeader = @revisionId

	SELECT @homestories = stories, @brandid=brandid FROM home 
	WHERE homeID IN (SELECT homeID FROM estimate WHERE estimateID = @estimateid)

    SELECT @icon = text1 FROM tblsqsconfig 
	WHERE code = 'NonStandardOptionIcon' and active = 1

	SELECT @promotionid = promotionid FROM estimate WHERE estimateid = @estimateid

	SELECT @promotiontype=promotiontypeid FROM promotion WHERE promotionid = @promotionid

	SELECT * INTO #promoproduct FROM promotionproduct WHERE promotionid = @promotionid

	SELECT  DISTINCT (optionid), 
	        SED.fkidEstimateDetails,
			ed.homeid, 
			sed.areaname as orginalarea, 
			case when (sed.areaname='Non Standard Request' AND g2.GroupName IS NOT NULL) then g2.GroupName else sed.GroupName end as groupname, 
			p.productid,
			sed.productname,
			SED.productdescription,
			--CASE WHEN sed.additionalinfo IS NOT NULL AND sed.additionalinfo<>''
			--     THEN SED.productdescription+'<br>'+sed.additionalinfo
			--     ELSE SED.productdescription			                     
   --         END AS productdescription,
			case when sed.areaname<>'Non Standard Request' 
			     then case when sed.areaname<>'Area surcharge' 
			               then null 
			               else SED.fkid_NonStandardArea 
			          end 
			     else 
			 	      case when SED.fkid_NonStandardArea is not null 
			 	           then SED.fkid_NonStandardArea 
			 	           else 0 
			 	      end 
			end as nonstandardcatID,			
			
			SED.fkid_NonStandardGroup as nonstandardgroupID,			
			SED.quantity,
			CASE WHEN SED.IsPromotionProduct = 0 
				THEN CAST(ISNULL(SED.itemprice,0) AS DECIMAL(18,2)) 
				ELSE CAST(0 AS DECIMAL(8,2)) END 
				AS sellprice,
			CASE WHEN SED.IsPromotionProduct = 0 	
				THEN CAST((ISNULL(SED.quantity,1) * ISNULL(SED.itemprice,0)) AS DECIMAL(18,2)) 
				ELSE CAST(0 AS DECIMAL(8,2)) END 
				AS totalprice,
			productareagroupid,
			ed.standardinclusion,
			standardoption,
			addextradesc,
			SED.ExtraDescription as enterdesc,
			SED.InternalDescription as internaldesc,
			sed.additionalinfo,
			case when @homestories=1 then a.sortorder else a.sortorderdouble end as sortorder,
			g.sortorder as gsortorder,
			p.sortorder as psortorder,
			p.uom,
			sed.fkidArea as areaid,
			sed.fkidgroup as groupid,
			sed.IsPromotionProduct as PromotionProduct,
			case when a2.areaid is null 
			     then case when @homestories=1 then a.sortorder else a.sortorderdouble end
			     else case when @homestories=1 then a2.sortorder else a2.sortorderdouble end 
			end as catorder, 
			case when sed.areaname='Non Standard Request' then a2.areaname else sed.areaname end as areaname,
			h.homename+' - Display at '+dd.suburb as displayAt,
			case when sed.areaname<>'Non Standard Request' then '' else @icon end as icon,
			StandardPackageInclusion,
			case when g2.GroupID is null
				then g.sortorder else g2.sortorder
			end as gorder, 
			--g.sortorder as 'gorder',
			p.sortorder as porder,
			SED.ItemAccepted,
			p.isstudiomproduct,
			--do not print studio M attribute for STM-COL revision
			case when @revisiontypeid = 7 then '' else CAST(sed.studiomattributes AS VARCHAR(MAX)) end AS studiomattributes,
			ISNULL(sed.selectedimageid,'') AS selectedimageid,
            '                    ' as change,
            0 as fkidStandardInclusions,
            p.fkPriceDisplayCodeID AS productpricedisplaycode,
			SED.fkid_NonStandardPriceDisplayCode AS nonstandardpricedisplaycode,
            CAST('' AS VARCHAR(MAX)) AS   oldproductdescription, 
            CAST('' AS VARCHAR(MAX)) AS   oldadditionalinfo,
            CAST('' AS VARCHAR(MAX)) AS   oldextradesc  
			into #temp

	from tbl_SalesEstimate_EstimateDetails SED 
	inner join estimatedetails ed on SED.fkidEstimateDetails=ed.EstimateDetailsID
	inner join area a on ed.areaid=a.areaid
	inner join [group] g on ed.groupid=g.groupid
	inner join [product] p on ed.productid=p.productid 
    left join area a2 on sed.fkid_NonStandardArea=a2.areaid
    left join [Group] g2 on sed.fkid_NonStandardGroup= g2.GroupID
    left join display dd on ed.homedisplayid=dd.homeid
    left join home h on ed.homedisplayid=h.homeid
    left join #promoproduct pp on ed.productareagroupid=pp.pagid
	where SED.fkid_SalesEstimate_EstimateHeader = @revisionId

-- get previous version details

    DECLARE @previousrevisionid INT
	EXEC sp_SalesEstimate_GetPreviousRevisonForVariation @revisionId, @previousrevisionid OUTPUT

	DECLARE @variationstartrevisionid INT --The first revision of this variation process
	--Check if there is any rejected variations
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateid 
	AND id_SalesEstimate_EstimateHeader > @previousrevisionid
	AND id_SalesEstimate_EstimateHeader < @revisionId
	AND fkid_SalesEstimate_Status = 3)--Rejected
	BEGIN
		SELECT @variationstartrevisionid = MIN (id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateid AND 
		id_SalesEstimate_EstimateHeader > (SELECT MAX(id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader 
		WHERE fkidEstimate = @estimateid AND id_SalesEstimate_EstimateHeader < @revisionId AND fkid_SalesEstimate_Status = 3) 
	END
	ELSE
	BEGIN
		SELECT @variationstartrevisionid = MIN (id_SalesEstimate_EstimateHeader) FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate = @estimateid AND
		id_SalesEstimate_EstimateHeader > @previousrevisionid
	END
	
	DECLARE @deleteditems TABLE (estimatedetailsid INT, pagid INT)

	--Get all deleted NSR items since this variation process started		
	INSERT INTO @deleteditems SELECT DISTINCT fkidEstimateDetails, fkidProductAreaGroup 
	FROM tbl_SalesEstimate_RemovedItems Rmv INNER JOIN tbl_SalesEstimate_EstimateHeader Hdr 
	ON Rmv.fkidRevision = Hdr.id_SalesEstimate_EstimateHeader 
	INNER JOIN ProductAreaGroup Pag on Rmv.fkidProductAreaGroup = Pag.ProductAreaGroupID
	WHERE Hdr.fkidEstimate = @estimateid AND Pag.AreaID = 43 
	AND Hdr.id_SalesEstimate_EstimateHeader >= @variationstartrevisionid
	AND Hdr.id_SalesEstimate_EstimateHeader <= @revisionId
	
	--We only care about NSR items that have been added back in
	DELETE FROM @deleteditems WHERE estimatedetailsid NOT IN (SELECT ISNULL(fkidEstimateDetails,0) FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader = @revisionId)

    SELECT  SED.*, SED.fkidProductAreaGroup as ProductAreaGroupID, 
    CASE WHEN (fkid_NonStandardArea IS NULL OR fkid_NonStandardArea=0) THEN SED.fkidArea ELSE fkid_NonStandardArea END AS AreaId,
    CASE WHEN (fkid_NonStandardGroup IS NULL OR fkid_NonStandardGroup=0)  THEN SED.fkidGroup ELSE fkid_NonStandardGroup END AS GroupId
    INTO #templastED
    FROM tbl_SalesEstimate_EstimateDetails SED 
    --INNER JOIN EstimateDetails ED     ON SED.fkidEstimateDetails = ED.EstimateDetailsID
    WHERE fkid_SalesEstimate_EstimateHeader = @previousrevisionid AND fkidEstimateDetails IS NOT NULL AND fkidEstimateDetails > 0
    
    UPDATE #templastED SET ItemPrice = 0 WHERE IsPromotionProduct = 1
    
    -- get merged revision typeid
    
    SELECT id_SalesEstimate_RevisionType
    INTO   #mergedrevisiontype
    FROM   tbl_SalesEstimate_RevisionType
    WHERE  (ExcMRSGroupIDWhenSplit = CAST(@mrsgroupid AS VARCHAR(255))
    OR ExcMRSGroupIDWhenSplit LIKE '%,'+CAST(@mrsgroupid AS VARCHAR(255))+',%'
    OR ExcMRSGroupIDWhenSplit LIKE '%,'+CAST(@mrsgroupid AS VARCHAR(255)))
    
    SELECT Distinct fkidArea
    INTO #mergedarea 
    FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
    WHERE fkid_SalesEstimate_RevisionType IN (SELECT id_SalesEstimate_RevisionType FROM #mergedrevisiontype WHERE fkidArea IS NOT NULL)
    
    SELECT Distinct fkidGroup
    INTO #mergedgroup 
    FROM tbl_SalesEstimate_RevisionTypeAreaGroup 
    WHERE fkid_SalesEstimate_RevisionType IN (SELECT id_SalesEstimate_RevisionType FROM #mergedrevisiontype WHERE fkidGroup IS NOT NULL)
       
    
    -- end merged revision  
    
	-- For studio M revisions, compare only Areas and Groups that are applicable to that revision type
	IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE fkid_SalesEstimate_RevisionType = @revisiontypeid AND ExcludeDefinedAreaGroup = 1)
	BEGIN
		--Colour Selection (ExcludeDefinedAreaGroup = 1) 
		 
		DELETE FROM #templastED 
		WHERE AreaId IN (SELECT fkidArea FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE fkidArea IS NOT NULL) AND
		      Areaid NOT IN (SELECT fkidarea FROM #mergedarea)
		
		DELETE FROM #templastED 
		WHERE GroupId IN (SELECT fkidGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE fkidGroup IS NOT NULL) AND
		      Groupid NOT IN (SELECT fkidGroup FROM #mergedgroup )                 
	END
	ELSE IF EXISTS (SELECT * FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE fkid_SalesEstimate_RevisionType = @revisiontypeid)
	BEGIN
		--All other Studio M Revision Types except Colour Selection (ExcludeDefinedAreaGroup = 0)
		DELETE FROM #templastED WHERE AreaId NOT IN (SELECT fkidArea FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE fkidArea IS NOT NULL AND fkid_SalesEstimate_RevisionType = @revisiontypeid)
		DELETE FROM #templastED WHERE GroupId NOT IN (SELECT fkidGroup FROM tbl_SalesEstimate_RevisionTypeAreaGroup WHERE fkidGroup IS NOT NULL AND fkid_SalesEstimate_RevisionType = @revisiontypeid)	
	END

-- get all changes compare with previous revision
    UPDATE  #temp  
    SET change = CASE WHEN t2.fkidEstimateDetails IS NULL
                    THEN 'NEW'
                    ELSE CASE WHEN (t2.fkid_NonStandardArea<>t1.nonstandardcatID OR
                               t2.fkid_NonStandardGroup<>t1.nonstandardgroupID OR
                               t2.ItemPrice<>t1.sellprice OR 
                               t2.Quantity<>t1.Quantity OR
                               t2.additionalinfo<>t1.additionalinfo OR
                               t2.extradescription<>t1.enterdesc OR
                               t2.IsPromotionProduct<>t1.PromotionProduct
                               
                               )
                              THEN 'CHANGED'
                              ELSE ''
                         END
                    END,
             --oldproductdescription=
             --   CASE WHEN t2.productdescription<>REPLACE(REPLACE(t1.productdescription,'<br>',''),t1.additionalinfo,'') OR t2.additionalinfo<>t1.additionalinfo OR t2.extradescription<>t1.enterdesc
             --     THEN
             --       CASE WHEN t2.additionalinfo IS NOT NULL AND t2.additionalinfo<>''
             --       THEN CASE WHEN (t2.extradescription IS NOT NULL AND  t2.extradescription<>'' )
             --                 THEN t2.productdescription+'<br>'+t2.additionalinfo+'<br>'+t2.extradescription
             --                 ELSE t2.productdescription+'<br>'+t2.additionalinfo
             --            END
             --       ELSE CASE WHEN (t2.extradescription IS NOT NULL AND  t2.extradescription<>'' )
             --                 THEN t2.productdescription+'<br>'+t2.extradescription
             --                 ELSE t2.productdescription
             --            END
             --       END
             --    ELSE ''
             --   END ,
                
            oldproductdescription=
            CASE WHEN t2.productdescription<>t1.productdescription
                 THEN t2.productdescription
                 ELSE ''
            END,
            oldadditionalinfo=
            CASE WHEN ISNULL(t2.additionalinfo,'')<>ISNULL(t1.additionalinfo,'')
                 THEN ISNULL(t2.additionalinfo,'')
                 ELSE ''
            END,
            oldextradesc=
            CASE WHEN ISNULL(t2.extradescription,'')<>ISNULL(t1.enterdesc,'')
                 THEN ISNULL(t2.extradescription,'')
                 ELSE ''
            END,                                           
            Quantity= CASE WHEN t2.Quantity IS NULL 
                           THEN t1.Quantity 
                           ELSE
                                CASE WHEN  t2.Quantity<>t1.Quantity
                                     THEN  t1.Quantity-t2.Quantity
                                     ELSE  0--t1.Quantity
                                END
                      END,
            sellprice= CASE WHEN t2.ItemPrice IS NULL 
                            THEN CAST(t1.sellprice AS DECIMAL(18,2)) 
                            ELSE 
                                 CASE WHEN t1.sellprice<>t2.ItemPrice  
                                      THEN CAST(t1.sellprice-t2.ItemPrice AS DECIMAL(18,2))
                                      ELSE 0--CAST(t1.sellprice AS DECIMAL(18,2))
                                 END 
                       END,
            totalprice=CASE WHEN t2.Quantity IS NULL 
                            THEN CAST(t1.Quantity*t1.sellprice AS DECIMAL(18,2))
                            ELSE 
                                 CASE WHEN (t1.Quantity*t1.sellprice)<>(t2.Quantity*t2.ItemPrice)
                                      THEN CAST((t1.Quantity*t1.sellprice)-(t2.Quantity*t2.ItemPrice) AS DECIMAL(18,2)) 
                                      ELSE 0--CAST(t1.Quantity*t1.sellprice AS DECIMAL(18,2)) 
                                 END
                       END
    FROM	#temp t1
    LEFT JOIN #templastED t2 ON t1.ProductAreaGroupID =t2.ProductAreaGroupID AND t1.fkidEstimateDetails>0
    
 
    
   -- get studio m changes
    DECLARE @tempHIArevisionid INT
	SELECT a.fkidEstimate,
	       a.id_SalesEstimate_EstimateHeader, 
	       RevisionNumber,
	       fkid_SalesEstimate_RevisionType,
	       ISNULL(cd.DocumentType,'') AS DocumentType
	INTO  #temprevision
	FROM
	(SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate in (SELECT estimateid FROM Estimate WHERE BCContractNumber=@contractno)) a
	LEFT JOIN tbl_SalesEstimate_CustomerDocument cd ON a.id_SalesEstimate_EstimateHeader=cd.fkid_SalesEstimate_EstimateHeader
	WHERE cd.Active=1
	ORDER BY fkidEstimate, RevisionNumber
	  -- get hia revision
	SET @tempHIArevisionid=0
	IF(Exists(SELECT * FROM #temprevision WHERE DocumentType='Contract'))
	   SELECT @tempHIArevisionid=id_SalesEstimate_EstimateHeader FROM #temprevision WHERE DocumentType='Contract'
	   
    IF(@tempHIArevisionid>0 AND @revisionId>@tempHIArevisionid)-- only the revision after HIA contract, colur change are concider as change 
       BEGIN
			UPDATE  #temp  
			SET change = LTRIM(change+' STUDIOM'),
				productdescription='<b>Note:</b> Colour Selection updated as per customer request.<br>'+t1.productdescription
			FROM	#temp t1
			INNER JOIN #templastED t2 ON t1.ProductAreaGroupID =t2.ProductAreaGroupID AND (CAST(ISNULL(t1.studiomattributes,'') AS VARCHAR(MAX))<>CAST(ISNULL(t2.studiomattributes,'') AS VARCHAR(MAX)))
       END 
   -- end studio m changes

   -- get deleted items for SO
    insert into #temp
    select 
           DISTINCT (ed.optionid), 
	        t1.fkidEstimateDetails,
			ed.homeid, 
            t1.AreaName AS orginalarea, 
			t1.groupname, 
			p.productid,
			t1.productname,
			--CASE WHEN t1.additionalinfo IS NOT NULL AND t1.additionalinfo<>''
			--     THEN t1.productdescription+'<br>'+t1.additionalinfo
			--     ELSE t1.productdescription			                     
            --END AS productdescription,
            t1.productdescription,
			case when t1.areaname<>'Non Standard Request' then 
				case when t1.areaname<>'Area surcharge' then null else ed.nonstandardcatID end else 
				case when ed.nonstandardcatID is not null then ed.nonstandardcatID else 0 end 
			end as nonstandardcatID,
			t1.fkid_NonStandardGroup as nonstandardgroupID,		
			t1.quantity,
			CAST((ISNULL(t1.itemprice,0)*-1) AS DECIMAL(18,2))  AS sellprice,
			cast((-1*t1.quantity * ISNULL(t1.itemprice,0)) as decimal(18,2))	as totalprice,
			t1.fkidproductareagroup as productareagroupid,
			ed.standardinclusion,
			ed.standardoption,
			ed.addextradesc,
			t1.ExtraDescription as enterdesc,
			t1.InternalDescription as internaldesc,
			t1.additionalinfo,
			case when @homestories=1 then a.sortorder else a.sortorderdouble end as sortorder,
			g.sortorder as gsortorder,
			p.sortorder as psortorder,
			p.uom,
			t1.fkidarea as areaid,
			t1.fkidgroup as groupid,
			t1.ispromotionproduct as promotionproduct,
			case when a2.areaid is null 
			     then case when @homestories=1 then a.sortorder else a.sortorderdouble end
			     else case when @homestories=1 then a2.sortorder else a2.sortorderdouble end 
			end as catorder,
			t1.AreaName as areaname,
			--case when a2.areaid is not null then a2.areaname else a.areaname end as areaname,
			h.homename+' - Display at '+dd.suburb as displayAt,
			case when a.areaname<>'Non Standard Request' then '' else @icon end as icon,
			ed.StandardPackageInclusion, 
			g.sortorder as gorder,
			p.sortorder as porder,
			t1.ItemAccepted,
			p.isstudiomproduct,
			CAST(t1.studiomattributes AS VARCHAR(MAX)) AS studiomattributes,
			ISNULL(t1.selectedimageid,'') AS selectedimageid,
            'DELETED' as change,
            0 as fkidStandardInclusions,
            p.fkPriceDisplayCodeID AS productpricedisplaycode,
            p.fkPriceDisplayCodeID AS nonstandardpricedisplaycode,
            CAST('' AS VARCHAR(MAX)) AS   oldproductdescription, 
            CAST('' AS VARCHAR(MAX)) AS   oldadditionalinfo,
            CAST('' AS VARCHAR(MAX)) AS   oldextradesc   
    FROM	#templastED t1 
	inner join estimatedetails ed on t1.fkidEstimateDetails=ed.EstimateDetailsID
	inner join area a on t1.areaid=a.areaid
	inner join [group] g on t1.groupid=g.groupid
	inner join [product] p on ed.productid=p.productid 
    left join area a2 on t1.fkid_NonStandardArea=a2.areaid
    left join display dd on ed.homedisplayid=dd.homeid
    left join home h on ed.homedisplayid=h.homeid
    left join #promoproduct pp on ed.productareagroupid = pp.pagid
    LEFT JOIN #temp t2 ON t1.ProductAreaGroupID = t2.ProductAreaGroupID
    WHERE t2.fkidEstimateDetails IS NULL

	--Change NSR items that have been deleted and added back in from 'Changed' to 'New'
	UPDATE #temp SET change = 'NEW', 
		Quantity = SED.Quantity, 
		sellprice = SED.ItemPrice, 
		totalprice = CAST(SED.Quantity*SED.ItemPrice AS DECIMAL(18,2)),
		oldproductdescription = '',
		oldadditionalinfo = '',
		oldextradesc = ''
	FROM #temp TMP INNER JOIN @deleteditems DEL ON TMP.fkidEstimateDetails = DEL.estimatedetailsid
	LEFT JOIN #templastED t2 ON TMP.fkidEstimateDetails=t2.fkidEstimateDetails AND TMP.fkidEstimateDetails>0
	INNER JOIN tbl_SalesEstimate_EstimateDetails SED ON SED.fkidEstimateDetails = TMP.fkidEstimateDetails 
		AND SED.fkid_SalesEstimate_EstimateHeader = @revisionId
	WHERE TMP.change = 'CHANGED' AND (t2.fkid_NonStandardArea <> TMP.nonstandardcatID OR t2.fkid_NonStandardGroup <> TMP.nonstandardgroupID
	OR t2.additionalinfo<>TMP.additionalinfo OR t2.extradescription<>TMP.enterdesc)

	--Add NSR itmes that have been deleted and added back in as 'Deleted'
    insert into #temp
    select 
           DISTINCT (ed.optionid), 
	        t1.fkidEstimateDetails,
			ed.homeid, 
            t1.AreaName AS orginalarea, 
			t1.groupname, 
			p.productid,
			t1.productname,
			--CASE WHEN t1.additionalinfo IS NOT NULL AND t1.additionalinfo<>''
			--     THEN t1.productdescription+'<br>'+t1.additionalinfo
			--     ELSE t1.productdescription			                     
            --END AS productdescription,
            t1.productdescription,
			case when t1.areaname<>'Non Standard Request' then 
				case when t1.areaname<>'Area surcharge' then null else ed.nonstandardcatID end else 
				case when ed.nonstandardcatID is not null then ed.nonstandardcatID else 0 end 
			end as nonstandardcatID,
			
			t1.fkid_NonStandardGroup as nonstandardgroupID,		
			t1.quantity,
			CAST((ISNULL(t1.itemprice,0)*-1) AS DECIMAL(18,2))  AS sellprice,
			cast((-1*t1.quantity * ISNULL(t1.itemprice,0)) as decimal(18,2))	as totalprice,
			t1.fkidproductareagroup as productareagroupid,
			ed.standardinclusion,
			ed.standardoption,
			ed.addextradesc,
			t1.ExtraDescription as enterdesc,
			t1.InternalDescription as internaldesc,
			t1.additionalinfo,
			case when @homestories=1 then a.sortorder else a.sortorderdouble end as sortorder,
			g.sortorder as gsortorder,
			p.sortorder as psortorder,
			p.uom,
			t1.fkidarea as areaid,
			t1.fkidgroup as groupid,
			t1.ispromotionproduct as promotionproduct,
			case when a2.areaid is null 
			     then case when @homestories=1 then a.sortorder else a.sortorderdouble end
			     else case when @homestories=1 then a2.sortorder else a2.sortorderdouble end 
			end as catorder,
			case when a2.areaid is not null then a2.areaname else a.areaname end as areaname,
			h.homename+' - Display at '+dd.suburb as displayAt,
			case when a.areaname<>'Non Standard Request' then '' else @icon end as icon,
			ed.StandardPackageInclusion, 
			g.sortorder as gorder,
			p.sortorder as porder,
			t1.ItemAccepted,
			p.isstudiomproduct,
			CAST(t1.studiomattributes AS VARCHAR(MAX)) AS studiomattributes,
			ISNULL(t1.selectedimageid,'') AS selectedimageid,
            'DELETED' as change,
            0 as fkidStandardInclusions,
            p.fkPriceDisplayCodeID AS productpricedisplaycode,
            p.fkPriceDisplayCodeID AS nonstandardpricedisplaycode,
            CAST('' AS VARCHAR(MAX)) AS   oldproductdescription, 
            CAST('' AS VARCHAR(MAX)) AS   oldadditionalinfo,
            CAST('' AS VARCHAR(MAX)) AS   oldextradesc     
    FROM	#templastED t1
    inner join #temp tmp on t1.fkidEstimateDetails = tmp.fkidEstimateDetails 
    inner join @deleteditems del on t1.fkidEstimateDetails = del.estimatedetailsid
	inner join estimatedetails ed on t1.fkidEstimateDetails=ed.EstimateDetailsID
	inner join area a on t1.areaid=a.areaid
	inner join [group] g on t1.groupid=g.groupid
	inner join [product] p on ed.productid=p.productid 
    left join area a2 on t1.fkid_NonStandardArea=a2.areaid
    left join display dd on ed.homedisplayid=dd.homeid
    left join home h on ed.homedisplayid=h.homeid
    left join #promoproduct pp on ed.productareagroupid=pp.pagid
    WHERE tmp.change = 'NEW'


-- format product description
    update #temp
    set productdescription=REPLACE(productdescription,CHAR(13)+CHAR(10),'<br>'), 
        oldproductdescription=REPLACE(oldproductdescription,CHAR(13)+CHAR(10),'<br>'),     
        enterdesc=REPLACE(enterdesc,CHAR(13)+CHAR(10),'<br>'),
        internaldesc=REPLACE(internaldesc,CHAR(13)+CHAR(10),'<br>')

    update #temp
    set productdescription=REPLACE(productdescription,CHAR(13),'<br>'),
        oldproductdescription=REPLACE(oldproductdescription,CHAR(13),'<br>'),
        enterdesc=REPLACE(enterdesc,CHAR(13),'<br>'),
        internaldesc=REPLACE(internaldesc,CHAR(13),'<br>')
    
    update #temp
    set productdescription=REPLACE(productdescription,'’','&#8217;') ,
        enterdesc=REPLACE(enterdesc,'’','&#8217;'),
        internaldesc=REPLACE(internaldesc,'’','&#8217;')   
    
    update #temp
    set productdescription=REPLACE(productdescription,'•','&#8226;') ,
        enterdesc=REPLACE(enterdesc,'•','&#8226;') ,
        internaldesc=REPLACE(internaldesc,'•','&#8226;') 
        
    update #temp
    set productdescription=REPLACE(productdescription,'–','&#8211;') ,
        productname=REPLACE(productname,'–','&#8211;') ,
        enterdesc=REPLACE(enterdesc,'–','&#8211;') ,
        internaldesc=REPLACE(internaldesc,'–','&#8211;')         
 
    update #temp
    set selectedimageid=''
    where selectedimageid='0' 
 -- end format product desc   



-- Change of Facade Variation
	DECLARE @previousestimateid INT
	SELECT @previousestimateid = fkidEstimate FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @previousrevisionid

	IF @estimateid <> @previousestimateid
	BEGIN
		DECLARE @previoushomeprice DECIMAL, @newhomeprice DECIMAL, @pricediff DECIMAL
		SELECT @previoushomeprice = HomePrice FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @previousrevisionid
		SELECT @newhomeprice = HomePrice FROM tbl_SalesEstimate_EstimateHeader WHERE id_SalesEstimate_EstimateHeader = @revisionId
		SET @pricediff = @newhomeprice - @previoushomeprice
		
		DECLARE @previoushomename VARCHAR(250), @newhomename VARCHAR(205), @previoushomecode VARCHAR(250), @newhomecode VARCHAR(205)
		SELECT @previoushomename = H.HomeName, @previoushomecode = H.ProductID FROM Estimate E INNER JOIN Home H ON E.HomeID = H.HomeID WHERE E.EstimateID = @previousestimateid
		SELECT @newhomename = HomeName, @newhomecode = ProductID FROM Home WHERE HomeID = @homeid
		
		INSERT INTO #temp 
		VALUES (
			0, 
	        0,
			@homeid, 
            'Facade Change', 
			'Facade Change', 
			@previoushomecode,
			@previoushomename,
			@previoushomename,
			0, --nonstandardcatID
			0, --nonstandardgroupID		
			1, --quantity
			0, --sellprice
			0, --totalprice
			0, --productareagroupid
			0, --standardinclusion
			1, --standardoption
			'', --addextradesc
			'', --enterdesc
			'', --internaldesc
			'', --additionalinfo
			0, --sortorder
			0, --gsortorder
			0, --psortorder
			'IT', --uom
			0, --areaid
			0, --groupid
			0, --promotionproduct
			0, --catorder
			'Facade Change', --areaname
			'', --displayAt
			'', --icon
			0, --StandardPackageInclusion 
			0, --gorder,
			0, --porder
			1, --ItemAccepted
			0, --isstudiomproduct
			'', --studiomattributes
			'', --selectedimageid
            'DELETED',
            0, --fkidStandardInclusions
            10, --productpricedisplaycode,
            10, --nonstandardpricedisplaycode 	
            '',
            '',
            ''	
		)
		
		INSERT INTO #temp 
		VALUES (
			0, 
	        0,
			@homeid, 
            'Facade Change', 
			'Facade Change', 
			@newhomecode,
			@newhomename,
			'<b>NEW FACADE</b></br>' + @newhomename,
			0, --nonstandardcatID
			0, --nonstandardgroupID		
			1, --quantity
			@pricediff, --sellprice
			@pricediff, --totalprice
			0, --productareagroupid
			0, --standardinclusion
			1, --standardoption
			'', --addextradesc
			'', --enterdesc
			'', --internaldesc
			'', --additionalinfo
			0, --sortorder
			0, --gsortorder
			0, --psortorder
			'IT', --uom
			0, --areaid
			0, --groupid
			0, --promotionproduct
			0, --catorder
			'Facade Change', --areaname
			'', --displayAt
			'', --icon
			0, --StandardPackageInclusion 
			0, --gorder,
			1, --porder
			1, --ItemAccepted
			0, --isstudiomproduct
			'', --studiomattributes
			'', --selectedimageid
            'NEW',
            0, --fkidStandardInclusions
            10, --productpricedisplaycode,
            10, --nonstandardpricedisplaycode 
            ''	,
            '',
            ''	
		)		
	END 
-- End Change of Facade Variation


 
-- output
	SELECT t.*,
			  CASE WHEN nonstandardpricedisplaycode IS NOT NULL AND nonstandardpricedisplaycode <> 10 --NONE
				   THEN (SELECT TOP 1 UPPER(PriceDisplayDesc) FROM tblPriceDisplayCode WHERE PriceDisplayCodeID = nonstandardpricedisplaycode)
				   ELSE
					 CASE WHEN productpricedisplaycode IS NOT NULL AND productpricedisplaycode <> 10 --NONE
					 THEN (SELECT TOP 1 UPPER(PriceDisplayDesc) FROM tblPriceDisplayCode WHERE PriceDisplayCodeID = productpricedisplaycode)
					 ELSE			
					   CASE WHEN t.totalprice<0 
					   THEN '-$'+CAST(CAST(t.totalprice*-1 AS DECIMAL(18,2)) AS VARCHAR) 
					   ELSE 
						   CASE WHEN t.totalprice=0
								THEN ' -- '
								ELSE '$'+CAST(t.totalprice AS VARCHAR) 
						   END
					   END 
				   END
			   END
		   AS printprice,
		   pim.[Image] 
	from #temp t
	left join tbl_studiom_productimage pim on ISNULL(t.selectedimageid,0)=pim.id_studiom_productimage
	where change<>''
	order by catorder,gorder,porder,productname

	DROP TABLE #temp
    DROP TABLE #templastED

END

GO
