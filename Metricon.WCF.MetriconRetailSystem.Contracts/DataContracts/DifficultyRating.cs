using System;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class DifficultyRating
    {
        [DataMember]
        public int DifficultyRatingId { get; set; }

        [DataMember]
        public string DifficultyRatingName { get; set; }
    }
}

