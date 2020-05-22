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
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class AssignOwner : ChildWindow
    {
        private int _recordId;
        private int _estimateRevisionId;
        private string _recordType;
        private string _currentContractStatus;
        private RetailSystemClient _mrsClient;

        public AssignOwner(object dataContext)
        {
            InitializeComponent();

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            EstimateGridItem item = (EstimateGridItem)dataContext;

            _recordType = item.RecordType;
            _recordId = item.RecordId;
            _currentContractStatus = item.ContractStatusName;

            txtMessageEditEstimateLockInfo.Visibility = item.EditEstimateUserID > 0 ? Visibility.Visible : Visibility.Collapsed;
            if (txtMessageEditEstimateLockInfo.Visibility == Visibility.Visible)
            {
                txtMessageEditEstimateLockInfo.Text = string.Format(txtMessageEditEstimateLockInfo.Text, item.EditEstimateUserName);
            }

            txtMessage.Text = String.Format(txtMessage.Text, item.EstimateId.ToString());
            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());            

            PopulateOwners(item.RevisionTypeId);
        }


        public void PopulateOwners(int revisionTypeId)
        {
            BusyIndicator1.IsBusy = true;

            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted);
            _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, revisionTypeId);
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<User> owners = e.Result;
                owners.Insert(0, new User { FullName = "Select", RegionId = 0, UserId = 0 });
                cmbOwner.ItemsSource = owners;

                cmbOwner.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted");

            BusyIndicator1.IsBusy = false;
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOwner.SelectedIndex > 0)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Assigning Estimate...";

                if (_recordType == "Queue")
                {
                    _mrsClient.AssignQueuedEstimateCompleted += new EventHandler<AssignQueuedEstimateCompletedEventArgs>(mrsClient_AssignQueuedEstimateCompleted);
                    _mrsClient.AssignQueuedEstimateAsync(_recordId, (App.Current as App).CurrentUserId, Convert.ToInt32(cmbOwner.SelectedValue));
                }
                else
                {
                    _mrsClient.AssignWorkingEstimateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_AssignWorkingEstimateCompleted);
                    _mrsClient.AssignWorkingEstimateAsync(_recordId, (App.Current as App).CurrentUserId, Convert.ToInt32(cmbOwner.SelectedValue));
                }
            }
            else
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Owner is required";
                param.Content = "Please specify the Owner";
                RadWindow.Alert(param);
            }
        }

        void mrsClient_AssignQueuedEstimateCompleted(object sender, AssignQueuedEstimateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                _estimateRevisionId = e.Result;
                if (_currentContractStatus == "Pending")
                    UpdateContractStatus();
                else
                    CreateLog();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "AssignQueuedEstimateCompleted");
        }

        void mrsClient_AssignWorkingEstimateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                _estimateRevisionId = _recordId;
                if (_currentContractStatus == "Pending")
                    UpdateContractStatus();
                else
                    CreateLog();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "AssignWorkingEstimateCompleted");
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

        //Change Contract Status in CRM from 'Pending' to 'Work In Progress' 
        private void UpdateContractStatus()
        {
            _mrsClient.SetContractStatusCompleted += new EventHandler<SetContractStatusCompletedEventArgs>(mrsClient_SetContractStatusCompleted);
            _mrsClient.SetContractStatusAsync((App.Current as App).CurrentUserLoginName, _estimateRevisionId, ContractStatus.WorkInProgress);
        }

        void mrsClient_SetContractStatusCompleted(object sender, SetContractStatusCompletedEventArgs e)
        {
            CreateLog();
        }

        private void CreateLog()
        {
            User selectedOwner = (User)cmbOwner.SelectedItem;
            string description = string.Format("Sales Estimate was assigned to {0}", selectedOwner.FullName); 

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.Assign,
                _estimateRevisionId,
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
    }
}

