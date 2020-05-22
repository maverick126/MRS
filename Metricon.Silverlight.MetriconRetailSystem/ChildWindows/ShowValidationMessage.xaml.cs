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
    public partial class ShowValidationMessage : ChildWindow
    {

        public RetailSystemClient mrsClient;
        public int estimaterevisionid = 0;
        public ShowValidationMessage(List<ValidationErrorMessage> errorMessage, bool show, int revisionid)
        {
            estimaterevisionid = revisionid;
            InitializeComponent();
            errorGrid.ItemsSource = errorMessage;
            if (show)
            {
                ContinueButton.Visibility = Visibility.Visible;
            }
            else
            {
                ContinueButton.Visibility = Visibility.Collapsed;
            }
            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {   
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        private void errorGrid_RowLoaded(object sender, Telerik.Windows.Controls.GridView.RowLoadedEventArgs e)
        {
            GridViewRow row = e.Row as GridViewRow;
            if (row != null)
            {
                ValidationErrorMessage vms = row.DataContext as ValidationErrorMessage;
                if (vms.AllowGoAhead)
                    row.Background = new SolidColorBrush(Color.FromArgb(25, 204, 255, 153));// SolidColorBrush(Colors.Transparent);
                else
                    row.Background = new SolidColorBrush(Color.FromArgb(25, 255, 153, 204));
            }

        }

        private void HyperlinkButton_AddClick(object sender, RoutedEventArgs e)
        {
            Grid gr=(Grid)((HyperlinkButton)e.OriginalSource).Parent;
            ValidationErrorMessage er = ((GridViewCell)(gr.Parent)).ParentRow.DataContext as ValidationErrorMessage;

            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.SaveSelectedItemCompleted += delegate(object o, SaveSelectedItemCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if (es.Result > 0)
                    {
                        RefreshErrorMessage(estimaterevisionid); 
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SaveSelectedItemCompleted");
            };

            MRSclient.SaveSelectedItemAsync(er.HomeDisplayOptionId, estimaterevisionid, er.PagID, (App.Current as App).CurrentUserId);
            
        }

        private void HyperlinkButton_UpgradeClick(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            //EstimateDetails pag = new EstimateDetails();
            Grid gr=(Grid)((HyperlinkButton)e.OriginalSource).Parent;
            ValidationErrorMessage er = ((GridViewCell)(gr.Parent)).ParentRow.DataContext as ValidationErrorMessage;
            string productid = (er.ErrorMessage.Replace("Standard Inclusion - ", "")).Replace(" should be added to estimate.", "");

            AddUpgradeFromStandardInclusion acceptDlg = new AddUpgradeFromStandardInclusion(estimaterevisionid, er.HomeDisplayOptionId, productid);
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Add Upgrade Window";
            win.Content = acceptDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_AddOptionClosed);
            win.ShowDialog();
        }

        private void HyperlinkButton_AnswerClick(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            EstimateDetails pag = new EstimateDetails();
            Grid gr=(Grid)((HyperlinkButton)e.OriginalSource).Parent;
            ValidationErrorMessage er = ((GridViewCell)(gr.Parent)).ParentRow.DataContext as ValidationErrorMessage;

            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.GetPagByIDCompleted += delegate(object o, GetPagByIDCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    pag = es.Result;
                    AppOptionFromTree acceptDlg = new AppOptionFromTree(pag, "STUDIOM_ANSWER", 0);
                    win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                    win.Header = "Add Option Window";
                    win.Content = acceptDlg;
                    win.Closed += new EventHandler<WindowClosedEventArgs>(win_AddOptionClosed);
                    win.ShowDialog();
                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "GetPagByIDCompleted");
                }
            };
            mrsClient.GetPagByIDAsync(estimaterevisionid, er.HomeDisplayOptionId);

        }

        void win_AddOptionClosed(object sender, WindowClosedEventArgs e)
        {

            RadWindow dlg = (RadWindow)sender;
            ParameterClass p = (ParameterClass)dlg.DataContext;
            if (p != null)
            {
                EstimateDetails pag = p.SelectedPAG;

                bool? result = dlg.DialogResult;
                if (result.HasValue && result.Value)
                {
                    if (p.SelectedItemID != "" || p.SelectedStandardInclusionID != "")
                    {
                        SaveData(p, p.Action);
                    }
                }
            }
        }

        public void SaveData(ParameterClass p, string action)
        {
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            MRSclient.SaveSelectedItemsFromOptionTreeToEstimateCompleted += delegate(object o, SaveSelectedItemsFromOptionTreeToEstimateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    RefreshErrorMessage(estimaterevisionid);
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "SaveSelectedItemsFromOptionTreeToEstimateCompleted");
            };
            MRSclient.SaveSelectedItemsFromOptionTreeToEstimateAsync(p.SelectedItemID, 
                p.SelectedStandardInclusionID, 
                estimaterevisionid.ToString(), 
                p.StudioMQANDA, 
                (App.Current as App).CurrentUserId.ToString(), 
                action,
                p.SelectedDerivedCosts,
                p.SelectedBTPCostExcGSTs,
                p.SelectedBTPCostOverwriteFlags,
                p.SelectedDBCCostExcGSTs,
                p.SelectedDBCCostOverwriteFlags,
                p.SelectedQuantities,
                p.SelectedPrices,
                p.SelectedIsAccepteds,
                p.SelectedAreaIds,
                p.SelectedGroupIds,
                p.SelectedPriceDisplayCodeIds,
                p.SelectedIsSiteWorks,
                p.SelectedProductDescriptions,
                p.SelectedAdditionalNotes,
                p.SelectedExtraDescriptions,
                p.SelectedInternalDescriptions);
        }

        public void RefreshErrorMessage(int revisionid)
        {
            List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
            bool show = true;
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.ValidateStudioMEstimateCompleted += delegate(object o, ValidateStudioMEstimateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    foreach (ValidationErrorMessage s in es.Result)
                    {

                        if (show && !s.AllowGoAhead)
                        {
                            show = false;
                        }
                        result.Add(s);
                    }
                    errorGrid.ItemsSource = result;

                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "ValidateStudioMEstimateCompleted");
                }
            };

            mrsClient.ValidateStudioMEstimateAsync(revisionid);
        }
    }
}

