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

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class ComparisonPrint : ChildWindow
    {
        string _sourceId;
        string _destinationId;
        string filterlist = "";
        public ComparisonPrint(string sourceId, string destinationId, string para)
        {
            InitializeComponent();

            _sourceId = sourceId;
            _destinationId = destinationId;
            filterlist = para;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        private void DifferentOnlyPrintButton_Click(object sender, RoutedEventArgs e)
        {
            PopupPrintWindowVariationFormat();
        }

        private void FullComparisonPrintButton_Click(object sender, RoutedEventArgs e)
        {
            PopupPrintWindowGrid(true);
        }

        private void PopupPrintWindowGrid(bool fullcomparison)
        {
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintEstimateComparison.aspx?Source=" + _sourceId + "&Destination=" + _destinationId + "&fullcomparison=" + fullcomparison.ToString()+"&filter=" + filterlist, UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");

            this.DialogResult = true;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        private void PopupPrintWindowVariationFormat()
        {
            //System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintEstimateComparison.aspx?Source=" + _sourceId + "&Destination=" + _destinationId + "&fullcomparison=" + fullcomparison.ToString(), UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");
            System.Windows.Browser.HtmlPage.Window.Navigate(new Uri("../PrintEstimateComparisonVariationFormat.aspx?Source=" + _sourceId + "&Destination=" + _destinationId , UriKind.Relative), "_blank", "toolbar=0,menubar=1,location=0,status=0,top=0,left=0,resizable=1");

            this.DialogResult = true;
            RadWindow window = this.ParentOfType<RadWindow>();
            if (window != null)
                window.Close();
        }

        private void FilterPrintButton_Click(object sender, RoutedEventArgs e)
        {
            PopupPrintWindowGrid(false);
        }
    }
}

