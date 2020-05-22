using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class CustomerSupportActionAvailability
    {
        [DataMember]
        public bool PreStudioVariationAvailable { get; set; }

        [DataMember]
        public bool CustomerSupportAvailable { get; set; }

        [DataMember]
        public bool ContractDraftAvailable { get; set; }

        [DataMember]
        public bool AssignSTMSplitAvailable { get; set; }

        [DataMember]
        public bool FinalContractAvailable { get; set; }

        [DataMember]
        public bool PreSiteVariationAvailable { get; set; }

        [DataMember]
        public bool BuildingVariationAvailable { get; set; }

        [DataMember]
        public bool ChangeContractTypeAvailable { get; set; }

        [DataMember]
        public bool ChangeJobFlowTypeAvailable { get; set; }
    }
}
