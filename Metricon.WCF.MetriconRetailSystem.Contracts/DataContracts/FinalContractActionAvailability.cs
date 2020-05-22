
using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class FinalContractActionAvailability
    {
        [DataMember]
        public bool PreSiteVariationAvailable { get; set; }

        [DataMember]
        public bool BuildingVariationAvailable { get; set; }
    }
}
