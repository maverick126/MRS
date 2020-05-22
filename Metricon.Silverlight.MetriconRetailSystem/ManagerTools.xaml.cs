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
using System.Windows.Navigation;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Metricon.Silverlight.MetriconRetailSystem.ChildWindows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;


namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class ManagerTools : Page
    {
        private EstimateDetails pag;
        private NotesTemplateViewModel.NoteTemplate template;
        private RetailSystemClient mrsClient;
        public ManagerTools()
        {
            InitializeComponent();
        }

        private void HyperlinkButton_AddTemplate_Click(object sender, RoutedEventArgs e)
        {
            NewNotesTemplate noteDlg = new NewNotesTemplate(0);
            RadWindow win = new RadWindow();
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.Header = "Add Note Template";
            win.Content = noteDlg;
            win.ShowDialog();
        }
        void win_Closed(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? result = dlg.DialogResult;
            if (result.HasValue && result.Value)
                ((NotesTemplateViewModel)LayoutRoot.DataContext).SearchNotesTemplate();
        }

        private void HyperlinkButton_AddItem_Click(object sender, RoutedEventArgs e)
        {
            NotesTemplateViewModel.NoteTemplate nt = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as NotesTemplateViewModel.NoteTemplate;
            AddNotesTempateItems noteDlg = new AddNotesTempateItems(nt.TemplateID.ToString());
            noteDlg.Title = "Add Items to Template -- " + nt.TemplateName;
            RadWindow win = new RadWindow();
            win.Height = 650;
            win.Width = 900;
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Header = "Add Items to Note Template";
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.Content = noteDlg;
            win.ShowDialog();
        }

        private void HyperlinkButton_RemoveItems_Click(object sender, RoutedEventArgs e)
        {
            pag = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as EstimateDetails;
            RadWindow.Confirm("Are you sure you want to remove this item from tempale?",new EventHandler<WindowClosedEventArgs>(item_confirm_close));
        }
        void item_confirm_close(object sender, WindowClosedEventArgs e)
        {
            
            RadWindow dlg = (RadWindow)sender;
            bool? confirm = dlg.DialogResult;
            if ((bool)confirm)
            {
                ((NotesTemplateViewModel)LayoutRoot.DataContext).RemoveNoteTemplateItemFromTemplate(pag);
            }

        }

        private void HyperlinkButton_RemoveTemplate_Click(object sender, RoutedEventArgs e)
        {
            template = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as NotesTemplateViewModel.NoteTemplate;
            RadWindow.Confirm("Remove template will deactivate all items configured to this template.\n\nAre you sure you want to remove this template?", new EventHandler<WindowClosedEventArgs>(template_confirm_close));
        }

        private void HyperlinkButton_Copy_Click(object sender, RoutedEventArgs e)
        {
            template = ((GridViewCell)((HyperlinkButton)e.OriginalSource).Parent).ParentRow.DataContext as NotesTemplateViewModel.NoteTemplate;

            NewNotesTemplate noteDlg = new NewNotesTemplate(template.TemplateID);
            RadWindow win = new RadWindow();
            win.WindowStartupLocation = Telerik.Windows.Controls.WindowStartupLocation.CenterScreen;
            win.Closed += new EventHandler<WindowClosedEventArgs>(win_Closed);
            win.Header = "Copying " + template.TemplateName;
            win.Content = noteDlg;
            win.ShowDialog();
        }

        void template_confirm_close(object sender, WindowClosedEventArgs e)
        {
            RadWindow dlg = (RadWindow)sender;
            bool? confirm = dlg.DialogResult;
            if ((bool)confirm)
            {
                ((NotesTemplateViewModel)LayoutRoot.DataContext).RemoveNoteTemplate(template);
            }
        }

        private void RadButton_Save_Click(object sender, RoutedEventArgs e)
        {
            decimal qty = 1; 
            decimal price=0;
            int pagid, templateid;
            string extradesc, internaldesc;
            bool ok = true;
            Grid gd = ((RadButton)e.OriginalSource).Parent as Grid;
            RadTabControl tab = gd.Parent.FindChildByType<RadTabControl>();

            //try
            //{
            //    qty = decimal.Parse(((TextBox)gd.FindName("txtQuantity")).Text);
            //}
            //catch (Exception ex)
            //{
            //    RadWindow.Alert("Please enter a valid quantity.");
            //    ok = false;
            //}
            //try
            //{
            //    price = decimal.Parse(((TextBox)gd.FindName("txtPrice")).Text);
            //}
            //catch (Exception ex)
            //{
            //    RadWindow.Alert("Please enter a valid price.");
            //    ok = false;
            //}

            //if (ok)
            //{
                pagid = int.Parse(((TextBlock)gd.FindName("txtPAGID")).Text);
                templateid = int.Parse(((TextBlock)gd.FindName("txttemplateID")).Text);
                extradesc=((TextBox)tab.FindName("txtDesc")).Text;
                internaldesc = ((TextBox)tab.FindName("txtInternalDesc")).Text;
 //               ((NotesTemplateViewModel)LayoutRoot.DataContext).UpdateNoteTemplateItem(templateid.ToString(), pagid.ToString(), qty, price, extradesc, (App.Current as App).CurrentUserId);
            //}
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            NotesTemplateViewModel.NoteTemplate ed = ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.DataContext as NotesTemplateViewModel.NoteTemplate;
            int active = 0;
            if ((bool)chk.IsChecked)
            {
                active = 1;
                ed.Active = true;
            }
            else
            {
                active = 0;
                ed.Active = false;
            }

            ((NotesTemplateViewModel)LayoutRoot.DataContext).UpdateNoteTemplateStatus(ed.TemplateID, active, (App.Current as App).CurrentUserId, "STATUS");

        }

        private void Private_Click(object sender, RoutedEventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            NotesTemplateViewModel.NoteTemplate ed = ((GridViewCell)((CheckBox)e.OriginalSource).Parent).ParentRow.DataContext as NotesTemplateViewModel.NoteTemplate;
            int isPrivate = 0;
            if ((bool)chk.IsChecked)
            {
                isPrivate = 1;
                ed.IsPrivate = true;
            }
            else
            {
                isPrivate = 0;
                ed.IsPrivate = false;
            }

            ((NotesTemplateViewModel)LayoutRoot.DataContext).UpdateNoteTemplateStatus(ed.TemplateID, isPrivate, (App.Current as App).CurrentUserId, "ISPRIVATE");

        }

        private void RadGridView2_CellEditEnded(object sender, GridViewCellEditEndedEventArgs e)
        {
            NotesTemplateViewModel.NoteTemplate ed = (NotesTemplateViewModel.NoteTemplate)e.Cell.DataContext;
            UpdateNoteTemplateName(ed.TemplateID, ed.TemplateName);
        }

        public void UpdateNoteTemplateName(int templateid, string templatename)
        {
            mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.CheckNewNoteTemplateNameExistsCompleted += delegate(object o, CheckNewNoteTemplateNameExistsCompletedEventArgs es)
            {
                if (es.Error == null)
                {
                    if (es.Result.ToString().ToUpper() == "OK")
                    {

                        ((NotesTemplateViewModel)LayoutRoot.DataContext).UpdateNoteTemplateName(templateid, (App.Current as App).CurrentUserId,templatename, "NAME");
                    }
                    else
                    {
                        MessageBox.Show(es.Result.ToString());
                        ((NotesTemplateViewModel)LayoutRoot.DataContext).SearchNotesTemplate();
                    }
                }
                else
                    ExceptionHandler.PopUpErrorMessage(es.Error, "CheckNewNoteTemplateNameExistsCompleted");

            };

            mrsClient.CheckNewNoteTemplateNameExistsAsync(templateid, templatename);
            mrsClient = null;
        }

        private void chkdefaultqty_Click(object sender, RoutedEventArgs e)
        {
            CheckBox ck = (CheckBox)e.OriginalSource;

            EstimateDetails ed = (EstimateDetails)ck.DataContext;
            StackPanel panel = ck.ParentOfType<StackPanel>();
            TextBox txtqty = (TextBox)panel.FindName("txtQuantity");
            if ((bool)ck.IsChecked)
            {
                txtqty.IsEnabled = false;
                txtqty.Text = "";
                ed.Quantity = 0;
            }
            else
            {
                txtqty.IsEnabled = true;
                txtqty.Text = ed.Quantity.ToString();
            }
        }

        private void detailgrid_RowDetailsVisibilityChanged(object sender, GridViewRowDetailsEventArgs e)
        {
            if (e.Visibility == Visibility.Visible)
            {
                GridViewRow row = e.Row as GridViewRow;

                if (row != null)
                {
                    EstimateDetails ed = row.DataContext as EstimateDetails;
                    StackPanel panel = (StackPanel)e.DetailsElement;

                    TextBox txtqty = (TextBox)panel.FindName("txtQuantity");
                    if (ed.UseDefaultQuantity)
                    {
                        txtqty.IsEnabled = false;
                    }
                    else
                    {
                        txtqty.IsEnabled = true;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;
            EstimateDetails item = (EstimateDetails)btn.DataContext;
            ((NotesTemplateViewModel)LayoutRoot.DataContext).UpdateNoteTemplateItem(item);
            item.ModifiedOn = DateTime.Now.ToString("dd/MM/yyyy");
            item.ModifiedBy = (App.Current as App).CurrentUserFullName;
            CloseDetailsPanel(sender, e);
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
    }
}
