IF  (NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_SalesEstimate_GetSelectedPromotions]') AND type in (N'P', N'PC')))
	BEGIN
		EXEC dbo.sp_executesql @statement = N'
		CREATE PROCEDURE [dbo].[sp_SalesEstimate_GetSelectedPromotions]
		AS
		BEGIN
			SET NOCOUNT ON;
		END';
	END
GO 

ALTER PROCEDURE [dbo].[sp_SalesEstimate_GetSelectedPromotions]
	@revisionid int
as
set nocount on
BEGIN

    declare @tempTab table
    (
        t_id int identity(1,1),
        promotion varchar(max)
    )

    declare @total int, @idx int, @tempStr varchar(max), @tempPromo varchar(max),@base varchar(20),@promotionid int
    declare @estimateid int, @mainrevisionid int, @revisiontypeid int
    select @estimateid=fkidEstimate, @revisiontypeid=fkid_SalesEstimate_RevisionType from tbl_SalesEstimate_EstimateHeader where id_SalesEstimate_EstimateHeader = @revisionid
    select @promotionid=promotionid from Estimate where EstimateID=@estimateid
    SET @mainrevisionid = @revisionid
    
    IF (@revisiontypeid IN (7,8,9,10,11,12,21,22)) --Studio M revisions
    BEGIN
		SET @mainrevisionid = (SELECT TOP 1 id_SalesEstimate_EstimateHeader FROM tbl_SalesEstimate_EstimateHeader WHERE fkid_SalesEstimate_RevisionType = 6 AND fkidEstimate = @estimateid)
    END

    SELECT te.*, ed.areaid, ed.groupid,ed.ProductID
    INTO   #tempMRSED
    FROM   tbl_SalesEstimate_EstimateDetails te
    INNER JOIN EstimateDetails ed ON te.fkidEstimateDetails=ed.EstimateDetailsID
    WHERE  fkid_SalesEstimate_EstimateHeader=@mainrevisionid

    set @idx=1
    set @tempStr=''

	select t.productname, case when p.promotionid is not null then 0 else 1 end as sortorder 
	into #temppromo
    from #tempMRSED t
	inner join tblmultiplepromotion mp on t.productid=mp.baseproductid and mp.fkidPromotionID=@promotionid
	inner join promotion p on mp.fkidpromotionid=p.promotionid

    insert into @tempTab
	SELECT productName FROM #temppromo
    order by sortorder, productname

    select @total=count(*) from @tempTab

    if (@total>0)
      BEGIN
         while (@idx<=@total)
             BEGIN
                select @tempPromo=promotion from @tempTab where t_id=@idx
                if (@tempPromo is not null)
                   BEGIN
                      if (@idx=1) set @tempStr=cast(@idx as varchar)+'. '+ @tempPromo else set @tempStr=@tempStr+'<br>' + cast(@idx as varchar)+'. '+@tempPromo
                   END
                set @idx=@idx+1
             END
      END
    else
      BEGIN
         set @tempStr= 'No Promotion Selected.'
      END
	
	DROP TABLE #tempMRSED
	DROP TABLE #temppromo
	
    select @tempStr as promotionName
END