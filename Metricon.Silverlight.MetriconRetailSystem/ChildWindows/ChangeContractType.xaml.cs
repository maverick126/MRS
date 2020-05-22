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
    public partial class ChangeContractType : ChildWindow
    {
        private int _estimateRevisionId;
        private RetailSystemClient _mrsClient;
        private string _headerText = string.Empty;
        private bool _anyMilestoneExists = false;

        public ChangeContractType(int revisionid, string headerText, bool anyMilestoneExists = false)
        {
            _estimateRevisionId = revisionid;
            _headerText = headerText;
            _anyMilestoneExists = anyMilestoneExists;
            InitializeComponent();
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            PopulateContractType();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbContractType.SelectedValue != null)
            {
                string contractType = null;
                string jobFlowType = null;
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = _headerText + " ...";
                if (_headerText == "Change Job Flow")
                    jobFlowType = cmbContractType.SelectedValue.ToString();
                else
                    contractType = cmbContractType.SelectedValue.ToString();
                _mrsClient.UpdateContractTypeCompleted += new EventHandler<UpdateContractTypeCompletedEventArgs>(UpdateContractTypeCompleted);
                _mrsClient.UpdateContractTypeAsync(_estimateRevisionId, contractType, jobFlowType, (App.Current as App).CurrentUserId);
            }
            else
            {
            }
        }
        void UpdateContractTypeCompleted(object sender, UpdateContractTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                CreateLog();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateContractTypeCompleted");
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

        public void PopulateContractType()
        {
            BusyIndicator1.IsBusy = true;
            string configCode = "MRSProcessType";
            if (_headerText == "Change Job Flow")
                configCode = "MRSJobFlow";
            txtMessage.Text = _headerText + " To : ";
            _mrsClient.GetContractTypeCompleted += new EventHandler<GetContractTypeCompletedEventArgs>(mrsClient_GetContractTypeCompleted);
            _mrsClient.GetContractTypeAsync(configCode);
        }

        void mrsClient_GetContractTypeCompleted(object sender, GetContractTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                List<ContractType> availableOptionsList = e.Result.ToList();
                if (_headerText == "Change Job Flow" && _anyMilestoneExists)
                {
                    availableOptionsList = availableOptionsList.Where(w => w.ContractTypeID != "FT").ToList();
                }
                cmbContractType.ItemsSource = availableOptionsList;
                cmbContractType.SelectedValuePath = "ContractTypeID";
                cmbContractType.DisplayMemberPath = "ContractTypeName";
                cmbContractType.SelectedValue = (App.Current as App).SelectedContractType;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "mrsClient_GetContractTypeCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void CreateLog()
        {

            string description = _headerText.Replace("Change ","") + " has been changed to " + cmbContractType.Text + " by user " + (App.Current as App).CurrentUserLoginName;

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                 _headerText == "Change Job Flow" ? MRSLogAction.ChangeJobFlow : MRSLogAction.ChangeContractType,
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

