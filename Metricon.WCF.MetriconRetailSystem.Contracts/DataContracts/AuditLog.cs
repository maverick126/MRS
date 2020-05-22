using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class AuditLog
    {
        [DataMember]
        public string LogId { get; set; }

        [DataMember]
        public string RevisionNumber { get; set; }

        [DataMember]
        public string EstimateNumber { get; set; }

        [DataMember]
        public string RevisionType { get; set; }

        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string Action { get; set; }

        [DataMember]
        public DateTime LogTime { get; set; }

        [DataMember]
        public string Description { get; set; }

    }
}

