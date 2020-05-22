
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAreasByState]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAreasByState]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAreasByState] 
@stateid		INT,
@selectedareaid INT
AS
BEGIN
	SET NOCOUNT ON;
	
		DECLARE @areaTable TABLE (AreaID INT, AreaName VARCHAR(255), SortOrder INT)

		INSERT INTO @areaTable
	
		SELECT 0 AS areaID,'Please Select' AS areaName, 0 AS sortorder
	   
		UNION
	   
		SELECT DISTINCT a.AreaID, a.AreaName, 1 
		FROM  Area a
		INNER JOIN ProductAreaGroup pag ON a.AreaID=pag.AreaID
		INNER JOIN product pt ON pag.ProductID=pt.ProductID
		WHERE pt.fkStateID=@stateid AND a.Active=1 AND pag.Active=1 AND a.AreaID<>43
		
		UNION
		
		SELECT AreaID, AreaName, 1 FROM Area WHERE AreaID = @selectedareaid
		
		SELECT DISTINCT AreaID, AreaName, SortOrder FROM @areaTable ORDER BY SortOrder, AreaName 

	SET NOCOUNT OFF;
END

GO