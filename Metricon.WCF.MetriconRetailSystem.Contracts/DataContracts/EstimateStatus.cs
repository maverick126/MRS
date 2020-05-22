using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateStatus
    {
        [DataMember]
        public int StatusId { get; set; }

        [DataMember]
        public string StatusName { get; set; }
    }
}
