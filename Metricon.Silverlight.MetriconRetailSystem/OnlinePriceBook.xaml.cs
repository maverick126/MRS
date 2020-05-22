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
    public partial class OnlinePriceBook : Page
    {
        public OnlinePriceBook()
        {
            InitializeComponent();

            var areaDescriptor = new GroupDescriptor()
            {
                Member = "AreaName"
            };
            RadGridSearchProduct.GroupDescriptors.Add(areaDescriptor);
            var groupDescriptor = new GroupDescriptor()
            {
                Member = "GroupName"
            };
            RadGridSearchProduct.GroupDescriptors.Add(groupDescriptor);

            ((EstimateViewModel)LayoutRoot.DataContext).loadPriceRegion(false);
            ((EstimateViewModel)LayoutRoot.DataContext).loadHomesFullName();
            ((EstimateViewModel)LayoutRoot.DataContext).loadAreas();
            ((EstimateViewModel)LayoutRoot.DataContext).loadGroups();
            cmbRegionSearchProduct.SelectedIndex = 0;
            cmbHomeSearchProduct.SelectedIndex = 0;
            cmbAreaSearchProduct.SelectedIndex = 0;
            cmbGroupSearchProduct.SelectedIndex = 0;  
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ((EstimateViewModel)LayoutRoot.DataContext).SelectedRegionId = 0;
            ((EstimateViewModel)LayoutRoot.DataContext).SelectedHomeId = 0;
            ((EstimateViewModel)LayoutRoot.DataContext).ProductName = string.Empty;
            ((EstimateViewModel)LayoutRoot.DataContext).ProductDescription = string.Empty;
            ((EstimateViewModel)LayoutRoot.DataContext).SelectedAreaId = 0;
            ((EstimateViewModel)LayoutRoot.DataContext).SelectedGroupId = 0;

            NavigationService.Refresh();
        }

        private void btnLoadSearchProduct_Click(object sender, RoutedEventArgs e)
        {
            //string regionid = ((EstimateViewModel)LayoutRoot.DataContext).SelectedRegionId.ToString();
            string regionid = ((SQSSalesRegion)cmbRegionSearchProduct.SelectedItem).RegionId.ToString();
            int homeid = ((SQSHome)cmbHomeSearchProduct.SelectedItem).HomeID;
            string productname = textboxProductName.Text;
            string productdesc = textboxProductDesc.Text;
            int areaid = ((SQSArea)cmbAreaSearchProduct.SelectedItem).AreaID;
            int groupid = ((SQSGroup)cmbGroupSearchProduct.SelectedItem).GroupID;

            ((EstimateViewModel)LayoutRoot.DataContext).SearchAllProductsExtended(regionid, homeid, productname, productdesc, areaid, groupid);
        }
    }

}
