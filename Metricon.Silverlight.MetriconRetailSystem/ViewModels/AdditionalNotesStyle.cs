using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;

namespace Metricon.Silverlight.MetriconRetailSystem.ViewModels
{
    public class AdditionalNotesStyle : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is EstimateDetails)
            {
                EstimateDetails pag = item as EstimateDetails;
                if (pag.HomeDisplayOptionId == 0)
                {
                    return NotAvailableStyle;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
        public Style NotAvailableStyle { get; set; }
    }
}
