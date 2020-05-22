----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_AddNewNotesTemplate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_AddNewNotesTemplate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_SalesEstimate_AddNewNotesTemplate] 
@templatename			VARCHAR(500)	,
@regionid				INT				,
@userid					INT
AS
BEGIN

	SET NOCOUNT ON;

		IF (NOT EXISTS(SELECT * FROM tbl_SalesEstimate_NotesTemplate WHERE RTRIM(templatename)=RTRIM(@templatename) AND fkidsubregion=@regionid))
		BEGIN
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
		END

	SET NOCOUNT OFF;

END

GO