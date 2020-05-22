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
    public partial class SetDifficultyRating : ChildWindow
    {
        private int _recordId;
        private string _recordType;
        private int _difficultyRatingId;

        public SetDifficultyRating(object dataContext)
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;

            EstimateGridItem item = (EstimateGridItem)dataContext;

            _recordType = item.RecordType;
            _recordId = item.RecordId;
            _difficultyRatingId = item.DifficultyRatingId;

            txtMessage.Text = String.Format(txtMessage.Text, item.EstimateId.ToString());
            this.Title = String.Format(this.Title.ToString(), item.EstimateId.ToString());

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetDifficultyRatingsCompleted += new EventHandler<GetDifficultyRatingsCompletedEventArgs>(mrsClient_GetDifficultyRatingsCompleted);
            mrsClient.GetDifficultyRatingsAsync();
        }

        void mrsClient_GetDifficultyRatingsCompleted(object sender, GetDifficultyRatingsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ObservableCollection<DifficultyRating> ratings = e.Result;
                ratings.Insert(0, new DifficultyRating { DifficultyRatingId = 0, DifficultyRatingName = "Select" });
                cmbDifficultyRating.ItemsSource = ratings;

                if (_difficultyRatingId > 0)
                    cmbDifficultyRating.SelectedValue = _difficultyRatingId;
                else
                    cmbDifficultyRating.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetDifficultyRatingsCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbDifficultyRating.SelectedIndex > 0)
            {
                BusyIndicator1.IsBusy = true;
                BusyIndicator1.BusyContent = "Saving Estimate...";

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                if (_recordType == "Queue")
                {
                    mrsClient.UpdateQueueDifficultyRatingCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateDifficultyRatingCompleted);
                    mrsClient.UpdateQueueDifficultyRatingAsync(_recordId, Convert.ToInt32(cmbDifficultyRating.SelectedValue));
                }
                else if (_recordType == "EstimateHeader")
                {
                    mrsClient.UpdateEstimateDifficultyRatingCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateDifficultyRatingCompleted);
                    mrsClient.UpdateEstimateDifficultyRatingAsync(_recordId, Convert.ToInt32(cmbDifficultyRating.SelectedValue), (App.Current as App).CurrentUserId);
                }
                else
                {
                    BusyIndicator1.IsBusy = false;

                    DialogParameters param = new DialogParameters();
                    param.Header = "Error";
                    param.Content = "Invalid Record Type";
                    RadWindow.Alert(param);
                }
            }
            else
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Difficulty Rating is required";
                param.Content = "Please specify the Difficulty Rating";
                RadWindow.Alert(param);
            }
        }

        void mrsClient_UpdateDifficultyRatingCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
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
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateDifficultyRatingCompleted");
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

