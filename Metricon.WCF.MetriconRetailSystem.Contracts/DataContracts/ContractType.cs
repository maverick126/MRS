using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class ContractType
    {
        [DataMember]
        public string ContractTypeID { get; set; }

        [DataMember]
        public string ContractTypeName { get; set; }
    }
}
