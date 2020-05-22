using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;

using System.Threading;
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
using Telerik.Windows;
using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Metricon.Silverlight.MetriconRetailSystem.ViewModels;
using Telerik.Windows.Documents;
using Telerik.Windows.Documents.Proofing;



namespace Metricon.Silverlight.MetriconRetailSystem
{
    public partial class EditOption : UserControl
    {
        private int _userid = (App.Current as App).CurrentUserId;
        //private EstimateViewModel _estimatevm;
        public EditOption()
        {
            InitializeComponent();
            //_estimatevm = new EstimateViewModel();
        }

        private void btnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (tabDesc.SelectedIndex == 0)
            {
                RadSpellChecker.Check(this.txtDesc, SpellCheckingMode.WordByWord);
            }
            else if (tabDesc.SelectedIndex == 1)
            {
                RadSpellChecker.Check(this.txtExtraDesc, SpellCheckingMode.WordByWord);
            }
            else if (tabDesc.SelectedIndex == 2)
            {
                RadSpellChecker.Check(this.txtInternalDesc, SpellCheckingMode.WordByWord);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string desc = txtDesc.Text;
            string extradesc = txtExtraDesc.Text;
            string internaldesc = txtInternalDesc.Text;
            string estimatedetailsid = txtEstimateDeatilsID.Text;
            decimal quantity, price;

            bool ok = true;
            quantity = 1;
            price = 0;
            try
            {
                quantity = decimal.Parse(txtQuantity.Text);
            }
            catch (Exception ex)
            {
                ok = false;
                RadWindow.Alert("Please enter a valid quantity !");
            }

            try
            {
                price = decimal.Parse(txtPrice.Text);
            }
            catch (Exception ex)
            {
                ok = false;
                RadWindow.Alert("Please enter a valid price !");
            }
            if (ok)
            {
                SaveData(desc, extradesc, internaldesc, quantity, price);
            }
        }

        private void SaveData(string pdesc, string pextradesc, string pinternaldesc, decimal pquantity, decimal pprice)
        {
            //EstimateDetails ed = new EstimateDetails();
            //ed.EstimateDetailsId = int.Parse(txtEstimateDeatilsID.Text);
            //ed.ProductDescription = pdesc;
            //ed.ExtraDescription = pextradesc;
            //ed.InternalDescription = pinternaldesc;
            //ed.Quantity = pquantity;
            //ed.Price = pprice;

            //_estimatevm.UpdateEstimateDetails((EstimateDetails)this.DataContext);

            EstimateViewModel vm = (EstimateViewModel)this.Resources["EstimateVM"];
            vm.UpdateEstimateDetails((EstimateDetails)this.DataContext, 0);
        }
    }
}
