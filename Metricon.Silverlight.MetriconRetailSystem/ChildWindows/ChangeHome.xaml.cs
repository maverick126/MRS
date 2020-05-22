using System;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Messaging;

using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

using Telerik.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class ChangeHome : ChildWindow
    {
        private int estimaterevisionid;
        private int estimatestateid;
        private int selectedfacadehomeid;
        private string selectedfacadehomename;
        ChangeHomeViewModel cvm;
        private ObservableCollection<ValidationErrorMessage> _message = new ObservableCollection<ValidationErrorMessage>();
        public ChangeHome(int estimaterevid, int state)
        {
            estimaterevisionid = estimaterevid;
            estimatestateid = state;
            cvm = new ChangeHomeViewModel(state);
            InitializeComponent();
            this.DataContext = cvm;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string detailIDsSelected = string.Empty;
            string detailOptionsSeleced = string.Empty;
            string detailPricesSeleced = string.Empty;

            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "Changing Home...";

            if (errorGrid.ItemsSource != null)
            { 
                foreach (ValidationErrorMessage item in (ObservableCollection<ValidationErrorMessage>)errorGrid.ItemsSource)
                {
                    if (item.Reason == "0" && !item.CopyAsNSR)
                        continue;
                    detailIDsSelected += item.PagID + ",";
                    detailOptionsSeleced += (item.CopyAsNSR ? "1" : item.QuantityUseCurrent ? "2" : item.QuantityUseNew ? "3" : item.PriceUseCurrent ? "4" : item.PriceUseNew ? "5" : "0") + ",";
                    detailPricesSeleced += item.SellPrice + ",";
                }
                if (detailIDsSelected.Length > 0)
                    detailIDsSelected = detailIDsSelected.Substring(0, detailIDsSelected.Length - 1);
                if (detailOptionsSeleced.Length > 0)
                    detailOptionsSeleced = detailOptionsSeleced.Substring(0, detailOptionsSeleced.Length - 1);
                if (detailPricesSeleced.Length > 0)
                    detailPricesSeleced = detailPricesSeleced.Substring(0, detailPricesSeleced.Length - 1);
            }
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.ChangeHomeCompleted += new EventHandler<ChangeHomeCompletedEventArgs>(mrsClient_ChangeHomeCompleted);
            MRSclient.ChangeHomeAsync(estimaterevisionid, selectedfacadehomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, DateTime.Now.ToString("dd/MMM/yyyy"), (App.Current as App).CurrentUserId);
        }

        void mrsClient_ChangeHomeCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                CreateLog();
                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
            {
                BusyIndicator1.IsBusy = false;
                ExceptionHandler.PopUpErrorMessage(e.Error, "ChangeHomeCompleted");
            }
        }

        private void CreateLog()
        {

            string description = "Home has been changed to " + selectedfacadehomename + " by user " + (App.Current as App).CurrentUserFullName;
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            MRSclient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.ChangeHome,
                estimaterevisionid,
                description,
                0);
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
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

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            SQSHome h= ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as SQSHome;
            selectedfacadehomeid = h.HomeID;
            selectedfacadehomename = h.HomeName;
            CheckHomeConfigurationDifference(estimaterevisionid, selectedfacadehomeid, DateTime.Now.ToString("dd/MMM/yyyy"));

            homenamegrid.Visibility = Visibility.Collapsed;
            WarningGrid.Visibility = Visibility.Visible;

        }

        public void CheckHomeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            if (_message.Count == 0)
            {
                RetailSystemClient MRSclient = new RetailSystemClient();
                MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                MRSclient.CheckHomeConfigurationDifferenceCompleted += new EventHandler<CheckHomeConfigurationDifferenceCompletedEventArgs>(mrsClient_CheckHomeConfigurationDifferenceCompleted);
                MRSclient.CheckHomeConfigurationDifferenceAsync(revisionid, newfacadehomeid, effectivedate);
            }
            else
            {
                mrsClient_CheckHomeConfigurationDifferenceCompleted(null, null);
            }
        }

        void mrsClient_CheckHomeConfigurationDifferenceCompleted(object sender, CheckHomeConfigurationDifferenceCompletedEventArgs e)
        {
            if (e != null)
            {
                if (e.Error == null)
                {
                    foreach (var p in e.Result)
                    {
                        _message.Add(p);
                    }
                    OKButton.IsEnabled = true;
                    if (_message.Count > 0)
                    {
                        errorGrid.ItemsSource = _message;
                    }
                    else
                    {
                        OKButton_Click(null, null);
                    }

                }
                else
                    ExceptionHandler.PopUpErrorMessage(e.Error, "mrsClient_CheckHomeConfigurationDifferenceCompleted");
            }
        }

        private void buttonSearchHomes_Click(object sender, RoutedEventArgs e)
        {
            cvm.GetHomeList(textBoxSearchHome.Text);
        }

        private void buttonClearFilter_Click(object sender, RoutedEventArgs e)
        {
            textBoxSearchHome.Text = "";
            textBoxSearchHome.Focus();
            cvm.HomeList.Clear();
            cvm.SearchResultCount = 0;
            cvm.SearchResultVisibility = Visibility.Collapsed;
        }

    }
}

