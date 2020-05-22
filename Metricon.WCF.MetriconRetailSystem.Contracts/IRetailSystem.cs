using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ServiceModel;
using System.Runtime.Serialization;


namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    public enum RESULT_TYPE
    {
        ALL = 0,
        CURRENT = 1
    }

    [ServiceContract]
    public interface IRetailSystem
    {
        [OperationContract]
        void AcceptOriginalEstimate(int estimateId, int userId);

        [OperationContract]
        int AssignQueuedEstimate(int queueId, int userId, int ownerId);

        [OperationContract]
        void AssignWorkingEstimate(int estimateRevisionId, int userId, int ownerId);

        [OperationContract]
        void CompleteEstimate(int revisionId, int userId, int statusId, int statusReasonId, int revisionTypeId, int ownerId);

        [OperationContract]
        List<EstimateGridItem> GetQueuedEstimates(
            int revisionTypeId, //0 equals all
            int regionId,
            int roleId,
            string customerNumber,
            string contractNumber,
            int salesConsultantId,
            string lotNumber,
            string streetName,
            string suburb, string businessUnit);

        //[OperationContract]
        //List<EstimateGridItem> GetAssignedEstimatesByUser(
        //    int revisionTypeId, //0 equals all
        //    int roleId,
        //    int statusId,
        //    int userId,
        //    string customerNumber,
        //    string contractNumber,
        //    int salesConsultantId,
        //    string lotNumber,
        //    string streetName,
        //    string suburb);

        //[OperationContract]
        //List<EstimateGridItem> GetAssignedEstimatesByRegion(
        //    int revisionTypeId, //0 equals all
        //    int roleId,
        //    int statusId,
        //    int regionId,
        //    string customerNumber,
        //    string contractNumber,
        //    int salesConsultantId,
        //    string lotNumber,
        //    string streetName,
        //    string suburb);

        [OperationContract]
        List<EstimateGridItem> GetAssignedEstimates(
            int revisionTypeId, //0 equals all
            int roleId,
            int statusId,
            int userId,
            int regionId,
            string customerNumber,
            string contractNumber,
            int salesConsultantId,
            string lotNumber,
            string streetName,
            string suburb, string businessUnit);

        [OperationContract]
        EstimateHeader GetEstimateHeader(int revisionId);

        [OperationContract]
        List<EstimateDetails> GetEstimateDetails(int revisionId);

        //[OperationContract]
        //List<string[]> GetEstimateDetailsAsArray(int revisionId);

        //[OperationContract]
        //void CommenceWork(int revisionId, int userId);

        [OperationContract]
        List<EstimateHeader> GetEstimatesRevisions(int estimateId);

        [OperationContract]
        string UndoThisRevision(int bcContractNumber, int estimateId, int estimateRevisionId, int userId, string reasonComment);

        [OperationContract]
        List<EstimateHeader> UndoThisRevisionValidate(int estimateId, int bcContractNumber, int estimateRevisionId);

        [OperationContract]
        string UndoCurrentMilestone(int estimateRevisionId, int userId, string reasonComment);

        [OperationContract]
        string UndoSetAsContract(int estimateRevisionId, int userId, string reasonComment);
        //[OperationContract]
        //void InsertProduct(int revisionId, int estimateDetailsId, int userId);

        [OperationContract]
        void UpdateEstimateDetails(
            int revisionDetailsId,
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
            string costdbc
            );

        [OperationContract]
        EstimateDetails DeleteProduct(int revisionDetailsId, string reason, int reasonid, int userId);

        //[OperationContract]
        //EstimateDetails DeleteProducts(int revisionDetailsId, string estimatedetailsidstring, string areaidstring, string groupidstring, string reason, int reasonid, int userId);

        [OperationContract]
        void InsertComment(int revisionId, string comment, int userId);

        [OperationContract]
        void UpdateComment(int estimateRevisionId, string comment, int userid,  int variationnumber, string variationsummary);

        [OperationContract]
        void UpdateEstimateStatus(int revisionId, int statusId, int statusReasonId, int userId);

        [OperationContract]
        void UpdateEstimateDifficultyRating(int estimateRevisionId, int difficultyRatingId, int userId);

        [OperationContract]
        void UpdateQueueDifficultyRating(int queueId, int difficultyRatingId);

        [OperationContract]
        void MarginReport_SaveDetails(int estimateRevisionId, int titledLand, int titledLandDays, int basePriceExtensionDays, DateTime effectiveDate, double bpeCharge, int requiredBPEChargeType, int userId);

        [OperationContract]
        MarginReportDetail MarginReport_GetDetails(int estimateRevisionId);

        [OperationContract]
        void UpdateEstimateDueDate(int estimateRevisionId, DateTime duedate, int userId);

        [OperationContract]
        void UpdateEstimateAppointmentTime(int estimateRevisionId, DateTime appointmentTime, int userId);

        [OperationContract]
        void UpdateQueueDueDate(int queueId, DateTime duedate, int userid);

        [OperationContract]
        void UpdateEstimateEffectiveDate(int estimateRevisionId, int priceId, int userId);

        //[OperationContract]
        //bool CheckEstimateUpdatability(int revisionId);

        //[OperationContract]
        //void ReEditEstimate(int revisionId, int userId);

        [OperationContract]
        List<User> GetUsersByRegionAndRole(int regionId, int roleId);

        [OperationContract]
        List<User> GetUsersByRegionAndRevisionType(int regionId, int revisionTypeId);

        [OperationContract]
        User GetCurrentUser(string loginName);

        [OperationContract]
        List<UserRole> GetUserRoles(int userId);

        [OperationContract]
        List<EstimateStatus> GetEstimateStatuses();

        [OperationContract]
        List<StatusReason> GetStatusReasons(int statusId, int revisionTypeId);

        [OperationContract]
        List<DifficultyRating> GetDifficultyRatings();

        [OperationContract]
        List<RevisionType> GetRevisionTypeAccess(int roleId);
        
        [OperationContract]
        List<HomePrice> GetHomePrices(int estimateRevisionId);

        [OperationContract]
        List<AuditLog> GetAuditTrail(int estimateId);

        [OperationContract]
        List<EstimateDetailsComparison> CompareEstimateDetails(int estimateRevisionIdA, int estimateRevisionIdB);

        [OperationContract]
        List<EstimateHeaderComparison> CompareEstimateHeader(int estimateRevisionIdA, int estimateRevisionIdB);

        [OperationContract]
        int GetLatestEstimateRevisionId(int estimateId);

        [OperationContract]
        int GetResubmittedEstimateCount(int userId, int regionId);

        //[OperationContract]
        //int GetOnHoldEstimateCount(string revisionTypeIds, int userId, int regionId);

        //[OperationContract]
        //List<PAG> GetOptionTree(string revisionid);

        //[OperationContract]
        //List<EstimateDetails> GetOptionTreeAsEstimateDetails(string revisionid);

        [OperationContract]
        List<OptionTreeProducts> GetOptionTreeFromMasterHome(string regionid, string homeid);

        //[OperationContract]
        //List<string[]> GetOptionTreeAsArray(string revisionid);

        [OperationContract]
        Boolean CheckValidProductByRevision(int revisionId, string productId);

        [OperationContract]
        List<OptionTreeProducts> GetOptionTreeAsOptionTreeProductsForEstimateItemReplace(string revisionId, string areaName, string groupName);

        [OperationContract]
        List<OptionTreeProducts> GetOptionTreeAsOptionTreeProducts(string revisionid);

        [OperationContract]
        List<OptionTreeProducts> GetOptionTreeFromAllProducts(string regionid, string searchText);

        [OperationContract]
        List<OptionTreeProducts> GetOptionTreeFromAllProductsExtended(int stateid, string regionid, int homeid, string productname, string productdesc, int areaid, int groupid);

        [OperationContract]
        List<PAG> GetSelectedPAG(string estimateid, string revisionnumber);

        [OperationContract]
        List<EstimateComments> GetCommentsForAnEstimate(string revisionid);

        [OperationContract]
        bool GetAccessPermission(string revisionid, string userid, string roleid);

        [OperationContract]
        List<int> GetEstimateCount(int userId, int roleId);

        [OperationContract]
        int SaveSelectedItem(int selectedid, int revisionid, int pagid, int userid);

        [OperationContract]
        bool SaveEditItemDetails(int selectedid, int revisionid, decimal qty, decimal sellprice, string productdescription, string extradescription, string internaldescription);

        [OperationContract]
        bool RemoveItem(int selectedid, int estimateid);

        [OperationContract]
        EstimateDetails CopyItemFromOptionTreeToEstimate(int estimatedetailsid, int revisiondetailsid,  int revisionid, int productareagroupid, int userid);

        [OperationContract]
        EstimateDetails CopyItemFromMasterHomeToEstimate(int regionid, int optionid, int revisionid, int userid);

        [OperationContract]
        EstimateDetails CopyItemFromAllProductsToEstimate(int regionid, string productid, int revisionid, int userid);

        [OperationContract]
        bool SynchronizeNewOptionToEstimate(int revisionid);

        [OperationContract]
        List<EstimateDetails> GetAdditionalNotesTemplateAndProducts(int revisionid, int userid);

        [OperationContract]
        List<EstimateDetails> GetAdditionalNotesTemplateAndProductsByRegion(string templatename, string subregionid, int userid, int active, int selectedroleid);

        [OperationContract]
        bool UpdateNoteTemplate(int templateid, string templatename, int status, int userid, string action);

        [OperationContract]
        string CheckNewNoteTemplateNameExists(int templateid, string templatename);

        [OperationContract]
        bool AddAdditonalNotesTemplate(string templatename, int revisionid, int userid);

        //[OperationContract]
        //bool CreateActivityForOpportunity(string opportunityid, string username, string subject, DateTime duedate, string mobilephone, string notes);

        [OperationContract]
        List<SQSConfiguration> GetSQSConfiguration(string configCode, string codeValue);

        [OperationContract]
        List<SQSSalesRegion> GetSalesRegionByState(string stateid);

        [OperationContract]
        List<SQSSalesRegion> GetPriceRegionByState(string stateid);

        [OperationContract]
        bool CreateSalesEstimateLog(string username, MRSLogAction action, int estimateRevisionId, string extraDescription, int reasonCode);

        [OperationContract]
        bool SetContractStatus(string username, int estimateRevisionId, ContractStatus status);

        [OperationContract]
        bool RemoveItemFromNotesTemplate(string templateid, string productareagroupid, int userid);

        [OperationContract]
        List<EstimateDetails> GetAvailableItemsForNotesTemplate(string templateid, string searchtext);

        [OperationContract]
        bool AddItemToNotesTemplate(string templateid, string selecteditemids, int userid);

        [OperationContract]
        bool AddNewNotesTemplate(string templatename, string regionid, int userid);

        [OperationContract]
        bool RemoveNotesTemplate(string templateid, int userid);

        [OperationContract]
        bool CopyNotesTemplate(string templatename, string regionid, int userid, string templateid);

        [OperationContract]
        bool CreateTaskForContract(string contractid, int revisionid, string subject, DateTime duedate, string category, string notes);

        [OperationContract]
        bool UpdateNotesTemplateItem(string templateid, string productareagroupid, decimal quanitity, decimal price, string extradescription, string internaldescription, string additionalinfo, int userid, bool usedefaultquantity);

        //[OperationContract]
        //bool SyncCustomerDetailsFromBCToCRM(string customerNo, int userid);

        [OperationContract]
        string SynchroniseCustomerDetails(string contractNo);

        [OperationContract]
        List<NonStandardCategory> GetNonstandardCategory();

        [OperationContract]
        List<NonStandardCategory> GetNonstandardCategoryByState(int stateid, int selectedareaid);

        [OperationContract]
        List<NonStandardGroup> GetNonstandardGroups(int selectedareaid, int stateid, int selectedgroupid);

        [OperationContract]
        List<PriceDisplayCode> GetPriceDisplayCodes();

        [OperationContract]
        List<ProductImage> GetProductImages(string productid, int supplierid);

        [OperationContract]
        string CheckEstimateLockStatus(int estimaterevisionid);
        
        [OperationContract]
        void UnlockEstimate(int estimaterevisionid, int type);

        [OperationContract]
        List<ValidationErrorMessage> ValidateStudioMEstimate(int estimaterevisionid);

        [OperationContract]
        bool UpdateItemAcceptance(string revisionestimatedetailsid, int accepted, int userid);

        [OperationContract]
        ContractDraftActionAvailability GetContractDraftActionAvailability(int estimateRevisionId);

        [OperationContract]
        FinalContractActionAvailability GetFinalContractActionAvailability(int estimateRevisionId, string contractNumber);

        [OperationContract]
        CustomerSupportActionAvailability GetCustomerSupportActionAvailability(int estimateRevisionId, string contractNumber);

        [OperationContract]
        SalesEstimatorActionAvailability GetSalesEstimatorActionAvailability(int estimateRevisionId, int userid);

        [OperationContract]
        bool GetContractDraftCreationVisibility(int estimateRevisionId);

        [OperationContract]
        bool GetFinalContractCreationVisibility(int estimateRevisionId);

        [OperationContract]
        void GetEstimateRevisionStatus(EstimateRevisionStatus status);

        [OperationContract]
        void CreateSplitStudioMRevisions(int estimateRevisionId, string revisionTypeIds, string assignedToUserIds, int createdbyId);

        [OperationContract]
        void MergeStudioMRevisions(int estimateRevisionId, int createdbyId);

        [OperationContract]
        void CreateContractDraft(int estimateRevisionId, int createdbyId/*, DateTime appointment*/);

        [OperationContract]
        void CreateFinalContract(int estimateRevisionId, int createdbyId/*, DateTime appointment*/);

        [OperationContract]
        void CreateCscVariation(int estimateRevisionId, int createdbyId);

        [OperationContract]
        string CreateStudioMRevision(int estimateRevisionId, int ownerId, DateTime appointmentDateTime, int revisionTypeId, int createdbyId);

        [OperationContract]
        string ValidateSetEstimateStatus(int estimateRevisionId, int nextRevisionTypeId);

        [OperationContract]
        string GetCustomerDocumentType(int estimateRevisionId);

        [OperationContract]
        int UpdateCustomerDocumentDetails(CustomerDocumentDetails document);

        [OperationContract]
        CustomerDocumentDetails GetCustomerDocumentDetails(int estimateRevisionId);

        [OperationContract]
        List<SimplePAG> GetRelevantPAGFromOnePAG(string estimatedetailsid, /*string standardinclusionid,*/ string revisionid);

        [OperationContract]
        void CreateVariation(int estimateRevisionId, int revisionTypeId, int userId);

        [OperationContract]
        void RejectVariation(int estimateRevisionId, int userId);

        [OperationContract]
        List<EstimateDetails> SaveSelectedItemsFromOptionTreeToEstimate(string optionidstring, 
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
            string internaldescriptionstring);

        [OperationContract]
        List<EstimateDetails> ReplaceSaveSelectedItemsFromOptionTreeToEstimate(string sourceEstimateRevisionDetailsId, string optionidstring,
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
            );

        [OperationContract]
        RevisionTypePermission CheckRevisionTypeAllowToAddNSR(int revisontypeid);

        [OperationContract]
        List<ValidationErrorMessage> ValidateAcceptFlagForRevision(int estimaterevisionid, int userroleid);
        
        [OperationContract]
        List<ValidationErrorMessage> ValidateStudioMRevisions(int estimaterevisionid);

        [OperationContract]
        bool ValidateAppointmentDate(int estimaterevisionid);

        [OperationContract]
        List<NextRevision> GetNextEstimateRevision(int estimateRevisionId, int statusId);

        [OperationContract]
        List<AuditLog> GetAuditLogs(int revisionid, int revisiondetailid);

        [OperationContract]
        List<DeletedItem> GetDeletedItems(int revisionid, RESULT_TYPE resulttype);

        [OperationContract]
        bool ReAddDeletedEstimateItem(int sourceEstimateRevisionId, int targetEstimateRevisionId, int OptionId, int userId);

        [OperationContract]
        bool ReAddDeletedMasterPromotionEstimateItem(int estimateRevisionId, int OptionId, int userId);

        [OperationContract]
        List<SQSHome> GetAllFacadeFromRevisonID(int revisionid);

        [OperationContract]
        List<SQSHome> GetAllAvailableHomeByState(int stateid, string searchText, bool showdisplayhomes);

        [OperationContract]
        List<SQSHome> GetHomeFullNameByState(int stateid, int userId);
        [OperationContract]
        List<SQSArea> GetAreaNameWithAll();
        [OperationContract]
        List<SQSGroup> GetGroupNameWithAll();

        [OperationContract]
        List<ValidationErrorMessage> CheckFacadeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate);

        [OperationContract]
        List<ValidationErrorMessage> CheckHomeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate);

        [OperationContract]
        bool ChangeFacade(int revisionid, int newfacadehomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid);

        [OperationContract]
        bool ChangeHome(int revisionid, int newfacadehomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid);

        [OperationContract]
        List<ContractType> GetContractType(string configCode);

        [OperationContract]
        bool UpdateContractType(int revisionid, string contracttype, string jobflowtype, int userid);

        [OperationContract]
        string GetHomeName(int revisionid);

        [OperationContract]
        bool UpdateHomeName(int revisionid, string homename, int userid);

        [OperationContract]
        List<ValidationErrorMessage> CopyEstimateCheckDifference(string sourceEstimatenumber, string destinationEstimatenumber);

        [OperationContract]
        bool CopyEstimate(string sourceEstimatenumber, string destinationEstimatenumber);

        [OperationContract]
        List<decimal> GetAreaSurcharge(int revisionid);

        [OperationContract]
        EstimateDetails GetPagByID(int estimaterevisionid, int optionid);

        [OperationContract]
        List<SimplePAG> GetUpgradeOptionListForStandardInclusion(int estimaterevisionid, int originateoptionid);

        [OperationContract]
        string GetStudioMQandA(int optionId);

        [OperationContract]
        List<SharepointDoc> Sharepoint_GetFileList(string opportunityid, string contractnumber);


        [OperationContract]
        bool Sharepoint_SharepointUploadFile(string filename,byte[] contents, string oldname, string opportunityid, string doccategory, string doctype);

        [OperationContract]
        bool Sharepoint_DeleteFileFromSharepointLibrary(SharepointDoc doc, string opportunityid, string contractnumber);

        [OperationContract]
        List<SharepointDocumentType> Sharepoint_GetSalesDocumentType();

        [OperationContract]
        bool SendNotificationEmail(string templateobjectid, string actionuserfullname, string recipientfullname, string contractnumber, string templateobjecttype, string emailtype, string estimatenumber, string revisionnumber, string comments);

        [OperationContract]
        bool SendCrmEmail(Guid contractId, int recipientId, string subject, string body);

        [OperationContract]
        string GetStudioMQuestionForAProduct(string pproductid);

        [OperationContract]
        List<PAG> GetInLieuStandardPromotionItems(int estimaterevisionid, int originateoptionid);

        [OperationContract]
        RoleAccessModule GetRoleAccessModule(int roleid);

        [OperationContract]
        List<ItemRemoveReason> GetItemRemoveReason();

        [OperationContract]
        List<StudioMItem> GetItemsNeedSetDefaultAnswer(string revisionid);

        [OperationContract]
        bool SetDefaultAnswerForEstimateRevision(string idstring, string studiomstring, string usercode);

        [OperationContract]
        void RegisterEvent(string action, int revisionid, int userid);

        [OperationContract]
        DateTime GetStudioMAppointmentTime(string contractNumber, string eventNumber);

        [OperationContract]
        void UpdateEstimateDetailsDescription(List<EstimateDetails> items, int userId);

        [OperationContract]
        bool MoveEstimateDetailItem(int revisionDetailsIdSource, int revisionDetailsIdTarget, int userId);

        [OperationContract]
        List<DocuSignDocStatusInfo> DocuSign_GetDocumentInfo(string revisionid, string estimateid);

        [OperationContract]
        bool DocuSign_PushDocumentToTheProcessQueue(string revisionid, string printtype, string documenttype, int userid);

        [OperationContract]
        List<EstimateDetails> GetPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid);

        [OperationContract]
        List<PromotionPAG> GetExistingPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid);

        [OperationContract]
        List<EstimateDetails> GetEstimateDetailsByIDString(string selectedrevisiondetailsid);

        [OperationContract]
        bool DeleteMasterPromotionItem(string masterpromotionitemid, string selectedpromotionitemids, int userid);

        [OperationContract]
        bool DocuSign_RemoveDocumentFromTheProcessQueue(string integrationid);

        [OperationContract]
        List<CRMContact> GetCRMContactForAccountAsSigner(Guid accountid);

        [OperationContract]
        List<DocuSignHistory> DocuSign_GetEnvelopeHistory(string envelopeid);

        [OperationContract]
        List<DocuSignHistory> DocuSign_GetEnvelopeHistoryByRevision(string revisionid, string versiontype, string printtype);

        [OperationContract]
        string DocuSign_ValidateSignerAndDocuemnt(string estimateid, string versionnumber, string recipientname, string recipienttype, string recipientaction);

        [OperationContract]
        List<EstimateGridItem> SearchSpecificJob(string customernumber, string contractnumber, string SelectedSalesConsultantId, string LotNumber, string StreetName, string Suburb, string businessUnit);

        [OperationContract]
        int ResetEditEstimateUserID(int estimateRevisionId, int editEstimateUserID);

        [OperationContract]
        bool ApplyRounding(int revisionid);

        [OperationContract]
        EstimateDisclaimerUpdateDetail GetEstimateDisclaimerUpdateDetails(int revisionid);

        [OperationContract]
        bool SaveDisclaimerUpdateDetails(int revisionId, int typeId, int disclaimerNewId, int userId);

        [OperationContract]
        List<GenericClassCodeName> GetBusinessUnits(int regionid);

        [OperationContract]
        string GetBCForecastDate(string bcContractNumber);

        //[OperationContract]
        //List<District> GetDistrictList();

        //[OperationContract]
        //List<OperatingCenter> GetOperatingCenterList();

    }
}
