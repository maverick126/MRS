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
using System.Windows.Browser;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class CustomerPrintSelection : ChildWindow
    {
        public int _estimateRevisionId;
        private bool _includeProductNameAndCode;
        private bool _includeUOMAndQuantity;
        private bool _includeSpecifications;
        private int _estimateDisclaimerCurrentId;
        private int _estimateDisclaimerNewId;
        private RetailSystemClient _mrsClient;

        public CustomerPrintSelection(int revisionid, bool includeProductNameAndCode, bool includeUOMAndQuantity, int estimateDisclaimerCurrentId, int estimateDisclaimerNewId, bool includeSpecifications)
        {
            InitializeComponent();
            _estimateRevisionId = revisionid;
            _includeProductNameAndCode = includeProductNameAndCode;
            _includeUOMAndQuantity = includeUOMAndQuantity;
            _includeSpecifications = includeSpecifications;

            _estimateDisclaimerCurrentId = estimateDisclaimerCurrentId;
            _estimateDisclaimerNewId = estimateDisclaimerNewId;
            if (_estimateDisclaimerCurrentId == _estimateDisclaimerNewId)
            {
                stackPanelUpdateDisclaimer.Visibility = Visibility.Collapsed;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxUpdateNewDisclaimer.IsChecked == true)
                SaveDisclaimerUpdateDetails();
            else
                Print();
        }

        private void SaveDisclaimerUpdateDetails()
        {
            string description = "Update New Disclaimer User: " + (App.Current as App).CurrentUserFullName + ", Disclaimer Id:" + _estimateDisclaimerNewId;

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            _mrsClient.SaveDisclaimerUpdateDetailsCompleted += new EventHandler<SaveDisclaimerUpdateDetailsCompletedEventArgs>(_mrsClient_SaveDisclaimerUpdateDetailsCompleted);
            _mrsClient.SaveDisclaimerUpdateDetailsAsync(_estimateRevisionId, 0, _estimateDisclaimerNewId, (App.Current as App).CurrentUserId);

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.UpdateNewDisclaimer,
                _estimateRevisionId,
                description,
                0);
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            // no operation
        }

        void _mrsClient_SaveDisclaimerUpdateDetailsCompleted(object sender, SaveDisclaimerUpdateDetailsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                _estimateDisclaimerCurrentId = _estimateDisclaimerNewId;
            }
            else
            {
                MessageBox.Show("Error when updating new disclaimer to the current revision.");
            }
            Print();
        }

        private void Print()
        {
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            _mrsClient.GetAreaSurchargeCompleted += new EventHandler<GetAreaSurchargeCompletedEventArgs>(_mrsClient_GetAreaSurchargeCompleted);
            _mrsClient.GetAreaSurchargeAsync(_estimateRevisionId);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            CloseRadWindow();
        }
        private void CloseRadWindow()
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        void _mrsClient_GetAreaSurchargeCompleted(object sender, GetAreaSurchargeCompletedEventArgs e)
        {
            string printversion = "FULLDETAILS";
            string includestd = "0";
            if ((bool)chkIncludeSTD.IsChecked)
            {
                includestd = "1";
            }

            if ((bool)rdlump.IsChecked)
            {
                printversion = "lUMPSUM";
            }
            else if ((bool)rdsummary.IsChecked)
            {
                printversion = "FULLSUMMARY";
            }
            else
            {
                printversion = "FULLTOTAL";
            }
            if (e.Error == null)
            {
                if (e.Result[0] == 1)
                {
                    MessageBoxResult confirm = MessageBox.Show("There is a " + e.Result[1].ToString("c") + " surcharge, would you like to merge it into home price?\r\nClick OK to merge or otherwise click Cancel.", "", MessageBoxButton.OKCancel);
                    if (confirm == MessageBoxResult.OK)
                    {
                        System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=1&type=customer&version=" + printversion + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&IncludeProductNameAndCode=" + _includeProductNameAndCode + "&IncludeUOMAndQuantity=" + _includeUOMAndQuantity + "&includeSpecifications=" + _includeSpecifications + "&includestd=" + includestd, UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                    }
                    else
                    {
                        System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=0&type=customer&version=" + printversion + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&IncludeProductNameAndCode=" + _includeProductNameAndCode + "&IncludeUOMAndQuantity=" + _includeUOMAndQuantity + "&includeSpecifications=" + _includeSpecifications + "&includestd=" + includestd, UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                    }
                }
                else
                {
                    System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=0&type=customer&version=" + printversion + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&IncludeProductNameAndCode=" + _includeProductNameAndCode + "&IncludeUOMAndQuantity=" + _includeUOMAndQuantity + "&includeSpecifications=" + _includeSpecifications + "&includestd=" + includestd, UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                }

                this.DialogResult = true;
                CloseRadWindow();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAreaSurchargeCompleted");
        }

        private void buttonViewNewDisclaimer_Click(object sender, RoutedEventArgs e)
        {
            HtmlPage.Window.Navigate(new Uri("../PrintDisclaimer.aspx?merge=1&type=customer" + "&Version=New" + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
        }

        private void buttonViewCurrentDisclaimer_Click(object sender, RoutedEventArgs e)
        {
            HtmlPage.Window.Navigate(new Uri("../PrintDisclaimer.aspx?merge=1&type=customer" + "&Version=Current" + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");

        }

    }
}

