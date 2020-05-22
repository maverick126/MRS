----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetPriceRegion]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetPriceRegion]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetPriceRegion]
@stateid int
as
BEGIN
	Select r.RegionID,r.regionname
    from region r
    inner join tblpriceregiongroupmapping pm on pm.fkregionid=r.regionid
    inner join tblregiongroup on pm.fkidregiongroup=idregiongroup
    where r.active =1 and fkstateid=@stateid
    order by Regionid
END

GO