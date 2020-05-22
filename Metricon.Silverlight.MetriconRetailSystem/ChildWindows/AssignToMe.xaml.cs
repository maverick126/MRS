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
    public partial class AssignToMe : ChildWindow
    {
        private int _recordId;
        private int _estimateRevisionId;
        private string _currentContractStatus;
        private RetailSystemClient _mrsClient;

        public AssignToMe(object dataContext)
        {
            InitializeComponent();

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            EstimateGridItem item = (EstimateGridItem)dataContext;

            _recordId = item.RecordId;
            _currentContractStatus = item.ContractStatusName;

            txtMessage.Text = String.Format(txtMessage.Text, item.EstimateId.ToString());
            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());    
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicator1.IsBusy = true;

            _mrsClient.AssignQueuedEstimateCompleted += new EventHandler<AssignQueuedEstimateCompletedEventArgs>(mrsClient_AssignQueuedEstimateCompleted);
            _mrsClient.AssignQueuedEstimateAsync(_recordId, (App.Current as App).CurrentUserId, (App.Current as App).CurrentUserId);
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
            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.AssignToMe,
                _estimateRevisionId,
                null,
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
    }
}

