using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

using Telerik.Windows;
using Telerik.Windows.Controls.GridView;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class AddUpgradeFromStandardInclusion : ChildWindow
    {
        public RetailSystemClient mrsClient;
        public int estimaterevisionid = 0;
        public int originateoptionid = 0;
        public string productid = "";
        public List<SimplePAG> upgradelist;
        public ParameterClass p = new ParameterClass();
        public AddUpgradeFromStandardInclusion(int revisionid, int porginateoptionid, string pproductid)
        {
            estimaterevisionid = revisionid;
            originateoptionid = porginateoptionid;
            productid = pproductid;
            
            InitializeComponent();
            upgradelist = new List<SimplePAG>();
            GetUpgradeOptionList();
            txttitle.Text =   "("+productid+")";
        }

        private void GetUpgradeOptionList()
        {
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.GetUpgradeOptionListForStandardInclusionCompleted += delegate(object o, GetUpgradeOptionListForStandardInclusionCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if (es.Result.Count > 0)
                    {
                        upgradelist = es.Result.ToList();
                        PAGGrid.ItemsSource = upgradelist;
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SaveSelectedItemCompleted");
            };

            MRSclient.GetUpgradeOptionListForStandardInclusionAsync(estimaterevisionid, originateoptionid);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();

            string selecteditemids = "";
            string selectedstandardinclusionids = "";
            foreach (SimplePAG item in upgradelist)
            {
                if (item.Selected) // here reuse this column to hold the selection of check box
                {
                    selecteditemids = item.HomeDisplayOptionID.ToString();
                }
            }
            p.SelectedItemID = selecteditemids;
            p.SelectedStandardInclusionID = selectedstandardinclusionids;
            p.StudioMQANDA = "";
            p.Action = "OPTIONTREE";

            if (window != null)
            {
                window.DataContext = p;
                window.DialogResult = true;
                window.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DataContext = p;
                window.DialogResult = false;
                window.Close();
            }
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            SimplePAG pag = ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.DataContext as SimplePAG;
            foreach (SimplePAG p in upgradelist)
            {
                if (p.HomeDisplayOptionID != pag.HomeDisplayOptionID)
                {
                    p.Selected = false;
                }
            }
        }
    }
}

