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
    public partial class ModifyComments : ChildWindow
    {
        private int _estimateRevisionId;
        private string _comments;

        public ModifyComments(int estimateRevisionId)
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;

            _estimateRevisionId = estimateRevisionId;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetEstimateHeaderCompleted += new EventHandler<GetEstimateHeaderCompletedEventArgs>(mrsClient_GetEstimateHeaderCompleted);
            mrsClient.GetEstimateHeaderAsync(_estimateRevisionId);
        }

        void mrsClient_GetEstimateHeaderCompleted(object sender, GetEstimateHeaderCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                EstimateHeader estimate = e.Result;

                txtComments.Text = estimate.Comments;
                _comments = estimate.Comments;

                this.Title = String.Format(this.Title.ToString(), estimate.EstimateId.ToString());
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateHeaderCompleted");

            BusyIndicator1.IsBusy = false; 
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // If the Comments has been modified
            if (_comments != txtComments.Text)
            {

                BusyIndicator1.IsBusy = true;

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.UpdateCommentCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateCommentCompleted);
                mrsClient.UpdateCommentAsync(_estimateRevisionId, txtComments.Text, (App.Current as App).CurrentUserId, 0,"");
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

        void mrsClient_UpdateCommentCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateCommentCompleted");
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

