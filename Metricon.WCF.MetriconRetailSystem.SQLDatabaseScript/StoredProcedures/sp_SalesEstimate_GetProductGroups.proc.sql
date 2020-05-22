----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetProductGroups]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetProductGroups]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetProductGroups]
@selectedareaid  INT,
@stateid         INT,
@selectedgroupid INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @groupTable TABLE (GroupID INT, GroupName VARCHAR(255))

	INSERT INTO @groupTable
	
    SELECT g.groupID, groupName 
    FROM (SELECT * FROM ProductAreaGroup WHERE AreaID=@selectedareaid) pag
    INNER JOIN product pt ON pag.ProductID=pt.ProductID
    INNER JOIN [Group] g ON pag.groupid=g.groupid
    WHERE  pag.active = 1 AND g.Active=1 AND pt.fkStateID=@stateid
 
    UNION 
    
    SELECT GroupID, GroupName FROM [Group] WHERE GroupID = @selectedgroupid

	SELECT DISTINCT GroupID, GroupName FROM @groupTable ORDER BY GroupName

	SET NOCOUNT OFF;
END

GO
