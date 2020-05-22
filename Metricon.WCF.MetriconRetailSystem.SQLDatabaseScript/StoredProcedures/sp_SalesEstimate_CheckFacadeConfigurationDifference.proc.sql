----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CheckFacadeConfigurationDifference]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CheckFacadeConfigurationDifference]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <13/02/2013>
-- Description:	<get all facade for a home based on revision>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_CheckFacadeConfigurationDifference] 
@revisionid			INT			,
@newfacadehomeid	INT			,
@date				VARCHAR(20)
AS
BEGIN

	SET NOCOUNT ON;
	
	DECLARE @change TABLE
	(
	    areaname		VARCHAR(100),
	    groupname		VARCHAR(100),
	    productname		VARCHAR(500),
	    error			VARCHAR(1000),
	    reason			INT,
	    pagid			INT
	)
	
	DECLARE @regionid INT, @stateid INT
	
	SELECT     @regionid=e.regionid, @stateid=h.fkStateID
	FROM       tbl_SalesEstimate_EstimateHeader eh
	INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
	INNER JOIN home h	  ON e.HomeID=h.homeid
	WHERE      eh.id_SalesEstimate_EstimateHeader=@revisionid
	
-- get all latest price
	SELECT		DISTINCT p.ProductID
	INTO		#tempProduct 
	FROM		product p
	INNER JOIN  ProductAreaGroup pag on p.ProductID=pag.productid
	WHERE		pag.active=1 AND p.active=1 and p.fkstateid=@stateid

	SELECT		PriceID,p.ProductID,SellPrice,EffectiveDate,CreatedDate,RegionID,PromotionPrice 
	INTO		#tempPrice
    FROM		price p 
    INNER JOIN  #tempProduct tp on p.productid=tp.productid
	WHERE		p.active=1 and regionid=@regionid and effectivedate<@date

	SELECT		productid,MAX(effectivedate) AS effectivedate 
	INTO		#price1
    FROM		#tempPrice 
	GROUp BY    productid
    HAVING		MAX(effectivedate)<@date

	SELECT		p1.productid,p1.effectivedate,MAX(price.createddate) AS createddate 
	INTO		#price2
    FROM		#price1 p1 
	INNER JOIN  #tempPrice price	ON p1.productid=price.productid AND p1.effectivedate=price.effectivedate
    GROUP BY	p1.productid, p1.effectivedate

	SELECT		price.productid AS productid, MAX(ISNULL(price.sellprice,0.0)) AS sellprice, 
	            MAX(ISNULL(price.promotionprice,0.0)) AS promotionprice,p2.effectivedate
	INTO		#currentPrice
	FROM		#price2 p2 
    INNER JOIN  #tempPrice price ON p2.productid=price.productid AND p2.effectivedate=price.effectivedate AND p2.createddate=price.createddate
	GROUP BY	price.productid,p2.effectivedate
	
-- get current revision details, only the options not inclusions		
    SELECT		ed2.*, ed.ItemPrice AS MRSPrice, ed.Quantity AS MRSQuantity
    INTO		#tempED
    FROM		tbl_SalesEstimate_EstimateDetails ed
    INNER JOIN  EstimateDetails ed2		ON ed.fkidEstimateDetails=ed2.EstimateDetailsID
    WHERE	    fkid_SalesEstimate_EstimateHeader=@revisionid		AND
				fkidEstimateDetails >0
				
-- get all new facade configurations

    SELECT		*
    INTO		#tempHDO
    FROM		HomeDisplayOption
    WHERE		HomeID=@newfacadehomeid					AND
				HomeDisplayID IS NULL					AND
				Active=1
-- find all missing PAG in new facade

    INSERT INTO @change
    SELECT
				a.AreaName,
				g.GroupName,
				'['+pag.productid+'] '+ed.productname,
				'Not available in new facade.',
				0,
				pag.productareagroupid	
    FROM		#tempED ed
    LEFT JOIN   #tempHDO hdo			ON ed.productareagroupid=hdo.productareagroupid
    INNER JOIN  ProductAreaGroup pag    ON ed.productareagroupid=pag.productareagroupid
    INNER JOIN  Area a					ON pag.AreaID=a.AreaID
    INNER JOIN  [Group] g				ON pag.GroupID=g.GroupID
    WHERE		hdo.optionid IS NULL
    
-- find all PAG which quantity changed

    INSERT INTO @change
    SELECT
				a.AreaName,
				g.GroupName,
				'['+pag.productid+'] '+ed.productname,
				'Quantity changed. Original façade is '+CAST(CAST(ed.MRSQuantity AS DECIMAL(18,2)) AS VARCHAR)+' new facade quantity is '+CAST(CAST(hdo.quantity AS DECIMAL(18,2)) AS VARCHAR)+'. System will change new facade quantity to '+CAST(CAST(ed.MRSQuantity AS DECIMAL(18,2)) AS VARCHAR)+'.',
				1,
				pag.productareagroupid	
    FROM		#tempED ed
    INNER JOIN  #tempHDO hdo			ON ed.productareagroupid=hdo.productareagroupid
    INNER JOIN  ProductAreaGroup pag    ON ed.productareagroupid=pag.productareagroupid
    INNER JOIN  Area a					ON pag.AreaID=a.AreaID
    INNER JOIN  [Group] g				ON pag.GroupID=g.GroupID
    WHERE		ed.MRSQuantity <> hdo.quantity

-- find all PAG which price changed

    INSERT INTO @change
    SELECT
				a.AreaName,
				g.GroupName,
				'['+pag.productid+'] '+ed.productname,
				'Unit price changed. Original façade is $'+CAST(CAST(ed.MRSPrice AS DECIMAL(18,2)) AS VARCHAR)+' new facade price is $'+CAST(CAST(cp.sellprice AS DECIMAL(18,2)) AS VARCHAR)+'. System will change new facade price to $'+CAST(CAST(ed.MRSPrice AS DECIMAL(18,2)) AS VARCHAR)+'.',
				2,
				pag.productareagroupid
    FROM		#tempED ed
    INNER JOIN  #tempHDO hdo			ON ed.productareagroupid=hdo.productareagroupid
    INNER JOIN  ProductAreaGroup pag    ON ed.productareagroupid=pag.productareagroupid
    INNER JOIN  Area a					ON pag.AreaID=a.AreaID
    INNER JOIN  [Group] g				ON pag.GroupID=g.GroupID
    INNER JOIN  #currentprice cp		ON pag.productid=cp.productid
    WHERE		ed.MRSPrice <> cp.sellprice
	
-- export result

    SELECT * FROM @change				
		
	SET NOCOUNT OFF;
END
GO
