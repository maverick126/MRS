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

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class EstimateHistory : ChildWindow
    {
        public EstimateHistory(object dataContext)
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;

            EstimateGridItem item = (EstimateGridItem)dataContext;

            txtCustomerName.Text = item.CustomerName;
            txtContractNumber.Text = item.ContractNumber.ToString();
            txtCustomerNumber.Text = item.CustomerNumber.ToString();
            txtEstimateNumber.Text = item.EstimateId.ToString();
            txtJobFlow.Text = item.JobFlowType;
            txtContractType.Text = item.ContractType;

            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetEstimatesRevisionsCompleted += new EventHandler<GetEstimatesRevisionsCompletedEventArgs>(mrsClient_GetEstimatesRevisionsCompleted);
            mrsClient.GetEstimatesRevisionsAsync(item.EstimateId);
        }

        void mrsClient_GetEstimatesRevisionsCompleted(object sender, GetEstimatesRevisionsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                History.ItemsSource = e.Result;


                if (!((App)App.Current).CurrentRoleAccessModule.AccessMarginModule)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        GridViewColumn item = History.ColumnFromDisplayIndex(7);
                        History.Columns.Remove(item);
                    }
 
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimatesRevisionsCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        //private void RadContextMenu_ItemClick(object sender, RadRoutedEventArgs e)
        //{
        //    RadContextMenu menu = (RadContextMenu)sender;
        //    RadMenuItem clickedItem = e.OriginalSource as RadMenuItem;
        //    GridViewRow row = menu.GetClickedElement<GridViewRow>();
            
        //    if (clickedItem != null && row != null)
        //    {
        //        EstimateHeader estimate = (EstimateHeader)row.DataContext;

        //        string header = Convert.ToString(clickedItem.Header);
        //        int selectedEstimateRevisionId = estimate.RecordId;
        //        RadWindow win = new RadWindow();
        //        switch (header)
        //        {
        //            case "Print Preview":
        //                PrintPreview previewDlg = new PrintPreview(selectedEstimateRevisionId);
        //                win.Header = "Print Preview";
        //                win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
        //                win.Content = previewDlg;
        //                win.ShowDialog();
        //                break;

        //            default:
        //                break;
        //        }
        //    }
        //}

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            HyperlinkButton btn = (HyperlinkButton)sender;
            GridViewRow row = btn.GetVisualParent<GridViewRow>();

            if (row != null)
            {
                EstimateHeader estimate = (EstimateHeader)row.DataContext;
                int selectedEstimateRevisionId = estimate.RecordId;
                RadWindow win = new RadWindow();
                PrintPreview previewDlg = new PrintPreview(selectedEstimateRevisionId, estimate.RevisionTypeId, (estimate.RevisionTypeCode.ToLower().Contains("pc") || estimate.RevisionTypeCode.ToLower().Contains("contract") || estimate.RevisionTypeCode.ToLower().Contains("variation")) ? true : false);
                win.Header = "Print Preview";
                win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                win.Content = previewDlg;
                win.ShowDialog();
            }
        }

    }
}

