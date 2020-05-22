using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{  
    [DataContract]
    public class StandardInlusionAndUpgradeOption
    {
        [DataMember]
        public string Standardinclusion { get; set; }

        [DataMember]
        public string UpgradeOption { get; set; }
    }
}
