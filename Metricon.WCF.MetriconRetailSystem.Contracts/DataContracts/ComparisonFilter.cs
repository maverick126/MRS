using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class ComparisonFilter
    {
        [DataMember]
        public string ColumnUniqueName { get; set; }

        [DataMember]       
        public string FilterValue { get; set; }
    }
}
