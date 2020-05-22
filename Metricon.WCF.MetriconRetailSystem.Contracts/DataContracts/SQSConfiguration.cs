using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SQSConfiguration
    {
        [DataMember]
        public string Code { get; set; }

        [DataMember]
        public string CodeValue { get; set; }

        [DataMember]
        public string CodeText { get; set; }
    }
}
