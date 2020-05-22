using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class SQSSalesRegion
    {
        [DataMember]
        public int RegionId { get; set; }

        [DataMember]
        public string RegionName { get; set; }

    }
}
