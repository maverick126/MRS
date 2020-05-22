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
using System.Collections.ObjectModel;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class CreateStudioMSplit : ChildWindow
    {
        private int _revisionTypeId;
        private int _estimateRevisionId;
        private RetailSystemClient _mrsClient;

        public CreateStudioMSplit(int estimateRevisionId, int revisionTypeId)
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

            //_mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted);
            //_mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, _revisionTypeId);
            PopulateOwners();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if ((checkBoxColour.IsChecked ?? false) && radComboBoxColour.SelectedValue.ToString() == "0")
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Select Studio M revision(s)";
                param.Content = "Please select a person for Colour.";
                RadWindow.Alert(param);
                return;
            }
            else if ((checkBoxElectricals.IsChecked ?? false) && radComboBoxElectricals.SelectedValue.ToString() == "0")
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Select Studio M revision(s)";
                param.Content = "Please select a person for Electricals.";
                RadWindow.Alert(param);
                return;
            }
            if ((checkBoxLandscaping.IsChecked ?? false) && radComboBoxLandscaping.SelectedValue.ToString() == "0")
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Select Studio M revision(s)";
                param.Content = "Please select a person for Landscaping.";
                RadWindow.Alert(param);
                return;
            }
            else if ((checkBoxAppliances.IsChecked ?? false) && radComboBoxAppliances.SelectedValue.ToString() == "0")
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Select Studio M revision(s)";
                param.Content = "Please select a person for Appliances.";
                RadWindow.Alert(param);
                return;
            }
            if ((checkBoxCarpets.IsChecked ?? false) && radComboBoxCarpets.SelectedValue.ToString() == "0")
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Select Studio M revision(s)";
                param.Content = "Please select a person for Carpets.";
                RadWindow.Alert(param);
                return;
            }
            else if ((checkBoxTimberFloor.IsChecked ?? false) && radComboBoxTimberFloor.SelectedValue.ToString() == "0")
            {
                DialogParameters param = new DialogParameters();
                param.Header = "Select Studio M revision(s)";
                param.Content = "Please select a person for TimberFloor.";
                RadWindow.Alert(param);
                return;
            }
            else
            {
                string revisionTypeIds = string.Empty;
                string assignedToUserIds = string.Empty;

                if (checkBoxColour.IsChecked ?? false)
                {
                    revisionTypeIds += ",7";
                    assignedToUserIds += "," + radComboBoxColour.SelectedValue.ToString();
                }
                if (checkBoxElectricals.IsChecked ?? false)
                {
                    revisionTypeIds += ",8";
                    assignedToUserIds += "," + radComboBoxElectricals.SelectedValue.ToString();
                }
                if (checkBoxLandscaping.IsChecked ?? false)
                {
                    revisionTypeIds += ",29";
                    assignedToUserIds += "," + radComboBoxLandscaping.SelectedValue.ToString();
                }
                if (checkBoxAppliances.IsChecked ?? false)
                {
                    revisionTypeIds += ",28";
                    assignedToUserIds += "," + radComboBoxAppliances.SelectedValue.ToString();
                }
                if (checkBoxCarpets.IsChecked ?? false)
                {
                    revisionTypeIds += ",12";
                    assignedToUserIds += "," + radComboBoxCarpets.SelectedValue.ToString();
                }
                if (checkBoxTimberFloor.IsChecked ?? false)
                {
                    revisionTypeIds += ",22";
                    assignedToUserIds += "," + radComboBoxTimberFloor.SelectedValue.ToString();
                }
                    
                if (!string.IsNullOrWhiteSpace(revisionTypeIds))
                {
                    BusyIndicator1.IsBusy = true;
                    BusyIndicator1.BusyContent = "Creating Ready for Studio M Split Revisions...";
                    if ((App.Current as App).SelectedEstimateRevisionTypeID == 5)
                    {
                        // CSC - in progress select complete Ready for Studio M needs current one also to move Completed
                        revisionTypeIds = "6" + revisionTypeIds;
                        assignedToUserIds = (App.Current as App).CurrentUserId.ToString() + assignedToUserIds;
                    }
                    else
                    {
                        // remove the initial comma ,
                        revisionTypeIds = revisionTypeIds.Substring(1);
                    }
                    _mrsClient.CreateSplitStudioMRevisionsCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_CreateSplitStudioMRevisionsCompleted);
                    _mrsClient.CreateSplitStudioMRevisionsAsync(_estimateRevisionId, revisionTypeIds, assignedToUserIds, (App.Current as App).CurrentUserId);
                }
                else
                {
                    DialogParameters param = new DialogParameters();
                    param.Header = "Select Studio M revision(s)";
                    param.Content = "Please assign one or more Studio M revision(s).";
                    RadWindow.Alert(param);
                    return;
                }
            }
        }
        
        void mrsClient_CreateSplitStudioMRevisionsCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            BusyIndicator1.IsBusy = false;

            if (e.Error == null)
            {
                (App.Current as App).SelectedStatusId = 1; //In Progress

                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "CreateSplitStudioMRevisionsCompleted");
        }

        public void PopulateOwners()
        {
            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Color);
            _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, 7);
        }

        //void mrsClient_GetUsersByRegionAndRevisionTypeCompleted(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        //{
        //    if (e.Error == null)
        //    {
        //        ObservableCollection<User> owners = e.Result;
        //        owners.Insert(0, new User { FullName = "Select", RegionId = 0, UserId = 0 });
        //        cmbOwner.ItemsSource = owners;

        //        cmbOwner.SelectedIndex = 0;

        //        if (_estimateRevisionTypeId == 7) //Colour Selection
        //        {
        //            _mrsClient.GetStudioMAppointmentTimeCompleted += new EventHandler<GetStudioMAppointmentTimeCompletedEventArgs>(_mrsClient_GetStudioMAppointmentTimeCompleted);
        //            _mrsClient.GetStudioMAppointmentTimeAsync(_contractNumber.ToString(), "2800");
        //        }
        //        else if (_estimateRevisionTypeId == 8) //Electrical Selection
        //        {
        //            _mrsClient.GetStudioMAppointmentTimeCompleted += new EventHandler<GetStudioMAppointmentTimeCompletedEventArgs>(_mrsClient_GetStudioMAppointmentTimeCompleted);
        //            _mrsClient.GetStudioMAppointmentTimeAsync(_contractNumber.ToString(), "3047");
        //        }
        //        else
        //            BusyIndicator1.IsBusy = false;
        //    }
        //    else
        //    {
        //        ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted");
        //        BusyIndicator1.IsBusy = false;
        //    }

        //}

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Color(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User defualtuser = new User { FullName = "Select", RegionId = 0, UserId = 0 };
                ObservableCollection<User> owners = e.Result;

                owners.Insert(0, defualtuser);
                radComboBoxColour.ItemsSource = owners;
                radComboBoxColour.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted_Color");

            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted -= new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Color);
            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Electricals);
            _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, 8);
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Electricals(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User defualtuser = new User { FullName = "Select", RegionId = 0, UserId = 0 };
                ObservableCollection<User> owners = e.Result;

                owners.Insert(0, defualtuser);
                radComboBoxElectricals.ItemsSource = owners;
                radComboBoxElectricals.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted_Electricals");

            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted -= new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Electricals);
            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Landscaping);
            _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, 29);
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Landscaping(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User defualtuser = new User { FullName = "Select", RegionId = 0, UserId = 0 };
                ObservableCollection<User> owners = e.Result;

                owners.Insert(0, defualtuser);
                radComboBoxLandscaping.ItemsSource = owners;
                radComboBoxLandscaping.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted_Landscaping");

            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted -= new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Landscaping);
            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Appliances);
            _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, 28);
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Appliances(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User defualtuser = new User { FullName = "Select", RegionId = 0, UserId = 0 };
                ObservableCollection<User> owners = e.Result;

                owners.Insert(0, defualtuser);
                radComboBoxAppliances.ItemsSource = owners;
                radComboBoxAppliances.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted_Appliances");

            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted -= new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Appliances);
            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Carpets);
            _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, 12);
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Carpets(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User defualtuser = new User { FullName = "Select", RegionId = 0, UserId = 0 };
                ObservableCollection<User> owners = e.Result;

                owners.Insert(0, defualtuser);
                radComboBoxCarpets.ItemsSource = owners;
                radComboBoxCarpets.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted_Carpets");

            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted -= new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_Carpets);
            _mrsClient.GetUsersByRegionAndRevisionTypeCompleted += new EventHandler<GetUsersByRegionAndRevisionTypeCompletedEventArgs>(mrsClient_GetUsersByRegionAndRevisionTypeCompleted_TimberFloor);
            _mrsClient.GetUsersByRegionAndRevisionTypeAsync((App.Current as App).CurrentRegionId, 22);
        }

        void mrsClient_GetUsersByRegionAndRevisionTypeCompleted_TimberFloor(object sender, GetUsersByRegionAndRevisionTypeCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                User defualtuser = new User { FullName = "Select", RegionId = 0, UserId = 0 };
                ObservableCollection<User> owners = e.Result;

                owners.Insert(0, defualtuser);
                radComboBoxTimberFloor.ItemsSource = owners;
                radComboBoxTimberFloor.SelectedIndex = 0;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetUsersByRegionAndRevisionTypeCompleted_TimberFloor");
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

        private void CheckBoxColour_Click(object sender, RoutedEventArgs e)
        {
            if (radComboBoxColour != null)
            {
                radComboBoxColour.IsEnabled = ((CheckBox)sender).IsChecked ?? false;
                if (!radComboBoxColour.IsEnabled)
                    radComboBoxColour.SelectedIndex = 0;
            }
        }

        private void CheckBoxElectricals_Click(object sender, RoutedEventArgs e)
        {
            if (radComboBoxElectricals != null)
            {
                radComboBoxElectricals.IsEnabled = ((CheckBox)sender).IsChecked ?? false;
                if (!radComboBoxElectricals.IsEnabled)
                    radComboBoxElectricals.SelectedIndex = 0;
            }
        }

        private void CheckBoxLandscaping_Click(object sender, RoutedEventArgs e)
        {
            radComboBoxLandscaping.IsEnabled = ((CheckBox)sender).IsChecked??false;
            if (!radComboBoxLandscaping.IsEnabled)
                radComboBoxLandscaping.SelectedIndex = 0;
        }

        private void CheckBoxAppliances_Click(object sender, RoutedEventArgs e)
        {
            radComboBoxAppliances.IsEnabled = ((CheckBox)sender).IsChecked ?? false;
            if (!radComboBoxAppliances.IsEnabled)
                radComboBoxAppliances.SelectedIndex = 0;
        }

        private void CheckBoxCarpets_Click(object sender, RoutedEventArgs e)
        {
            radComboBoxCarpets.IsEnabled = ((CheckBox)sender).IsChecked ?? false;
            if (!radComboBoxCarpets.IsEnabled)
                radComboBoxCarpets.SelectedIndex = 0;
        }

        private void CheckBoxTimberFloor_Click(object sender, RoutedEventArgs e)
        {
            radComboBoxTimberFloor.IsEnabled = ((CheckBox)sender).IsChecked ?? false;
            if (!radComboBoxTimberFloor.IsEnabled)
                radComboBoxTimberFloor.SelectedIndex = 0;
        }
    }
}

