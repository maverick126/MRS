
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetUpgradeOptionsForStandardInclusion]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetUpgradeOptionsForStandardInclusion]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetUpgradeOptionsForStandardInclusion]
@revisionid					INT,
@originateestimatedetailsid INT
AS
BEGIN

	SET NOCOUNT ON;
        DECLARE @estimateid INT, @brandid INT

        SELECT		@estimateid=fkidEstimate,
                    @brandid=h.BrandID
        FROM		tbl_SalesEstimate_EstimateHeader eh
        INNER JOIN	Estimate e	ON eh.fkidEstimate=e.EstimateID
        INNER JOIN  Home h		ON e.HomeID=h.HomeID
        WHERE		id_SalesEstimate_EstimateHeader=@revisionid
        
        SELECT *
        INTO   #temped
        FROM   EstimateDetails
        WHERE  EstimateID=@estimateid AND Active=1

        SELECT t1.productareagroupid, t1.productid, t1.areaid, t1.groupid
        INTO   #tempinclusionpag
        FROM   #temped t1 
        WHERE  EstimateDetailsid=@originateestimatedetailsid

        SELECT		ed.estimatedetailsid, 
                    ed.productareagroupid, 
                    ed.productid,
                    ed.productname+' ('+ed.productid+')' AS productname,
                    ed.areaname,
                    ed.groupname,
                    ed.productdescription,
                    CAST(0 AS BIT) AS selected                   
        FROM		#tempinclusionpag t1
        INNER JOIN	tbl_StudioM_InclusionValidationRule v	ON t1.productid=v.fkidInclusionProduct AND fkidBrand=@brandid
        INNER JOIN	ProductAreaGroup pag						ON v.fkidUpgradeProduct=pag.ProductID
        INNER JOIN	#temped ed								ON pag.ProductAreaGroupID=ed.ProductAreaGroupID
 

	SET NOCOUNT OFF;
END
GO