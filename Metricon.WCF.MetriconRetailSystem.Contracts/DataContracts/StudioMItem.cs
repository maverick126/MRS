using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class StudioMItem
    {
        [DataMember]
        public int MRSEstimateDetailsId { get; set; }

        [DataMember]
        public string StudioMQestion { get; set; }

        [DataMember]
        public string StudioMDefaultAnswer { get; set; }

    }
}
