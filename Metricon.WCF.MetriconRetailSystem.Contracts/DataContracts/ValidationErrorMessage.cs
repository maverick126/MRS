using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class ValidationErrorMessage
    {
        [DataMember]
        public int HomeDisplayOptionId { get; set; }

        [DataMember]
        public int PagID { get; set; }

        [DataMember]
        public string Area { get; set; }

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public decimal SellPrice { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string PossibleUpgrade { get; set; }

        [DataMember]
        public string ErrorIcon { get; set; }

        [DataMember]
        public string ErrorIconToolTips { get; set; }

        [DataMember]
        public bool AllowGoAhead { get; set; }

        [DataMember]
        public bool AddVisible { get; set; }

        [DataMember]
        public bool UpgradeVisible { get; set; }

        [DataMember]
        public bool AnswerVisible { get; set; }

        [DataMember]
        public double AddImageOpacity { get; set; }

        [DataMember]
        public double UpgradeImageOpacity { get; set; }

        [DataMember]
        public double AnswerImageOpacity { get; set; }

        [DataMember]
        public bool CopyAsNSR { get; set; }

        [DataMember]
        public bool QuantityUseCurrent { get; set; }

        [DataMember]
        public bool QuantityUseNew { get; set; }

        [DataMember]
        public bool PriceUseCurrent { get; set; }

        [DataMember]
        public bool PriceUseNew { get; set; }
    }
}
