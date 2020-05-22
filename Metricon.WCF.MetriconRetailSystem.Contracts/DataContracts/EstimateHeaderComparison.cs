using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class EstimateHeaderComparison
    {
        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string ValueA { get; set; }

        [DataMember]
        public string ValueB { get; set; }
    }

}
