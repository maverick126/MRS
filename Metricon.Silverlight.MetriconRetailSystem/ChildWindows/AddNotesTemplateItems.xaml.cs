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
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class AddNotesTempateItems : ChildWindow
    {
        public static string _templateid;
        private RetailSystemClient mrsClient2;
        private string selecteditemids = "";

        public AddNotesTempateItems(string templateid)
        {
            _templateid = templateid;
            InitializeComponent();
            
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //((NotesTemplateItemsViewModel)LayoutRoot.DataContext).SaveItemsToNoteTempLate();
            int userid = (App.Current as App).CurrentUserId;
            foreach (var item in ((NotesTemplateItemsViewModel)LayoutRoot.DataContext).AvailableNoteTemplateItem)
            {
                if (item.PromotionProduct) // here reuse this column to hold the selection of check box
                {
                    if (selecteditemids == "")
                    {
                        selecteditemids = item.ProductAreaGroupID.ToString();
                    }
                    else
                    {
                        selecteditemids = selecteditemids + "," + item.ProductAreaGroupID.ToString();
                    }
                    //_selectednotetemplateitem.Add(item);
                }
            }
            mrsClient2 = new RetailSystemClient();
            mrsClient2.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient2.AddItemToNotesTemplateCompleted += new EventHandler<AddItemToNotesTemplateCompletedEventArgs>(mrsClient2_AddItemToNotesTemplateCompleted);
            mrsClient2.AddItemToNotesTemplateAsync(AddNotesTempateItems._templateid, selecteditemids, userid);
            mrsClient2 = null;

        }
        public void mrsClient2_AddItemToNotesTemplateCompleted(object sender, AddItemToNotesTemplateCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (e.Result)
                {
                    RadWindow window = this.ParentOfType<RadWindow>();
                    if (window != null)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    RadWindow.Alert("AddItemToNotesTemplate failed!\r\nPlease contact the System Administrator.");
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "AddItemToNotesTemplateCompleted");

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

        private void RadButton_Search_Click(object sender, RoutedEventArgs e)
        {
            ((NotesTemplateItemsViewModel)LayoutRoot.DataContext).SearchItem();
        }
    }
}

