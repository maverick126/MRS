using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class DeletedItem
    {
        [DataMember]
        public int RevisionId { get; set; }

        [DataMember]
        public string RevisionName { get; set; }

        [DataMember]
        public int HomeDisplayOptionId { get; set; }

        [DataMember]
        public string AreaName { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string ProductName { get; set; }

        [DataMember]
        public string ProductDescription { get; set; }

        [DataMember]
        public string AdditionalNotes { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public decimal TotalPrice { get; set; }

        [DataMember]
        public string Uom { get; set; }

        [DataMember]
        public string DeletedOn { get; set; }

        [DataMember]
        public string DeletedBy { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public bool PromotionProduct { get; set; }

        [DataMember]
        public bool IsMasterPromotion { get; set; }
    }
}
