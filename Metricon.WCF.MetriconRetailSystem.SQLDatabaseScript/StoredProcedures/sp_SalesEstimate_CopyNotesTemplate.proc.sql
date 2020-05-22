----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CopyNotesTemplate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CopyNotesTemplate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_CopyNotesTemplate] 
@templatename			VARCHAR(500)	,
@regionid				INT				,
@userid					INT				,
@templateid				INT
AS
BEGIN

	SET NOCOUNT ON;

		IF (NOT EXISTS(SELECT * FROM tbl_SalesEstimate_NotesTemplate WHERE RTRIM(templatename)=RTRIM(@templatename) AND fkidsubregion=@regionid))
		BEGIN
			DECLARE @newtemplateid INT
		   INSERT INTO	tbl_SalesEstimate_NotesTemplate
		   SELECT		@templatename,
						0,
						@regionid,
						GETDATE(),
						@userid,
						NULL,
						NULL,
						1,
						@userid,
						1
			SET @newtemplateid = SCOPE_IDENTITY()
			
			INSERT INTO tbl_SalesEstimate_NotesTemplateItems 
			SELECT @newtemplateid, 
					fkidProductAreaGroup, 
					GETDATE(), 
					@userid, 
					1, 
					ExtraDescription, 
					Quantity, 
					Price, 
					GETDATE(), 
					@userid, 
					InternalDescription, 
					AdditionalInfo 
			FROM tbl_SalesEstimate_NotesTemplateItems WHERE fkid_SalesEstimate_NotesTemplate = @templateid AND Active = 1					
		END

	SET NOCOUNT OFF;

END

GO