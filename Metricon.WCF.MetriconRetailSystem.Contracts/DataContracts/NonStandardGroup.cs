using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class NonStandardGroup
    {
        [DataMember]
        public int GroupId { get; set; }

        [DataMember]
        public string GroupName { get; set; }
    }
}
