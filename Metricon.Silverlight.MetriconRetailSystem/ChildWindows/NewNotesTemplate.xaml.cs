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

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class NewNotesTemplate : ChildWindow
    {
        private int _templateId;

        public NewNotesTemplate(int templateId)
        {
            _templateId = templateId;

            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.txtTempaltename.Text != "")
            {
                if (cmbSalesRegion.SelectedValue.ToString() != "0")
                {
                    RetailSystemClient mrsClient = new RetailSystemClient();
                    mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

                    if (_templateId == 0)
                    {
                        mrsClient.AddNewNotesTemplateCompleted += new EventHandler<AddNewNotesTemplateCompletedEventArgs>(mrsClient_AddNewNotesTemplateCompleted);
                        mrsClient.AddNewNotesTemplateAsync(txtTempaltename.Text, cmbSalesRegion.SelectedValue.ToString(), (App.Current as App).CurrentUserId);
                    }
                    else
                    {
                        mrsClient.CopyNotesTemplateCompleted += new EventHandler<CopyNotesTemplateCompletedEventArgs>(mrsClient_CopyNotesTemplateCompleted);
                        mrsClient.CopyNotesTemplateAsync(txtTempaltename.Text, cmbSalesRegion.SelectedValue.ToString(), (App.Current as App).CurrentUserId, _templateId.ToString());
                    }
                    this.DialogResult = true;
                }
                else
                {
                    RadWindow.Alert("Please select a region.");
                }
            }
            else
            {
                RadWindow.Alert("Please enter a template name.");
            }
            
        }

        void mrsClient_CopyNotesTemplateCompleted(object sender, CopyNotesTemplateCompletedEventArgs e)
        {
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
                ExceptionHandler.PopUpErrorMessage(e.Error, "CopyNotesTemplateCompleted");
        }

        void mrsClient_AddNewNotesTemplateCompleted(object sender, AddNewNotesTemplateCompletedEventArgs e)
        {
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
                ExceptionHandler.PopUpErrorMessage(e.Error, "AddNewNotesTemplateCompleted");
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

