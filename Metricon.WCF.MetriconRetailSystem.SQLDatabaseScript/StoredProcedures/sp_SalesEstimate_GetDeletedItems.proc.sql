----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetDeletedItems]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetDeletedItems]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetDeletedItems]
	@revisionId int
AS
BEGIN

	SELECT A.AreaName, G.GroupName, Prd.ProductID, Prd.ProductName, Prd.ProductDescription, Rem.RemovedDate, Usr.username, Rem.Reason AS Comment, prs.DeletionReason AS reason
	FROM tbl_SalesEstimate_RemovedItems Rem
	LEFT JOIN tbl_SalesEstimate_PredefinedDeletionReason prs ON Rem.fkid_SalesEstimate_PredefinedDeletionReason=prs.idSalesEstimate_PredefinedDeletionReason
	INNER JOIN ProductAreaGroup Pag ON Rem.fkidProductAreaGroup = Pag.ProductAreaGroupID
	INNER JOIN [Area] A ON Pag.AreaID = A.AreaID
	INNER JOIN [Group] G ON Pag.GroupID = G.GroupID
	INNER JOIN Product Prd ON Pag.ProductID = Prd.ProductID
	INNER JOIN tbluser Usr ON Rem.RemovedBy = Usr.userid   
	WHERE fkidRevision = @revisionId

END

GO