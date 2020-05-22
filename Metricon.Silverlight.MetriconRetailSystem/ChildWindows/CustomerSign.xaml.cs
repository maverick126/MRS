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
using System.IO;

using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

using Telerik.Windows;
using Telerik.Windows.Controls.GridView;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class CustomerSign : ChildWindow
    {
        private int _estimateRevisionId, _revisiontypeid;
        private string _printType;
        private DocuSignViewModel cvm;
        private int signinofficecount = 0;
        private string _revisionnumber;
        private string _estimateid;
  
        public CustomerSign(int estimateRevisionId, int revisiontypeid, int estimateid, int revisionnumber, string accountid)
        {
            _estimateRevisionId = estimateRevisionId;
            _revisiontypeid = revisiontypeid;
            _revisionnumber = revisionnumber.ToString();
            _estimateid = estimateid.ToString();
            cvm = new DocuSignViewModel(estimateRevisionId.ToString(), estimateid.ToString(), revisiontypeid.ToString(), revisionnumber.ToString(), accountid);

            InitializeComponent();
            this.DataContext = cvm;

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            CloseRadWindow();
        }
        private void CloseRadWindow()
        {
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string methodid = (cboMethod.SelectedItem as DocuSignMethod).MethodID.ToString();
                if (methodid == "0")
                {
                    MessageBox.Show("The document has been submitted to DocuSign for processing.");
                }
                else
                {
                    MessageBox.Show("The document has been successfully voided.");
                }
                CloseRadWindow();
                //string html=e.Result.ToString();
                // ((DocuSignViewModel)LayoutRoot.DataContext).PushDocumentToProcessingQueue(_estimateRevisionId.ToString(), docinfo.printtype, docinfo.documenttype, ((App)App.Current).CurrentUserId);
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "Save file error");

            BusyIndicator1.IsBusy = false;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {

            DocuSignDocStatusInfo docinfo = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as DocuSignDocStatusInfo;

            RadWindow win = new RadWindow();
            win.ResizeMode = ResizeMode.NoResize;
            DocuSignHistory previewDlg2 = new DocuSignHistory(docinfo.envelopeId, _estimateRevisionId.ToString(), docinfo.versiontype, docinfo.printtype);
            win.Header = "DocuSign History";
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Content = previewDlg2;
            win.ShowDialog();

        }


        private void Docu_RowLoaded(object sender, RowLoadedEventArgs e)
        {
            HyperlinkButton hl;
            GridViewRow row = e.Row as GridViewRow;
            if (row != null)
            {
                DocuSignDocStatusInfo ed = row.DataContext as DocuSignDocStatusInfo;
                if (row != null && ed != null)
                {

                    foreach (GridViewCell Cell in row.Cells)
                    {
                        if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnHistory")
                        {
                            hl = Cell.FindChildByType<HyperlinkButton>();
                            if (ed.envelopeId != null && ed.envelopeId != "" && ed.envelopeId != Guid.Empty.ToString())
                            {
                                hl.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                hl.Visibility = Visibility.Collapsed;
                            }
                        }

                    }


                }

            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string selecteddoctype = ((DocuSignViewModel)LayoutRoot.DataContext)._documenttype;
            string selectedreferenceid = "";

            string selectedrecipientsname = "";
            string selectedrecipientsemail = "";
            string selectedOrder = "";
            string selectedsortorder = "";
            string selectedRecipientType="";
            string actionstring = "";
            string mergesurcharge = "0";
            string includestd = "0";
            string errormessage = "";
            int signinofficecount = 0;
            int customerdocumentsentcount = 0;
            string customerdocumentname = "";

            bool signinoffice = false;

            BusyIndicator1.IsBusy = true;
            List<DocumentAttributes> listd = new List<DocumentAttributes>();
            string methodid = (cboMethod.SelectedItem as DocuSignMethod).MethodID.ToString();
            // get selected value from document grid

            foreach (var item2 in ((DocuSignViewModel)LayoutRoot.DataContext).DocuSignStatus)
            {
                selectedreferenceid = item2.estimateid;
                if (item2.Selected)
                {

                    DocumentAttributes da = new DocumentAttributes();
                    da.VersionType = item2.versiontype;
                    da.VersionNumber = item2.revisionnumber;
                    da.PrintType = item2.printtype;
                    da.EnableSendViaDocuSign = item2.EnableSendViaDocuSign;
                    da.EnableSignInPerson = item2.EnableSignInPerson;
                    da.EnableVoid = item2.EnableVoid;
                    da.Status = item2.status;
                    if (item2.status==null || (item2.status!=null && item2.status.ToLower().Contains("voided")))
                    {
                        da.EnvelopeID = Guid.Empty.ToString();
                    }
                    else
                    {
                        da.EnvelopeID = item2.envelopeId;
                    }
                    da.IntegrationID = item2.integrationid.ToString();

                    listd.Add(da);
                }
                else
                {
                    if (item2.versiontype.ToUpper().Contains("CUSTOMER") && (item2.status!=null  && item2.status.ToUpper() == "SENT"))
                    {
                        customerdocumentname = item2.versiontype;
                        customerdocumentsentcount = customerdocumentsentcount + 1;
                    }
                }
            }

            // get selected value from recipient grid
            foreach (var item in ((DocuSignViewModel)LayoutRoot.DataContext).Contacts)
            {
                if (item.Selected && item.SortOrder>0)
                {
                    if (selectedrecipientsname == "")
                    {
                        selectedrecipientsname = item.RecipientName;
                        selectedrecipientsemail = item.RecipientEmail;
                        selectedOrder = item.RoutingOrder.ToString();
                        actionstring = item.SelectedAction.ToString();
                        selectedsortorder = item.SortOrder.ToString();
                        selectedRecipientType=item.RecipientType;
                    }
                    else
                    {
                        selectedrecipientsname = selectedrecipientsname + "," + item.RecipientName;
                        selectedrecipientsemail = selectedrecipientsemail + "," + item.RecipientEmail;
                        selectedOrder = selectedOrder + "," + item.RoutingOrder.ToString();
                        actionstring = actionstring + "," + item.SelectedAction.ToString();
                        selectedsortorder = selectedsortorder + "," + item.SortOrder.ToString();
                        selectedRecipientType=selectedRecipientType+","+item.RecipientType;
                    }

                    if (item.SelectedAction == 2)
                    {
                        signinofficecount = signinofficecount + 1;
                        signinoffice = true;
                    }
                }

            }
            if ((bool)chkmerge.IsChecked)
            {
                mergesurcharge = "1";
            }
            else
            {
                mergesurcharge = "0";
            }


            if ((bool)chkinclusion.IsChecked)
            {
                includestd = "1";
            }
            else
            {
                includestd = "0";
            }

            #region simple validation and warning message of UI
            if (listd.Count < 1)
            {
                MessageBox.Show("Please select at least one of the document.");
                BusyIndicator1.IsBusy = false;
                return;
            }

            if (methodid == "0")
            {
                if (selectedrecipientsname == "")
                {
                    MessageBox.Show("Please select at least one of the customer recipient.");
                    BusyIndicator1.IsBusy = false;
                    return;
                }

                if (actionstring.Contains("0"))
                {
                    MessageBox.Show("Not all the recipients has the right action.");
                    BusyIndicator1.IsBusy = false;
                    return;
                }

                if (customerdocumentsentcount > 0 && listd[0].VersionType.ToUpper().Contains("CUSTOMER")) // only allow one of the 3 customer copy sent to customer at a time
                {
                    MessageBox.Show("A copy of " + customerdocumentname + " has been sent for signing. Please void the document first then send this copy.");
                    BusyIndicator1.IsBusy = false;
                    return;
                }

                if (signinoffice)
                {
                    if (!ValidateSignInOffice(out errormessage))
                    {
                        MessageBox.Show(errormessage);
                        BusyIndicator1.IsBusy = false;
                        return;
                    }
                }
                else
                {
                    if (!ValidateSginRemote(listd[0], methodid, out errormessage))
                    {
                        MessageBox.Show(errormessage);
                        BusyIndicator1.IsBusy = false;
                        return;
                    }
                }

            }
            else if(methodid=="1")// void
            {
                if (txtvoidreason.Text.Trim()=="")
                {
                    MessageBox.Show("Please enter void reason.");
                    BusyIndicator1.IsBusy = false;
                    return;
                }
                if (listd[0].Status.ToUpper().Contains("VOIDED") || listd[0].Status.ToUpper().Contains("COMPLETED"))
                {
                    MessageBox.Show("Can NOT void envelope in status Void or Completed.");
                    BusyIndicator1.IsBusy = false;
                    return;
                }
            }

            if (txtsubject.Text.Trim() == "")
            {
                MessageBox.Show("Email subject can NOT be blank.");
                BusyIndicator1.IsBusy = false;
                return;
            }
            if (txtbody.Text.Trim()== "")
            {
                MessageBox.Show("Email body can NOT be blank.");
                BusyIndicator1.IsBusy = false;
                return;
            }
            #endregion

            #region validate in database for further business rule
            if (methodid == "0")
            {
                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                mrsClient.DocuSign_ValidateSignerAndDocuemntCompleted += delegate(object o, DocuSign_ValidateSignerAndDocuemntCompletedEventArgs es)
                {
                    if (es.Error == null)
                    {
                        if (es.Result.ToUpper() == "OK")
                        {
                            Processing(signinoffice, listd, selecteddoctype, includestd, mergesurcharge, selectedrecipientsname, selectedrecipientsemail, selectedOrder, actionstring, selectedsortorder);
                        }
                        else
                        {
                            RadWindow.Alert(es.Result);
                            BusyIndicator1.IsBusy = false;
                            return;
                        }
                    }
                };

                mrsClient.DocuSign_ValidateSignerAndDocuemntAsync(_estimateid, _revisionnumber, selectedrecipientsname, selectedRecipientType, actionstring);
            }
            else // void envelope
            {
                Processing(false, listd, selecteddoctype, includestd, mergesurcharge, selectedrecipientsname, selectedrecipientsemail, selectedOrder, actionstring, selectedsortorder);
            }
            #endregion
        }

        private void Processing(bool signinoffice, List<DocumentAttributes> listd, string selecteddoctype, string includestd, string mergesurcharge, string selectedrecipientsname, string selectedrecipientsemail, string selectedOrder, string actionstring, string selectedsortorder)
        {
            string methodid = (cboMethod.SelectedItem as DocuSignMethod).MethodID.ToString();
            string emailbody = txtbody.Text.Replace(Environment.NewLine, "~!~").Replace("\n", "~!~").Replace("\r", "~!~");
            try
            {
                if (signinoffice)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to sign the document now?",  "Confirmation", MessageBoxButton.OKCancel);
                    if (result == MessageBoxResult.OK)
                    {

                        foreach (DocumentAttributes da in listd)
                        {
                            if ((da.EnvelopeID == null || da.EnvelopeID == "" || da.EnvelopeID == Guid.Empty.ToString()) && da.IntegrationID != "0" && !da.Status.ToLower().Contains("voided"))
                            {
                                BusyIndicator1.IsBusy = false;
                                MessageBox.Show("Please wait until the document has been integrated to Docusign.");
                            }
                            else
                            {
                                if (da.VersionType.ToUpper().Contains("CUSTOMER"))
                                {
                                    Uri uri = new Uri("../PrintEstimateNewTemplate.aspx?merge=" + mergesurcharge + "&type=customer&version=" + da.PrintType + "&documenttype=" + selecteddoctype + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&includestd=" + includestd + "&userid=" + ((App)App.Current).CurrentUserId.ToString() + "&action=signinoffice" + "&recipients=" + selectedrecipientsname + "&recipientsemail=" + selectedrecipientsemail + "&routingorder=" + selectedOrder + "&methods=" + actionstring + "&envelopeid=" + da.EnvelopeID + "&sortorder=" + selectedsortorder + "&docusignintegration=1&emailsubject=" + txtsubject.Text + "&emailbody=" + emailbody, UriKind.Relative);
                                    BusyIndicator1.IsBusy = false;
                                    System.Windows.Browser.HtmlPage.Window.Navigate(uri, "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                                }
                                else if (da.VersionType.ToUpper().Contains("STUDIO"))
                                {
                                    BusyIndicator1.IsBusy = true;
                                    Uri uri = new Uri("../PrintEstimateStudioMTemplate.aspx?merge=" + mergesurcharge + "&type=studiom&version=" + da.PrintType + "&documenttype=" + selecteddoctype + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&includestd=" + includestd + "&userid=" + ((App)App.Current).CurrentUserId.ToString() + "&action=signinoffice" + "&recipients=" + selectedrecipientsname + "&recipientsemail=" + selectedrecipientsemail + "&routingorder=" + selectedOrder + "&sortorder=" + selectedsortorder + "&methods=" + actionstring + "&docusignintegration=1&emailsubject=" + txtsubject.Text + "&emailbody=" + emailbody, UriKind.Relative);
                                    BusyIndicator1.IsBusy = false;
                                    System.Windows.Browser.HtmlPage.Window.Navigate(uri, "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                                }
                                else if (da.VersionType.ToUpper().Contains("VARIATION"))
                                {
                                    BusyIndicator1.IsBusy = true;
                                    Uri uri = new Uri("../PrintVariation.aspx?merge=" + mergesurcharge + "&type=variation&version=" + da.PrintType + "&documenttype=" + selecteddoctype + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&includestd=" + includestd + "&userid=" + ((App)App.Current).CurrentUserId.ToString() + "&action=signinoffice" + "&recipients=" + selectedrecipientsname + "&recipientsemail=" + selectedrecipientsemail + "&routingorder=" + selectedOrder + "&sortorder=" + selectedsortorder + "&methods=" + actionstring + "&docusignintegration=1&emailsubject=" + txtsubject.Text + "&emailbody=" + emailbody, UriKind.Relative);
                                    BusyIndicator1.IsBusy = false;
                                    System.Windows.Browser.HtmlPage.Window.Navigate(uri, "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
                                }
                                else
                                {
                                    BusyIndicator1.IsBusy = false;
                                }
                            }
                        }
                        CloseRadWindow();
                    }
                    else
                    {
                        BusyIndicator1.IsBusy = false;
                    }
                }
                else
                {
                    foreach (DocumentAttributes da in listd)
                    {
                        //if (!ValidateSginRemote(da, methodid, out errormessage))
                        //{
                        //    MessageBox.Show(errormessage);
                        //    return;
                        //}
                        //else
                        //{
                        foreach (var item2 in ((DocuSignViewModel)LayoutRoot.DataContext).DocuSignStatus)
                        {
                            if (item2.Selected)
                            {
                                if (methodid == "0")
                                {
                                    item2.status = "Sent";
                                }
                                else if (methodid == "1")
                                {
                                    item2.status = "Voided";
                                }
                                item2.statusChangedDateTime = DateTime.Now.ToString("dd/MM/yyyy");
                            }
                        }
                        //}
                        if (methodid == "0")
                        {
                            if (da.VersionType.ToUpper().Contains("CUSTOMER"))
                            {
                                BusyIndicator1.IsBusy = true;
                                Uri uri = new Uri("../PrintEstimateNewTemplate.aspx?merge=" + mergesurcharge + "&type=customer&version=" + da.PrintType + "&documenttype=" + selecteddoctype + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&includestd=" + includestd + "&userid=" + ((App)App.Current).CurrentUserId.ToString() + "&action=SendViaDocuSign" + "&recipients=" + selectedrecipientsname + "&recipientsemail=" + selectedrecipientsemail + "&routingorder=" + selectedOrder + "&sortorder=" + selectedsortorder + "&methods=" + actionstring + "&docusignintegration=1&emailsubject=" + txtsubject.Text + "&emailbody=" + emailbody, UriKind.Relative);
                                System.Net.WebClient client = new System.Net.WebClient();
                                client.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                                client.DownloadStringAsync(uri);
                            }
                            else if (da.VersionType.ToUpper().Contains("STUDIO"))
                            {
                                BusyIndicator1.IsBusy = true;
                                Uri uri = new Uri("../PrintEstimateStudioMTemplate.aspx?merge=" + mergesurcharge + "&type=studiom&version=" + da.PrintType + "&documenttype=" + selecteddoctype + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&includestd=" + includestd + "&userid=" + ((App)App.Current).CurrentUserId.ToString() + "&action=SendViaDocuSign" + "&recipients=" + selectedrecipientsname + "&recipientsemail=" + selectedrecipientsemail + "&routingorder=" + selectedOrder + "&sortorder=" + selectedsortorder + "&methods=" + actionstring + "&docusignintegration=1&emailsubject=" + txtsubject.Text + "&emailbody=" + emailbody, UriKind.Relative);
                                System.Net.WebClient client = new System.Net.WebClient();
                                client.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                                client.DownloadStringAsync(uri);
                            }
                            else if (da.VersionType.ToUpper().Contains("VARIATION"))
                            {
                                BusyIndicator1.IsBusy = true;
                                Uri uri = new Uri("../PrintVariation.aspx?merge=" + mergesurcharge + "&type=variation&version=" + da.PrintType + "&documenttype=" + selecteddoctype + "&EstimateRevisionId=" + _estimateRevisionId.ToString() + "&includestd=" + includestd + "&userid=" + ((App)App.Current).CurrentUserId.ToString() + "&action=SendViaDocuSign" + "&recipients=" + selectedrecipientsname + "&recipientsemail=" + selectedrecipientsemail + "&routingorder=" + selectedOrder + "&sortorder=" + selectedsortorder + "&methods=" + actionstring + "&docusignintegration=1&emailsubject=" + txtsubject.Text + "&emailbody=" + emailbody, UriKind.Relative);
                                System.Net.WebClient client = new System.Net.WebClient();
                                client.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                                client.DownloadStringAsync(uri);
                            }
                            else
                            {
                                BusyIndicator1.IsBusy = false;
                            }

                        }
                        else
                        {
                            BusyIndicator1.IsBusy = true;

                            if (da.EnvelopeID != null && da.EnvelopeID != "" && da.EnvelopeID != Guid.Empty.ToString())
                            {
                                Uri uri = new Uri("../DocuSignVoidEnvelope.aspx?integrationid=" + da.IntegrationID.ToString() + "&envelopeid=" + da.EnvelopeID + "&username=" + ((App)App.Current).CurrentUserFullName.ToString() + "&voidreason=" + txtvoidreason.Text.Replace(Environment.NewLine, "~!~").Replace("\n", "~!~").Replace("\r", "~!~"), UriKind.Relative);
                                System.Net.WebClient client = new System.Net.WebClient();
                                client.DownloadStringCompleted += new System.Net.DownloadStringCompletedEventHandler(client_DownloadStringCompleted);
                                client.DownloadStringAsync(uri);
                            }
                            else
                            {
                                DocuSignViewModel dmv = this.DataContext as DocuSignViewModel;
                                dmv.RemoveDocuSignDocumentsFromQueue(da.IntegrationID);
                                BusyIndicator1.IsBusy = false;
                                MessageBox.Show("The document has been successfully voided.");
                                CloseRadWindow();
                                
                            }
                        }

                        
                    }
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool ValidateSignInOffice(out string errormessage)
        {
            int order, biggestordersigninoffice, smallestorderremotesign;
            bool result = true;
            biggestordersigninoffice = 0;
            smallestorderremotesign = 999;
            errormessage = "";
            signinofficecount = 0;

            foreach (var item in ((DocuSignViewModel)LayoutRoot.DataContext).Contacts)
            {
                if (item.Selected)
                {

                    order = int.Parse(item.RoutingOrder.ToString());
                    if (item.SelectedAction == 2)
                    {
                        if (order > biggestordersigninoffice)
                        {
                            biggestordersigninoffice = order;
                        }
                        signinofficecount = signinofficecount + 1;
                    }
                    else
                    {
                        if (order < smallestorderremotesign)
                        {
                            smallestorderremotesign = order;
                        }
                    }

                }
            }
            //if (signinofficecount > 1)
            //{
            //    errormessage = "Sign in office only be able to apply to one recipient at a time.";
            //    result = false;
            //}

            if (smallestorderremotesign < biggestordersigninoffice)
            {
                errormessage = "Sign in office routing order should NOT be bigger than remote signing.";
                result = false;
            }

            return result;
        }

        private bool ValidateSginRemote(DocumentAttributes da, string methodid, out string errormessage)
        {
            bool result = true;
            errormessage = "";
            if (!da.EnableSendViaDocuSign && methodid == "0")
            {
                errormessage = "You can NOT send envelope to client as it is " + da.Status + ".";
                result = false;
            }
            if (!da.EnableVoid && methodid == "1")
            {
                if (da.Status == null || da.Status.Trim() == "")
                {
                    errormessage = "You can NOT void this envelope as it doesn't exists.";
                    result = false;
                }
                else
                {
                    errormessage = "You can NOT void an envelope in " + da.Status + " status.";
                    result = false;
                }
            }

            return result;
        }
        private void Signer_DataLoaded(object sender, EventArgs e)
        {
            (Signer.Columns["action"] as GridViewComboBoxColumn).ItemsSource = cvm.DocuSignActions;
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            DocuSignDocStatusInfo docinfo = ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.DataContext as DocuSignDocStatusInfo;
            foreach (var item2 in ((DocuSignViewModel)LayoutRoot.DataContext).DocuSignStatus)
            {
                if (item2.printtype != docinfo.printtype || item2.versiontype != docinfo.versiontype)
                {
                    item2.Selected = false;
                }
            }
            string doctype = "";
            if (docinfo.documentnumber != "0")
            {
                doctype = docinfo.documenttype + docinfo.documentnumber;
            }
            else
            {
                doctype = docinfo.documenttype;
            }
            ((DocuSignViewModel)LayoutRoot.DataContext).GetEmail(doctype);
        }

        private void btnAddRecipient_Click(object sender, RoutedEventArgs e)
        {
            if (txtUserName.Text.Trim() == "" || txtEmail.Text.Trim() == "")
            {
                MessageBox.Show("Please enter name or email address.");
                return;
            }
            else if (txtEmail.Text.Trim().Length<5 ||txtEmail.Text.Trim().IndexOf("@") <= 1 || txtEmail.Text.Trim().IndexOf("@") >= txtEmail.Text.Trim().Length || txtEmail.Text.Trim().IndexOf(".") <= 1 || txtEmail.Text.Trim().IndexOf(".") >= txtEmail.Text.Trim().Length)
            {
                MessageBox.Show("Please enter valid email address.");
                return;
            }

            ValidateAndAddRecipient(txtUserName.Text.Trim(), txtEmail.Text.Trim(), cmbType.Text);
 
        }

        private void ValidateAndAddRecipient(string username, string email, string type)
        {
            bool add = true;
            foreach (Recipient ep in ((DocuSignViewModel)LayoutRoot.DataContext).Contacts)
            {
                if (ep.RecipientName.ToLower() == username.ToLower() && ep.RecipientEmail.ToLower() == email.ToLower())
                {
                    add = false;
                    break;
                }
            }
            if (add)
            {
                ((DocuSignViewModel)LayoutRoot.DataContext).AddUserToSingerList(username, email,type);
            }
            else
            {
                MessageBox.Show("This recipient already exists!");
            }
        }

        private void btnAddMe_Click(object sender, RoutedEventArgs e)
        {
            ValidateAndAddRecipient((App.Current as App).CurrentUserFullName, (App.Current as App).CurrentUserFullName.Replace(" ", "") + "@metricon.com.au","Staff");
        }

        private void cboMethod_SelectionChanged(object sender, Telerik.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboMethod.Text.ToLower().Contains("void"))
            {
                panelrecipient.IsEnabled = false;
                panelemail.Visibility = Visibility.Collapsed;
                panelvoidreason.Visibility = Visibility.Visible;
            }
            else
            {
                panelrecipient.IsEnabled = true;
                panelemail.Visibility = Visibility.Visible;
                panelvoidreason.Visibility = Visibility.Collapsed;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Recipient rec = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as Recipient;
            ((DocuSignViewModel)LayoutRoot.DataContext).Contacts.Remove(rec);
        }

        private void Signer_RowLoaded(object sender, RowLoadedEventArgs e)
        {
            HyperlinkButton hl;
            GridViewRow row = e.Row as GridViewRow;
            if (row != null)
            {
                Recipient ed = row.DataContext as Recipient;
                if (row != null && ed != null)
                {

                    foreach (GridViewCell Cell in row.Cells)
                    {
                        if (Cell.FindChildByType<HyperlinkButton>() != null && Cell.FindChildByType<HyperlinkButton>().Name == "btnDelete")
                        {
                            hl = Cell.FindChildByType<HyperlinkButton>();
                            if (!ed.ManualAdded)
                            {
                                hl.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                hl.Visibility = Visibility.Visible;
                            }
                        }

                    }


                }

            }
        }

 
        private void UpdateRoutingOrder(string order)
        {
            int intorder, maxorder, differ;
            try
            {
                intorder = int.Parse(order);
                maxorder = ((DocuSignViewModel)LayoutRoot.DataContext).GetMaxClientRoutingOrder();
                if (((DocuSignViewModel)LayoutRoot.DataContext).oldmaxrclientoutingorder != maxorder)
                {
                    differ = maxorder - ((DocuSignViewModel)LayoutRoot.DataContext).oldmaxrclientoutingorder;
                    ((DocuSignViewModel)LayoutRoot.DataContext).oldmaxrclientoutingorder = maxorder;
                    ((DocuSignViewModel)LayoutRoot.DataContext).UpdateStaffRoutingOrder(maxorder);
                    foreach (var item in Signer.Items)
                    {
                        var row = Signer.ItemContainerGenerator.ContainerFromItem(item) as GridViewRow;

                        int temp;
                        if (row != null)
                        {
                            TextBox tb = row.Cells[4].FindChildByType<TextBox>();
                            HyperlinkButton i = row.Cells[0].FindChildByType<HyperlinkButton>();

                            if (tb != null && i != null && i.Visibility == Visibility.Visible)
                            {
                                temp = int.Parse(tb.Text);
                                tb.Text = (temp + differ).ToString();
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Please enter a valid routing order.");
            }
        }

 

        private void txtRoutingorder_MouseLeave(object sender, MouseEventArgs e)
        {
            UpdateRoutingOrder(((TextBox)sender).Text);
        }

        private void txtRoutingorder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateRoutingOrder(((TextBox)sender).Text);
            }
        }

 
    }
}

