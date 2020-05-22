----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetUserByLoginName]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetUserByLoginName]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetUserByLoginName]
	@loginName varchar(50)
AS
BEGIN

	SET NOCOUNT ON;

	SELECT TOP 1
	U.userid AS UserID, 
	U.username AS FullName,
	M.fkidSubRegion AS RegionID,
	U.fkstateid AS StateID,
	R.SubRegionName AS RegionName,
	U.fkidPrimaryRole AS PrimaryRoleId,
	spm.fkRegionID AS priceregionid

	FROM tblUser U
 
	INNER JOIN tblUserSubRegionMapping M 
	ON U.userid = M.fkidUser

	INNER JOIN tblSubRegion R
	ON R.idSubRegion = M.fkidSubRegion
	
	INNER JOIN tblSubRegionPriceRegionMapping spm 
	ON R.idSubRegion = spm.fkidSubRegion	

	WHERE LoginName = @loginName
	
	AND U.active = 'Y'
	
	AND R.Active = 1
	
	AND M.Active = 1	

END

GO