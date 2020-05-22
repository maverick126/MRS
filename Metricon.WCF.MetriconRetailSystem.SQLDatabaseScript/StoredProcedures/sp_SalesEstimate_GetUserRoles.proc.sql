
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetUserRoles]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetUserRoles]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetUserRoles]
	@userId int
AS
BEGIN

	SET NOCOUNT ON;

	SELECT DISTINCT
	R.idRole AS RoleID, 
	R.Description AS RoleName,
	R.IsManager AS IsManager 
	FROM tblUserRole UR 
	INNER JOIN tblRole R
    ON UR.fkidRole = R.idRole	
    INNER JOIN tbl_SalesEstimate_RevisionTypeAccess A 
	ON A.fkidRole = UR.fkidRole
	WHERE UR.fkidUser = @userId AND UR.active=1
	ORDER BY RoleName ASC

	SET NOCOUNT OFF;

END