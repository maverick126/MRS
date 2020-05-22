using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateHeader : EstimateBase
    {
        [DataMember]
        public string Comments { get; set; }

        [DataMember]
        public DateTime EffectiveDate { get; set; }

        [DataMember]
        public decimal HomePrice { get; set; }

        [DataMember]
        public string HomeRange { get; set; }

        [DataMember]
        public decimal PromotionValue { get; set; }

        [DataMember]
        public string PromotionName { get; set; }

        [DataMember]
        public string Region { get; set; }

        [DataMember]
        public decimal TotalPrice { get; set; }

        [DataMember]
        public decimal TotalPriceExc { get; set; }

        [DataMember]
        public decimal UpgradeValue { get; set; }

        [DataMember]
        public decimal SiteWorkValue { get; set; }

        [DataMember]
        public string LotNumber { get; set; }

        [DataMember]
        public string StreetNumber { get; set; }

        [DataMember]
        public string StreetAddress { get; set; }

        [DataMember]
        public string Suburb { get; set; }

        [DataMember]
        public string PostCode { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string SalesAcceptor { get; set; }

        [DataMember]
        public string DraftPerson { get; set; }

        [DataMember]
        public string SalesEstimator { get; set; }

        [DataMember]
        public string CSC { get; set; }

        [DataMember]
        public decimal TotalCostBTP { get; set; }
        [DataMember]
        public decimal TotalCostDBC { get; set; }

        [DataMember]
        public decimal Margin { get; set; }

        [DataMember]
        public decimal TotalMargin { get; set; }

        [DataMember]
        public string MarginString { get; set; }

        [DataMember]
        public decimal TargetMargin { get; set; }

        [DataMember]
        public string CustomerDocumentName { get; set; }

        [DataMember]
        public string CustomerDocumentDesc { get; set; }

        [DataMember]
        public DateTime DepositDate { get; set; }

        [DataMember]
        public string HomeAndLandPackage { get; set; }

        [DataMember]
        public decimal TotalVariation { get; set; }

        [DataMember]
        public int StdPriceHoldDays { get; set; }

        [DataMember]
        public DateTime PriceExpiryDate { get; set; }

        [DataMember]
        public int ReversedPriceHoldDays { get; set; }

        [DataMember]
        public int BasePriceExtensionDays { get; set; }

        [DataMember]
        public decimal RequiredPBE5Percent { get; set; }

        [DataMember]
        public decimal RequiredPBERollback { get; set; }
    }
}
