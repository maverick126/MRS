----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetStudioMQuestionForAProduct]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetStudioMQuestionForAProduct]
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
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetStudioMQuestionForAProduct]
@productid VARCHAR(50)
AS
BEGIN

	SET NOCOUNT ON;
      DECLARE @defaultQA VARCHAR(MAX)
      SET @defaultQA='<Brands>
						  <Brand id="18" name="Metricon Homes">
							<Questions>
							  <Question id="129" text="Studio M Clarification(Free Text)" type="Free Text" mandatory="0">
								<Answers>
								  <Answer id="510" text="Studio M Clarification" />
								</Answers>
							  </Question>
							</Questions>
						  </Brand>
						</Brands>	'
						
      SELECT ISNULL(StudioMQAndA,@defaultQA ) AS studiomquestion 
      FROM product
      WHERE ProductID=@productid

	SET NOCOUNT OFF;
END

GO