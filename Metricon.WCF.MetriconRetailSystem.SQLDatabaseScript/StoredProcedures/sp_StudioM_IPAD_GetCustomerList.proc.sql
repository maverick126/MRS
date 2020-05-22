----------------------------------------------------------
--
----------------------------------------------------------
/*
change from if exists then drop and create new one to check if not exists then create new one then update to expected.
this way, can easily keep the contrains for the sp. if drop existing one, could have the potential problem to keep the contrains.
*/
IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_StudioM_IPAD_GetCustomerList]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_StudioM_IPAD_GetCustomerList]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO

ALTER PROCEDURE [dbo].[sp_StudioM_IPAD_GetCustomerList]
@colorconsultant		VARCHAR(10)		,
@primarycontactname		VARCHAR(50)		,
@contractsuburb			VARCHAR(50)		,	
@contractstreet			VARCHAR(50)				
AS
BEGIN

	SET NOCOUNT ON;
            DECLARE @sql VARCHAR(MAX)
			SET @sql='
			SELECT DISTINCT
		    t.BCCustomerID					AS  customernumber			,
		    t.BCContractNumber				AS  contractnumber			, 
            CASE WHEN ct.contactid=a.primarycontactid 
                 then 1 
                 else 0 
            end								AS	primarycontact			,
            a.name							AS	customername			,
			ct.salutation					AS	salutation				,
			ct.firstname					AS	firstname				,
			ct.lastname						AS	lastname				,
			ct.address1_line1				AS	ADDRESS1				,
			ct.address1_city				AS	SUBURB					,
			ct.address1_stateorprovince		AS	STATE					,
			ct.address1_postalcode			AS	POSTCODE				,
			ct.telephone1					AS	PHWORK					,
			ct.telephone2					AS	PHHOME					,
			ct.mobilephone					AS	MOBILE					,
			ct.fax							AS	FAX						,
			ct.emailaddress1				AS  email					,
			ct.address1_country				AS	COUNTRY					,			
			c.new_lotnumber					AS  contractlotnumber		,
			c.new_address_streetnumber		AS  contractstreetno		,
			c.new_address_street			AS  contractstreet			,
			c.new_address_suburb			AS	contractsuburb			,
			c.new_address_postcode			AS  contractpostcode		,
            c.new_state						AS  contractstate			,
            ''AU''							AS	contractcountry			,
		    c.new_m_estatename  			AS	Estate					,
			h.homename													,
			t.estimateid					AS	estimatenumber			,
			CAST(t.homesellprice AS DECIMAL(18,2)) AS homesellprice		,
			r.regionname												,											
			AppointmentDateTime 										,
			u.username						AS	salesconsultantname		,
			u2.username						AS	revisionowner			,
			b.BrandName						AS	brandname				,
			d.DepositDate					AS	depositdate				,
			t.EstimateActualDate			AS	actualdate				,
			t.EstimateDate					AS  createddate				,
			t.comments						AS	revisioncomments		,
			t.id_SalesEstimate_EstimateHeader AS estimaterevisionid		,
			t.RevisionTypeName				AS	revisiontype			,
			t.HomePriceEffectiveDate		AS	effectivedate			,
            CASE WHEN t.fkidpackage = NULL 
                 then 0 
                 else 1		
            end								AS houseandland	,
            t.revisionNo										 			
            INTO	#temp

			FROM  (
			select eh.AppointmentDateTime,e.homeid,e.homesellprice, e.BCContractNumber,e.EstimateDate,e.EstimateActualDate,
			e.estimateid,e.BCCustomerID,e.BCSalesConsultant,e.regionid, e.fkidpackage , eh.comments , 
			CAST(eh.revisionnumber AS VARCHAR)+'' (''+rt.abbreviation+'')'' AS revisionNo , e.fkidOpportunity, 
			eh.id_SalesEstimate_EstimateHeader, rt.RevisionTypeName, eh.fkidOwner, eh.HomePriceEffectiveDate
			from tbl_SalesEstimate_EstimateHeader  eh
			inner join estimate e on eh.fkidestimate=e.estimateid  
			inner join tbl_SalesEstimate_RevisionType rt on eh.fkid_SalesEstimate_RevisionType=rt.id_SalesEstimate_RevisionType 
		    WHERE fkidowner='+@colorconsultant+' and fkid_salesestimate_status=1 and fkid_SalesEstimate_revisiontype in (7,8,9,10,11,12,21,22)) t			
            INNER JOIN syn_crm_new_contract c ON t.bcContractnumber COLLATE database_default=c.new_name COLLATE database_default
            INNER JOIN syn_crm_account  a ON c.new_customerid=a.accountid
            INNER JOIN syn_crm_contact ct ON a.accountid=ct.accountid AND ct.statecode = 0
			
			INNER JOIN home h		ON t.homeid=h.homeid
			INNER JOIN region r		ON t.regionid=r.regionid 
			INNER JOIN tblUser u	ON t.BCSalesConsultant = u.usercode 
			INNER JOIN tblUser u2	ON t.fkidOwner = u2.userid
			INNER JOIN brand b		ON h.BrandID = b.BrandID
			INNER JOIN DepositDetails d ON t.BCContractNumber = d.BCContractNumber AND t.fkidOpportunity = d.fkidOpportunity'
			
			IF (@primarycontactname<>'')	SET @sql=@sql+' AND (ct.firstname LIKE ''%'+@primarycontactname+'%'' OR ct.lastname LIKE ''%'+@primarycontactname+'%'')'
			IF (@contractsuburb<>'')		SET @sql=@sql+' AND c.new_address_suburb LIKE ''%'+@contractsuburb+'%'''
			IF (@contractstreet<>'')		SET @sql=@sql+' AND c.new_address_street LIKE ''%'+@contractstreet+'%'''			 

			SET @sql=@sql+'           
            ORDER BY t.BCCustomerID, t.BCContractNumber, primarycontact desc, ct.firstname, ct.lastname

			--Get promotion name

            SELECT estimateid AS tempid,max(productareagroupid) as pagid 
            into #temp2
            FROM estimatedetails 
            where estimateid in (select distinct estimatenumber from #temp)
            and selected=1 and areaid=52 and groupid=251 and promotionproduct=0 and productareagroupid<>41152
            group by estimateid
							    
            select t.*,p.productname into #temp3 from #temp2 t
            inner join productareagroup pag on t.pagid=pag.productareagroupid
            inner join product p on pag.productid=p.productid
            
            select distinct t.*, t.estimatenumber as estimateid, t3.* 
            from #temp t
            left join #temp3 t3 on t.estimatenumber=t3.tempid
            order by t.customernumber, t.revisionNo, t.contractnumber, primarycontact desc
            
            DROP TABLE #temp
            DROP TABLE #temp2
            DROP TABLE #temp3
            '			

			EXEC (@sql)
			
	SET NOCOUNT OFF;
END

GO