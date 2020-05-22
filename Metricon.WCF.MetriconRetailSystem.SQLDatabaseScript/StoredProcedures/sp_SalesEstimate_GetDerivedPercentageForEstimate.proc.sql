
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetDerivedPercentageForEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetDerivedPercentageForEstimate]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetDerivedPercentageForEstimate] 
@estimateid					INT,
@derivedhomepercentage		DECIMAL(18,4) OUTPUT,
@deriveditempercentage		DECIMAL(18,4) OUTPUT,
@targetmargin				DECIMAL(18,4) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
		DECLARE @regionId INT, @stateid INT
	    DECLARE @brandid INT, @storey INT

		SELECT 
			@regionId=est.regionid,
			@brandid=h1.BrandID,
			@storey=h1.Stories,
			@stateid=h1.fkStateID
		FROM Estimate Est         
		INNER JOIN Home h1 ON Est.HomeID=h1.HomeID
		WHERE Est.EstimateID = @estimateid
		
		IF( EXISTS(	SELECT *
					FROM tbl_salesestimate_thresholdPercentage
					WHERE fkidBrand=@brandid AND fkidRegion=@regionId AND Storey=@storey AND Active=1)
		   )
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE fkidBrand=@brandid AND fkidRegion=@regionId AND Storey=@storey AND Active=1
			END
		ELSE IF( EXISTS(	SELECT *
					FROM tbl_salesestimate_thresholdPercentage
					WHERE fkidBrand=@brandid AND fkidRegion=@regionId AND Storey=0 AND Active=1)
		   )
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE fkidBrand=@brandid AND fkidRegion=@regionId AND Storey=0 AND Active=1
			END		
		ELSE IF( EXISTS(	SELECT *
					FROM tbl_salesestimate_thresholdPercentage
					WHERE fkidRegion=@regionId AND fkidBrand=0 AND  Storey=@storey AND Active=1)
		   )
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE  fkidRegion=@regionId AND fkidBrand=0 AND Storey=@storey AND Active=1
			END	
		ELSE IF( EXISTS(	SELECT *
					FROM tbl_salesestimate_thresholdPercentage
					WHERE fkidRegion=@regionId AND fkidBrand=0 AND  Storey=0 AND Active=1)
		   )
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE  fkidRegion=@regionId AND fkidBrand=0 AND Storey=0 AND Active=1
			END	
		ELSE IF( EXISTS(	SELECT *
					FROM tbl_salesestimate_thresholdPercentage
					WHERE fkidRegion=0 AND fkidBrand=@brandid AND  Storey=@storey AND Active=1)
		   )
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE  fkidRegion=0 AND fkidBrand=@brandid AND Storey=@storey AND Active=1
			END	
		ELSE IF( EXISTS(	SELECT *
					FROM tbl_salesestimate_thresholdPercentage
					WHERE fkidRegion=0 AND fkidBrand=@brandid AND  Storey=0 AND Active=1)
		   )
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE  fkidRegion=0 AND fkidBrand=@brandid AND Storey=0 AND Active=1
			END	
		ELSE IF( EXISTS(	SELECT *
					FROM tbl_salesestimate_thresholdPercentage
					WHERE fkidRegion=0 AND fkidBrand=0 AND  Storey=@storey AND Active=1 AND fkidstate=@stateid)
		   )
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE  fkidRegion=0 AND fkidBrand=0 AND Storey=@storey AND Active=1 AND fkidstate=@stateid
			END																	
		ELSE
			BEGIN
				SELECT @derivedhomepercentage=(HomeMargin/100),
				       @deriveditempercentage=(ItemMargin/100),
				       @targetmargin=(TargetMargin/100)
				FROM tbl_salesestimate_thresholdPercentage
				WHERE fkidBrand=0 AND fkidRegion=0 AND Storey=0 AND Active=1 AND fkidstate=@stateid
			END	

	SET NOCOUNT OFF;
END


GO