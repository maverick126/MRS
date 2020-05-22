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
    public partial class ContractDraftActions : ChildWindow
    {
        private RetailSystemClient _mrsClient;
        private int _estimateRevisionTypeId;
        private int _estimateRevisionId;
        private int _contractNumber;

        public ContractDraftActions(object dataContext, int estimateRevisionTypeId)
        {
            InitializeComponent();

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            EstimateGridItem item = (EstimateGridItem)dataContext;
            _estimateRevisionId = item.RecordId;
            _contractNumber = item.ContractNumber;
            _estimateRevisionTypeId = estimateRevisionTypeId;
            txtMessage.Text = String.Format(txtMessage.Text, item.EstimateId.ToString());
            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());

            PopulateOwners(estimateRevisionTypeId);
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

                if (_estimateRevisionTypeId == 7) //Colour Selection
                {
                    _mrsClient.GetStudioMAppointmentTimeCompleted += new EventHandler<GetStudioMAppointmentTimeCompletedEventArgs>(_mrsClient_GetStudioMAppointmentTimeCompleted);
                    _mrsClient.GetStudioMAppointmentTimeAsync(_contractNumber.ToString(), "2800");
                }
                else if (_estimateRevisionTypeId == 8) //Electrical Selection
                {
                    _mrsClient.GetStudioMAppointmentTimeCompleted += new EventHandler<GetStudioMAppointmentTimeCompletedEventArgs>(_mrsClient_GetStudioMAppointmentTimeCompleted);
                    _mrsClient.GetStudioMAppointmentTimeAsync(_contractNumber.ToString(), "3047");
                }
                else
                    BusyIndicator1.IsBusy = false;
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted");
                BusyIndicator1.IsBusy = false;
            }
            
        }

        void _mrsClient_GetStudioMAppointmentTimeCompleted(object sender, GetStudioMAppointmentTimeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                DateTime appointmentTime = e.Result;
                if (appointmentTime != DateTime.MinValue)
                    dtAppointment.SelectedValue = appointmentTime;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetStudioMAppointmentTimeCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbOwner.SelectedIndex == 0)
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Owner is required";
                param.Content = "Please specify the Owner";
                RadWindow.Alert(param);
                return;
            }
            else if (!dtAppointment.SelectedValue.HasValue)
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Appointment Time is required";
                param.Content = "Please specify the Appointment Time";
                RadWindow.Alert(param);
                return;
            }
            else if (dtAppointment.SelectedValue.Value < DateTime.Now)
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Invalid Appointment Time";
                param.Content = "The specified Appointment Time is in the past.\r\nPlease select another Appointment Time.";
                RadWindow.Alert(param);
                return;
            }
            else
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Creating Estimate Revision...";

                _mrsClient.CreateStudioMRevisionCompleted += new EventHandler<CreateStudioMRevisionCompletedEventArgs>(mrsClient_CreateStudioMRevisionCompleted);
                _mrsClient.CreateStudioMRevisionAsync(_estimateRevisionId, Convert.ToInt32(cmbOwner.SelectedValue), dtAppointment.SelectedValue.Value, _estimateRevisionTypeId, (App.Current as App).CurrentUserId);
            }

        }

        void mrsClient_CreateStudioMRevisionCompleted(object sender, CreateStudioMRevisionCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                int newrevisionid =int.Parse(e.Result.ToString());
                //RadWindow window = this.ParentOfType<RadWindow>();
                //if (window != null)
                //{
                //    window.DialogResult = true;
                //    window.Close();
                //}
                CreateLog(newrevisionid);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateStudioMRevisionCompleted");
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

        private void CreateLog(int newrevisionid)
        {
            User selectedOwner = (User)cmbOwner.SelectedItem;
            string username = (App.Current as App).CurrentUserFullName;
            string description = string.Format("Sales Estimate was assigned to {0} by {1}.", selectedOwner.FullName, username);

            _mrsClient.CreateSalesEstimateLogCompleted += new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.Assign,
                newrevisionid,
                description,
                0);
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, CreateSalesEstimateLogCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            MessageBoxResult result = MessageBox.Show("Estimate revision created successfully!\r\n\r\nDo you want to view the newly created revision?\r\nClick OK to view the new revision, or Cancel to go back to Ready for Studio M revision.", "Estimate Revision Created", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                (App.Current as App).SelectedRevisionTypeId = _estimateRevisionTypeId;
                (App.Current as App).SelectedStatusId = 1; //In Progress
            }

            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }
    }
}

