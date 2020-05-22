using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class User
    {
        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string EmailAddress { get; set; }

        [DataMember]
        public int RegionId { get; set; }

        [DataMember]
        public string RegionName { get; set; }

        [DataMember]
        public int StateId { get; set; }

        [DataMember]
        public int PrimaryRoleId { get; set; }

        [DataMember]
        public int LoginPriceRegionId { get; set; }
    }
}
