using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

using WebSupergoo.ABCpdf9;
using WebSupergoo.ABCpdf9.Objects;
using WebSupergoo.ABCpdf9.Atoms;
using System.Data.Odbc;
using System.Globalization;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class PrintMargin : System.Web.UI.Page
    {
        public const string PRINTPDF_DEFAULT_FONT = "Tahoma";
        public const double PRINTPDF_DEFAULT_FONTSIZE = 9;
        public const string HTML_EMPTY_STRING = "&nbsp;";
        private decimal _bpeChargeAmount = 0;
        private decimal totalPrice = 0;
        private decimal totalPriceExc = 0;
        private decimal totalMarginValue = 0;
        private bool titledLand = false;
        private int titledLandDays = 0;
        private decimal _targetMarginPercent = 0;
        private int _requiredBPEChargeType = 0;
        private decimal _bpeRollbackPrice = 0;

        private decimal HomePrice = 0;
        private decimal HomePriceExc = 0;
        private decimal HomeCostBTP = 0;
        private decimal HomeCostDBC = 0;
        private decimal HomeMarginBTP = 0;
        private decimal HomeMarginDBC = 0;
        private string HomeMarginPercentageBTP = "";
        private string HomeMarginPercentageDBC = "";

        private decimal PromotionValuePrice = 0;
        private decimal PromotionValuePriceExc = 0;
        private decimal PromotionValueCostBTP = 0;
        private decimal PromotionValueCostDBC = 0;
        private decimal PromotionValueMarginBTP = 0;
        private decimal PromotionValueMarginDBC = 0;
        private string PromotionValueMarginPercentageBTP = "";
        private string PromotionValueMarginPercentageDBC = "";

        private decimal BudgetedDiscountsUTTTargetMargin = 0;
        private decimal BudgetedCostsSiteOtherTargetMargin = 0;
        private decimal BudgetedCostsSiteOtherMarginTargetMargin = 0;
        private decimal BudgetedCostsSiteOtherRunningJobMarginTargetMargin = 0;
        private decimal BudgetedCostsSiteOtherRunningJobMargin = 0;
        private decimal BudgetedDiscountsUTTRunningJobMarginTargetMargin = 0;
        private decimal BudgetedDiscountsUTTRunningJobMargin = 0;
        private decimal BudgetedDiscountsUTTCostTargetMargin = 0;
        private decimal BudgetedDiscountsUTTMarginTargetMargin = 0;
        private string BudgetedDiscountsUTTMarginPercentageTargetMargin = "";

        private decimal BudgetedDiscountsUTT = 0;
        private decimal BudgetedDiscountsUTTExc = 0;
        private decimal BudgetedDiscountsUTTCost = 0;
        private decimal BudgetedDiscountsUTTMargin = 0;
        private string BudgetedDiscountsUTTMarginPercentage = "";

        private decimal totalHomePromoBudDiscountsPriceIncGSTTargetMargin = 0;
        private decimal totalHomePromoBudDiscountsCostTargetMargin = 0;
        private decimal totalHomePromoBudDiscountsMarginTargetMargin = 0;
        private string TotalHomePromoBudDiscountsMarginPercentageTargetMargin = "";
        private string TotalHomePromoBudDiscountsMarginPercentageTargetRunningJobMarginTargetMargin = "";
        private string TotalHomePromoBudDiscountsMarginPercentageRunningJobMarginTargetMargin = "";

        private decimal totalHomePromoBudDiscountsPriceIncGST = 0;
        private decimal totalHomePromoBudDiscountsCost = 0;
        private decimal totalHomePromoBudDiscountsMargin = 0;
        private string TotalHomePromoBudDiscountsMarginPercentage = "";
        private string TotalHomePromoBudDiscountsMarginPercentageTargetRunningJobMargin = "";
        private string TotalHomePromoBudDiscountsMarginPercentageRunningJobMargin = "";

        private decimal SiteworkPrice = 0;
        private decimal SiteworkPriceExc = 0;
        private decimal SiteworkCostBTP = 0;
        private decimal SiteworkCostDBC = 0;
        private decimal SiteworkMarginBTP = 0;
        private decimal SiteworkMarginDBC = 0;
        private string SiteworkMarginPercentageTargetCategoryMargin = "";
        private string SiteworkMarginPercentageTargetRunningJobMargin = "";
        private string SiteworkMarginPercentageJobCategoryMargin = "";
        private string SiteworkMarginPercentageRunningJobMargin = "";

        private decimal UpgradePrice = 0;
        private decimal UpgradePriceExc = 0;
        private decimal UpgradeCostBTP = 0;
        private decimal UpgradeCostDBC = 0;
        private decimal UpgradeMarginBTP = 0;
        private decimal UpgradeMarginDBC = 0;
        private string UpgradeMarginPercentageTargetCategoryMargin = "";
        private string UpgradeMarginPercentageTargetRunningJobMargin = "";
        private string UpgradeMarginPercentageJobCategoryMargin = "";
        private string UpgradeMarginPercentageRunningJobMargin = "";

        private decimal PriceHoldValue = 0;
        private decimal PriceHoldValueExc = 0;
        private decimal PriceHoldValueCost = 0;
        private decimal PriceHoldValueMargin = 0;
        private string PriceHoldValueMarginPercentageTargetCategoryMargin = "";
        private string PriceHoldValueMarginPercentageTargetRunningJobMargin = "";
        private string PriceHoldValueMarginPercentageJobCategoryMargin = "";
        private string PriceHoldValueMarginPercentageRunningJobMargin = "";

        private decimal DiscountsExcUTT = 0;
        private decimal DiscountsExcUTTExcGst = 0;
        private decimal DiscountsExcUTTCost = 0;
        private decimal DiscountsExcUTTMargin = 0;
        private string DiscountsExcUTTMarginPercentageTargetCategoryMargin = "";
        private string DiscountsExcUTTMarginPercentageTargetRunningJobMargin = "";
        private string DiscountsExcUTTMarginPercentageJobCategoryMargin = "";
        private string DiscountsExcUTTMarginPercentageRunningJobMargin = "";

        private decimal totalRetailPriceIncGSTTargetMargin = 0;
        private decimal totalPriceExcTargetMargin = 0;
        private decimal totalCostTargetMargin = 0;
        private decimal totalMarginTargetMargin = 0;
        private string TotalMarginPercentageTargetMargin = "";

        private decimal totalRetailPriceIncGSTJobMargin = 0;
        private decimal totalPriceExcJobMargin = 0;
        private decimal totalCostJobMargin = 0;
        private decimal totalMarginJobMargin = 0;
        private string TotalMarginPercentageJobMargin = "";

        private decimal gst = decimal.Parse("1.1");

        private string contractnumber="";
        private string estimatenumber="";
        private string revisionnumber="";
        private string revisiontype="";
        private string address = "";
        private string opCenter = "";
        private string brand = "";
        private string houseType = "";
        private double stdPriceHoldDays = 0;
        private double basePriceExtensionDays = 0;
        private string depositDate = "";
        private string priceEffectiveDate = "";
        private string expectedACCDate = "";
        private string priceExpiryDate = "";
        private string facade = "";
        private string separatorDetailSectionColumnHeader = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
        private string DATETIME_FORMAT = "dd/MM/yyyy";

        int estimateRevisionId = 0;
        private static string connectionstring = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            decimal gst = decimal.Parse("1.1");
            int.TryParse(Request.QueryString["revisionid"], out estimateRevisionId);
            DateTime dateTimeTemp = DateTime.Now;
            DateTime.TryParse(Request.QueryString["bcForecastDate"].ToString(), out dateTimeTemp);
            expectedACCDate = dateTimeTemp.ToString(DATETIME_FORMAT);

            GetEstimateHeader(estimateRevisionId);

            Doc theDoc = new Doc();
            theDoc.MediaBox.SetRect(0, 0, 842, 590);
            theDoc.Rect.String = "30 25 842 567";

            string estimateDetails = GetDetailsComparisonContent(estimateRevisionId);

            string docBody = @"
<html>
<head>
    <title></title>
    <style type='text/css'>
    body 
    {
        font-family:Tahoma, Verdana, Arial;
        font-size: 8px;
        margin: 0px;
    }
    table
    {
        border-color:#6699CC;
        border-width:1px 1px 1px 1px;
        border-style:solid;
    }
    td
    {
        border-color:#6699CC;
        border-width:0px 0px 0px 0px;
        padding:5px;
        margin:0;
        font-family:Tahoma, Verdana, Arial;
        font-size: 8px;
        border-style: solid;
    }
    .SummaryTable
    {
        border-style:none;
    }
    .TableHeader
    {
        background-color:#050F4C;
        text-align:center;
        color:#ffffff;
    }
    .SourceRevision
    {
        background-color:#F0F8FF;
    }
    .DestinationRevision
    {
        background-color:#F5F5DC;
    }
      tr:nth-of-type(odd) {
      background-color:#ccc;
    }
    </style>
</head>
<body>

    <div>
    <span style='font-family:Tahoma, Verdana, Arial;font-size: 15px;color:#050F4C;'><b>Margin Details Report</b></span>
    </div>
    <br />
    <table cellpadding='0' cellspacing='0' style='border-style:none; background-color: lightgray; width: 100%;' >
        <tr >
            <td style='font-size: 8px;'><b>Job Details</b></td>            
            <td colspan='5' style='font-size: 8px;'><b>Contract No:</b> " + contractnumber.ToString() + separatorDetailSectionColumnHeader + "<b>Estimate No:</b> " + estimatenumber.ToString() + separatorDetailSectionColumnHeader + "<b>Revision No:</b> " + revisionnumber.ToString() + separatorDetailSectionColumnHeader + "<b>Revision Type:</b> " + revisiontype.ToString() + separatorDetailSectionColumnHeader + "<b>Job Stage:</b> " + revisiontype.ToString() + @"</b> </td>   
            <td style='width: 200px; font-size: 8px;'><b>Address:</b> " + address.ToString() + @"</td>    
        </tr>
        <tr >
            <td align='left' style='width: 100px; font-size: 8px;'><b>Region & Home Details</b></td>            
            <td colspan='6' style='font-size:</b> 8px;'><b>Op Center:</b> " + opCenter.ToString() + separatorDetailSectionColumnHeader + "<b>Brand:</b> " + brand.ToString() + separatorDetailSectionColumnHeader + "<b>House Type:</b> " + houseType.ToString() + separatorDetailSectionColumnHeader + "<b>Facade:</b> " + facade.ToString() + @"</b> </td>   
        </tr>
        <tr >
            <td style='font-size: 8px;'><b>Effective Dates</b></td>            
            <td colspan='6' style='font-size: 8px;'><b>Today's Date:</b> " + DateTime.Now.ToString(DATETIME_FORMAT) + separatorDetailSectionColumnHeader + "<b>Deposit Date:</b> " + depositDate.ToString() + separatorDetailSectionColumnHeader + "<b>Price Effective Date:</b> " + priceEffectiveDate.ToString() + separatorDetailSectionColumnHeader + "<b> Expected ACC Date:</b> " + expectedACCDate.ToString() + @"</td>   
         </tr>
        <tr>
            <td style='font-size: 8px;'><b>Price Hold Period</b></td>";

            if (_requiredBPEChargeType == 0)
            {
                docBody += "<td colspan = '6' style = 'font-size: 8px;'><b> Std Price Hold Days:</b> " + stdPriceHoldDays + separatorDetailSectionColumnHeader + "<b>Titled Land Offer:</b> " + (titledLand ? "Yes" : "No") + separatorDetailSectionColumnHeader + "<b>Revised Price Hold Days:</b> " + titledLandDays.ToString() + separatorDetailSectionColumnHeader + "<b> Price Expiry Date:</b> " + expectedACCDate.ToString() + separatorDetailSectionColumnHeader + "<b> Required BPE Charge(@0.5 %):</b> " + _bpeChargeAmount.ToString("c") + @" </b></td>";
            }
            else if (_requiredBPEChargeType == 1)
            {
                docBody += "<td colspan = '6' style = 'font-size: 8px;'><b> Std Price Hold Days:</b> " + stdPriceHoldDays + separatorDetailSectionColumnHeader + "<b>Titled Land Offer:</b> " + (titledLand ? "Yes" : "No") + separatorDetailSectionColumnHeader + "<b>Revised Price Hold Days:</b> " + titledLandDays.ToString() + separatorDetailSectionColumnHeader + "<b> Price Expiry Date:</b> " + expectedACCDate.ToString() + separatorDetailSectionColumnHeader + "<b> Required BPE Charge (roll forward):</b> " + _bpeChargeAmount.ToString("c") + @" </b></td>";
            }

            docBody += @"</tr>        
    </table>
    <div>&nbsp;</div> 
    <table cellpadding='0' cellspacing='0' width='100%' style='border-style:none;'>
        <tr>
            <td colspan='10' class='TableHeader' style='text-align: left; font-size: 11px;'><b>TARGET MARGINS</b></td>
            <td colspan='1' class='TableHeader' style='text-align: right; font-size: 11px;'><b>" + _targetMarginPercent + @"%</b></td>
        </tr>
        <tr>
            <td style='width: 1100px;'>&nbsp;</td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Home</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Promotion</b></td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Budgeted Discounts (UTT)</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Budgeted Costs (Site/Other)</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Home + Promo</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Site Works</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Upgrades</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Price Hold</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Discounts (ex UTT)</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>TOTAL</b> </td>
        </tr>
         <tr >
            <td style='border-style:none; font-size: 8px;'><b>Retail (inc GST)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(HomePrice,0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(PromotionValuePrice,0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(BudgetedDiscountsUTTTargetMargin,0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(BudgetedCostsSiteOtherTargetMargin, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalHomePromoBudDiscountsPriceIncGSTTargetMargin, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(SiteworkPrice, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(UpgradePrice, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalRetailPriceIncGSTTargetMargin, 0).ToString("c0") + @"</b></td> 
        </tr>
        <tr >
            <td style='font-size: 8px;'><b>Budgeted Cost (ex GST)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(HomeCostBTP, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(PromotionValueCostBTP, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(BudgetedDiscountsUTTCostTargetMargin, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + 0.ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalHomePromoBudDiscountsCostTargetMargin, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(SiteworkCostBTP, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(UpgradeCostBTP, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalCostTargetMargin, 0).ToString("c0") + @"</b></td>
        </tr>
        <tr >
            <td style='font-size: 8px;'><b>Margin ($)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(HomeMarginBTP, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + ((PromotionValueMarginBTP > 0) ? Math.Round(PromotionValueMarginBTP, 0).ToString("c0") : "N/A") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + ((BudgetedDiscountsUTTMarginTargetMargin == 0) ? "N/A" : Math.Round(BudgetedDiscountsUTTMarginTargetMargin, 0).ToString("c0")) + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(BudgetedCostsSiteOtherMarginTargetMargin, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalHomePromoBudDiscountsMarginTargetMargin, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(SiteworkMarginBTP, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(UpgradeMarginBTP, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalMarginTargetMargin,0).ToString("c0") + @"</b> </td>
        </tr>
        <tr >
            <td style='font-size: 8px;'><b>Category Margin (%)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + HomeMarginPercentageBTP + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + PromotionValueMarginPercentageBTP + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + BudgetedDiscountsUTTMarginPercentageTargetMargin + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>100.00%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + TotalHomePromoBudDiscountsMarginPercentageTargetMargin + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + SiteworkMarginPercentageJobCategoryMargin + @"%</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + UpgradeMarginPercentageTargetCategoryMargin + @"%</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>N/A</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + TotalMarginPercentageTargetMargin + @"%</b> </td>
        </tr>
        <tr >
            <td style='border-style:none; font-size: 8px;'><b>Running Job Margin (%)</b></td>
            <td style='font-size: 8px;text-align: right;'><b></b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + (100 * (HomeMarginBTP + PromotionValueMarginBTP) / (HomePriceExc + PromotionValuePriceExc)).ToString("F2") + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + BudgetedDiscountsUTTRunningJobMarginTargetMargin.ToString("F2") + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + BudgetedCostsSiteOtherRunningJobMarginTargetMargin.ToString("F2") + @"%</b></td>
            <td style='font-size: 8px;text-align: right; border: 1px solid red; background-color: yellow;'><b>" + _targetMarginPercent + @"%</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + SiteworkMarginPercentageTargetRunningJobMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + UpgradeMarginPercentageTargetRunningJobMargin + @"%</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + PriceHoldValueMarginPercentageTargetRunningJobMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + DiscountsExcUTTMarginPercentageTargetRunningJobMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + TotalMarginPercentageTargetMargin.ToString() + @"%</b> </td>
        </tr>
    </table>
    <div>&nbsp;</div> 
    <table cellpadding='0' cellspacing='0' width='100%' style='border-style:none;'>
        <tr>
            <td colspan='10' class='TableHeader' style='text-align: left; font-size: 11px;'><b>JOB MARGINS</b></td>
            <td colspan='1' class='TableHeader' style='text-align: right; font-size: 11px;'><b>" + TotalMarginPercentageJobMargin.ToString() + @"%</b></td>
        </tr>
        <tr>
            <td style='width: 1100px;'>&nbsp;</td>          
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Home</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Promotions</b></td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Budgeted Discounts (UTT)</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Budgeted Costs (Site/Other)</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Home + Promo</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Site Works</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Upgrades</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Price Holds</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>Discounts</b> </td>
            <td width='200px;' style='font-size: 8px;text-align: right;'><b>TOTAL</b> </td>
        </tr>
         <tr >
            <td style='border-style:none; font-size: 8px;'><b>Retail (inc GST)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(HomePrice, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(PromotionValuePrice, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(BudgetedDiscountsUTT, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + 0.ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalHomePromoBudDiscountsPriceIncGST, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(SiteworkPrice, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(UpgradePrice, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(PriceHoldValue, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(DiscountsExcUTT, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalRetailPriceIncGSTJobMargin, 0).ToString("c0") + @"</b></td> 
        </tr>
        <tr >
            <td style='font-size: 8px;'><b>Current Cost (ex GST)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(HomeCostDBC, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(PromotionValueCostDBC, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(BudgetedDiscountsUTTCost, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + 0.ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalHomePromoBudDiscountsCost, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(SiteworkCostDBC, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(UpgradeCostDBC, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(PriceHoldValueCost, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(DiscountsExcUTTCost, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalCostJobMargin, 0).ToString("c0") + @"</b></td>
        </tr>
        <tr >
            <td style='font-size: 8px;'><b>Margin ($)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(HomeMarginDBC,0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + ((PromotionValueMarginDBC > 0) ? Math.Round(PromotionValueMarginDBC, 0).ToString("c0") : "N/A") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + ((BudgetedDiscountsUTTMargin == 0) ? "N/A" : Math.Round(BudgetedDiscountsUTTMargin, 0).ToString("c0")) + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + 0.ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalHomePromoBudDiscountsMargin, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(SiteworkMarginDBC, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(UpgradeMarginDBC, 0).ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(PriceHoldValueMargin, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(DiscountsExcUTTMargin, 0).ToString("c0") + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + Math.Round(totalMarginJobMargin, 0).ToString("c0") + @"</b> </td>
        </tr>
        <tr >
            <td style='font-size: 8px;'><b>Category Margin (%)</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + HomeMarginPercentageDBC + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + PromotionValueMarginPercentageDBC + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + BudgetedDiscountsUTTMarginPercentage + @"</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + 0.ToString("c0") + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + TotalHomePromoBudDiscountsMarginPercentage + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + SiteworkMarginPercentageJobCategoryMargin + @"%</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + UpgradeMarginPercentageJobCategoryMargin + @"%</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + PriceHoldValueMarginPercentageJobCategoryMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + DiscountsExcUTTMarginPercentageJobCategoryMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + TotalMarginPercentageJobMargin + @"%</b> </td>
        </tr>
        <tr >
            <td style='border-style:none; font-size: 8px;'><b>Running Job Margin (%)</b></td>
            <td style='font-size: 8px;text-align: right;'><b></b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + (100 * (HomeMarginDBC + PromotionValueMarginDBC) / (HomePriceExc + PromotionValuePriceExc)).ToString("F2") + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + BudgetedDiscountsUTTRunningJobMargin.ToString("F2") + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + BudgetedCostsSiteOtherRunningJobMargin.ToString("F2") + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + TotalHomePromoBudDiscountsMarginPercentageRunningJobMargin + @"</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + SiteworkMarginPercentageRunningJobMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + UpgradeMarginPercentageRunningJobMargin + @"%</b> </td>
            <td style='font-size: 8px;text-align: right;'><b>" + PriceHoldValueMarginPercentageRunningJobMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;'><b>" + DiscountsExcUTTMarginPercentageRunningJobMargin + @"%</b></td>
            <td style='font-size: 8px;text-align: right;border: 1px solid red; background-color: lightgreen;'><b>" + TotalMarginPercentageJobMargin.ToString() + @"%</b> </td>
        </tr>
    </table>
    <div style='page-break-before:always'>&nbsp;</div> 
    <table cellpadding='0' cellspacing='0' width='100%; border-collapse: collapse;'>
        <tr>
            <td class='TableHeader' style='text-align:left; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:75px;min-width:75px;max-width:75px;'>Area</td>
            <td class='TableHeader' style='text-align:left; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:75px;min-width:75px;max-width:75px;'>Group</td>
            <td class='TableHeader' style='text-align:left;min-width: 300px; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:200px;min-width:200px;max-width:200px;'>Product</td>
            <td class='TableHeader' style='text-align:center; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:25px;min-width:25px;max-width:25px;'>UOM</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Qty</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Retail Price<br>(inc GST)</td>         
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Cost<br>(ex GST)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Margin($)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Actual Margin(%)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Target Margin(%)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:30px;min-width:30px;max-width:30px;'>Override By</td>
        </tr>
         " + estimateDetails + @"</table>   
    </body>
    </html>";

            int theID = theDoc.AddImageHtml(docBody);
            while (theDoc.Chainable(theID))
            {
                theDoc.Page = theDoc.AddPage();
                theID = theDoc.AddImageToChain(theID);
            }
            Doc theDoc2 = new Doc();
            theDoc.Append(theDoc2);

            for (int i = 1; i <= theDoc.PageCount; i++)
            {
                // add footer
                theDoc.PageNumber = i;
                if (i > 2)
                {
                    theDoc.Rect.Pin = 0;
                    theDoc.Rect.Position(30, 545);
                    theDoc.Rect.Width = 815;
                    theDoc.Rect.Height = 50;
                    theDoc.TextStyle.LeftMargin = 0;
                    theDoc.Color.String = "40 40 40";
                    theDoc.TextStyle.Size = 8;
                    theDoc.AddImageHtml(@"<html>
<head>
    <title></title>
    <style type='text/css'>
    body 
    {
        font-family:Tahoma, Verdana, Arial;
        font-size: 8px;
        margin: 0px;
    }
    table
    {
        border-color:#6699CC;
        border-width:1px 1px 1px 1px;
        border-style:solid;
    }
    td
    {
        border-color:#6699CC;
        border-width:0px 0px 0px 0px;
        padding:5px;
        margin:0;
        font-family:Tahoma, Verdana, Arial;
        font-size: 8px;
        border-style: solid;
    }
    .SummaryTable
    {
        border-style:none;
    }
    .TableHeader
    {
        background-color:#050F4C;
        text-align:center;
        color:#ffffff;
    }
    .SourceRevision
    {
        background-color:#F0F8FF;
    }
    .DestinationRevision
    {
        background-color:#F5F5DC;
    }
      tr:nth-of-type(odd) {
      background-color:#ccc;
    }
    </style>
</head>
<body><table cellpadding='0' cellspacing='0' width='100%; border-collapse: collapse;'><tr>
            <td class='TableHeader' style='text-align:left; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:75px;min-width:75px;max-width:75px;'>Area</td>
            <td class='TableHeader' style='text-align:left; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:75px;min-width:75px;max-width:75px;'>Group</td>
            <td class='TableHeader' style='text-align:left;min-width: 300px; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:200px;min-width:200px;max-width:200px;'>Product</td>
            <td class='TableHeader' style='text-align:center; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:25px;min-width:25px;max-width:25px;'>UOM</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Qty</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Retail Price<br>(inc GST)</td>         
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Cost<br>(ex GST)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Margin($)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Actual Margin(%)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>Target Margin(%)</td>
            <td class='TableHeader' style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:30px;min-width:30px;max-width:30px;'>Override By</td>
        </tr></table></body>
    </html>");
                }

                theDoc.Rect.Pin = 0;
                theDoc.Rect.Position(2, 2);
                theDoc.Rect.Width = 860;
                theDoc.Rect.Height = 25;
                theDoc.TextStyle.LeftMargin = 0;
                theDoc.Color.String = "40 40 40";
                theDoc.TextStyle.Size = 8;                
                theDoc.AddImageHtml("<span style='font-family:Tahoma, Verdana, Arial;font-size:7px;width:680px;'>&nbsp;</span><span style='font-family:Tahoma, Verdana, Arial;font-size:7px;'>" + DateTime.Today.Date.ToString(DATETIME_FORMAT) + " | Page " + i.ToString() + " of " + theDoc.PageCount.ToString() + "</span>");
      
                theDoc.Transform.Reset();           // Default, no rotation.

                //theDoc.AddImageHtml("<img src='" + Server.MapPath(@"~/images/disk.jpg") + "' width='16' height='16'/>&nbsp;<span style='font-family:Tahoma, Verdana, Arial;font-size:8px;'>Cost overwritten by SE.</span>&nbsp;&nbsp;&nbsp;<img src='" + Server.MapPath(@"~/images/accept.jpg") + "' width='16' height='16'/>&nbsp;<span style='font-family:Tahoma, Verdana, Arial;font-size:8px;'>Derived cost.</span>&nbsp;&nbsp;&nbsp;<img src='" + Server.MapPath(@"~/images/close.jpg") + "' width='16' height='16'/>&nbsp;<span style='font-family:Tahoma, Verdana, Arial;font-size:8px;'>Not derived cost.</span><span style='font-family:Tahoma, Verdana, Arial;font-size:8px;width:420px;'>&nbsp;</span><span style='font-family:Tahoma, Verdana, Arial;font-size:8px;'>" + DateTime.Today.Date.ToString(DATETIME_FORMAT) + " | Page " + i.ToString() + " Of " + theDoc.PageCount.ToString() + "</span>");
                //theDoc.Flatten();

                //theDoc.Rect.Pin = 0;
                //theDoc.Rect.Position(650, 0);
                //theDoc.Rect.Width = 110;
                //theDoc.Rect.Height = 20;
                //theDoc.TextStyle.LeftMargin = 20;
                //theDoc.Color.String = "40 40 40";
                //theDoc.TextStyle.Size = 6;

                //theDoc.Transform.Reset();           // Default, no rotation.
                //theDoc.AddImageHtml("<span style='font-family:Tahoma, Verdana, Arial;font-size:8px;'>" + DateTime.Today.Date.ToString(DATETIME_FORMAT) + " | Page " + i.ToString() + " Of " + theDoc.PageCount.ToString() + "</span>");
                theDoc.Flatten();
            }
            // end footer

            Random R = new Random();
            byte[] theData = theDoc.GetData();
            Response.Clear();
            Response.AddHeader("content-type", "application/pdf");
            Response.AddHeader("content-disposition", "inline; filename='EstimateComparison" + "_" + R.Next(1000).ToString() + ".pdf'");

            if (Context.Response.IsClientConnected)
            {
                Context.Response.OutputStream.Write(theData, 0, theData.Length);
                Context.Response.Flush();
            }

            theDoc.Clear();
        }

        private void GetEstimateHeader(int estimateRevisionId)
                                             
        {
            
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateHeader";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["EstimateNumber"] != DBNull.Value)
                                estimatenumber = dr["EstimateNumber"].ToString();

                            if (dr["ContractNumber"] != DBNull.Value)
                                contractnumber =  dr["ContractNumber"].ToString();

                            if (dr["RevisionNumber"] != DBNull.Value)
                                revisionnumber =  dr["RevisionNumber"].ToString();

                            if (dr["Revisiontypecode"] != DBNull.Value)
                                revisiontype = dr["Revisiontypecode"].ToString();

                            // LOT 1007, 25  WHISPER BOULEVARD, POINT COOK, VIC, 3030
                            if (dr["LotNumber"] != DBNull.Value)
                                address = "LOT " + dr["LotNumber"].ToString() + ", ";
                            address += dr["StreetNumber"].ToString() + " " + dr["StreetAddress"].ToString() + ", " + dr["Suburb"].ToString() + ", " + dr["State"].ToString() + " " + dr["PostCode"].ToString();

                            if (dr["RegionName"] != DBNull.Value)
                                opCenter = dr["RegionName"].ToString();

                            if (dr["BrandName"] != DBNull.Value)
                                brand = dr["BrandName"].ToString();

                            if (dr["HomeName"] != DBNull.Value)
                                houseType = dr["HomeName"].ToString();

                            if (dr["Facade"] != DBNull.Value)
                                facade = dr["Facade"].ToString();


                            if (dr["stdPriceHoldDays"] != DBNull.Value)
                            {
                                stdPriceHoldDays = Convert.ToDouble(dr["stdPriceHoldDays"].ToString());
                            }
                            if (dr["basePriceExtensionDays"] != DBNull.Value)
                            {
                                basePriceExtensionDays = Convert.ToDouble(dr["basePriceExtensionDays"].ToString());
                            }
                            if (dr["titledLand"] != DBNull.Value)
                            {
                                titledLand = Convert.ToBoolean(dr["titledLand"].ToString());
                            }
                            if (dr["titledLandDays"] != DBNull.Value)
                            {
                                titledLandDays = Convert.ToInt32(dr["titledLandDays"].ToString());
                            }
                            if (dr["targetMarginPercent"] != DBNull.Value)
                            {
                                _targetMarginPercent = Convert.ToDecimal(dr["targetMarginPercent"].ToString());
                            }
                            if (dr["RequiredBPEChargeType"] != DBNull.Value)
                            {
                                _requiredBPEChargeType = Convert.ToInt16(dr["RequiredBPEChargeType"].ToString());
                            }
                            if (dr["bpe5Percent"] != DBNull.Value)
                            {
                                _bpeChargeAmount = Convert.ToDecimal(dr["bpe5Percent"].ToString());
                            }

                            DateTime dateTemp = DateTime.Now;
                            if (dr["depositDate"] != DBNull.Value)
                            {
                                depositDate = dr["depositDate"].ToString();
                                DateTime.TryParse(dr["depositDate"].ToString(), out dateTemp);
                                depositDate = dateTemp.ToString(DATETIME_FORMAT);
                                priceExpiryDate = dateTemp.AddDays(basePriceExtensionDays).ToString(DATETIME_FORMAT);
                            }

                            if (dr["EffectiveDate"] != DBNull.Value)
                            {
                                dateTemp = DateTime.Now;
                                DateTime.TryParse(dr["EffectiveDate"].ToString(), out dateTemp);
                                priceEffectiveDate = dateTemp.ToString(DATETIME_FORMAT);
                            }
                            //if (dr["depositDate"] != DBNull.Value)
                            //    depositDate = dr["depositDate"].ToString();

                            //if (dr["Totalcost"] != DBNull.Value)
                            //    totalCost = Convert.ToDecimal(dr["Totalcost"]);

                            if (dr["Totalprice"] != DBNull.Value)
                                totalPrice = Convert.ToDecimal(dr["Totalprice"]);
                            
                            if (dr["Homeprice"] != DBNull.Value)
                                HomePrice = Convert.ToDecimal(dr["Homeprice"]);
                            if (dr["HomeCostBTP"] != DBNull.Value)
                                HomeCostBTP = Convert.ToDecimal(dr["HomeCostBTP"]);
                            if (dr["HomeCostDBC"] != DBNull.Value)
                                HomeCostDBC = Convert.ToDecimal(dr["HomeCostDBC"]);
                            HomePriceExc = HomePrice / gst;
                            HomeMarginBTP = HomePriceExc - HomeCostBTP;
                            HomeMarginDBC = HomePriceExc - HomeCostDBC;
                            if (HomePriceExc != 0)
                            {
                                HomeMarginPercentageBTP = (100 * (HomeMarginBTP / HomePriceExc)).ToString("F");
                                HomeMarginPercentageDBC = (100 * (HomeMarginDBC / HomePriceExc)).ToString("F");
                            }
                            else
                            {
                                HomeMarginPercentageBTP = "N/A";
                                HomeMarginPercentageDBC = "N/A";
                            }


                            if (dr["PromotionValue"] != DBNull.Value)
                                PromotionValuePrice = Convert.ToDecimal(dr["PromotionValue"]);
                            if (dr["PromotionValueCostBTP"] != DBNull.Value)
                                PromotionValueCostBTP = Convert.ToDecimal(dr["PromotionValueCostBTP"]);
                            if (dr["PromotionValueCostDBC"] != DBNull.Value)
                                PromotionValueCostDBC = Convert.ToDecimal(dr["PromotionValueCostDBC"]);
                            PromotionValuePriceExc = PromotionValuePrice / gst;
                            PromotionValueMarginBTP = PromotionValuePriceExc - PromotionValueCostBTP;
                            if (PromotionValuePriceExc != 0)
                            {
                                PromotionValueMarginPercentageBTP = (100 * (PromotionValueMarginBTP / PromotionValuePriceExc)).ToString("F") + "%";
                            }
                            else
                            {
                                PromotionValueMarginPercentageBTP = "N/A";
                            }
                            PromotionValueMarginDBC = PromotionValuePriceExc - PromotionValueCostDBC;
                            if (PromotionValuePriceExc != 0)
                            {
                                PromotionValueMarginPercentageDBC = (100 * (PromotionValueMarginDBC / PromotionValuePriceExc)).ToString("F") + "%";
                            }
                            else
                            {
                                PromotionValueMarginPercentageDBC = "N/A";
                            }

                            if (dr["BudgetedDiscountIncUTTTargetMargin"] != DBNull.Value)
                                BudgetedDiscountsUTTTargetMargin = Convert.ToDecimal(dr["BudgetedDiscountIncUTTTargetMargin"]);

                            BudgetedDiscountsUTTCostTargetMargin = 0;
                            decimal budgetedDiscountsUTTExcTargetMargin = BudgetedDiscountsUTTTargetMargin / gst;
                            BudgetedDiscountsUTTMarginTargetMargin = budgetedDiscountsUTTExcTargetMargin - BudgetedDiscountsUTTCostTargetMargin;
                            if (budgetedDiscountsUTTExcTargetMargin != 0)
                            {
                                BudgetedDiscountsUTTMarginPercentageTargetMargin = (100 * (BudgetedDiscountsUTTMarginTargetMargin / budgetedDiscountsUTTExcTargetMargin)).ToString("F") + "%";
                            }
                            else
                            {
                                BudgetedDiscountsUTTMarginPercentageTargetMargin = "N/A";
                            }
                            BudgetedDiscountsUTTRunningJobMarginTargetMargin = (100 * (HomeMarginBTP + PromotionValueMarginBTP + BudgetedDiscountsUTTMarginTargetMargin) / (HomePriceExc + PromotionValuePriceExc + budgetedDiscountsUTTExcTargetMargin));


                            BudgetedDiscountsUTTRunningJobMargin = (100 * (HomeMarginDBC + PromotionValueMarginDBC + BudgetedDiscountsUTTMargin) / (HomePriceExc + PromotionValuePriceExc + BudgetedDiscountsUTTExc));

                            if (dr["BudgetedCostsSiteOtherTargetMargin"] != DBNull.Value)
                                BudgetedCostsSiteOtherTargetMargin = Convert.ToDecimal(dr["BudgetedCostsSiteOtherTargetMargin"]);
                            decimal budgetedCostsSiteOtherExcGSTTargetMargin = BudgetedCostsSiteOtherTargetMargin / gst;
                            BudgetedCostsSiteOtherMarginTargetMargin = budgetedCostsSiteOtherExcGSTTargetMargin - 0; // cost 0
                            BudgetedCostsSiteOtherRunningJobMarginTargetMargin = (100 * (HomeMarginBTP + PromotionValueMarginBTP + BudgetedDiscountsUTTMarginTargetMargin + BudgetedCostsSiteOtherMarginTargetMargin) / (HomePriceExc + PromotionValuePriceExc + budgetedDiscountsUTTExcTargetMargin + budgetedCostsSiteOtherExcGSTTargetMargin));


                            if (dr["BudgetedDiscountIncUTT"] != DBNull.Value)
                                BudgetedDiscountsUTT = Convert.ToDecimal(dr["BudgetedDiscountIncUTT"]);
                            BudgetedDiscountsUTTCost = 0;
                            BudgetedDiscountsUTTExc = BudgetedDiscountsUTT / gst;
                            BudgetedDiscountsUTTMargin = BudgetedDiscountsUTTExc - BudgetedDiscountsUTTCost;
                            if (BudgetedDiscountsUTTExc != 0)
                            {
                                BudgetedDiscountsUTTMarginPercentage = (100 * (BudgetedDiscountsUTTMargin / BudgetedDiscountsUTTExc)).ToString("F") + "%";
                            }
                            else
                            {
                                BudgetedDiscountsUTTMarginPercentage = "N/A";
                            }

                            totalHomePromoBudDiscountsPriceIncGSTTargetMargin = HomePrice + PromotionValuePrice  + BudgetedDiscountsUTTTargetMargin + BudgetedCostsSiteOtherTargetMargin;
                            totalHomePromoBudDiscountsPriceIncGST = HomePrice + PromotionValuePrice  + BudgetedDiscountsUTT;

                            decimal totalHomePromoBudDiscountsPriceExcGSTTargetMargin = totalHomePromoBudDiscountsPriceIncGSTTargetMargin / gst;
                            totalHomePromoBudDiscountsCostTargetMargin = HomeCostBTP + PromotionValueCostBTP + BudgetedDiscountsUTTCost;
                            totalHomePromoBudDiscountsCost = HomeCostDBC + PromotionValueCostDBC + BudgetedDiscountsUTTCost;
                            totalHomePromoBudDiscountsMarginTargetMargin = totalHomePromoBudDiscountsPriceExcGSTTargetMargin - totalHomePromoBudDiscountsCostTargetMargin;
                            if (totalHomePromoBudDiscountsPriceExcGSTTargetMargin != 0)
                            {
                                TotalHomePromoBudDiscountsMarginPercentageTargetMargin = (100 * (totalHomePromoBudDiscountsMarginTargetMargin / totalHomePromoBudDiscountsPriceExcGSTTargetMargin)).ToString("F") + "%";
                            }
                            else
                            {
                                TotalHomePromoBudDiscountsMarginPercentageTargetMargin = "N/A";
                            }
                            TotalHomePromoBudDiscountsMarginPercentageTargetRunningJobMargin = (100 * totalHomePromoBudDiscountsMarginTargetMargin / totalHomePromoBudDiscountsPriceExcGSTTargetMargin).ToString("F") + "%";

                            decimal totalHomePromoBudDiscountsPriceExcGST = totalHomePromoBudDiscountsPriceIncGST / gst;
                            totalHomePromoBudDiscountsCostTargetMargin = HomeCostBTP + PromotionValueCostBTP + BudgetedDiscountsUTTCost;
                            totalHomePromoBudDiscountsCost = HomeCostDBC + PromotionValueCostDBC + BudgetedDiscountsUTTCost;
                            totalHomePromoBudDiscountsMargin = totalHomePromoBudDiscountsPriceExcGST - totalHomePromoBudDiscountsCost;
                            if (totalHomePromoBudDiscountsPriceExcGST != 0)
                            {
                                TotalHomePromoBudDiscountsMarginPercentage = (100 * (totalHomePromoBudDiscountsMargin / totalHomePromoBudDiscountsPriceExcGST)).ToString("F") + "%";
                            }
                            else
                            {
                                TotalHomePromoBudDiscountsMarginPercentage = "N/A";
                            }
                            TotalHomePromoBudDiscountsMarginPercentageRunningJobMargin = (100 * totalHomePromoBudDiscountsMargin / totalHomePromoBudDiscountsPriceExcGST).ToString("F") + "%";


                            if (dr["SiteWorkValue"] != DBNull.Value)
                                SiteworkPrice = Convert.ToDecimal(dr["SiteWorkValue"]);
                            if (dr["SiteworkCostBTP"] != DBNull.Value)
                                SiteworkCostBTP = Convert.ToDecimal(dr["SiteworkCostBTP"]);
                            if (dr["SiteworkCostDBC"] != DBNull.Value)
                                SiteworkCostDBC = Convert.ToDecimal(dr["SiteworkCostDBC"]);
                            SiteworkPriceExc = SiteworkPrice / gst;
                            SiteworkMarginBTP = SiteworkPriceExc - SiteworkCostBTP;
                            SiteworkMarginDBC = SiteworkPriceExc - SiteworkCostDBC;
                            if (SiteworkPriceExc != 0)
                            {
                                SiteworkMarginPercentageTargetCategoryMargin = (100 * (SiteworkMarginBTP / SiteworkPriceExc)).ToString("F");
                            }
                            else
                            {
                                SiteworkMarginPercentageTargetCategoryMargin = "N/A";
                            }
                            SiteworkMarginPercentageTargetRunningJobMargin = (100 * (totalHomePromoBudDiscountsMarginTargetMargin + SiteworkMarginBTP) / (totalHomePromoBudDiscountsPriceExcGSTTargetMargin + SiteworkPriceExc)).ToString("F");
                            if (SiteworkPriceExc != 0)
                            {
                                SiteworkMarginPercentageJobCategoryMargin = (100 * (SiteworkMarginDBC / SiteworkPriceExc)).ToString("F");
                            }
                            else
                            {
                                SiteworkMarginPercentageJobCategoryMargin = "N/A";
                            }
                            SiteworkMarginPercentageRunningJobMargin = (100 * (totalHomePromoBudDiscountsMargin + SiteworkMarginDBC) / (totalHomePromoBudDiscountsPriceExcGST + SiteworkPriceExc)).ToString("F");


                            if (dr["UpgradeValue"] != DBNull.Value)
                                UpgradePrice = Convert.ToDecimal(dr["UpgradeValue"]);
                            if (dr["UpgradeCostBTP"] != DBNull.Value)
                                UpgradeCostBTP = Convert.ToDecimal(dr["UpgradeCostBTP"]);
                            if (dr["UpgradeCostDBC"] != DBNull.Value)
                                UpgradeCostDBC = Convert.ToDecimal(dr["UpgradeCostDBC"]);
                            UpgradePriceExc = UpgradePrice / gst;
                            UpgradeMarginBTP = UpgradePriceExc - UpgradeCostBTP;
                            UpgradeMarginDBC = UpgradePriceExc - UpgradeCostDBC;
                            if (UpgradePriceExc != 0)
                            {
                                UpgradeMarginPercentageTargetCategoryMargin = (100 * (UpgradeMarginBTP / UpgradePriceExc)).ToString("F");
                            }
                            else
                            {
                                UpgradeMarginPercentageTargetCategoryMargin = "N/A";
                            }
                            UpgradeMarginPercentageTargetRunningJobMargin = (100 * (totalHomePromoBudDiscountsMarginTargetMargin + SiteworkMarginBTP + UpgradeMarginBTP) / (totalHomePromoBudDiscountsPriceExcGSTTargetMargin + SiteworkPriceExc + UpgradePriceExc)).ToString("F");
                            if (UpgradePriceExc != 0)
                            {
                                UpgradeMarginPercentageJobCategoryMargin = (100 * (UpgradeMarginDBC / UpgradePriceExc)).ToString("F");
                            }
                            else
                            {
                                UpgradeMarginPercentageJobCategoryMargin = "N/A";
                            }
                            UpgradeMarginPercentageRunningJobMargin = (100 * (totalHomePromoBudDiscountsMargin + SiteworkMarginDBC + UpgradeMarginDBC) / (totalHomePromoBudDiscountsPriceExcGST + SiteworkPriceExc + UpgradePriceExc)).ToString("F");


                            if (dr["BPE5Percent"] != DBNull.Value)
                                PriceHoldValue = Convert.ToDecimal(dr["BPE5Percent"]);
                            PriceHoldValueExc = PriceHoldValue / gst;
                            PriceHoldValueMargin = PriceHoldValueExc - PriceHoldValueCost;
                            if (PriceHoldValueExc != 0)
                            {
                                PriceHoldValueMarginPercentageTargetCategoryMargin = (100 * (PriceHoldValueMargin / PriceHoldValueExc)).ToString("F");
                            }
                            else
                            {
                                PriceHoldValueMarginPercentageTargetCategoryMargin = "N/A";
                            }
                            PriceHoldValueMarginPercentageTargetRunningJobMargin = (100 * (totalHomePromoBudDiscountsMarginTargetMargin + SiteworkMarginBTP + UpgradeMarginBTP) / (totalHomePromoBudDiscountsPriceExcGSTTargetMargin + SiteworkPriceExc + UpgradePriceExc)).ToString("F");
                            if (PriceHoldValueExc != 0)
                            {
                                PriceHoldValueMarginPercentageJobCategoryMargin = (100 * (PriceHoldValueMargin / PriceHoldValueExc)).ToString("F");
                            }
                            else
                            {
                                PriceHoldValueMarginPercentageJobCategoryMargin = "0";
                            }
                            PriceHoldValueMarginPercentageRunningJobMargin = (100 * (totalHomePromoBudDiscountsMargin + SiteworkMarginDBC + UpgradeMarginDBC + PriceHoldValueMargin) / (totalHomePromoBudDiscountsPriceExcGST + SiteworkPriceExc + UpgradePriceExc + PriceHoldValueExc)).ToString("F");

                            if (dr["DiscountExcUTT"] != DBNull.Value)
                                DiscountsExcUTT = Convert.ToDecimal(dr["DiscountExcUTT"]);
                            if (dr["DiscountsExcUTTCost"] != DBNull.Value)
                                DiscountsExcUTTCost = Convert.ToDecimal(dr["DiscountsExcUTTCost"]);
                            DiscountsExcUTTExcGst = DiscountsExcUTT / gst;
                            DiscountsExcUTTMargin = DiscountsExcUTTExcGst - DiscountsExcUTTCost;
                            if (DiscountsExcUTTExcGst != 0)
                            {
                                DiscountsExcUTTMarginPercentageTargetCategoryMargin = (100 * (DiscountsExcUTTMargin / DiscountsExcUTTExcGst)).ToString("F") + "%";
                            }
                            else
                            {
                                DiscountsExcUTTMarginPercentageTargetCategoryMargin = "N/A";
                            }
                            DiscountsExcUTTMarginPercentageTargetRunningJobMargin = (100 * (totalHomePromoBudDiscountsMarginTargetMargin + SiteworkMarginBTP + UpgradeMarginBTP) / (totalHomePromoBudDiscountsPriceExcGSTTargetMargin + SiteworkPriceExc + UpgradePriceExc)).ToString("F");
                            if (DiscountsExcUTT != 0)
                            {
                                DiscountsExcUTTMarginPercentageJobCategoryMargin = (100 * (DiscountsExcUTTMargin / DiscountsExcUTTExcGst)).ToString("F");
                            }
                            else
                            {
                                DiscountsExcUTTMarginPercentageJobCategoryMargin = "N/A";
                            }
                            DiscountsExcUTTMarginPercentageRunningJobMargin = (100 * (totalHomePromoBudDiscountsMargin + SiteworkMarginDBC + UpgradeMarginDBC + PriceHoldValueMargin + DiscountsExcUTTMargin) / (totalHomePromoBudDiscountsPriceExcGST + SiteworkPriceExc + UpgradePriceExc + PriceHoldValueExc + DiscountsExcUTTExcGst)).ToString("F");

                            totalRetailPriceIncGSTTargetMargin = HomePrice + PromotionValuePrice + BudgetedDiscountsUTTTargetMargin + BudgetedCostsSiteOtherTargetMargin + SiteworkPrice + UpgradePrice;
                            totalPriceExcTargetMargin = totalRetailPriceIncGSTTargetMargin / gst;
                            totalCostTargetMargin = totalHomePromoBudDiscountsCostTargetMargin + SiteworkCostBTP + UpgradeCostBTP;
                            totalMarginTargetMargin = totalPriceExcTargetMargin - totalCostTargetMargin;
                            if (totalPriceExcTargetMargin != 0)
                            {
                                TotalMarginPercentageTargetMargin = (100 * (totalMarginTargetMargin / totalPriceExcTargetMargin)).ToString("F");
                            }
                            else
                            {
                                TotalMarginPercentageTargetMargin = "N/A";
                            }

                            totalRetailPriceIncGSTJobMargin = HomePrice + PromotionValuePrice + BudgetedDiscountsUTT + SiteworkPrice + UpgradePrice + PriceHoldValue + DiscountsExcUTT;
                            totalPriceExcJobMargin = totalRetailPriceIncGSTJobMargin / gst;
                            totalCostJobMargin = totalHomePromoBudDiscountsCost + SiteworkCostDBC + UpgradeCostDBC + PriceHoldValueCost + DiscountsExcUTTCost;
                            totalMarginJobMargin = totalPriceExcJobMargin - totalCostJobMargin;
                            if (totalPriceExcJobMargin != 0)
                            {
                                TotalMarginPercentageJobMargin = (100 * (totalMarginJobMargin / totalPriceExcJobMargin)).ToString("F");
                            }
                            else
                            {
                                TotalMarginPercentageJobMargin = "N/A";
                            }

                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }

        }
        private string GetDetailsComparisonContent(int estimateRevisionId)
        {

            StringBuilder contentString = new StringBuilder();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            bool addToContent = true;

                            StringBuilder comparisonString = new StringBuilder();
                            comparisonString.Append("<tr>");

                            comparisonString.Append("<td style='text-align:left; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px; width:75px;min-width:75px;max-width:75px;'>");
                            comparisonString.Append(dr["AreaName"] != DBNull.Value ? dr["AreaName"].ToString() : HTML_EMPTY_STRING);
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:left; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px; width:75px;min-width:75px;max-width:75px;'>");
                            comparisonString.Append(dr["GroupName"] != DBNull.Value ? dr["GroupName"].ToString() : HTML_EMPTY_STRING);
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:left; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:200px;min-width:200px;max-width:200px;'>");
                            comparisonString.Append(dr["ProductName"].ToString());
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:center; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:25px;min-width:25px;max-width:25px;'>");
                            comparisonString.Append(dr["Uom"] != DBNull.Value ? dr["Uom"].ToString() : HTML_EMPTY_STRING);
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>");
                            comparisonString.Append(dr["Quantity"] != DBNull.Value ? Convert.ToDecimal(dr["Quantity"]).ToString() : HTML_EMPTY_STRING);
                            comparisonString.Append("</td>");

                            //comparisonString.Append("<td>");
                            //if (dr["PromotionProduct"].ToString() == "0" || dr["PromotionProduct"].ToString().ToUpper() == "FALSE")
                            //{
                            //    comparisonString.Append(dr["totalprice"] != DBNull.Value ? Convert.ToDecimal(dr["totalprice"]).ToString("c0") : HTML_EMPTY_STRING);
                            //}
                            //else
                            //{
                            //    comparisonString.Append("INCLUDED");
                            //}
                            //comparisonString.Append("</td>");
                            //comparisonString.Append("<td>");
                            //comparisonString.Append(dr["ItemPrice"] != DBNull.Value ? Convert.ToDecimal(dr["ItemPrice"]).ToString("c0") : HTML_EMPTY_STRING);
                            //comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px; width:50px;min-width:50px;max-width:50px;'>");
                            if (dr["PromotionProduct"].ToString() == "0" || dr["PromotionProduct"].ToString().ToUpper() == "FALSE")
                            {
                                comparisonString.Append(dr["totalprice"] != DBNull.Value ? (Convert.ToDecimal(dr["totalprice"])).ToString("c") : HTML_EMPTY_STRING);
                            }
                            else
                            {
                                comparisonString.Append("INCLUDED");
                            }
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px; width:50px;min-width:50px;max-width:50px;'>");
                            //if (dr["showmargin"].ToString() == "1")
                            //{
                            comparisonString.Append(dr["totalCostExcGST"] != DBNull.Value ? Convert.ToDecimal(dr["totalCostExcGST"]).ToString("c") : HTML_EMPTY_STRING);
                            //}
                            //else
                            //{
                            //    comparisonString.Append("--");
                            //}
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px;width:50px;min-width:50px;max-width:50px;'>");
                            if (dr["totalCostExcGST"] != null && dr["totalCostExcGST"].ToString() != "" && dr["showmargin"].ToString() == "1")
                            {
                                if (dr["PromotionProduct"].ToString() == "0" || dr["PromotionProduct"].ToString().ToUpper() == "FALSE")
                                {
                                    comparisonString.Append(dr["totalprice"] != DBNull.Value ? Convert.ToDecimal(Math.Round((Convert.ToDecimal(dr["totalprice"]) / gst),2) - Convert.ToDecimal(dr["totalCostExcGST"])).ToString("c") : HTML_EMPTY_STRING);
                                }
                                else
                                {
                                    comparisonString.Append("--");
                                }
                            }
                            else
                            {
                                comparisonString.Append("--");
                            }
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px; width:50px;min-width:50px;max-width:50px;'>");
                            if (dr["Margin"] != null && dr["Margin"].ToString() != "" && dr["showmargin"].ToString() == "1")
                            {
                                if (dr["PromotionProduct"].ToString() == "0" || dr["PromotionProduct"].ToString().ToUpper() == "FALSE")
                                {
                                    comparisonString.Append(dr["Margin"].ToString().ToString() + "%");
                                }
                                else
                                {
                                    comparisonString.Append("--");
                                }
                            }
                            else
                            {
                                comparisonString.Append("--");
                            }
                            comparisonString.Append("</td>");
                            comparisonString.Append("<td style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px; width:50px;min-width:50px;max-width:50px;'>");
                            if (dr["TargetMarginPercent"] != null && dr["TargetMarginPercent"].ToString() != "" && dr["showmargin"].ToString() == "1")
                            {
                                if (dr["PromotionProduct"].ToString() == "0" || dr["PromotionProduct"].ToString().ToUpper() == "FALSE")
                                {
                                    comparisonString.Append(dr["TargetMarginPercent"].ToString().ToString() + "%");
                                }
                                else
                                {
                                    comparisonString.Append("--");
                                }
                            }
                            else
                            {
                                comparisonString.Append("--");
                            }
                            comparisonString.Append("</td>");

                            comparisonString.Append("<td style='text-align:right; border-left: 1px solid gray; border-bottom: 1px solid gray; padding: 2px; width:30px;min-width:30px;max-width:30px;'>");
                            if (int.Parse(dr["CostOverWriteBy"].ToString()) > 0 )
                            {
                                //comparisonString.Append("<img src='" + Server.MapPath(@"~/images/disk.jpg") + "' alt='Overwritten by SE.' width='16' height='16'/>");
                                comparisonString.Append("SE");
                            }
                            else
                            {
                                if (dr["DerivedCost"].ToString() == "1" || dr["DerivedCost"].ToString().ToUpper() == "TRUE")
                                {
                                    //comparisonString.Append("<img src='" + Server.MapPath(@"~/images/accept.jpg") + "' alt='Yes' width='16' height='16'/>");
                                    comparisonString.Append("Derived Cost");
                                }
                                else
                                {
                                    //comparisonString.Append("<img src='" + Server.MapPath(@"~/images/close.jpg") + "' alt='No.' width='16' height='16'/>");
                                    comparisonString.Append("&nbsp;");
                                }
                            }
                            comparisonString.Append("</td>");
                            comparisonString.Append("</tr>");
                            //comparisonString.Append("<tr class='trline'><td colspan='10' class='trline'><img src='" + Server.MapPath(@"~/images/line.gif") + "' width='845' height='17' alt='' /></td></tr>");

                            if (addToContent)
                                contentString.Append(comparisonString);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return contentString.ToString();
        }


    }
}