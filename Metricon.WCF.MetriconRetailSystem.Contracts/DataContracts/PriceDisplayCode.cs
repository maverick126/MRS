using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class PriceDisplayCode
    {
        [DataMember]
        public int PriceDisplayCodeId { get; set; }

        [DataMember]
        public string PriceDisplayCodeDescription { get; set; }
    }
}