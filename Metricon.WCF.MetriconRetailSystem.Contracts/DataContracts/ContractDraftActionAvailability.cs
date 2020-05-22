
using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class ContractDraftActionAvailability
    {
        [DataMember]
        public bool ColourSelectionAvailable { get; set; }

        [DataMember]
        public bool ElectricalSelectionAvailable { get; set; }

        [DataMember]
        public bool PavingSelectionAvailable { get; set; }

        [DataMember]
        public bool TilesSelectionAvailable { get; set; }

        [DataMember]
        public bool DeckingSelectionAvailable { get; set; }

        [DataMember]
        public bool CarpetSelectionAvailable { get; set; }

        [DataMember]
        public bool CurtainSelectionAvailable { get; set; }

        [DataMember]
        public bool FloorSelectionAvailable { get; set; }

        [DataMember]
        public bool ApplianceSelectionAvailable { get; set; }

        [DataMember]
        public bool LandscapingSelectionAvailable { get; set; }

        [DataMember]
        public bool StudioMAvailable { get; set; }
    }
}
