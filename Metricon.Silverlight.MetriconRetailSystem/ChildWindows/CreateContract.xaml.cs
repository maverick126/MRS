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
    public partial class CreateContract : ChildWindow
    {
        private int _revisionTypeId;
        private int _estimateRevisionId;
        private RetailSystemClient _mrsClient;

        public CreateContract(int estimateRevisionId, int revisionTypeId)
        {
            InitializeComponent();

            _mrsClient = new RetailSystemClient();
            _mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            _estimateRevisionId = estimateRevisionId;
            _revisionTypeId = revisionTypeId;

            if (_revisionTypeId == 6) //Contract Draft
                txtMessage.Text = "Do you really want to create Studio M Split Revisions?";
            else if (_revisionTypeId == 13) //Final Contract
                txtMessage.Text = "Do you really want to create Final Contract?";

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //Commented 20130724 Moved Appointment Time to CSC revision
            //if (dtAppointment.SelectedValue != null)
            //{
            //    if (dtAppointment.SelectedValue < DateTime.Now)
            //    {
            //        DialogParameters param = new DialogParameters();
            //        param.Header = "Invalid Appointment Time";
            //        param.Content = "The specified Appointment Time is in the past.\r\nPlease select another Appointment Time.";
            //        RadWindow.Alert(param);
            //    }
            //    else
            //    {
            //        DateTime appointmentTime = dtAppointment.SelectedValue.Value;

                    if (_revisionTypeId == 6) //Contract Draft
                    {
                        BusyIndicator1.IsBusy = true;
                        BusyIndicator1.BusyContent = "Creating Ready for Studio M Split Revisions...";

                        _mrsClient.CreateContractDraftCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CreateContractDraftCompleted);
                        _mrsClient.CreateContractDraftAsync(_estimateRevisionId, (App.Current as App).CurrentUserId/*, DateTime.MinValue*/);
                    }
                    else if (_revisionTypeId == 13) //Final Contract
                    {
                        BusyIndicator1.IsBusy = true;
                        BusyIndicator1.BusyContent = "Creating Final Contract...";

                        _mrsClient.CreateFinalContractCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CreateFinalContractCompleted);
                        _mrsClient.CreateFinalContractAsync(_estimateRevisionId, (App.Current as App).CurrentUserId/*, DateTime.MinValue*/);
                    }
            //    }
            //}
        }

        void mrsClient_CreateFinalContractCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            if (e.Error == null)
            {
                (App.Current as App).SelectedRevisionTypeId = 13; //Final Contract
                (App.Current as App).SelectedStatusId = 2; //Completed

                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateFinalContractCompleted");
        }

        void mrsClient_CreateContractDraftCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            if (e.Error == null)
            {
                (App.Current as App).SelectedRevisionTypeId = 6; //Ready for Studio M Split Revisions
                (App.Current as App).SelectedStatusId = 2; //Completed

                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateContractDraftCompleted");
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

