using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateDetailsComparison
    {
        [DataMember]
        public string AreaName { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string ProductNameA { get; set; }

        [DataMember]
        public string ProductNameB { get; set; }

        [DataMember]
        public string UomA { get; set; }

        [DataMember]
        public string UomB { get; set; }

        [DataMember]
        public string PriceA { get; set; }

        [DataMember]
        public string QuantityA { get; set; }

        [DataMember]
        public string PriceB { get; set; }

        [DataMember]
        public string QuantityB { get; set; }

        [DataMember]
        public string Changes { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string ProductDescriptionA { get; set; }

        [DataMember]
        public string ProductDescriptionB { get; set; }
        [DataMember]
        public string PagID { get; set; }

    }

}

