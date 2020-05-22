using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class Product
    {
        [DataMember]
        public string ProductNumber { get; set; }
    }
}
