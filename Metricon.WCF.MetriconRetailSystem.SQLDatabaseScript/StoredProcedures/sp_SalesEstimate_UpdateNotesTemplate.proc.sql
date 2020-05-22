----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_UpdateNotesTemplate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_UpdateNotesTemplate]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_UpdateNotesTemplate] 
@templateid			INT		,
@templatename       VARCHAR(200),
@status				INT	,
@userid             INT	,
@action             VARCHAR(20)  -- name, only update name, status only update status
AS
BEGIN

	SET NOCOUNT ON;

        IF (@action='NAME')
            BEGIN
                UPDATE tbl_SalesEstimate_NotesTemplate
                SET    TemplateName=@templatename,
                       ModifiedOn=GETDATE(),
                       ModifiedBy=@userid
                WHERE id_SalesEstimate_NotesTemplate=@templateid
            END
            
        ELSE IF (@action='STATUS')
            BEGIN
                UPDATE tbl_SalesEstimate_NotesTemplate
                SET    Active=@status,
                       ModifiedOn=GETDATE(),
                       ModifiedBy=@userid
                WHERE id_SalesEstimate_NotesTemplate=@templateid
            END 
            
        ELSE IF (@action='ISPRIVATE')
            BEGIN
                UPDATE tbl_SalesEstimate_NotesTemplate
                SET    IsPrivate=@status,
                       ModifiedOn=GETDATE(),
                       ModifiedBy=@userid
                WHERE id_SalesEstimate_NotesTemplate=@templateid
            END                       


	SET NOCOUNT OFF;
END
GO
