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
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using System.ComponentModel;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class ReplaceEstimateItem : ChildWindow
    {
        private BackgroundWorker optionTreeWorker = new BackgroundWorker();
        public RetailSystemClient mrsClient = null;
        private List<EstimateDetails> _optiontreeSource;
        public int estimaterevisionid = 0;
        public ParameterClass p = new ParameterClass();
        RadGridView radGridViewSelected;
        EstimateDetails estimateSource;

        public ReplaceEstimateItem(int revisionid, EstimateDetails edSource)
        {
            estimaterevisionid = revisionid;
            estimateSource = edSource;

            InitializeComponent();

            if (LayoutRoot != null)
                ((ReplaceEstimateItemViewModel)LayoutRoot.DataContext).IsBusyOptionTree = true;
            textBlockProductID.Text = edSource.ProductId.ToString();
            radioButtonSameAreaGroup.IsChecked = true;
        }

        private void radioButtonSameAreaGroup_Checked(object sender, RoutedEventArgs e)
        {
            if (mrsClient == null)
            {
                mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            }
            mrsClient.GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompleted += new EventHandler<GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompletedEventArgs>(mrsClient_GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompleted);
            mrsClient.GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceAsync(EstimateList.SelectedEstimateRevisionId.ToString(), estimateSource.AreaName, estimateSource.GroupName);
            radGridViewSelected = RadGridView1;

            //var areaDescriptor = new GroupDescriptor()
            //{
            //    Member = "AreaName"
            //};
            //radGridViewSelected.GroupDescriptors.Add(areaDescriptor);
            //var groupDescriptor = new GroupDescriptor()
            //{
            //    Member = "GroupName"
            //};
            //radGridViewSelected.GroupDescriptors.Add(groupDescriptor);
        }

        private void radioButtonAllAreaGroup_Checked(object sender, RoutedEventArgs e)
        {
            if (mrsClient == null)
            {
                mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
            }
            mrsClient.GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompleted += new EventHandler<GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompletedEventArgs>(mrsClient_GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompleted);
            mrsClient.GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceAsync(EstimateList.SelectedEstimateRevisionId.ToString(), "", "");
            ((ReplaceEstimateItemViewModel)LayoutRoot.DataContext).IsBusyOptionTree = true;
            radGridViewSelected = RadGridView1;
        }

        void mrsClient_GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompleted(object sender, GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ((ReplaceEstimateItemViewModel)LayoutRoot.DataContext).IsBusyOptionTree = true;
                _optiontreeSource = new List<EstimateDetails>();

                List<EstimateDetails> optiontreeTemp = new List<EstimateDetails>();
                int index = 0;
                foreach (OptionTreeProducts p in e.Result)
                {
                    //if (++index > 1000)
                    //    break;
                    EstimateDetails ed = new EstimateDetails();
                    ed.ProductDescription = p.D;
                    ed.ProductName = p.N;
                    ed.Price = p.P;
                    ed.AreaName = p.A;
                    ed.GroupName = p.G;
                    ed.AreaId = p.AI;
                    ed.GroupId = p.GI;
                    ed.StandardOption = p.S;
                    ed.Quantity = p.Q;
                    ed.HomeDisplayOptionId = p.I;
                    ed.SiteWorkItem = p.W;
                    ed.StudioMSortOrder = p.O;
                    ed.Uom = p.U;
                    //ed.CostExcGST = p.CG;
                    //ed.DerivedCost = p.DC;

                    if (p.M == 0) // Not Studio M
                    {
                        ed.StudioMProduct = false;
                    }
                    else if (p.M == 1) // Studio M Manadatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch.png";
                        ed.StudioMTooltips = "Studio M Product. Question not answered yet.";
                    }
                    else if (p.M == 2) // Studio M No Question
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/green_box.png";
                        ed.StudioMTooltips = "There are no studio M questions.";
                    }
                    else if (p.M == 3) // Studio M Non-Mandatory
                    {
                        ed.StudioMProduct = true;
                        ed.StudioMIcon = "./images/color_swatch_gray.png";
                        ed.StudioMTooltips = "Studio M Product. Answers are not mandatory.";
                    }

                    if (p.S)
                    {
                        ed.SOSI = "./images/upgrade.png";
                        ed.SOSIToolTips = "Upgrade Option.";
                    }

                    //if (p.DC)
                    //{
                    //    ed.DerivedCostIcon = "./images/link.png";
                    //    ed.DerivedCostTooltips = "Derived Cost.";
                    //}
                    //else
                    //{
                    //    ed.DerivedCostIcon = "./images/spacer.gif";
                    //    ed.DerivedCostTooltips = "";
                    //}
                    optiontreeTemp.Add(ed);
                }

                //optiontreeTemp[0].Changed = true;
                // Copy full option tree to re-use later
                _optiontreeSource.AddRange(optiontreeTemp);

                //PAGGrid.ItemsSource = _optiontreeSource;

                radGridViewSelected.ItemsSource = _optiontreeSource;
                if (!optionTreeWorker.IsBusy)
                    // Build Option Tree in another thread 
                    optionTreeWorker.RunWorkerAsync();
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetOptionTreeAsOptionTreeProductsCompleted");
            }


            // Remove Event Handler
            mrsClient.GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompleted -= new EventHandler<GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompletedEventArgs>(mrsClient_GetOptionTreeAsOptionTreeProductsForEstimateItemReplaceCompleted);

            ((ReplaceEstimateItemViewModel)LayoutRoot.DataContext).IsBusyOptionTree = false;
        }

        private void OptionsGrid_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
        {
            if (e.Visibility == Visibility.Visible)
            {
                GridViewRow row = e.Row as GridViewRow;

                if (row != null)
                {
                    SimplePAG ed = row.DataContext as SimplePAG;
                    decimal retailprice = 0;

                    if (row != null && ed != null)
                    {
                        StackPanel panel = (StackPanel)e.DetailsElement;
                        RadTabControl rtc = (RadTabControl)panel.FindName("tabDesc");
                        CheckBox chksitework = (CheckBox)panel.FindName("chkSitework");

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
                        GetNonStandardAreas(cmbcategory, ed.AreaID);

                        RadComboBox cmbPriceDisplay = (RadComboBox)panel.FindName("cmbPriceDisplay");
                        cmbPriceDisplay.ItemsSource = ((AddOptionViewModel)LayoutRoot.DataContext).EstimateNonStandardPriceDisplayCode;
                        cmbPriceDisplay.SelectedValue = ed.PriceDisplayCodeId;

                        RadComboBox cmbgroup = (RadComboBox)panel.FindName("cmbGroup");
                        GetNonStandardGroups(ed.AreaID, cmbgroup, ed.GroupID);

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

                        if (ed.AreaName.ToUpper().Contains("NON STANDARD REQUEST"))
                        {
                            cmbcategory.IsEnabled = true;
                            cmbgroup.IsEnabled = true;
                            if (txtdesc != null)
                                txtdesc.IsReadOnly = false;
                            chksitework.IsEnabled = true;


                        }
                        else
                        {
                            cmbcategory.IsEnabled = false;
                            chksitework.IsEnabled = false;
                            cmbgroup.IsEnabled = false;
                        }

                        if (ed.ItemAllowToChangeDescription)
                        {
                            txtdesc.IsReadOnly = false;
                        }
                        else
                        {
                            if (txtdesc != null)
                                txtdesc.IsReadOnly = true;
                        }

                        TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
                        retailprice = decimal.Parse(txtPrice.Text);
                        if (!ed.ItemAllowToChangePrice)
                        {
                            txtPrice.IsReadOnly = true;

                        }
                        else
                        {
                            txtPrice.IsReadOnly = false;
                        }

                        TextBox txtQty = (TextBox)panel.FindName("txtQuantity");
                        if (!ed.ItemAllowToChangeQuantity)
                        {
                            txtQty.IsReadOnly = true;
                        }
                        else
                        {
                            txtQty.IsReadOnly = false;
                        }

                        TextBox txtCost = (TextBox)panel.FindName("txtCostExcGST");
                        TextBlock lblCost = (TextBlock)panel.FindName("lblcost");

                        TextBox txtMargin = (TextBox)panel.FindName("txtMargin");
                        TextBlock lblMargin = (TextBlock)panel.FindName("lblmargin");

                        if ((App.Current as App).CurrentRoleAccessModule.AccessMarginModule && ed.ItemAllowToChangePrice)// only sales estimator can change cost
                        {
                            txtCost.IsReadOnly = false;
                            txtMargin.IsEnabled = true;
                        }
                        else
                        {
                            txtCost.IsReadOnly = true;
                            txtMargin.IsEnabled = false;
                        }


                        CheckBox derivedcost = (CheckBox)panel.FindName("chkDerivedCost");
                        TextBlock lblderivedcost = (TextBlock)panel.FindName("lblderivedcost");

                        if (((App)App.Current).CurrentRoleAccessModule.AccessMarginModule)
                        {
                            if (ed.Margin.Trim() != "" && retailprice >= 0)
                            {
                                txtMargin.Text = ed.Margin.ToString() + "%";
                            }
                            else
                            {
                                txtMargin.Text = "";
                            }

                            lblMargin.Visibility = Visibility.Visible;
                            txtMargin.Visibility = Visibility.Visible;
                            lblCost.Visibility = Visibility.Visible;
                            txtCost.Visibility = Visibility.Visible;
                            lblderivedcost.Visibility = Visibility.Visible;
                            derivedcost.Visibility = Visibility.Visible;

                        }
                        else
                        {
                            lblMargin.Visibility = Visibility.Collapsed;
                            txtMargin.Visibility = Visibility.Collapsed;
                            lblCost.Visibility = Visibility.Collapsed;
                            txtCost.Visibility = Visibility.Collapsed;
                            lblderivedcost.Visibility = Visibility.Collapsed;
                            derivedcost.Visibility = Visibility.Collapsed;

                        }

                        TextBox txtSubtotal = (TextBox)panel.FindName("txtSubtotal");
                        txtSubtotal.IsReadOnly = true;

                        CheckBox chkAccepted = (CheckBox)panel.FindName("chkAccepted");

                        if (!EstimateList.revisiontypepermission.ReadOnly && (App.Current as App).SelectedEstimateAllowToAcceptItem)
                        {
                            chkAccepted.IsEnabled = true;
                        }
                        else
                        {
                            chkAccepted.IsEnabled = false;
                        }
                    }
                }
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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DataContext = p;
                window.DialogResult = false;
                window.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DataContext = p;
                window.DialogResult = false;
                window.Close();
            }
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

        private void RadGridView1_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            if (e.AvailableOperators.Contains(FilterOperator.Contains))
                e.DefaultOperator1 = FilterOperator.Contains;
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
                }
            }
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
        }

        private void btnAddOption_Click(object sender, RoutedEventArgs e)
        {
            RadWindow win = new RadWindow();
            EstimateDetails pag = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as EstimateDetails;
            //if (!pag.AreaName.ToUpper().Contains("NON STANDARD"))
            //{
            ReplaceAppOptionFromTree acceptDlg = new ReplaceAppOptionFromTree(estimateSource, pag, "OPTIONTREE", 3);
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Replace Product Window";
            win.Content = acceptDlg;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_AddOptionClosed);
            win.ShowDialog();
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
                        //((EstimateViewModel)LayoutRoot.DataContext).SaveSelectedOptionsFromTreeToEstimate(p.SelectedItemID,
                        //    p.SelectedStandardInclusionID,
                        //    EstimateList.SelectedEstimateRevisionId.ToString(),
                        //    p.StudioMQANDA,
                        //    (App.Current as App).CurrentUserId.ToString(),
                        //    p.SelectedDerivedCosts,
                        //    p.SelectedCostExcGSTs,
                        //    p.SelectedQuantities,
                        //    p.SelectedPrices,
                        //    p.SelectedIsAccepteds,
                        //    p.SelectedAreaIds,
                        //    p.SelectedGroupIds,
                        //    p.SelectedPriceDisplayCodeIds,
                        //    p.SelectedIsSiteWorks,
                        //    p.SelectedProductDescriptions,
                        //    p.SelectedAdditionalNotes,
                        //    p.SelectedExtraDescriptions,
                        //    p.SelectedInternalDescriptions);

                        RadWindow window = this.ParentOfType<RadWindow>();
                        if (window != null)
                        {
                            window.DataContext = p;
                            window.DialogResult = true;
                            window.Close();
                        }
                    }
                }
            }
        }


        private void RadGridOtherOption_FilterOperatorsLoading(object sender, FilterOperatorsLoadingEventArgs e)
        {
            if (e.AvailableOperators.Contains(FilterOperator.Contains))
                e.DefaultOperator1 = FilterOperator.Contains;
        }



    }
}

