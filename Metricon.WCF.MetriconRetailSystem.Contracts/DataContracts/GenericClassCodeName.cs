using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class GenericClassCodeName
    {
        [DataMember]
        public string CodeValue { get; set; }

        [DataMember]
        public string CodeText { get; set; }
    }
}
