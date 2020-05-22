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
    public partial class SetDueDate : ChildWindow
    {
        private int _recordId;
        private string _recordType;

        public SetDueDate(object dataContext)
        {
            InitializeComponent();

            EstimateGridItem item = (EstimateGridItem)dataContext;

            _recordId = item.RecordId;
            _recordType = item.RecordType;

            txtMessage.Text = String.Format(txtMessage.Text, item.EstimateId.ToString());
            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());       

            if (item.DueDate != new DateTime(1, 1, 1))
                dDueDate.SelectedDate = item.DueDate;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (dDueDate.SelectedDate != null)
            {
                if (dDueDate.SelectedDate < DateTime.Now)
                {
                    BusyIndicator1.IsBusy = false;

                    DialogParameters param = new DialogParameters();
                    param.Header = "Invalid Due Date";
                    param.Content = "The specified Due Date is in the past.\r\nPlease select another Due Date.";
                    RadWindow.Alert(param);
                }
                else
                {
                    BusyIndicator1.IsBusy = true;

                    RetailSystemClient mrsClient = new RetailSystemClient();
                    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                    if (_recordType == "Queue")
                    {
                        mrsClient.UpdateQueueDueDateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateDueDateCompleted);
                        mrsClient.UpdateQueueDueDateAsync(_recordId, dDueDate.SelectedDate.Value, (App.Current as App).CurrentUserId);
                    }
                    else if (_recordType == "EstimateHeader")
                    {
                        mrsClient.UpdateEstimateDueDateCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateDueDateCompleted);
                        mrsClient.UpdateEstimateDueDateAsync(_recordId, dDueDate.SelectedDate.Value, (App.Current as App).CurrentUserId);
                    }
                    else
                    {
                        DialogParameters param = new DialogParameters();
                        param.Header = "Error";
                        param.Content = "Invalid Record Type";
                        RadWindow.Alert(param);
                    }
                }
            }
            else
            {
                BusyIndicator1.IsBusy = false;

                DialogParameters param = new DialogParameters();
                param.Header = "Due Date is required";
                param.Content = "Please specify the Due Date";
                RadWindow.Alert(param);
            }
            
        }

        void mrsClient_UpdateDueDateCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
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
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateDueDateCompleted");
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

