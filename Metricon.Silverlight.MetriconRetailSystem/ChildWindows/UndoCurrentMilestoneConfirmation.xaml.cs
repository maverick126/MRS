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
    public partial class UndoCurrentMilestoneConfirmation : ChildWindow
    {
        private int _bcContractNumber;
        private int _sourceEstimateNo;
        private int _selectedEstimateRevisionId;
        private int _currentRevisionNumber;
        private string _currentRevisionTypeCode;
        private int confirmStep;
        public UndoCurrentMilestoneConfirmation(object dataContext)
        {
            InitializeComponent();

            EstimateGridItem item = (EstimateGridItem)dataContext;
            _bcContractNumber = item.ContractNumber;
            _sourceEstimateNo = item.EstimateId;
            _selectedEstimateRevisionId = item.RecordId;
            _currentRevisionNumber = item.RevisionNumber;
            _currentRevisionTypeCode = item.RevisionTypeCode;

             confirmStep = 1;
             textBlockMessage.Text = "This action will reset current milestone revision from this job and set the latest revision of this job to work in progress." + Environment.NewLine + "This process is not reversible." + Environment.NewLine + Environment.NewLine + "Do you wish to perform this action?";

        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (confirmStep == 1)
            {
                confirmStep = 2;                
                OKButton.Visibility = Visibility.Collapsed;
                UndoButton.Visibility = Visibility.Visible;
                textBlockMessage.Text = "Please enter the reason for undo.";
                txtReason.Visibility = Visibility.Visible;
            }
            else
            {
                if (txtReason.Text.Trim() != "")
                {
                    BusyIndicator1.IsBusy = true;
                    RetailSystemClient mrsClient = new RetailSystemClient();
                    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                    mrsClient.UndoCurrentMilestoneCompleted += delegate(object o, UndoCurrentMilestoneCompletedEventArgs es)
                    {
                        if (es.Error == null)
                        {
                            if (es.Result != null)
                            {

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
                            ExceptionHandler.PopUpErrorMessage(es.Error, "ResetCurrentMilestoneCompleted");
                    };

                    mrsClient.UndoCurrentMilestoneAsync(_selectedEstimateRevisionId, (App.Current as App).CurrentUserId, txtReason.Text);
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
            string description = "Reset current milestone by user " + (App.Current as App).CurrentUserLoginName + "." + Environment.NewLine + "Reason:" + Environment.NewLine + txtReason.Text + Environment.NewLine + "Revisons have been removed are:" + Environment.NewLine+result;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.UndoCurrentMilestone,
                _selectedEstimateRevisionId,
                description,
                0);
        }
    }
}

