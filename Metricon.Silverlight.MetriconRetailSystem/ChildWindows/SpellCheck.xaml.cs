using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Text;

using Metricon.Silverlight.MetriconRetailSystem.MRSService;
using Telerik.Windows.Controls;

namespace Metricon.Silverlight.MetriconRetailSystem.ChildWindows
{
    public partial class SpellCheck : ChildWindow
    {
        private int _estimateRevisionId;
        private string _productsDescription;

        public SpellCheck(int estimateRevisionId)
        {
            InitializeComponent();

            BusyIndicator1.IsBusy = true;

            _estimateRevisionId = estimateRevisionId;

            RetailSystemClient mrsClient = new RetailSystemClient();
            mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());

            mrsClient.GetEstimateDetailsCompleted += new EventHandler<GetEstimateDetailsCompletedEventArgs>(mrsClient_GetEstimateDetailsCompleted);
            mrsClient.GetEstimateDetailsAsync(_estimateRevisionId);
        }

        void mrsClient_GetEstimateDetailsCompleted(object sender, GetEstimateDetailsCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                StringBuilder products = new StringBuilder();
                List<EstimateDetails> estimateDetails = e.Result.ToList<EstimateDetails>();
                foreach (EstimateDetails item in estimateDetails)
                {
                    products.AppendFormat("<<PRODUCT CODE:{0}-ID:{1}>>", item.ProductId, item.EstimateRevisionDetailsId.ToString());
                    products.AppendLine();
                    products.AppendLine("<<STANDARD DESCRIPTION>>");
                    products.AppendLine(item.ProductDescription);
                    products.AppendLine("<<ADDITIONAL NOTES>>");
                    products.AppendLine(item.AdditionalNotes);
                    products.AppendLine("<<EXTRA DESCRIPTION>>");
                    products.AppendLine(item.ExtraDescription);
                    //products.AppendLine("<<INTERNAL DESCRIPTION>>");
                    //products.AppendLine(item.InternalDescription);
                }

                _productsDescription = products.ToString();
                txtProducts.Text = _productsDescription;
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "GetEstimateHeaderCompleted");

            BusyIndicator1.IsBusy = false; 
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            string productDetails = txtProducts.Text;
            string[] items = System.Text.RegularExpressions.Regex.Split(productDetails, "<<PRODUCT CODE:");

            if (items.Length > 1)
            {
                BusyIndicator1.IsBusy = true;

                ObservableCollection<EstimateDetails> products = new ObservableCollection<EstimateDetails>();

                //Start from 1 as item 0 is always blank
                for (int i = 1; i < items.Length; i++)
                {
                    string itemDesc = items[i];
                    int revDetailsIdStartIndex = itemDesc.IndexOf("-ID:") + 4;
                    int revDetailsIdEndIndex = itemDesc.IndexOf(">>", revDetailsIdStartIndex);
                    int standardDescStartIndex = itemDesc.IndexOf("<<STANDARD DESCRIPTION>>") + 24;
                    int standardDescEndIndex = itemDesc.IndexOf("<<ADDITIONAL NOTES>>");
                    int additionalNotesStartIndex = itemDesc.IndexOf("<<ADDITIONAL NOTES>>") + 20;
                    int additionalNotesEndIndex = itemDesc.IndexOf("<<EXTRA DESCRIPTION>>");
                    int extraDescStartIndex = itemDesc.IndexOf("<<EXTRA DESCRIPTION>>") + 21;
                    //int extraDescEndIndex = itemDesc.IndexOf("<<INTERNAL NOTES>>");
                    //int intDescStartIndex = itemDesc.IndexOf("<<INTERNAL NOTES>>") + 24;

                    string revisionDetailsId = itemDesc.Substring(revDetailsIdStartIndex, revDetailsIdEndIndex - revDetailsIdStartIndex);
                    string standardDesc = itemDesc.Substring(standardDescStartIndex, standardDescEndIndex - standardDescStartIndex);
                    string additionalNotes = itemDesc.Substring(additionalNotesStartIndex, additionalNotesEndIndex - additionalNotesStartIndex);
                    string extraDesc = itemDesc.Substring(extraDescStartIndex);
                    //string extraDesc = itemDesc.Substring(extraDescStartIndex, extraDescEndIndex - extraDescStartIndex);
                    //string intDesc = itemDesc.Substring(intDescStartIndex);

                    EstimateDetails product = new EstimateDetails();
                    product.EstimateRevisionDetailsId = Convert.ToInt32(revisionDetailsId);
                    product.ProductDescription = standardDesc.Trim();
                    product.AdditionalNotes = additionalNotes.Trim();
                    product.ExtraDescription = extraDesc.Trim();
                    //product.InternalDescription = intDesc.Trim();

                    products.Add(product);
                }

                RetailSystemClient mrsClient = new RetailSystemClient();
                mrsClient.Endpoint.Address = new System.ServiceModel.EndpointAddress(Internal.Utilities.GetMetriconRetailSystemWcfClientEndpointUrl());
                mrsClient.UpdateEstimateDetailsDescriptionCompleted += new EventHandler<System.ComponentModel.AsyncCompletedEventArgs>(mrsClient_UpdateEstimateDetailsDescriptionCompleted);
                mrsClient.UpdateEstimateDetailsDescriptionAsync(products, (App.Current as App).CurrentUserId);
            }
        }

        void mrsClient_UpdateEstimateDetailsDescriptionCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                this.DialogResult = true;
                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            else
                ExceptionHandler.PopUpErrorMessage(e.Error, "UpdateEstimateDetailsDescriptionCompleted");

            BusyIndicator1.IsBusy = false;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            bool exit = true;
            if (_productsDescription != txtProducts.Text)
            {
                if (MessageBox.Show("Product descriptions have been modified.\r\nDo you really want to cancel without saving the changes?", "Close Without Saving", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                    exit = false;
            }
            if (exit)
            {
                RadWindow window = this.ParentOfType<RadWindow>();
                if (window != null)
                {
                    window.DialogResult = false;
                    window.Close();
                }
            }
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            RadSpellChecker.WindowSettings.SpellCheckingWindowsWidth = 800;
            RadSpellChecker.WindowSettings.SpellCheckingWindowsHeight = 600;
            RadSpellChecker.Check(txtProducts, SpellCheckingMode.AllAtOnce);
        }
    }
}

