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
using Telerik.Windows.Controls;
using System.Windows.Browser;
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class PrintPreview : ChildWindow
    {
        private int _estimateRevisionId,_revisiontypeid;
        private string _printType;
        private int _estimateDisclaimerCurrentId, _estimateDisclaimerNewId, _estimateVariationDisclaimerCurrentId, _estimateVariationDisclaimerNewId;
        private RetailSystemClient _mrsClient;

        public PrintPreview(int estimateRevisionId, int revisiontypeid, bool isMilestoneRevisionSelected)
        {
            InitializeComponent();

            _estimateRevisionId = estimateRevisionId;
            _revisiontypeid = revisiontypeid;

            if (int.Parse((App.Current as App).CurrentUserStateID) == 3)
            {
                checkBoxDoNotIncludeUOMAndQuantity.IsChecked = true;
                checkBoxDoNotIncludeUOMAndQuantity.IsHitTestVisible = false;
                checkBoxDoNotIncludeUOMAndQuantity.IsTabStop = false;

                checkBoxDoNotIncludeContractPriceOnVariation.IsChecked = false;
                checkBoxDoNotIncludeContractPriceOnVariation.IsHitTestVisible = false;
                checkBoxDoNotIncludeContractPriceOnVariation.IsTabStop = false;
            }

            if (isMilestoneRevisionSelected)
            {
                checkBoxIncludeSpecifications.IsHitTestVisible = false;
                checkBoxIncludeSpecifications.IsTabStop = false;
            }

            //if ((App.Current as App).SelectedEstimateRevisionTypeID >= 2) // changed to open for everyone from STS 
            //{
            //    ChangeOnlyButton.Visibility = Visibility.Visible;
            //    if (((App.Current as App).SelectedEstimateRevisionTypeID >= 7 && (App.Current as App).SelectedEstimateRevisionTypeID<=12) ||
            //        ((App.Current as App).SelectedEstimateRevisionTypeID == 21 || (App.Current as App).SelectedEstimateRevisionTypeID==22) ||
            //        (App.Current as App).SelectedEstimateRevisionTypeID == 16 || (App.Current as App).SelectedEstimateRevisionTypeID == 26 
            //        ) // for all studiom revisions and pvar-color, bvar color 
            //    {
            //        TextBlock tb = (TextBlock)ChangeOnlyButton.FindName("lblprint");
            //        tb.Text = "Today's Change";
            //    }
            //}
            //else
            //{
            //    ChangeOnlyButton.Visibility = Visibility.Collapsed;
            //}
            if (_revisiontypeid >= 2) // changed to open for everyone from STS 
            {
                ChangeOnlyButton.Visibility = Visibility.Visible;
                if ((_revisiontypeid >= 7 && _revisiontypeid <= 12) ||
                    (_revisiontypeid == 21 || _revisiontypeid == 22) ||
                    _revisiontypeid == 16 || _revisiontypeid == 26 || 
                    _revisiontypeid == 28 || _revisiontypeid == 29 
                    ) // for all studiom revisions and pvar-color, bvar color 
                {
                    TextBlock tb = (TextBlock)ChangeOnlyButton.FindName("lblprint");
                    tb.Text = "Today's Change";
                }
            }
            else
            {
                ChangeOnlyButton.Visibility = Visibility.Collapsed;
            }
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            _mrsClient.GetEstimateDisclaimerUpdateDetailsCompleted += new EventHandler<GetEstimateDisclaimerUpdateDetailsCompletedEventArgs>(_mrsClient_GetEstimateDisclaimerUpdateDetailsCompleted);
            _mrsClient.GetEstimateDisclaimerUpdateDetailsAsync(_estimateRevisionId);
        }

        private void ExternalPrintButton_Click(object sender, RoutedEventArgs e)
        {
            //_printType = "customer";
            //Print();
            RadWindow win = new RadWindow();
            CustomerPrintSelection previewDlg = new CustomerPrintSelection(_estimateRevisionId, checkBoxDoNotIncludeProductNameAndCode.IsChecked==false, checkBoxDoNotIncludeUOMAndQuantity.IsChecked==false, _estimateDisclaimerCurrentId, _estimateDisclaimerNewId, checkBoxIncludeSpecifications.IsChecked ?? true);
            win.Header = "Select Customer Version";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = previewDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.ShowDialog();
        }

        private void InternalPrintButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            InternalPrintSelection previewDlg = new InternalPrintSelection(_estimateRevisionId, "internal", checkBoxDoNotIncludeProductNameAndCode.IsChecked == false, checkBoxDoNotIncludeUOMAndQuantity.IsChecked == false, _estimateDisclaimerCurrentId, _estimateDisclaimerNewId, checkBoxIncludeSpecifications.IsChecked ?? true);
            win.Header = "Select Print Version";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = previewDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.ShowDialog();
        }

        private void StudioMPrintButton_Click(object sender, RoutedEventArgs e)
        {
            _printType = "studiom";
            Print();
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

        private void ChangeOnlyButton_Click(object sender, RoutedEventArgs e)
        {
            _printType = "changeonly";
            Print();
        }

        private void Print()
        {
            if (_estimateVariationDisclaimerCurrentId != _estimateVariationDisclaimerNewId)
            {
                RadWindow win = new RadWindow();
                VariationPrintSelection previewDlg = new VariationPrintSelection(_estimateRevisionId, "Variation Print Disclaimer Update Selection", checkBoxDoNotIncludeProductNameAndCode.IsChecked == false, checkBoxDoNotIncludeUOMAndQuantity.IsChecked == false, checkBoxDoNotIncludeContractPriceOnVariation.IsChecked==false, _estimateVariationDisclaimerCurrentId, _estimateVariationDisclaimerNewId, checkBoxIncludeSpecifications.IsChecked ?? true);
                win.Header = "Variation Print Disclaimer Update Selection";
                win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                win.Content = previewDlg;
                win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
                win.ShowDialog();
            }
            else
            {
                if (_printType == "studiom")
                {
                    System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=0&type=" + _printType + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                    this.DialogResult = true;
                    CloseRadWindow();
                }
                else if (_printType == "changeonly")
                {
                    System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintVariation.aspx?merge=0&type=" + _printType + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&IncludeProductNameAndCode=" + !checkBoxDoNotIncludeProductNameAndCode.IsChecked + "&IncludeUOMAndQuantity=" + !checkBoxDoNotIncludeUOMAndQuantity.IsChecked + "&IncludeContractPriceOnVariation=" + !checkBoxDoNotIncludeContractPriceOnVariation.IsChecked + "&includeSpecifications=" + checkBoxIncludeSpecifications.IsChecked, UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                    this.DialogResult = true;
                    CloseRadWindow();
                }
                else
                {
                    _mrsClient = new RetailSystemClient();
                    _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
                    _mrsClient.GetAreaSurchargeCompleted += new EventHandler<GetAreaSurchargeCompletedEventArgs>(_mrsClient_GetAreaSurchargeCompleted);
                    _mrsClient.GetAreaSurchargeAsync(_estimateRevisionId);
                }
            }
        }

        void _mrsClient_GetAreaSurchargeCompleted(object sender, GetAreaSurchargeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result[0] == 1)
                {
                    MessageBoxResult confirm = MessageBox.Show("There is a " + e.Result[1].ToString("c") + " surcharge, would you like to merge it into home price?\r\nClick OK to merge or otherwise click Cancel.", "", MessageBoxButton.OKCancel);
                    if (confirm == MessageBoxResult.OK)
                    {
                        System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=1&type=" + _printType + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                    }
                    else
                    {
                        System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=0&type=" + _printType + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                    }
                }
                else
                {
                    System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintLoading.aspx?merge=0&type=" + _printType + "&EstimateRevisionId=" + _estimateRevisionId.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                }

                this.DialogResult = true;
                CloseRadWindow();              
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAreaSurchargeCompleted");
        }

        void win_Closed(object sender, WindowClosedEventArgs e)
        {
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            _mrsClient.GetEstimateDisclaimerUpdateDetailsCompleted += new EventHandler<GetEstimateDisclaimerUpdateDetailsCompletedEventArgs>(_mrsClient_GetEstimateDisclaimerUpdateDetailsCompleted);
            _mrsClient.GetEstimateDisclaimerUpdateDetailsAsync(_estimateRevisionId);
        }

        void _mrsClient_GetEstimateDisclaimerUpdateDetailsCompleted(object sender, GetEstimateDisclaimerUpdateDetailsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    _estimateDisclaimerCurrentId = e.Result.DisclaimerCurrentId;
                    _estimateDisclaimerNewId = e.Result.DisclaimerNewId;
                    _estimateVariationDisclaimerCurrentId = e.Result.DisclaimerVariationCurrentId;
                    _estimateVariationDisclaimerNewId = e.Result.DisclaimerVariationNewId;
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateDisclaimerUpdateDetailsCompleted");
        }

    }
}

