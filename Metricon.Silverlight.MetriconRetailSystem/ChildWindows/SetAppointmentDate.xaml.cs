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
    public partial class SetAppointmentDate : ChildWindow
    {
        private int _recordId;
        private string _recordType;
        private RetailSystemClient _mrsClient;

        public SetAppointmentDate(object dataContext)
        {
            InitializeComponent();

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            EstimateGridItem item = (EstimateGridItem)dataContext;

            _recordId = item.RecordId;
            _recordType = item.RecordType;

            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());

            if (item.AppointmentDate != new DateTime(1, 1, 1))
                dtAppointment.SelectedValue = item.AppointmentDate;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (dtAppointment.SelectedValue != null)
            {
                if (dtAppointment.SelectedValue < DateTime.Now)
                {
                    BusyIndicator1.IsBusy = false;

                    DialogParameters param = new DialogParameters();
                    param.Header = "Invalid Appointment Time";
                    param.Content = "The specified Appointment Time is in the past.\r\nPlease select another Appointment Time.";
                    RadWindow.Alert(param);
                }
                else
                {
                    BusyIndicator1.IsBusy = true;
                    
                    _mrsClient.UpdateEstimateAppointmentTimeCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateEstimateAppointmentTimeCompleted);
                    _mrsClient.UpdateEstimateAppointmentTimeAsync(_recordId, dtAppointment.SelectedValue.Value, (App.Current as App).CurrentUserId);
                }
            }
            else
            {
                BusyIndicator1.IsBusy = false;

                DialogParameters param = new DialogParameters();
                param.Header = "Appointment Time is required";
                param.Content = "Please specify the Appointment Time";
                RadWindow.Alert(param);
            }
        }

        void mrsClient_UpdateEstimateAppointmentTimeCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _mrsClient.UpdateEstimateAppointmentTimeCompleted -= new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateEstimateAppointmentTimeCompleted);

            if (e.Error == null)
            {
                CreateLog();
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateEstimateAppointmentTimeCompleted");
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
            _mrsClient.CreateSalesEstimateLogCompleted +=new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);
            _mrsClient.CreateSalesEstimateLogAsync(
                (App.Current as App).CurrentUserLoginName,
                MRSLogAction.UpdateAppointment,
                _recordId,
                "Appointment Time has changed to " + dtAppointment.SelectedValue.Value.ToString("dd/MM/yyyy hh:mm tt"),
                0);
        }

        void mrsClient_CreateSalesEstimateLogCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            _mrsClient.CreateSalesEstimateLogCompleted -= new EventHandler<CreateSalesEstimateLogCompletedEventArgs>(mrsClient_CreateSalesEstimateLogCompleted);

            BusyIndicator1.IsBusy = false;

            if (e.Error == null)
            {
                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateSalesEstimateLogCompleted");
        }
    }
}

