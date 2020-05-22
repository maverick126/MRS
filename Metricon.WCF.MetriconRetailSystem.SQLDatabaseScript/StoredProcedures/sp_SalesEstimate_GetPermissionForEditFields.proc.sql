----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetPermissionForEditFields]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetPermissionForEditFields]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <12/02/2015>
-- Description:	<get edit field permission for SE>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetPermissionForEditFields]
@revisionid						INT		,
@allowchangeprice				INT	OUTPUT, 
@allowchangeqty					INT OUTPUT, 
@allowchangeDesc				INT	OUTPUT, 
@allowchangePriceDisplayCode	INT OUTPUT
AS
BEGIN
 
	SET NOCOUNT ON;

		DECLARE @stateid INT , @revisiontypeid INT 
		SELECT  
			@stateid=h.fkStateID,
			@revisiontypeid=eh.fkid_SalesEstimate_RevisionType
		FROM		tbl_SalesEstimate_EstimateHeader eh
		INNER JOIN	estimate e	ON eh.fkidestimate=e.estimateid
		INNER JOIN	home h		ON e.homeid=h.homeid
		WHERE id_SalesEstimate_EstimateHeader=@revisionId

		IF(EXISTS(SELECT * FROM tbl_SalesEstimate_OpenEditModule WHERE fkidstate=@stateid AND active=1))-- for all SE revisions
		   BEGIN
			   SELECT   @allowchangeDesc=openproductstandarddescription
			   FROM     tbl_SalesEstimate_OpenEditModule
			   WHERE    fkidstate=@stateid AND active=1
			   		   
		       IF(@revisiontypeid IN (4,15, 19, 25)) 
		           BEGIN 
					   SELECT   
								@allowchangeprice=openunitprice,
								@allowchangePriceDisplayCode=openpricedisplaycode,
								@allowchangeqty=openquantity
					   FROM     tbl_SalesEstimate_OpenEditModule
					   WHERE    fkidstate=@stateid AND active=1
				   END
			    ELSE
				   BEGIN
					   SET @allowchangeprice=0
					   SET @allowchangePriceDisplayCode=0
					   SET @allowchangeqty=0
				   END			    
			       
		   END
		ELSE
		   BEGIN
			   SET @allowchangeDesc=0
			   SET @allowchangeprice=0
			   SET @allowchangePriceDisplayCode=0
			   SET @allowchangeqty=0
		   END  				  

	SET NOCOUNT OFF;
END

GO
