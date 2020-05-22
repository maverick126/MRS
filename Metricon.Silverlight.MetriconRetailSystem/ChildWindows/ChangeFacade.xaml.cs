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
    public partial class ChangeFacade : ChildWindow
    {
        private int estimaterevisionid;
        private int selectedfacadehomeid;
        private string selectedfacadehomename;
        ChangeFacadeViewModel cvm;
        private ObservableCollection<ValidationErrorMessage> _message = new ObservableCollection<ValidationErrorMessage>();
        public ChangeFacade(int revisionid)
        {
            estimaterevisionid = revisionid;
            cvm = new ChangeFacadeViewModel(revisionid);
            InitializeComponent();
            this.DataContext = cvm;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string detailIDsSelected = string.Empty;
            string detailOptionsSeleced = string.Empty;
            string detailPricesSeleced = string.Empty;

            if (errorGrid.ItemsSource != null)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Changing Facade...";

                foreach (ValidationErrorMessage item in (ObservableCollection<ValidationErrorMessage>)errorGrid.ItemsSource)
                {
                    if (item.Reason=="0" && !item.CopyAsNSR)
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

            MRSclient.ChangeFacadeCompleted += new EventHandler<ChangeFacadeCompletedEventArgs>(mrsClient_ChangeFacadeCompleted);
            MRSclient.ChangeFacadeAsync(estimaterevisionid, selectedfacadehomeid, detailIDsSelected, detailOptionsSeleced, detailPricesSeleced, DateTime.Now.ToString("dd/MMM/yyyy"), (App.Current as App).CurrentUserId);
        }

        void mrsClient_ChangeFacadeCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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
                ExceptionHandler.PopUpErrorMessage(e.Error, "ChangeFacadeCompleted");
            }
        }

        private void CreateLog()
        {

            string description = "Facade has been changed to " + selectedfacadehomename + " by user " + (App.Current as App).CurrentUserFullName;
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            MRSclient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            MRSclient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.ChangeFacade,
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
            CheckFacadeConfigurationDifference(estimaterevisionid, selectedfacadehomeid, DateTime.Now.ToString("dd/MMM/yyyy"));

            homenamegrid.Visibility = Visibility.Collapsed;
            WarningGrid.Visibility = Visibility.Visible;

        }

        public void CheckFacadeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            if (_message.Count == 0)
            {
                RetailSystemClient MRSclient = new RetailSystemClient();
                MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                MRSclient.CheckFacadeConfigurationDifferenceCompleted += new EventHandler<CheckFacadeConfigurationDifferenceCompletedEventArgs>(mrsClient_CheckFacadeConfigurationDifferenceCompleted);
                MRSclient.CheckFacadeConfigurationDifferenceAsync(revisionid, newfacadehomeid, effectivedate);
            }
            else
            {
                mrsClient_CheckFacadeConfigurationDifferenceCompleted(null, null);
            }
        }

        void mrsClient_CheckFacadeConfigurationDifferenceCompleted(object sender, CheckFacadeConfigurationDifferenceCompletedEventArgs e)
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
                    ExceptionHandler.PopUpErrorMessage(e.Error, "mrsClient_CheckFacadeConfigurationDifferenceCompleted");
            }
        }
    }
}

