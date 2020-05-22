 /*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetContactsOfCustomer]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetContactsOfCustomer]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

-- =============================================
-- Author:		<FZ>
-- Create date: <12/06/2012>
-- Description:	get customer contacts from BC
-- =============================================
ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetContactsOfCustomer]
@customercode		VARCHAR(10)
AS
BEGIN

	SET NOCOUNT ON;

      DECLARE @final TABLE
      (
			CustomerNo		VARCHAR(10),
			seq				INT			,
			salutation		VARCHAR(50),
			lastname		VARCHAR(100),
			firstname		VARCHAR(100),
			customerstreet	VARCHAR(100),
			customersuburb	VARCHAR(50), 
			customerstate	VARCHAR(10), 
            customerpostcode VARCHAR(10),
            phone			VARCHAR(20),
            mobile			VARCHAR(100),
            email			VARCHAR(100),
            [primary]		INT		
      )
      
      DECLARE @minseq INT
      
      INSERT INTO	@final
      SELECT 
					IVTU_CUSTNO,
					ivtu_seqnum,
					ivtu_title,
					ivtu_surname,
					ivtu_firstname,
					ivtu_address,
					ivtu_suburb, 
					ivtu_state, 
                    ivtu_zip,
                    ivtu_phhome,
                    ivtu_mobile,
					 CASE WHEN PATINDEX('%;%',EMLA_ADDRESS)>1 and PATINDEX('%;%',EMLA_ADDRESS)<LEN(EMLA_ADDRESS) THEN SUBSTRING(EMLA_ADDRESS,1,PATINDEX('%;%',EMLA_ADDRESS)-1) ELSE 
						 CASE WHEN PATINDEX('%ÿ%',EMLA_ADDRESS)>1 and PATINDEX('%ÿ%',EMLA_ADDRESS)<LEN(EMLA_ADDRESS) THEN SUBSTRING(EMLA_ADDRESS,1,PATINDEX('%ÿ%',EMLA_ADDRESS)-1) ELSE REPLACE(REPLACE(REVERSE(REPLACE(REVERSE(REPLACE(EMLA_ADDRESS,'ht','Û')),'þ','')),'Û','ht'),'ÿ','')
						 END 
					 END,
					 --'softwaredevelopment@metricon.com.au',
					 0                    				 
      FROM			[SQLCLUS01].[MdwhNational].[dbo].IVCTBLU cu
      LEFT JOIN     [SQLCLUS01].[MdwhNational].[dbo].EMLADDB em		ON LTRIM(RTRIM(cu.ivtu_custno))+'00'+LTRIM(RTRIM(cu.ivtu_seqnum))=LTRIM(RTRIM(emla_refcode))
      WHERE			IVTU_CUSTNO=@customercode
      ORDER BY		IVTU_CUSTNO,ivtu_seqnum
      
      SELECT	@minseq=MIN(seq) 
      FROM		@final
      GROUP BY	CustomerNo
      
      UPDATE	@final
      SET		[primary]=1
      WHERE		seq=@minseq
      
      SELECT	*
      FROM		@final
      
      
     
	SET NOCOUNT OFF;
END

GO