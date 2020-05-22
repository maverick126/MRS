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

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class ChangeHomeName : ChildWindow
    {
        private int _estimateRevisionId;
        private string _homeName;
        private RetailSystemClient _mrsClient;

        public ChangeHomeName(int revisionid)
        {
            _estimateRevisionId = revisionid;
            InitializeComponent();
            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            PopulateHomeName();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            BusyIndicator1.IsBusy = true;
            BusyIndicator1.BusyContent = "Changing Home Name ...";

            string homeName = txtHomeName.Text.Trim();

            if (homeName != _homeName)
            {
                if (homeName == string.Empty)
                    homeName = null;

                _mrsClient.UpdateHomeNameCompleted += new EventHandler<UpdateHomeNameCompletedEventArgs>(_mrsClient_UpdateHomeNameCompleted);
                _mrsClient.UpdateHomeNameAsync(_estimateRevisionId, homeName, (App.Current as App).CurrentUserId);
            }
            else
            {
                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = false;
                    window.Close();
                }
            }
        }

        public void PopulateHomeName()
        {
            BusyIndicator1.IsBusy = true;

            _mrsClient.GetHomeNameCompleted += new EventHandler<GetHomeNameCompletedEventArgs>(_mrsClient_GetHomeNameCompleted);
            _mrsClient.GetHomeNameAsync(_estimateRevisionId);
        }

        void _mrsClient_GetHomeNameCompleted(object sender, GetHomeNameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                _homeName = e.Result;
                txtHomeName.Text = _homeName;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "mrsClient_GetContractTypeCompleted");
            BusyIndicator1.IsBusy = false;
        }

        void _mrsClient_UpdateHomeNameCompleted(object sender, UpdateHomeNameCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                CreateLog();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateHomeNameCompleted");
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

        private void CreateLog()
        {
            string newHomeName = "blank";
            if (!string.IsNullOrEmpty(txtHomeName.Text.Trim()))
                newHomeName = txtHomeName.Text.Trim();

            string description = "Home Display Name has been changed to " + newHomeName + " by user " + (App.Current as App).CurrentUserLoginName;

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.ChangeHomeName,
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

