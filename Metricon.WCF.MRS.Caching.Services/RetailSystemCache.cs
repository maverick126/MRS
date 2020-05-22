using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using Metricon.WCF.MRS.Caching.Services.Services.Interfaces;
using Metricon.WCF.MRS.Caching.Services.ServiceReference1;
using Metricon.WCF.MetriconRetailSystem.Contracts;

namespace Metricon.WCF.MRS.Caching.Services
{
    // Add the attribute to the service implementations:
    public class RetailSystemCache : MetriconRetailSystem.Contracts.IRetailSystem
    {
        private static long CurrentRequestCount = 0;

        private const int EstimateStatusAccepted = 2;

        private readonly ICacheService _cacheService;

        public RetailSystemCache(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }
        public void AcceptOriginalEstimate(int estimateId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();
            client.AcceptOriginalEstimate(estimateId, userId);
        }

        public int AssignQueuedEstimate(int queueId, int userId, int ownerId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.AssignQueuedEstimate(queueId, userId, ownerId);
        }

        public void AssignWorkingEstimate(int estimateRevisionId, int userId, int ownerId)
        {
            RetailSystemClient client = new RetailSystemClient();
            client.AssignWorkingEstimate(estimateRevisionId, userId, ownerId);
        }

        public void CompleteEstimate(int revisionId, int userId, int statusId, int statusReasonId, int revisionTypeId, int ownerId)
        {
            RetailSystemClient client = new RetailSystemClient();
            client.CompleteEstimate(revisionId, userId, statusId, statusReasonId, revisionTypeId, ownerId);
        }

        public List<EstimateGridItem> GetQueuedEstimates(
            int revisionTypeId,
            int regionId,
            int roleId,
            string customerNumber,
            string contractNumber,
            int salesConsultantId,
            string lotNumber,
            string streetName,
            string suburb, string businessUnit)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetQueuedEstimates(
                revisionTypeId,
                regionId,
                roleId,
                customerNumber,
                contractNumber,
                salesConsultantId,
                lotNumber,
                streetName,
                suburb,
                businessUnit).ToList();
        }

        

        public List<EstimateGridItem> GetAssignedEstimates(
            int revisionTypeId,
            int roleId,
            int statusId,
            int userId,
            int regionId,
            string customerNumber,
            string contractNumber,
            int salesConsultantId,
            string lotNumber,
            string streetName,
            string suburb, string businessUnit)
        {
            return Performant(
                $"GetAssignedEstimates#{revisionTypeId}#{roleId}#{statusId}#{userId}#{regionId}#{customerNumber}" +
                        $"#{contractNumber}#{salesConsultantId}#{lotNumber}#{streetName}#{suburb}#{businessUnit}",
                () =>
                {
                    RetailSystemClient client = new RetailSystemClient();
                    try
                    {
                        return client.GetAssignedEstimates(
                            revisionTypeId,
                            roleId,
                            statusId,
                            userId,
                            regionId,
                            customerNumber,
                            contractNumber,
                            salesConsultantId,
                            lotNumber,
                            streetName,
                            suburb,
                            businessUnit
                            ).ToList();
                    }catch(Exception ex)
                    { return null; }
                });
        }

        public EstimateHeader GetEstimateHeader(int revisionId)
        {
            return Performant(
                $"GetEstimateHeader#{revisionId}",
                () =>
                {
                    RetailSystemClient client = new RetailSystemClient();
                    return client.GetEstimateHeader(revisionId);
                });
        }

        public List<EstimateDetails> GetEstimateDetails(int revisionId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetEstimateDetails(revisionId).ToList();
        }

        public List<EstimateHeader> GetEstimatesRevisions(int estimateId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetEstimatesRevisions(estimateId).ToList();
        }

        public string UndoThisRevision(int bcContractNumber, int estimateId, int estimateRevisionId, int userId, string reasonComment)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.UndoThisRevision(bcContractNumber, estimateId, estimateRevisionId, userId, reasonComment);
        }

        public List<EstimateHeader> UndoThisRevisionValidate(int estimateId, int bcContractNumber, int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.UndoThisRevisionValidate(estimateId, bcContractNumber, estimateRevisionId).ToList();
        }

        public string UndoCurrentMilestone(int estimateRevisionId, int userId, string reasonComment)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.UndoCurrentMilestone(estimateRevisionId, userId, reasonComment);
        }

        public string UndoSetAsContract(int estimateRevisionId, int userId, string reasonComment)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.UndoSetAsContract(estimateRevisionId, userId, reasonComment);
        }

        public void UpdateEstimateDetails(int revisionDetailsId,
                                        decimal price,
                                        decimal quantity,
                                        decimal totalprice,
                                        string productDescription,
                                        string extraDescription,
                                        string internalDescription,
                                        string additionalnotes,
                                        string studioManswer,
                                        int itemaccepted,
                                        int categoryid,
                                        int groupid,
                                        int pricedisplayid,
                                        int userId,
                                        int applyanswertoallgroup,
                                        string selectedimageid,
                                        bool issiteworkitem,
                                        string costbtp,
                                        string costdbc)
        {
            RetailSystemClient client = new RetailSystemClient();
            client.UpdateEstimateDetails(revisionDetailsId,
                                        price,
                                        quantity,
                                        totalprice,
                                        productDescription,
                                        extraDescription,
                                        internalDescription,
                                        additionalnotes,
                                        studioManswer,
                                        itemaccepted,
                                        categoryid,
                                        groupid,
                                        pricedisplayid,
                                        userId,
                                        applyanswertoallgroup,
                                        selectedimageid,
                                        issiteworkitem,
                                        costbtp,
                                        costdbc);
        }

        public EstimateDetails DeleteProduct(int revisionDetailsId, string reason, int reasonid, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.DeleteProduct(revisionDetailsId, reason, reasonid, userId);
        }

        public void InsertComment(int revisionId, string comment, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.InsertComment(revisionId, comment, userId); 
        }

        public void UpdateComment(int estimateRevisionId, string comment, int userid, int variationnumber, string variationsummary)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateComment(estimateRevisionId, comment, userid,variationnumber, variationsummary);
        }


        public void UpdateEstimateDifficultyRating(int estimateRevisionId, int difficultyRatingId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateEstimateDifficultyRating(estimateRevisionId, difficultyRatingId, userId);
        }


        public void UpdateQueueDifficultyRating(int queueId, int difficultyRatingId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateQueueDifficultyRating(queueId, difficultyRatingId);
        }

        public void MarginReport_SaveDetails(int estimateRevisionId, int titledLand, int titledLandDays, int basePriceExtensionDays, DateTime effectiveDate, double bpeCharge, int requiredBPEChargeType, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.MarginReport_SaveDetails(estimateRevisionId, titledLand, titledLandDays, basePriceExtensionDays, effectiveDate, bpeCharge, requiredBPEChargeType, userId);
        }

        public MarginReportDetail MarginReport_GetDetails(int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.MarginReport_GetDetails(estimateRevisionId);
        }

        public void UpdateEstimateDueDate(int estimateRevisionId, DateTime duedate, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateEstimateDueDate(estimateRevisionId, duedate, userId);
        }

        public void UpdateEstimateAppointmentTime(int estimateRevisionId, DateTime appointmentTime, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateEstimateAppointmentTime(estimateRevisionId, appointmentTime, userId);
        }

        public void UpdateQueueDueDate(int queueId, DateTime duedate, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateQueueDueDate(queueId, duedate, userid);
        }

        public void UpdateEstimateStatus(int revisionId, int statusId, int statusReasonId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateEstimateStatus(revisionId, statusId, statusReasonId, userId);
        }

        public void UpdateEstimateEffectiveDate(int estimateRevisionId, int priceId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UpdateEstimateEffectiveDate(estimateRevisionId, priceId, userId);
        }

        public List<User> GetUsersByRegionAndRole(int regionId, int roleId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetUsersByRegionAndRole(regionId, roleId).ToList();
        }

        public List<User> GetUsersByRegionAndRevisionType(int regionId, int revisionTypeId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetUsersByRegionAndRevisionType(regionId, revisionTypeId).ToList();
        }

        public User GetCurrentUser(string loginName)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetCurrentUser(loginName);
        }

        public List<UserRole> GetUserRoles(int userId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetUserRoles(userId).ToList();
        }

        public List<EstimateStatus> GetEstimateStatuses()
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetEstimateStatuses().ToList();
        }

        public List<StatusReason> GetStatusReasons(int statusId, int revisionTypeId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetStatusReasons(statusId, revisionTypeId).ToList();
        }

        public List<DifficultyRating> GetDifficultyRatings()
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetDifficultyRatings().ToList();
        }

        public List<RevisionType> GetRevisionTypeAccess(int roleId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetRevisionTypeAccess(roleId).ToList();
        }

        public List<HomePrice> GetHomePrices(int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetHomePrices(estimateRevisionId).ToList();
        }

        public List<AuditLog> GetAuditTrail(int estimateId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetAuditTrail(estimateId).ToList();
        }

        public List<EstimateDetailsComparison> CompareEstimateDetails(int estimateRevisionIdA, int estimateRevisionIdB)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.CompareEstimateDetails(estimateRevisionIdA, estimateRevisionIdB).ToList();
        }

        public List<EstimateHeaderComparison> CompareEstimateHeader(int estimateRevisionIdA, int estimateRevisionIdB)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.CompareEstimateHeader(estimateRevisionIdA, estimateRevisionIdB).ToList();
        }

        public int GetLatestEstimateRevisionId(int estimateId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetLatestEstimateRevisionId(estimateId);
        }

        public int GetResubmittedEstimateCount(int userId, int regionId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetResubmittedEstimateCount(userId, regionId);
        }

        public List<OptionTreeProducts> GetOptionTreeFromMasterHome(string regionid, string homeid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetOptionTreeFromMasterHome(regionid, homeid).ToList();
        }

        public List<OptionTreeProducts> GetOptionTreeFromAllProducts(string regionid, string searchText)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetOptionTreeFromAllProducts(regionid, searchText).ToList();
        }

        public List<OptionTreeProducts> GetOptionTreeFromAllProductsExtended(int stateid, string regionid, int homeid, string productname, string productdesc, int areaid, int groupid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetOptionTreeFromAllProductsExtended(stateid, regionid, homeid, productname, productdesc, areaid, groupid).ToList();
        }

        

        public Boolean CheckValidProductByRevision(int revisionId, string productId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CheckValidProductByRevision(revisionId, productId);
        }

        public List<OptionTreeProducts> GetOptionTreeAsOptionTreeProductsForEstimateItemReplace(string revisionId, string areaName, string groupName)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetOptionTreeAsOptionTreeProductsForEstimateItemReplace(revisionId, areaName, groupName).ToList();
        }

        public List<OptionTreeProducts> GetOptionTreeAsOptionTreeProducts(string revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetOptionTreeAsOptionTreeProducts(revisionid).ToList();
        }

        public List<PAG> GetSelectedPAG(string estimateid, string revisionnumber)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetSelectedPAG(estimateid, revisionnumber).ToList();
        }

        public List<EstimateComments> GetCommentsForAnEstimate(string revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetCommentsForAnEstimate(revisionid).ToList();
        }
        public bool GetAccessPermission(string revisionid, string userid, string roleid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAccessPermission(revisionid, userid, roleid);
        }

        public List<int> GetEstimateCount(int userId, int roleId)
        {
            return Performant($"GetEstimateCount#{userId}#{roleId}",
                () =>
                {
                    try
                    {
                        RetailSystemClient client = new RetailSystemClient();
                        return client.GetEstimateCount(userId, roleId).ToList();
                    }
                    catch(Exception)
                    {
                        return null;
                    }
                });
        }

        public int SaveSelectedItem(int selectedid, int revisionid, int pagid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.SaveSelectedItem(selectedid, revisionid, pagid, userid);
        }

        public bool SaveEditItemDetails(int selectedid, int revisionid, decimal qty, decimal sellprice, string productdescription, string extradescription, string internaldescription)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.SaveEditItemDetails(selectedid, revisionid, qty, sellprice, productdescription, extradescription, internaldescription);
        }

        public bool RemoveItem(int selectedid, int estimateid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.RemoveItem(selectedid, estimateid);
        }

        public EstimateDetails CopyItemFromOptionTreeToEstimate(int homedisplayoptionid, int revisiondetailsid, int revisionid, int productareagroupid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CopyItemFromOptionTreeToEstimate(homedisplayoptionid, revisiondetailsid, revisionid, productareagroupid, userid);
        }

        public EstimateDetails CopyItemFromMasterHomeToEstimate(int regionid, int optionid, int revisionid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CopyItemFromMasterHomeToEstimate(regionid, optionid, revisionid, userid);
        }

        public EstimateDetails CopyItemFromAllProductsToEstimate(int regionid, string productid, int revisionid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CopyItemFromAllProductsToEstimate(regionid, productid, revisionid, userid);
        }

        public bool SynchronizeNewOptionToEstimate(int revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.SynchronizeNewOptionToEstimate(revisionid);
        }

        public List<EstimateDetails> GetAdditionalNotesTemplateAndProducts(int revisionid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAdditionalNotesTemplateAndProducts(revisionid, userid).ToList();
        }

        public List<EstimateDetails> GetAdditionalNotesTemplateAndProductsByRegion(string templatename, string subregionid, int userid, int active, int selectedroleid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAdditionalNotesTemplateAndProductsByRegion(templatename, subregionid, userid, active, selectedroleid).ToList();
        }

        public bool AddAdditonalNotesTemplate(string templatename, int revisionid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.AddAdditonalNotesTemplate(templatename, revisionid, userid);
        }



        public bool CreateTaskForContract(string contractid, int revisionid, string subject, DateTime duedate, string category, string notes)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.CreateTaskForContract(contractid, revisionid, subject, duedate, category, notes);
        }


        public List<SQSSalesRegion> GetSalesRegionByState(string stateid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetSalesRegionByState(stateid).ToList();
        }

        public List<SQSSalesRegion> GetPriceRegionByState(string stateid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetPriceRegionByState(stateid).ToList();
        }
        public bool CreateSalesEstimateLog(string username, MRSLogAction action, int estimateRevisionId, string extraDescription, int reasonCode)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.CreateSalesEstimateLog(username, action, estimateRevisionId, extraDescription, reasonCode);
        }


        public bool SetContractStatus(string username, int estimateRevisionId, ContractStatus status)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.SetContractStatus(username, estimateRevisionId, status);
        }

        public bool RemoveItemFromNotesTemplate(string templateid, string productareagroupid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.RemoveItemFromNotesTemplate(templateid, productareagroupid, userid);
        }

        public bool UpdateNotesTemplateItem(string templateid, string productareagroupid, decimal quanitity, decimal price, string extradescription, string internaldescription, string additionalinfo, int userid, bool usedefaultquantity)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.UpdateNotesTemplateItem(templateid, productareagroupid, quanitity, price, extradescription, internaldescription,additionalinfo,  userid, usedefaultquantity);
        }
        public List<EstimateDetails> GetAvailableItemsForNotesTemplate(string templateid,string searchtext)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAvailableItemsForNotesTemplate(templateid, searchtext).ToList();
        }

        public bool AddItemToNotesTemplate(string templateid, string selecteditemids, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.AddItemToNotesTemplate(templateid, selecteditemids, userid);
        }

        public bool AddNewNotesTemplate(string templatename, string regionid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.AddNewNotesTemplate(templatename, regionid, userid);
        }

        public bool RemoveNotesTemplate(string templateid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.RemoveNotesTemplate(templateid, userid);
        }

        public bool CopyNotesTemplate(string templatename, string regionid, int userid, string templateid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CopyNotesTemplate(templatename, regionid, userid, templateid);
        }

        public RoleAccessModule GetRoleAccessModule(int roleid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetRoleAccessModule(roleid);
        }

        

        public string SynchroniseCustomerDetails(string contractnumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.SynchroniseCustomerDetails(contractnumber);
        }


        public List<NonStandardCategory> GetNonstandardCategory()
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetNonstandardCategory().ToList();
        }

        public List<NonStandardCategory> GetNonstandardCategoryByState(int stateid, int selectedareaid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetNonstandardCategoryByState(stateid, selectedareaid).ToList();
        }

        public List<NonStandardGroup> GetNonstandardGroups(int selectedareaid, int stateid, int selectedgroupid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetNonstandardGroups(selectedareaid, stateid, selectedgroupid).ToList();
        }

        public List<PriceDisplayCode> GetPriceDisplayCodes()
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetPriceDisplayCodes().ToList();
        }

        public List<ProductImage> GetProductImages(string productid, int supplierid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetProductImages(productid, supplierid).ToList();
        }

        public string CheckEstimateLockStatus(int estimaterevisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CheckEstimateLockStatus(estimaterevisionid);
        }

        public void UnlockEstimate(int estimaterevisionid, int type)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.UnlockEstimate(estimaterevisionid, type);
        }

        public List<ValidationErrorMessage> ValidateStudioMEstimate(int estimaterevisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ValidateStudioMEstimate(estimaterevisionid).ToList();
        }

        public bool UpdateItemAcceptance(string revisionestimatedetailsid, int accepted, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.UpdateItemAcceptance(revisionestimatedetailsid, accepted, userid);
        }

        public ContractDraftActionAvailability GetContractDraftActionAvailability(int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetContractDraftActionAvailability(estimateRevisionId);
        }

        public bool GetContractDraftCreationVisibility(int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetContractDraftCreationVisibility(estimateRevisionId);
        }

        public bool GetFinalContractCreationVisibility(int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetFinalContractCreationVisibility(estimateRevisionId);
        }

        public FinalContractActionAvailability GetFinalContractActionAvailability(int estimateRevisionId, string contractNumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetFinalContractActionAvailability(estimateRevisionId, contractNumber);
        }

        public CustomerSupportActionAvailability GetCustomerSupportActionAvailability(int estimateRevisionId, string contractNumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetCustomerSupportActionAvailability(estimateRevisionId, contractNumber);
        }

        public SalesEstimatorActionAvailability GetSalesEstimatorActionAvailability(int estimateRevisionId, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetSalesEstimatorActionAvailability(estimateRevisionId, userid);
        }

        public void CreateSplitStudioMRevisions(int estimateRevisionId, string revisionTypeIds, string assignedToUserIds, int createdbyId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.CreateSplitStudioMRevisions(estimateRevisionId, revisionTypeIds, assignedToUserIds, createdbyId);
        }

        public void MergeStudioMRevisions(int estimateRevisionId, int createdbyId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.MergeStudioMRevisions(estimateRevisionId, createdbyId);
        }

        public void CreateContractDraft(int estimateRevisionId, int createdbyId/*, DateTime appointment*/)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.CreateContractDraft(estimateRevisionId, createdbyId/*, appointment*/);
        }

        public void CreateFinalContract(int estimateRevisionId, int createdbyId/*, DateTime appointment*/)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.CreateFinalContract(estimateRevisionId, createdbyId/*, appointment*/);
        }

        public void CreateCscVariation(int estimateRevisionId, int createdbyId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.CreateCscVariation(estimateRevisionId, createdbyId);
        }

        public string CreateStudioMRevision(int estimateRevisionId, int ownerId, DateTime appointmentDateTime, int revisionTypeId, int createdbyId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CreateStudioMRevision(estimateRevisionId, ownerId, appointmentDateTime, revisionTypeId, createdbyId);
        }

        public string ValidateSetEstimateStatus(int estimateRevisionId, int nextRevisionTypeId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ValidateSetEstimateStatus(estimateRevisionId, nextRevisionTypeId);
        }

        public void CreateVariation(int estimateRevisionId, int revisionTypeId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.CreateVariation(estimateRevisionId, revisionTypeId, userId);
        }

        public void RejectVariation(int estimateRevisionId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.RejectVariation(estimateRevisionId, userId);
        }

        public string GetCustomerDocumentType(int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetCustomerDocumentType(estimateRevisionId);
        }

        public int UpdateCustomerDocumentDetails(CustomerDocumentDetails document)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.UpdateCustomerDocumentDetails(document);
        }

        public CustomerDocumentDetails GetCustomerDocumentDetails(int estimateRevisionId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetCustomerDocumentDetails(estimateRevisionId);
        }

        //Dummy function to allow client access to EstimateRevisionStatus
        public void GetEstimateRevisionStatus(EstimateRevisionStatus status)
        {
            RetailSystemClient client = new RetailSystemClient();

            client.GetEstimateRevisionStatus(status);
        }

        public List<SimplePAG> GetRelevantPAGFromOnePAG(string estimatedetailsid, string revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetRelevantPAGFromOnePAG(estimatedetailsid, revisionid).ToList();
        }

        public List<EstimateDetails> SaveSelectedItemsFromOptionTreeToEstimate(string optionidstring, 
            string standardinclusionidstring, 
            string revisionid, 
            string studiomanswer, 
            string userid, 
            string action, 
            string derivedcost, 
            string costbtpexcgststring,
            string costbtpoverwriteflagstring,
            string costdbcexcgststring,
            string costdbcoverwriteflagstring,
            string quantitystring,
            string pricestring,
            string isacceptedstring,
            string areaidstring,
            string groupidstring,
            string pricedisplaycodestring,
            string issiteworkstring,
            string productdescriptionstring,
            string additionalnotestring,
            string extradescriptionstring,
            string internaldescriptionstring)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.SaveSelectedItemsFromOptionTreeToEstimate(optionidstring, 
                standardinclusionidstring, 
                revisionid, 
                studiomanswer, 
                userid,
                action, 
                derivedcost, 
                costbtpexcgststring,
                costbtpoverwriteflagstring,
                costdbcexcgststring,
                costdbcoverwriteflagstring,
                quantitystring,
                pricestring,
                isacceptedstring,
                areaidstring,
                groupidstring,
                pricedisplaycodestring,
                issiteworkstring,
                productdescriptionstring,
                additionalnotestring,
                extradescriptionstring,
                internaldescriptionstring).ToList();
        }
        public List<EstimateDetails> ReplaceSaveSelectedItemsFromOptionTreeToEstimate(string sourceEstimateRevisionDetailsId, string optionidstring,
                string standardinclusionidstring,
                string revisionid,
                string studiomanswer,
                string userid,
                string action,
                string derivedcost,
                string costbtpexcgst,
                string costdbcexcgst,
                string quantitystring,
                string pricestring,
                string isacceptedstring,
                string areaidstring,
                string groupidstring,
                string pricedisplaycodestring,
                string issiteworkstring,
                string productdescriptionstring,
                string additionalnotestring,
                string extradescriptionstring,
                string internaldescriptionstring,
                string copyquantity,
                string copyadditionalnotes,
                string copyextradescription,
                string copyinternalnotes
            )
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ReplaceSaveSelectedItemsFromOptionTreeToEstimate(sourceEstimateRevisionDetailsId, optionidstring,
                standardinclusionidstring,
                revisionid,
                studiomanswer,
                userid,
                action,
                derivedcost,
                costbtpexcgst,
                costdbcexcgst,
                quantitystring,
                pricestring,
                isacceptedstring,
                areaidstring,
                groupidstring,
                pricedisplaycodestring,
                issiteworkstring,
                productdescriptionstring,
                additionalnotestring,
                extradescriptionstring,
                internaldescriptionstring,
                copyquantity,
                copyadditionalnotes,
                copyextradescription,
                copyinternalnotes
                ).ToList();
        }
        public RevisionTypePermission CheckRevisionTypeAllowToAddNSR(int revisontypeid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CheckRevisionTypeAllowToAddNSR(revisontypeid);
        }

        public List<ValidationErrorMessage> ValidateAcceptFlagForRevision(int estimaterevisionid, int userroleid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ValidateAcceptFlagForRevision(estimaterevisionid, userroleid).ToList();
        }

        public List<ValidationErrorMessage> ValidateStudioMRevisions(int estimaterevisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ValidateStudioMRevisions(estimaterevisionid).ToList();
        }

        public bool ValidateAppointmentDate(int estimaterevisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ValidateAppointmentDate(estimaterevisionid);
        }

        public List<NextRevision> GetNextEstimateRevision(int estimateRevisionId, int statusId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetNextEstimateRevision(estimateRevisionId, statusId).ToList();
        }

        public List<AuditLog> GetAuditLogs(int revisionid, int estimatedetailid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAuditLogs(revisionid, estimatedetailid).ToList();
        }

        public List<DeletedItem> GetDeletedItems(int revisionid, RESULT_TYPE resulttype)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetDeletedItems(revisionid, resulttype).ToList();
        }

        public bool ReAddDeletedEstimateItem(int sourceEstimateRevisionId, int targetEstimateRevisionId, int optionId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ReAddDeletedEstimateItem(sourceEstimateRevisionId, targetEstimateRevisionId, optionId, userId);
        }

        public bool ReAddDeletedMasterPromotionEstimateItem(int estimateRevisionId, int optionId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.ReAddDeletedMasterPromotionEstimateItem(estimateRevisionId, optionId, userId);
        }

        public List<SQSHome> GetAllFacadeFromRevisonID(int revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAllFacadeFromRevisonID(revisionid).ToList();
        }
        public List<SQSHome> GetAllAvailableHomeByState(int stateid, string searchText, bool showdisplayhomes)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAllAvailableHomeByState(stateid, searchText, showdisplayhomes).ToList();
        }
        public List<SQSHome> GetHomeFullNameByState(int stateid, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetHomeFullNameByState(stateid, userId).ToList();
        }
        public List<SQSArea> GetAreaNameWithAll()
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetAreaNameWithAll().ToList();
        }
        public List<SQSGroup> GetGroupNameWithAll()
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.GetGroupNameWithAll().ToList();
        }
        public List<ValidationErrorMessage> CheckFacadeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CheckFacadeConfigurationDifference(revisionid, newfacadehomeid, effectivedate).ToList();
        }
        public List<ValidationErrorMessage> CheckHomeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CheckHomeConfigurationDifference(revisionid, newfacadehomeid, effectivedate).ToList();
        }

        public bool ChangeFacade(int revisionid, int newfacadehomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();


            Trace.TraceInformation("MRSWCF-ChangeFacade - Information - revisionid={0},newfacadehomeid={1},detailIDsSelected={2},detailOptionsSeleced={3},detailPricesSeleced={4},effectivedate={5},userid={6}", revisionid, newfacadehomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);

            return client.ChangeFacade(revisionid, newfacadehomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);
        }

        public bool ChangeHome(int revisionid, int newhomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();


            Trace.TraceInformation("MRSWCF-ChangeHome - Information - revisionid={0},newhomeid={1},detailIDsSelected={2},detailOptionsSeleced={3},detailPricesSeleced={4},effectivedate={5},userid={6}", revisionid, newhomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);

            return client.ChangeHome(revisionid, newhomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);
        }

        public bool UpdateNoteTemplate(int templateid, string templatename, int status, int userid, string action)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.UpdateNoteTemplate(templateid, templatename, status, userid, action);
        }

        public string CheckNewNoteTemplateNameExists(int templateid, string templatename)
        {
            RetailSystemClient client = new RetailSystemClient();

            return client.CheckNewNoteTemplateNameExists(templateid, templatename);
        }
        public List<ContractType> GetContractType(string configCode)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetContractType(configCode).ToList();
        }

        public List<SQSConfiguration> GetSQSConfiguration(string configCode, string codeValue)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetSQSConfiguration(configCode, codeValue).ToList();
        }

        public bool UpdateContractType(int revisionid, string contracttype, string jobflowtype, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.UpdateContractType(revisionid, contracttype, jobflowtype, userid);
        }

        public string GetHomeName(int revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetHomeName(revisionid);
        }

        public bool UpdateHomeName(int revisionid, string homename, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.UpdateHomeName(revisionid, homename, userid);
        }

        public List<ValidationErrorMessage> CopyEstimateCheckDifference(string sourceEstimatenumber, string destinationEstimatenumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.CopyEstimateCheckDifference(sourceEstimatenumber, destinationEstimatenumber).ToList();
        }

        public bool CopyEstimate(string sourceEstimatenumber, string destinationEstimatenumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.CopyEstimate(sourceEstimatenumber, destinationEstimatenumber);
        }

        public List<decimal> GetAreaSurcharge(int revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetAreaSurcharge(revisionid).ToList();
        }

        public EstimateDetails GetPagByID(int estimaterevisionid, int optionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetPagByID(estimaterevisionid, optionid);
        }

        public List<SimplePAG> GetUpgradeOptionListForStandardInclusion(int estimaterevisionid, int originateoptionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetUpgradeOptionListForStandardInclusion(estimaterevisionid, originateoptionid).ToList();
        }

        public string GetStudioMQandA(int optionId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetStudioMQandA(optionId);
        }

        public string GetStudioMQuestionForAProduct(string pproductid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetStudioMQuestionForAProduct(pproductid);
        }

        public List<PAG> GetInLieuStandardPromotionItems(int estimaterevisionid, int originateoptionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetInLieuStandardPromotionItems(estimaterevisionid, originateoptionid).ToList();
        }

        public List<SharepointDoc> Sharepoint_GetFileList(string opportunityid, string contractnumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.Sharepoint_GetFileList(opportunityid, contractnumber).ToList();
        }

        public bool Sharepoint_SharepointUploadFile(string filename,byte[] contents, string oldname, string opportunityid, string doccategory, string doctype)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.Sharepoint_SharepointUploadFile(filename, contents, oldname, opportunityid, doccategory, doctype);
        }

        public bool Sharepoint_DeleteFileFromSharepointLibrary(SharepointDoc doc, string opportunityid, string contractnumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.Sharepoint_DeleteFileFromSharepointLibrary(doc, opportunityid, contractnumber);
        }

        public List<SharepointDocumentType> Sharepoint_GetSalesDocumentType()
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.Sharepoint_GetSalesDocumentType().ToList();
        }

        public List<ItemRemoveReason> GetItemRemoveReason()
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetItemRemoveReason().ToList();
        }

        public List<StudioMItem> GetItemsNeedSetDefaultAnswer(string revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetItemsNeedSetDefaultAnswer(revisionid).ToList(); 
        }

        public bool SetDefaultAnswerForEstimateRevision(string idstring, string studiomstring, string usercode)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.SetDefaultAnswerForEstimateRevision(idstring, studiomstring, usercode);
        }


        public bool SendNotificationEmail(string templateobjectid, string actionuserfullname, string recipientfullname, string contractnumber,  string templateobjecttype, string emailtype, string estimatenumber, string revisionnumber, string comments)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.SendNotificationEmail(templateobjectid, actionuserfullname, recipientfullname, contractnumber, templateobjecttype, emailtype, estimatenumber, revisionnumber, comments);
        }

        public bool SendCrmEmail(Guid contractId, int recipientId, string subject, string body)
        {
            // this fucntion was commented on 27/01/2015 as we decide use SQL to send notification email.
            return true;
        }

        public void RegisterEvent(string action, int revisionid, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();
            client.RegisterEvent(action, revisionid, userid);
        }

        public DateTime GetStudioMAppointmentTime(string contractNumber, string eventNumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetStudioMAppointmentTime(contractNumber, eventNumber);
        }

        public void UpdateEstimateDetailsDescription(List<EstimateDetails> items, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();
            client.UpdateEstimateDetailsDescription(items.ToArray(), userId);
        }

        public bool MoveEstimateDetailItem(int revisionDetailsIdSource, int revisionDetailsIdTarget, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();


            return client.MoveEstimateDetailItem(revisionDetailsIdSource,
                                        revisionDetailsIdTarget,
                                        userId);
        }

        public List<DocuSignDocStatusInfo> DocuSign_GetDocumentInfo(string revisionid, string estimateid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.DocuSign_GetDocumentInfo(revisionid, estimateid).ToList();
        }

        public bool DocuSign_PushDocumentToTheProcessQueue(string revisionid, string printtype, string documenttype, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.DocuSign_PushDocumentToTheProcessQueue(revisionid, printtype, documenttype, userid);
        }

        public List<EstimateDetails> GetPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetPromotionProductByMasterPromotionRevisionDetailsID(revisiondetailsid).ToList();
        }

        public List<PromotionPAG> GetExistingPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetExistingPromotionProductByMasterPromotionRevisionDetailsID(revisiondetailsid).ToList();
        }

        public List<EstimateDetails> GetEstimateDetailsByIDString(string selectedrevisiondetailsid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetEstimateDetailsByIDString(selectedrevisiondetailsid).ToList();
        }

        public bool DeleteMasterPromotionItem(string masterpromotionitemid, string selectedpromotionitemids, int userid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.DeleteMasterPromotionItem(masterpromotionitemid, selectedpromotionitemids,userid);            
        }

        public bool DocuSign_RemoveDocumentFromTheProcessQueue(string integrationid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.DocuSign_RemoveDocumentFromTheProcessQueue(integrationid);
        }

        public List<CRMContact> GetCRMContactForAccountAsSigner(Guid accountid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetCRMContactForAccountAsSigner(accountid).ToList();
        }

        public List<DocuSignHistory> DocuSign_GetEnvelopeHistory(string envelopeid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.DocuSign_GetEnvelopeHistory(envelopeid).ToList();
        }

        public List<DocuSignHistory> DocuSign_GetEnvelopeHistoryByRevision(string revisionid, string versiontype, string printtype)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.DocuSign_GetEnvelopeHistoryByRevision(revisionid, versiontype, printtype).ToList();
        }

        public string DocuSign_ValidateSignerAndDocuemnt(string estimateid, string versionnumber, string recipientname, string recipienttype, string recipientaction)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.DocuSign_ValidateSignerAndDocuemnt(estimateid,versionnumber,recipientname,recipienttype,recipientaction);
        }

        public List<EstimateGridItem> SearchSpecificJob(string customernumber, string contractnumber, string SelectedSalesConsultantId, string LotNumber, string StreetName,string Suburb, string businessUnit)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.SearchSpecificJob(customernumber, contractnumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb, businessUnit).ToList();
        }

        public int ResetEditEstimateUserID(int estimateRevisionId, int editEstimateUserID)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.ResetEditEstimateUserID(estimateRevisionId, editEstimateUserID);
        }

        public bool ApplyRounding(int revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.ApplyRounding(revisionid);
        }

        public EstimateDisclaimerUpdateDetail GetEstimateDisclaimerUpdateDetails(int revisionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetEstimateDisclaimerUpdateDetails(revisionid);
        }

        public bool SaveDisclaimerUpdateDetails(int revisionId, int typeId, int disclaimerNewId, int userId)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.SaveDisclaimerUpdateDetails(revisionId, typeId, disclaimerNewId, userId);
        }

        public List<GenericClassCodeName> GetBusinessUnits(int regionid)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetBusinessUnits(regionid).ToList();
        }

        public string GetBCForecastDate(string bcContractNumber)
        {
            RetailSystemClient client = new RetailSystemClient();
            return client.GetBCForecastDate(bcContractNumber);
        }


        private T Performant<T>(string key, Func<T> action)
        {
            Interlocked.Increment(ref CurrentRequestCount);
            try
            {
                try
                {
                    if (_cacheService.KeyExists(key))
                        return _cacheService.Get<T>(key);
                }
                catch (Exception ex)
                {
                    //Log it - Serilog wtih DB logging.
                }

                T returnValue = action();
                try
                {
                    _cacheService.SetAsync(key, returnValue);
                }
                catch (Exception ex)
                {
                    //Log it - Serilog to put the same in database.
                }
                return returnValue;
            }
            finally
            {
                //Log the key usage information
                Interlocked.Decrement(ref CurrentRequestCount);
            }
        }

    }
}
