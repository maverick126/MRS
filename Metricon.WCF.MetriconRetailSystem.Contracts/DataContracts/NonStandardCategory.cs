using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class NonStandardCategory
    {
        [DataMember]
        public int  CategoryId { get; set; }

        [DataMember]
        public string CategoryName { get; set; }
    }
}
