
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_AssignWorkingSalesEstimate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_AssignWorkingSalesEstimate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
ALTER PROCEDURE [dbo].[sp_SalesEstimate_AssignWorkingSalesEstimate]
	-- Add the parameters for the stored procedure here
	@estimateRevisionId INT, @userId INT, @ownerId INT
AS
BEGIN
    DECLARE @mrsgroupid INT
    
    SELECT @mrsgroupid=r.MRSGroupID 
    FROM tblUserSubRegionMapping usm
    INNER JOIN tblSubRegionPriceRegionMapping spm ON usm.fkidSubRegion=spm.fkidSubRegion
    INNER JOIN Region r ON spm.fkRegionID=r.RegionID
    WHERE usm.fkidUser=@ownerId
    

	UPDATE tbl_SalesEstimate_EstimateHeader SET 
	fkidOwner = @ownerId,
	ModifiedOn = getdate(),
	ModifiedBy = @userId,
	fkid_salesestimate_MRSGroup=@mrsgroupid
	WHERE id_SalesEstimate_EstimateHeader = @estimateRevisionId
	
	-- send email alert if successful
	IF (@@ERROR=0)
	   BEGIN
	     IF (@userId<>@ownerId) -- not assign to me need email
	        BEGIN
            	 EXEC sp_SalesEstimate_SendAlertEmail  @userId,  @ownerId, @estimateRevisionId, 'ASSIGN'
		    END
	   END
END

GO
