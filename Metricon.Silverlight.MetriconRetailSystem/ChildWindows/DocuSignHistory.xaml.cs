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
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class DocuSignHistory : ChildWindow
    {
        private string _envelopeid = "";
        private DocusignHistoryViewModel dshvm;
        public DocuSignHistory(string envelopeid, string revisionid, string versiontype, string printtype)
        {
            _envelopeid = envelopeid;
            dshvm = new DocusignHistoryViewModel(_envelopeid, revisionid, versiontype,printtype);

            InitializeComponent();
            this.DataContext = dshvm;
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

