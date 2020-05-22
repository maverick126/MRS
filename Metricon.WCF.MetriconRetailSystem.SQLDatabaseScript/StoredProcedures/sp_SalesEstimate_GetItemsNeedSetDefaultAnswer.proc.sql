 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetItemsNeedSetDefaultAnswer]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetItemsNeedSetDefaultAnswer]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <14/01/2015>
-- Description:	get product and studiom q and a need answer
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetItemsNeedSetDefaultAnswer]
@revisionid INT
AS
BEGIN

	SET NOCOUNT ON;
      
       SELECT eh.id_SalesEstimate_EstimateDetails,
              pt.StudioMQAndA ,
              eh.StudioMAttributes
       FROM
      (SELECT * FROM tbl_SalesEstimate_EstimateDetails WHERE fkid_SalesEstimate_EstimateHeader=@revisionid) eh
       INNER JOIN EstimateDetails ed ON eh.fkidEstimateDetails=ed.EstimateDetailsID
       INNER JOIN product pt ON ed.ProductID=pt.ProductID
       WHERE pt.isStudioMProduct=1 AND
             pt.StudioMQAndA IS NOT NULL AND RTRIM(LTRIM(CAST(pt.StudioMQAndA AS VARCHAR(MAX))))<>'' AND
             (eh.StudioMAttributes IS NULL OR CAST(eh.StudioMAttributes AS VARCHAR(MAX))='') 
             --and id_SalesEstimate_EstimateDetails=1502186

	SET NOCOUNT OFF;
END

GO
