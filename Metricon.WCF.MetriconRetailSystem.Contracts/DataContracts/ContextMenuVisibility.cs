using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Metricon.WCF.MetriconRetailSystem.Contracts
{
    [DataContract]
    public class ContextMenuVisibility
    {
        [DataMember]
        public bool Show_View_Menu { get; set; }

        [DataMember]
        public bool Show_Edit_Menu { get; set; }

        [DataMember]
        public bool Show_Accept_Menu { get; set; }

        [DataMember]
        public bool Show_Reject_Menu { get; set; }

        [DataMember]
        public bool Show_Edit_Comments_Menu { get; set; }

        [DataMember]
        public bool Show_Assign_To_Me_Menu { get; set; }

        [DataMember]
        public bool Show_Assign_Menu { get; set; }

        [DataMember]
        public bool Show_View_History_Menu { get; set; }

        [DataMember]
        public bool Show_Audit_Trail_Menu { get; set; }

        [DataMember]
        public bool Show_Difficulty_Rating_Menu { get; set; }

        [DataMember]
        public bool Show_DueDate_Menu { get; set; }

        [DataMember]
        public bool Show_Appointment_Time_Menu { get; set; }

        [DataMember]
        public bool Show_Price_Effective_Date_Menu { get; set; }

        [DataMember]
        public bool Show_On_Hold_Menu { get; set; }

        [DataMember]
        public bool Show_Cancel_Menu { get; set; }

        [DataMember]
        public bool Show_Activate_Menu { get; set; }

        [DataMember]
        public bool Show_Colour_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Electrical_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Paving_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Tile_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Decking_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Carpet_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Curtains_And_Blinds_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Timber_Floor_Selection_Menu { get; set; }

        [DataMember]
        public bool Show_Merge_Studio_M_Revisions_Menu { get; set; }

        [DataMember]
        public bool Show_Final_Contract_Menu { get; set; }

        [DataMember]
        public bool Show_Pre_Site_Variation_Menu { get; set; }

        [DataMember]
        public bool Show_Building_Variation_Menu { get; set; }

        [DataMember]
        public bool Show_Customer_Support_Coordinator_Menu { get; set; }

        [DataMember]
        public bool Show_Pre_Studio_M_Variation_Menu { get; set; }

        [DataMember]
        public bool Show_Ready_For_Studio_M_Menu { get; set; }

        [DataMember]
        public bool Show_Change_Facade_Menu { get; set; }

        [DataMember]
        public bool Show_Change_Contract_Type_Menu { get; set; }

    }
}
