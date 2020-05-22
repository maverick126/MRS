using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Primitives;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using Telerik.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
#if !SILVERLIGHT
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
#endif
#if SILVERLIGHT
using SelectionChangedEventArgs = Telerik.Windows.Controls.SelectionChangedEventArgs;
using Telerik.Windows.Persistence;
#endif


namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class Estimate : Page
    {
        public static int RevisionTypeId;
        public string studiomquestionxml;
        public string selectedstudiomanswer;
        public string studiomanswerxml;
        public int previoustab, currenttab;
        private EstimateDetails deletedED;
        public RetailSystemClient mrsClient;
        public ObservableCollection<StudioMSupplierBrand> suplist;
        public ObservableCollection<StudioMQuestion> qulist;
        //public ObservableCollection<StudioMQuestion> qulist2; //Merge Question and Answer (In case some Questions are inactive)
        public ObservableCollection<StudioMAnswer> awlist;
        public ObservableCollection<StudioMSupplierBrand> selectedsup;
        public ObservableCollection<StudioMQuestion> selectedqu;
        public ObservableCollection<StudioMAnswer> selectedaw;
        public RadTabControl rd;
        public EstimateGridItem item;
        public string selectediamgeid = "";
        public bool goaheadaccept = true;
        public decimal gst = decimal.Parse("1.1");
        private IsolatedStorageSettings appSettings = IsolatedStorageSettings.ApplicationSettings;
        private ArrayList columnorder = new ArrayList();
        private string gridSaveLayout = "custom";//"custom or telerik"; // App.Current.Resources["SaveLayout"].ToString()
        public bool isSalesEstimatingRevision = false;
        public Estimate()
        {
            InitializeComponent();

            //if ((App.Current as App).CurrentAction == "EDIT")
            //{
            //    ((EstimateViewModel)LayoutRoot.DataContext).SynchronizeNewOptionsToEstimate(EstimateList.SelectedEstimateRevisionId);
            //}
            suplist = new ObservableCollection<StudioMSupplierBrand>();
            qulist = new ObservableCollection<StudioMQuestion>();
            //qulist2 = new ObservableCollection<StudioMQuestion>();
            awlist = new ObservableCollection<StudioMAnswer>();
            selectedsup = new ObservableCollection<StudioMSupplierBrand>();
            selectedqu = new ObservableCollection<StudioMQuestion>();
            selectedaw = new ObservableCollection<StudioMAnswer>();
            item = new EstimateGridItem();

            previoustab = 0;
            currenttab = 0;

            if ((App.Current as App).SelectedEstimateRevisionTypeID == 4 || //Sales Estimating
                (App.Current as App).SelectedEstimateRevisionTypeID == 15 || //Presite Variation - Sales Estimating
                (App.Current as App).SelectedEstimateRevisionTypeID == 25 || //Pre Studio M Variation - Sales Estimating
                (App.Current as App).SelectedEstimateRevisionTypeID == 19)  //Building Variation - Building Estimator
            {
                isSalesEstimatingRevision = true;
            }

            if ((App.Current as App).CurrentAction != "EDIT")
            {
                //((RadGridView)this.OptionTabControl.FindChildByType<RadGridView>()).IsEnabled = false;
                btnAccept.Visibility = Visibility.Collapsed;
                btnReject.Visibility = Visibility.Collapsed;
                btnComments.Visibility = Visibility.Collapsed;
                btnSyncCustomerDetails.Visibility = Visibility.Collapsed;
                OptionTreePane.Visibility = Visibility.Collapsed;
                OtherOptionsPane.Visibility = Visibility.Collapsed;
                SearchProductPane.Visibility = Visibility.Collapsed;
                AdditionalNotesPane.Visibility = Visibility.Collapsed;
                btnUnlock.Visibility = Visibility.Collapsed;
                btnSetDocumentType.Visibility = Visibility.Collapsed;
                btnSpellCheck.Visibility = Visibility.Collapsed;
                btnChangeHomeName.Visibility = Visibility.Collapsed;
                btnSetStudioMAnswer.Visibility = Visibility.Collapsed;
                btnAnswerAll.Visibility = Visibility.Collapsed;
                btnCustomerSign.Visibility = Visibility.Collapsed;
                btnCopyEstimate.Visibility = Visibility.Collapsed;
            }
            else
            {
                (App.Current as App).SelectedEstimateRevisionId = EstimateList.SelectedEstimateRevisionId;

                if ((App.Current as App).CurrentUserRoleId != 4 || (
                    (App.Current as App).SelectedEstimateRevisionTypeID != 7 && //Colour Selection
                    (App.Current as App).SelectedEstimateRevisionTypeID != 8 && //Electrical Selection
                    (App.Current as App).SelectedEstimateRevisionTypeID != 9 && //Paving Selection
                    (App.Current as App).SelectedEstimateRevisionTypeID != 10 && //Tile Selection
                    (App.Current as App).SelectedEstimateRevisionTypeID != 11 && //Decking Selection
                    (App.Current as App).SelectedEstimateRevisionTypeID != 12 //Carpet Selection
                    ))// if not CC or revision type is STM
                {
                    btnUnlock.Visibility = Visibility.Collapsed;
                }

                //Templates tab should only appears in STS and SE revisions 
                if ((App.Current as App).SelectedEstimateRevisionTypeID != 2 && //Sales Accept
                    (App.Current as App).SelectedEstimateRevisionTypeID != 4 && //Sales Estimating
                    (App.Current as App).SelectedEstimateRevisionTypeID != 15 && //Pre Site Variation - Sales Estimating
                    (App.Current as App).SelectedEstimateRevisionTypeID != 25) //Pre Studio M Variation - Sales Estimating
                {
                    AdditionalNotesPane.Visibility = Visibility.Collapsed;
                }

                if ((App.Current as App).SelectedEstimateAllowToViewStudioMTab)
                {
                    btnSetStudioMAnswer.Visibility = Visibility.Visible;
                }
                else
                {
                    btnSetStudioMAnswer.Visibility = Visibility.Collapsed;
                    btnAnswerAll.Visibility = Visibility.Collapsed;
                }
            }

            item.CustomerName = EstimateList.SelectedCustomerName;
            item.CustomerNumber = EstimateList.SelectedCustomerNumber;
            item.ContractNumber = EstimateList.SelectedContractNumber;
            item.EstimateId = EstimateList.SelectedEstimateID;
            item.ContractStatusName = EstimateList.SelectedContractStatusName;
            item.RecordId = EstimateList.SelectedEstimateRevisionId;
            item.RevisionTypeId = EstimateList.SelectedRevisionTypeID;
            item.RevisionNumber = EstimateList.SelectedRevisionNumber;
            item.Accountid = EstimateList.SelectedAccountID;

            //18/08/2015 WINTE - All revisions are now sorted/grouped the same way  
            var areaDescriptor = new GroupDescriptor()
            {
                Member = "AreaName"
            };
            RadGridView1.GroupDescriptors.Add(areaDescriptor);
            var groupDescriptor = new GroupDescriptor()
            {
                Member = "GroupName"
            };
            RadGridView1.GroupDescriptors.Add(groupDescriptor);


            if (!appSettings.Contains("ColArea"))
                appSettings.Add("ColArea", "150");

            if (!appSettings.Contains("ColGroup"))
                appSettings.Add("ColGroup", "150");

            if (!appSettings.Contains("ColProductName"))
                appSettings.Add("ColProductName", "150");

            if (!appSettings.Contains("ColProductDesc"))
                appSettings.Add("ColProductDesc", "150");

            if (!appSettings.Contains("ColAdditionalNotes"))
                appSettings.Add("ColAdditionalNotes", "150");

            if (!appSettings.Contains("ColQuantity"))
                appSettings.Add("ColQuantity", "80");

            if (!appSettings.Contains("ColPrice"))
                appSettings.Add("ColPrice", "80");

            if (!appSettings.Contains("ColTotal"))
                appSettings.Add("ColTotal", "80");

            if (!appSettings.Contains("ColMarginPercent"))
                appSettings.Add("ColMarginPercent", "80");

            if (!appSettings.Contains("ColUom"))
                appSettings.Add("ColUom", "50");

            if (!appSettings.Contains("ColAccepted"))
                appSettings.Add("ColAccepted", "80");

            if (!appSettings.Contains("ColSoSi"))
                appSettings.Add("ColSoSi", "30");

            if (!appSettings.Contains("ColRemove"))
                appSettings.Add("ColRemove", "30");

            if (!appSettings.Contains("ColCopy"))
                appSettings.Add("ColCopy", "60");

            if (!appSettings.Contains("ColChanges"))
                appSettings.Add("ColChanges", "95");

            if (!appSettings.Contains("ColReplace"))
                appSettings.Add("ColReplace", "50");

            if (!appSettings.Contains("ColAreaIndex"))
                appSettings.Add("ColAreaIndex", "1");

            if (!appSettings.Contains("ColGroupIndex"))
                appSettings.Add("ColGroupIndex", "2");

            if (!appSettings.Contains("ColProductNameIndex"))
                appSettings.Add("ColProductNameIndex", "3");

            if (!appSettings.Contains("ColProductDescIndex"))
                appSettings.Add("ColProductDescIndex", "4");

            if (!appSettings.Contains("ColAdditionalNotesIndex"))
                appSettings.Add("ColAdditionalNotesIndex", "5");

            if (!appSettings.Contains("ColQuantityIndex"))
                appSettings.Add("ColQuantityIndex", "6");

            if (!appSettings.Contains("ColPriceIndex"))
                appSettings.Add("ColPriceIndex", "7");

            if (!appSettings.Contains("ColTotalIndex"))
                appSettings.Add("ColTotalIndex", "8");

            if (!appSettings.Contains("ColMarginPercentIndex"))
                appSettings.Add("ColMarginPercentIndex", "9");

            if (!appSettings.Contains("ColUomIndex"))
                appSettings.Add("ColUomIndex", "10");

            if (!appSettings.Contains("ColAcceptedIndex"))
                appSettings.Add("ColAcceptedIndex", "11");

            if (!appSettings.Contains("ColSoSiIndex"))
                appSettings.Add("ColSoSiIndex", "12");

            if (!appSettings.Contains("ColRemoveIndex"))
                appSettings.Add("ColRemoveIndex", "13");

            if (!appSettings.Contains("ColCopyIndex"))
                appSettings.Add("ColCopyIndex", "14");

            if (!appSettings.Contains("ColChangesIndex"))
                appSettings.Add("ColChangesIndex", "15");

            if (!appSettings.Contains("ColReplaceIndex"))
                appSettings.Add("ColReplaceIndex", "16");

            // for backward compatibility if any of the deleted column are already on the Isololate storage for the first time not to mess up the layout force reset column order
            if (appSettings.Contains("ColPhotoIndex") || appSettings.Contains("ColStudioMIndex") || appSettings.Contains("ColDerivedCostIndex"))
                btnResetColumnOrder_Click(null, null);

            GridColumnOrder o1 = new GridColumnOrder();
            o1.Order = 1;
            o1.ColumnName = "ColAreaIndex";
            columnorder.Add(o1);

            GridColumnOrder o2 = new GridColumnOrder();
            o2.Order = 2;
            o2.ColumnName = "ColGroupIndex";
            columnorder.Add(o2);

            GridColumnOrder o3 = new GridColumnOrder();
            o3.Order = 3;
            o3.ColumnName = "ColProductNameIndex";
            columnorder.Add(o3);

            GridColumnOrder o4 = new GridColumnOrder();
            o4.Order = 4;
            o4.ColumnName = "ColProductDescIndex";
            columnorder.Add(o4);

            GridColumnOrder o5 = new GridColumnOrder();
            o5.Order = 5;
            o5.ColumnName = "ColAdditionalNotesIndex";
            columnorder.Add(o5);

            GridColumnOrder o6 = new GridColumnOrder();
            o6.Order = 6;
            o6.ColumnName = "ColQuantityIndex";
            columnorder.Add(o6);

            GridColumnOrder o7 = new GridColumnOrder();
            o7.Order = 7;
            o7.ColumnName = "ColPriceIndex";
            columnorder.Add(o7);

            GridColumnOrder o8 = new GridColumnOrder();
            o8.Order = 8;
            o8.ColumnName = "ColTotalIndex";
            columnorder.Add(o8);

            GridColumnOrder o9 = new GridColumnOrder();
            o9.Order = 9;
            o9.ColumnName = "ColMarginPercentIndexIndex";
            columnorder.Add(o9);

            GridColumnOrder o10 = new GridColumnOrder();
            o10.Order = 10;
            o10.ColumnName = "ColUomIndex";
            columnorder.Add(o10);

            GridColumnOrder o11 = new GridColumnOrder();
            o11.Order = 11;
            o11.ColumnName = "ColAcceptedIndex";
            columnorder.Add(o11);

            GridColumnOrder o12 = new GridColumnOrder();
            o12.Order = 12;
            o12.ColumnName = "ColSoSiIndex";
            columnorder.Add(o12);

            GridColumnOrder o13 = new GridColumnOrder();
            o13.Order = 13;
            o13.ColumnName = "ColRemoveIndex";
            columnorder.Add(o13);

            GridColumnOrder o14 = new GridColumnOrder();
            o14.Order = 14;
            o14.ColumnName = "ColCopyIndex";
            columnorder.Add(o14);

            GridColumnOrder o15 = new GridColumnOrder();
            o15.Order = 15;
            o15.ColumnName = "ColChangesIndex";
            columnorder.Add(o15);

            GridColumnOrder o16 = new GridColumnOrder();
            o16.Order = 16;
            o16.ColumnName = "ColReplaceIndex";
            columnorder.Add(o16);
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        //protected void CheckRevisonTypePermission(int revisiontypeid)
        //{
        //    mrsClient = new RetailSystemClient();
        //    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
        //    mrsClient.CheckRevisionTypeAllowToAddNSRCompleted += delegate(object o, CheckRevisionTypeAllowToAddNSRCompletedEventArgs es)
        //    {
        //        if (es.Error == null)
        //        {
        //            revisiontypepermission = es.Result;
        //        }
        //    };

        //    mrsClient.CheckRevisionTypeAllowToAddNSRAsync(revisiontypeid);

        //}
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {

            //MessageBox.Show("From");
            //// Fix bug in DataContext of EstimateList when RadPane is left open
            //OptionTreePane.IsHidden = true;
            //AdditionalNotesPane.IsHidden = true;
            //CommentsPane.IsHidden = true;
            //DocumentsPane.IsHidden = true;
            //EstimateInformationPane.IsHidden = true;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // Fix bug in DataContext of EstimateList when RadPane is left open
            OptionTreePane.IsHidden = true;
            OtherOptionsPane.IsHidden = true;
            SearchProductPane.IsHidden = true;
            AdditionalNotesPane.IsHidden = true;
            CommentsPane.IsHidden = true;
            // CRM 9 onwards this will be disabled, upload directly in CRM 
            // DocumentsPane.IsHidden = true;
            EstimateInformationPane.IsHidden = true;

            ResetEditEstimateUserID();

            if (EstimateList.revisiontypepermission.ValidateStandardInclusion) // studio M
            {
                btnUnlock_Click(sender, e);// unlock the estimate when click back button
            }
            else
            {
                NavigationService.GoBack();
            }

        }


        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            if (EstimateList.revisiontypepermission.ValidateStandardInclusion) // studio M
            {
                ValidateStandardInclusion(EstimateList.SelectedEstimateRevisionId);
            }
            else if (EstimateList.revisiontypepermission.ValidateAccept) //Always True
            {
                //if (RevisionTypeId == 5) //CSC revision
                //{
                //    ValidateAppointmentDate(EstimateList.SelectedEstimateRevisionId);
                //}
                //else
                //{
                //Validate Accepted Flag for STA/SE and NSR Area/Group for all
                ValidateAcceptedFlag(EstimateList.SelectedEstimateRevisionId, ((App)App.Current).CurrentUserRoleId);
                //}
            }
            else
            {
                AcceptEstimate();
            }
        }

        public void ValidateAcceptedFlag(int estimaterevisionid, int userroleid)
        {
            List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
            bool show = false;
            goaheadaccept = true;
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.ValidateAcceptFlagForRevisionCompleted += delegate(object o, ValidateAcceptFlagForRevisionCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    foreach (ValidationErrorMessage s in es.Result)
                    {
                        result.Add(s);
                        if (!s.AllowGoAhead)
                        {
                            goaheadaccept = false;
                        }
                    }
                    if (result.Count > 0 && !goaheadaccept)
                    {
                        show = false;
                    }
                    else if (result.Count > 0 && goaheadaccept)
                    {
                        show = true;
                    }
                    if (result.Count > 0)
                    {
                        RadWindow win = new RadWindow();
                        ShowValidationMessage messageDlg = new ShowValidationMessage(result, show, estimaterevisionid);
                        win.Header = "Validation Error Message";
                        win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                        win.Content = messageDlg;
                        win.Closed += new EventHandler<WindowClosedEventArgs>(ValidationWin_Closed);
                        win.ShowDialog();
                    }
                    else
                    {
                        AcceptEstimate();
                    }
                }
            };

            mrsClient.ValidateAcceptFlagForRevisionAsync(estimaterevisionid, userroleid);

        }

        public void ValidateAppointmentDate(int estimaterevisionid, int userroleid)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.ValidateAppointmentDateCompleted += delegate(object o, ValidateAppointmentDateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if ((bool)es.Result)
                    {
                        ValidateAcceptedFlag(estimaterevisionid, userroleid);
                    }
                    else
                    {
                        RadWindow.Alert("Please specify Appointment Time before Accepting/Rejecting this estimate");
                        return;
                    }
                }
            };

            mrsClient.ValidateAppointmentDateAsync(estimaterevisionid);
        }


        public void ValidateStandardInclusion(int estimaterevisionid)
        {
            List<ValidationErrorMessage> result = new List<ValidationErrorMessage>();
            bool show = true;
            goaheadaccept = true;
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.ValidateStudioMEstimateCompleted += delegate(object o, ValidateStudioMEstimateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    foreach (ValidationErrorMessage s in es.Result)
                    {
                        if (show && !s.AllowGoAhead)
                        {
                            show = false;
                            goaheadaccept = false;
                        }
                        result.Add(s);
                    }

                    if (result.Count > 0 && !goaheadaccept)
                    {
                        show = false;
                    }
                    else if (result.Count > 0 && goaheadaccept)
                    {
                        show = true;
                    }
                    if (result.Count > 0)
                    {
                        RadWindow win = new RadWindow();
                        ShowValidationMessage messageDlg = new ShowValidationMessage(result, show, estimaterevisionid);
                        win.Header = "Validation Error Message";
                        win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                        win.Content = messageDlg;
                        win.Closed += new EventHandler<WindowClosedEventArgs>(ValidationWin_Closed);
                        win.ShowDialog();
                    }
                    else
                    {
                        AcceptEstimate();
                    }

                }
            };

            mrsClient.ValidateStudioMEstimateAsync(estimaterevisionid);
        }

        void ValidationWin_Closed(object sender, WindowClosedEventArgs e)
        {
            ((EstimateViewModel)LayoutRoot.DataContext).ForceRefreshTab();
            if (goaheadaccept)
            {
                AcceptEstimate();
            }
        }

        void CopyFromAvailableOptionTreeWin_Closed(object sender, WindowClosedEventArgs e)
        {
            if (e.DialogResult == true) { 
                GridViewRow row = sender as GridViewRow;
                if (row != null)
                {
                    EstimateDetails pag = row.DataContext as EstimateDetails;
                    ((EstimateViewModel)LayoutRoot.DataContext).CopyItemFromAllProductsToEstimate(pag);
                }
            }
        }

        private void AcceptEstimate()
        {
            //((EstimateViewModel)LayoutRoot.DataContext).SynchronizeCustomerDetails();
            RadWindow win = new RadWindow();
            SetEstimateStatus acceptDlg = new SetEstimateStatus(EstimateList.SelectedEstimateRevisionId, 2);
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Complete Estimate Window";
            win.Content = acceptDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.ShowDialog();
        }
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            SetEstimateStatus rejectDlg = new SetEstimateStatus(EstimateList.SelectedEstimateRevisionId, 3);

            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Reject Estimate Window";
            win.Content = rejectDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.ShowDialog();
        }

        void win_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                NavigationService.GoBack();
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            RadTabControl radtab = this.OptionTabControl.FindChildByType<RadTabControl>();

            try
            {
                TextBox txt = (TextBox)radtab.SelectedContent;
                RadSpellChecker.Check(txt, SpellCheckingMode.WordByWord);
            }
            catch (Exception ex)
            {
            }

            //if (radtab.SelectedIndex == 0)
            //{
            //    RadSpellChecker.Check(txt, SpellCheckingMode.WordByWord);
            //}
            //else if (radtab.SelectedIndex == 1)
            //{
            //    RadSpellChecker.Check(txt, SpellCheckingMode.WordByWord);
            //}
            //else if (radtab.SelectedIndex == 2)
            //{
            //    RadSpellChecker.Check(txt, SpellCheckingMode.WordByWord);
            //}
        }

        private void btnSetDocumentType_Click(object sender, RoutedEventArgs e)
        {
            RadWindow setCustomerDocWin = new RadWindow();
            SetCustomerDocument setCustomerDocDlg = new SetCustomerDocument(EstimateList.SelectedEstimateRevisionId, (App.Current as App).CurrentUserId);
            setCustomerDocWin.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            setCustomerDocWin.Header = ((EstimateViewModel)LayoutRoot.DataContext).CustomerDocumentType;
            setCustomerDocWin.Content = setCustomerDocDlg;
            setCustomerDocWin.Closed += new EventHandler<WindowClosedEventArgs>(setDocumentType_Closed);
            setCustomerDocWin.ShowDialog();
        }

        void setDocumentType_Closed(object sender, WindowClosedEventArgs e)
        {
            bool activePCDocument = (e.DialogResult ?? false);

            if (!activePCDocument && ((EstimateViewModel)LayoutRoot.DataContext).SetAsContractState)
            { 
                ((EstimateViewModel)LayoutRoot.DataContext).ShowSetAsContract = Visibility.Visible;
            }
            else
            {
                ((EstimateViewModel)LayoutRoot.DataContext).ShowSetAsContract = Visibility.Collapsed;
            }
            commentsWin_Closed(sender, e);
        }

        private void btnSetAsContract_Click(object sender, RadRoutedEventArgs e)
        {
            RadWindow setCustomerDocWin = new RadWindow();
            SetCustomerDocument setCustomerDocDlg = new SetCustomerDocument(EstimateList.SelectedEstimateRevisionId, (App.Current as App).CurrentUserId);
            setCustomerDocWin.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            setCustomerDocWin.Header = "Set As Contract";
            setCustomerDocDlg.Title = "Set As Contract";
            setCustomerDocWin.Content = setCustomerDocDlg;
            setCustomerDocWin.Closed += new EventHandler<WindowClosedEventArgs>(setAsContract_Closed);
            setCustomerDocWin.ShowDialog();
        }

        void setAsContract_Closed(object sender, WindowClosedEventArgs e)
        {
            bool activeContractDocument = (e.DialogResult ?? false);

            ((EstimateViewModel)LayoutRoot.DataContext).ShowSetDocumentType = activeContractDocument || !((EstimateViewModel)LayoutRoot.DataContext).SetAsPCState ? Visibility.Collapsed : Visibility.Visible;

            commentsWin_Closed(sender, e);
        }

        private void OptionsGrid_RowLoaded(object sender, RowLoadedEventArgs e)
        {
            HyperlinkButton hl;
            TextBlock tb;
            Image img;
            CheckBox ckbx;

            GridViewRow row = e.Row as GridViewRow;
            if (row != null)
            {
                EstimateDetails ed = row.DataContext as EstimateDetails;

                ed.ItemAllowToRemove = false;

                if (ed.PromotionProduct)
                {
                    ed.SOSI = "./images/promotion.png";
                    ed.SOSIToolTips = "Promotion items.";
                }

                string[] managerIds = ed.CreatedByUserManagerIds.Split(',');
                bool allow=false;
                if (row != null && ed != null)
                {
                    //based on 12/09/2012 feedback, everyone should be able to remove any item but need the reason.

                    if (!EstimateList.revisiontypepermission.ReadOnly) // studio M CC and remove everything
                    {
                        foreach (GridViewCell Cell in row.Cells)
                        {
                            if (Cell.FindChildByType<HyperlinkButton>() != null)
                            {
                                allow = ((EstimateViewModel)LayoutRoot.DataContext).IsRoleAllowtoAccessThisRevision;
                                hl = Cell.FindChildByType<HyperlinkButton>();
                                tb = Cell.FindChildByType<TextBlock>();
                                img = Cell.FindChildByType<Image>();
                                if (hl.Name == "btnCopy" || hl.Name == "btnReplace" || hl.Name == "btnRemoveMasterPromotion")
                                {
                                    if (hl.Name == "btnCopy" || hl.Name == "btnReplace" || (hl.Name == "btnRemoveMasterPromotion" && ed.IsMasterPromotion))
                                    {
                                        if (hl.Name == "btnReplace" && ed.IsMasterPromotion)
                                        {
                                            hl.IsEnabled = false;
                                            if (tb != null)
                                                tb.Opacity = 0.3;
                                            if (img != null)
                                                img.Opacity = 0.3;
                                        }
                                        else if (hl.Name == "btnRemoveMasterPromotion" && ed.IsMasterPromotion && ed.IsPrePackageItem && !isSalesEstimatingRevision)
                                        {
                                            hl.IsEnabled = false;
                                            if (tb != null)
                                                tb.Opacity = 0.3;
                                            if (img != null)
                                                img.Opacity = 0.3;
                                        }
                                        else
                                        {
                                            // check if IsPrePackageItem and if not sales estimating revision then disable it
                                            if (hl.Name == "btnReplace" &&
                                                ed.IsPrePackageItem && 
                                                (!isSalesEstimatingRevision || !allow)
                                               )
                                            {
                                                hl.IsEnabled = false;
                                                if (tb != null)
                                                    tb.Opacity = 0.3;
                                                if (img != null)
                                                    img.Opacity = 0.3;
                                            }
                                            else
                                            {
                                                hl.IsEnabled = true;
                                                if (tb != null)
                                                    tb.Opacity = 1;
                                                if (img != null)
                                                    img.Opacity = 1;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        hl.IsEnabled = false;
                                        if (tb != null)
                                            tb.Opacity = 0.3;
                                        if (img != null)
                                            img.Opacity = 0.3;
                                    }
                                }
                            }
                            if (Cell.FindChildByType<CheckBox>() != null)
                            {
                                ckbx = Cell.FindChildByType<CheckBox>();
                                
                                if (ckbx != null && (ckbx.Name == "chkAccept" || ckbx.Name == "chkDeleted"))
                                {
                                    ckbx.IsEnabled = true;
                                    if (ckbx.Name == "chkAccept" && !(App.Current as App).SelectedEstimateAllowToAcceptItem)
                                    {
                                        ckbx.Visibility = System.Windows.Visibility.Collapsed;
                                    }
                                    // check if IsPrePackageItem and if not sales estimator then disable it
                                    //else if (ckbx.Name == "chkDeleted" && ed.IsPrePackageItem && (App.Current as App).CurrentUserRoleId != 5)
                                    else if (ckbx.Name == "chkDeleted" && ed.IsPrePackageItem &&
                                           (!((App.Current as App).SelectedEstimateRevisionTypeID == 4 || (App.Current as App).SelectedEstimateRevisionTypeID == 15 || (App.Current as App).SelectedEstimateRevisionTypeID == 19 || (App.Current as App).SelectedEstimateRevisionTypeID == 25) ||
                                           !(allow && ((App.Current as App).CurrentUserRoleId == 5 || (App.Current as App).CurrentUserRoleId == 6 || (App.Current as App).CurrentUserRoleId == 57 || (App.Current as App).CurrentUserRoleId == 70 || (App.Current as App).CurrentUserRoleId == 79))
                                           )
                                        )
                                        ckbx.IsEnabled = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (GridViewCell Cell in row.Cells)
                        {
                            if (Cell.FindChildByType<HyperlinkButton>() != null)
                            {
                                hl = Cell.FindChildByType<HyperlinkButton>();
                                tb = Cell.FindChildByType<TextBlock>();
                                img = Cell.FindChildByType<Image>();
                                if (hl.Name == "btnCopy" || hl.Name == "btnReplace" || hl.Name == "btnRemoveMasterPromotion")
                                {
                                    hl.IsEnabled = false;
                                    if (tb != null)
                                        tb.Opacity = 0.3;
                                    if (img != null)
                                        img.Opacity = 0.3;
                                }
                            }
                            // some items may have multiple controls, so do not add a else for the previous line
                            if (Cell.FindChildByType<CheckBox>() != null)
                            {
                                ckbx = Cell.FindChildByType<CheckBox>();
                                if (ckbx != null && (ckbx.Name == "chkAccept" || ckbx.Name == "chkDeleted"))
                                {
                                    ckbx.IsEnabled = false;
                                    if (ckbx.Name == "chkAccept" && !(App.Current as App).SelectedEstimateAllowToAcceptItem)
                                    {
                                        ckbx.Visibility = System.Windows.Visibility.Collapsed;
                                    }
                                }
                            }
                        }
                    }
                    if ((App.Current as App).CurrentUserRoleId == 3 && !ed.ItemAccepted && ed.PreviousChanged)
                    {
                        row.Background = new SolidColorBrush(Color.FromArgb(75, 255, 204, 204));
                    }
                    else
                    {
                        row.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                    }
                }
            }
        }

        private void ClearAllExistingStudioMCollections()
        {

            //selectedaw.Clear();
            ////suplist.Clear();
            //awlist.Clear();
            //qulist.Clear();
        }
        private string GetStudioMQuestionByProduct(string pproductid)
        {
            string result = "";
            return result;
        }

        private void OptionsGrid_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
        {
            if (e.Visibility == Visibility.Visible)
            {
                GridViewRow row = e.Row as GridViewRow;

                if (row != null)
                {
                    EstimateDetails ed = row.DataContext as EstimateDetails;
                    selectediamgeid = ed.SelectedImageID;
                    string[] managerIds = ed.CreatedByUserManagerIds.Split(',');
                    decimal retailprice = 0;

                    if (row != null && ed != null)
                    {
                        StackPanel panel = (StackPanel)e.DetailsElement;
                        RadTabControl rtc = (RadTabControl)panel.FindName("tabDesc");
                        CheckBox chksitework = (CheckBox)panel.FindName("chkSitework");

                        //there is a bug in NSR standard desc, if there are more than one NSR, input blank for both standard desc, then enter one desc for first NSR, click save, the is nothing display on the grid
                        //after investigation found after select other tab then back to standard desc tab, the bug was gone. so the next two statements are doing the switch tab to extra desc then back to standard desc
                        // this fixes the bug.
                        rtc.SelectedIndex = 1;
                        rtc.SelectedIndex = 0;
                        currenttab = 0;
                        previoustab = 0;

                        //TextBox txtdesc = (TextBox)panel.FindName("txtDesc");
                        RadTabItem im = (RadTabItem)rtc.FindName("tabstandarddesc");
                        TextBox txtdesc = (TextBox)im.Content;

                        RadTabItem imadd = (RadTabItem)rtc.FindName("tabadditionaldesc");
                        Image addionalimage = (Image)imadd.FindName("imgAdditional");

                        if (ed.AdditionalNotes.Trim() != "")
                            addionalimage.Visibility = Visibility.Visible;
                        else
                            addionalimage.Visibility = Visibility.Collapsed;

                        RadTabItem imextra = (RadTabItem)rtc.FindName("tabextradesc");
                        Image extraimage = (Image)imadd.FindName("imgExtra");

                        if (ed.ExtraDescription.Trim() != "")
                            extraimage.Visibility = Visibility.Visible;
                        else
                            extraimage.Visibility = Visibility.Collapsed;

                        RadTabItem iminternal = (RadTabItem)rtc.FindName("tabinternaldesc");
                        Image internalimage = (Image)imadd.FindName("imgInternal");

                        if (ed.InternalDescription.Trim() != "")
                            internalimage.Visibility = Visibility.Visible;
                        else
                            internalimage.Visibility = Visibility.Collapsed;


                        RadComboBox cmbcategory = (RadComboBox)panel.FindName("cmbCategory");
                        //cmbcategory.ItemsSource = ((EstimateViewModel)LayoutRoot.DataContext).EstimateNonStandardCategory;
                        //cmbcategory.SelectedValue = ed.NonstandardCategoryID;
                        GetNonStandardAreas(cmbcategory, ed.NonstandardCategoryID);

                        RadComboBox cmbPriceDisplay = (RadComboBox)panel.FindName("cmbPriceDisplay");
                        cmbPriceDisplay.ItemsSource = ((EstimateViewModel)LayoutRoot.DataContext).EstimateNonStandardPriceDisplayCode;
                        cmbPriceDisplay.SelectedValue = ed.PriceDisplayCodeId;

                        RadComboBox cmbgroup = (RadComboBox)panel.FindName("cmbGroup");
                        //cmbgroup.ItemsSource = ((EstimateViewModel)LayoutRoot.DataContext).EstimateNonStandardGroup;
                        //cmbgroup.SelectedValue = ed.NonstandardGroupID;
                        GetNonStandardGroups(ed.NonstandardCategoryID, cmbgroup, ed.NonstandardGroupID);


                        if (ed.ItemAllowToChangeDisplayCode)
                        {
                            TextBlock txtPriceDisplay = (TextBlock)panel.FindName("txtPriceDisplay");
                            txtPriceDisplay.Visibility = System.Windows.Visibility.Collapsed;
                            cmbPriceDisplay.Visibility = System.Windows.Visibility.Visible;
                        }
                        else
                        {
                            TextBlock txtPriceDisplay = (TextBlock)panel.FindName("txtPriceDisplay");
                            txtPriceDisplay.Visibility = System.Windows.Visibility.Visible;
                            cmbPriceDisplay.Visibility = System.Windows.Visibility.Collapsed;
                        }

                        TextBox txtDBCCostExcGST = (TextBox)panel.FindName("txtDBCCostExcGST");
                        TextBlock lblDBCcost = (TextBlock)panel.FindName("lblDBCcost");

                        if ((App.Current as App).CurrentAction != "EDIT" || EstimateList.revisiontypepermission.ReadOnly) // View Mode
                        {
                            cmbcategory.IsEnabled = false;
                            chksitework.IsEnabled = false;
                            cmbgroup.IsEnabled = false;
                            cmbPriceDisplay.IsEnabled = false;

                            TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
                            txtPrice.IsReadOnly = true;
                            retailprice = decimal.Parse(txtPrice.Text);

                            TextBox txtQty = (TextBox)panel.FindName("txtQuantity");
                            txtQty.IsReadOnly = true;

                            txtDBCCostExcGST.IsReadOnly = true;

                            TextBox txtDesc = (TextBox)panel.FindName("txtDesc");
                            if (txtDesc != null)
                                txtDesc.IsReadOnly = true;

                            TextBox txtExtraDesc = (TextBox)panel.FindName("txtExtraDesc");
                            if (txtExtraDesc != null)
                                txtExtraDesc.IsReadOnly = true;

                            TextBox txtInternalDesc = (TextBox)panel.FindName("txtInternalDesc");
                            if (txtInternalDesc != null)
                                txtInternalDesc.IsReadOnly = true;

                            TextBox txtAdditionalNotes = (TextBox)panel.FindName("txtAdditionalNotes");
                            if (txtAdditionalNotes != null)
                                txtAdditionalNotes.IsReadOnly = true;

                            RadButton btnSave = (RadButton)panel.FindName("btnSave");
                            if (btnSave != null)
                                btnSave.Visibility = Visibility.Collapsed;

                            RadButton btnCancel = (RadButton)panel.FindName("btnCancel");
                            if (btnCancel != null)
                                btnCancel.Visibility = Visibility.Collapsed;

                            RadButton btnItemSpellCheck = (RadButton)panel.FindName("btnItemSpellCheck");
                            if (btnItemSpellCheck != null)
                                btnItemSpellCheck.Visibility = Visibility.Collapsed;

                            TextBox txtMarginBTPCost = (TextBox)panel.FindName("txtMarginBTPCost");
                            TextBlock lblMarginBTPCost = (TextBlock)panel.FindName("lblmarginBTPCost");
                            TextBox txtMarginDBCCost = (TextBox)panel.FindName("txtMarginDBCCost");
                            TextBlock lblMarginDBCCost = (TextBlock)panel.FindName("lblmarginDBCCost");
                            CheckBox derivedcost = (CheckBox)panel.FindName("chkDerivedCost");
                            TextBlock lblderivedcost = (TextBlock)panel.FindName("lblderivedcost");

                            bool showCostAndMargin = false;
                            if ((App.Current as App).CurrentUserRoleId == 6 || // Estimating Manager
                                (App.Current as App).CurrentUserRoleId == 57 || // Operation Manager
                                isSalesEstimatingRevision) 
                            {
                                showCostAndMargin = true;
                            }

                            if (((App)App.Current).CurrentRoleAccessModule.AccessMarginModule && showCostAndMargin)
                            {
                                if (ed.Margin.Trim() != "" && retailprice >= 0)
                                {
                                    decimal gst = decimal.Parse("1.1");
                                    decimal price = ed.Price;
                                    decimal costbtp = 0;
                                    decimal.TryParse(ed.UpdatedBTPCostExcGST, out costbtp);
                                    decimal marginbtp = Math.Round(100 * ((price / gst) - costbtp) / (price / gst), 2);
                                    txtMarginBTPCost.Text = marginbtp.ToString() + "%";
                                }
                                else
                                {
                                    txtMarginBTPCost.Text = "";
                                }
                                if (ed.Margin.Trim() != "" && retailprice >= 0)
                                {
                                    txtMarginDBCCost.Text = ed.Margin.ToString() + "%";
                                }
                                else
                                {
                                    txtMarginDBCCost.Text = "";
                                }
                                //if ((ed.AreaName.ToUpper() == "PROMOTION" || ed.GroupName.ToUpper().Replace(" ", "") == "PROMOTION-DISCOUNT" || ed.GroupName.ToUpper() == "DISCOUNT") && retailprice<0)
                                //{
                                //    lblMargin.Visibility = Visibility.Collapsed;
                                //    txtMargin.Visibility = Visibility.Collapsed;
                                //    lblCost.Visibility = Visibility.Collapsed;
                                //    txtDBCCostExcGST.Visibility = Visibility.Collapsed;
                                //    lblderivedcost.Visibility = Visibility.Collapsed;
                                //    derivedcost.Visibility = Visibility.Collapsed;

                                //}
                                //else
                                //{
                                lblMarginBTPCost.Visibility = Visibility.Visible;
                                txtMarginBTPCost.Visibility = Visibility.Visible;
                                lblMarginDBCCost.Visibility = Visibility.Visible;
                                txtMarginDBCCost.Visibility = Visibility.Visible;
                                lblDBCcost.Visibility = Visibility.Visible;
                                txtDBCCostExcGST.Visibility = Visibility.Visible;
                                lblderivedcost.Visibility = Visibility.Visible;
                                derivedcost.Visibility = Visibility.Visible;
                                //}
                            }
                            else
                            {
                                lblMarginBTPCost.Visibility = Visibility.Collapsed;
                                txtMarginDBCCost.Visibility = Visibility.Collapsed;
                                lblDBCcost.Visibility = Visibility.Collapsed;
                                txtDBCCostExcGST.Visibility = Visibility.Collapsed;
                                lblderivedcost.Visibility = Visibility.Collapsed;
                                derivedcost.Visibility = Visibility.Collapsed;

                            }
                        }
                        //else if (
                        //    ed.PromotionProduct || // Cannot modify Promotion Products
                        //    (
                        //    !ed.AreaName.ToUpper().Contains("NON STANDARD REQUEST") && // Cannot modify Std Options created by other users
                        //    !ed.AreaName.ToUpper().Contains("SITE WORKS & CONNECTIONS") &&
                        //    ed.CreatedByUserId != (App.Current as App).CurrentUserId &&
                        //    !managerIds.Contains<string>((App.Current as App).CurrentUserId.ToString()) // Managers can modify Std options created by their team member
                        //    )     
                        //    )
                        else
                        {
                            if (ed.AreaId == 43)
                            {
                                cmbcategory.IsEnabled = true;
                                cmbgroup.IsEnabled = true;

                                if (isSalesEstimatingRevision)
                                {
                                    txtdesc.IsReadOnly = false;
                                    chksitework.IsEnabled = true;
                                }
                                else
                                {
                                    txtdesc.IsReadOnly = ed.IsPrePackageItem;
                                    chksitework.IsEnabled = !ed.IsPrePackageItem;
                                }
                            }
                            else
                            {
                                cmbcategory.IsEnabled = false;
                                chksitework.IsEnabled = isSalesEstimatingRevision;  //true for Sales Estimating revisions, false for any other revision types
                                cmbgroup.IsEnabled = false;

                                if (ed.ItemAllowToChangeDescription)
                                {
                                    txtdesc.IsReadOnly = false;
                                }
                                else
                                {
                                    if (txtdesc != null)
                                        txtdesc.IsReadOnly = true;
                                }

                                //this condition reference to REQ-479
                                //if (ed.ItemAllowToChangePrice)
                                //{
                                //    TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
                                //    txtPrice.IsEnabled = true;
                                //}
                                //else
                                //{
                                //    if (ed.PriceDisplayCodeId != 10)
                                //    {
                                //        TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
                                //        txtPrice.IsEnabled = false;
                                //    }
                                //}
                            }

                            TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
                            retailprice = decimal.Parse(txtPrice.Text);

                            if (ed.ItemAllowToChangePrice && (!ed.IsPrePackageItem || isSalesEstimatingRevision))
                            {
                                txtPrice.IsReadOnly = false;
                            }
                            else
                            {
                                txtPrice.IsReadOnly = true;
                            }

                            TextBox txtQty = (TextBox)panel.FindName("txtQuantity");
                            if (ed.ItemAllowToChangeQuantity && (!ed.IsPrePackageItem || isSalesEstimatingRevision))
                            {
                                txtQty.IsReadOnly = false;
                            }
                            else
                            {
                                txtQty.IsReadOnly = true;
                            }

                            TextBox txtMarginBTPCost = (TextBox)panel.FindName("txtMarginBTPCost");
                            TextBlock lblMarginBTPCost = (TextBlock)panel.FindName("lblmarginBTPCost");

                            if ((App.Current as App).CurrentRoleAccessModule.AccessMarginModule && ed.ItemAllowToChangePrice && isSalesEstimatingRevision)// only sales estimator can change cost
                            {
                                txtDBCCostExcGST.IsReadOnly = false;
                                txtMarginBTPCost.IsEnabled = true;
                            }
                            else
                            {
                                txtDBCCostExcGST.IsReadOnly = true;
                                txtMarginBTPCost.IsEnabled = false;
                            }

                            TextBox txtMarginDBCCost = (TextBox)panel.FindName("txtMarginDBCCost");
                            TextBlock lblMarginDBCCost = (TextBlock)panel.FindName("lblmarginDBCCost");

                            if ((App.Current as App).CurrentRoleAccessModule.AccessMarginModule && ed.ItemAllowToChangePrice && isSalesEstimatingRevision)// only sales estimator can change cost
                            {
                                txtDBCCostExcGST.IsReadOnly = false;
                                txtMarginDBCCost.IsEnabled = true;
                            }
                            else
                            {
                                txtDBCCostExcGST.IsReadOnly = true;
                                txtMarginDBCCost.IsEnabled = false;
                            }

                            CheckBox derivedcost = (CheckBox)panel.FindName("chkDerivedCost");
                            TextBlock lblderivedcost = (TextBlock)panel.FindName("lblderivedcost");

                            bool showCostAndMargin = false;
                            if ((App.Current as App).CurrentUserRoleId == 6 || // Estimating Manager
                                (App.Current as App).CurrentUserRoleId == 57 || // Operation Manager
                                isSalesEstimatingRevision)
                            {
                                showCostAndMargin = true;
                            }

                            if (((App)App.Current).CurrentRoleAccessModule.AccessMarginModule && showCostAndMargin)
                            {
                                if (ed.Margin.Trim() != "" && retailprice >= 0)
                                {
                                    txtMarginBTPCost.Text = ed.Margin.ToString() + "%";
                                }
                                else
                                {
                                    txtMarginBTPCost.Text = "";
                                }
                                if (ed.MarginDBCCost != null && ed.MarginDBCCost.Trim() != "" && retailprice >= 0)
                                {
                                    txtMarginDBCCost.Text = ed.MarginDBCCost.ToString() + "%";
                                }
                                else
                                {
                                    txtMarginDBCCost.Text = "";
                                }

                                //if ((ed.AreaName.ToUpper() == "PROMOTION" || ed.GroupName.ToUpper().Replace(" ", "") == "PROMOTION-DISCOUNT" || ed.GroupName.ToUpper() == "DISCOUNT") && retailprice<0)
                                //{
                                //    lblMargin.Visibility = Visibility.Collapsed;
                                //    txtMargin.Visibility = Visibility.Collapsed;
                                //    lblCost.Visibility = Visibility.Collapsed;
                                //    txtDBCCostExcGST.Visibility = Visibility.Collapsed;
                                //    lblderivedcost.Visibility = Visibility.Collapsed;
                                //    derivedcost.Visibility = Visibility.Collapsed;
                                //}
                                //else
                                //{
                                lblMarginBTPCost.Visibility = Visibility.Visible;
                                txtMarginBTPCost.Visibility = Visibility.Visible;
                                lblMarginDBCCost.Visibility = Visibility.Visible;
                                txtMarginDBCCost.Visibility = Visibility.Visible;
                                lblDBCcost.Visibility = Visibility.Visible;
                                txtDBCCostExcGST.Visibility = Visibility.Visible;
                                lblderivedcost.Visibility = Visibility.Visible;
                                derivedcost.Visibility = Visibility.Visible;

                                //}
                            }
                            else
                            {
                                lblMarginBTPCost.Visibility = Visibility.Collapsed;
                                txtMarginBTPCost.Visibility = Visibility.Collapsed;
                                lblMarginDBCCost.Visibility = Visibility.Collapsed;
                                txtMarginDBCCost.Visibility = Visibility.Collapsed;
                                lblDBCcost.Visibility = Visibility.Collapsed;
                                txtDBCCostExcGST.Visibility = Visibility.Collapsed;
                                lblderivedcost.Visibility = Visibility.Collapsed;
                                derivedcost.Visibility = Visibility.Collapsed;

                            }


                            TextBox txtExtraDesc = (TextBox)panel.FindName("txtExtraDesc");
                            if (txtExtraDesc != null) {
                                txtExtraDesc.IsReadOnly = false;
                            }

                            TextBox txtInternalDesc = (TextBox)panel.FindName("txtInternalDesc");
                            if (txtInternalDesc != null)
                            {
                                txtInternalDesc.IsReadOnly = false;
                            }
                            TextBox txtAdditionalNotes = (TextBox)panel.FindName("txtAdditionalNotes");
                            if (txtAdditionalNotes != null)
                            {
                                if (!ed.IsPrePackageItem || isSalesEstimatingRevision)
                                    txtAdditionalNotes.IsReadOnly = false;
                                else
                                    txtAdditionalNotes.IsReadOnly = true;
                            }
                        }

                        if ((ed.Uom == "NT" || cmbPriceDisplay.SelectedValue.ToString() == "5") && (string.IsNullOrWhiteSpace(txtDBCCostExcGST.Text.ToString())))
                            txtDBCCostExcGST.Text = "0";

                        TextBox txtSubtotal = (TextBox)panel.FindName("txtSubtotal");
                        txtSubtotal.IsReadOnly = true;

                        CheckBox chkAccepted = (CheckBox)panel.FindName("chkAccepted");
                        chkAccepted.IsChecked = ed.ItemAccepted;

                        //if (ed.AreaName.ToUpper().Contains("NON STANDARD REQUEST") || ed.AreaName.ToUpper().Contains("SITE WORKS & CONNECTIONS") || (App.Current as App).CurrentUserRoleId==62)
                        if (!EstimateList.revisiontypepermission.ReadOnly && (App.Current as App).SelectedEstimateAllowToAcceptItem)
                        {
                            chkAccepted.IsEnabled = true;
                        }
                        else
                        {
                            chkAccepted.IsEnabled = false;
                        }

                        if ((ed.StudioMProduct || ed.AreaId == 43) && EstimateList.revisiontypepermission.AllowToViewStudioMTab) // this is for studioM product or NSR and all studio M roles
                        {
                            //TBD RadTabItem rti = (RadTabItem)panel.FindName("studiomtab");
                            //RadButton rbn = (RadButton)panel.FindName("btnSave");
                            //rbn.IsEnabled = false;

                            //TBD rti.Visibility = Visibility.Visible;

                            //studiomquestionxml = ed.StudioMQuestion;
                            //selectedstudiomanswer = ed.StudioMAnswer;
                            //studiomanswerxml = ed.StudioMAnswer;
                        }

                        //LoadProductImage(ed.ProductId);
                    }

                }
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            PrintPreview previewDlg = new PrintPreview(EstimateList.SelectedEstimateRevisionId, EstimateList.SelectedRevisionTypeID, EstimateList.IsMilestoneRevisionSelected);
            win.Header = "Print Preview";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = previewDlg;
            win.ShowDialog();
        }

        private void btnComments_Click(object sender, RoutedEventArgs e)
        {
            RadWindow commentsWin = new RadWindow();
            ModifyComments commentsDlg = new ModifyComments(EstimateList.SelectedEstimateRevisionId);
            commentsWin.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            commentsWin.Header = "Edit Comment Window";
            commentsWin.Content = commentsDlg;
            commentsWin.Closed += new EventHandler<WindowClosedEventArgs>(commentsWin_Closed);
            commentsWin.ShowDialog();
        }

        void commentsWin_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            //Refresh comments
            if (result.HasValue && result.Value)
                ((EstimateViewModel)LayoutRoot.DataContext).GetComments();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            //Refresh page with edit enabled
            (App.Current as App).CurrentAction = "EDIT";
            EstimateList.revisiontypepermission.ReadOnly = false;
            NavigationService.Refresh();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int applytoall = 0;
            string imageid = "";
            RadButton btn = (RadButton)sender;
            EstimateDetails item = (EstimateDetails)btn.DataContext;
            StackPanel panel = btn.ParentOfType<StackPanel>();

            StackPanel parentpanel = panel.ParentOfType<StackPanel>();

            RadTabControl rtc = (RadTabControl)parentpanel.FindName("tabDesc");

            TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
            TextBox txtQty = (TextBox)panel.FindName("txtQuantity");
            TextBox txtSubtotal = (TextBox)panel.FindName("txtSubtotal");
            CheckBox chkaccept = (CheckBox)panel.FindName("chkAccepted");
            RadComboBox cmbcategory = (RadComboBox)panel.FindName("cmbCategory");
            RadComboBox cmbgroup = (RadComboBox)panel.FindName("cmbGroup");
            RadComboBox cmbpricedisplay = (RadComboBox)panel.FindName("cmbPriceDisplay");
            CheckBox chksitework = (CheckBox)panel.FindName("chkSitework");
            TextBox txtBTPCost = (TextBox)panel.FindName("txtBTPCostExcGST");
            TextBox txtDBCCost = (TextBox)panel.FindName("txtDBCCostExcGST");
            // get apply to all checkbox value
            RadTabItem im = (RadTabItem)rd.FindName("studiomtab");
            ScrollViewer sv2 = (ScrollViewer)im.Content;
            //Grid gr = (Grid)im.Content;
            Grid gr2 = (Grid)sv2.Content; //(Grid)sv2.FindName("studiomgrid");
            //Grid gr2 = (Grid)im.Content;
            Grid gr = null;

            foreach (UIElement elem in gr2.Children)
            {
                if (elem is Grid)
                {
                    gr = (Grid)elem;
                }
            }
            if (gr != null)
            {
                foreach (UIElement elem in gr.Children)
                {
                    if (elem is CheckBox)
                    {
                        if (((CheckBox)elem).Name == "chkAll" && (bool)((CheckBox)elem).IsChecked)
                        {
                            applytoall = 1;
                            break;
                        }
                        else
                        {
                            applytoall = 0;
                        }
                    }
                }
            }
            // get selected image id
            RadTabItem im2 = (RadTabItem)rtc.FindName("phototab");
            ScrollViewer sv = (ScrollViewer)im2.Content;
            Grid grimage = (Grid)sv.Content;


            foreach (UIElement elem in grimage.Children)
            {
                if (elem is RadioButton)
                {
                    if ((bool)((RadioButton)elem).IsChecked)
                    {
                        imageid = ((RadioButton)elem).Name;
                        break;
                    }
                }
            }

            int categoryid = int.Parse(cmbcategory.SelectedValue.ToString());
            int groupid = int.Parse(cmbgroup.SelectedValue.ToString());
            int pricedisplayid = int.Parse(cmbpricedisplay.SelectedValue.ToString());

            decimal quantity = -1;
            decimal costbtp = -1;
            decimal costdbc = -1;
            string costbtpresult = "";
            string costdbcresult = "";
            decimal price = -1;
            decimal total = -1;

            if (!ValidateStudioMAttributes())
                return;

            try
            {
                quantity = decimal.Parse(txtQty.Text);
                //if (quantity <= 0)
                //{
                //    RadWindow.Alert("Please enter a valid quantity!");
                //    return;
                //}
            }
            catch (Exception)
            {
                RadWindow.Alert("Please enter a valid quantity!");
                return;
            }

            if (txtBTPCost.Text.Trim() != "")
            {
                try
                {

                    costbtp = decimal.Parse(txtBTPCost.Text);
                    costbtpresult = costbtp.ToString();
                }
                catch (Exception)
                {
                    RadWindow.Alert("Please enter a valid btp cost!");
                    return;
                }
            }
            else
            {
                costbtpresult = "";
            }
            if (txtDBCCost.Text.Trim() != "")
            {
                try
                {

                    costdbc = decimal.Parse(txtDBCCost.Text);
                    costdbcresult = costdbc.ToString();
                }
                catch (Exception)
                {
                    RadWindow.Alert("Please enter a valid dbc cost!");
                    return;
                }
            }
            else
            {
                costdbcresult = "";
            }
            try
            {
                price = decimal.Parse(txtPrice.Text);
            }
            catch (Exception)
            {
                RadWindow.Alert("Please enter a valid price!");
                return;
            }

            try
            {
                total = decimal.Parse(txtSubtotal.Text);
            }
            catch (Exception)
            {
                RadWindow.Alert("Please enter a valid subtotal.");
                return;
            }

            int len = 0;
            if (item.AreaName.Length > 20)
            {
                len = 20;
            }
            else
            {
                len = item.AreaName.Length;
            }
            //if (EstimateList.SelectedRevisionTypeID == 4 || EstimateList.SelectedRevisionTypeID==2 )
            //{
            if ((categoryid == 0 || groupid == 0) && item.AreaId == 43)
            {
                RadWindow.Alert("Please select the Area and Group for Non Standard Item!");
                return;
            }
            //}
            //else
            //{
            //    if (categoryid == 0 && item.AreaName.ToUpper().Substring(0, len) == "NON STANDARD REQUEST")
            //    {
            //        RadWindow.Alert("Please select the Area for Non Standard Item!");
            //        return;
            //    }
            //}

            //if (pricedisplayid != 10 && item.AreaName.ToUpper().Substring(0, len) == "NON STANDARD REQUEST" && price != 0)
            if (pricedisplayid != 10 && price != 0)
            {
                RadWindow.Confirm("The selected Price Display Code does not allow Unit Price.\r\nDo you want to reset the Unit Price to 0?", new EventHandler<WindowClosedEventArgs>((s, args) =>
                {
                    if (args.DialogResult == true)
                    {
                        txtPrice.Text = "0";
                        txtSubtotal.Text = "0.00";
                        txtDBCCost.Text = "0";

                        btnSave_Click(sender, e);
                    }
                    else
                    {
                        RadWindow.Alert("Please change Price Display Code or Unit Price and try to save again.");
                        return;
                    }
                }));

                return;
            }

            RadTabItem stdt = (RadTabItem)rtc.FindName("tabstandarddesc");
            TextBox txtdesc = (TextBox)stdt.Content;

            RadTabItem stdt2 = (RadTabItem)rtc.FindName("tabadditionaldesc");
            TextBox txtdesc2 = (TextBox)stdt2.Content;
            if (txtdesc2.Text == "")
            {
                txtdesc2.Text = item.UpdatedAdditionalNotes;
            }

            RadTabItem stdt3 = (RadTabItem)rtc.FindName("tabextradesc");
            TextBox txtdesc3 = (TextBox)stdt3.Content;
            if (txtdesc3.Text == "")
            {
                txtdesc3.Text = item.UpdatedExtraDescription;
            }

            RadTabItem stdt4 = (RadTabItem)rtc.FindName("tabinternaldesc");
            TextBox txtdesc4 = (TextBox)stdt4.Content;
            if (txtdesc4.Text == "")
            {
                txtdesc4.Text = item.UpdatedInternalDescription;
            }
            string descShort = string.Empty;

            if (txtdesc != null)
            {
                descShort = txtdesc.Text;
                if (descShort.Length > 100)
                {
                    descShort = descShort.Substring(0, 100) + "...";
                }
            }
            item.SiteWorkItem = (bool)chksitework.IsChecked;
            item.UpdatedSiteWorkItem = (bool)chksitework.IsChecked;
            if (item.Changes == null)
                item.Changes = "";

            if (item.Changes != "NEW")
            {
                if (item.Price != price)
                {
                    if (item.Changes == "")
                    {
                        item.Changes = "PRC";
                    }
                    else if (!item.Changes.Contains("PRC"))
                    {
                        if (!item.Changes.Contains("DESC"))
                        {
                            item.Changes = item.Changes + "\r\nPRC";
                        }
                        else
                        {
                            item.Changes = item.Changes.Replace("DESC", "PRC\r\nDESC");
                        }
                    }
                }
            }
            item.UpdatedPrice = price;
            item.Price = price;
            item.SelectedImageID = imageid;

            if (item.Changes != "NEW")
            {
                if (item.Quantity != quantity)
                {
                    if (item.Changes == "")
                    {
                        item.Changes = "QTY";
                    }
                    else if (!item.Changes.Contains("QTY"))
                    {
                        item.Changes = "QTY\r\n" + item.Changes;
                    }
                }
            }
            item.UpdatedQuantity = quantity;
            item.Quantity = quantity;
            item.ItemAccepted = (bool)chkaccept.IsChecked;
            item.ProductDescriptionShort = descShort;
            if (item.Changes != "NEW")
            {
                if (item.ProductDescription != txtdesc.Text || item.AdditionalNotes != txtdesc2.Text || item.ExtraDescription != txtdesc3.Text || item.InternalDescription != txtdesc4.Text)
                {
                    if (item.Changes == "")
                    {
                        item.Changes = "DESC";
                    }
                    else if (!item.Changes.Contains("DESC"))
                    {
                        item.Changes = item.Changes + "\r\nDESC";
                    }
                }
            }
            if (item.AreaId == 43)
            {
                item.ProductDescription = txtdesc.Text;
                item.UpdatedProductDescription = txtdesc.Text;
            }
            item.TotalPrice = total;
            item.UpdatedTotalPrice = total;
            item.UpdatedPriceDisplayCodeId = pricedisplayid;

            if (txtDBCCost.Text == "")
            {
                item.Margin = "";
            }
            else
            {
                if (price != 0)
                {
                    item.Margin = (100 * (price / gst - costdbc) / (price / gst)).ToString("F");
                }
                else
                {
                    item.Margin = "";
                }
            }
            //work out area name include category
            string temp = item.AreaName;
            string temp2 = "";
            int start = temp.IndexOf("(");
            int end = temp.IndexOf(")");
            if (start > 0 && end > 0 && start <= end)
            {
                temp2 = temp.Substring(start, end - start + 1);
                temp = temp.Replace(temp2, "");
            }
            else
            {
                temp = item.AreaName;
            }
            if (item.AreaId == 43)
            {
                item.AreaName = cmbcategory.Text;
                item.GroupName  = cmbgroup.Text;
                item.UpdatedNonstandardCategoryID = categoryid;
                item.UpdatedNonstandardGroupID = groupid;
                item.UpdatedPriceDisplayCodeId = pricedisplayid;
            }


            if (item.StudioMProduct)
            {
                if (item.StudioMAnswer == "" && item.StudioMQuestion != null && item.StudioMQuestion != "")
                {
                    item.StudioMIcon = "./images/color_swatch.png";
                    item.StudioMTooltips = "Studio M Product. Question not answered yet.";
                }
                else
                {
                    item.StudioMIcon = "./images/green_box.png";
                    item.StudioMTooltips = "Studio M Product. Question answered.";
                }
            }

            if (item.UpdatedBTPCostExcGST != item.CostExcGST)
            {
                item.DerivedCostIcon = "./image/spacer.gif";
                item.DerivedCostTooltips = "";
            }
            if (item.UpdatedDBCCostExcGST != item.CostExcGST)
            {
                item.DerivedCostIcon = "./image/spacer.gif";
                item.DerivedCostTooltips = "";
            }

            if (costdbcresult != item.CostExcGST) // cost has been changed.
            {
                item.DerivedCost = false;
            }

            if (pricedisplayid == 5 && (string.IsNullOrWhiteSpace(costdbcresult)))
                costdbcresult = "0";
            item.CostExcGST = costdbcresult;
            item.UpdatedBTPCostExcGST = costbtpresult;
            item.UpdatedDBCCostExcGST = costdbcresult;

            item.MarginString = item.Margin;
            if (!string.IsNullOrWhiteSpace(item.MarginString))
                item.MarginString += "%";

            selectedstudiomanswer = item.StudioMAnswer;
            if (applytoall == 1)
            {
                ((EstimateViewModel)LayoutRoot.DataContext).UpdateStudioMAnswerToAllGroup(selectedstudiomanswer, item.ProductId, item.GroupName);
            }

            ((EstimateViewModel)LayoutRoot.DataContext).UpdateEstimateDetails(item, applytoall);

            switch ((((EstimateViewModel)LayoutRoot.DataContext)).SelectedTabIndex)
            {
                case 0:
                    if (!item.SiteWorkItem)// if current tab is Site works tab then remove it only when unticked
                        (((EstimateViewModel)LayoutRoot.DataContext)).SiteCostTab.Options.Remove(item);
                    break;
                case 1:
                    // if current tab is not site works then remove it only when ticked
                    if (item.SiteWorkItem)
                        (((EstimateViewModel)LayoutRoot.DataContext)).UpgradeTab.Options.Remove(item);
                    break;
                case 2:
                    // For NonStandardTab do not move based Site works as it must remain shown here
                    break;
                //case 3:
                //    // if current tab is not site works then remove it only when ticked
                //    if (item.SiteWorkItem)
                //        (((EstimateViewModel)LayoutRoot.DataContext)).StandardInclusionTab.Options.Remove(item);
                //    break;
                case 3:
                    // For PreviewTab do not move based Site works as it must remain shown here
                    break;
                default:
                    break;
            }
        
            CloseDetailsPanel(sender, e);
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            EstimateDetails item = (EstimateDetails)btn.DataContext;
            StackPanel panel = btn.ParentOfType<StackPanel>();
            item.UpdatedQuantity = item.Quantity;
            item.UpdatedPrice = item.Price;
            item.UpdatedTotalPrice = item.Price * item.Quantity;
            item.UpdatedExtraDescription = item.ExtraDescription;
            item.UpdatedProductDescription = item.ProductDescription;
            item.UpdatedInternalDescription = item.InternalDescription;
            item.UpdatedAdditionalNotes = item.AdditionalNotes;
            item.UpdatedNonstandardCategoryID = item.NonstandardCategoryID;
            item.UpdatedNonstandardGroupID = item.NonstandardGroupID;
            item.UpdatedSiteWorkItem = item.SiteWorkItem;
            item.UpdatedBTPCostExcGST = item.UpdatedBTPCostExcGST;
            item.UpdatedDBCCostExcGST = item.UpdatedDBCCostExcGST;

            TextBox txtQuantity = (TextBox)panel.FindName("txtQuantity");
            txtQuantity.Text = item.UpdatedQuantity.ToString();

            TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
            txtPrice.Text = item.UpdatedPrice.ToString();


            TextBox txtSubtotal = (TextBox)panel.FindName("txtSubtotal");
            txtSubtotal.Text = item.UpdatedTotalPrice.ToString("F");
            CloseDetailsPanel(sender, e);
            // close opened details panel


        }

        private void CloseDetailsPanel(object sender, RoutedEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;
            while (dep != null && !(dep is GridViewRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            if (dep != null && dep is GridViewRow)
            {
                GridViewRow row = (GridViewRow)dep;

                row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

                Button button = row.Cells[0].FindChildByType<Button>();
                if (button != null)
                {
                    button.Content = button.Content.ToString() == "+" ? "-" : "+";
                }
            }
        }

        private void RadGridView2_CellLoaded(object sender, CellEventArgs e)
        {
            if (e.Cell is GridViewCell)
            {
                GridViewColumn col = e.Cell.Column;
                if (col.UniqueName == "naColumn")
                {
                    EstimateDetails item = (EstimateDetails)e.Cell.DataContext;
                    if (item.HomeDisplayOptionId == 0)
                    {
                        Image img = (Image)e.Cell.Content;
                        if (img != null)
                            img.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Image img = (Image)e.Cell.Content;
                        if (img != null)
                            img.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
            }
        }


        private void tabDesc_SelectionChanged(object sender, RadSelectionChangedEventArgs e)
        {
            rd = (RadTabControl)sender;
            //RadTabControl rdc = (RadTabControl) e.OriginalSource;
            //EstimateDetails ed = rdc.DataContext as EstimateDetails;
            EstimateDetails ed = rd.DataContext as EstimateDetails;

            if (rd.SelectedIndex == 6)
            {
                // Audit Logs
                showAuditLog(ed);
            }
            else
            {
                if (rd.SelectedIndex == 5)
                {
                    RadBusyIndicator c1 = rd.Parent as RadBusyIndicator;
                    c1.IsBusy = true;
                }
                //SQSHome h = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as SQSHome;
                //Grid rc = ((RadTabControl)e.OriginalSource).Parent as Grid;
                //Grid g2 = (Grid)rc.Parent;
                //StackPanel sp = (StackPanel)g2.Parent;

                mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
                mrsClient.GetStudioMQuestionForAProductCompleted += delegate (object ob, GetStudioMQuestionForAProductCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        ed.StudioMQuestion = es.Result;
                    //RadTabItem im2 = (RadTabItem)rd.FindName("studiomtab");
                    //ScrollViewer sv2 = (ScrollViewer)im2.Content;
                    //RadBusyIndicator sbi = (RadBusyIndicator)sv2.FindName("StudioMBusyIndicator");
                    //sbi.IsBusy = false;

                    #region xml processing

                    studiomquestionxml = ed.StudioMQuestion;
                        selectedstudiomanswer = ed.StudioMAnswer;
                        studiomanswerxml = ed.StudioMAnswer;

                    // this is for photo tab

                    if (currenttab != rd.SelectedIndex)
                        {
                            previoustab = currenttab;
                        }
                        currenttab = rd.SelectedIndex;

                        if (previoustab == 5) //if privous tab is studioM
                    {
                            ContructAnswerXML();
                        }

                        if (rd.SelectedIndex == 4)
                        {

                            LoadProductImage(ed.ProductId, rd);

                        }
                        else if (rd.SelectedIndex == 5) // this is studioM tab
                    {
                            try
                            {
                                if (ed.AreaId == 43)
                                {
                                    studiomquestionxml = App.Current.Resources["nsrquestions"].ToString();
                                }

                                RadBusyIndicator sbi = rd.Parent as RadBusyIndicator;
                                sbi.IsBusy = false;

                                suplist = new ObservableCollection<StudioMSupplierBrand>();
                                qulist = new ObservableCollection<StudioMQuestion>();
                                awlist = new ObservableCollection<StudioMAnswer>();
                                selectedsup = new ObservableCollection<StudioMSupplierBrand>();
                                selectedqu = new ObservableCollection<StudioMQuestion>();
                                selectedaw = new ObservableCollection<StudioMAnswer>();
                            //ClearAllExistingStudioMCollections();
                            RadTabItem im = (RadTabItem)rd.FindName("studiomtab");

                                ScrollViewer sv = (ScrollViewer)im.Content;
                            //Grid gr = (Grid)im.Content;
                            //Grid gr = (Grid)sv.FindName("studiomgrid");
                            //object o = sv.FindName("studiomgrid");
                            object o = sv.Content;
                                Grid gr;
                                if (o == null)
                                {
                                    gr = new Grid();
                                    gr.Name = "studiomgrid";
                                }
                                else
                                {
                                    gr = (Grid)o;
                                }
                            // re build all controls in studiomgrid
                            gr.Children.Clear();

                                var ro1 = new RowDefinition { Height = new GridLength(10) };
                                gr.RowDefinitions.Add(ro1);

                                ro1 = new RowDefinition { Height = new GridLength(22) };
                                gr.RowDefinitions.Add(ro1);

                                ro1 = new RowDefinition { Height = new GridLength(20) };
                                gr.RowDefinitions.Add(ro1);

                                ro1 = new RowDefinition { Height = new GridLength() };
                                gr.RowDefinitions.Add(ro1);


                                var col_1 = new ColumnDefinition { Width = new GridLength(350) };
                                gr.ColumnDefinitions.Add(col_1);

                                col_1 = new ColumnDefinition { Width = new GridLength(10) };
                                gr.ColumnDefinitions.Add(col_1);

                                col_1 = new ColumnDefinition { Width = new GridLength(500) };
                                gr.ColumnDefinitions.Add(col_1);

                            //build QAGrid
                            Grid gr2 = new Grid();
                                gr2.Name = "QAGrid";

                                var col = new ColumnDefinition { Width = new GridLength(350) };
                                gr2.ColumnDefinitions.Add(col);

                                col = new ColumnDefinition { Width = new GridLength(10) };
                                gr2.ColumnDefinitions.Add(col);

                                col = new ColumnDefinition { Width = new GridLength(500) };
                                gr2.ColumnDefinitions.Add(col);

                                Border b = new Border();
                                Thickness tk = new Thickness();
                                tk.Top = 3;
                                b.VerticalAlignment = VerticalAlignment.Center;
                                b.Height = 3;
                                b.BorderThickness = tk;
                                b.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 240, 248, 255));

                                gr.Children.Add(b);
                                Grid.SetColumn(b, 0);
                                Grid.SetColumnSpan(b, 4);
                                Grid.SetRow(b, 2);



                                gr.Children.Add(gr2);
                                Grid.SetColumn(gr2, 0);
                                Grid.SetColumnSpan(gr2, 4);
                                Grid.SetRow(gr2, 3);

                            //ClearAllExistingStudioMCollections();
                            //populate the question/answer list and select answer
                            if (suplist.Count == 0)
                                {
                                    if (studiomquestionxml != "")
                                    {
                                        XDocument doc = new XDocument();
                                        doc = XDocument.Parse(studiomquestionxml);

                                    //IEnumerable<XElement> el = (from p in doc.Descendants("Brand") select p);
                                    IEnumerable<XElement> el = (from p in doc.Descendants("Brand")
                                                                    orderby (string)p.Attribute("name")
                                                                    select p);
                                    // set default value
                                    StudioMSupplierBrand ss = new StudioMSupplierBrand();
                                        ss.SupplierBrandID = "0";
                                        ss.SupplierBrandName = "Please Select ...";
                                        suplist.Add(ss);

                                        foreach (XElement sup in el)
                                        {
                                            ss = new StudioMSupplierBrand();
                                            ss.SupplierBrandID = sup.Attribute("id").Value;
                                            ss.SupplierBrandName = sup.Attribute("name").Value;
                                            suplist.Add(ss);


                                            IEnumerable<XElement> question = (from q in doc.Descendants("Question")
                                                                              where (string)q.Parent.Parent.Attribute("id") == sup.Attribute("id").Value
                                                                              select q);

                                            foreach (XElement qu in question)
                                            {
                                                bool m = false;
                                                if (qu.Attribute("mandatory") != null)
                                                {
                                                    if (qu.Attribute("mandatory").Value == "1")
                                                    {
                                                        m = true;
                                                    }
                                                    else
                                                    {
                                                        m = false;
                                                    }
                                                }
                                                else
                                                {
                                                    m = false;
                                                }


                                                StudioMQuestion sq = new StudioMQuestion();
                                                sq.QuestionID = qu.Attribute("id").Value;
                                                sq.QuestionText = qu.Attribute("text").Value;
                                                sq.QuestionType = qu.Attribute("type").Value;
                                                sq.Mandatory = m;
                                                sq.SupplierBrandID = ss.SupplierBrandID;
                                                sq.IsActive = true;
                                                qulist.Add(sq);


                                                IEnumerable<XElement> answer = (from aw in doc.Descendants("Answer")
                                                                                where (string)aw.Parent.Parent.Attribute("id") == qu.Attribute("id").Value &&
                                                                                (string)aw.Parent.Parent.Parent.Parent.Attribute("id") == sup.Attribute("id").Value
                                                                                orderby (string)aw.Attribute("text")
                                                                                select aw);

                                                if (sq.QuestionType.Equals("SINGLE SELECTION", StringComparison.OrdinalIgnoreCase))
                                                {
                                                    StudioMAnswer sa1 = new StudioMAnswer();
                                                    sa1.AnswerID = "0";
                                                    sa1.AnswerText = "Please Select...";
                                                    sa1.QuestionID = sq.QuestionID;
                                                    sa1.SupplierBrandID = ss.SupplierBrandID;
                                                    awlist.Add(sa1);
                                                }

                                                foreach (XElement aw in answer)
                                                {
                                                    StudioMAnswer sa = new StudioMAnswer();
                                                    sa.AnswerID = aw.Attribute("id").Value;
                                                    sa.AnswerText = aw.Attribute("text").Value;
                                                    sa.QuestionID = sq.QuestionID;
                                                    sa.SupplierBrandID = ss.SupplierBrandID;
                                                    awlist.Add(sa);

                                                }

                                            }
                                        }
                                    }
                                    else
                                    {
                                        Thickness m2 = new Thickness();
                                        m2.Bottom = 10;
                                        m2.Top = 30;
                                        m2.Left = 160;

                                        TextBlock tx3 = new TextBlock();
                                        tx3.Text = "There are no Studio M questions for this product.";
                                        tx3.HorizontalAlignment = HorizontalAlignment.Center;
                                        tx3.FontSize = 15;
                                        tx3.FontStyle = FontStyles.Italic;
                                        tx3.Foreground = new SolidColorBrush(Colors.Orange);
                                        tx3.Margin = m2;
                                        gr2.Children.Add(tx3);
                                        Grid.SetColumn(tx3, 0);
                                        Grid.SetColumnSpan(tx3, 3);
                                        Grid.SetRow(tx3, 0);
                                    }
                                }
                            //dynamic create controls 

                            if (suplist.Count > 0)
                                {
                                //grid row and column definition

                                if (selectedstudiomanswer != "")
                                    {
                                        populateselectedanswer();
                                    }

                                    foreach (StudioMSupplierBrand sp in selectedsup)
                                    {
                                        bool brandExists = false;
                                        foreach (StudioMSupplierBrand brd in suplist)
                                        {
                                            if (brd.SupplierBrandID == sp.SupplierBrandID)
                                            {
                                                brandExists = true;
                                                break;
                                            }
                                        }

                                        if (!brandExists)
                                        {
                                            StudioMSupplierBrand newB = new StudioMSupplierBrand();
                                            newB.SupplierBrandID = sp.SupplierBrandID;
                                            newB.SupplierBrandName = sp.SupplierBrandName;

                                            suplist.Add(newB);
                                        }
                                    }

                                    TextBlock tx1 = new TextBlock();
                                    tx1.Text = "Please Select Brand:";
                                    RadComboBox dropsuppler = new RadComboBox();
                                    dropsuppler.ItemsSource = suplist;
                                    dropsuppler.SelectedValuePath = "SupplierBrandID";
                                    dropsuppler.DisplayMemberPath = "SupplierBrandName";
                                    dropsuppler.Name = "cmbSupplier";
                                    dropsuppler.Width = 300;
                                    dropsuppler.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(dropsuppler_SelectionChanged);
                                    dropsuppler.HorizontalAlignment = HorizontalAlignment.Left;

                                // add controls to grid
                                if (!gr.Children.Contains(dropsuppler))
                                    {

                                        gr.Children.Add(tx1);
                                        Grid.SetColumn(tx1, 0);
                                        Grid.SetRow(tx1, 1);
                                        gr.Children.Add(dropsuppler);
                                        Grid.SetColumn(dropsuppler, 2);
                                        Grid.SetRow(dropsuppler, 1);

                                        if (selectedaw.Count == 0)
                                        {
                                            if (suplist.Count == 2)
                                            {
                                                dropsuppler.SelectedValue = suplist[1].SupplierBrandID;
                                            }
                                            else
                                            {
                                                dropsuppler.SelectedIndex = 0;
                                            }
                                        }
                                        else
                                        {
                                            dropsuppler.SelectedValue = selectedaw[0].SupplierBrandID;

                                            if (dropsuppler.SelectedValue == null)
                                                dropsuppler.SelectedIndex = 0;
                                        }

                                    }
                                }
                                if (!(App.Current as App).SelectedEstimateAllowToViewStudioMTab) // if user is not studiom ,, then lock the form
                            {
                                    foreach (UIElement elem in gr.Children)
                                    {
                                        if (elem is RadComboBox)
                                        {
                                            RadComboBox dpsup = (RadComboBox)elem;
                                            dpsup.IsEnabled = false;
                                            break;
                                        }
                                    }
                                    foreach (UIElement elem in gr2.Children)
                                    {
                                        if (elem is RadComboBox)
                                        {
                                            RadComboBox dp = (RadComboBox)elem;
                                            dp.IsEnabled = false;
                                        }
                                        else if (elem is CheckBox)
                                        {
                                            CheckBox ck = (CheckBox)elem;
                                            ck.IsEnabled = false;
                                        }
                                        else if (elem is TextBox)
                                        {
                                            TextBox txt = (TextBox)elem;
                                            txt.IsEnabled = false;
                                        }
                                    }

                                }

                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    #endregion

                }
                };
                mrsClient.GetStudioMQuestionForAProductAsync(ed.ProductId);
            }
        }

        private void DisplayImage(RadTabControl rd, List<ProductImage> li)
        {
            EstimateDetails ed = (EstimateDetails)rd.DataContext;
            Grid gr;
            ScrollViewer sv = (ScrollViewer)rd.FindName("imgscroll");
            gr = (Grid)sv.FindName("imagegrid");
            if (gr == null)
            {
                gr = new Grid();
                gr.Name = "imagegrid";
            }
            else
            {
                gr.Children.Clear();
            }

            if (gr != null)
            {
                //grid row and column definition
                Thickness m1 = new Thickness();
                m1.Bottom = 5;
                m1.Top = 1;
                m1.Left = 10;

                var rw = new RowDefinition { Height = new GridLength(145) };
                gr.RowDefinitions.Add(rw);
                gr.Margin = m1;
                //int imagecount=((EstimateViewModel)LayoutRoot.DataContext).ImageList.Count;

                if (li.Count > 0)
                {
                    int idx = 0;
                    // add no image column

                    RadButton rdbutton = new RadButton();
                    rdbutton.Content = "Clear";
                    rdbutton.Name = "btnClear" + ed.EstimateRevisionDetailsId.ToString();
                    rdbutton.Width = 70;
                    rdbutton.Height = 18;
                    rdbutton.AddHandler(RadButton.ClickEvent, new RoutedEventHandler(btnClear_Click));
                    if (EstimateList.revisiontypepermission.ReadOnly)
                    {
                        rdbutton.IsEnabled = false;
                    }

                    gr.Children.Add(rdbutton);
                    Grid.SetColumn(rdbutton, idx);
                    Grid.SetRow(rdbutton, 2);

                    idx = idx + 1;
                    // add images
                    foreach (ProductImage pi in li)
                    {

                        byte[] b = pi.image;
                        BitmapImage bi = new BitmapImage();
                        bi.SetSource(new MemoryStream(b, 0, b.Length));

                        Image img = new Image();
                        img.Source = bi;
                        img.Width = 180;
                        img.Height = 135;

                        img.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        img.Stretch = Stretch.Fill;

                        HyperlinkButton hl = new HyperlinkButton();
                        hl.Content = img;
                        hl.Click += new System.Windows.RoutedEventHandler(viewimage);

                        // add buttons to grid
                        gr.RowDefinitions.Add(new RowDefinition());
                        //gr.RowDefinitions.Add(new RowDefinition());

                        gr.ColumnDefinitions.Add(new ColumnDefinition());
                        gr.Children.Add(hl);
                        Grid.SetColumn(hl, idx);
                        Grid.SetRow(hl, 0);
                        // add supplier name label
                        TextBlock tx = new TextBlock();
                        tx.Text = pi.suppliername;
                        tx.TextWrapping = TextWrapping.Wrap;
                        tx.HorizontalAlignment = HorizontalAlignment.Center;
                        tx.VerticalAlignment = VerticalAlignment.Top;

                        TextBlock tx2 = new TextBlock();
                        tx2.Text = "(" + pi.imagename + ")";
                        tx2.TextWrapping = TextWrapping.Wrap;
                        tx2.HorizontalAlignment = HorizontalAlignment.Center;

                        gr.Children.Add(tx);
                        Grid.SetColumn(tx, idx);
                        Grid.SetRow(tx, 1);

                        gr.Children.Add(tx2);
                        Grid.SetColumn(tx2, idx);
                        Grid.SetRow(tx2, 2);

                        RadioButton rb = new RadioButton();
                        rb.Name = pi.imageID.ToString();
                        if (selectediamgeid == pi.imageID.ToString())
                        {
                            rb.IsChecked = true;
                        }
                        else
                        {
                            rb.IsChecked = false;
                        }
                        rb.HorizontalAlignment = HorizontalAlignment.Center;
                        if (EstimateList.revisiontypepermission.ReadOnly)
                        {
                            rb.IsEnabled = false;
                        }
                        // add buttons to grid
                        gr.RowDefinitions.Add(new RowDefinition());
                        gr.RowDefinitions.Add(new RowDefinition());

                        gr.ColumnDefinitions.Add(new ColumnDefinition());
                        gr.Children.Add(rb);
                        Grid.SetColumn(rb, idx);
                        Grid.SetRow(rb, 3);

                        idx = idx + 1;

                    }
                }
                else
                {
                    Thickness m2 = new Thickness();
                    m2.Bottom = 10;
                    m2.Top = 60;
                    m2.Left = 300;

                    TextBlock tx3 = new TextBlock();
                    tx3.Text = "There are no photos for this product.";
                    tx3.HorizontalAlignment = HorizontalAlignment.Center;
                    tx3.FontSize = 15;
                    tx3.FontStyle = FontStyles.Italic;
                    tx3.Foreground = new SolidColorBrush(Colors.Orange);
                    tx3.Margin = m2;
                    gr.Children.Add(tx3);
                    Grid.SetColumn(tx3, 0);
                    Grid.SetRow(tx3, 0);
                }
            }
        }

        private void populateselectedanswer()
        {
            try
            {
                XDocument doc = new XDocument();
                doc = XDocument.Parse(selectedstudiomanswer);
                string selectedsupplierid, selectedquestionid;

                IEnumerable<XElement> el = (from p in doc.Descendants("Brand")
                                            orderby (string)p.Attribute("name")
                                            select p);
                foreach (XElement sup in el)
                {
                    StudioMSupplierBrand su = new StudioMSupplierBrand();
                    su.SupplierBrandID = sup.Attribute("id").Value;
                    su.SupplierBrandName = sup.Attribute("name").Value;
                    selectedsup.Add(su);

                    selectedsupplierid = sup.Attribute("id").Value;

                    IEnumerable<XElement> question = (from q in doc.Descendants("Question") where (string)q.Parent.Parent.Attribute("id") == selectedsupplierid select q);
                    foreach (XElement qu in question)
                    {
                        StudioMQuestion q = new StudioMQuestion();
                        q.QuestionID = qu.Attribute("id").Value;
                        q.QuestionText = qu.Attribute("text").Value;
                        q.QuestionType = qu.Attribute("type").Value;
                        q.SupplierBrandID = selectedsupplierid;
                        q.Mandatory = false;

                        selectedqu.Add(q);

                        selectedquestionid = qu.Attribute("id").Value;

                        IEnumerable<XElement> answer = (from aw in doc.Descendants("Answer")
                                                        where (string)aw.Parent.Parent.Attribute("id") == selectedquestionid &&
                                                        (string)aw.Parent.Parent.Parent.Parent.Attribute("id") == selectedsupplierid
                                                        orderby (string)aw.Attribute("text")
                                                        select aw);


                        //StudioMAnswer sa1 = new StudioMAnswer();
                        //sa1.AnswerID = "0";
                        //sa1.AnswerText = "Please Select...";
                        //sa1.QuestionID = selectedquestionid;
                        //sa1.SupplierBrandID = selectedsupplierid;
                        //selectedaw.Add(sa1);
                        foreach (XElement aw in answer)
                        {
                            StudioMAnswer sa = new StudioMAnswer();
                            sa.AnswerID = aw.Attribute("id").Value;
                            sa.AnswerText = aw.Attribute("text").Value;
                            sa.QuestionID = selectedquestionid;
                            sa.SupplierBrandID = selectedsupplierid;
                            selectedaw.Add(sa);
                        }

                    }
                }
            }
            catch (Exception)
            {
                selectedaw.Clear();
            }
        }
        void dropanswer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count > 0)
            //    RecordingDetails.DataContext = e.AddedItems[0];

        }
        private void dropsuppler_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            RadComboBox dsup = (RadComboBox)sender;
            ObservableCollection<StudioMAnswer> selectedanswerforthisquestion = new ObservableCollection<StudioMAnswer>();
            string selectedsupplierid;

            if (dsup.SelectedValue != null)
            {
                selectedsupplierid = dsup.SelectedValue.ToString();

                Grid gr = (Grid)(dsup.ParentOfType<Grid>()).FindName("QAGrid");

                int rowindex = 0;

                if (gr != null)
                {
                    gr.Children.Clear();
                    gr.RowDefinitions.Clear();

                    //Loop thru all questions in StudioMAttributes
                    foreach (StudioMQuestion ques in selectedqu)
                    {
                        //Check whether the selected question is still in StudioMQAndA
                        bool questionExists = false;
                        foreach (StudioMQuestion q in qulist)
                        {
                            if (q.QuestionID == ques.QuestionID && q.SupplierBrandID == ques.SupplierBrandID)
                            {
                                questionExists = true;
                                break;
                            }
                        }

                        //If the question in StudioMAttributes is no longer in StudioMQAndA
                        if (!questionExists)
                        {
                            //Check whether this question is answered
                            bool questionAnswered = false;
                            foreach (StudioMAnswer ans in selectedaw)
                            {
                                if (ans.QuestionID == ques.QuestionID && !string.IsNullOrEmpty(ans.AnswerText))
                                {
                                    questionAnswered = true;
                                    break;
                                }
                            }

                            //Only add inactive question to Studio M tab when it's been answered
                            if (questionAnswered)
                            {
                                StudioMQuestion newQ = new StudioMQuestion();
                                newQ.QuestionID = ques.QuestionID;
                                newQ.QuestionText = ques.QuestionText;
                                newQ.QuestionType = ques.QuestionType;
                                newQ.SupplierBrandID = ques.SupplierBrandID;
                                newQ.Mandatory = ques.Mandatory;
                                newQ.IsActive = false;

                                qulist.Add(newQ);
                            }
                        }
                    }

                    //Add answers that are not in the StudioMQandA
                    foreach (StudioMAnswer ans in selectedaw)
                    {
                        bool answerExists = false;
                        foreach (StudioMAnswer a in awlist)
                        {
                            if (a.QuestionID == ans.QuestionID && a.AnswerID == ans.AnswerID && a.SupplierBrandID == ans.SupplierBrandID)
                            {
                                answerExists = true;
                                break;
                            }
                        }

                        if (!answerExists)
                        {
                            StudioMAnswer newA = new StudioMAnswer();
                            newA.AnswerID = ans.AnswerID;
                            newA.AnswerText = ans.AnswerText;
                            newA.QuestionID = ans.QuestionID;
                            newA.SupplierBrandID = ans.SupplierBrandID;

                            awlist.Add(newA);
                        }
                    }

                    foreach (StudioMQuestion q in qulist)
                    {
                        if (q.SupplierBrandID == selectedsupplierid)
                        {
                            ObservableCollection<StudioMAnswer> filteredAnser = new ObservableCollection<StudioMAnswer>();
                            //StudioMAnswer a1 = new StudioMAnswer();
                            //a1.AnswerID = "0";
                            //a1.AnswerText = "Please Select...";
                            //a1.QuestionID = q.QuestionID;
                            //a1.SupplierBrandID = q.SupplierBrandID;
                            //filteredAnser.Add(a1);

                            foreach (StudioMAnswer a in awlist)
                            {
                                if (a.SupplierBrandID == selectedsupplierid && a.QuestionID == q.QuestionID)
                                {
                                    if (!filteredAnser.Contains(a))
                                    {
                                        filteredAnser.Add(a);
                                    }
                                }
                            }

                            selectedanswerforthisquestion.Clear();
                            foreach (StudioMAnswer a in selectedaw)
                            {
                                if (a.QuestionID == q.QuestionID && a.SupplierBrandID == selectedsupplierid)
                                {
                                    if (!selectedanswerforthisquestion.Contains(a))
                                    {
                                        selectedanswerforthisquestion.Add(a);
                                    }
                                }
                            }

                            RowDefinition questionRowDef;
                            string questions = (q.QuestionText.Replace("(Multiple Selection)", "")).Replace("(Single Selection)", "");
                            int h = ((questions.Length / 60) + 1) * 22;
                            questionRowDef = new RowDefinition { Height = new GridLength(h) };
                            gr.RowDefinitions.Add(questionRowDef);
                            //if (questions.Length <= 60)
                            //{
                            //    rw22 = new RowDefinition { Height = new GridLength(20) };
                            //}
                            //else if (questions.Length > 60 && questions.Length <= 120)
                            //{
                            //    rw22 = new RowDefinition { Height = new GridLength(40) };

                            //}
                            //else
                            //{
                            //    rw22 = new RowDefinition { Height = new GridLength(60) };
                            //}
                            switch (q.QuestionType.ToUpper())
                            {
                                case "SINGLE SELECTION":
                                    //create question text
                                    TextBlock tx = new TextBlock();
                                    if (q.Mandatory)
                                    {
                                        tx.Text = q.QuestionText.Replace("(Single Selection)", "") + "*";
                                    }
                                    else
                                    {
                                        tx.Text = q.QuestionText.Replace("(Single Selection)", "");
                                    }


                                    tx.TextWrapping = TextWrapping.Wrap;
                                    tx.VerticalAlignment = VerticalAlignment.Center;
                                    gr.Children.Add(tx);

                                    Grid.SetRow(tx, rowindex);
                                    Grid.SetColumn(tx, 0);
                                    /*
                                    //create answer dropdown
                                    //ObservableCollection<string> sourcetext = new ObservableCollection<string>();
                                    //foreach (StudioMAnswer item in filteredAnser)
                                    //{
                                    //    sourcetext.Add(item.AnswerText.ToString());
                                    //}

                                    //RadAutoCompleteBox dropanswer = new RadAutoCompleteBox();
                                    //dropanswer.Name = "single_selection_" + q.QuestionID;
                                    //dropanswer.WatermarkContent = "Type to search...";
                                    //dropanswer.TextSearchMode = TextSearchMode.Contains;
                                    //dropanswer.AutoCompleteMode = AutoCompleteMode.Suggest;
                                    //dropanswer.SelectionMode =  AutoCompleteSelectionMode.Single;
                                    //dropanswer.TextSearchPath = "AnswerText";
                                    //AutoCompleteBox dropanswer = new AutoCompleteBox();
                                    //dropanswer.Name = "single_selection_" + q.QuestionID;
                                    //dropanswer.FilterMode = AutoCompleteFilterMode.Contains;
                                    //dropanswer.ItemsSource = sourcetext;
                                    //dropanswer.SelectionChanged += new Telerik.Windows.Controls.SelectionChangedEventHandler(dropanswer_SelectionChanged);
                                    //dropanswer.Width = 300;

                                   
                                    //if (selectedanswerforthisquestion.Count > 0 && selectedanswerforthisquestion[0].AnswerID != "")
                                    //{
                                        
                                    //    dropanswer.SelectedItem = selectedanswerforthisquestion[0].AnswerText; // single selection only has one answer
                                    //}
                
                                    ////else
                                    ////{
                                    ////    dropanswer.SelectedItem = "";
                                    ////}
                                    //dropanswer.VerticalAlignment = VerticalAlignment.Center;
                                    //dropanswer.HorizontalAlignment = HorizontalAlignment.Left;

                                    //gr.Children.Add(dropanswer);
                                    //Grid.SetRow(dropanswer, rowindex);
                                    //Grid.SetColumn(dropanswer, 2);
                                     * */


                                    RadComboBox dropanswer = new RadComboBox();
                                    //StudioMAnswer a1 = new StudioMAnswer();
                                    //a1.AnswerID = "0";
                                    //a1.AnswerText = "Please Select...";
                                    //a1.QuestionID = q.QuestionID;
                                    //a1.SupplierBrandID = q.SupplierBrandID;
                                    //filteredAnser.Add(a1);
                                    dropanswer.ItemsSource = filteredAnser;
                                    dropanswer.SelectedValuePath = "AnswerID";
                                    dropanswer.DisplayMemberPath = "AnswerText";
                                    dropanswer.Width = 300;
                                    dropanswer.Height = 20;

                                    dropanswer.HorizontalAlignment = HorizontalAlignment.Left;
                                    dropanswer.Name = "single_selection_" + q.QuestionID;


                                    if (selectedanswerforthisquestion.Count == 1 && selectedanswerforthisquestion[0].AnswerID != "")
                                    {
                                        dropanswer.SelectedValue = selectedanswerforthisquestion[0].AnswerID; // single selection only has one answer
                                    }
                                    //else if (selectedanswerforthisquestion.Count > 1 && selectedanswerforthisquestion[1].AnswerID != "")
                                    //{
                                    //    dropanswer.SelectedValue = selectedanswerforthisquestion[0].AnswerID;
                                    //}
                                    else
                                    {

                                        if (q.Mandatory && filteredAnser.Count == 2)
                                        {
                                            dropanswer.SelectedValue = filteredAnser[1].AnswerID;
                                        }
                                        else
                                        {
                                            dropanswer.SelectedValue = "0";
                                        }
                                    }
                                    dropanswer.VerticalAlignment = VerticalAlignment.Center;

                                    gr.Children.Add(dropanswer);
                                    Grid.SetRow(dropanswer, rowindex);
                                    Grid.SetColumn(dropanswer, 2);

                                    break;
                                case "MULTIPLE SELECTION":
                                    //create dropdown

                                    int index = 0;
                                    TextBlock tx2 = new TextBlock();
                                    if (q.Mandatory)
                                    {
                                        tx2.Text = q.QuestionText.Replace("(Multiple Selection)", "") + "*";
                                    }
                                    else
                                    {
                                        tx2.Text = q.QuestionText.Replace("(Multiple Selection)", "");
                                    }

                                    tx2.TextWrapping = TextWrapping.Wrap;
                                    tx2.VerticalAlignment = VerticalAlignment.Top;

                                    gr.Children.Add(tx2);
                                    Grid.SetRow(tx2, rowindex);
                                    Grid.SetColumn(tx2, 0);

                                    foreach (StudioMAnswer a in filteredAnser)
                                    {
                                        rowindex++;

                                        int answerRowHeight = 22;
                                        RowDefinition answerRowDef = new RowDefinition { Height = new GridLength(answerRowHeight) };
                                        gr.RowDefinitions.Add(answerRowDef);

                                        CheckBox chk = new CheckBox();
                                        chk.Content = a.AnswerText;
                                        chk.Name = "chk_" + q.QuestionID + "_" + index.ToString();
                                        chk.Height = 20;
                                        chk.VerticalAlignment = VerticalAlignment.Center;
                                        gr.Children.Add(chk);
                                        Grid.SetRow(chk, rowindex);
                                        Grid.SetColumn(chk, 0);

                                        foreach (StudioMAnswer ans in selectedanswerforthisquestion)
                                        {
                                            if (ans.AnswerID == a.AnswerID)
                                            {
                                                chk.IsChecked = true;
                                                break;
                                            }
                                        }

                                        index++;
                                    }

                                    break;
                                case "FREE TEXT":
                                    //create dropdown
                                    TextBlock tx3 = new TextBlock();
                                    if (q.Mandatory)
                                    {
                                        tx3.Text = q.QuestionText + "*";
                                    }
                                    else
                                    {
                                        tx3.Text = q.QuestionText;
                                    }
                                    tx3.TextWrapping = TextWrapping.Wrap;
                                    tx3.VerticalAlignment = VerticalAlignment.Center;
                                    gr.Children.Add(tx3);
                                    Grid.SetRow(tx3, rowindex);
                                    Grid.SetColumn(tx3, 0);

                                    TextBox tb = new TextBox();
                                    tb.Width = 300;
                                    tb.Height = 20;
                                    tb.HorizontalAlignment = HorizontalAlignment.Left;
                                    tb.Name = "freetext_" + q.QuestionID;

                                    if (selectedanswerforthisquestion.Count > 0 && selectedanswerforthisquestion[0].AnswerID != "")
                                    {
                                        tb.Text = selectedanswerforthisquestion[0].AnswerText; // single selection only has one answer
                                    }
                                    else
                                    {
                                        tb.Text = "";
                                    }
                                    tb.VerticalAlignment = VerticalAlignment.Center;
                                    gr.Children.Add(tb);
                                    Grid.SetRow(tb, rowindex);
                                    Grid.SetColumn(tb, 2);

                                    break;
                                case "INTEGER":
                                    //create dropdown
                                    TextBlock tx4 = new TextBlock();
                                    if (q.Mandatory)
                                    {
                                        tx4.Text = q.QuestionText + "*";
                                    }
                                    else
                                    {
                                        tx4.Text = q.QuestionText;
                                    }
                                    tx4.TextWrapping = TextWrapping.Wrap;
                                    tx4.VerticalAlignment = VerticalAlignment.Center;
                                    gr.Children.Add(tx4);
                                    Grid.SetRow(tx4, rowindex);
                                    Grid.SetColumn(tx4, 0);

                                    TextBox tb2 = new TextBox();
                                    tb2.Width = 300;
                                    tb2.Height = 20;
                                    tb2.HorizontalAlignment = HorizontalAlignment.Left;
                                    tb2.Name = "txt_int_" + q.QuestionID;

                                    if (selectedanswerforthisquestion.Count > 0 && selectedanswerforthisquestion[0].AnswerID != "")
                                    {
                                        tb2.Text = selectedanswerforthisquestion[0].AnswerText; // single selection only has one answer
                                    }
                                    else
                                    {
                                        tb2.Text = "";
                                    }
                                    tb2.VerticalAlignment = VerticalAlignment.Center;
                                    gr.Children.Add(tb2);
                                    Grid.SetRow(tb2, rowindex);
                                    Grid.SetColumn(tb2, 2);

                                    break;
                                case "DECIMAL":
                                    //create dropdown
                                    TextBlock tx5 = new TextBlock();
                                    if (q.Mandatory)
                                    {
                                        tx5.Text = q.QuestionText + "*";
                                    }
                                    else
                                    {
                                        tx5.Text = q.QuestionText;
                                    }
                                    tx5.TextWrapping = TextWrapping.Wrap;
                                    tx5.VerticalAlignment = VerticalAlignment.Center;
                                    gr.Children.Add(tx5);
                                    Grid.SetRow(tx5, rowindex);
                                    Grid.SetColumn(tx5, 0);

                                    TextBox tb3 = new TextBox();
                                    tb3.Width = 300;
                                    tb3.Height = 20;
                                    tb3.HorizontalAlignment = HorizontalAlignment.Left;
                                    tb3.Name = "decimal_" + q.QuestionID;

                                    if (selectedanswerforthisquestion.Count > 0 && selectedanswerforthisquestion[0].AnswerID != "")
                                    {
                                        tb3.Text = selectedanswerforthisquestion[0].AnswerText; // single selection only has one answer
                                    }
                                    else
                                    {
                                        tb3.Text = "";
                                    }
                                    tb3.VerticalAlignment = VerticalAlignment.Center;
                                    gr.Children.Add(tb3);
                                    Grid.SetRow(tb3, rowindex);
                                    Grid.SetColumn(tb3, 2);

                                    break;
                                default:
                                    break;
                            }

                            rowindex = rowindex + 1;
                            // var rw = new RowDefinition { Height = new GridLength(25, GridUnitType.Pixel) };

                            //gr.RowDefinitions.Add(rw);

                        }
                    }

                    // add apply answer to all group check box;

                    if (selectedsupplierid != "0")
                    {
                        //rowindex = rowindex + 1;

                        var rw2 = new RowDefinition { Height = new GridLength(20) };
                        gr.RowDefinitions.Add(rw2);
                        rw2 = new RowDefinition { Height = new GridLength(20) };
                        gr.RowDefinitions.Add(rw2);

                        TextBlock tbl = new TextBlock();
                        tbl.Text = "Apply these answers to same products in same group but other areas.";
                        tbl.TextWrapping = TextWrapping.Wrap;
                        tbl.HorizontalAlignment = HorizontalAlignment.Left;
                        tbl.FontStyle = FontStyles.Italic;
                        tbl.Foreground = new SolidColorBrush(Colors.Orange);
                        gr.Children.Add(tbl);
                        Grid.SetRow(tbl, rowindex);
                        Grid.SetRowSpan(tbl, 2);
                        Grid.SetColumn(tbl, 0);

                        //TextBlock tbl2 = new TextBlock();
                        //tbl2.Text = "in same group but other areas.";
                        //tbl2.HorizontalAlignment = HorizontalAlignment.Left;
                        //tbl2.FontStyle = FontStyles.Italic;
                        //tbl2.Foreground = new SolidColorBrush(Colors.Orange);
                        //gr.Children.Add(tbl2);
                        //Grid.SetRow(tbl2, rowindex+1);
                        //Grid.SetColumn(tbl2, 0);


                        CheckBox chkAllGroup = new CheckBox();
                        chkAllGroup.Name = "chkAll";
                        gr.Children.Add(chkAllGroup);
                        Grid.SetRow(chkAllGroup, rowindex);
                        Grid.SetColumn(chkAllGroup, 2);
                    }

                }

            }

        }

        private bool ValidateStudioMAttributes()
        {
            RadTabItem im = (RadTabItem)rd.FindName("studiomtab");
            ScrollViewer sv2 = (ScrollViewer)im.Content;
            Grid gr2 = (Grid)sv2.Content; //(Grid)sv2.FindName("studiomgrid");

            Grid gr = null;
            RadComboBox dpsup = null;
            foreach (UIElement elem in gr2.Children)
            {
                if (elem is RadComboBox)
                {
                    dpsup = (RadComboBox)elem;
                }
                else if (elem is Grid)
                {
                    gr = (Grid)elem;
                }
            }


            string answerXML = "";

            //RadComboBox dpsup = (RadComboBox)gr2.FindName("cmbSupplier");
            if (dpsup != null && dpsup.SelectedValue.ToString() != "0")
            {
                string selectedsupplierid = dpsup.SelectedValue.ToString();
                answerXML = @"<Brands>";
                answerXML = answerXML + @"<Brand id=""" + selectedsupplierid + @""" name=""" + dpsup.Text.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                answerXML = answerXML + @"<Questions>";

                foreach (StudioMQuestion q in qulist)
                {
                    if (q.SupplierBrandID == selectedsupplierid)
                    {
                        ObservableCollection<StudioMAnswer> filteredAnser = new ObservableCollection<StudioMAnswer>();
                        foreach (StudioMAnswer a in awlist)
                        {
                            if (a.SupplierBrandID == selectedsupplierid && a.QuestionID == q.QuestionID)
                            {
                                filteredAnser.Add(a);
                            }
                        }

                        answerXML = answerXML + @"<Question id=""" + q.QuestionID + @""" text=""" + q.QuestionText.Replace(@"""", @"&quot;") + @""" type=""" + q.QuestionType.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                        answerXML = answerXML + @"<Answers>";
                        switch (q.QuestionType.ToUpper())
                        {
                            case "SINGLE SELECTION":
                                string tempname3 = "single_selection_" + q.QuestionID;
                                bool selectedsingle = true;
                                /*
                                //RadAutoCompleteBox autobox = new RadAutoCompleteBox();
                                AutoCompleteBox autobox = new AutoCompleteBox();
                                foreach (UIElement elem in gr.Children)
                                {
                                    if (elem is AutoCompleteBox)
                                    {
                                        if (((AutoCompleteBox)elem).Name == tempname3)
                                        {
                                            autobox = (AutoCompleteBox)elem;
                                            string a = autobox.Text;
                                            if (q.Mandatory && a == "")
                                            {
                                                selectedsingle = false;
                                            }
                                        }
                                    }
                                }
                                if (selectedsingle)
                                {
                                    string id = "";
                                    if (autobox.SelectedItem!=null && autobox.Text != "")
                                    {
                                        foreach (StudioMAnswer asw in filteredAnser)
                                        {
                                            if (asw.AnswerText.ToUpper() == autobox.Text.ToUpper())
                                            {
                                                id = asw.AnswerID;
                                            }
                                        }
                                        answerXML = answerXML + @"<Answer id=""" + id + @""" text=""" + autobox.Text.Replace(@"""", @"&quot;") + @"""/>";
                                    }
                                }
                                else
                                {
                                    RadWindow.Alert(new TextBlock { Text = "Please select an answer for question: \r\n" + q.QuestionText + ".", TextWrapping = TextWrapping.Wrap });
                                    return false;
                                }
                                 * */


                                RadComboBox rcm = new RadComboBox();

                                foreach (UIElement elem in gr.Children)
                                {
                                    if (elem is RadComboBox)
                                    {
                                        if (((RadComboBox)elem).Name == tempname3)
                                        {
                                            rcm = (RadComboBox)elem;
                                            if (q.Mandatory && rcm.SelectedValue.ToString() == "0")
                                            {
                                                selectedsingle = false;
                                            }
                                        }
                                    }
                                }

                                if (selectedsingle)
                                {
                                    if (rcm.SelectedValue.ToString() != "0")
                                    {
                                        answerXML = answerXML + @"<Answer id=""" + rcm.SelectedValue.ToString() + @""" text=""" + rcm.Text.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                    }
                                }
                                else
                                {
                                    RadWindow.Alert(new TextBlock { Text = "Please select an answer for question: \r\n" + q.QuestionText + ".", TextWrapping = TextWrapping.Wrap });
                                    return false;
                                }

                                break;
                            case "MULTIPLE SELECTION":
                                //create dropdown
                                bool selected = false;
                                int idex = 0;
                                foreach (StudioMAnswer a in filteredAnser)
                                {
                                    CheckBox chk;
                                    string tempname = "chk_" + q.QuestionID + "_" + idex.ToString();
                                    foreach (UIElement elem in gr.Children)
                                    {
                                        if (elem is CheckBox)
                                        {
                                            if (((CheckBox)elem).Name == tempname && (bool)((CheckBox)elem).IsChecked)
                                            {
                                                answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + a.AnswerText.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                                selected = true;
                                                break;
                                            }
                                        }
                                    }

                                    idex = idex + 1;
                                }
                                if (!selected && q.Mandatory)
                                {
                                    RadWindow.Alert(new TextBlock { Text = "Please select at least one answer for question: \r\n" + q.QuestionText + ".", TextWrapping = TextWrapping.Wrap });
                                    return false;
                                }
                                break;
                            case "FREE TEXT":
                                //create dropdown
                                TextBox txtfree = new TextBox();
                                string tempname4 = "freetext_" + q.QuestionID;
                                foreach (UIElement elem in gr.Children)
                                {
                                    if (elem is TextBox)
                                    {
                                        if (((TextBox)elem).Name == tempname4)
                                        {
                                            txtfree = (TextBox)elem;
                                            break;
                                        }
                                    }
                                }

                                if (txtfree.Text.Trim() == "" && q.Mandatory)
                                {
                                    RadWindow.Alert(new TextBlock { Text = "Please enter the answer for question: \r\n" + q.QuestionText + ".", TextWrapping = TextWrapping.Wrap });
                                    return false;
                                }
                                else
                                {
                                    foreach (StudioMAnswer a in filteredAnser)
                                    {
                                        answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + txtfree.Text.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                    }
                                }
                                break;
                            case "INTEGER":
                                //create dropdown
                                string tempname2 = "txt_int_" + q.QuestionID;
                                TextBox tb = new TextBox();
                                //TextBox tb = (TextBox)gr.FindName(tempname2);

                                foreach (UIElement elem in gr.Children)
                                {
                                    if (elem is TextBox)
                                    {
                                        if (((TextBox)elem).Name == tempname2)
                                        {
                                            tb = (TextBox)elem;
                                            break;
                                        }
                                    }
                                }
                                if (q.Mandatory || (tb.Text != "" && !q.Mandatory))
                                {
                                    try
                                    {
                                        int i;
                                        i = int.Parse(tb.Text);
                                        foreach (StudioMAnswer a in filteredAnser)
                                        {
                                            answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + i.ToString() + @"""/>";
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        RadWindow.Alert(new TextBlock { Text = "Please enter a valid integer as answer for question: \r\n" + q.QuestionText + ".", TextWrapping = TextWrapping.Wrap });
                                        rd.SelectedIndex = 4;
                                        return false;
                                    }
                                }
                                else
                                {
                                    foreach (StudioMAnswer a in filteredAnser)
                                    {
                                        answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""&quot;""/>";
                                    }
                                }

                                break;
                            case "DECIMAL":
                                //create dropdown
                                string tempname5 = "decimal_" + q.QuestionID;
                                TextBox txtdecimal = new TextBox();
                                //TextBox txtdecimal = (TextBox)gr.FindName(tempname5);
                                foreach (UIElement elem in gr.Children)
                                {
                                    if (elem is TextBox)
                                    {
                                        if (((TextBox)elem).Name == tempname5)
                                        {
                                            txtdecimal = (TextBox)elem;
                                            break;
                                        }
                                    }
                                }
                                if (q.Mandatory || (txtdecimal.Text != "" && !q.Mandatory))
                                {
                                    try
                                    {
                                        decimal d = decimal.Parse(txtdecimal.Text);
                                        foreach (StudioMAnswer a in filteredAnser)
                                        {
                                            answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + d.ToString() + @"""/>";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        RadWindow.Alert(new TextBlock { Text = "Please enter a valid decimal as answer for question: \r\n" + q.QuestionText + ".", TextWrapping = TextWrapping.Wrap });
                                        return false;
                                    }
                                }
                                else
                                {
                                    foreach (StudioMAnswer a in filteredAnser)
                                    {
                                        answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""&quot;""/>";
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                        answerXML = answerXML + @"</Answers>";
                        answerXML = answerXML + @"</Question>";
                    }

                }

                answerXML = answerXML + @"</Questions></Brand></Brands>";

                EstimateDetails item = (EstimateDetails)im.DataContext;
                item.StudioMAnswer = answerXML;
                selectedstudiomanswer = answerXML;
            }

            return true;
        }

        private void ContructAnswerXML()
        {
            RadTabItem im = (RadTabItem)rd.FindName("studiomtab");
            ScrollViewer sv = (ScrollViewer)im.Content;
            //Grid gr = (Grid)im.Content;
            Grid gr2 = (Grid)sv.Content; //(Grid)sv.FindChildByType<Grid>();
            //Grid gr2 = (Grid)im.Content;
            if (gr2 != null)
            {
                Grid gr = null;
                RadComboBox dpsup = null;
                foreach (UIElement elem in gr2.Children)
                {
                    if (elem is RadComboBox)
                    {
                        dpsup = (RadComboBox)elem;
                    }
                    else if (elem is Grid)
                    {
                        gr = (Grid)elem;
                    }
                }


                string answerXML = "";

                if (dpsup != null && dpsup.SelectedValue != null)
                {
                    string selectedsupplierid = dpsup.SelectedValue.ToString();
                    answerXML = @"<Brands>";
                    answerXML = answerXML + @"<Brand id=""" + selectedsupplierid + @""" name=""" + dpsup.Text.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                    answerXML = answerXML + @"<Questions>";

                    foreach (StudioMQuestion q in qulist)
                    {
                        if (q.SupplierBrandID == selectedsupplierid)
                        {
                            ObservableCollection<StudioMAnswer> filteredAnser = new ObservableCollection<StudioMAnswer>();
                            foreach (StudioMAnswer a in awlist)
                            {
                                if (a.SupplierBrandID == selectedsupplierid && a.QuestionID == q.QuestionID)
                                {
                                    filteredAnser.Add(a);
                                }
                            }

                            answerXML = answerXML + @"<Question id=""" + q.QuestionID + @""" text=""" + q.QuestionText.Replace(@"""", @"&quot;") + @""" type=""" + q.QuestionType.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @""">";
                            answerXML = answerXML + @"<Answers>";
                            switch (q.QuestionType.ToUpper())
                            {
                                case "SINGLE SELECTION":
                                    RadComboBox rcm = new RadComboBox();
                                    string tempname3 = "single_selection_" + q.QuestionID;
                                    foreach (UIElement elem in gr.Children)
                                    {
                                        if (elem is RadComboBox)
                                        {
                                            if (((RadComboBox)elem).Name == tempname3)
                                            {
                                                rcm = (RadComboBox)elem;
                                            }
                                        }
                                    }

                                    //rcm = (RadComboBox)gr.FindName(tempname3);
                                    answerXML = answerXML + @"<Answer id=""" + rcm.SelectedValue.ToString() + @""" text=""" + rcm.Text.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                    break;
                                case "MULTIPLE SELECTION":
                                    //create dropdown
                                    bool selected = false;
                                    int idex = 0;
                                    foreach (StudioMAnswer a in filteredAnser)
                                    {
                                        CheckBox chk;
                                        string tempname = "chk_" + q.QuestionID + "_" + idex.ToString();
                                        foreach (UIElement elem in gr.Children)
                                        {
                                            if (elem is CheckBox)
                                            {
                                                if (((CheckBox)elem).Name == tempname && (bool)((CheckBox)elem).IsChecked)
                                                {
                                                    answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + a.AnswerText.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                                    selected = true;
                                                    break;
                                                }
                                            }
                                        }

                                        idex = idex + 1;
                                    }

                                    break;
                                case "FREE TEXT":
                                    //create dropdown
                                    TextBox txtfree = new TextBox();
                                    string tempname4 = "freetext_" + q.QuestionID;
                                    foreach (UIElement elem in gr.Children)
                                    {
                                        if (elem is TextBox)
                                        {
                                            if (((TextBox)elem).Name == tempname4)
                                            {
                                                txtfree = (TextBox)elem;
                                                break;
                                            }
                                        }
                                    }

                                    if (txtfree.Text.Trim() != "")
                                    {
                                        foreach (StudioMAnswer a in filteredAnser)
                                        {
                                            answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + txtfree.Text.Replace(@"""", @"&quot;").Replace(@"&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;") + @"""/>";
                                        }
                                    }

                                    break;
                                case "INTEGER":
                                    //create dropdown
                                    string tempname2 = "txt_int_" + q.QuestionID;
                                    TextBox tb = new TextBox();
                                    //TextBox tb = (TextBox)gr.FindName(tempname2);

                                    foreach (UIElement elem in gr.Children)
                                    {
                                        if (elem is TextBox)
                                        {
                                            if (((TextBox)elem).Name == tempname2)
                                            {
                                                tb = (TextBox)elem;
                                                break;
                                            }
                                        }
                                    }


                                    foreach (StudioMAnswer a in filteredAnser)
                                    {
                                        answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + tb.Text + @"""/>";
                                    }


                                    break;
                                case "DECIMAL":
                                    //create dropdown
                                    string tempname5 = "decimal_" + q.QuestionID;
                                    TextBox txtdecimal = new TextBox();
                                    //TextBox txtdecimal = (TextBox)gr.FindName(tempname5);
                                    foreach (UIElement elem in gr.Children)
                                    {
                                        if (elem is TextBox)
                                        {
                                            if (((TextBox)elem).Name == tempname5)
                                            {
                                                txtdecimal = (TextBox)elem;
                                                break;
                                            }
                                        }
                                    }

                                    foreach (StudioMAnswer a in filteredAnser)
                                    {
                                        answerXML = answerXML + @"<Answer id=""" + a.AnswerID + @""" text=""" + txtdecimal.Text + @"""/>";
                                    }

                                    break;

                                default:
                                    break;
                            }
                            answerXML = answerXML + @"</Answers>";
                            answerXML = answerXML + @"</Question>";
                        }

                    }

                    EstimateDetails item = (EstimateDetails)im.DataContext;
                    answerXML = answerXML + @"</Questions></Brand></Brands>";
                    item.StudioMAnswer = answerXML;
                    selectedstudiomanswer = answerXML;

                }
            }
        }

        private void viewimage(object sender, EventArgs e)
        {
            Image im = (Image)((HyperlinkButton)sender).Content;
            BitmapImage bi = (BitmapImage)im.Source;
            ShowImage simage = new ShowImage(bi);
            RadWindow win = new RadWindow();
            win.Height = 600;
            win.Width = 800;

            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = simage;
            win.Header = "View Product Images";
            win.ShowDialog();
        }

        private void LoadProductImage(string productid, RadTabControl rd)
        {
            List<ProductImage> ImageList = new List<ProductImage>();
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            MRSclient.GetProductImagesCompleted += delegate(object o, GetProductImagesCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    ImageList = new List<ProductImage>();
                    if (es.Result != null)
                    {
                        foreach (ProductImage pi in es.Result)
                        {
                            ImageList.Add(pi);
                        }
                        DisplayImage(rd, ImageList);
                    }
                }
                else
                {
                    ExceptionHandler.PopUpErrorMessage(es.Error, "GetProductImagesCompleted");
                }
            };

            MRSclient.GetProductImagesAsync(productid, 0);


        }

        private void RadGridView1_RowLoaded(object sender, RowLoadedEventArgs e)
        {
            HyperlinkButton hl;
            TextBlock tb;
            Image img;
            GridViewRow row = e.Row as GridViewRow;
            if (row != null)
            {
                EstimateDetails ed = row.DataContext as EstimateDetails;
                if (row != null && ed != null)
                {
                    if (ed.AreaId == 43)
                    {
                        if (/*EstimateList.revisiontypepermission.AllowToAddNSR && */ !EstimateList.revisiontypepermission.ReadOnly) //All revisions can now add NSR
                        {
                            foreach (GridViewCell Cell in row.Cells)
                            {
                                if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnAddOption")
                                {
                                    hl = Cell.FindChildByType<HyperlinkButton>();
                                    tb = Cell.FindChildByType<TextBlock>();
                                    tb.Opacity = 1;
                                    hl.IsEnabled = true;
                                    img = Cell.FindChildByType<Image>();
                                    img.Opacity = 1;
                                }
                                else if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnCopy")
                                {
                                    hl = Cell.FindChildByType<HyperlinkButton>();
                                    hl.IsEnabled = false;
                                    tb = Cell.FindChildByType<TextBlock>();
                                    tb.Opacity = 0.3;
                                    img = Cell.FindChildByType<Image>();
                                    img.Opacity = 0.3;
                                }
                            }
                        }
                        else
                        {
                            foreach (GridViewCell Cell in row.Cells)
                            {
                                if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnAddOption")
                                {
                                    hl = Cell.FindChildByType<HyperlinkButton>();
                                    tb = Cell.FindChildByType<TextBlock>();
                                    tb.Opacity = 0.3;
                                    hl.IsEnabled = false;
                                    img = Cell.FindChildByType<Image>();
                                    img.Opacity = 0.3;
                                }
                                else if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnCopy")
                                {
                                    hl = Cell.FindChildByType<HyperlinkButton>();
                                    hl.IsEnabled = false;
                                    tb = Cell.FindChildByType<TextBlock>();
                                    tb.Opacity = 0.3;
                                    img = Cell.FindChildByType<Image>();
                                    img.Opacity = 0.3;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (!EstimateList.revisiontypepermission.ReadOnly)
                        {
                            if (!EstimateList.revisiontypepermission.AllowToAddNSR)
                            { //All revisions can now add NSR
                                /*
                                foreach (GridViewCell Cell in row.Cells)
                                {
                                    if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnAddOption")
                                    {
                                        hl = Cell.FindChildByType<HyperlinkButton>();
                                        tb = Cell.FindChildByType<TextBlock>();
                                        tb.Opacity = 1;
                                        hl.IsEnabled = true;
                                        img = Cell.FindChildByType<Image>();
                                        img.Opacity = 1;
                                    }
                                    else if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnCopy")
                                    {
                                        hl = Cell.FindChildByType<HyperlinkButton>();
                                        hl.IsEnabled = false;
                                        tb = Cell.FindChildByType<TextBlock>();
                                        tb.Opacity = 0.3;
                                        img = Cell.FindChildByType<Image>();
                                        img.Opacity = 0.3;
                                    }
                                }
                                */
                            }
                            else
                            {
                                foreach (GridViewCell Cell in row.Cells)
                                {
                                    if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnAddOption")
                                    {
                                        hl = Cell.FindChildByType<HyperlinkButton>();
                                        tb = Cell.FindChildByType<TextBlock>();
                                        tb.Opacity = 1;
                                        hl.IsEnabled = true;
                                        img = Cell.FindChildByType<Image>();
                                        img.Opacity = 1;
                                    }
                                    else if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnCopy")
                                    {
                                        hl = Cell.FindChildByType<HyperlinkButton>();
                                        hl.IsEnabled = true;
                                        tb = Cell.FindChildByType<TextBlock>();
                                        tb.Opacity = 1;
                                        img = Cell.FindChildByType<Image>();
                                        img.Opacity = 1;
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (GridViewCell Cell in row.Cells)
                            {
                                if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnAddOption")
                                {
                                    hl = Cell.FindChildByType<HyperlinkButton>();
                                    tb = Cell.FindChildByType<TextBlock>();
                                    tb.Opacity = 0.3;
                                    hl.IsEnabled = false;
                                    img = Cell.FindChildByType<Image>();
                                    img.Opacity = 0.3;
                                }
                                else if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnCopy")
                                {
                                    hl = Cell.FindChildByType<HyperlinkButton>();
                                    hl.IsEnabled = false;
                                    tb = Cell.FindChildByType<TextBlock>();
                                    tb.Opacity = 0.3;
                                    img = Cell.FindChildByType<Image>();
                                    img.Opacity = 0.3;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void ResetEditEstimateUserID()
        {
            if ((App.Current as App).SelectedEstimateRevisionId > 0)
            {
                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.ResetEditEstimateUserIDCompleted += new EventHandler<ResetEditEstimateUserIDCompletedEventArgs>(mrsClient_ResetEditEstimateUserIDCompleted);
                mrsClient.ResetEditEstimateUserIDAsync((App.Current as App).SelectedEstimateRevisionId, 0);
            }
        }
        void mrsClient_ResetEditEstimateUserIDCompleted(object sender, ResetEditEstimateUserIDCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // success
                (App.Current as App).SelectedEstimateRevisionId = 0;
            }
            else
            {
                // error when reseting the user from edit mode
            }
        }

        private void btnUnlock_Click(object sender, RoutedEventArgs e)
        {
            RetailSystemClient MRSclient = new RetailSystemClient();
            MRSclient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            MRSclient.UnlockEstimateAsync(EstimateList.SelectedEstimateRevisionId, 3); // unlock the web app hold estimate

            NavigationService.GoBack();
        }

        public class StudioMSupplier
        {
            public string SupplierID { get; set; }
            public string SupplierName { get; set; }
        }

        public class StudioMSupplierBrand
        {
            public string SupplierBrandID { get; set; }
            public string SupplierBrandName { get; set; }
        }

        public class StudioMQuestion
        {
            public string QuestionID { get; set; }
            public string QuestionText { get; set; }
            public string QuestionType { get; set; }
            public string SupplierBrandID { get; set; }
            public bool Mandatory { get; set; }
            public bool IsActive { get; set; }
        }

        public class StudioMAnswer
        {
            public string AnswerID { get; set; }
            public string AnswerText { get; set; }
            public string QuestionID { get; set; }
            public string SupplierBrandID { get; set; }
        }
        public class GridColumnOrder
        {
            public int Order { get; set; }
            public string ColumnName { get; set; }

        }

        private void MarkForDeletionButton_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            EstimateDetails ed = ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.DataContext as EstimateDetails;
            if ((bool)chk.IsChecked)
            {
                ed.ItemAllowToRemove = true;
                ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.Background = new SolidColorBrush(Color.FromArgb(75, 255, 204, 204));
            }
            else
            {
                ed.ItemAllowToRemove = false;
                ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            }
        }

        private void DeleteAllSelectedItemsButton_Click(object sender, RoutedEventArgs e)
        {
            //RadWindow.Confirm("The selected items in the estimate will be deleted.\r\nDo you want to delete all selected items?", new EventHandler<WindowClosedEventArgs>((s, args) =>
            //{
            //    if (args.DialogResult == true)
            //    {
                    string items = "";
                    List<EstimateDetails> listEstimateDetails = new List<EstimateDetails>();
                    foreach (EstimateDetails ed in ((EstimateViewModel)LayoutRoot.DataContext).OptionListTabs[((EstimateViewModel)LayoutRoot.DataContext).SelectedTabIndex].Options)
                    {
                        if (ed.UpdatedItemDeleted)
                        {
                            listEstimateDetails.Add(ed);
                            items += ed.ProductId + ", ";
                        }
                    }
                    if (listEstimateDetails.Count > 0)
                    {
                        items = items.Substring(0, items.Length - 2);

                        RadWindow win = new RadWindow();
                        //if (!ed.IsMasterPromotion)
                        //{
                        RemoveItemReasonWindow messageDlg = new RemoveItemReasonWindow(listEstimateDetails, items);
                        win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                        win.Header = "Delete " + listEstimateDetails.Count + " Item(s)";
                        win.Content = messageDlg;
                        win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndOpenAccept);
                        win.ShowDialog();
                        //}
                        //else
                        //{
                        //    RemoveMasterPromotionItemWindow messageDlg2 = new RemoveMasterPromotionItemWindow(ed);
                        //    win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                        //    win.Header = "Delete Promotion(s)";
                        //    win.Content = messageDlg2;
                        //    win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndOpenAccept);
                        //    win.ShowDialog();
                        //}
                    }
            //    }
            //}));
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            //deletedED = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).DataContext as EstimateDetails;
            deletedED = ((Grid)((HyperlinkButton)e.OriginalSource).Parent).DataContext as EstimateDetails;
            RadWindow win = new RadWindow();
            if (deletedED.IsMasterPromotion)
            {
                RemoveMasterPromotionItemWindow messageDlg2 = new RemoveMasterPromotionItemWindow(deletedED);
                win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
                win.Header = "Remove Promotion";
                win.Content = messageDlg2;
                win.Closed += new EventHandler<WindowClosedEventArgs>(win_ClosedAndOpenAccept);
                win.ShowDialog();
            }
            //RadWindow.Confirm("Remove template will inactivate all items configured to this template.\n\nAre you sure you want to remove this tempale?", new EventHandler<WindowClosedEventArgs>(template_confirm_close));
        }

        void win_ClosedAndOpenAccept(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            RemoveParameterClass rpc = (RemoveParameterClass)dlg.DataContext;
            if (rpc == null)
            {
                rpc = new RemoveParameterClass();
                rpc.RemovedPAG = new List<EstimateDetails>();
                rpc.LeftPAG = new List<PromotionPAG>();
            }
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
            {
                for (int indexCurrent = 0; indexCurrent < ((EstimateViewModel)LayoutRoot.DataContext).OptionListTabs[((EstimateViewModel)LayoutRoot.DataContext).SelectedTabIndex].Options.Count;)
                {
                    deletedED =  ((EstimateViewModel)LayoutRoot.DataContext).OptionListTabs[((EstimateViewModel)LayoutRoot.DataContext).SelectedTabIndex].Options[indexCurrent];
                    if (deletedED.UpdatedItemDeleted)
                    {
                        deletedED.ItemAccepted = false;
                        if (deletedED.AreaId == 43 && (result.HasValue && result.Value))
                        {
                            deletedED.AreaName = "Non Standard Request";
                            deletedED.Quantity = 1;
                            deletedED.Price = 0;
                            deletedED.TotalPrice = 0;
                            deletedED.ExtraDescription = "Subject to builder acceptance.";
                            deletedED.NonstandardCategoryID = 0;
                            deletedED.NonstandardGroupID = 0;
                            deletedED.InternalDescription = "";
                            deletedED.AdditionalNotes = "";
                            deletedED.SOSI = "./images/upgrade.png";
                            deletedED.SOSIToolTips = "Upgrade Option.";
                            deletedED.CostExcGST = "";
                            deletedED.UpdatedBTPCostExcGST = "";
                            deletedED.UpdatedDBCCostExcGST = "";
                        }
                        deletedED.StudioMAnswer = "";
                        ((EstimateViewModel)LayoutRoot.DataContext).RemoveOptionFromList(deletedED, rpc.RemovedPAG, rpc.LeftPAG);
                    }
                    else
                        indexCurrent++;
                }
            }
            ((EstimateViewModel)LayoutRoot.DataContext).ForceRefreshTab();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            EstimateDetails ed = ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.DataContext as EstimateDetails;
            int accepted = 0;
            if ((bool)chk.IsChecked)
            {
                accepted = 1;
                ed.UpdatedItemAccepted = true;
            }
            else
            {
                accepted = 0;
                ed.UpdatedItemAccepted = false;
            }

            ((EstimateViewModel)LayoutRoot.DataContext).UpdateItemAcceptance(ed.EstimateRevisionDetailsId.ToString(), accepted, (App.Current as App).CurrentUserId);

        }

        private void CheckBoxDeleted_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            EstimateDetails ed = ((Grid)((CheckBox)e.OriginalSource).Parent).DataContext as EstimateDetails;
            int deleted = 0;
            if ((bool)chk.IsChecked)
            {
                deleted = 1;
                ed.UpdatedItemDeleted = true;
            }
            else
            {
                deleted = 0;
                ed.UpdatedItemDeleted = false;
            }
        }

        private void btnAddOption_Click(object sender, RoutedEventArgs e)
        {
            //string derivedcost = "0";
            //string costprice = "";
            RadWindow win = new RadWindow();
            EstimateDetails pag = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as EstimateDetails;
            //if (!pag.AreaName.ToUpper().Contains("NON STANDARD"))
            //{
            AppOptionFromTree acceptDlg = new AppOptionFromTree(pag, "OPTIONTREE", OptionTabControl.SelectedIndex);
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Add Option Window";
            win.Content = acceptDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_AddOptionClosed);
            win.ShowDialog();
            //}
            //else
            //{
            //if (pag.DerivedCost)
            //    derivedcost = "1";
            //if (pag.CostExcGST != null)
            //{
            //    costprice = pag.CostExcGST.ToString();
            //}
            //((EstimateViewModel)LayoutRoot.DataContext).SaveSelectedOptionsFromTreeToEstimate(pag.EstimateDetailsId.ToString(),
            //   pag.StandardInclusionId.ToString(),
            //   EstimateList.SelectedEstimateRevisionId.ToString(),
            //   "", 
            //   (App.Current as App).CurrentUserId.ToString(),
            //   derivedcost,
            //   costprice
            //   );
            //}


        }

        void win_AddOptionClosed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            ParameterClass p = (ParameterClass)dlg.DataContext;
            if (p != null)
            {
                EstimateDetails pag = p.SelectedPAG;

                bool? result = dlg.DialogResult;
                if (result.HasValue && result.Value)
                {
                    if (p.SelectedItemID != "" || p.SelectedStandardInclusionID != "")
                    {
                        ((EstimateViewModel)LayoutRoot.DataContext).SaveSelectedOptionsFromTreeToEstimate(p.SelectedItemID,
                            p.SelectedStandardInclusionID,
                            EstimateList.SelectedEstimateRevisionId.ToString(),
                            p.StudioMQANDA,
                            (App.Current as App).CurrentUserId.ToString(),
                            p.SelectedDerivedCosts,
                            p.SelectedBTPCostExcGSTs,
                            p.SelectedBTPCostOverwriteFlags,
                            p.SelectedDBCCostExcGSTs,
                            p.SelectedDBCCostOverwriteFlags,
                            p.SelectedQuantities,
                            p.SelectedPrices,
                            p.SelectedIsAccepteds,
                            p.SelectedAreaIds,
                            p.SelectedGroupIds,
                            p.SelectedPriceDisplayCodeIds,
                            p.SelectedIsSiteWorks,
                            p.SelectedProductDescriptions,
                            p.SelectedAdditionalNotes,
                            p.SelectedExtraDescriptions,
                            p.SelectedInternalDescriptions);
                    }
                }
            }
        }

        void win_ReplaceOptionClosed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            ParameterClass p = (ParameterClass)dlg.DataContext;
            if (p != null)
            {
                EstimateDetails pag = p.SelectedPAG;

                bool? result = dlg.DialogResult;
                if (result.HasValue && result.Value)
                {
                    if (p.SelectedItemID != "" || p.SelectedStandardInclusionID != "")
                    {
                        ((EstimateViewModel)LayoutRoot.DataContext).ReplaceEstimateItemsSaveSelectedOptionsFromTreeToEstimate(deletedED, p.SelectedItemID,
                            p.SelectedStandardInclusionID,
                            EstimateList.SelectedEstimateRevisionId.ToString(),
                            p.StudioMQANDA,
                            (App.Current as App).CurrentUserId.ToString(),
                            p.SelectedDerivedCosts,
                            p.SelectedBTPCostExcGSTs,
                            p.SelectedDBCCostExcGSTs,
                            p.SelectedQuantities,
                            p.SelectedPrices,
                            p.SelectedIsAccepteds,
                            p.SelectedAreaIds,
                            p.SelectedGroupIds,
                            p.SelectedPriceDisplayCodeIds,
                            p.SelectedIsSiteWorks,
                            p.SelectedProductDescriptions,
                            p.SelectedAdditionalNotes,
                            p.SelectedExtraDescriptions,
                            p.SelectedInternalDescriptions,
                            p.CopyQuantity,
                            p.CopyAdditionalNotes,
                            p.CopyExtraDescriptions,
                            p.CopyInternalNotes
                            );
                    }
                }
            }
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            item.ContractType = ((EstimateViewModel)LayoutRoot.DataContext).ContractType;
            item.JobFlowType = ((EstimateViewModel)LayoutRoot.DataContext).JobFlowType;
            EstimateHistory historyDlg = new EstimateHistory(item);
            win.Header = "Estimate History";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = historyDlg;
            win.ShowDialog();
        }

        private void btnCompare_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            CompareEstimates compareDlg = new CompareEstimates(item);
            win.Header = "Compare Estimates";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = compareDlg;
            win.ResizeMode = ResizeMode.NoResize;
            win.ShowDialog();
        }

        private void btnDeletedItems_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            DeletedItemList deletedDlg = new DeletedItemList(item.RecordId);
            win.Header = "Deleted Items";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = deletedDlg;
            win.ResizeMode = ResizeMode.NoResize;
            win.CanClose = false;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_DeletedItemsClosed);
            win.ShowDialog();
        }

        private void btnChangeHomeName_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            ChangeHomeName changeNameDlg = new ChangeHomeName(item.RecordId);
            win.Header = "Change Home Name";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = changeNameDlg;
            win.ResizeMode = ResizeMode.NoResize;
            win.ShowDialog();
        }

        private void btnCopyEstimate_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            CopyEstimate copyEstimateDlg = new CopyEstimate(item.RecordId, item.EstimateId);
            win.Header = "Copy Estimate";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = copyEstimateDlg;
            win.ResizeMode = ResizeMode.NoResize;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_CopyEstimateClosed);
            win.ShowDialog();
        }

        void win_CopyEstimateClosed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                NavigationService.Refresh();
        }

        void win_DeletedItemsClosed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                ((EstimateViewModel)LayoutRoot.DataContext).ForceRefreshTab();
        }

        private void btnCustomerSign_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            CustomerSign custSignDlg = new CustomerSign(item.RecordId, item.RevisionTypeId, item.EstimateId, item.RevisionNumber, item.Accountid);
            win.Header = "DocuSign";
            win.ResizeMode = ResizeMode.NoResize;
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = custSignDlg;
            win.ShowDialog();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            RadTabItem im2 = (RadTabItem)rd.FindName("phototab");
            ScrollViewer sv = (ScrollViewer)im2.Content;
            Grid grimage = (Grid)sv.Content;


            foreach (UIElement elem in grimage.Children)
            {
                if (elem is RadioButton)
                {
                    if ((bool)((RadioButton)elem).IsChecked)
                    {

                        ((RadioButton)elem).IsChecked = false;
                    }
                }
            }
        }

        private void OptionsGrid_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            if (e.AvailableOperators.Contains(FilterOperator.Contains))
                e.DefaultOperator1 = FilterOperator.Contains;
        }

        private void RadGridView1_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            if (e.AvailableOperators.Contains(FilterOperator.Contains))
                e.DefaultOperator1 = FilterOperator.Contains;
        }

        private void OptionsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            RadGridView rgd = (RadGridView)OptionTabControl.FindChildByType<RadGridView>();
            if (rgd != null)
            {
                if ((App.Current as App).CurrentAction != "EDIT")
                {
                    HyperlinkButton btnRemove = (HyperlinkButton)rgd.FindName("btnRemove");
                    if (btnRemove != null)
                        btnRemove.IsEnabled = false;
                }
                if (gridSaveLayout == "custom")
                {
                    /* Commented out 2015-09-30 by WinTe as it causes issue with enabling/disabling Delete and Copy buttons
                    if (EstimateList.SelectedRevisionTypeCode.ToUpper() == "STS" || 
                        EstimateList.SelectedRevisionTypeCode.ToUpper() == "SC" || 
                        EstimateList.SelectedRevisionTypeCode.ToUpper() == "SE" ||
                        EstimateList.SelectedRevisionTypeCode.ToUpper() == "PVAR-SE" ||
                        EstimateList.SelectedRevisionTypeCode.ToUpper() == "PSTM-SE" ||
                        EstimateList.SelectedRevisionTypeCode.ToUpper() == "BVAR-BE") 
                    {

                        rgd.Columns["ColAccepted"].IsVisible = true;
                    }
                    else
                    {
                        rgd.Columns["ColAccepted"].IsVisible = false;
                    }
                    */
                    rgd.Columns["ColArea"].Width = Convert.ToDouble((string)appSettings["ColArea"]);
                    rgd.Columns["ColGroup"].Width = Convert.ToDouble((string)appSettings["ColGroup"]);
                    rgd.Columns["ColProductName"].Width = Convert.ToDouble((string)appSettings["ColProductName"]);
                    rgd.Columns["ColProductDesc"].Width = Convert.ToDouble((string)appSettings["ColProductDesc"]);
                    rgd.Columns["ColAdditionalNotes"].Width = Convert.ToDouble((string)appSettings["ColAdditionalNotes"]);
                    rgd.Columns["ColQuantity"].Width = Convert.ToDouble((string)appSettings["ColQuantity"]);
                    rgd.Columns["ColPrice"].Width = Convert.ToDouble((string)appSettings["ColPrice"]);
                    rgd.Columns["ColTotal"].Width = Convert.ToDouble((string)appSettings["ColTotal"]);
                    rgd.Columns["ColMarginPercent"].Width = Convert.ToDouble((string)appSettings["ColMarginPercent"]);
                    rgd.Columns["ColUom"].Width = Convert.ToDouble((string)appSettings["ColUom"]);
                    rgd.Columns["ColAccepted"].Width = Convert.ToDouble((string)appSettings["ColAccepted"]);
                    rgd.Columns["ColSoSi"].Width = Convert.ToDouble((string)appSettings["ColSoSi"]);
                    rgd.Columns["ColRemove"].Width = Convert.ToDouble((string)appSettings["ColRemove"]);
                    rgd.Columns["ColCopy"].Width = Convert.ToDouble((string)appSettings["ColCopy"]);
                    rgd.Columns["ColChanges"].Width = Convert.ToDouble((string)appSettings["ColChanges"]);
                    rgd.Columns["ColReplace"].Width = Convert.ToDouble((string)appSettings["ColReplace"]);

                    int colAdditionalNotesNewColumnIndex = 0;
                    foreach (GridColumnOrder o in columnorder)
                    {

                        if (o.ColumnName == "ColAreaIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColAreaIndex"]);
                        else if (o.ColumnName == "ColGroupIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColGroupIndex"]);
                        else if (o.ColumnName == "ColProductNameIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColProductNameIndex"]);
                        else if (o.ColumnName == "ColProductDescIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColProductDescIndex"]);
                        else if (o.ColumnName == "ColAdditionalNotesIndex")
                        {
                            if (appSettings.Contains("ColAdditionalNotesIndex"))
                            {
                                o.Order = Convert.ToInt32((appSettings["ColAdditionalNotesIndex"].ToString()));
                            }
                            else
                            {
                                colAdditionalNotesNewColumnIndex = Convert.ToInt32(appSettings["ColProductDescIndex"].ToString()) + 1;
                                o.Order = colAdditionalNotesNewColumnIndex;
                                appSettings.Add("ColAdditionalNotesIndex", colAdditionalNotesNewColumnIndex);
                            }
                        }
                        else if (o.ColumnName == "ColQuantityIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColQuantityIndex"]);
                        else if (o.ColumnName == "ColPriceIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColPriceIndex"]);
                        else if (o.ColumnName == "ColTotalIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColTotalIndex"]);
                        else if (o.ColumnName == "ColMarginPercentIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColMarginPercentIndex"]);
                        else if (o.ColumnName == "ColUomIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColUomIndex"]);
                        else if (o.ColumnName == "ColAcceptedIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColAcceptedIndex"]);
                        else if (o.ColumnName == "ColSoSiIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColSoSiIndex"]);
                        else if (o.ColumnName == "ColRemoveIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColRemoveIndex"]);
                        else if (o.ColumnName == "ColCopyIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColCopyIndex"]);
                        else if (o.ColumnName == "ColChangesIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColChangesIndex"]);
                        else if (o.ColumnName == "ColReplaceIndex")
                            o.Order = Convert.ToInt32((string)appSettings["ColReplaceIndex"]);
                    }

                    for (int idx = 1; idx <= 16; idx++)
                        rgd.Columns[idx].DisplayIndex = 1;

                        for (int idx = 1; idx <= 16; idx++)
                        {
                            foreach (GridColumnOrder o in columnorder)
                            {
                                if (o.Order == idx)
                                {
                                //if (colAdditionalNotesNewColumnIndex < 1 || o.Order <= colAdditionalNotesNewColumnIndex) // inserting the additional notes next to the product description, if first time shift all columns on the right to 1, otherwise keep same order
                                //    rgd.Columns[o.ColumnName.Replace("Index", "")].DisplayIndex = o.Order;
                                //else
                                    rgd.Columns[o.ColumnName.Replace("Index", "")].DisplayIndex = o.Order;
                                break;
                            }
                        }
                    }

                    for (int i = 0; i < rgd.Columns.Count; i++)
                    {
                        var columnHeader = rgd.ChildrenOfType<GridViewHeaderCell>().Where(headerCell => headerCell.Column.DisplayIndex == i).FirstOrDefault();
                        if (columnHeader != null)
                        {
                            if (columnHeader.Column.ColumnFilterDescriptor.IsActive == true)
                                columnHeader.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));
                        }
                    }
                }
                else // telerik
                    LoadLayout(rgd);
            }
        }

        private void cmbCategory_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            //((EstimateViewModel)LayoutRoot.DataContext).GetNonStandardGroups(int.Parse(((RadComboBox)sender).SelectedValue.ToString()));
            int defaultgroupid = 0;
            RadComboBox rdbox = (RadComboBox)sender;
            EstimateDetails ed = (EstimateDetails)rdbox.DataContext;
            StackPanel panel = rdbox.ParentOfType<StackPanel>();
            RadComboBox cmbgroup = (RadComboBox)panel.FindName("cmbGroup");

            if (rdbox.SelectedValue != null)
            {
                if (int.Parse(rdbox.SelectedValue.ToString()) == ed.NonstandardCategoryID)
                {
                    defaultgroupid = ed.NonstandardGroupID;
                }
                else
                {
                    defaultgroupid = 0;
                }
                GetNonStandardGroups(int.Parse(rdbox.SelectedValue.ToString()), cmbgroup, defaultgroupid);
            }
        }

        public void GetNonStandardGroups(int selectedareaid, RadComboBox cmbgroup, int selectedgroupid)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.GetNonstandardGroupsCompleted += delegate(object o, GetNonstandardGroupsCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if (es.Result.Count > 0)
                    {
                        cmbgroup.ItemsSource = es.Result;
                        cmbgroup.SelectedValue = selectedgroupid;
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "GetNonstandardGroupsCompleted");
            };

            mrsClient.GetNonstandardGroupsAsync(selectedareaid, int.Parse(((App)App.Current).CurrentUserStateID), selectedgroupid);
        }

        public void GetNonStandardAreas(RadComboBox cmbcategory, int selectedareaid)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.GetNonstandardCategoryByStateCompleted += delegate(object o, GetNonstandardCategoryByStateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if (es.Result.Count > 0)
                    {
                        cmbcategory.ItemsSource = es.Result;
                        cmbcategory.SelectedValue = selectedareaid;
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "GetNonStandardAreasCompleted");
            };

            mrsClient.GetNonstandardCategoryByStateAsync(int.Parse(((App)App.Current).CurrentUserStateID), selectedareaid);
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            ((EstimateViewModel)LayoutRoot.DataContext).ForceRefreshTab();
        }

        //To make sure that tab order is the same after the panes are unpinned
        private void RadDocking_PaneStateChange(object sender, RadRoutedEventArgs e)
        {
            RadPane selectedPane = (RadPane)e.Source;
            if (selectedPane.Name == "OptionTreePane" && selectedPane.IsPinned == false)
            {
                CommentsPane.RemoveFromParent();
                AdditionalNotesPane.RemoveFromParent();
                // CRM 9 onwards this will be disabled, upload directly in CRM
                //DocumentsPane.RemoveFromParent();
                OtherOptionsPane.RemoveFromParent();
                SearchProductPane.RemoveFromParent();

                BottomPaneGroup.Items.Add(CommentsPane);
                BottomPaneGroup.Items.Add(AdditionalNotesPane);
                // CRM 9 onwards this will be disabled, upload directly in CRM
                // BottomPaneGroup.Items.Add(DocumentsPane);
                BottomPaneGroup.Items.Add(OtherOptionsPane);
                BottomPaneGroup.Items.Add(SearchProductPane);
            }
            else if (selectedPane.Name == "CommentsPane" && selectedPane.IsPinned == false)
            {
                AdditionalNotesPane.RemoveFromParent();
                // CRM 9 onwards this will be disabled, upload directly in CRM
                // DocumentsPane.RemoveFromParent();
                OtherOptionsPane.RemoveFromParent();
                SearchProductPane.RemoveFromParent();

                BottomPaneGroup.Items.Add(AdditionalNotesPane);
                // CRM 9 onwards this will be disabled, upload directly in CRM
                // BottomPaneGroup.Items.Add(DocumentsPane);
                BottomPaneGroup.Items.Add(OtherOptionsPane);
                BottomPaneGroup.Items.Add(SearchProductPane);
            }
            else if (selectedPane.Name == "AdditionalNotesPane" && selectedPane.IsPinned == false)
            {
                // CRM 9 onwards this will be disabled, upload directly in CRM
                // DocumentsPane.RemoveFromParent();
                OtherOptionsPane.RemoveFromParent();
                SearchProductPane.RemoveFromParent();
                // CRM 9 onwards this will be disabled, upload directly in CRM
                //BottomPaneGroup.Items.Add(DocumentsPane);
                BottomPaneGroup.Items.Add(OtherOptionsPane);
                BottomPaneGroup.Items.Add(SearchProductPane);
            }
            else if (selectedPane.Name == "DocumentsPane" && selectedPane.IsPinned == false)
            {
                // CRM 9 onwards this will be disabled, upload directly in CRM
                // DocumentsPane.RemoveFromParent();
                OtherOptionsPane.RemoveFromParent();
                SearchProductPane.RemoveFromParent();
                // CRM 9 onwards this will be disabled, upload directly in CRM
                // BottomPaneGroup.Items.Add(DocumentsPane);
                BottomPaneGroup.Items.Add(OtherOptionsPane);
                BottomPaneGroup.Items.Add(SearchProductPane);
            }
            else if (selectedPane.Name == "OtherOptionsPane" && selectedPane.IsPinned == false)
            {
                OtherOptionsPane.RemoveFromParent();
                BottomPaneGroup.Items.Add(OtherOptionsPane);
                BottomPaneGroup.Items.Add(SearchProductPane);
            }
            else if (selectedPane.Name == "SearchProductPane" && selectedPane.IsPinned == false)
            {
                SearchProductPane.RemoveFromParent();
                BottomPaneGroup.Items.Add(OtherOptionsPane);
                BottomPaneGroup.Items.Add(SearchProductPane);
            }
        }

        private void OptionsGrid_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            if (gridSaveLayout == "custom")
            {
                if (e.Column.UniqueName != null && e.Column.UniqueName == "ColArea")
                {
                    appSettings["ColArea"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColGroup")
                {
                    appSettings["ColGroup"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColProductName")
                {
                    appSettings["ColProductName"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColProductDesc")
                {
                    appSettings["ColProductDesc"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColAdditionalNotes")
                {
                    appSettings["ColAdditionalNotes"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColQuantity")
                {
                    appSettings["ColQuantity"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColPrice")
                {
                    appSettings["ColPrice"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColTotal")
                {
                    appSettings["ColTotal"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColMarginPercent")
                {
                    appSettings["ColMarginPercent"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColUom")
                {
                    appSettings["ColUom"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColAccepted")
                {
                    appSettings["ColAccepted"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColSoSi")
                {
                    appSettings["ColSoSi"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColRemove")
                {
                    appSettings["ColRemove"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColCopy")
                {
                    appSettings["ColCopy"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColChanges")
                {
                    appSettings["ColChanges"] = e.NewWidth.Value.ToString();
                }
                else if (e.Column.UniqueName != null && e.Column.UniqueName == "ColReplace")
                {
                    appSettings["ColReplace"] = e.NewWidth.Value.ToString();
                }            
            }
            else
                SaveLayout(((RadGridView)sender));
        }

        //private void ruAttachment_FileUploadStarting(object sender, FileUploadStartingEventArgs e)
        //{
        //    FileInfo fileInfo = this.ruAttachment.CurrentSession.CurrentFile.File;
        //    FileStream fileStream = fileInfo.OpenRead();
        //    byte[] contents = new byte[fileStream.Length];
        //    fileStream.Read(contents, 0, contents.Length); 
        //}

        private void btnDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            SharepointDoc doc = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as SharepointDoc;
            ((EstimateViewModel)LayoutRoot.DataContext).SharepointDocuments.Remove(doc);
            ((EstimateViewModel)LayoutRoot.DataContext).DeleteSharepointDocument(doc);

        }

        //private void ruAttachment_UploadStarted(object sender, UploadStartedEventArgs e)
        //{

        //}

        private void RadButton_Click(object sender, RoutedEventArgs e)
        {
            // OpenFileDialog _openFileDialog = new OpenFileDialog();
            //_openFileDialog.Multiselect = false;
            //bool? dialogResult = _openFileDialog.ShowDialog();
            //if (dialogResult.Value)
            //{
            //    if (cmbDocType.SelectedValue.ToString() != "0")
            //    {
            //        SharepointDocumentType dp = (SharepointDocumentType)cmbDocType.SelectedItem;
            //        txtFileName.Text = _openFileDialog.File.Name;
            //        Stream fileStream = _openFileDialog.File.OpenRead();
            //        byte[] contents = new byte[fileStream.Length];
            //        fileStream.Read(contents, 0, contents.Length);

            //        ((EstimateViewModel)LayoutRoot.DataContext).UploadDocumentToSharepoint(txtFileName.Text,contents,dp.DocumentCategory, dp.DocumentType);
            //        txtFileName.Text = "";
            //    }
            //    else
            //    {
            //        MessageBox.Show("Please select a document type for the upload file.");
            //    }
            //}
        }

        private void ruAttachment_FileUploaded(object sender, FileUploadedEventArgs e)
        {
            //RadUploadSelectedFile uploadedFile = e.SelectedFile;
            //string newFileName = uploadedFile.Name;
        }


        private void updateMargin(object sender, string callfrom)
        {
            decimal price, costbtp, costdbc, marginbtp, margindbc;
            decimal gst = decimal.Parse("1.1");
            TextBox txtbox = (TextBox)sender;
            EstimateDetails item = (EstimateDetails)txtbox.DataContext;

            //item.UpdatedTotalPrice = item.UpdatedQuantity * item.UpdatedPrice;
            StackPanel panel = txtbox.ParentOfType<StackPanel>();

            TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
            TextBox txtBTPCostExcGST = (TextBox)panel.FindName("txtBTPCostExcGST");
            TextBox tbMarginBTPCost = (TextBox)panel.FindName("txtMarginBTPCost");
            TextBox txtDBCCostExcGST = (TextBox)panel.FindName("txtDBCCostExcGST");
            TextBox tbMarginDBCCost = (TextBox)panel.FindName("txtMarginDBCCost");

            try
            {
                price = decimal.Parse(txtPrice.Text.Trim());
                //price = (decimal)((int)Math.Round(price, 0));
            }
            catch (Exception ex)
            {
                price = decimal.Parse("-9999999.99");
            }
            try
            {
                costbtp = decimal.Parse(txtBTPCostExcGST.Text);
            }
            catch (Exception ex)
            {
                costbtp = decimal.Parse("-9999999.99");
            }
            try
            {
                costdbc = decimal.Parse(txtDBCCostExcGST.Text);
            }
            catch (Exception ex)
            {
                costdbc = decimal.Parse("-9999999.99");
            }
            try
            {
                marginbtp = decimal.Parse(tbMarginBTPCost.Text.Replace("%", ""));
            }
            catch (Exception ex)
            {
                marginbtp = decimal.Parse("-9999999.99");
            }
            try
            {
                margindbc = decimal.Parse(tbMarginDBCCost.Text.Replace("%", ""));
            }
            catch (Exception ex)
            {
                margindbc = decimal.Parse("-9999999.99");
            }

            if (callfrom.ToUpper() == "UNITPRICE")
            {
                txtPrice.Text = price.ToString("F");
                if (price <= -999999)
                {
                    if (costbtp > -999999 && marginbtp > -999999)
                    {
                        //price = (cost + (marginbtp / 100) * cost) * gst;
                        if (!marginbtp.Equals(100))
                        {
                            price = (costbtp / (1 - marginbtp / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            marginbtp = Math.Round(100 * ((price / gst) - costbtp) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMarginBTPCost.Text = marginbtp.ToString() + "%";
                        }
                    }
                    if (costdbc > -999999 && margindbc > -999999)
                    {
                        //price = (cost + (margindbc / 100) * cost) * gst;
                        if (!margindbc.Equals(100))
                        {
                            price = (costdbc / (1 - margindbc / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            margindbc = Math.Round(100 * ((price / gst) - costdbc) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMarginDBCCost.Text = margindbc.ToString() + "%";
                        }
                    }
                }
                else
                {
                    if (costbtp < -999999 && marginbtp > -999999)
                    {
                        costbtp = (price / gst) * (1 - marginbtp / 100);
                        txtBTPCostExcGST.Text = costbtp.ToString("F");
                    }
                    else if (costbtp > -999999)
                    {
                        if (price != 0)
                        {
                            marginbtp = Math.Round(100 * ((price / gst) - costbtp) / (price / gst), 2);
                            tbMarginBTPCost.Text = marginbtp.ToString() + "%";
                        }
                        else
                        {
                            tbMarginBTPCost.Text = "";
                        }
                    }
                    if (costdbc < -999999 && margindbc > -999999)
                    {
                        costdbc = (price / gst) * (1 - margindbc / 100);
                        txtDBCCostExcGST.Text = costdbc.ToString("F");
                    }
                    else if (costdbc > -999999)
                    {
                        if (price != 0)
                        {
                            margindbc = Math.Round(100 * ((price / gst) - costdbc) / (price / gst), 2);
                            tbMarginDBCCost.Text = margindbc.ToString() + "%";
                        }
                        else
                        {
                            tbMarginDBCCost.Text = "";
                        }
                    }
                }
            }
            else if (callfrom.ToUpper() == "COST")
            {
                if (costbtp <= -999999)
                {
                    if (price > -999999 && marginbtp > -999999)
                    {
                        costbtp = (price / gst) * (1 - marginbtp / 100);
                        txtBTPCostExcGST.Text = costbtp.ToString("F");
                    }
                    if (price > -999999 && margindbc > -999999)
                    {
                        costdbc = (price / gst) * (1 - margindbc / 100);
                        txtDBCCostExcGST.Text = costdbc.ToString("F");
                    }
                }
                else
                {
                    // BTP Cost
                    if (price < -999999 && marginbtp > -999999)
                    {
                        //price = (cost + (margin / 100) * cost) * gst;
                        if (!marginbtp.Equals(100))
                        {
                            price = (costbtp / (1 - marginbtp / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            marginbtp = Math.Round(100 * ((price / gst) - costbtp) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMarginBTPCost.Text = marginbtp.ToString() + "%";
                        }
                    }
                    else if (price > -999999)
                    {
                        if (price != 0)
                        {
                            marginbtp = Math.Round(100 * ((price / gst) - costbtp) / (price / gst), 2);
                            tbMarginBTPCost.Text = marginbtp.ToString() + "%";
                        }
                        else
                        {
                            tbMarginBTPCost.Text = "";
                        }
                    }
                    // DBC Cost
                    if (price < -999999 && margindbc > -999999)
                    {
                        //price = (cost + (margin / 100) * cost) * gst;
                        if (!margindbc.Equals(100))
                        {
                            price = (costdbc / (1 - margindbc / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            margindbc = Math.Round(100 * ((price / gst) - costdbc) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMarginDBCCost.Text = margindbc.ToString() + "%";
                        }
                    }
                    else if (price > -999999)
                    {
                        if (price != 0)
                        {
                            margindbc = Math.Round(100 * ((price / gst) - costdbc) / (price / gst), 2);
                            tbMarginDBCCost.Text = margindbc.ToString() + "%";
                        }
                        else
                        {
                            tbMarginDBCCost.Text = "";
                        }
                    }
                }
            }
            else if (callfrom.ToUpper() == "BTPCostMARGIN")
            {
                if (costbtp <= -999999 || costbtp == 0)
                {
                    if (price > -999999 && marginbtp > -999999)
                    {
                        costbtp = (price / gst) * (1 - marginbtp / 100);
                        txtBTPCostExcGST.Text = costbtp.ToString("F");
                    }
                }
                else
                {
                    if (marginbtp > -999999)
                    {
                        if (marginbtp != 100 && costbtp != 0)
                        {
                            price = (costbtp / (1 - marginbtp / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            marginbtp = Math.Round(100 * ((price / gst) - costbtp) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMarginBTPCost.Text = marginbtp.ToString() + "%";
                        }

                    }
                    else
                    {
                        if (price > -999999 && costbtp > -999999 && price != 0)
                        {
                            marginbtp = Math.Round(100 * ((price / gst) - costbtp) / (price / gst), 2);
                            tbMarginBTPCost.Text = marginbtp.ToString() + "%";
                        }
                    }
                }
            }
            else if (callfrom.ToUpper() == "DBCCostMARGIN")
            {
                if (costdbc <= -999999 || costdbc == 0)
                {
                    if (price > -999999 && margindbc > -999999)
                    {
                        costdbc = (price / gst) * (1 - margindbc / 100);
                        txtDBCCostExcGST.Text = costdbc.ToString("F");
                    }
                }
                else
                {
                    if (margindbc > -999999)
                    {
                        if (margindbc != 100 && costdbc != 0)
                        {
                            price = (costdbc / (1 - margindbc / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            margindbc = Math.Round(100 * ((price / gst) - costdbc) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMarginDBCCost.Text = margindbc.ToString() + "%";
                        }

                    }
                    else
                    {
                        if (price > -999999 && costdbc > -999999 && price != 0)
                        {
                            margindbc = Math.Round(100 * ((price / gst) - costdbc) / (price / gst), 2);
                            tbMarginDBCCost.Text = margindbc.ToString() + "%";
                        }
                    }
                }
            }
        }

        private void btnPrintMargin_Click(object sender, RoutedEventArgs e)
        {
            string bcForecastDate = string.Empty;

            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetBCForecastDateCompleted += delegate (object o, GetBCForecastDateCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    bcForecastDate = es.Result;
                }
                System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintMargin.aspx?revisionid=" + EstimateList.SelectedEstimateRevisionId.ToString() + "&bcForecastDate=" + bcForecastDate, UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
            };

            mrsClient.GetBCForecastDateAsync(EstimateList.SelectedContractNumber.ToString());

            //this.DialogResult = true;
            //RadWindow window = this.ParentOfType<RadWindow>();
            //if (window != null)
            //    window.Close();
        }

        private void txtMarginBTPCost_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "BTPCostMARGIN");
            updateSubTotal(sender);
        }

        private void txtMarginDBCcost_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "DBCcostMARGIN");
            updateSubTotal(sender);
        }

        private void txtPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "UNITPRICE");
            updateSubTotal(sender);
        }
        private void txtBTPCostExcGST_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "COST");
        }
        private void txtDBCCostExcGST_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "COST");
        }
        private void txtQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            updateSubTotal(sender);
        }

        private void updateSubTotal(object sender)
        {
            decimal qty, price;

            TextBox txtbox = (TextBox)sender;
            EstimateDetails item = (EstimateDetails)txtbox.DataContext;

            //item.UpdatedTotalPrice = item.UpdatedQuantity * item.UpdatedPrice;
            StackPanel panel = txtbox.ParentOfType<StackPanel>();

            TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
            TextBox txtQty = (TextBox)panel.FindName("txtQuantity");
            TextBox txtSubtotal = (TextBox)panel.FindName("txtSubtotal");
            try
            {
                qty = decimal.Parse(txtQty.Text);
            }
            catch
            {
                RadWindow.Alert("Please enter a valid quantity!");
                return;
            }

            try
            {
                price = decimal.Parse(txtPrice.Text);
            }
            catch
            {
                var opend = RadWindowManager.Current.GetWindows();
                if (opend.Count == 0)
                {
                    RadWindow.Alert("Please enter a valid unit price!");
                }
                return;
            }

            if (item.Quantity != item.UpdatedQuantity || item.Price != item.UpdatedPrice || price != item.UpdatedPrice || qty != item.UpdatedQuantity)
            {
                item.UpdatedTotalPrice = Math.Round(qty * price);
                txtSubtotal.Text = String.Format("{0:0.00}", Math.Round(qty * price));
            }
        }


        private void OptionsGrid_ColumnReordered(object sender, GridViewColumnEventArgs e)
        {
            int oldcolumnorder = 0;

            if (gridSaveLayout == "custom")
            {
                foreach (GridColumnOrder o in columnorder)
                {
                    if (o.ColumnName.ToUpper().Replace("INDEX", "").Trim() == e.Column.UniqueName.ToUpper())
                    {
                        oldcolumnorder = o.Order;
                        o.Order = e.Column.DisplayIndex;
                        break;
                    }
                }

                foreach (GridColumnOrder o in columnorder)
                {

                    if (oldcolumnorder > e.Column.DisplayIndex)
                    {
                        if (o.Order == e.Column.DisplayIndex && o.ColumnName.ToUpper().Replace("INDEX", "").Trim() != e.Column.UniqueName.ToUpper())
                        {
                            o.Order = o.Order + 1;
                        }
                        else if (o.Order > e.Column.DisplayIndex && o.Order < oldcolumnorder)
                        {
                            o.Order = o.Order + 1;
                        }
                    }
                    else if (oldcolumnorder < e.Column.DisplayIndex)
                    {
                        if (o.Order <= e.Column.DisplayIndex && o.Order > oldcolumnorder && o.ColumnName.ToUpper().Replace("INDEX", "").Trim() != e.Column.UniqueName.ToUpper())
                        {
                            o.Order = o.Order - 1;
                        }
                    }

                }

                foreach (GridColumnOrder o in columnorder)
                {
                    appSettings[o.ColumnName] = o.Order.ToString();
                }
            }
            else
                SaveLayout(((RadGridView)sender));
        }


        private void btnResetColumnOrder_Click(object sender, RoutedEventArgs e)
        {
            appSettings["ColAreaIndex"] = "1";
            appSettings["ColGroupIndex"] = "2";
            appSettings["ColProductNameIndex"] = "3";
            appSettings["ColProductDescIndex"] = "4";
            appSettings["ColAdditionalNotesIndex"] = "5";
            appSettings["ColQuantityIndex"] = "6";
            appSettings["ColPriceIndex"] = "7";
            appSettings["ColTotalIndex"] = "8";
            appSettings["ColMarginPercentIndex"] = "9";
            appSettings["ColUomIndex"] = "10";
            appSettings["ColAcceptedIndex"] = "11";
            appSettings["ColSoSiIndex"] = "12";
            appSettings["ColRemoveIndex"] = "13";
            appSettings["ColCopyIndex"] = "14";
            appSettings["ColChangesIndex"] = "15";
            appSettings["ColReplaceIndex"] = "16";

            //appSettings["ColCopy"] = "60";

            OptionsGrid_Loaded(sender, e);
        }

        private void btnSetStudioMAnswer_Click(object sender, RoutedEventArgs e)
        {
            ((EstimateViewModel)LayoutRoot.DataContext).SetDefaultAnswerToStudioMProducts("");
        }

        private void btnAnswerAll_Click(object sender, RoutedEventArgs e)
        {
            ((EstimateViewModel)LayoutRoot.DataContext).SetDefaultAnswerToStudioMProducts("ALL");
        }

        private void btnSpellCheck_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            SpellCheck spellCheckDlg = new SpellCheck(item.RecordId);
            win.Header = "Spell Check";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = spellCheckDlg;
            win.ResizeMode = ResizeMode.NoResize;
            win.Closed += new EventHandler<WindowClosedEventArgs>(SpellCheckWin_Closed);
            win.ShowDialog();
        }

        void SpellCheckWin_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                ((EstimateViewModel)LayoutRoot.DataContext).ForceRefreshTab();
        }

        private void OtherOptionsPane_Loaded(object sender, RoutedEventArgs e)
        {
            ((EstimateViewModel)LayoutRoot.DataContext).loadPriceRegion();
            ((EstimateViewModel)LayoutRoot.DataContext).loadHomes();
        }

        private void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRegion.Text.Trim() == "" || cmbHome.Text.Trim() == "")
            {
                MessageBox.Show("Please select price region and home.");
            }
            else
            {

                ((EstimateViewModel)LayoutRoot.DataContext).SearchOtherHomeProducts(cmbRegion.SelectedValue.ToString(), cmbHome.SelectedValue.ToString());

                var areaDescriptor = new GroupDescriptor()
                {
                    Member = "AreaName"
                };
                RadGridOtherOption.GroupDescriptors.Add(areaDescriptor);
                var groupDescriptor = new GroupDescriptor()
                {
                    Member = "GroupName"
                };
                RadGridOtherOption.GroupDescriptors.Add(groupDescriptor);
            }
        }

        private void RadGridOtherOption_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            if (e.AvailableOperators.Contains(FilterOperator.Contains))
                e.DefaultOperator1 = FilterOperator.Contains;
        }

        private void RadGridView_Filtered(object sender, GridViewFilteredEventArgs e)
        {
            if (e.ColumnFilterDescriptor != null)
            {
                var column = (GridViewBoundColumnBase)e.ColumnFilterDescriptor.Column as GridViewBoundColumnBase;
                var columnHeader = ((RadGridView)sender).ChildrenOfType<GridViewHeaderCell>().Where(headerCell => headerCell.Column == column).FirstOrDefault();

                if (columnHeader != null)
                {
                    if (e.ColumnFilterDescriptor.IsActive == true)
                        columnHeader.Background = new RadialGradientBrush(Color.FromArgb(75, 255, 255, 71), Color.FromArgb(75, 255, 255, 0));
                    else
                        columnHeader.Background = null;
                }
            }
            if (((RadGridView)sender).Name == "OptionsGrid")
                SaveLayout(((RadGridView)sender));
        }

        private void btnReplace_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            EstimateDetails ed = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as EstimateDetails;
            deletedED = ed;
            ReplaceEstimateItem acceptDlg = new ReplaceEstimateItem(item.RecordId, ed);
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Replace Estimate Item";
            win.Content = acceptDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_ReplaceOptionClosed);
            win.ShowDialog();
        }
        //private void chkdisplay_Checked(object sender, RoutedEventArgs e)
        //{
        //    ((EstimateViewModel)LayoutRoot.DataContext).FilterHomes((bool)chkdisplay.IsChecked);
        //}

        private void Estimates_Sorted(object sender, GridViewSortedEventArgs e)
        {
            if (gridSaveLayout == "telerik")
                SaveLayout(((RadGridView)sender));
        }
        private void SaveLayout(RadGridView rgv)
        {
            // Obtain the isolated storage for an application.
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                //IsolatedStorageFileStream s = store.OpenFile(@"GridsCommand.txt", FileMode.Create, FileAccess.Write);
                using (Stream stream = new IsolatedStorageFileStream(@"EstimatesGridsCommand.txt", FileMode.Create, FileAccess.Write, store))
                {
                    if (stream != null)
                    {
                        PersistenceManager manager = new PersistenceManager();
                        Stream str = manager.Save(rgv);

                        CopyStream(str, stream);
                    }
                }
            }
        }
        private void LoadLayout(RadGridView rgv)
        {
            // Obtain the isolated storage for an application.
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists("EstimatesGridsCommand.txt"))
                {
                    using (Stream stream = new IsolatedStorageFileStream(@"EstimatesGridsCommand.txt", FileMode.Open, FileAccess.Read, store))
                    {
                        if (stream != null)
                        {
                            PersistenceManager manager = new PersistenceManager();
                            manager.Load(rgv, stream);
                        }
                    }
                }
            }
        }
        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, len);
            }
        }

        private void cmbPriceDisplay_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RadComboBox radComboBoxPriceDisplayCode = (RadComboBox)sender;
            EstimateDetails ed = (EstimateDetails)radComboBoxPriceDisplayCode.DataContext;
            StackPanel panel = radComboBoxPriceDisplayCode.ParentOfType<StackPanel>();
            TextBox txtDBCCostExcGST = (TextBox)panel.FindName("txtDBCCostExcGST");
            if ((ed.Uom == "NT" || radComboBoxPriceDisplayCode.SelectedValue.ToString() == "5") && (string.IsNullOrWhiteSpace(txtDBCCostExcGST.Text.ToString())))
                txtDBCCostExcGST.Text = "0";
        }

        private void RadGridSearchProduct_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {

        }

        private void RadGridSearchProduct_FilterOperatorsLoading_1(object sender, FilterOperatorsLoadingEventArgs e)
        {

        }

        private void SearchProductPane_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnLoadSearchProduct_Click(object sender, RoutedEventArgs e)
        {
            if (cmbRegionSearchProduct.Text.Trim() == "")
            {
                MessageBox.Show("Please select price region.");
            }
            else
            {
                ((EstimateViewModel)LayoutRoot.DataContext).SearchAllProducts(cmbRegionSearchProduct.SelectedValue.ToString(), textboxProduct.Text.ToString());
            }

        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            AddProductInLieuOfStandardPromotion addDlg = new AddProductInLieuOfStandardPromotion(EstimateList.SelectedEstimateRevisionId);
            win.Header = "Add Product in lieu of Standard or Promotion";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = addDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(CopyFromAvailableOptionTreeWin_Closed);
            win.ShowDialog();
        }
        private void showAuditLog(EstimateDetails item)
        {
            RadTabItem tabItem = (RadTabItem)rd.FindName("radTabItemAuditlog");
            if (tabItem != null)
            {
                var estimateRevisionDetailsId = item.EstimateRevisionDetailsId;

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
                mrsClient.GetAuditLogsCompleted += new EventHandler<GetAuditLogsCompletedEventArgs>(mrsClient_GetAuditLogCompleted);
                mrsClient.GetAuditLogsAsync(EstimateList.SelectedEstimateRevisionId, estimateRevisionDetailsId);

                tabItem.Visibility = Visibility.Visible;
                tabItem.IsSelected = true;
                tabItem.Focus();
            }
        }

        void mrsClient_GetAuditLogCompleted(object sender, GetAuditLogsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                RadGridView gridAuditLog = (RadGridView)rd.FindName("gridAuditLog");
                if (gridAuditLog != null)
                {
                    gridAuditLog.ItemsSource = e.Result;
                }
                else
                {

                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetAuditLogCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void btnEnterMargin_Click(object sender, RadRoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            EnterMarginAttributes enterMarginDlg = new EnterMarginAttributes(EstimateList.SelectedEstimateRevisionId, (EstimateViewModel)LayoutRoot.DataContext);

            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Enter Margin Attributes Window";
            win.Content = enterMarginDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(winEnterMargin_Closed);
            win.ShowDialog();
        }

        void winEnterMargin_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                NavigationService.Refresh();
        }

        private void btnRounding_Click(object sender, RadRoutedEventArgs e)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            mrsClient.ApplyRoundingCompleted += delegate(object o, ApplyRoundingCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    ((EstimateViewModel)LayoutRoot.DataContext).RefreshHeaderInfo();

                    foreach (EstimateDetails ed in ((EstimateViewModel)LayoutRoot.DataContext)._upgradeSource)
                    {
                        decimal temp =  ed.TotalPrice.Round(0, MidpointRounding.AwayFromZero);
                        ed.TotalPrice = temp;
                        ed.UpdatedTotalPrice = temp;
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "ApplyRoundingCompleted");
            };

            mrsClient.ApplyRoundingAsync(EstimateList.SelectedEstimateRevisionId);
        }


        //private void txtSubtotal_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    decimal qty, price, total;

        //    TextBox txtbox = (TextBox)sender;
        //    EstimateDetails item = (EstimateDetails)txtbox.DataContext;

        //    //item.UpdatedTotalPrice = item.UpdatedQuantity * item.UpdatedPrice;
        //    StackPanel panel = txtbox.ParentOfType<StackPanel>();

        //    TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
        //    TextBox txtQty = (TextBox)panel.FindName("txtQuantity");
        //    TextBox txtSubtotal = (TextBox)panel.FindName("txtSubtotal");
        //    try
        //    {
        //        qty = decimal.Parse(txtQty.Text);
        //    }
        //    catch
        //    {
        //        RadWindow.Alert("Please enter a valid quantity!");
        //        return;
        //    }

        //    try
        //    {
        //        price = decimal.Parse(txtPrice.Text);
        //    }
        //    catch
        //    {
        //        var opend = RadWindowManager.Current.GetWindows();
        //        if (opend.Count == 0)
        //        {
        //            RadWindow.Alert("Please enter a valid unit price!");
        //        }
        //        return;
        //    }

        //    try
        //    {
        //        total = decimal.Parse(txtSubtotal.Text);
        //    }
        //    catch
        //    {
        //        RadWindow.Alert("Please enter a valid subtotal!");
        //        return;
        //    }

        //    if (total - (qty * price) < -1 || total - qty * price > 1)
        //    {
        //        RadWindow.Alert("Subtotal does not equal quantity times unit price.");
        //        return;
        //    }
        //    else
        //    {
        //        item.UpdatedTotalPrice = total;
        //    }
        //}
 

    }

    public enum MidpointRounding
    {
        ToEven,
        AwayFromZero
    }
    public static class DecimalExtensions
    {
        public static decimal Round(this decimal d, MidpointRounding mode)
        {
            return d.Round(0, mode);
        }

        /// <summary>
        /// Rounds using arithmetic (5 rounds up) symmetrical (up is away from zero) rounding
        /// </summary>
        /// <param name="d">A Decimal number to be rounded.</param>
        /// <param name="decimals">The number of significant fractional digits (precision) in the return value.</param>
        /// <returns>The number nearest d with precision equal to decimals. If d is halfway between two numbers, then the nearest whole number away from zero is returned.</returns>
        public static decimal Round(this decimal d, int decimals, MidpointRounding mode)
        {
            if (mode == MidpointRounding.ToEven)
            {
                return decimal.Round(d, decimals);
            }
            else
            {
                decimal factor = Convert.ToDecimal(Math.Pow(10, decimals));
                int sign = Math.Sign(d);
                return Decimal.Truncate(d * factor + 0.5m * sign) / factor;
            }
        }
    }
}
