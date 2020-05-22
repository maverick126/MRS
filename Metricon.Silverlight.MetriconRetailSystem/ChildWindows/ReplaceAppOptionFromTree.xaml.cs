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
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class ReplaceAppOptionFromTree : ChildWindow
    {
        public ParameterClass p = new ParameterClass();
        public int previoustab, currenttab;
        public RadTabControl rd;
        public string selectedstudiomanswer;
        public string studiomquestionxml;
        public string studiomanswerxml;
        public string parameteraction;
        public RetailSystemClient mrsClient;
        public ObservableCollection<StudioMSupplierBrand> suplist = new ObservableCollection<StudioMSupplierBrand>();
        public ObservableCollection<StudioMQuestion> qulist = new ObservableCollection<StudioMQuestion>();
        public ObservableCollection<StudioMAnswer> awlist = new ObservableCollection<StudioMAnswer>();
        public ObservableCollection<StudioMAnswer> selectedaw = new ObservableCollection<StudioMAnswer>();
        EstimateDetails estimateSource;

        public ReplaceAppOptionFromTree(EstimateDetails edSource, EstimateDetails pag, string action, int currentTab)
        {
            estimateSource = edSource;
            p.SelectedPAG = pag;
            parameteraction = action;
            AddOptionViewModel apm = new AddOptionViewModel(
                pag.HomeDisplayOptionId.ToString(), 
                EstimateList.SelectedEstimateRevisionId.ToString(), 
                currentTab);

            InitializeComponent();
            this.DataContext = apm;
        }

        void MRSclient_GetStudioMQandACompleted(object sender, GetStudioMQandACompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result != null)
                {
                    studiomquestionxml = e.Result;
                }
            }
            else
            {
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetStudioMQandACompleted");
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();

            string selecteditemids = "";
            string selectedstandardinclusionids = "";
            string selectedderivedcosts = "";
            string selectedbtpcostexcgsts = "";
            string selecteddbccostexcgsts = "";
            string selectedquantities = "";
            string selectedprices = "";
            string selectedisaccepteds = "";
            string selectedareaids = "";
            string selectedgroupids = "";
            string selectedpricedisplaycodeids = "";
            string selectedissiteworks = "";
            string selectedproductdescriptions = "";
            string selectedadditionalnotes = "";
            string selectedextradescriptions = "";
            string selectedinternaldescriptions = "";
            int itemCount = 0;
            foreach (var item in ((AddOptionViewModel)LayoutRoot.DataContext).OptionList)
            {
                if (item.Selected) // here reuse this column to hold the selection of check box
                {
                    if (item.HomeDisplayOptionID > 0)
                    {
                        if (item.PriceDisplayCodeId != 10 && item.Price != 0)
                        {
                            RadWindow.Confirm("The selected Price Display Code does not allow Unit Price.\r\nDo you want to reset the Unit Price to 0?", new EventHandler<WindowClosedEventArgs>((s, args) =>
                            {
                                if (args.DialogResult == true)
                                {
                                    item.Price = 0.00m;
                                    item.TotalPrice = 0.00m;
                                    item.CostBTPExcGST = 0.00m;
                                    item.Margin = "";
                                }
                                else
                                {
                                    RadWindow.Alert("Please change Price Display Code or Unit Price and try to add product again.");
                                }
                            }));

                            return;
                        }
                        
                        if (itemCount == 0)
                        {
                            selecteditemids = item.HomeDisplayOptionID.ToString();
                            selectedderivedcosts = item.DerivedCost ? "1" : "0";
                            selectedbtpcostexcgsts = item.CostBTPExcGST.ToString();
                            selecteddbccostexcgsts = item.CostDBCExcGST.ToString();
                            selectedprices = item.Price.ToString();
                            selectedisaccepteds = item.IsAccepted ? "1" : "0";
                            selectedareaids = item.AreaID.ToString();
                            selectedgroupids = item.GroupID.ToString();
                            selectedpricedisplaycodeids = item.PriceDisplayCodeId.ToString();
                            selectedissiteworks = item.IsSiteWork ? "1" : "0";
                            selectedproductdescriptions = (item.ProductDescription ?? "").ToString();
                                selectedquantities = item.Quantity.ToString();
                                selectedinternaldescriptions = (item.InternalDescription ?? "").ToString();
                                selectedadditionalnotes = (item.AdditionalNotes ?? "").ToString();
                                selectedextradescriptions = (item.ExtraDescription ?? "").ToString();
                        }
                        else
                        {
                            selecteditemids = selecteditemids + "#$#!" + item.HomeDisplayOptionID.ToString();
                            selectedderivedcosts = selectedderivedcosts + "#$#!" + (item.DerivedCost ? "1" : "0");
                            selectedbtpcostexcgsts = selectedbtpcostexcgsts + "#$#!" + item.CostBTPExcGST.ToString();
                            selecteddbccostexcgsts = selecteddbccostexcgsts + "#$#!" + item.CostBTPExcGST.ToString();
                            selectedprices = selectedprices + "#$#!" + item.Price.ToString();
                            selectedisaccepteds = selectedisaccepteds + "#$#!" + (item.IsAccepted ? "1" : "0");
                            selectedareaids = selectedareaids + "#$#!" + item.AreaID.ToString();
                            selectedgroupids = selectedgroupids + "#$#!" + item.GroupID.ToString();
                            selectedpricedisplaycodeids = selectedpricedisplaycodeids + "#$#!" + item.PriceDisplayCodeId.ToString();
                            selectedissiteworks = selectedissiteworks + "#$#!" + (item.IsSiteWork ? "1" : "0");
                            selectedproductdescriptions = selectedproductdescriptions + "#$#!" + (item.ProductDescription ?? "").ToString();
                                                                                   
                                selectedquantities = selectedquantities + "#$#!" + item.Quantity.ToString();
                                selectedinternaldescriptions = selectedinternaldescriptions + "#$#!" + (item.InternalDescription ?? "").ToString();
                                selectedadditionalnotes = selectedadditionalnotes + "#$#!" + (item.AdditionalNotes ?? "").ToString();
                                selectedextradescriptions = selectedextradescriptions + "#$#!" + (item.ExtraDescription ?? "").ToString();
                        }

                        itemCount++;

                    }
                    else if (item.StandardInclusionsID > 0)
                    {
                        if (selectedstandardinclusionids == "")
                            selectedstandardinclusionids = item.StandardInclusionsID.ToString();        
                        else
                            selectedstandardinclusionids = selectedstandardinclusionids + "#$#!" + item.StandardInclusionsID.ToString();
                    }
                }
            }
            p.SelectedItemID = selecteditemids;
            p.SelectedStandardInclusionID = selectedstandardinclusionids;
            p.SelectedDerivedCosts = selectedderivedcosts;
            p.SelectedBTPCostExcGSTs = selectedbtpcostexcgsts;
            p.SelectedDBCCostExcGSTs = selecteddbccostexcgsts;
            p.SelectedQuantities = selectedquantities;
            p.SelectedPrices = selectedprices;
            p.SelectedIsAccepteds = selectedisaccepteds;
            p.SelectedAreaIds = selectedareaids;
            p.SelectedGroupIds = selectedgroupids;
            p.SelectedPriceDisplayCodeIds = selectedpricedisplaycodeids;
            p.SelectedIsSiteWorks = selectedissiteworks;
            p.SelectedProductDescriptions = selectedproductdescriptions;
            p.SelectedAdditionalNotes = selectedadditionalnotes;
            p.SelectedExtraDescriptions = selectedextradescriptions;
            p.SelectedInternalDescriptions = selectedinternaldescriptions;
            p.CopyQuantity = checkBoxCopyQuantity.IsChecked==true ? "1" : "0";
            p.CopyAdditionalNotes = checkBoxCopyAdditionalNotes.IsChecked == true ? "1" : "0";
            p.CopyExtraDescriptions = checkBoxCopyAdditionalDescription.IsChecked == true ? "1" : "0";
            p.CopyInternalNotes = checkBoxCopyInternalNotes.IsChecked == true ? "1" : "0";

            p.StudioMQANDA = "";
            p.Action = parameteraction;
            if (window != null)
            {
                window.DataContext = p;
                window.DialogResult = true;
                window.Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
            {
                window.DataContext = p;
                window.DialogResult = false;
                window.Close();
            }
        }

        private void tab_SelectionChanged(object sender, RadSelectionChangedEventArgs e)
        {
            rd = (RadTabControl)sender;

            if (currenttab != rd.SelectedIndex)
            {
                previoustab = currenttab;
            }
            currenttab = rd.SelectedIndex;

            if (previoustab == 1)
            {
                ContructAnswerXML();
            }

            if (rd.SelectedIndex == 1) // this is studioM tab
            {
                try
                {
                    //ClearAllExistingStudioMCollections();
                    RadTabItem im = (RadTabItem)rd.FindName("studiomtab");

                    Grid gr = (Grid)im.Content;

                    // re build all controls in studiomgrid
                    gr.Children.Clear();
                    Grid gr2 = new Grid();

                    var col = new ColumnDefinition { Width = new GridLength(330) };
                    gr2.ColumnDefinitions.Add(col);

                    col = new ColumnDefinition { Width = new GridLength(10) };
                    gr2.ColumnDefinitions.Add(col);

                    col = new ColumnDefinition { Width = new GridLength(420) };
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

                    gr2.Name = "QAGrid";
                    gr.Children.Add(gr2);
                    Grid.SetColumn(gr2, 0);
                    Grid.SetColumnSpan(gr2, 4);
                    Grid.SetRow(gr2, 3);

                    ClearAllExistingStudioMCollections();
                    //populate the question/answer list and select answer
                    if (suplist.Count == 0)
                    {
                        if (studiomquestionxml != null && studiomquestionxml != "")
                        {
                            XDocument doc = new XDocument();
                            doc = XDocument.Parse(studiomquestionxml);

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


                                IEnumerable<XElement> question = (from q in doc.Descendants("Question") where (string)q.Parent.Parent.Attribute("id") == sup.Attribute("id").Value select q);
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
                    selectedsupplierid = sup.Attribute("id").Value;
                    IEnumerable<XElement> question = (from q in doc.Descendants("Question") where (string)q.Parent.Parent.Attribute("id") == selectedsupplierid select q);
                    foreach (XElement qu in question)
                    {

                        selectedquestionid = qu.Attribute("id").Value;

                        IEnumerable<XElement> answer = (from aw in doc.Descendants("Answer")
                                                        where (string)aw.Parent.Parent.Attribute("id") == selectedquestionid &&
                                                        (string)aw.Parent.Parent.Parent.Parent.Attribute("id") == selectedsupplierid
                                                        orderby (string)aw.Attribute("text")
                                                        select aw);

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

        private void ContructAnswerXML()
        {
            RadTabItem im = (RadTabItem)rd.FindName("studiomtab");
            Grid gr2 = (Grid)im.Content;
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
                answerXML = answerXML + @"<Brand id=""" + selectedsupplierid + @""" name=""" + dpsup.Text.Replace(@"""", @"&quot;") + @""">";
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

                        answerXML = answerXML + @"<Question id=""" + q.QuestionID + @""" text=""" + q.QuestionText.Replace(@"""", @"&quot;") + @""" type=""" + q.QuestionType.Replace(@"""", @"&quot;") + @""">";
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

                //EstimateDetails item = (EstimateDetails)im.DataContext;
                answerXML = answerXML + @"</Questions></Brand></Brands>";
                //item.StudioMAnswer = answerXML;
                selectedstudiomanswer = answerXML;

            }

        }

        private void ClearAllExistingStudioMCollections()
        {

            selectedaw.Clear();
            suplist.Clear();
            awlist.Clear();
            qulist.Clear();
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

                        TextBox txtCostDBC = (TextBox)panel.FindName("txtCostDBCExcGST");
                        TextBlock lblCostDBC = (TextBlock)panel.FindName("lblcostdbc");

                        TextBox txtMargin = (TextBox)panel.FindName("txtMargin");
                        TextBlock lblMargin = (TextBlock)panel.FindName("lblmargin");

                        if ((App.Current as App).CurrentRoleAccessModule.AccessMarginModule && ed.ItemAllowToChangePrice)// only sales estimator can change cost
                        {
                            txtCostDBC.IsReadOnly = false;
                            txtMargin.IsEnabled = true;
                        }
                        else
                        {
                            txtCostDBC.IsReadOnly = true;
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
                            lblCostDBC.Visibility = Visibility.Visible;
                            txtCostDBC.Visibility = Visibility.Visible;
                            lblderivedcost.Visibility = Visibility.Visible;
                            derivedcost.Visibility = Visibility.Visible;

                        }
                        else
                        {
                            lblMargin.Visibility = Visibility.Collapsed;
                            txtMargin.Visibility = Visibility.Collapsed;
                            lblCostDBC.Visibility = Visibility.Collapsed;
                            txtCostDBC.Visibility = Visibility.Collapsed;
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

                                    //create answer dropdown

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


                                    if (selectedanswerforthisquestion.Count > 0 && selectedanswerforthisquestion[0].AnswerID != "")
                                    {
                                        dropanswer.SelectedValue = selectedanswerforthisquestion[0].AnswerID; // single selection only has one answer
                                    }
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
                                    tx2.VerticalAlignment = VerticalAlignment.Center;
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
                                        chk.Height = 16;
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
                            var rw = new RowDefinition { Height = new GridLength(25, GridUnitType.Pixel) };

                            gr.RowDefinitions.Add(rw);

                        }
                    }

                    // add apply answer to all group check box;

                    //if (selectedsupplierid != "0")
                    //{
                    //    //rowindex = rowindex + 1;

                    //    var rw2 = new RowDefinition { Height = new GridLength(20) };
                    //    gr.RowDefinitions.Add(rw2);
                    //    rw2 = new RowDefinition { Height = new GridLength(20) };
                    //    gr.RowDefinitions.Add(rw2);

                    //    TextBlock tbl = new TextBlock();
                    //    tbl.Text = "Apply answer to all groups";
                    //    tbl.FontStyle = FontStyles.Italic;
                    //    tbl.Foreground = new SolidColorBrush(Colors.Orange);
                    //    gr.Children.Add(tbl);
                    //    Grid.SetRow(tbl, rowindex);
                    //    Grid.SetColumn(tbl, 0);

                    //    CheckBox chkAllGroup = new CheckBox();
                    //    chkAllGroup.Name = "chkAll";
                    //    gr.Children.Add(chkAllGroup);
                    //    Grid.SetRow(chkAllGroup, rowindex);
                    //    Grid.SetColumn(chkAllGroup, 2);
                    //}

                }
            }

        }

        private void cmbCategory_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            //((EstimateViewModel)LayoutRoot.DataContext).GetNonStandardGroups(int.Parse(((RadComboBox)sender).SelectedValue.ToString()));
            int defaultgroupid = 0;
            RadComboBox rdbox = (RadComboBox)sender;
            SimplePAG ed = (SimplePAG)rdbox.DataContext;
            StackPanel panel = rdbox.ParentOfType<StackPanel>();
            RadComboBox cmbgroup = (RadComboBox)panel.FindName("cmbGroup");

            if (rdbox.SelectedValue != null)
            {
                if (int.Parse(rdbox.SelectedValue.ToString()) == ed.AreaID)
                {
                    defaultgroupid = ed.GroupID;
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

        private void txtMargin_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "MARGIN");
            updateSubTotal(sender);
        }

        private void txtPrice_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "UNITPRICE");
            updateSubTotal(sender);
        }

        private void txtCostExcGST_LostFocus(object sender, RoutedEventArgs e)
        {
            updateMargin(sender, "COST");
        }

        private void txtQuantity_LostFocus(object sender, RoutedEventArgs e)
        {
            updateSubTotal(sender);
        }

        private void updateMargin(object sender, string callfrom)
        {
            decimal price, cost, margin;
            decimal gst = decimal.Parse("1.1");
            TextBox txtbox = (TextBox)sender;

            StackPanel panel = txtbox.ParentOfType<StackPanel>();

            TextBox txtPrice = (TextBox)panel.FindName("txtPrice");
            TextBox txtCostDBC = (TextBox)panel.FindName("txtCostDBCExcGST");
            TextBox tbMargin = (TextBox)panel.FindName("txtMargin");

            try
            {
                price = decimal.Parse(txtPrice.Text.Trim());
            }
            catch (Exception)
            {
                price = decimal.Parse("-9999999.99");
            }
            try
            {
                cost = decimal.Parse(txtCostDBC.Text);
            }
            catch (Exception)
            {
                cost = decimal.Parse("-9999999.99");
            }
            try
            {
                margin = decimal.Parse(tbMargin.Text.Replace("%", ""));
            }
            catch (Exception)
            {
                margin = decimal.Parse("-9999999.99");
            }

            if (callfrom.ToUpper() == "UNITPRICE")
            {
                txtPrice.Text = price.ToString("F");
                if (price <= -999999)
                {
                    if (cost > -999999 && margin > -999999)
                    {
                        //price = (cost + (margin / 100) * cost) * gst;
                        if (!margin.Equals(100))
                        {
                            price = (cost / (1 - margin / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMargin.Text = margin.ToString() + "%";
                        }
                    }
                }
                else
                {
                    if (cost < -999999 && margin > -999999)
                    {
                        cost = (price / gst) * (1 - margin / 100);
                        txtCostDBC.Text = cost.ToString("F");
                    }
                    else if (cost > -999999)
                    {
                        if (price != 0)
                        {
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            tbMargin.Text = margin.ToString() + "%";
                        }
                        else
                        {
                            tbMargin.Text = "";
                        }
                    }
                }
            }
            else if (callfrom.ToUpper() == "COST")
            {
                if (cost <= -999999)
                {
                    if (price > -999999 && margin > -999999)
                    {
                        cost = (price / gst) * (1 - margin / 100);
                        txtCostDBC.Text = cost.ToString("F");
                    }
                }
                else
                {
                    if (price < -999999 && margin > -999999)
                    {
                        //price = (cost + (margin / 100) * cost) * gst;
                        if (!margin.Equals(100))
                        {
                            price = (cost / (1 - margin / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMargin.Text = margin.ToString() + "%";
                        }
                    }
                    else if (price > -999999)
                    {
                        if (price != 0)
                        {
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            tbMargin.Text = margin.ToString() + "%";
                        }
                        else
                        {
                            tbMargin.Text = "";
                        }
                    }
                }
            }
            else if (callfrom.ToUpper() == "MARGIN")
            {
                if (cost <= -999999 || cost == 0)
                {
                    if (price > -999999 && margin > -999999)
                    {
                        cost = (price / gst) * (1 - margin / 100);
                        txtCostDBC.Text = cost.ToString("F");
                    }
                }
                else
                {
                    if (margin > -999999)
                    {
                        if (margin != 100 && cost != 0)
                        {
                            price = (cost / (1 - margin / 100)) * gst;
                            //price = (decimal)((int)Math.Round(price, 0));
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMargin.Text = margin.ToString() + "%";
                        }

                    }
                    else
                    {
                        if (price > -999999 && cost > -999999 && price != 0)
                        {
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            tbMargin.Text = margin.ToString() + "%";
                        }
                    }
                }
            }
            /*
            if (callfrom.ToUpper() == "UNITPRICE")
            {
                if (price <= -999999)
                {
                    if (cost > -999999 && margin > -999999)
                    {
                        if (!margin.Equals(100))
                        {
                            price = (cost / (1 - margin / 100)) * gst;
                            price = (decimal)((int)Math.Round(price, 0));
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMargin.Text = margin.ToString() + "%";
                        }
                    }
                }
                else
                {
                    if (cost < -999999 && margin > -999999)
                    {
                        cost = (price / gst) * (1 - margin / 100);
                        txtCost.Text = cost.ToString("F");
                    }
                    else if (cost > -999999)
                    {
                        if (price != 0)
                        {
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            tbMargin.Text = margin.ToString() + "%";
                        }
                        else
                        {
                            tbMargin.Text = "";
                        }
                    }
                }
            }
            else if (callfrom.ToUpper() == "COST")
            {
                if (cost <= -999999)
                {
                    if (price > -999999 && margin > -999999)
                    {
                        cost = (price / gst) * (1 - margin / 100);
                        txtCost.Text = cost.ToString("F");
                    }
                }
                else
                {
                    if (price < -999999 && margin > -999999)
                    {
                        if (!margin.Equals(100))
                        {
                            price = (cost / (1 - margin / 100)) * gst;
                            price = (decimal)((int)Math.Round(price, 0));
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMargin.Text = margin.ToString() + "%";
                        }
                    }
                    else if (price > -999999)
                    {
                        if (price != 0)
                        {
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            tbMargin.Text = margin.ToString() + "%";
                        }
                        else
                        {
                            tbMargin.Text = "";
                        }
                    }
                }
            }
            else if (callfrom.ToUpper() == "MARGIN")
            {
                if (cost <= -999999)
                {
                    if (price > -999999 && margin > -999999)
                    {
                        cost = (price / gst) * (1 - margin / 100);
                        txtCost.Text = cost.ToString("F");
                    }
                }
                else
                {
                    if (margin > -999999)
                    {
                        if (margin != 100 &&  cost!=0)
                        {
                            price = (cost / (1 - margin / 100)) * gst;
                            price = (decimal)((int)Math.Round(price, 0));
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            txtPrice.Text = price.ToString("F");
                            tbMargin.Text = margin.ToString() + "%";
                        }
                    }
                    else
                    {
                        if (price > -999999 && cost > -999999 && price != 0)
                        {
                            margin = Math.Round(100 * ((price / gst) - cost) / (price / gst), 2);
                            tbMargin.Text = margin.ToString() + "%";
                        }
                    }
                }
            }
             * */
        }

        private void updateSubTotal(object sender)
        {
            decimal qty, price;

            TextBox txtbox = (TextBox)sender;
            SimplePAG item = (SimplePAG)txtbox.DataContext;

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

            item.TotalPrice = qty * price;

            txtSubtotal.Text = String.Format("{0:0.00}", (qty * price));
        }

        private bool ValidateStudioMAttributes()
        {

            RadTabItem im = (RadTabItem)rd.FindName("studiomtab");
            Grid gr2 = (Grid)im.Content;
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
                answerXML = answerXML + @"<Brand id=""" + selectedsupplierid + @""" name=""" + dpsup.Text.Replace(@"""", @"&quot;") + @""">";
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

                        answerXML = answerXML + @"<Question id=""" + q.QuestionID + @""" text=""" + q.QuestionText.Replace(@"""", @"&quot;") + @""" type=""" + q.QuestionType.Replace(@"""", @"&quot;") + @""">";
                        answerXML = answerXML + @"<Answers>";
                        switch (q.QuestionType.ToUpper())
                        {
                            case "SINGLE SELECTION":
                                bool selectedsingle = true;
                                RadComboBox rcm = new RadComboBox();
                                string tempname3 = "single_selection_" + q.QuestionID;
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

                                //rcm = (RadComboBox)gr.FindName(tempname3);
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
            }

            //EstimateDetails item = (EstimateDetails)im.DataContext;
            //item.StudioMAnswer = answerXML;
            selectedstudiomanswer = answerXML;

            return true;
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

        }

        public class StudioMAnswer
        {
            public string AnswerID { get; set; }
            public string AnswerText { get; set; }
            public string QuestionID { get; set; }
            public string SupplierBrandID { get; set; }
        }
    }

    #region classes

    #endregion

}

