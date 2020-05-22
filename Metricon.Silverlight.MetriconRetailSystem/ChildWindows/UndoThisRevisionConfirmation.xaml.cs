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

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class UndoThisRevisionConfirmation : ChildWindow
    {
        private int _bcContractNumber;
        private int _sourceEstimateNo;
        private int _selectedEstimateRevisionId;
        private int _currentRevisionNumber;
        private string _currentRevisionTypeCode;
        private int _previousRevisionId;
        private int _ownerId;
        private int confirmStep;

        public UndoThisRevisionConfirmation(object dataContext)
        {
            InitializeComponent();

            EstimateGridItem item = (EstimateGridItem)dataContext;
            _bcContractNumber = item.ContractNumber;
            _sourceEstimateNo = item.EstimateId;
            _selectedEstimateRevisionId = item.RecordId;
            _currentRevisionNumber = item.RevisionNumber;
            _currentRevisionTypeCode = item.RevisionTypeCode;
            _ownerId = item.OwnerId;

            OKButton.Visibility = Visibility.Collapsed;
            if ((App.Current as App).SelectedStatusId != (int)EstimateRevisionStatus.WorkInProgress && (App.Current as App).SelectedStatusId != (int)EstimateRevisionStatus.Accepted && _currentRevisionTypeCode != "RSTM") // not Split Studio M
                textBlockMessage.Text = "Undo cannot be performed on this status.";
            else if (_ownerId != (App.Current as App).CurrentUserId && !(App.Current as App).IsManager)
                textBlockMessage.Text = "Undo cannot be performed on this user.";
            else
            {
                BusyIndicator1.IsBusy = true;
                OKButton.Visibility = Visibility.Visible;
                confirmStep = 1;
                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.UndoThisRevisionValidateCompleted += new EventHandler<UndoThisRevisionValidateCompletedEventArgs>(mrsClient_UndoThisRevisionValidateCompleted);
                mrsClient.UndoThisRevisionValidateAsync(item.EstimateId, _bcContractNumber, _selectedEstimateRevisionId);
            }
        }

        void mrsClient_UndoThisRevisionValidateCompleted(object sender, UndoThisRevisionValidateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result.Count > 1)
                {
                    EstimateHeader ehCurrent = e.Result[e.Result.Count - 1];
                    if (ehCurrent != null && ehCurrent.StatusName == "Completed")
                    {
                        textBlockMessage.Text = String.Format("You are about to undo this revision {0}{1}, current status {2}." + Environment.NewLine + Environment.NewLine + "Current revision {3}({4}) will be changed back to Work In Progress.", ehCurrent.RevisionNumber, ehCurrent.RevisionTypeCode, ehCurrent.StatusName, _currentRevisionNumber, _currentRevisionTypeCode);
                    }
                    else
                    {
                        EstimateHeader ehPrevious = e.Result[e.Result.Count - 2];
                        textBlockMessage.Text = String.Format("You are about to undo this revision to the previous revision {0}{1}, status {2}" + Environment.NewLine + Environment.NewLine + "Current revision {3}({4}) will be deleted and it will be assigned back to {5}.", ehPrevious.RevisionNumber, ehPrevious.RevisionTypeCode, ehPrevious.StatusName, _currentRevisionNumber, _currentRevisionTypeCode, ehPrevious.OwnerName);
                    }
                }
                else
                {
                    textBlockMessage.Text = "Milestone must be reset before undoing a revision.";
                    OKButton.Visibility = Visibility.Collapsed;
                }
                    
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "UndoThisRevisionValidateCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (confirmStep == 1)
            {
                confirmStep = 2;
                OKButton.Visibility = Visibility.Collapsed;
                UndoButton.Visibility = Visibility.Visible;
                textBlockMessage.Text = "This Action is not reversible." + Environment.NewLine;
                textBlockReason.Text = "Enter reason for Undoing.";
                txtReason.Visibility = Visibility.Visible;
            }
            else
            {
                if (txtReason.Text.Trim() != "")
                {
                    BusyIndicator1.IsBusy = true;
                    RetailSystemClient mrsClient = new RetailSystemClient();
                    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                    mrsClient.UndoThisRevisionCompleted += delegate(object o, UndoThisRevisionCompletedEventArgs es)
                    {
                        if (es.Error == null)
                        {
                            if (es.Result != null)
                            {
                                Int32.TryParse(es.Result.ToString(), out _previousRevisionId);
                                CreateLog(es.Result.ToString());
                                RadWindow window = this.ParentOfType<RadWindow>();
                                if (window != null)
                                {
                                    window.DialogResult = true;
                                    window.Close();
                                }
                            }
                        }
                        else
                            ExceptionHandler.PopUpErrorMessage(es.Error, "UndoThisRevisionCompleted");
                    };

                    mrsClient.UndoThisRevisionAsync(_bcContractNumber, _sourceEstimateNo, _selectedEstimateRevisionId, _ownerId, txtReason.Text);
                }
                else
                {
                    MessageBox.Show("Please enter a reason for undo.");
                }
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

        private void CreateLog(string result)
        {
            string newDestinationEstimateNo = "blank";
            if (!string.IsNullOrEmpty(_sourceEstimateNo.ToString()))
                newDestinationEstimateNo = _sourceEstimateNo.ToString();

            string description = "Undo this revision " + _selectedEstimateRevisionId + " to " + _previousRevisionId + " by user " + (App.Current as App).CurrentUserLoginName + Environment.NewLine + "Reason:" + Environment.NewLine + txtReason.Text + Environment.NewLine + result;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.UndoRevision,
                _previousRevisionId,
                description,
                0);
        }
    }
}

