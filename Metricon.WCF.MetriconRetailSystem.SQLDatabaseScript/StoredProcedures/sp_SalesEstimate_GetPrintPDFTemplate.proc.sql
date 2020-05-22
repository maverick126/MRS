
----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetPrintPDFTemplate]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetPrintPDFTemplate]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO
-- =============================================
-- Author:		<FZ>
-- Create date: <08/02/2013>
-- Description:	<MRS estimate PDF template>
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetPrintPDFTemplate] 
@revisionId			INT,
@printtype          VARCHAR(20)
AS
BEGIN

	SET NOCOUNT ON;

      DECLARE @headerimagename VARCHAR(100), @revisiontype VARCHAR(20), @templatetype VARCHAR(20), @brandid INT, @stateid INT, @company VARCHAR(20)
      DECLARE @extendedday INT, @estimateid INT
      
      SELECT     @brandid=h.BrandID, @stateid=h.fkStateID, 
                 @company=CASE WHEN b.BrandName like '%home first%' THEN 'HOMEFIRST' ELSE 'METHOMES' END,
                 @estimateid=estimateid
      FROM       tbl_SalesEstimate_EstimateHeader eh
      INNER JOIN Estimate e ON eh.fkidEstimate=e.EstimateID
      INNER JOIN home h ON e.HomeID=h.HomeID
	  INNER JOIN Brand b ON h.BrandID=b.BrandID      
      WHERE      id_SalesEstimate_EstimateHeader=@revisionId
	  
	  IF @printtype	= 'changeonly'
	      BEGIN
	        SET @templatetype='variation'
	        SELECT @extendedday=ISNULL(ExtensionDays,0) 
	        FROM tbl_SalesEstimate_CustomerDocument
	        WHERE fkid_SalesEstimate_EstimateHeader=@revisionId
	      END
	  ELSE IF @printtype = 'studiom'
          SET @templatetype='studiom'     
	  ELSE    
	      SET @templatetype='template1'
	      
      SELECT     @headerimagename=
                 CASE WHEN rt.Abbreviation IN ('PVAR-CSC','PVAR-SE','PVAR-COL','PVAR-DF','BVAR-BSC','BVAR-BE','BVAR-DF') OR @printtype = 'changeonly'
                      THEN CASE WHEN @brandid IN (47,43,57,59)
                                THEN CASE WHEN rt.Abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR')
                                          THEN 'Header_Todayschange_homesolution.jpg'
                                          ELSE 'Header_Variation_homesolutions.jpg'
                                     END
                                WHEN @brandid = 58 THEN 'Header_Variation_homefirst.jpg'
                                ELSE CASE WHEN rt.Abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR')
                                          THEN 'Header_Todayschange.jpg'
                                          ELSE 'Header_Variation.jpg'
                                     END
                           END     
                      --ELSE CASE WHEN rt.Abbreviation IN ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR','STM-CUR','STM-FLR')
                        ELSE CASE WHEN @printtype = 'studiom'
                                THEN CASE WHEN @stateid<>3
                                          THEN 'vic_header_studiom.jpg'
                                          ELSE 'header_studiom.jpg'
                                     END
                                ELSE                   
									   CASE WHEN ISNULL(eh.ContractType,'PC')='PC'
											THEN CASE WHEN @brandid IN (47,43,57,59)
													  THEN 'header_pc_homesolutions.jpg'
													  WHEN @brandid = 58
													  THEN 'header_pc_homefirst.jpg'
													  ELSE 'header_pc.jpg'
												 END
											ELSE CASE WHEN @brandid IN (47,43,57,59) 
													  THEN 'header_contract_homesolutions.jpg'
													  WHEN @brandid = 58 
													  THEN 'header_contract_homefirst.jpg'
													  ELSE
														   CASE WHEN @stateid<>3 THEN 'header_contract.jpg'
																				 ELSE 'header_contract_QLD.jpg'
														   END
												 END
									   END
                            END
                 END, 
                 @revisiontype=rt.Abbreviation
      FROM       tbl_SalesEstimate_EstimateHeader eh
      INNER JOIN tbl_SalesEstimate_RevisionType rt ON eh.fkid_SalesEstimate_RevisionType=rt.id_SalesEstimate_RevisionType
      WHERE      id_SalesEstimate_EstimateHeader=@revisionId

      --IF (@revisiontype in ('STM-COL','STM-ELE','STM-PAV','STM-TIL','STM-DEC','STM-CAR') AND
      --    @printtype='studiom'
      --   )

      -- check if HIA contract or variation has been created, if yes when HIA contract revision is current revistion , use contract as header,
      -- if HIA revison is smaller than current revision, always show variation image
      DECLARE @hiarevisonid INT
      
      SELECT @hiarevisonid=fkid_SalesEstimate_EstimateHeader
      FROM tbl_SalesEstimate_CustomerDocument cd
      INNER JOIN (SELECT * FROM tbl_SalesEstimate_EstimateHeader WHERE fkidEstimate=@estimateid) eh ON cd.fkid_SalesEstimate_EstimateHeader=eh.id_SalesEstimate_EstimateHeader
      WHERE DocumentNumber=0 -- this is hia contract doc

      IF(@hiarevisonid IS NOT NULL)
         BEGIN
             IF(@hiarevisonid=@revisionId OR (@printtype = 'customer' AND @hiarevisonid<@revisionId))
                 BEGIN
                     IF(@brandid IN (47,43,57,59) )
                         SET @headerimagename='header_contract_homesolutions.jpg'
                     ELSE IF (@brandid = 58)
                         SET @headerimagename='header_contract_homefirst.jpg'
                     ELSE
                         IF(@stateid<>3)
                             SET @headerimagename='header_contract.jpg'
                         ELSE
                             SET @headerimagename='header_contract_QLD.jpg'
                 END
         END
      
      SELECT	Text2 as PDFTemplate, @headerimagename AS headerimage, @company AS company, ISNULL(@extendedday,0) AS extendedday
      FROM		tblSQSConfig
      WHERE		Code='MRS_Estimate_template'	AND
                Active=1						AND
                Text1=@templatetype

	SET NOCOUNT OFF;
END
GO
