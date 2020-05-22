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

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class CommenceWork : ChildWindow
    {
        private int _recordId;

        public CommenceWork(object dataContext)
        {
            InitializeComponent();

            EstimateGridItem item = (EstimateGridItem)dataContext;

            _recordId = item.RecordId;

            txtMessage.Text = String.Format(txtMessage.Text, item.EstimateId.ToString());
            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());  
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            //mrsClient.CommenceWorkCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CommenceWorkCompleted);
            //mrsClient.CommenceWorkAsync(_recordId, (App.Current as App).CurrentUserId);        
        }

        void mrsClient_CommenceWorkCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

