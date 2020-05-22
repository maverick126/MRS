using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class DocuSignHistory
    {
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string ActionStatus { get; set; }
        [DataMember]
        public DateTime ActionTime { get; set; }
        [DataMember]
        public string EnvelopeStatus { get; set; }

    }
}
