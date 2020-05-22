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
using System.Collections.ObjectModel;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class CopyEstimate : ChildWindow
    {
        public int estimaterevisionid = 0;
        private int _destinationEstimateNo;
        private int _sourceEstimateNo = 0;
        private RetailSystemClient _mrsClient;
        private ObservableCollection<ValidationErrorMessage> _message = new ObservableCollection<ValidationErrorMessage>();

        public CopyEstimate(int revisionid, int destinationEstimateNo)
        {
            estimaterevisionid = revisionid;
            _destinationEstimateNo = destinationEstimateNo;
            InitializeComponent();
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            txtSourceEstimateNumber.Value = null;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "Copying Estimate ...";

            Int32.TryParse(txtSourceEstimateNumber.Text.Trim(), out _sourceEstimateNo);

            if (_sourceEstimateNo != _destinationEstimateNo)
            {
                //if (_sourceEstimateNo == 0)
                  //  _sourceEstimateNo = null;

                _mrsClient.CopyEstimateCheckDifferenceCompleted += new EventHandler<CopyEstimateCheckDifferenceCompletedEventArgs>(_mrsClient_CopyEstimateCheckDifferenceCompleted);
                _mrsClient.CopyEstimateCheckDifferenceAsync(_sourceEstimateNo.ToString(), _destinationEstimateNo.ToString());
            }
            else
                CancelButton_Click(sender, null);
        }

        //public void PopulateDestinationEstimateNo()
        //{
        //    BusyIndicator1.IsBusy = true;

        //    _mrsClient.GetDestinationEstimateNoCompleted += new EventHandler<GetDestinationEstimateNoCompletedEventArgs>(_mrsClient_GetDestinationEstimateNoCompleted);
        //    _mrsClient.GetDestinationEstimateNoAsync(_estimateRevisionId);
        //}

        //void _mrsClient_GetDestinationEstimateNoCompleted(object sender, GetDestinationEstimateNoCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        _DestinationEstimateNo = e.Result;
        //        txtDestinationEstimateNo.Text = _DestinationEstimateNo;
        //    }
        //    else
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "mrsClient_GetContractTypeCompleted");
        //    BusyIndicator1.IsBusy = false;
        //}

        void _mrsClient_CopyEstimateCheckDifferenceCompleted(object sender, CopyEstimateCheckDifferenceCompletedEventArgs e)
        {
            List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
            if (e != null)
            {
                if (e.Error == null)
                {
                    if (e.Result != null)
                    {
                        if (e.Result.Count > 0)
                        {
                            foreach (var p in e.Result)
                            {
                                result.Add(p);
                            }

                            RadWindow win = new RadWindow();
                            ShowValidationMessage2 messageDlg = new ShowValidationMessage2(result, true, estimaterevisionid, _sourceEstimateNo, _destinationEstimateNo); //estimaterevisionid);
                            win.Header = "The following items cannot be copied as it does not exist on the original estimate.";
                            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                            win.Content = messageDlg;
                            win.Closed += new EventHandler<WindowClosedEventArgs>(ValidationWin_Closed);
                            win.ShowDialog();
                        }
                        else
                        {
                            // if no difference then copy all of them
                            _mrsClient.CopyEstimateCompleted += new EventHandler<CopyEstimateCompletedEventArgs>(_mrsClient_CopyEstimateCompleted);
                            _mrsClient.CopyEstimateAsync(_sourceEstimateNo.ToString(), _destinationEstimateNo.ToString());
                        }
                    }
                    else
                    {
                        // if no difference then copy all of them
                        _mrsClient.CopyEstimateCompleted += new EventHandler<CopyEstimateCompletedEventArgs>(_mrsClient_CopyEstimateCompleted);
                        _mrsClient.CopyEstimateAsync(_sourceEstimateNo.ToString(), _destinationEstimateNo.ToString());
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(e.Error, "CopyEstimateCompleted");
            }
        }

        void _mrsClient_CopyEstimateCompleted(object sender, CopyEstimateCompletedEventArgs e)
        {
            List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
            if (e != null)
            {
                if (e.Error == null)
                {
                    if (e.Result == true)
                    {
                        CreateLog();
                        ValidationWin_Closed(sender, null);
                    }
                    else
                    {
                    }
                }
            }
        }

        private void CreateLog()
        {
            string newDestinationEstimateNo = "blank";
            if (!string.IsNullOrEmpty(_sourceEstimateNo.ToString()))
                newDestinationEstimateNo = _sourceEstimateNo.ToString();

            string description = "Estimate has been copied to " + newDestinationEstimateNo + " by user " + (App.Current as App).CurrentUserLoginName;

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.CopyEstimate,
                estimaterevisionid,
                description,
                0);
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            ValidationWin_Closed(sender, null);
        }

        void ValidationWin_Closed(object sender, WindowClosedEventArgs e)
        {
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

    }
}

