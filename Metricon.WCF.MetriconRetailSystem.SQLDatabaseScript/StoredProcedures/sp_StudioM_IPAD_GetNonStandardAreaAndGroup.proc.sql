
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_StudioM_IPAD_GetNonStandardAreaAndGroup]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_StudioM_IPAD_GetNonStandardAreaAndGroup]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <01/12/2104>
-- Description:	<get all areas and groups with stateid for IPad>
-- =============================================
ALTER PROCEDURE [dbo].[sp_StudioM_IPAD_GetNonStandardAreaAndGroup] 
@revisionid		INT
AS
BEGIN
	SET NOCOUNT ON;
	
	   DECLARE @stateid INT
	   
	   SELECT @stateid=rg.fkStateID 
	   FROM tbl_SalesEstimate_EstimateHeader eh
	   INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
	   INNER JOIN tblPriceRegionGroupMapping prm ON e.RegionID=prm.fkRegionID
	   INNER JOIN tblRegionGroup rg ON prm.fkidRegionGroup=rg.idRegionGroup
	   WHERE eh.id_SalesEstimate_EstimateHeader=@revisionid

       SELECT DISTINCT a.AreaID, a.AreaName, g.GroupID, g.GroupName--, pt.fkStateID--, s.StateAbbreviation
       FROM  Area a
       INNER JOIN ProductAreaGroup pag ON a.AreaID=pag.AreaID
       INNER JOIN [Group] g ON pag.GroupID=g.GroupID
       INNER JOIN product pt ON pag.ProductID=pt.ProductID
       WHERE pt.fkStateID=@stateid AND a.Active=1 AND pag.Active=1 AND g.Active=1
       ORDER BY AreaName, GroupName
       
      	
	SET NOCOUNT OFF;
END

GO