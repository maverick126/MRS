using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class HomePrice
    {
        [DataMember]
        public int PriceId { get; set; }

        [DataMember]
        public string EffectiveDateOptionName { get; set; } //Display string in Combo Box

        [DataMember]
        public DateTime EffectiveDate { get; set; }

        [DataMember]
        public Decimal EffectivePrice { get; set; }
    }
}