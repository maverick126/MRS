using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Drawing;

using WebSupergoo.ABCpdf9;
using WebSupergoo.ABCpdf9.Objects;
using WebSupergoo.ABCpdf9.Atoms;

using Metricon.Silverlight.MetriconRetailSystem.Host.DocuSignWebService;
using Metricon.Silverlight.MetriconRetailSystem.Host.MetriconSalesWebService;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class PrintEstimateComparisonVariationFormat : System.Web.UI.Page
    {
        public const string PRINTPDF_DEFAULT_FONT = "Tahoma";
        public const double PRINTPDF_DEFAULT_FONTSIZE = 9;
        public const string HTML_EMPTY_STRING = "&nbsp;";
        private static string connectionstring = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ToString();
        int estimateNumber;
        int revisionNumberA;
        int revisionNumberB;
        string revisionTypeA;
        string revisionTypeB;
        string customerDocumentA;
        string customerDocumentB;
        DateTime effectiveDateA;
        DateTime effectiveDateB;
        decimal homePriceA;
        decimal homePriceB;
        decimal upgradeValueA;
        decimal upgradeValueB;
        decimal siteworkValueA;
        decimal siteworkValueB;
        decimal totalPriceA;
        decimal totalPriceB;
        string revisionTitleA;
        string revisionTitleB;
        string statusA;
        string statusB;
        string ownerA;
        string ownerB;
        string customer;
        string address;
        string lotAddress;
        string houseAndLandPackage;
        string houseName;
        int estimateRevisionIdA = 0;
        int estimateRevisionIdB = 0;
        bool fullConparison = false;
        string filter = "";
        protected void Page_Load(object sender, EventArgs e)
        {

            //filter = Request.QueryString["filter"].ToString();
            bool.TryParse(Request.QueryString["fullcomparison"], out fullConparison);
            int.TryParse(Request.QueryString["Source"], out estimateRevisionIdA);
            int.TryParse(Request.QueryString["Destination"], out estimateRevisionIdB);

            GetEstimateHeader(estimateRevisionIdA, out estimateNumber, out revisionTypeA, out revisionNumberA, out effectiveDateA, out homePriceA, out upgradeValueA, out siteworkValueA, out totalPriceA, out ownerA, out statusA, out houseName, out customer, out houseAndLandPackage, out customerDocumentA);
            GetEstimateHeader(estimateRevisionIdB, out estimateNumber, out revisionTypeB, out revisionNumberB, out effectiveDateB, out homePriceB, out upgradeValueB, out siteworkValueB, out totalPriceB, out ownerB, out statusB, out houseName, out customer, out houseAndLandPackage, out customerDocumentB);

            GetEstimateInformation(estimateNumber, out lotAddress, out address);

            revisionTitleA = "Revision " + revisionNumberA.ToString() + " (" + revisionTypeA + ")";
            if (!string.IsNullOrEmpty(customerDocumentA))
                revisionTitleA += " - " + customerDocumentA;

            revisionTitleB = "Revision " + revisionNumberB.ToString() + " (" + revisionTypeB + ")";
            if (!string.IsNullOrEmpty(customerDocumentB))
                revisionTitleB += " - " + customerDocumentB;

            Doc theDoc = new Doc();
            theDoc.MediaBox.SetRect(0, 0, 595, 842);
            theDoc.Rect.String = "30 35 565 812";

            // generate header
            string html = "";
            html = GenerateHeader();

            // generate body
            html = PrintEstimateBody(html);

            // generate PDF

            int theID = theDoc.AddImageHtml(html);
            while (theDoc.Chainable(theID))
            {
                theDoc.Page = theDoc.AddPage();
                theID = theDoc.AddImageToChain(theID);
            }


            // save PDF in the memory

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

        private string  GenerateHeader()
        {
            string html = "";
            html = html + @"<html>
                <head>         
                <style>
                * { margin: 0; padding: 0;}
                body { background: url(assets / bac.gif) no-repeat top left; height: 1145px; width: 810px;}
                img { border: 0px; margin: 0; padding: 0;}
                h1 {font-family:'Avant Garde', Arial, Helvetica, sans-serif; font-size: 20px; line-height: 16px; color:#000;text-transform: uppercase;font-style: normal;font-weight: 100;} 
                h2 {font-family:'Avant Garde', Arial, Helvetica, sans-serif; font-size: 14px; line-height: 16px; color:#000;text-transform: uppercase;font-style: normal;font-weight: 100;margin-bottom:5px;margin-top:15px;}
                p {font-family:'Avant Garde', Arial, Helvetica, sans-serif; font-size: 13px; line-height: 16px; color:#000;font-style: normal;font-weight: 100;line-height: 16px;margin:0;}
                .inner { padding: 0 6px 0 6px;} 
                .gcell {background:#e7e7e8;padding:0 12px 12px 12px;}
                .coltbl { width: 800px; table-layout: fixed; margin: 0;}
                .collft { width: 400px; padding-right:10px;}
                .colrgt { width: 400px; padding-left:10px;}
                .fs11 { font-size:11px;}
                .totaltbl {background-color: #e7e7e8;margin-top:12px;}
                .totaltr {background-color: #bdbec0;margin-top:4px;}
                .valuetd { text-align: right;}
                .valuettd { text-align: right; }
                .valuetd p { margin-right:10px; margin-top:8px; }
                .typetd h2 { margin: 0; margin-left:10px; margin-top:8px;}
                .typettd h2 {font-weight:bold; color:#000;margin-left:10px;margin-top:8px;font-size:15px;}
                .valuettd p {font-weight:bold; color:#000;margin-right:10px;margin-top:6px;font-size:14px;}
                .totalline {background-color: #fff;}
                .datahdrow { margin-top:15px; }
                .datahdrow td {font-family:'Avant Garde', Arial, Helvetica, sans-serif; font-size: 14px; line-height: 12px; color:#818284;text-transform: uppercase;font-style: normal;font-weight: 80;}
                .datatbl { margin-bottom:10px; }
                .datatbl td {  font-family:'Avant Garde', Arial, Helvetica, sans-serif; font-size: 15px; line-height: 18px; color:#000;font-style: normal;}
                .datatbl.rowheadingtr {background-color: #e7e7e8;}
                .datatbl.rowheadingtd {font-family:'Avant Garde', Arial, Helvetica, sans-serif; font-size: 14px; line-height: 12px; color:#818284;text-transform: uppercase;font-style: normal;font-weight: 80;padding:8px;}
                .datatbl.tdnumber {font-size:15px; color:#818284;}
                .datatbl.tdinner  { padding-right:10px;}
                .footer { margin-top:40px; padding-bottom:15px;}
                .footer.fleft { width: 300px; float:left;}
                .footer.fright { width: 300px; float:right; text-align:right;}
                .footer.fleft p, .footer.fright p { text-transform: uppercase; color:#818284;}
                .clear: { clear: both;}
                </style>
                </head>
                <body>
                <div class='inner'>
	            <table width='810' border='0' cellspacing='0' cellpadding='0' class='main'>
	                <tr>
	                <td><img src='$imagespacertoken$' width='265' height='20' alt='' /></td>
	                <td><img src='$imagespacertoken$' width='265' height='20' alt='' /></td>
	                <td><img src='$imagespacertoken$' width='265' height='20' alt='' /></td>
	                </tr>
                    <tr><td colspan='3' class='coltbl'><h1><b>Estimate " + estimateNumber.ToString() + @" Comparison " + revisionTitleA + @"&nbsp;--&nbsp; " + revisionTitleB + @"</b></h1></td></tr>
	                <tr><td>&nbsp;</td></tr>
                    <tr>                    
	                <td class='gcell' colspan='3' valign='top'>
	    	            <table width='100%' border='0' cellspacing='0' cellpadding='0' class='coltbl'>
	    		            <tr>
	    			            <td class='collft' valign='top'>
	    				            <h2>Customer Name/s:</h2>
	    			            </td>
	    			            <td class='colrgt' valign='top'>
	    				            <h2>" + customer + @"</h2>
	    			            </td>
	    		            </tr>
	    		            <tr>
	    			            <td class='collft' valign='top'>
						            <h2>Correspondence Address:</h2>
	    			            </td>
	    			            <td class='colrgt' valign='top'>
	    				            <h2>" + address + @"</h2>
	    			            </td>
	    		            </tr>
	    		            <tr>
	    			            <td class='collft' valign='top'>
						            <h2>Lot Address:</h2>
	    			            </td>
	    			            <td class='colrgt' valign='top'>
	    				            <h2>" + lotAddress + @"</h2>
	    			            </td>
	    		            </tr>
	    		            <tr>
	    			            <td class='collft' valign='top'>
						            <h2>House and Land Package:</h2>
	    			            </td>
	    			            <td class='colrgt' valign='top'>
	    				            <h2>" + houseAndLandPackage + @"</h2>
	    			            </td>
	    		            </tr>
	    		            <tr>
	    			            <td class='collft' valign='top'>
						            <h2>House Name:</h2>
	    			            </td>
	    			            <td class='colrgt' valign='top'>
	    				            <h2>" + houseName + @"</h2>
	    			            </td>
	    		            </tr>
	    	            </table>
	                </td>
	                </tr>
                    <tr>
                      <td><h2>Fields</h2></td>
                      <td><h2>"+ revisionTitleA+@"</h2></td>
                      <td><h2>"+ revisionTitleB + @"</h2></td>
                    </tr>
                    <tr>
                      <td><h2>Price Effective Date:</h2></td>
                      <td><h2>" + effectiveDateA.ToString("dd/MM/yyyy") + @"</h2></td>
                      <td><h2>" + effectiveDateB.ToString("dd/MM/yyyy") + @"</h2></td>
                    </tr>
                    <tr>
                      <td><h2>Home Price</h2></td>
                      <td><h2>" + homePriceA.ToString("c") + @"</h2></td>
                      <td><h2>" + homePriceB.ToString("c") + @"</h2></td>
                    </tr>
                    <tr>
                      <td><h2>Upgrade Value</h2></td>
                      <td><h2>" + upgradeValueA.ToString("c") + @"</h2></td>
                      <td><h2>" + upgradeValueB.ToString("c") + @"</h2></td>
                    </tr>
                    <tr>
                      <td><h2>Site Work Value</h2></td>
                      <td><h2>" + siteworkValueA.ToString("c") + @"</h2></td>
                      <td><h2>" + siteworkValueB.ToString("c") + @"</h2></td>
                    </tr>
                    <tr>
                      <td><h2>Total Price</h2></td>
                      <td><h2>" + totalPriceA.ToString("c") + @"</h2></td>
                      <td><h2>" + totalPriceB.ToString("c") + @"</h2></td>
                    </tr>
                    <tr>
                      <td><h2>Status</h2></td>
                      <td><h2>" + statusA + @"</h2></td>
                      <td><h2>" + statusB + @"</h2></td>
                    </tr>
                    <tr>
                      <td><h2>Owner</h2></td>
                      <td><h2>" + ownerA + @"</h2></td>
                      <td><h2>" + ownerB + @"</h2></td>
                    </tr>
             </table>

	            <table width='850' border='0' cellspacing='0' cellpadding='0' class='datahdrow' style='margin-bottom:0px;'>
	              <tr>
	                <td width='26'><img src='$imagespacertoken$' width='26' height='1' alt='' /></td>
	                <td width='158'><img src='$imagespacertoken$' width='158' height='1' alt='' /></td>
	                <td width='466'><img src='$imagespacertoken$ width='446' height='1' alt='' /></td>
	                <td width='50'><img src='$imagespacertoken$' width='50' height='1' alt='' /></td>
	                <td width='50'><img src='$imagespacertoken$' width='50' height='1' alt='' /></td>
	                <td  width='100'><img src='$imagespacertoken$' width='100' height='1' alt='' /></td>
	              </tr>
	              <tr>
	                <td colspan='2'>&nbsp;<br />Summary</td>
	                <td>&nbsp;<br />Description</td>
	                <td>&nbsp;<br />UOM</td>
	                <td>&nbsp;<br />QTY</td>
	                <td>Total Price<br />(INC. GST)</td>
	              </tr>
	              <tr>
	                <td ><img src='$imagespacertoken$' width='26' height='10' alt='' /></td>
	                <td ><img src='$imagespacertoken$' width='158' height='10' alt='' /></td>
	                <td ><img src='$imagespacertoken$' width='446' height='10' alt='' /></td>
	                <td ><img src='$imagespacertoken$' width='50' height='1' alt='' /></td>
	                <td ><img src='$imagespacertoken$' width='50' height='1' alt='' /></td>	    
	                <td ><img src='$imagespacertoken$' width='100' height='10' alt='' /></td>
	              </tr>	  
	            </table>
";

            html = html.Replace("$imagespacertoken$", Server.MapPath(@"~/images/spacer.gif"));
            /*
            string docBody = @"
                    <html>
                    <head>
                        <title></title>
                        <style type='text/css'>
                        body 
                        {
                            font-family:Tahoma, Verdana, Arial;
                            font-size: 10px;
                        }
                        table
                        {
                            border-color:#6699CC;
                            border-width:0 0 1px 1px;
                            border-style:solid;
                        }
                        td
                        {
                            border-color:#6699CC;
                            border-width:1px 1px 0 0;
                            border-style:solid;
                            padding:4px;
                            margin:0;
                            font-family:Tahoma, Verdana, Arial;
                            font-size: 10px;
                        }
                        .SummaryTable
                        {
                            border-style:none;
                        }
                        .TableHeader
                        {
                            background-color:#AFEEEE;
                            text-align:center;
                        }
                        .SourceRevision
                        {
                            background-color:#F0F8FF;
                        }
                        .DestinationRevision
                        {
                            background-color:#F5F5DC;
                        }

                        </style>
                    </head>
                    <body>
                        <table border='0' class='SummaryTable'>
                            <tr>
                                <td colspan='2' class='SummaryTable'><b>Estimate " + estimateNumber.ToString() + @" Comparison " + revisionTitleA + @"&nbsp;--&nbsp; " + revisionTitleB + @"<br /></td>
                            </tr>
                            <tr>
                                <td class='SummaryTable'>Customer:</td>
                                <td class='SummaryTable'>" + customer + @"</td>
                            </tr>
                            <tr>
                                <td class='SummaryTable'>Correspondence Address:</td>
                                <td class='SummaryTable'>" + address + @"</td>
                            </tr>
                            <tr>
                                <td class='SummaryTable'>Lot Address:</td>
                                <td class='SummaryTable'>" + lotAddress + @"</td>
                            </tr>
                            <tr>
                                <td class='SummaryTable'>House and Land Package:</td>
                                <td class='SummaryTable'>" + houseAndLandPackage + @"</td>
                            </tr>
                            <tr>
                                <td class='SummaryTable'>House Name:</td>
                                <td class='SummaryTable'>" + houseName + @"</td>
                            </tr>
                        </table>
                        <br />
                        <table cellpadding='0' cellspacing='0' width='100%'>
                            <tr>
                                <td colspan='3' class='TableHeader'>Estimate Header</td>
                            </tr>
                            <tr>
                                <td width='190' class='TableHeader'>Field</td>
                                <td width='200' class='TableHeader'>" + revisionTitleA + @"</td>
                                <td width='200' class='TableHeader'>" + revisionTitleB + @"</td>
                            </tr>
                            <tr>
                                <td>Price Effective Date</td>
                                <td class='SourceRevision'>" + effectiveDateA.ToString("dd/MM/yyyy") + @"</td>
                                <td class='DestinationRevision'>" + effectiveDateB.ToString("dd/MM/yyyy") + @"</td>
                            </tr>
                            <tr>
                                <td>Home Price</td>
                                <td class='SourceRevision'>" + homePriceA.ToString("c") + @"</td>
                                <td class='DestinationRevision'>" + homePriceB.ToString("c") + @"</td>
                            </tr>
                            <tr>
                                <td>Upgrade Value</td>
                                <td class='SourceRevision'>" + upgradeValueA.ToString("c") + @"</td>
                                <td class='DestinationRevision'>" + upgradeValueB.ToString("c") + @"</td>
                            </tr>
                            <tr>
                                <td>Site Work Value</td>
                                <td class='SourceRevision'>" + siteworkValueA.ToString("c") + @"</td>
                                <td class='DestinationRevision'>" + siteworkValueB.ToString("c") + @"</td>
                            </tr>
                            <tr>
                                <td>Total Price</td>
                                <td class='SourceRevision'>" + totalPriceA.ToString("c") + @"</td>
                                <td class='DestinationRevision'>" + totalPriceB.ToString("c") + @"</td>
                            </tr>
                            <tr>
                                <td>Status</td>
                                <td class='SourceRevision'>" + statusA + @"</td>
                                <td class='DestinationRevision'>" + statusB + @"</td>
                            </tr>
                            <tr>
                                <td>Owner</td>
                                <td class='SourceRevision'>" + ownerA + @"</td>
                                <td class='DestinationRevision'>" + ownerB + @"</td>
                            </tr>
                        </table> 
                    </body>
                    </html>";
                    */
            return html;
        }
        public string PrintEstimateBody(string html)
        {
            StringBuilder sb = new StringBuilder();
            int fontsize = 100;
            StringBuilder tempdesc = new StringBuilder();
            // Actual body contect starts from here.
            int RowIdentifier = 0;

            DBConnection DB = new DBConnection();
            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_CompareSaleEstimateDetails");
            EstimateViewCmd.Parameters["@estimateRevisionIdA"].Value = estimateRevisionIdA;
            EstimateViewCmd.Parameters["@estimateRevisionIdB"].Value = estimateRevisionIdB;

            DataSet OptionDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);
            //OptionDS.Tables[0].DefaultView.RowFilter = " change<>''";

            ArrayList AreaList = new ArrayList();

            foreach (DataRow OptionDR in OptionDS.Tables[0].DefaultView.Table.Rows)
            {
                tempdesc.Clear();
                if (OptionDR["change"] != null && OptionDR["change"].ToString().Trim() != "")
                {
                    if (OptionDR["AREANAME"].ToString() != "Area Surcharge")
                    {
                        RowIdentifier = RowIdentifier + 1;
                        if (!AreaList.Contains(OptionDR["AREANAME"].ToString()))
                        {
                            AreaList.Add(OptionDR["AREANAME"].ToString());
                            if (sb.ToString() != "")
                            {
                                sb.Append("</table>");
                            }

                            sb.Append("<table width='850' border='0' cellspacing='0' cellpadding='0' class='datatbl' style='width:850px;table-layout:fixed;'>");
                            sb.Append("<tr><td style='width:36px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + @"' width='36' height='1' alt='' /></td>");
                            sb.Append("<td style='width:158px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='158' height='1' alt='' /></td>");
                            sb.Append("<td style='width:456px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='456' height='1' alt='' /></td>");
                            sb.Append("<td style='width:50px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='50' height='1' alt='' /></td>");
                            sb.Append("<td style='width:50px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='50' height='1' alt='' /></td>");
                            sb.Append("<td style='width:100px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='100' height='1' alt='' /></td>");

                            sb.Append("<tr><td colspan='6' style='page-break-inside:avoid;'>");

                            sb.Append("<table width = '850' border = '0' cellspacing = '0' cellpadding = '0' class='datatbl' style='width:850px;table-layout:fixed;'>");
                            sb.Append("<tr><td style='width:36px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + @"' width='36' height='1' alt='' /></td>");
                            sb.Append("<td style='width:158px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='158' height='1' alt='' /></td>");
                            sb.Append("<td style='width:456px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='456' height='1' alt='' /></td>");
                            sb.Append("<td style='width:50px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='50' height='1' alt='' /></td>");
                            sb.Append("<td style='width:50px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='50' height='1' alt='' /></td>");
                            sb.Append("<td style='width:100px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='100' height='1' alt='' /></td></tr>");

                            sb.Append("<tr class='rowheadingtr'><td colspan='6' class='rowheadingtd' style='page-break-inside:avoid;'>" + OptionDR["AREANAME"].ToString());
                            sb.Append("</td></tr><tr><td colspan='6'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='1' height='10' alt='' /></td></tr>");
                            PrintEstimateBodyDetailItem(OptionDR, RowIdentifier, ref sb);

                            sb.Append("</table></td></tr>");
                        }
                        else
                        {   // 1px row border seperator

                            sb.Append("<tr class='trline'><td colspan='6' class='trline'><img src='" + Server.MapPath(@"~/images/line.gif") + "' width='846' height='17' alt='' /></td></tr>");

                            PrintEstimateBodyDetailItem(OptionDR, RowIdentifier, ref sb);
                        }

                    }
                }
            }
            sb.Append("</table><div class='clear'>&nbsp;</div></div></body></html>");

            return html+ sb.ToString();

        }

        public void PrintEstimateBodyDetailItem(DataRow OptionDR, int RowIdentifier, ref StringBuilder sb)
        {
            StringBuilder tempdesc = new StringBuilder();
            string tempimage = string.Empty;
            string tempdescB = "";

            //check if the product is promotionproduct
            bool promotionProduct = false;
            if(OptionDR["IsPromotionProductA"]!=null && OptionDR["IsPromotionProductA"].ToString()!="")
                 Convert.ToBoolean(OptionDR["IsPromotionProductA"]);

            bool isstudiomproduct=false;
 

            string ProductPrice = "";
            double Qty = 0.0;
            if (OptionDR["QuantityA"] != null && OptionDR["QuantityA"].ToString() != "")
                Qty=double.Parse(OptionDR["QuantityA"].ToString());
            else
                Qty = double.Parse(OptionDR["QuantityB"].ToString());

            if (OptionDR["totalprice"]!=null && OptionDR["totalprice"].ToString()!="" && OptionDR["totalprice"].ToString() != "TBA")
            {
                double RetailPrice = double.Parse(OptionDR["totalprice"].ToString());
                double totalprice = RetailPrice;
                ProductPrice = OptionDR["printprice"].ToString();//String.Format("{0:C}", totalprice);
            }
            else
                ProductPrice = "TBA";

            int packageType = 0;

            if (OptionDR["ProductDescriptionB"] != DBNull.Value && OptionDR["ProductDescriptionB"].ToString() != "")
            {
                tempdescB = OptionDR["ProductDescriptionB"].ToString();
                if (OptionDR["ProductDescriptionA"] != DBNull.Value && OptionDR["ProductDescriptionA"].ToString() != "" && OptionDR["ProductDescriptionA"].ToString() != OptionDR["ProductDescriptionB"].ToString())
                {
                    tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["ProductDescriptionB"].ToString()) + "</span><br>");

                    if (OptionDR["AdditionalInfoB"] != DBNull.Value && OptionDR["AdditionalInfoB"].ToString() != "" && OptionDR["AdditionalInfoA"] != DBNull.Value && OptionDR["AdditionalInfoA"].ToString() != "" && OptionDR["AdditionalInfoA"].ToString() != OptionDR["AdditionalInfoB"].ToString())
                    {
                        tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["AdditionalInfoB"].ToString()) + "</span><br>");
                        tempdescB= tempdescB+ Common.ReplaceCRLFByLineBreak(OptionDR["AdditionalInfoB"].ToString()) + "<br>";
                    }

                }
            }
            else if (OptionDR["AdditionalInfoB"] != DBNull.Value && OptionDR["AdditionalInfoB"].ToString() != "")
            {
                if (OptionDR["AdditionalInfoA"] != DBNull.Value && OptionDR["AdditionalInfoA"].ToString() != "" && OptionDR["AdditionalInfoA"].ToString() != OptionDR["AdditionalInfoB"].ToString())
                {
                    tempdescB = tempdescB + Common.ReplaceCRLFByLineBreak(OptionDR["AdditionalInfoB"].ToString()) + "<br>";
                    tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["AdditionalInfoB"].ToString()) + "</span><br>");
                }
                //else if ((OptionDR["productdescription"] != DBNull.Value && OptionDR["productdescription"].ToString() != ""))
                //{
                //    tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["productdescription"].ToString()) + "</span><br>");
                //}
                //tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["oldadditionalinfo"].ToString()) + "</span><br>");
            }

            if (OptionDR["ProductDescriptionA"] != DBNull.Value && OptionDR["ProductDescriptionA"].ToString() != "")
            {
                tempdesc.Append(Common.ReplaceCRLFByLineBreak(OptionDR["ProductDescriptionA"].ToString()));
            }
            if (OptionDR["AdditionalInfoA"] != DBNull.Value && OptionDR["AdditionalInfoA"].ToString() != "")
            {
                tempdesc.Append("<br>" + Common.ReplaceCRLFByLineBreak(OptionDR["AdditionalInfoA"].ToString()));
            }

            if (OptionDR["ExtraDescriptionB"] != DBNull.Value && OptionDR["ExtraDescriptionB"].ToString() != "" && OptionDR["ExtraDescriptionA"] != DBNull.Value && OptionDR["ExtraDescriptionA"].ToString() != "" && OptionDR["ExtraDescriptionA"].ToString()!= OptionDR["ExtraDescriptionB"].ToString())
            {
                tempdesc.Append("<span style='text-decoration: line-through;'><br><br><b>Extra Description:</b>" + Common.ReplaceCRLFByLineBreak(OptionDR["ExtraDescriptionB"].ToString()) + "</span>");
                tempdesc.Append("<br><b>Extra Description:</b>" + OptionDR["ExtraDescriptionA"].ToString());

                tempdescB = tempdescB + Common.ReplaceCRLFByLineBreak(OptionDR["ExtraDescriptionA"].ToString());
            }
            //else if (OptionDR["ENTERDESC"] != DBNull.Value && OptionDR["ENTERDESC"].ToString() != "")
            //{
            //    tempdesc.Append("<br><br><b>Extra Description:</b>" + OptionDR["ENTERDESC"].ToString());
            //}


            //if (isstudiomproduct)
            //{
            //    if (studiom != "")
            //    {
            //        tempdesc.Append("<br><br>" + studiom);
            //    }
            //}
            string temp2 = "";

            if (packageType == 1)
            {
                if (temp2 == "")
                {
                    temp2 = "<b>[Package Inclusion]</b>";
                }
                else
                {
                    temp2 = temp2 + ",<b>[Package Inclusion]</b>";
                }
            }
            if (packageType == 2)
            {
                if (temp2 == "")
                {
                    temp2 = "<b>[Developer Guideline]</b>";
                }
                else
                {
                    temp2 = temp2 + ",<b>[Developer Guideline]</b>";
                }
            }


            //if (OptionDR["displayAt"] != null && OptionDR["displayAt"].ToString().Trim() != "")
            //{
            //    if (temp2 == "")
            //    {
            //        temp2 = "<b>[" + OptionDR["displayAt"].ToString() + "]</b>";
            //    }
            //    else
            //    {
            //        temp2 = temp2 + ", <b>[" + OptionDR["displayAt"].ToString() + "]</b>";
            //    }
            //}

            //if (temp2 != "")
            //{
            //    tempdesc.Append("<br><br>" + temp2);
            //}

            //if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "")
            //{
            //    tempimage = "<br><img src='" + Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg") + "' width='" + w2.ToString() + "' height='" + h2.ToString() + "'/>";
            //}

            var colspanDesc = 1;
            if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "DELETED")
            {

                sb.Append("<tr><td valign='top' class='tdnumber'>" + RowIdentifier.ToString() + ".</td>");
                sb.Append("<td valign='top' style='word-wrap:break-word; page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'>" + OptionDR["ProductNameB"].ToString() + tempimage + "</span></span></div></td>");
                //if (includeUOMAndQuantity == "False")
                //{
                //    colspanDesc += 2;
                //}
                //if (estimateRevision_revisionTypeId == 7 || estimateRevision_revisionTypeId == 8 || estimateRevision_revisionTypeId == 9 || estimateRevision_revisionTypeId == 10 || estimateRevision_revisionTypeId == 11 || estimateRevision_revisionTypeId == 12 || estimateRevision_revisionTypeId == 21 || estimateRevision_revisionTypeId == 22)
                //{
                    // if it's studio M today's change, event hide price, only show description
                    sb.Append("<td valign='top' colspan='" + colspanDesc + "' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'><b>DELETED PRODUCT</b><br>" + tempdescB.ToString() + "</span></span></div></td>");
                //}
                //else
                //{
                //    sb.Append("<td valign='top' colspan='" + colspanDesc + "' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'><b>DELETED PRODUCT</b><br>" + tempdesc.ToString() + "</span></span></div></td>");
                //}

                    // SS - DELETED items we don't show QTY and UOM but leave empty on those TD's to be consistant aligned properly along with active items.
                    sb.Append("<td></td><td></td>");


                if (promotionProduct)
                {
                    sb.Append("<td valign='top'><span style='color:#666666;'><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></span><br></td></tr>");
                }
                else
                {

                    sb.Append("<td valign='top'><span style='color:#666666;'><span style='color:black;'>" + OptionDR["printprice"].ToString() + "</span></span></td></tr>");
                }
            }
            else
            {

                sb.Append("<tr><td valign='top' class='tdnumber'>" + RowIdentifier.ToString() + ".</td>");
                    sb.Append("<td valign='top' style='word-wrap:break-word; page-break-inside:avoid;'><div class='tdinner'>" + OptionDR["PRODUCTNAMEA"].ToString() + tempimage + "</div></td>");
 
                    if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "CHANGED")
                        sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'><b>AMENDED PRODUCT</b><br>" + tempdesc.ToString() + "</div></td>");
                    else if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "NEW")
                         sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'><b>NEW PRODUCT</b><br>" + tempdesc.ToString() + "</div></td>");
                    else
                        sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
 
                    sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["uomA"].ToString() + "</div></td>");
                    sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["quantityA"].ToString() + "</div></td>");
 
 


               if (promotionProduct)
                {
                    sb.Append("<td valign='top'><div class='tdinner'><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                }
                else
                {
                    decimal tempdecimal;

                        try
                        {
                            tempdecimal = decimal.Parse(OptionDR["printprice"].ToString().Replace("$", ""));
                            sb.Append("<td valign='top'><div class='tdinner'>" + String.Format("{0:C}", tempdecimal) + "</div></td></tr>");
                        }
                        catch (Exception)
                        {
                            sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["printprice"].ToString() + "</div></td></tr>");
                        }

                }
            }
            //}
        }
        private void GetEstimateInformation(int estimateNumber, out string lotAddress, out string customerAddress)
        {
            lotAddress = string.Empty;
            customerAddress = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetCustomerAddress";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateid", estimateNumber));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["CustomerAddress"] != DBNull.Value)
                                customerAddress = dr["CustomerAddress"].ToString();
                        }
                    }
                }
            }

            using (SqlConnection conn2 = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd2 = conn2.CreateCommand())
                {
                    cmd2.CommandText = "sp_SalesEstimate_GetEstimateLotAddress";
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.Add(new SqlParameter("@estimateid", estimateNumber));

                    conn2.Open();

                    using (SqlDataReader dr = cmd2.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["LotAddress"] != DBNull.Value)
                                lotAddress = dr["LotAddress"].ToString();
                        }
                    }
                }
            }
        }


        private void GetEstimateHeader(int estimateRevisionId,
                                        out int estimateNumber,
                                        out string revisionType,
                                        out int revisionNumber,
                                        out DateTime effectiveDate,
                                        out decimal homePrice,
                                        out decimal upgradeValue,
                                        out decimal siteworkValue,
                                        out decimal totalPrice,
                                        out string owner,
                                        out string status,
                                        out string homeName,
                                        out string customer,
                                        out string houseAndLand,
                                        out string customerDocument)
        {
            estimateNumber = 0;
            homePrice = 0m;
            revisionNumber = 0;
            upgradeValue = 0m;
            siteworkValue = 0m;
            totalPrice = 0m;
            revisionType = string.Empty;
            effectiveDate = DateTime.MinValue;
            owner = string.Empty;
            status = string.Empty;
            homeName = string.Empty;
            customer = string.Empty;
            houseAndLand = string.Empty;
            customerDocument = string.Empty;

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
                                estimateNumber = Convert.ToInt32(dr["EstimateNumber"]);

                            if (dr["HomePrice"] != DBNull.Value)
                                homePrice = Convert.ToDecimal(dr["HomePrice"]);

                            if (dr["RevisionNumber"] != DBNull.Value)
                                revisionNumber = Convert.ToInt32(dr["RevisionNumber"]);

                            if (dr["UpgradeValue"] != DBNull.Value)
                                upgradeValue = Convert.ToDecimal(dr["UpgradeValue"]);

                            if (dr["SiteWorkValue"] != DBNull.Value)
                                siteworkValue = Convert.ToDecimal(dr["SiteWorkValue"]);

                            totalPrice = homePrice + upgradeValue;

                            if (dr["RevisionTypeCode"] != DBNull.Value)
                                revisionType = dr["RevisionTypeCode"].ToString();

                            if (dr["CustomerDocumentName"] != DBNull.Value)
                                customerDocument = dr["CustomerDocumentName"].ToString();

                            if (dr["EffectiveDate"] != DBNull.Value)
                                effectiveDate = Convert.ToDateTime(dr["EffectiveDate"]);

                            if (dr["OwnerName"] != DBNull.Value)
                                owner = dr["OwnerName"].ToString();

                            if (dr["StatusName"] != DBNull.Value)
                                status = dr["StatusName"].ToString();

                            if (dr["HomeName"] != DBNull.Value)
                                homeName = dr["HomeName"].ToString();

                            if (dr["CustomerName"] != DBNull.Value)
                                customer = dr["CustomerName"].ToString();

                            if (dr["HouseAndLandPackage"] != DBNull.Value)
                                houseAndLand = dr["HouseAndLandPackage"].ToString();
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }

        }


    }

}