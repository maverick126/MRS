using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateDisclaimerUpdateDetail
    {
        [DataMember]
        public int RevisionId { get; set; }

        [DataMember]
        public int DisclaimerCurrentId { get; set; }

        [DataMember]
        public int DisclaimerNewId { get; set; }

        [DataMember]
        public int DisclaimerVariationCurrentId { get; set; }

        [DataMember]
        public int DisclaimerVariationNewId { get; set; }
    }
}