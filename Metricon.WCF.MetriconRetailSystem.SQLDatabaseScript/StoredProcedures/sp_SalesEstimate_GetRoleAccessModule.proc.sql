
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetRoleAccessModule]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetRoleAccessModule]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <22/08/2014>
-- Description:	<get role access module , no hard code in app>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetRoleAccessModule] 
@roleid		INT
AS
BEGIN
 
	SET NOCOUNT ON;
	
		SELECT * 
		FROM tbl_SalesEstimate_RoleModuleAccess
		WHERE Active=1 AND fkidRole=@roleid
       
	SET NOCOUNT OFF;
END

GO
