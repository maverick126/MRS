using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Configuration;
using System.Data;

using Metricon.WCF.MetriconRetailSystem.Contracts;
using Metricon.DAL.MetriconRetailSystem;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Xrm;
//using API;
//using API.Authentication;
using Microsoft.Crm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using System.Diagnostics;
using RestSharp;

namespace Metricon.WCF.MetriconRetailSystem.Services
{
    // Add the attribute to the service implementations:
    [ServiceErrorBehaviour(typeof(HttpErrorHandler))]
    [AiLogException]
    public class RetailSystem : IRetailSystem
    {
        private const int EstimateStatusAccepted = 2;

        public void AcceptOriginalEstimate(int estimateId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            int estimateHeaderId = dataAccess.AcceptOriginalEstimate(estimateId);
            this.CompleteEstimate(estimateHeaderId, userId, EstimateStatusAccepted, 0, 2, 0);
        }

        public int AssignQueuedEstimate(int queueId, int userId, int ownerId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.AssignQueuedEstimate(queueId, userId, ownerId); 
        }

        public void AssignWorkingEstimate(int estimateRevisionId, int userId, int ownerId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.AssignWorkingEstimate(estimateRevisionId, userId, ownerId);
        }

        public void CompleteEstimate(int revisionId, int userId, int statusId, int statusReasonId, int revisionTypeId, int ownerId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            BcDataAccess bcaccess = new BcDataAccess();
            string contractnumber="";
            string eventcode="";
            string staffcode = "";

            Trace.TraceInformation("MRSWCF-CompleteEstimate - Information - revisionId={0}, userId={1}, statusId={2}, statusReasonId={3},  revisionTypeId={4}, ownerId={5}", revisionId, userId, statusId, statusReasonId, revisionTypeId, ownerId);

            if (ownerId <= 0)
            {
                dataAccess.GetContractEventFromRevisonID(revisionId.ToString(), out contractnumber, out eventcode);
                if (contractnumber != "" && eventcode != "")
                {
                    staffcode = bcaccess.GetStaffCodeFromContractNumberAndEvent(contractnumber, eventcode);
                }
                if (staffcode != "")
                {
                    ownerId = dataAccess.GetUserIDFromUserCode(staffcode);
                    User u = dataAccess.GetUserById(ownerId);
                    string description = string.Format("Sales Estimate was assigned to {0}", u.FullName)+" (BusinessCraft).";
                    CreateSalesEstimateLog(u.FullName, MRSLogAction.Accept, revisionId, description, 0);
                }
            }

            dataAccess.CompleteEstimate(revisionId, userId, statusId, statusReasonId, revisionTypeId, ownerId);
            
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
            Trace.TraceInformation("MRSWCF-GetQueuedEstimates - Information - revisionTypeId={0}, roleId={1}, regionId={2}, customerNumber={3}, contractNumber={4}, salesConsultantId={5}, lotNumber={6}, streetName={7}, suburb={8}, businessUnit={9}", revisionTypeId, roleId, regionId, customerNumber, contractNumber, salesConsultantId, lotNumber, streetName, suburb, businessUnit);
            
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetQueuedEstimates(
                revisionTypeId,
                regionId,
                roleId,
                customerNumber,
                contractNumber,
                salesConsultantId,
                lotNumber,
                streetName,
                suburb,
                businessUnit);
        }

        //public List<EstimateGridItem> GetAssignedEstimatesByUser(
        //    int revisionTypeId,
        //    int roleId,
        //    int statusId, 
        //    int userId,            
        //    string customerNumber,
        //    string contractNumber,
        //    int salesConsultantId,
        //    string lotNumber,
        //    string streetName,
        //    string suburb)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetAssignedEstimates(
        //        revisionTypeId,
        //        roleId,
        //        statusId, 
        //        userId, 
        //        0,
        //        customerNumber,
        //        contractNumber,
        //        salesConsultantId,
        //        lotNumber,
        //        streetName,
        //        suburb);
        //}

        //public List<EstimateGridItem> GetAssignedEstimatesByRegion(
        //    int revisionTypeId,
        //    int roleId,
        //    int statusId, 
        //    int regionId, 
        //    string customerNumber,
        //    string contractNumber,
        //    int salesConsultantId,
        //    string lotNumber,
        //    string streetName,
        //    string suburb)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetAssignedEstimates(
        //        revisionTypeId,
        //        roleId,
        //        statusId, 
        //        0,
        //        regionId,
        //        customerNumber,
        //        contractNumber,
        //        salesConsultantId,
        //        lotNumber,
        //        streetName,
        //        suburb);
        //}

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
            System.Diagnostics.Trace.TraceInformation("MRSWCF-GetAssignedEstimates - Information - revisionTypeId={0}, roleId={1}, statusId={2}, userId={3}, regionId={4}, customerNumber={5}, contractNumber={6}, salesConsultantId={7}, lotNumber={8}, streetName={9}, suburb={10}, businessUnit={11}", revisionTypeId, roleId, statusId, userId, regionId, customerNumber, contractNumber, salesConsultantId, lotNumber, streetName, suburb, businessUnit);
            //System.Diagnostics.Trace.TraceWarning("GetAssignedEstimates - Warning");
            //System.Diagnostics.Trace.TraceError("GetAssignedEstimates - Error");

            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAssignedEstimates(
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
                );
        }

        public EstimateHeader GetEstimateHeader(int revisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            Trace.TraceInformation("MRSWCF-GetEstimateHeader - Information - revisionId={0}", revisionId);

            return dataAccess.GetEstimateHeader(revisionId);
        }

        public List<EstimateDetails> GetEstimateDetails(int revisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetEstimateDetails(revisionId);
        }

        //public List<string[]> GetEstimateDetailsAsArray(int revisionId)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetEstimateDetailsAsArray(revisionId);
        //}
        //public void CommenceWork(int revisionId, int userId)
        //{
        //    SqlDataAccess.CommenceWork(revisionId, userId);
        //}

        public List<EstimateHeader> GetEstimatesRevisions(int estimateId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetEstimateRevisions(estimateId);
        }

        public string UndoThisRevision(int bcContractNumber, int estimateId, int estimateRevisionId, int userId, string reasonComment)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            Trace.TraceInformation("MRSWCF-UndoThisRevision - Information - bcContractNumber={0},estimateId={1},estimateRevisionId={2},userId={3},reasonComment={4}", bcContractNumber, estimateId, estimateRevisionId, userId, reasonComment);

            return dataAccess.UndoThisRevision(bcContractNumber, estimateId, estimateRevisionId, userId, reasonComment);
        }

        public List<EstimateHeader> UndoThisRevisionValidate(int estimateId, int bcContractNumber, int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.UndoThisRevisionValidate(estimateId, bcContractNumber, estimateRevisionId);
        }

        public string UndoCurrentMilestone(int estimateRevisionId, int userId, string reasonComment)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            Trace.TraceInformation("MRSWCF-UndoCurrentMilestone - Information - estimateRevisionId={0}, userId={1}, reasonComment={2}", estimateRevisionId, userId, reasonComment);

            return dataAccess.UndoCurrentMilestone(estimateRevisionId, userId, reasonComment);
        }

        public string UndoSetAsContract(int estimateRevisionId, int userId, string reasonComment)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            Trace.TraceInformation("MRSWCF-UndoSetAsContract - Information - estimateRevisionId={0}, userId={1}, reasonComment={2}", estimateRevisionId, userId, reasonComment);

            return dataAccess.UndoSetAsContract(estimateRevisionId, userId, reasonComment);
        }
        //public void InsertProduct(int revisionId, int estimateDetailsId, int userId)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    dataAccess.InsertProduct(revisionId, estimateDetailsId, userId);
        //}

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
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateEstimateDetails(revisionDetailsId,
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
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.DeleteProduct(revisionDetailsId, reason, reasonid, userId);
        }

        //public EstimateDetails DeleteProducts(int revisionDetailsId, string estimatedetailsidstring, string areaidstring, string groupidstring, string reason, int reasonid, int userId)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.DeleteProducts(revisionDetailsId, estimatedetailsidstring, areaidstring, groupidstring, reason, reasonid, userId);
        //}

        public void InsertComment(int revisionId, string comment, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.InsertComment(revisionId, comment, userId); 
        }

        public void UpdateComment(int estimateRevisionId, string comment, int userid, int variationnumber, string variationsummary)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateComment(estimateRevisionId, comment, userid,variationnumber, variationsummary);
        }


        public void UpdateEstimateDifficultyRating(int estimateRevisionId, int difficultyRatingId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateEstimateDifficultyRating(estimateRevisionId, difficultyRatingId, userId);
        }


        public void UpdateQueueDifficultyRating(int queueId, int difficultyRatingId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateQueueDifficultyRating(queueId, difficultyRatingId);
        }

        public void MarginReport_SaveDetails(int estimateRevisionId, int titledLand, int titledLandDays, int basePriceExtensionDays, DateTime effectiveDate, double bpeCharge, int requiredBPEChargeType, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.MarginReport_SaveDetails(estimateRevisionId, titledLand, titledLandDays, basePriceExtensionDays, effectiveDate, bpeCharge, requiredBPEChargeType, userId);
        }

        public MarginReportDetail MarginReport_GetDetails(int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.MarginReport_GetDetails(estimateRevisionId);
        }

        public void UpdateEstimateDueDate(int estimateRevisionId, DateTime duedate, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateEstimateDueDate(estimateRevisionId, duedate, userId);
        }

        public void UpdateEstimateAppointmentTime(int estimateRevisionId, DateTime appointmentTime, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateEstimateAppointmentTime(estimateRevisionId, appointmentTime, userId);
        }

        public void UpdateQueueDueDate(int queueId, DateTime duedate, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateQueueDueDate(queueId, duedate, userid);
        }

        public void UpdateEstimateStatus(int revisionId, int statusId, int statusReasonId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateEstimateStatus(revisionId, statusId, statusReasonId, userId);
        }

        public void UpdateEstimateEffectiveDate(int estimateRevisionId, int priceId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UpdateEstimateEffectiveDate(estimateRevisionId, priceId, userId);
        }

        //public bool CheckEstimateUpdatability(int revisionId)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.CheckEstimateUpdatability(revisionId);
        //}

        //public void ReEditEstimate(int revisionId, int userId)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    dataAccess.ReEditEstimate(revisionId, userId);
        //}

        public List<User> GetUsersByRegionAndRole(int regionId, int roleId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetUsersByRegionAndRole(regionId, roleId);
        }

        public List<User> GetUsersByRegionAndRevisionType(int regionId, int revisionTypeId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetUsersByRegionAndRevisionType(regionId, revisionTypeId);
        }

        public User GetCurrentUser(string loginName)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetUserByLoginName(loginName);
        }

        public List<UserRole> GetUserRoles(int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetUserRoles(userId);
        }

        public List<EstimateStatus> GetEstimateStatuses()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetEstimateStatuses();
        }

        public List<StatusReason> GetStatusReasons(int statusId, int revisionTypeId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetStatusReasons(statusId, revisionTypeId);
        }

        public List<DifficultyRating> GetDifficultyRatings()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetDifficultyRatings();
        }

        public List<RevisionType> GetRevisionTypeAccess(int roleId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetRevisionTypeAccess(roleId);
        }

        public List<HomePrice> GetHomePrices(int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetHomePrices(estimateRevisionId);
        }

        public List<AuditLog> GetAuditTrail(int estimateId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAuditTrail(estimateId);
        }

        public List<EstimateDetailsComparison> CompareEstimateDetails(int estimateRevisionIdA, int estimateRevisionIdB)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CompareEstimateDetails(estimateRevisionIdA, estimateRevisionIdB);
        }

        public List<EstimateHeaderComparison> CompareEstimateHeader(int estimateRevisionIdA, int estimateRevisionIdB)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            List<EstimateHeaderComparison> headerComparisons = new List<EstimateHeaderComparison>();

            EstimateHeader headerA = dataAccess.GetEstimateHeader(estimateRevisionIdA);
            EstimateHeader headerB = dataAccess.GetEstimateHeader(estimateRevisionIdB);

            EstimateHeaderComparison effComparison = new EstimateHeaderComparison();
            effComparison.FieldName = "Price Effective Date";
            effComparison.ValueA = headerA.EffectiveDate.ToString("dd/MM/yyyy");
            effComparison.ValueB = headerB.EffectiveDate.ToString("dd/MM/yyyy");
            headerComparisons.Add(effComparison);

            EstimateHeaderComparison homePriceComparison = new EstimateHeaderComparison();
            homePriceComparison.FieldName = "Home Price";
            homePriceComparison.ValueA = headerA.HomePrice.ToString("c");
            homePriceComparison.ValueB = headerB.HomePrice.ToString("c");
            headerComparisons.Add(homePriceComparison);

            EstimateHeaderComparison upgradeValueComparison = new EstimateHeaderComparison();
            upgradeValueComparison.FieldName = "Upgrade Value";
            upgradeValueComparison.ValueA = headerA.UpgradeValue.ToString("c");
            upgradeValueComparison.ValueB = headerB.UpgradeValue.ToString("c");
            headerComparisons.Add(upgradeValueComparison);

            EstimateHeaderComparison siteWorkComparison = new EstimateHeaderComparison();
            siteWorkComparison.FieldName = "Site Work Value";
            siteWorkComparison.ValueA = headerA.SiteWorkValue.ToString("c");
            siteWorkComparison.ValueB = headerB.SiteWorkValue.ToString("c");
            headerComparisons.Add(siteWorkComparison);

            EstimateHeaderComparison totalPriceComparison = new EstimateHeaderComparison();
            totalPriceComparison.FieldName = "Total Price";
            totalPriceComparison.ValueA = headerA.TotalPrice.ToString("c");
            totalPriceComparison.ValueB = headerB.TotalPrice.ToString("c");
            headerComparisons.Add(totalPriceComparison);

            return headerComparisons;
        }

        public int GetLatestEstimateRevisionId(int estimateId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetLatestEstimateRevisionId(estimateId);
        }

        public int GetResubmittedEstimateCount(int userId, int regionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetResubmittedEstimateCount(userId, regionId);
        }

        //public int GetOnHoldEstimateCount(string revisionTypeIds, //comma separated i.e. '1, 2'
        //                                int userId, 
        //                                int regionId)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetOnHoldEstimateCount(revisionTypeIds, userId, regionId);
        //}

        //public List<PAG> GetOptionTree(string revisionid)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetOptionTree(revisionid);
        //}

        //public List<EstimateDetails> GetOptionTreeAsEstimateDetails(string revisionid)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetOptionTreeAsEstimateDetails(revisionid);
        //}

        public List<OptionTreeProducts> GetOptionTreeFromMasterHome(string regionid, string homeid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetOptionTreeFromMasterHome(regionid, homeid);
        }

        public List<OptionTreeProducts> GetOptionTreeFromAllProducts(string regionid, string searchText)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetOptionTreeFromAllProducts(regionid, searchText);
        }

        public List<OptionTreeProducts> GetOptionTreeFromAllProductsExtended(int stateid, string regionid, int homeid, string productname, string productdesc, int areaid, int groupid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetOptionTreeFromAllProductsExtended(stateid, regionid, homeid, productname, productdesc, areaid, groupid);
        }

        //public List<string[]> GetOptionTreeAsArray(string revisionid)
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetOptionTreeAsArray(revisionid);
        //}

        public Boolean CheckValidProductByRevision(int revisionId, string productId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CheckValidProductByRevision(revisionId, productId);
        }

        public List<OptionTreeProducts> GetOptionTreeAsOptionTreeProductsForEstimateItemReplace(string revisionId, string areaName, string groupName)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetOptionTreeAsOptionTreeProductsForEstimateItemReplace(revisionId, areaName, groupName);
        }

        public List<OptionTreeProducts> GetOptionTreeAsOptionTreeProducts(string revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetOptionTreeAsOptionTreeProducts(revisionid);
        }

        public List<PAG> GetSelectedPAG(string estimateid, string revisionnumber)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetSelectedPAG(estimateid, revisionnumber);
        }

        public List<EstimateComments> GetCommentsForAnEstimate(string revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetCommentsForAnEstimate(revisionid);
        }
        public bool GetAccessPermission(string revisionid, string userid, string roleid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAccessPermission(revisionid, userid, roleid);
        }

        public List<int> GetEstimateCount(int userId, int roleId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetEstimateCount(userId, roleId);
        }

        public int SaveSelectedItem(int selectedid, int revisionid, int pagid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.SaveSelectedItem(selectedid, revisionid, pagid, userid);
        }

        public bool SaveEditItemDetails(int selectedid, int revisionid, decimal qty, decimal sellprice, string productdescription, string extradescription, string internaldescription)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.SaveEditItemDetails(selectedid, revisionid, qty, sellprice, productdescription, extradescription, internaldescription);
        }

        public bool RemoveItem(int selectedid, int estimateid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.RemoveItem(selectedid, estimateid);
        }

        public EstimateDetails CopyItemFromOptionTreeToEstimate(int homedisplayoptionid, int revisiondetailsid, int revisionid, int productareagroupid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CopyItemFromOptionTreeToEstimate(homedisplayoptionid, revisiondetailsid, revisionid, productareagroupid, userid);
        }

        public EstimateDetails CopyItemFromMasterHomeToEstimate(int regionid, int optionid, int revisionid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CopyItemFromMasterHomeToEstimate(regionid, optionid, revisionid, userid);
        }

        public EstimateDetails CopyItemFromAllProductsToEstimate(int regionid, string productid, int revisionid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CopyItemFromAllProductsToEstimate(regionid, productid, revisionid, userid);
        }

        public bool SynchronizeNewOptionToEstimate(int revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.SynchronizeNewOptionToEstimate(revisionid);
        }

        public List<EstimateDetails> GetAdditionalNotesTemplateAndProducts(int revisionid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAdditionalNotesTemplateAndProducts(revisionid, userid);
        }

        public List<EstimateDetails> GetAdditionalNotesTemplateAndProductsByRegion(string templatename, string subregionid, int userid, int active, int selectedroleid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAdditionalNotesTemplateAndProductsByRegion(templatename, subregionid, userid, active, selectedroleid);
        }

        public bool AddAdditonalNotesTemplate(string templatename, int revisionid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.AddAdditonalNotesTemplate(templatename, revisionid, userid);
        }

/*
        public bool CreateActivityForOpportunity(string opportunityid, string username, string subject, DateTime duedate,string mobilephone, string notes)
        {
            try
            {
                //CRMConnection conn;
                //API.Authentication.ESAUserToken esaToken;
                //esaToken = new ESAUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], username);

                //conn = new CRMConnection(ConfigurationManager.AppSettings["CRM_URL"],
                //                                       ConfigurationManager.AppSettings["CRM_DatabaseName"],
                //                                       new CRMUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], ConfigurationManager.AppSettings["CRM_UserToken_Username"], ConfigurationManager.AppSettings["CRM_UserToken_Password"]),
                //                                       esaToken);

                //API.ActivityInterface a = new ActivityInterface();
                //Guid returnactivityid = a.CreatePhoneActivity(conn,
                //                                              new Guid(opportunityid),
                //                                              "opportunity",
                //                                              mobilephone,
                //                                              subject,
                //                                              duedate.ToString("s"), "outgoing", string.Empty, false);
                //a.UpdatePhoneActivity(conn, returnactivityid.ToString(), ActivityInterface.PhoneActivityFieldsToUpdate.description, notes);
                //return true;

                OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
                PhoneCall phone = new PhoneCall();
                phone.PhoneNumber = mobilephone;
                phone.Subject = subject;
                phone.ScheduledStart = duedate.ToUniversalTime();
                phone.DirectionCode = true; //Outgoing
                phone.Description = notes;

                serviceProxy.Create(phone);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

           
        }
*/

        public bool CreateTaskForContract(string contractid, int revisionid, string subject, DateTime duedate, string category, string notes)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            string loginName = dataAccess.GetEstimateSalesConsultantLoginName(revisionid);

            try
            {
                //CRMConnection conn;
                //API.Authentication.ESAUserToken esaToken;
                //esaToken = new ESAUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], loginName);

                //conn = new CRMConnection(ConfigurationManager.AppSettings["CRM_URL"],
                //                                       ConfigurationManager.AppSettings["CRM_DatabaseName"],
                //                                       new CRMUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], ConfigurationManager.AppSettings["CRM_UserToken_Username"], ConfigurationManager.AppSettings["CRM_UserToken_Password"]),
                //                                       esaToken);

                //API.ActivityInterface a = new ActivityInterface();
                //Guid taskid = a.CreateTask(conn, new Guid(contractid), "new_contract", subject, duedate.ToString("s"), notes, category, false);
                //return true;

                OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
                XrmServiceContext context = new XrmServiceContext(serviceProxy);
                /*
                 * since we implement all sales consultants are real system user 
                 * we need change the task owner to system user
                 
                var contact = (from c in context.ContactSet
                               where c.StateCode == 0 &&
                               c.New_SalesAgentDomainName == "METHOMES\\" + loginName &&
                               c.New_type_SalesAgent == true
                               select new { c.ContactId }).FirstOrDefault();

                if (contact != null)
                {
                    Task task = new Task();
                    task.Subject = subject;
                    task.Category = category;
                    task.ScheduledStart = duedate.ToUniversalTime();
                    task.Description = notes;
                    task.RegardingObjectId = new Microsoft.Xrm.Client.CrmEntityReference(New_contract.EntityLogicalName, new Guid(contractid));
                    task.new_externalsalesagentid = new Microsoft.Xrm.Client.CrmEntityReference(Contact.EntityLogicalName, contact.ContactId.Value);
                    serviceProxy.Create(task);
                    return true;
                }
                else
                    return false;
                 * */

                var user = (from c in context.SystemUserSet
                               where 
                               c.DomainName == "METHOMES\\" + loginName
                               select new { c.SystemUserId }).FirstOrDefault();

                if (user != null)
                {
                    Task task = new Task();
                    task.Subject = subject;
                    task.Category = category;
                    task.ScheduledStart = duedate.ToUniversalTime();
                    task.Description = notes;
                    task.RegardingObjectId = new Microsoft.Xrm.Sdk.EntityReference(New_contract.EntityLogicalName, new Guid(contractid));
                    task.OwnerId = new Microsoft.Xrm.Sdk.EntityReference(SystemUser.EntityLogicalName, user.SystemUserId.Value);
                    serviceProxy.Create(task);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }


        }


        public List<SQSSalesRegion> GetSalesRegionByState(string stateid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetSaleRegionByState(stateid);
        }

        public List<SQSSalesRegion> GetPriceRegionByState(string stateid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetPriceRegionByState(stateid);
        }
        public bool CreateSalesEstimateLog(string username, MRSLogAction action, int estimateRevisionId, string extraDescription, int reasonCode)
        {
            Guid contractId = Guid.Empty; 
            int estimateNumber = 0; 
            int revisionNumber = 0; 
            string revisionType = string.Empty;
            string title = string.Empty;

            try
            {
                SqlDataAccess dataAccess = new SqlDataAccess();
                dataAccess.GetEstimateForLogging(estimateRevisionId, out contractId, out revisionType, out revisionNumber, out estimateNumber);

                //CRMConnection conn;
                //API.Authentication.ESAUserToken esaToken;
                //esaToken = new ESAUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], username);

                //conn = new CRMConnection(ConfigurationManager.AppSettings["CRM_URL"],
                //                                       ConfigurationManager.AppSettings["CRM_DatabaseName"],
                //                                       new CRMUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], ConfigurationManager.AppSettings["CRM_UserToken_Username"], ConfigurationManager.AppSettings["CRM_UserToken_Password"]),
                //                                       esaToken);

                //MRSLogInterface log = new MRSLogInterface();
                //log.Action = action.ToString();
                //log.Contract = new KeyValuePair<Guid,string>(contractId, "new_contract");
                //log.User = new KeyValuePair<Guid,string>(conn.ESAUserToken.ESAID, "contact");
                //log.EstimateNumber = estimateNumber.ToString();
                //log.RevisionNumber = revisionNumber.ToString();
                //log.RevisionType = revisionType;
                //log.Title = title;
                
                //if (extraDescription != null && extraDescription != string.Empty)
                //    log.Description = extraDescription;

                //if (reasonCode > 0)
                //    log.ReasonCode = reasonCode.ToString();

                //log.Set(conn);

                OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
                XrmServiceContext context = new XrmServiceContext(serviceProxy);

                //var contact = (from c in context.ContactSet
                //               where c.StateCode == 0 &&
                //               c.New_SalesAgentDomainName == "METHOMES\\" + username.Replace(" ","") &&
                //               c.New_type_SalesAgent == true
                //               select new { c.ContactId }).FirstOrDefault();

                if (username.Trim() != "")
                {
                    New_mrslog log = new New_mrslog();
                    log.New_Action = action.ToString();
                    log.new_contractid = new Microsoft.Xrm.Sdk.EntityReference(New_contract.EntityLogicalName, contractId);
                    //log.new_contactid = new Microsoft.Xrm.Client.CrmEntityReference(Contact.EntityLogicalName, contact.ContactId.Value);
                    log.New_EstimateNumber = estimateNumber.ToString();
                    log.New_RevisionNumber = revisionNumber.ToString();
                    log.New_RevisionType = revisionType;
                    log.New_name = username;

                    if (extraDescription != null && extraDescription != string.Empty)
                        log.New_ExtraDescription = extraDescription;

                    if (reasonCode > 0)
                        log.New_ReasonCode = reasonCode.ToString();

                    serviceProxy.Create(log);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool SetContractStatus(string username, int estimateRevisionId, ContractStatus status)
        {
            try
            {
                SqlDataAccess dataAccess = new SqlDataAccess();
                Guid contractId = dataAccess.GetEstimateContractId(estimateRevisionId);

                //CRMConnection conn;
                //ESAUserToken esaToken;
                //esaToken = new ESAUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], username);

                //conn = new CRMConnection(ConfigurationManager.AppSettings["CRM_URL"],
                //                                        ConfigurationManager.AppSettings["CRM_DatabaseName"],
                //                                        new CRMUserToken(ConfigurationManager.AppSettings["CRM_DomainName"], ConfigurationManager.AppSettings["CRM_UserToken_Username"], ConfigurationManager.AppSettings["CRM_UserToken_Password"]),
                //                                        esaToken);

                //ContractInterface contract = new ContractInterface();

                //int contractStatusReason = 0;
                //string contractStatus = string.Empty;

                //switch (status)
                //{
                //    case ContractStatus.WorkInProgress:
                //        contractStatusReason = 3;
                //        contractStatus = "Active";
                //        break;
                //    case ContractStatus.OnHold:
                //        contractStatusReason = 4;
                //        contractStatus = "Active";
                //        break;
                //    case ContractStatus.Cancelled:
                //        contractStatusReason = 2;
                //        contractStatus = "Inactive";
                //        break;
                //    default:
                //        break;
                //}

                //contract.SetContractStatus(conn, contractId, contractStatusReason, contractStatus);

                OrganizationServiceProxy serviceProxy = CrmHelper.Connect();

                int contractStatusReason = 0;
                int contractStatus = 0;

                switch (status)
                {
                    case ContractStatus.WorkInProgress:
                        contractStatusReason = 3;
                        contractStatus = 0;
                        break;
                    case ContractStatus.OnHold:
                        contractStatusReason = 4;
                        contractStatus = 0;
                        break;
                    case ContractStatus.Cancelled:
                        contractStatusReason = 2;
                        contractStatus = 1;
                        break;
                    default:
                        break;
                }

                SetStateRequest req = new SetStateRequest();
                req.EntityMoniker = new EntityReference(New_contract.EntityLogicalName, contractId);
                req.State = new OptionSetValue(contractStatus);
                req.Status = new OptionSetValue(contractStatusReason);

                serviceProxy.Execute(req);

            }
            catch (Exception ex)
            {
                return false;
            }          

            return true;
        }

        public bool RemoveItemFromNotesTemplate(string templateid, string productareagroupid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.RemoveItemFromNotesTemplate(templateid, productareagroupid, userid);
        }

        public bool UpdateNotesTemplateItem(string templateid, string productareagroupid, decimal quanitity, decimal price, string extradescription, string internaldescription, string additionalinfo, int userid, bool usedefaultquantity)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.UpdateNotesTemplateItem(templateid, productareagroupid, quanitity, price, extradescription, internaldescription,additionalinfo,  userid, usedefaultquantity);
        }
        public List<EstimateDetails> GetAvailableItemsForNotesTemplate(string templateid,string searchtext)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAvailableItemsForNotesTemplate(templateid, searchtext);
        }

        public bool AddItemToNotesTemplate(string templateid, string selecteditemids, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.AddItemsToNotesTemplate(templateid, selecteditemids, userid);
        }

        public bool AddNewNotesTemplate(string templatename, string regionid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.AddNewNotesTemplate(templatename, regionid, userid);
        }

        public bool RemoveNotesTemplate(string templateid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.RemoveNotesTemplate(templateid, userid);
        }

        public bool CopyNotesTemplate(string templatename, string regionid, int userid, string templateid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CopyNotesTemplate(templatename, regionid, userid, templateid);
        }

        public RoleAccessModule GetRoleAccessModule(int roleid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetRoleAccessModule(roleid);
        }

        public string SynchroniseCustomerDetails(string contractnumber)
        {
            string result = string.Empty;
            string url = ConfigurationManager.AppSettings["CRMContractApiUrl"].ToString() + ConfigurationManager.AppSettings["WEBAPISyncCustomerMethodName"].ToString()+ "?ContractNumber=" + contractnumber;
            var request = new RestRequest("", Method.POST);
            RestClient rst = new RestClient(new Uri(url));
            var response = rst.Execute(request);

            //if (dr["Into_Service_Flag"].ToString() == "1" || dr["Into_Service_Flag"].ToString().ToUpper() == "TRUE")// for vic metro, if less than 30 days, update contract but ignore update the contract table unless after 30 days change to service and warranty 
            {
                //returnrow["successful"] = "1";
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    result = response.StatusCode.ToString();
                }
                //returnrow["schedulerid"] = schedulerid;
            }


            //try
            //{
            //    SqlDataAccess dataAccess = new SqlDataAccess();
            //    int customerNumber = 0;
            //    string accountId = string.Empty;
            //    string opportunityId = string.Empty;
            //    dataAccess.GetEstimateCustomerInformation(estimateId, out customerNumber, out accountId, out opportunityId);

            //    int contractNumber = 0;
            //    string contractId = string.Empty;
            //    dataAccess.GetEstimateContractInformation(estimateId, out contractNumber, out contractId);

            //    //get all contact list from BC
            //    //ds = dataAccess.GetContactsOfCustomer(customerNumber.ToString());

            //    DataTable contactsTable;
            //    BcDataAccess bcAccess = new BcDataAccess();
            //    contactsTable = bcAccess.GetContactsOfCustomer(customerNumber.ToString());

            //    //if (ds != null && ds.Tables[0].Rows.Count > 0)
            //    if (contactsTable.Rows.Count > 0)
            //    {
            //        SyncServiceCustomerFromBCToCRM(opportunityId, accountId, customerNumber.ToString(), contractNumber.ToString(), contactsTable);
            //    }

            //    DataTable contractTable = bcAccess.GetContractDetails(contractNumber.ToString());

            //    if (contractTable.Rows.Count > 0)
            //    {
            //        SyncContractFromBCToCRM(contractNumber.ToString(), contractTable);
            //    }

            //}
            //catch (Exception e)
            //{
            //    // throw e;
            //    result = e.Message;
            //}
            return result;
        }


        public void SyncServiceCustomerFromBCToCRM(string opportunityId, string accountid, string customercode, string contractnumber, DataTable contactTable)
        {

            Guid opportunityguidid = new Guid(opportunityId);
            Guid contactid;
            try
            {
                //loop to update CRM
                if (contactTable.Rows.Count > 0)
                {

                    // DeleteAcoContactAccount(opportunityguidid);

                    //check all BC contact to update/create in CRM
                    foreach (DataRow dr in contactTable.Rows)
                    {
                        // check if active contact exists
                        contactid = ContactExistsForOpportunity(opportunityguidid, dr["FirstName"].ToString(), dr["LastName"].ToString(), 1);

                        if (!contactid.Equals(Guid.Empty)) //contact exists, update contact 
                        {
                            UpdateContactForOpportunity(dr, opportunityguidid, contactid);
                        }
                        else // create contact
                        {
                            //check if inactive contact exists
                            contactid = ContactExistsForOpportunity(opportunityguidid, dr["FirstName"].ToString(), dr["LastName"].ToString(), 0);
                            if (!contactid.Equals(Guid.Empty)) // if inactive contact exists
                            {
                                SetContactStatus(contactid, 1); // activate contact
                                UpdateContactForOpportunity(dr, opportunityguidid, contactid);
                            }
                            else
                            {
                                contactid = ContactExists(dr["FirstName"].ToString(), dr["LastName"].ToString(), 1);
                                if (contactid.Equals(Guid.Empty)) // if active contact not exists
                                {
                                    CreateContactForOpportunity(dr, opportunityguidid);
                                }
                                else
                                {
                                    CreateAccountContactOpportunity(contactid, opportunityguidid, dr);
                                }
                            }
                        }
                    }

                    //check all CRM contact which are not in BC contacts, deactivate them
                    DataTable dt = GetActiveCRMContactForOpportunity(opportunityguidid);
                    
                    DataTable processedContacts = new DataTable();
                    processedContacts.Columns.Add("firstname");
                    processedContacts.Columns.Add("lastname");

                    bool exists;
                    bool alreadyProcessed;
                    foreach (DataRow dr in dt.Rows) // for each crm contact
                    {
                        exists = false;
                        foreach (DataRow dr2 in contactTable.Rows)
                        {
                            // loop through BC contact
                            if (dr["firstname"].ToString() == dr2["FirstName"].ToString() &&
                                dr["lastname"].ToString() == dr2["LastName"].ToString()
                                )
                            {
                                alreadyProcessed = false;
                                foreach (DataRow processedContact in processedContacts.Rows)
                                {
                                    if (dr["firstname"].ToString() == processedContact["firstname"].ToString() &&
                                        dr["lastname"].ToString() == processedContact["lastname"].ToString()
                                        )
                                    {
                                        alreadyProcessed = true;
                                        break;
                                    }
                                }

                                if (!alreadyProcessed)
                                {
                                    exists = true;

                                    DataRow contact = processedContacts.NewRow();
                                    contact["firstname"] = dr["firstname"].ToString();
                                    contact["lastname"] = dr["lastname"].ToString();
                                    processedContacts.Rows.Add(contact);

                                    break;
                                }
                            }
                        }

                        if (!exists) // if contact exists, do nothing if not deactivate this contact. that means deactivate contacts only exists in CRM not in BC
                        {
                            SetContactStatus(new Guid(dr["contactid"].ToString()), 0);
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static Guid CreateAccountContactOpportunity(Guid contactid, Guid opportunityId, DataRow dr)
        {
            Guid acoId = new Guid();
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();

            try
            {
                // add new relationship record
                if (contactid != Guid.Empty)
                {
                    Guid accountId = Guid.Empty;
                    if (dr["salutation"].ToString() == "ZZ")
                    {
                        accountId = AccountExistsForOpportunity(opportunityId, dr["firstname"].ToString(), dr["lastname"].ToString());
                        if (accountId == Guid.Empty)
                        {
                            accountId = CreateAccountForOpportunity(dr, opportunityId);
                        }
                    }

                    new_accountcontactopportunity acc = new new_accountcontactopportunity();
                    acc.Id = Guid.NewGuid();
                    acc.new_RelationshipType = new OptionSetValue();
                    acc.new_RelationshipType.Value = 100000000;
                    acc.new_Opportunity = new EntityReference(Xrm.New_contract.EntityLogicalName, opportunityId);
                    acc.new_Contact = new EntityReference(Xrm.Contact.EntityLogicalName, contactid);
                    if (accountId != Guid.Empty)
                    {
                        acc.new_Account = new EntityReference(Xrm.Contact.EntityLogicalName, accountId);
                    }
                    serviceProxy.Create(acc);
                    acoId = acc.Id;
                }
                // update Opportunity primary if this is primary contact
                if (Convert.ToBoolean(dr["Primary"]))
                {
                    Opportunity o = new Opportunity();
                    o.OpportunityId = opportunityId;
                    o.Name = dr["FirstName"].ToString() + " " + dr["LastName"].ToString();
                    o.ParentContactId = new Microsoft.Xrm.Sdk.EntityReference(Xrm.Contact.EntityLogicalName, contactid);
                    serviceProxy.Update(o);
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }

            return acoId;
        }

        public static Guid ContactExists(string firstname, string lastname, int active)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);
            ContactState state;
            if (active == 1)
                state = ContactState.Active;
            else
                state = ContactState.Inactive;
            try
            {
                var contact = (from n in context.ContactSet
                               where n.FirstName == firstname.Trim() &&
                                     n.LastName == lastname.Trim() 
                               select new
                               {
                                   n.ContactId
                               }).ToList();

                if (contact.Count > 0)
                {
                    return (Guid)contact[0].ContactId;
                }
                else
                {
                    return new Guid();
                }
            }
            catch (Exception ex)
            {
                return new Guid();
            }

        }

        public static Guid ContactExistsForOpportunity(Guid opportunityId, string firstname, string lastname, int active)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);
            ContactState state;
            if (active == 1)
                state = ContactState.Active;
            else
                state = ContactState.Inactive;
            try
            {
                var contact = (from n in context.ContactSet
                               join w in context.new_accountcontactopportunitySet on n.ContactId equals w.new_Contact.Id

                               where n.FirstName == firstname.Trim() &&
                                     n.LastName == lastname.Trim() &&
                                     w.new_Opportunity.Id.Equals(opportunityId)
                               select new
                               {
                                   n.ContactId
                               }).ToList();

                if (contact.Count > 0)
                {
                    return (Guid)contact[0].ContactId;
                }
                else
                {
                    return new Guid();
                }
            }
            catch (Exception ex)
            {
                return new Guid();
            }

        }

        public static Guid AccountExistsForOpportunity(Guid opportunityId, string firstname, string lastname)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);

            try
            {
                var account = (from n in context.AccountSet
                               join w in context.new_accountcontactopportunitySet on n.AccountId equals w.new_Account.Id

                               where n.Name == firstname.Trim() + " " + lastname.Trim() &&
                                     w.new_Opportunity.Id.Equals(opportunityId)
                               select new
                               {
                                   n.AccountId
                               }).ToList();

                if (account.Count > 0)
                {
                    return (Guid)account[0].AccountId;
                }
                else
                {
                    return new Guid();
                }
            }
            catch (Exception ex)
            {
                return new Guid();
            }

        }

        public static void DeleteAcoContactAccount(Guid opportunityId)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);

            var acoQuery = (from con in context.new_accountcontactopportunitySet
                            where con.new_Opportunity.Equals(opportunityId)
                            select new
                            {
                                con.new_accountcontactopportunityId,
                            });

            foreach (var aco in acoQuery)
            {
                new_accountcontactopportunity new_aco = new new_accountcontactopportunity();
                new_aco.new_accountcontactopportunityId = aco.new_accountcontactopportunityId;
                serviceProxy.Delete(Xrm.new_accountcontactopportunity.EntityLogicalName, aco.new_accountcontactopportunityId ?? Guid.NewGuid());
            }
        }

        public static void CreateContactForOpportunity(DataRow dr, Guid opportunityId)
        {
            //for this contact creation, no need to check duplication.

            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            Contact c = new Contact();

            c.Salutation = dr["salutation"].ToString();
            c.FirstName = dr["firstname"].ToString();
            c.LastName = dr["lastname"].ToString();
            c.Address1_Line1 = dr["Address"].ToString();
            c.Address1_PostalCode = dr["Postcode"].ToString();
            c.Address1_City = dr["Suburb"].ToString();
            c.Address1_StateOrProvince = dr["State"].ToString();
            c.Telephone1 = dr["Phone"].ToString();
            c.MobilePhone = dr["Mobile"].ToString();

            if (dr["EmailAddress"].ToString().IndexOf(";") > 0)
            {
                c.EMailAddress1 = dr["EmailAddress"].ToString().Substring(0, dr["EmailAddress"].ToString().IndexOf(";"));
            }
            else if (!string.IsNullOrEmpty(dr["EmailAddress"].ToString()))
            {
                c.EMailAddress1 = dr["EmailAddress"].ToString();
            }

            //c.ParentCustomerId = new EntityReference(Xrm.Account.EntityLogicalName, accountid);
            Guid cid = serviceProxy.Create(c);
            
            // add new relationship record
            if (cid != Guid.Empty)
            {
                Guid accountId = Guid.Empty;
                if (c.Salutation == "ZZ")
                {
                    accountId = AccountExistsForOpportunity(opportunityId, dr["firstname"].ToString(), dr["lastname"].ToString());
                    if (accountId == Guid.Empty)
                    {
                        accountId = CreateAccountForOpportunity(dr, opportunityId);
                    }
                }
                new_accountcontactopportunity acc = new new_accountcontactopportunity();
                acc.Id = Guid.NewGuid();
                acc.new_Opportunity = new EntityReference(Xrm.New_contract.EntityLogicalName, opportunityId);
                acc.new_Contact = new EntityReference(Xrm.Contact.EntityLogicalName, cid);
                if (accountId != Guid.Empty)
                {
                    acc.new_Account = new EntityReference(Xrm.Contact.EntityLogicalName, accountId);
                }
                serviceProxy.Create(acc);
            }

            //update Opportunity primary if this is primary contact
            if (Convert.ToBoolean(dr["Primary"]))
            {
                Opportunity o = new Opportunity();
                o.Name = dr["FirstName"].ToString() + " " + dr["LastName"].ToString();
                o.OpportunityId = opportunityId;
                o.ParentContactId = new Microsoft.Xrm.Sdk.EntityReference(Xrm.Contact.EntityLogicalName, cid);
                serviceProxy.Update(o);
            }

        }

        public static Guid CreateAccountForOpportunity(DataRow dr, Guid opportunityId)
        {
            //for this contact creation, no need to check duplication.

            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            Account a = new Account();
            
            a.Name = dr["firstname"].ToString() + " " + dr["lastname"].ToString();

            //c.ParentCustomerId = new EntityReference(Xrm.Account.EntityLogicalName, accountid);
            Guid aid = serviceProxy.Create(a);

            return aid;
        }

        public void SyncContractFromBCToCRM(string contractNumber, DataTable contractTable)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);

            var contractQuery = (from con in context.New_contractSet
                            where con.New_name.Equals(contractNumber)
                            select new
                            {
                                con.New_contractId
                            });

            foreach (var contract in contractQuery)
            {
                if (contract != null && contract.New_contractId.HasValue)
                {
                    DataRow contractRow = contractTable.Rows[0];

                    New_contract c = new New_contract();
                    c.New_contractId = contract.New_contractId.Value;
                    c.New_CustomerCode = contractRow["CustomerNumber"].ToString();
                    c.New_HouseCode = contractRow["HouseCode"].ToString();
                    c.New_house = contractRow["HouseName"].ToString();
                    c.New_contractstatus = contractRow["ContractStatus"].ToString();
                    c.New_LotNumber = contractRow["LotNumber"].ToString();
                    c.New_address_StreetNumber = contractRow["StreetNumber"].ToString();
                    c.New_address_Street = contractRow["StreetName"].ToString();
                    c.New_address_Suburb = contractRow["City"].ToString();
                    c.New_State = contractRow["State"].ToString();
                    c.New_address_PostCode = contractRow["Postcode"].ToString();

                    serviceProxy.Update(c);

                }
            }
        }

        public DataTable GetActiveCRMContactForOpportunity(Guid opportunityId)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);

            DataTable tempTable = new DataTable();
            tempTable.TableName = "contacttable";
            tempTable.Columns.Add("contactid");
            tempTable.Columns.Add("firstname");
            tempTable.Columns.Add("lastname");
            tempTable.Columns.Add("customerstreet");
            tempTable.Columns.Add("customerpostcode");
            tempTable.Columns.Add("customersuburb");
            tempTable.Columns.Add("customerstate");
            tempTable.Columns.Add("phone");
            tempTable.Columns.Add("mobile");
            tempTable.Columns.Add("email");

            var queryResults = (from n in context.ContactSet
                            join w in context.new_accountcontactopportunitySet on n.ContactId equals w.new_Contact.Id
                            where w.new_Opportunity.Id.Equals(opportunityId)
                            orderby n.CreatedOn ascending
                            select new
                            {
                                n.ContactId,
                                n.FirstName,
                                n.LastName,
                                n.Address1_Line1,
                                n.Address1_PostalCode,
                                n.Address1_City,
                                n.Address1_StateOrProvince,
                                n.Address1_Telephone1,
                                n.MobilePhone,
                                n.EMailAddress1
                            }).ToList();            
                                
            foreach (var c in queryResults)
            {
                DataRow dr = tempTable.NewRow();
                dr["contactid"] = c.ContactId.ToString();
                dr["firstname"] = c.FirstName;
                dr["lastname"] = c.LastName;
                dr["customerstreet"] = c.Address1_Line1;
                dr["customerpostcode"] = c.Address1_PostalCode;
                dr["customersuburb"] = c.Address1_City;
                dr["customerstate"] = c.Address1_StateOrProvince;
                dr["phone"] = c.Address1_Telephone1;
                dr["mobile"] = c.MobilePhone;
                dr["email"] = c.EMailAddress1;

                tempTable.Rows.Add(dr);
            }

            return tempTable;
        }
        //public Guid ContactExistsInOpportunity(string opportunityId, string firstname, string lastname, int active)
        //{
        //    OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
        //    XrmServiceContext context = new XrmServiceContext(serviceProxy);
        //    //int state;
        //    ContactState state = new ContactState();
        //    if (active == 1)
        //        state = ContactState.Active;
        //    else
        //        state = ContactState.Inactive;
        //    try
        //    {
        //        string firstName = null;
        //        string lastName = null;

        //        if (firstname.Trim() != string.Empty)
        //            firstName = firstname.Trim();

        //        if (lastname.Trim() != string.Empty)
        //            lastName = lastname.Trim();

        //        var contact = (from n in context.ContactSet
        //                       where n.FirstName == firstName &&
        //                             n.LastName == lastName &&
        //                             n.ParentCustomerId.Id.Equals(new Guid(opportunityId)) &&
        //                             n.StateCode.Value == state
        //                       select new
        //                       {
        //                           n.ContactId,
        //                           n.FirstName,
        //                           n.LastName
        //                       }).ToList();

        //        if (contact.Count > 0)
        //        {
        //            if (contact[0].FirstName == firstname && contact[0].LastName == lastname)
        //                return (Guid)contact[0].ContactId;
        //            else
        //                return new Guid();
        //        }
        //        else
        //        {
        //            return new Guid();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return new Guid();
        //    }

        //}

        private int GetSalutationIDFromSalutation(string salutation)
        {
            int saluationid=0;
            string titlestring = ConfigurationManager.AppSettings["CRM_TITLE_STRING"].ToString();
            string[] temp = titlestring.Split('|');
            try
            {
                if (temp.Length > 0)
                {
                    for (int i = 0; i < temp.Length; i++)
                    {
                        string[] temp2 = temp[i].Split(',');
                        if (salutation.ToUpper().Trim() == temp2[0].ToUpper().Trim())
                        {
                            saluationid = int.Parse(temp2[1]);
                            break;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                saluationid = 0;
            }
            return saluationid;
        }

        public void UpdateContactForOpportunity(DataRow dr, Guid opportunityId, Guid contactid)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);


            Contact c = new Contact();
            c.ContactId = contactid;
            c.Salutation = dr["Salutation"].ToString();
            int salutationCode = GetSalutationIDFromSalutation(dr["Salutation"].ToString());
            c.new_Salutation = new OptionSetValue();
            if (salutationCode > 0)
            {                
                c.new_Salutation.Value = salutationCode;
            }
            else
            {
                c.new_Salutation.Value = new int();
                c.new_Salutation = null;
            }
            c.Address1_Line1 = dr["Address"].ToString();
            c.Address1_PostalCode = dr["Postcode"].ToString();
            c.Address1_City = dr["Suburb"].ToString();
            c.Address1_StateOrProvince = dr["State"].ToString();
            c.Telephone1 = dr["Phone"].ToString();
            c.MobilePhone = dr["Mobile"].ToString();
            c.EMailAddress1 = dr["EmailAddress"].ToString();
            if (Convert.ToBoolean(dr["Primary"]))
            {
                c.Address1_AddressTypeCode = new OptionSetValue();
                c.Address1_AddressTypeCode.Value = 3; // set to primary
            }
            else
            {
                c.Address1_AddressTypeCode = new OptionSetValue();
                c.Address1_AddressTypeCode.Value = 4; // set to other
            }
            serviceProxy.Update(c);

            //update Opportunity primary if this is primary contact
            if (Convert.ToBoolean(dr["Primary"]))
            {
                Opportunity o = new Opportunity();
                o.OpportunityId = opportunityId;
                o.Name = dr["FirstName"].ToString() + " " + dr["LastName"].ToString();
                o.ParentContactId = new Microsoft.Xrm.Sdk.EntityReference(Xrm.Contact.EntityLogicalName, contactid);
                serviceProxy.Update(o);
            }

        }

        public void SetContactStatus(Guid contactid, int active)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            OptionSetValue stateCode = new OptionSetValue();
            OptionSetValue statusCode = new OptionSetValue();
            if (active == 1) // activate contact
            {
                stateCode.Value = 0;
                statusCode.Value = 1;
            }
            else // deactivate
            {
                stateCode.Value = 1;
                statusCode.Value = 2;
            }

            SetStateRequest req = new SetStateRequest();
            req.EntityMoniker = new EntityReference() { LogicalName = "contact", Id = contactid };
            req.State = stateCode;
            req.Status = statusCode;
            SetStateResponse resp = (SetStateResponse)serviceProxy.Execute(req);

        }

        //public void CreateContactForOpportunity(DataRow dr, Guid opportunityId)
        //{
        //    //for this contact creation, no need to check duplication.

        //    OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
        //    Contact c = new Contact();

        //    c.Salutation = dr["Salutation"].ToString();
        //    int salutationCode = GetSalutationIDFromSalutation(dr["Salutation"].ToString());
        //    c.new_Salutation = new OptionSetValue();
        //    if (salutationCode > 0)
        //        c.new_Salutation.Value = salutationCode;
        //    else
        //    {
        //        c.new_Salutation.Value = new int();
        //        c.new_Salutation = null;
        //    }
        //    c.FirstName = dr["FirstName"].ToString();
        //    c.LastName = dr["LastName"].ToString();
        //    c.Address1_Line1 = dr["Address"].ToString();
        //    c.Address1_PostalCode = dr["Postcode"].ToString();
        //    c.Address1_City = dr["Suburb"].ToString();
        //    c.Address1_StateOrProvince = dr["State"].ToString();
        //    c.Telephone1 = dr["Phone"].ToString();
        //    c.MobilePhone = dr["Mobile"].ToString();
        //    c.EMailAddress1 = dr["EmailAddress"].ToString();
        //    c.Address1_AddressTypeCode = new OptionSetValue();
        //    if (Convert.ToBoolean(dr["Primary"]))
        //    {
        //        c.Address1_AddressTypeCode.Value = 3; // set to primary
        //    }
        //    else
        //    {
        //        c.Address1_AddressTypeCode.Value = 4; // set to other
        //    }

        //    c.ParentCustomerId = new Microsoft.Xrm.Sdk.EntityReference(Xrm.Account.EntityLogicalName, accountid);
        //    Guid cid = serviceProxy.Create(c);

        //    //update account primary if this is primary contact
        //    if (Convert.ToBoolean(dr["Primary"]))
        //    {
        //        Opportunity a = new Opportunity();
        //        a.OpportunityId = opportunityId;
        //        a.PrimaryContactId = new Microsoft.Xrm.Sdk.EntityReference(Xrm.Contact.EntityLogicalName, cid);
        //        serviceProxy.Update(a);
        //    }

        //}

        public List<NonStandardCategory> GetNonstandardCategory()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetNonstandardCategory();
        }

        public List<NonStandardCategory> GetNonstandardCategoryByState(int stateid, int selectedareaid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetNonstandardCategoryByState(stateid, selectedareaid);
        }

        public List<NonStandardGroup> GetNonstandardGroups(int selectedareaid, int stateid, int selectedgroupid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetNonstandardGroups(selectedareaid, stateid, selectedgroupid);
        }

        public List<PriceDisplayCode> GetPriceDisplayCodes()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetPriceDisplayCodes();
        }

        public List<ProductImage> GetProductImages(string productid, int supplierid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetProductImages(productid, supplierid);
        }

        public string CheckEstimateLockStatus(int estimaterevisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CheckEstimateLockStatus(estimaterevisionid);
        }

        public void UnlockEstimate(int estimaterevisionid, int type)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.UnlockEstimate(estimaterevisionid, type);
        }

        public List<ValidationErrorMessage> ValidateStudioMEstimate(int estimaterevisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ValidateStudioMEstimate(estimaterevisionid);
        }

        public bool UpdateItemAcceptance(string revisionestimatedetailsid, int accepted, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.UpdateItemAcceptance(revisionestimatedetailsid, accepted, userid);
        }

        public ContractDraftActionAvailability GetContractDraftActionAvailability(int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetContractDraftActions(estimateRevisionId);
        }

        public bool GetContractDraftCreationVisibility(int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetContractDraftCreationVisibility(estimateRevisionId);
        }

        public bool GetFinalContractCreationVisibility(int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetFinalContractCreationVisibility(estimateRevisionId);
        }

        public FinalContractActionAvailability GetFinalContractActionAvailability(int estimateRevisionId, string contractNumber)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            FinalContractActionAvailability availability = dataAccess.GetFinalContractActions(estimateRevisionId);

            BcDataAccess bcAccess = new BcDataAccess();
            if (bcAccess.IsConstructionCommenced(contractNumber))
                availability.PreSiteVariationAvailable = false;
            else
                availability.BuildingVariationAvailable = false;

            return availability;
        }

        public CustomerSupportActionAvailability GetCustomerSupportActionAvailability(int estimateRevisionId, string contractNumber)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            CustomerSupportActionAvailability availability = dataAccess.GetCustomerSupportActions(estimateRevisionId);

            if (availability.BuildingVariationAvailable || availability.PreSiteVariationAvailable)
            {
                BcDataAccess bcAccess = new BcDataAccess();
                if (bcAccess.IsConstructionCommenced(contractNumber))
                    availability.PreSiteVariationAvailable = false;
                else
                    availability.BuildingVariationAvailable = false;
            }

            return availability;
        }

        public SalesEstimatorActionAvailability GetSalesEstimatorActionAvailability(int estimateRevisionId, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetSalesEstimatorActions(estimateRevisionId, userid);
        }

        public void CreateSplitStudioMRevisions(int estimateRevisionId, string revisionTypeIds, string assignedToUserIds, int createdbyId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.CreateSplitStudioMRevisions(estimateRevisionId, revisionTypeIds, assignedToUserIds, createdbyId);
        }

        public void MergeStudioMRevisions(int estimateRevisionId, int createdbyId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.MergeStudioMRevisions(estimateRevisionId, createdbyId);
        }

        public void CreateContractDraft(int estimateRevisionId, int createdbyId/*, DateTime appointment*/)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.CreateContractDraft(estimateRevisionId, createdbyId/*, appointment*/);
        }

        public void CreateFinalContract(int estimateRevisionId, int createdbyId/*, DateTime appointment*/)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.CreateFinalContract(estimateRevisionId, createdbyId/*, appointment*/);
        }

        public void CreateCscVariation(int estimateRevisionId, int createdbyId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.CreateCscVariation(estimateRevisionId, createdbyId);
        }

        public string CreateStudioMRevision(int estimateRevisionId, int ownerId, DateTime appointmentDateTime, int revisionTypeId, int createdbyId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CreateStudioMRevision(estimateRevisionId, ownerId, appointmentDateTime, revisionTypeId, createdbyId);
        }

        public string ValidateSetEstimateStatus(int estimateRevisionId, int nextRevisionTypeId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ValidateSetEstimateStatus(estimateRevisionId, nextRevisionTypeId);
        }

        public void CreateVariation(int estimateRevisionId, int revisionTypeId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.CreateVariation(estimateRevisionId, revisionTypeId, userId);
        }

        public void RejectVariation(int estimateRevisionId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.RejectVariation(estimateRevisionId, userId);
        }

        public string GetCustomerDocumentType(int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetCustomerDocumentType(estimateRevisionId);
        }

        public int UpdateCustomerDocumentDetails(CustomerDocumentDetails document)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.UpdateCustomerDocumentDetails(document);
        }

        public CustomerDocumentDetails GetCustomerDocumentDetails(int estimateRevisionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetCustomerDocumentDetails(estimateRevisionId);
        }

        //Dummy function to allow client access to EstimateRevisionStatus
        public void GetEstimateRevisionStatus(EstimateRevisionStatus status)
        {

        }

        public List<SimplePAG> GetRelevantPAGFromOnePAG(string estimatedetailsid, /*string standardinclusionid,*/ string revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetRelevantPAGFromOnePAG(estimatedetailsid, /*standardinclusionid,*/ revisionid);
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
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.SaveSelectedItemsFromOptionTreeToEstimate(optionidstring, 
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
                internaldescriptionstring);
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
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ReplaceSaveSelectedItemsFromOptionTreeToEstimate(sourceEstimateRevisionDetailsId, optionidstring,
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
                );
        }
        public RevisionTypePermission CheckRevisionTypeAllowToAddNSR(int revisontypeid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CheckRevisionTypeAllowToAddNSR(revisontypeid);
        }

        public List<ValidationErrorMessage> ValidateAcceptFlagForRevision(int estimaterevisionid, int userroleid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ValidateAcceptFlagForRevision(estimaterevisionid, userroleid);
        }

        public List<ValidationErrorMessage> ValidateStudioMRevisions(int estimaterevisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ValidateStudioMRevisions(estimaterevisionid);
        }

        public bool ValidateAppointmentDate(int estimaterevisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ValidateAppointmentDate(estimaterevisionid);
        }

        public List<NextRevision> GetNextEstimateRevision(int estimateRevisionId, int statusId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetNextEstimateRevision(estimateRevisionId, statusId);
        }

        public List<AuditLog> GetAuditLogs(int revisionid, int estimatedetailid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAuditLogs(revisionid, estimatedetailid);
        }

        public List<DeletedItem> GetDeletedItems(int revisionid, RESULT_TYPE resulttype)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetDeletedItems(revisionid, (int)resulttype);
        }

        public bool ReAddDeletedEstimateItem(int sourceEstimateRevisionId, int targetEstimateRevisionId, int optionId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ReAddDeletedEstimateItem(sourceEstimateRevisionId, targetEstimateRevisionId, optionId, userId);
        }

        public bool ReAddDeletedMasterPromotionEstimateItem(int estimateRevisionId, int optionId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ReAddDeletedMasterPromotionEstimateItem(estimateRevisionId, optionId, userId);
        }

        public List<SQSHome> GetAllFacadeFromRevisonID(int revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAllFacadeFromRevisonID(revisionid);
        }
        public List<SQSHome> GetAllAvailableHomeByState(int stateid, string searchText, bool showdisplayhomes)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAllAvailableHomesByState(stateid, searchText, showdisplayhomes);
        }
        public List<SQSHome> GetHomeFullNameByState(int stateid, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetHomeFullNameByState(stateid, userId);
        }
        public List<SQSArea> GetAreaNameWithAll()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAreaNameWithAll();
        }
        public List<SQSGroup> GetGroupNameWithAll()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetGroupNameWithAll();
        }
        public List<ValidationErrorMessage> CheckFacadeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CheckFacadeConfigurationDifference(revisionid, newfacadehomeid, effectivedate);
        }
        public List<ValidationErrorMessage> CheckHomeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CheckHomeConfigurationDifference(revisionid, newfacadehomeid, effectivedate);
        }

        public bool ChangeFacade(int revisionid, int newfacadehomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            Trace.TraceInformation("MRSWCF-ChangeFacade - Information - revisionid={0},newfacadehomeid={1},detailIDsSelected={2},detailOptionsSeleced={3},detailPricesSeleced={4},effectivedate={5},userid={6}", revisionid, newfacadehomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);

            return dataAccess.ChangeFacade(revisionid, newfacadehomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);
        }

        public bool ChangeHome(int revisionid, int newhomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            Trace.TraceInformation("MRSWCF-ChangeHome - Information - revisionid={0},newhomeid={1},detailIDsSelected={2},detailOptionsSeleced={3},detailPricesSeleced={4},effectivedate={5},userid={6}", revisionid, newhomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);

            return dataAccess.ChangeHome(revisionid, newhomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, effectivedate, userid);
        }

        public bool UpdateNoteTemplate(int templateid, string templatename, int status, int userid, string action)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.UpdateNotesTemplateStatus(templateid.ToString(), userid, status, templatename, action);
        }

        public string CheckNewNoteTemplateNameExists(int templateid, string templatename)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CheckNewNotesTemplateNameExists(templateid, templatename);
        }
        public List<ContractType> GetContractType(string configCode)
        {
            List<ContractType> listContractType = new List<ContractType>();
            SqlDataAccess dataAccess = new SqlDataAccess();
            List<SQSConfiguration> listSQSConfig = dataAccess.GetSQSConfiguration(configCode);

            foreach (SQSConfiguration config in listSQSConfig)
            {
                ContractType t = new ContractType();
                t.ContractTypeID = config.CodeValue;
                t.ContractTypeName = config.CodeText;
                listContractType.Add(t);
            }
            if (listContractType.Count < 1)
            { 
                string idstring = ConfigurationManager.AppSettings["ContractTypeBriefString"].ToString();
                string namestring = ConfigurationManager.AppSettings["ContractTypeNameString"].ToString();
                string[] temp1 = idstring.Split(',');
                string[] temp2 = namestring.Split(',');
                if (temp1.Length > 0)
                {
                    for (int i = 0; i < temp1.Length; i++)
                    {
                        ContractType t = new ContractType();
                        t.ContractTypeID = temp1[i];
                        t.ContractTypeName = temp2[i];

                        listContractType.Add(t) ;
                    }

                }
            }

            return listContractType;
        }
        public List<SQSConfiguration> GetSQSConfiguration(string configCode, string codeValue)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            return dataAccess.GetSQSConfiguration(configCode, codeValue);
        }

        public bool UpdateContractType(int revisionid, string contracttype, string jobflowtype, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.UpdateContractType(revisionid, contracttype, jobflowtype, userid);
        }

        public string GetHomeName(int revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetHomeName(revisionid);
        }

        public bool UpdateHomeName(int revisionid, string homename, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.UpdateHomeName(revisionid, homename, userid);
        }

        public List<ValidationErrorMessage> CopyEstimateCheckDifference(string sourceEstimatenumber, string destinationEstimatenumber)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CopyEstimateCheckDifference(sourceEstimatenumber, destinationEstimatenumber);
        }

        public bool CopyEstimate(string sourceEstimatenumber, string destinationEstimatenumber)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.CopyEstimate(sourceEstimatenumber, destinationEstimatenumber);
        }

        public List<decimal> GetAreaSurcharge(int revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetAreaSurcharge(revisionid);
            //return true;
        }

        public EstimateDetails GetPagByID(int estimaterevisionid, int optionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetPagByID(estimaterevisionid, optionid);
        }
        public List<SimplePAG> GetUpgradeOptionListForStandardInclusion(int estimaterevisionid, int originateoptionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetUpgradeOptionListForStandardInclusion(estimaterevisionid, originateoptionid);
        }

        public string GetStudioMQandA(int optionId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetStudioMQandA(optionId);
        }

        public string GetStudioMQuestionForAProduct(string pproductid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.LoadStudioMQuestionForAProduct(pproductid);
        }

        public List<PAG> GetInLieuStandardPromotionItems(int estimaterevisionid, int originateoptionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetInLieuStandardPromotionItems(estimaterevisionid, originateoptionid);
        }

        public List<SharepointDoc> Sharepoint_GetFileList(string opportunityid, string contractnumber)
        {
            List<SharepointDoc> lsp = new List<SharepointDoc>();
            MetriconSalesWebService.MetriconSales ms = new MetriconSalesWebService.MetriconSales();
            DataTable dt = ms.Sharepoint_GetSharepointFileListInFolder(opportunityid, contractnumber);
            foreach (DataRow dr in dt.Rows)
            {
                SharepointDoc sd = new SharepointDoc();
                sd.FileID = dr["fileid"].ToString();
                sd.DocumentName = dr["filename"].ToString();
                sd.DocumentCategory = dr["DocCategory"].ToString();
                sd.DocumentType = dr["DocType"].ToString();
                sd.DocumentGroup = dr["DocGroup"].ToString();
                sd.FileURI = ConfigurationManager.AppSettings["EstimateDocumentServerName"] + ConfigurationManager.AppSettings["EstimateDocumentLibraryName"] + "/" + contractnumber + "/" + dr["filename"].ToString();
                lsp.Add(sd);
            }

            return lsp;
        }

        public bool Sharepoint_SharepointUploadFile(string filename,byte[] contents, string oldname, string opportunityid, string doccategory, string doctype)
        {
            MetriconSalesWebService.MetriconSales ms = new MetriconSalesWebService.MetriconSales();
            ms.Sharepoint_UploadAttachement(filename,contents, oldname,opportunityid,"0",doccategory, doctype);
            return true;
        }

        public bool Sharepoint_DeleteFileFromSharepointLibrary(SharepointDoc doc, string opportunityid, string contractnumber)
        {
            MetriconSalesWebService.MetriconSales ms = new MetriconSalesWebService.MetriconSales();
            string[] temp=doc.FileURI.Split('/');
            string filename = temp[temp.Length - 1].ToString();
            ms.Sharepoint_RemoveFileFromList(opportunityid, contractnumber, doc.FileID,filename);
            return true;
        }

        public List<SharepointDocumentType> Sharepoint_GetSalesDocumentType()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetSalesDocumentType();
        }

        public List<ItemRemoveReason> GetItemRemoveReason()
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetItemRemoveReason();
        }
        public List<StudioMItem> GetItemsNeedSetDefaultAnswer(string revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetItemsNeedSetDefaultAnswer(revisionid);
        }

        public bool SetDefaultAnswerForEstimateRevision(string idstring, string studiomstring, string usercode)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.SetDefaultAnswerForEstimateRevision(idstring, studiomstring, usercode);
        }


        public bool SendNotificationEmail(string templateobjectid, string actionuserfullname, string recipientfullname, string contractnumber,  string templateobjecttype, string emailtype, string estimatenumber, string revisionnumber, string comments)
        {
            OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            XrmServiceContext context = new XrmServiceContext(serviceProxy);

            try
            {
                SqlDataAccess dataAccess = new SqlDataAccess();

                // get recipient contact guid from full name

                DataSet ds= dataAccess.GetContactGuidFromFullName(recipientfullname,"salesconsultant");
                Guid salesconsultantid = new Guid(ds.Tables[0].Rows[0]["contactid"].ToString());
    
                Guid templateGuid;
                if (emailtype.ToUpper() == "REJECTED")
                {
                    templateGuid = new Guid(ConfigurationManager.AppSettings["MRS_REJECT_EMAIL_TEMPLATE_ID"].ToString());
                }
                else
                {
                    templateGuid = new Guid(ConfigurationManager.AppSettings["MRS_ACCEPT_EMAIL_TEMPLATE_ID"].ToString());
                }
          
                InstantiateTemplateRequest tempReq = new InstantiateTemplateRequest();
                tempReq.TemplateId = templateGuid;

                tempReq.ObjectId = new Guid(templateobjectid);
                tempReq.ObjectType = templateobjecttype; // 

                InstantiateTemplateResponse tempRes = (InstantiateTemplateResponse)serviceProxy.Execute(tempReq);
                Email email = (Email)tempRes.EntityCollection[0];

                ActivityParty to = new ActivityParty();
                to.PartyId = new EntityReference("contact", salesconsultantid);
                ActivityParty from = new ActivityParty();
                from.PartyId = new EntityReference("queue", new Guid(ConfigurationManager.AppSettings["CRM_OPPORTUNITY_EMAIL_QUEUE_ID"].ToString()));
                email.To = new ActivityParty[] { to };
                email.From = new ActivityParty[] { from };
                email.RegardingObjectId = new EntityReference(templateobjecttype, new Guid(templateobjectid));

                if (emailtype.ToUpper() == "REJECTED")
                {
                    email.Description = email.Description.Replace("$RejectingUserName$", actionuserfullname);
                }
                else // accepted
                {
                    email.Description = email.Description.Replace("$AcceptingUserName$", actionuserfullname);
                }
                email.Subject.Replace("$EstimateNo$", estimatenumber);
                email.Subject.Replace("$RevisionNo$", revisionnumber);
                email.Description = email.Description.Replace("$SalesConsultantName$", recipientfullname);
                email.Description = email.Description.Replace("$EstimateNo$", estimatenumber);
                email.Description = email.Description.Replace("$RevisionNo$", revisionnumber);
                email.Description = email.Description.Replace("$comments$", comments);

                Guid emailGuid = serviceProxy.Create(email);

                if(emailtype.ToUpper() == "ACCEPTED")
                {
                    //ActivityMimeAttachment attachment = new ActivityMimeAttachment
                    //{
                    //    ObjectId = new Microsoft.Xrm.Client.CrmEntityReference(Email.EntityLogicalName, emailGuid),
                    //    ObjectTypeCode = Email.EntityLogicalName,
                    //    //Subject = filename,
                    //    Body = System.Convert.ToBase64String(contents),
                    //    FileName = filename
                    //};


                    //Guid attachmentGuid = serviceProxy.Create(attachment);
                }

                SendEmailRequest req = new SendEmailRequest();
                req.EmailId = emailGuid;
                req.TrackingToken = string.Empty;
                req.IssueSend = true;
                SendEmailResponse res = (SendEmailResponse)serviceProxy.Execute(req);

                return true;
            }

            catch (Exception)
            {
                return false;
            }

        }

        public bool SendCrmEmail(Guid contractId, int recipientId, string subject, string body)
        {
            // this fucntion was commented on 27/01/2015 as we decide use SQL to send notification email.
            return true;
            //try
            //{
            //    SqlDataAccess dataAccess = new SqlDataAccess();
            //    User recipient = dataAccess.GetUserById(recipientId);

            //    OrganizationServiceProxy serviceProxy = CrmHelper.Connect();
            //    XrmServiceContext context = new XrmServiceContext(serviceProxy);

            //    ActivityParty toParty = new ActivityParty();
            //    toParty.AddressUsed = recipient.EmailAddress; // Raw email address string
            //    toParty.ParticipationTypeMask = 2; // 2 for TO, 3 for CC 

            //    ActivityParty fromParty = new ActivityParty();
            //    fromParty.PartyId = new EntityReference("queue", new Guid(ConfigurationManager.AppSettings["MrsNotificationQueueId"].ToString()));

            //    Email email = new Email();
            //    email.RegardingObjectId = new Microsoft.Xrm.Client.CrmEntityReference(New_contract.EntityLogicalName, contractId);
            //    email.To = new ActivityParty[] { toParty };
            //    email.From = new ActivityParty[] { fromParty };
            //    email.Subject = subject;
            //    email.Description = body;

            //    Guid emailId = serviceProxy.Create(email);

            //    SendEmailRequest req = new SendEmailRequest();
            //    req.EmailId = emailId;
            //    req.TrackingToken = string.Empty;
            //    req.IssueSend = true;
            //    SendEmailResponse res = (SendEmailResponse)serviceProxy.Execute(req);

            //    return true;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}
        }

        public void RegisterEvent(string action, int revisionid, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            dataAccess.RegisterEvent(action, revisionid, userid);
        }

        public DateTime GetStudioMAppointmentTime(string contractNumber, string eventNumber)
        {
            BcDataAccess bcAccess = new BcDataAccess();
            return bcAccess.GetStudioMAppointmentTime(contractNumber, eventNumber);
        }

        public void UpdateEstimateDetailsDescription(List<EstimateDetails> items, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            foreach (EstimateDetails item in items)
            {
                dataAccess.UpdateEstimateDetailsDescription(item.EstimateRevisionDetailsId,
                                            item.ProductDescription,
                                            item.AdditionalNotes,
                                            item.ExtraDescription,
                                            userId);
            }
        }

        public bool MoveEstimateDetailItem(int revisionDetailsIdSource, int revisionDetailsIdTarget, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            return dataAccess.MoveEstimateDetailItem(revisionDetailsIdSource,
                                        revisionDetailsIdTarget,
                                        userId);
        }

        public List<DocuSignDocStatusInfo> DocuSign_GetDocumentInfo(string revisionid, string estimateid)
        {
            DocuSignWebService.DocuSignWebService dsv= new DocuSignWebService.DocuSignWebService();
            List<DocuSignDocStatusInfo> lv = new List<DocuSignDocStatusInfo>();
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetDocuSignEnvelopStatus(revisionid, estimateid);
            //DateTime fromdate = dataAccess.GetDocuSignSendDate(revisionid,estimateid);

            //DocuSignWebService.EnvelopeStatusInfo[] ev= dsv.DocuSign_GetEnvelopeStatus(fromdate, estimateid);
            //for (int i = 0; i < ev.Length; i++)
            //{
            //    DocuSignDocStatusInfo ev1 = new DocuSignDocStatusInfo();
            //    ev1.deleted = ev[i].deleted;
            //    ev1.documenttype=ev[i].documenttype;
            //    ev1.printtype = ev[i].printtype;
            //    ev1.envelopeId = ev[i].envelopeId;
            //    ev1.estimateid = ev[i].estimateid;
            //    ev1.recipients = ev[i].recipients;
            //    ev1.revisionnumber = ev[i].revisionnumber;
            //    ev1.status = ev[i].status;
            //    ev1.statusChangedDateTime = ev[i].statusChangedDateTime;
                
            //    lv.Add(ev1);
            //}

            //return lv;
        }

        public bool DocuSign_PushDocumentToTheProcessQueue(string revisionid, string printtype, string documenttype, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.DocuSign_PushDocumentToTheProcessQueue(revisionid, printtype, documenttype, userid);
        }

        public List<EstimateDetails> GetPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetPromotionProductByMasterPromotionRevisionDetailsID(revisiondetailsid);
        }

        public List<PromotionPAG> GetExistingPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetExistingPromotionProductByMasterPromotionRevisionDetailsID(revisiondetailsid);
        }

        public List<EstimateDetails> GetEstimateDetailsByIDString(string selectedrevisiondetailsid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetEstimateDetailsByIDString(selectedrevisiondetailsid);
        }

        public bool DeleteMasterPromotionItem(string masterpromotionitemid, string selectedpromotionitemids, int userid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.DeleteMasterPromotionItem(masterpromotionitemid, selectedpromotionitemids,userid);            
        }
        public bool DocuSign_RemoveDocumentFromTheProcessQueue(string integrationid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.RemoveDocuSignDocumentFromTheProcessQueue(integrationid);
        }

        public List<CRMContact> GetCRMContactForAccountAsSigner(Guid accountid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetDocuSignSigner(accountid.ToString());
        }

        public List<DocuSignHistory> DocuSign_GetEnvelopeHistory(string envelopeid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetDocuSignEnvelopeHistory(envelopeid);
        }

        public List<DocuSignHistory> DocuSign_GetEnvelopeHistoryByRevision(string revisionid, string versiontype, string printtype)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetDocuSignEnvelopeHistoryByRevision(revisionid, versiontype, printtype);
        }
        public string DocuSign_ValidateSignerAndDocuemnt(string estimateid, string versionnumber, string recipientname, string recipienttype, string recipientaction)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ValidateSignerAndDocuemnt(estimateid,versionnumber,recipientname,recipienttype,recipientaction);
        }

        public List<EstimateGridItem> SearchSpecificJob(string customernumber, string contractnumber, string SelectedSalesConsultantId, string LotNumber, string StreetName,string Suburb, string businessUnit)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();

            Trace.TraceInformation("MRSWCF-SearchSpecificJob - Information - customernumber={0}, contractnumber={1}, SelectedSalesConsultantId={2}, LotNumber={3}, StreetName={4}, Suburb={5}, businessUnit={6}", customernumber, contractnumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb, businessUnit);

            return dataAccess.SearchSpecificJob(customernumber, contractnumber, SelectedSalesConsultantId, LotNumber, StreetName, Suburb, businessUnit);
        }

        public int ResetEditEstimateUserID(int estimateRevisionId, int editEstimateUserID)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ResetEditEstimateUserID(estimateRevisionId, editEstimateUserID);
        }

        public bool ApplyRounding(int revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.ApplyRounding(revisionid);
        }

        public EstimateDisclaimerUpdateDetail GetEstimateDisclaimerUpdateDetails(int revisionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetEstimateDisclaimerUpdateDetails(revisionid);
        }
        public bool SaveDisclaimerUpdateDetails(int revisionId, int typeId, int disclaimerNewId, int userId)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.SaveDisclaimerUpdateDetails(revisionId, typeId, disclaimerNewId, userId);
        }

        public List<GenericClassCodeName> GetBusinessUnits(int regionid)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetBusinessUnits(regionid);
        }

        public string GetBCForecastDate(string bcContractNumber)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            return dataAccess.GetBCForecastDate(bcContractNumber);
        }

        //public List<District> GetDistrictList()
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetDistrictList();
        //}

        //public List<OperatingCenter> GetOperatingCenterList()
        //{
        //    SqlDataAccess dataAccess = new SqlDataAccess();
        //    return dataAccess.GetOperatingCenterList();
        //}
    }
}
