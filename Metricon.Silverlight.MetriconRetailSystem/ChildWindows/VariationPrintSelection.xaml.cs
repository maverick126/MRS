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
    public partial class VariationPrintSelection : ChildWindow
    {
        public int _estimateRevisionId;
        string _printType;
        private RetailSystemClient _mrsClient;
        private bool _includeProductNameAndCode;
        private bool _includeUOMAndQuantity;
        private bool _includeContractPriceOnVariation;
        private bool _includeSpecifications;
        private int _estimateMRSTypeID = 5;
        private int _estimateVariationDisclaimerCurrentId;
        private int _estimateVariationDisclaimerNewId;

        public VariationPrintSelection(int revisionid, string printType, bool includeProductNameAndCode, bool includeUOMAndQuantity, bool includeContractPriceOnVariation, int estimateVariationDisclaimerCurrentId, int estimateVariationDisclaimerNewId, bool includeSpecifications)
        {
            InitializeComponent();
            _estimateRevisionId = revisionid;
            _printType = printType;
            _includeProductNameAndCode = includeProductNameAndCode;
            _includeUOMAndQuantity = includeUOMAndQuantity;
            _includeSpecifications = includeSpecifications;
            _includeContractPriceOnVariation = includeContractPriceOnVariation;

            _estimateVariationDisclaimerCurrentId = estimateVariationDisclaimerCurrentId;
            _estimateVariationDisclaimerNewId = estimateVariationDisclaimerNewId;
            if (_estimateVariationDisclaimerCurrentId == _estimateVariationDisclaimerNewId)
            {
                stackPanelUpdateDisclaimer.Visibility = Visibility.Collapsed;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            _printType = "changeonly";
            if (checkBoxUpdateNewDisclaimer.IsChecked == true)
                SaveDisclaimerUpdateDetails();
            else
                Print();
        }

        private void SaveDisclaimerUpdateDetails()
        {
            string description = "Update New Disclaimer User: " + (App.Current as App).CurrentUserFullName + ", Disclaimer Id:" + _estimateVariationDisclaimerNewId;

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            _mrsClient.SaveDisclaimerUpdateDetailsCompleted += new EventHandler<SaveDisclaimerUpdateDetailsCompletedEventArgs>(_mrsClient_SaveDisclaimerUpdateDetailsCompleted);
            _mrsClient.SaveDisclaimerUpdateDetailsAsync(_estimateRevisionId, _estimateMRSTypeID, _estimateVariationDisclaimerNewId, (App.Current as App).CurrentUserId);

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.UpdateNewDisclaimer,
                _estimateRevisionId,
                description,
                0);
        }

        void _mrsClient_SaveDisclaimerUpdateDetailsCompleted(object sender, SaveDisclaimerUpdateDetailsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                _estimateVariationDisclaimerCurrentId = _estimateVariationDisclaimerNewId;
            }
            else
            {
                MessageBox.Show("Error when updating new disclaimer to the current revision.");
            }

            Print();
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            // no operation
        }
         
        private void Print()
        {
            if (_printType == "studiom")
            {
                System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=0&type=" + _printType + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                this.DialogResult = true;
                CloseRadWindow();
            }
            else if (_printType == "changeonly")
            {
                System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintVariation.aspx?merge=0&type=" + _printType + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&IncludeProductNameAndCode=" + _includeProductNameAndCode + "&IncludeUOMAndQuantity=" + _includeUOMAndQuantity + "&includeSpecifications=" + _includeSpecifications + "&IncludeContractPriceOnVariation=" + _includeContractPriceOnVariation, UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                this.DialogResult = true;
                CloseRadWindow();
            }
            else
            {
                //_mrsClient = new RetailSystemClient();
                //_mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
                //_mrsClient.GetAreaSurchargeCompleted += new EventHandler<GetAreaSurchargeCompletedEventArgs>(_mrsClient_GetAreaSurchargeCompleted);
                //_mrsClient.GetAreaSurchargeAsync(_estimateRevisionId);
            }

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

        private void buttonViewNewDisclaimer_Click(object sender, RoutedEventArgs e)
        {
            HtmlPage.Window.Navigate(new Uri("../PrintDisclaimer.aspx?merge=1&type=changeonly" + "&Version=New" + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
        }

        private void buttonViewCurrentDisclaimer_Click(object sender, RoutedEventArgs e)
        {
            HtmlPage.Window.Navigate(new Uri("../PrintDisclaimer.aspx?merge=1&type=changeonly" + "&Version=Current" + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
        }
    }
}

