using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class MarginReportDetail
    {
        [DataMember]
        public int RevisionID { get; set; }

        [DataMember]
        public string BCForecastDate { get; set; }

        [DataMember]
        public int DaysFrom { get; set; }

        [DataMember]
        public int BasePriceExtensionDays { get; set; }

        [DataMember]
        public bool TitledLand { get; set; }

        [DataMember]
        public int TitledLandDays { get; set; }

        [DataMember]
        public double BPECharge { get; set; }

        [DataMember]
        public int RequiredBPEChargeType { get; set; }

        [DataMember]
        public double RequiredBPECharge { get; set; }

        [DataMember]
        public double TodaysPrice { get; set; }

        [DataMember]
        public List<HomePrice> PriceEffectiveDates { get; set; }
    }
}
