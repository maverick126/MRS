IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAreaSurcharge]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAreaSurcharge]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAreaSurcharge]
@estimateRevisionId int
AS
BEGIN



--if (EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON ED.EstimateDetailsID = SED.fkidEstimateDetails WHERE SED.fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND ED.areaid = 1))
--    SELECT 1 AS result, SUM(ItemPrice * SED.Quantity) as surcharge from tbl_SalesEstimate_EstimateDetails SED INNER JOIN EstimateDetails ED ON ED.EstimateDetailsID = SED.fkidEstimateDetails WHERE SED.fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND ED.areaid = 1
--else
--    SELECT 0 AS result, 0 AS surcharge

if (EXISTS(SELECT * FROM tbl_SalesEstimate_EstimateDetails  WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND fkidarea = 1))
    SELECT 1 AS result, SUM(ItemPrice * Quantity) as surcharge from tbl_SalesEstimate_EstimateDetails  WHERE fkid_SalesEstimate_EstimateHeader = @estimateRevisionId AND fkidarea = 1
else
    SELECT 0 AS result, 0 AS surcharge
    
END