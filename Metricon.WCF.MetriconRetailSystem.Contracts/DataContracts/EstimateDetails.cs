using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateDetails
    {
        [DataMember]
        public int EstimateRevisionDetailsId { get; set; }

        [DataMember]
        public int HomeDisplayOptionId { get; set; }

        [DataMember]
        public int StandardInclusionId { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public decimal UpdatedPrice { get; set; }

        [DataMember]
        public decimal UpdatedQuantity { get; set; }

        [DataMember]
        public decimal TotalPrice { get; set; }

        [DataMember]
        public decimal UpdatedTotalPrice { get; set; }

        [DataMember]
        public string ProductDescription { get; set; }

        [DataMember]
        public string ProductDescriptionShort { get; set; }

        [DataMember]
        public string ExtraDescription { get; set; }

        [DataMember]
        public string InternalDescription { get; set; }

        [DataMember]
        public string AdditionalNotes { get; set; }

        [DataMember]
        public bool StandardOption { get; set; }

        [DataMember]
        public bool PromotionProduct { get; set; }

        [DataMember]
        public int AreaId { get; set; }

        [DataMember]
        public string AreaName { get; set; }

        [DataMember]
        public int GroupId { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string ProductId { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public string Uom { get; set; }

        [DataMember]
        public string TemplateName { get; set; }

        [DataMember]
        public string TemplateID { get; set; }

        [DataMember]
        public string RegionName { get; set; }

        [DataMember]
        public int ProductAreaGroupID { get; set; }

        [DataMember]
        public int CreatedByUserId { get; set; }

        [DataMember]
        public string CreatedByUserManagerIds { get; set; }

        [DataMember]
        public string UpdatedExtraDescription { get; set; }

        [DataMember]
        public string UpdatedInternalDescription { get; set; }

        [DataMember]
        public string UpdatedProductDescription { get; set; }

        [DataMember]
        public string UpdatedAdditionalNotes { get; set; }

        [DataMember]
        public bool ItemAccepted { get; set; }

        [DataMember]
        public bool UpdatedItemAccepted { get; set; }

        [DataMember]
        public bool UpdatedItemDeleted { get; set; }

        [DataMember]
        public int NonstandardCategoryID { get; set; }

        [DataMember]
        public int NonstandardGroupID { get; set; }

        [DataMember]
        public int UpdatedNonstandardCategoryID { get; set; }

        [DataMember]
        public int UpdatedNonstandardGroupID { get; set; }

        [DataMember]
        public bool StudioMProduct { get; set; }

        [DataMember]
        public int ProductPhotoCount { get; set; }

        [DataMember]
        public string SOSI { get; set; }

        [DataMember]
        public string SOSIToolTips { get; set; }

        [DataMember]
        public string StudioMQuestion { get; set; }

        [DataMember]
        public string StudioMAnswer { get; set; }

        [DataMember]
        public string StudioMIcon { get; set; }

        [DataMember]
        public string StudioMTooltips { get; set; }

        [DataMember]
        public string SelectedImageID { get; set; }

        [DataMember]
        public bool SiteWorkItem { get; set; }

        [DataMember]
        public bool UpdatedSiteWorkItem { get; set; }

        [DataMember]
        public int StudioMSortOrder { get; set; }

        [DataMember]
        public bool Changed { get; set; }

        [DataMember]
        public bool PreviousChanged { get; set; }

        [DataMember]
        public bool StudioMAnswerMandatory { get; set; }

        [DataMember]
        public bool ItemAllowToRemove { get; set; }

        [DataMember]
        public bool ItemAllowToChangeQuantity { get; set; }

        [DataMember]
        public bool ItemAllowToChangePrice { get; set; }

        [DataMember]
        public bool ItemAllowToChangeDescription { get; set; }

        [DataMember]
        public bool ItemAllowToChangeDisplayCode { get; set; }

        [DataMember]
        public bool TemplateActive { get; set; }

        [DataMember]
        public string PriceDisplayCodeDesc { get; set; }

        [DataMember]
        public int PriceDisplayCodeId { get; set; }

        [DataMember]
        public int UpdatedPriceDisplayCodeId { get; set; }

        [DataMember]
        public bool DerivedCost { get; set; }

        [DataMember]
        public string CostExcGST { get; set; }

        [DataMember]
        public string UpdatedBTPCostExcGST { get; set; }

        [DataMember]
        public string UpdatedDBCCostExcGST { get; set; }

        [DataMember]
        public string DerivedCostIcon { get; set; }

        [DataMember]
        public string DerivedCostTooltips { get; set; }

        [DataMember]
        public string Margin { get; set; }

        [DataMember]
        public string MarginDBCCost { get; set; }

        [DataMember]
        public string MarginString { get; set; }

        [DataMember]
        public string OwnerName { get; set; }

        [DataMember]
        public bool IsPrivate { get; set; }

        [DataMember]
        public string Changes { get; set; }

        [DataMember]
        public bool IsMasterPromotion { get; set; }

        [DataMember]
        public int Homeid { get; set; }

        [DataMember]
        public int HomeDisplayID { get; set; }

        [DataMember]
        public bool IsPrePackageItem { get; set; }

        [DataMember]
        public string PrePackageItemDescription { get; set; }

        [DataMember]
        public bool IsColMarginPercentAvailable { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public string CreatedOn { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public string ModifiedOn { get; set; }

        [DataMember]
        public bool UseDefaultQuantity { get; set; }
    }
}
