using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class RoleAccessModule
    {
        [DataMember]
        public int RoleId { get; set; }

        [DataMember]
        public bool AccessMarginModule { get; set; }

        [DataMember]
        public bool AccessStudioMModule { get; set; }
    }
}
