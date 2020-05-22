
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_CheckNotesTemplateNameExists]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_CheckNotesTemplateNameExists]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_CheckNotesTemplateNameExists] 
@templateid				INT	,
@templatename			VARCHAR(200)
AS
BEGIN

	SET NOCOUNT ON;

        -- get all subregion in the same group exclude current template region
        DECLARE @groupid INT, @subregionid INT
        DECLARE @result VARCHAR(300)
        
        SELECT   @subregionid=fkidSubRegion
        FROM     tbl_SalesEstimate_NotesTemplate
        WHERE    id_SalesEstimate_NotesTemplate=@templateid
        
        SELECT   @groupid=GroupID
        FROM     tblSubRegion
        WHERE    idSubRegion=@subregionid
        
        SELECT   idSubRegion
        INTO     #tempsubregion
        FROM     tblSubRegion
        WHERE    GroupID=@groupid AND idSubRegion<>@subregionid
       -- get all templates in the subregions
       
       SELECT    *
       INTO      #template
       FROM      tbl_SalesEstimate_NotesTemplate
       WHERE     fkidSubRegion IN (SELECT idSubRegion FROM #tempsubregion)
       
       -- check duplication
       
       IF (EXISTS(SELECT * FROM #template WHERE templatename=@templatename))
            BEGIN
               SELECT @result='New template name exists in '+s.SubRegionName+' Please make sure name is unique.'
               FROM   #template t1
               INNER JOIN tblSubRegion s ON t1.fkidsubregion=s.idSubRegion
               WHERE  templatename=@templatename
            END
       ELSE        
            BEGIN
               SELECT @result='OK'
            END
            
       SELECT    @result AS result 

    SET NOCOUNT OFF
END

GO