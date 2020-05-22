using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class OptionTreeProducts
    {
        [DataMember(Name="I")]
        public int HomeDisplayOptionId { get; set; }

        [DataMember(Name = "A")]
        public string AreaName { get; set; }

        [DataMember(Name = "G")]
        public string GroupName { get; set; }

        [DataMember(Name = "AI")]
        public int AreaId { get; set; }

        [DataMember(Name = "GI")]
        public int GroupId { get; set; }

        [DataMember(Name = "PI")]
        public string ProductId { get; set; }

        [DataMember(Name = "N")]
        public string ProductName { get; set; }

        [DataMember(Name = "D")]
        public string ProductDescription { get; set; }

        [DataMember(Name = "P")]
        public decimal Price { get; set; }

        [DataMember(Name = "Q")]
        public decimal Quantity { get; set; }

        [DataMember(Name = "M")]
        public int StudioMProduct { get; set; }
        // 0 Non Studio M
        // 1 Studio M Mandatory
        // 2 Studio M No question
        // 3 Studio M Non-mandatory

        [DataMember(Name = "S")]
        public bool StandardOption { get; set; }

        [DataMember(Name = "W")]
        public bool IsSiteWork { get; set; }

        [DataMember(Name = "O")]
        public int StudioMSortOrder { get; set; }


        [DataMember(Name = "U")]
        public string UOM { get; set; }
        //[DataMember(Name = "DC")]
        //public bool DerivedCost { get; set; }

        //[DataMember(Name = "CG")]
        //public decimal CostExcGST { get; set; }

    }
}
