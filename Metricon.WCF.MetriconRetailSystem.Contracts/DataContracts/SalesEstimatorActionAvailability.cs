
using System;
using System.Collections.Generic;

using System.Runtime.Serialization;


namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SalesEstimatorActionAvailability
    {
        [DataMember]
        public bool ChangeFacadeAvailable { get; set; }

        [DataMember]
        public bool ChangeContractTypeAvailable { get; set; }

        [DataMember]
        public bool ChangeJobFlowTypeAvailable { get; set; }

        [DataMember]
        public bool ChangePriceEffectiveDateAvailable { get; set; }
    }
}
