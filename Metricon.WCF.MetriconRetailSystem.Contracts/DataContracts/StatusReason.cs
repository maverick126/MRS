using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class StatusReason
    {
        [DataMember]
        public int StatusReasonId { get; set; }

        [DataMember]
        public string StatusReasonName { get; set; }
    }
}

