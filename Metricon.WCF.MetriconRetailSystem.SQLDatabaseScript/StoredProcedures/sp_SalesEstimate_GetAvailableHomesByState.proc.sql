----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetAvailableHomesByState]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetAvailableHomesByState]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <08/01/2016,>
-- Description:	<get all available homes for a state>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetAvailableHomesByState]
@stateid  INT
AS
BEGIN

	SET NOCOUNT ON;

       SELECT 0 AS homeid,CAST('' AS VARCHAR(150)) AS homename, 0 AS displayhome INTO #temp
-- get parent homes, must be active homes
       INSERT INTO #temp
       SELECT h.HomeID, homename,0
       FROM home h
       INNER JOIN Brand b ON h.BrandID=b.BrandID
       WHERE ParentHomeID IS NULL AND
             h.Active=1 AND
             h.fkStateID=@stateid AND
             b.Active=1
       ORDER BY HomeName
       
-- get display homes regardless active or not       

       INSERT INTO #temp
       SELECT h.HomeID, ISNULL(h.HomeName,'')+' (Display at '+RTRIM(ISNULL(d.Suburb,''))+')'+' - '+BrandName,1        
       FROM home h
       INNER JOIN Display d ON h.HomeID=d.HomeID
       INNER JOIN Brand b on h.BrandID=b.BrandID
       WHERE 
             h.fkStateID=@stateid 
       ORDER BY HomeName
       
       DELETE FROM #temp WHERE homeid=0
       
       SELECT * FROM #temp 
       ORDER BY homename

	SET NOCOUNT OFF;
END

GO