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

using WebSupergoo.ABCpdf5;
using WebSupergoo.ABCpdf5.Objects;
using WebSupergoo.ABCpdf5.Atoms;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class PrintEstimateComparison : System.Web.UI.Page
    {
        public const string PRINTPDF_DEFAULT_FONT = "Tahoma";
        public const double PRINTPDF_DEFAULT_FONTSIZE = 9;
        public const string HTML_EMPTY_STRING = "&nbsp;";
        private static string connectionstring = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ToString();
        string filterlist = "";
        protected void Page_Load(object sender, EventArgs e)
        {
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
            

            if(Page.Request.QueryString["filter"]!=null && Page.Request.QueryString["filter"].ToString()!="")
                    filterlist = Page.Request.QueryString["filter"].ToString();

            int estimateRevisionIdA = 0; 
            int estimateRevisionIdB = 0;

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
            theDoc.MediaBox.SetRect(0, 0, 842, 595);
            theDoc.Rect.String = "30 35 812 565";

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
            <td colspan='2' class='SummaryTable'><b>Estimate " + estimateNumber.ToString() + @" Comparison</b>&nbsp;&nbsp;&nbsp;&nbsp;Source: " + revisionTitleA + @"&nbsp;&nbsp;&nbsp;&nbsp;Destination: " + revisionTitleB + @"<br /></td>
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
            <td width='280' class='TableHeader'>Field</td>
            <td width='280' class='TableHeader'>" + revisionTitleA + @"</td>
            <td width='280' class='TableHeader'>" + revisionTitleB + @"</td>
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
    <br />
    <table cellpadding='0' cellspacing='0' width='840'>
        <tr>
            <td colspan='11' class='TableHeader'>Estimate Details</td>
        </tr>
        <tr>
            <td  class='TableHeader'>&nbsp;</td>
            <td colspan='4' class='TableHeader'>" + revisionTitleA + @"</td>
            <td colspan='4' class='TableHeader'>" + revisionTitleB + @"</td>
            <td class='TableHeader'>&nbsp;</td>
        </tr>
        <tr>
            <td class='SourceRevision' width='40'>Area/Group</td>
            <td class='SourceRevision' width='290'>Product Description</td>
            <td class='SourceRevision' width='20'>UOM</td>
            <td class='SourceRevision' width='30'>Qty</td>
            <td class='SourceRevision' width='45'>Price</td>
            <td class='DestinationRevision' width='290'>Product Description</td>
            <td class='DestinationRevision' width='20'>UOM</td>
            <td class='DestinationRevision' width='30'>Qty</td>
            <td class='DestinationRevision' width='45'>Price</td>
            <td width='30'>Changes</td>
        </tr>" + GetDetailsComparisonContent(estimateRevisionIdA, estimateRevisionIdB) + @"</table> 
</body>
</html>";

            int theID = theDoc.AddImageHtml(docBody);
            while (theDoc.Chainable(theID))
            {
                theDoc.Page = theDoc.AddPage();
                theID = theDoc.AddImageToChain(theID);
            }

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


        private void GetEstimateHeader( int estimateRevisionId,
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

        private string GetDetailsComparisonContent(int estimateRevisionIdA, int estimateRevisionIdB)
        {
            bool fullConparison = false;

            bool.TryParse(Request.QueryString["fullcomparison"], out fullConparison);

            StringBuilder contentString = new StringBuilder();

            DBConnection DB = new DBConnection();
            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_CompareSaleEstimateDetails");
            EstimateViewCmd.Parameters["@estimateRevisionIdA"].Value = estimateRevisionIdA;
            EstimateViewCmd.Parameters["@estimateRevisionIdB"].Value = estimateRevisionIdB;

            DataSet OptionDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);

            if(!fullConparison)
              OptionDS.Tables[0].DefaultView.RowFilter = " pagid in (" + filterlist + ")";

            //using (SqlConnection conn = new SqlConnection(connectionstring))
            //{
            //    using (SqlCommand cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = "sp_SalesEstimate_CompareSaleEstimateDetails";
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add(new SqlParameter("@estimateRevisionIdA", estimateRevisionIdA));
            //        cmd.Parameters.Add(new SqlParameter("@estimateRevisionIdB", estimateRevisionIdB));

            //        conn.Open();

            //        using (SqlDataReader dr = cmd.ExecuteReader())
            //        {
            //            while (dr.Read())
            //            {
            foreach (DataRow dr in  OptionDS.Tables[0].DefaultView.ToTable().Rows)                          
                   {
                        bool addToContent = true;
                        StringBuilder comparisonString = new StringBuilder();
                        comparisonString.Append("<tr>");
                        comparisonString.Append("<td>");
                        if (dr["AreaName"] != DBNull.Value)
                        {
                            if(dr["GroupName"] != DBNull.Value)
                            {
                                comparisonString.Append( dr["AreaName"].ToString()+@" / "+ dr["GroupName"].ToString());
                            }
                            else
                            {
                                comparisonString.Append(dr["AreaName"].ToString());
                            }
                        }
                        else
                        {
                            if (dr["GroupName"] != DBNull.Value)
                            {
                                comparisonString.Append( dr["GroupName"].ToString());
                            }
                            else
                            {
                                comparisonString.Append("&nbsp;");
                            }
                        }
                        comparisonString.Append("</td>");
                //comparisonString.Append("<td>");
                //comparisonString.Append(dr["AreaName"] != DBNull.Value ? dr["AreaName"].ToString() : HTML_EMPTY_STRING);
                //comparisonString.Append("</td>");

                //comparisonString.Append("<td>");
                //comparisonString.Append(dr["GroupName"] != DBNull.Value ? dr["GroupName"].ToString() : HTML_EMPTY_STRING);
                //comparisonString.Append("</td>");



                        string tempdesc= "&nbsp;";
                        if (dr["ProductDescriptionA"] != null && dr["ProductDescriptionA"].ToString() != "") tempdesc = dr["ProductDescriptionA"].ToString();
                        if (dr["ExtraDescriptionA"] != null && dr["ExtraDescriptionA"].ToString() != "") tempdesc = tempdesc+"<br> <b>Extra Desc:</b><br>"+ dr["ExtraDescriptionA"].ToString();
                        if (dr["AdditionalinfoA"] != null && dr["AdditionalinfoA"].ToString() != "") tempdesc = tempdesc + "<br> <b>Additonal Info:</b><br>" + dr["AdditionalinfoA"].ToString();

                        comparisonString.Append("<td>");
                        comparisonString.Append(dr["ProductNameA"] != DBNull.Value ? dr["ProductNameA"].ToString()+"<br>"+ tempdesc : tempdesc);
                        comparisonString.Append("</td>");

                        //comparisonString.Append("<td>");
                        //comparisonString.Append(tempdesc);
                        //comparisonString.Append("</td>");

                        comparisonString.Append("<td>");
                        comparisonString.Append(dr["UomA"] != DBNull.Value ? dr["UomA"].ToString() : HTML_EMPTY_STRING);
                        comparisonString.Append("</td>");

                        comparisonString.Append("<td>");
                        comparisonString.Append(dr["QuantityA"] != DBNull.Value ? Convert.ToDecimal(dr["QuantityA"]).ToString() : HTML_EMPTY_STRING);
                        comparisonString.Append("</td>");

                        comparisonString.Append("<td>");
                        comparisonString.Append(dr["PriceA"] != DBNull.Value ? Convert.ToDecimal(dr["PriceA"]).ToString("c") : HTML_EMPTY_STRING);
                        comparisonString.Append("</td>");

                        string tempdesc2 = "&nbsp;";
                        if (dr["ProductDescriptionB"] != null && dr["ProductDescriptionB"].ToString() != "") tempdesc2 = dr["ProductDescriptionB"].ToString();
                        if (dr["ExtraDescriptionB"] != null && dr["ExtraDescriptionB"].ToString() != "") tempdesc2 = tempdesc2 + "<br> <b>Extra Desc:</b><br>" + dr["ExtraDescriptionB"].ToString();
                        if (dr["AdditionalinfoB"] != null && dr["AdditionalinfoB"].ToString() != "") tempdesc2 = tempdesc2 + "<br> <b>Additonal Info:</b><br>" + dr["AdditionalinfoB"].ToString();

                        comparisonString.Append("<td>");
                        comparisonString.Append(dr["ProductNameB"] != DBNull.Value ? dr["ProductNameB"].ToString()+"<br>"+ tempdesc2 : tempdesc2);
                        comparisonString.Append("</td>");
                //comparisonString.Append("<td>");
                //comparisonString.Append(tempdesc2);
                //comparisonString.Append("</td>");

                       comparisonString.Append("<td>");
                        comparisonString.Append(dr["UomB"] != DBNull.Value ? dr["UomB"].ToString() : HTML_EMPTY_STRING);
                        comparisonString.Append("</td>");

                        comparisonString.Append("<td>");
                        comparisonString.Append(dr["QuantityB"] != DBNull.Value ? Convert.ToDecimal(dr["QuantityB"]).ToString() : HTML_EMPTY_STRING);
                        comparisonString.Append("</td>");

                        comparisonString.Append("<td>");
                        comparisonString.Append(dr["PriceB"] != DBNull.Value ? Convert.ToDecimal(dr["PriceB"]).ToString("c") : HTML_EMPTY_STRING);
                        comparisonString.Append("</td>");

                        comparisonString.Append("<td>");

                        if (dr["ProductNameA"] != DBNull.Value && dr["ProductNameB"] != DBNull.Value)
                        {
                            string changes = string.Empty;

                            if (dr["PriceA"].ToString() != dr["PriceB"].ToString())
                                changes = "PRC";

                            if (dr["QuantityA"].ToString() != dr["QuantityB"].ToString())
                                changes = changes == string.Empty ? "QTY" : changes + ", QTY";

                            if (dr["ProductDescriptionA"].ToString() != dr["ProductDescriptionB"].ToString() ||
                                dr["ExtraDescriptionA"].ToString() != dr["ExtraDescriptionB"].ToString() ||
                                dr["InternalDescriptionA"].ToString() != dr["InternalDescriptionB"].ToString())
                                changes = changes == string.Empty ? "DESC" : changes + ", DESC";

                            if (changes == string.Empty)
                            {
                                comparisonString.Append(HTML_EMPTY_STRING);

                                ////if (!fullConparison)
                                //    addToContent = true;
                            }
                            else
                                comparisonString.Append(changes);
                        }
                        else
                            comparisonString.Append("*");

                        comparisonString.Append("</td>");

                        comparisonString.Append("</tr>");

                        //if (addToContent)
                            contentString.Append(comparisonString);
                    }

            //            dr.Close();
            //        }

            //        conn.Close();
            //    }
            //}
            return contentString.ToString();
        }
    }
}