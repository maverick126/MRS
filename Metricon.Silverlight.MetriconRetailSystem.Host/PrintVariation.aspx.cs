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
using Metricon.Silverlight.MetriconRetailSystem.Host.Internal;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class PrintVariation : System.Web.UI.Page
    {
        private string consultantCode;
        private string consultantName;
        private string brandname;
        private string LotAddress = "";
        private string headerimagename = "spacer.gif";
        private int merge;
        private int breakdown;
        private double surcharge;
        private bool draft;
        private string BCContractnumber = "";
        private bool houseAndLandPkgContract;
        private int selectedPriceRegionStateID = 0;
        private string homebrandname = "";
        private string displaycentre = "";
        private string region = "";
        private string pricedate = "";
        private string packagecreateddate = "";
        private int EstimateID = 0;
        private DataSet dds;
        private MetriconSales MS;

        private int estimateRevisionId;
        private int estimateRevision_estimateId;
        private string estimateRevision_accountId;
        private string estimateRevision_opportunityId;
        private string estimateRevision_revisionNumber;
        private string estimateRevision_homeName;
        private string estimateRevision_revisionType;
        private string estimateRevision_revisionTypeBrief;
        private int estimateRevision_revisionTypeId;
        private string estimateRevision_revisionOwner;
        private string estimateRevision_effectiveDate;
        private double estimateRevision_landPrice;
        private double estimateRevision_homePrice;
        private double estimateRevision_siteworkPrice;
        private double estimateRevision_upgradePrice;
        private double estimateRevision_promotionValue;
        private double estimateRevision_surcharge;
        private string estimateRevision_state;
        private string estimateRevision_documenttype;
        private int estimateRevision_regionId;
        private int estimateRevision_statusId;
        private bool merge_areaSurcharge;
        private string estimateRevision_internalVersion;
        private string ESTIMATE_INTERNALCOPY_STAMP = "Internal Copy";
        private string BC_Company = "METHOMES";
        private string includeProductNameAndCode = "False";
        private string includeUOMAndQuantity = "False";
        private string includeSpecifications = "False";
        private string includeContractPriceOnVariation = "False";
        private string extentiondays = "0";
        private string variationnumber = "&nbsp;";

        private string printversion = "";
        private string primarycontact = "";
        private string primarycontactemail = "";
        private string documenttype = "";
        private string versiontype = "";
        private string routingorder = "";
        private string userid = "";
        private string action = "";
        private string recipients = "";
        private string recipientsemail = "";
        private string recipientsortorder = "";
        private string envelopeid = "";
        private string methods = "";
        private string docusignintegration = "0";
        private string emailsubject = "Metricon – Please DocuSign";
        private string emailbody = "Please review and sign document via the link above.";

        protected void Page_Load(object sender, EventArgs e)
        {
            // ===== Added to retrieve Estimate Header from Estimate Revision =====
            estimateRevision_internalVersion = "";

            if (Request.QueryString["type"] != null)
                estimateRevision_internalVersion = Request.QueryString["type"].ToString().ToUpper().Replace("VARIATION","CHANGEONLY");

            if (Request.QueryString["version"] != null && Request.QueryString["version"].ToString() != "")
                printversion = Request.QueryString["version"].ToString().ToUpper();

            if (Request.QueryString["IncludeProductNameAndCode"] != null)
                includeProductNameAndCode = Request.QueryString["IncludeProductNameAndCode"].ToString();

            if (Request.QueryString["IncludeUOMAndQuantity"] != null)
                includeUOMAndQuantity = Request.QueryString["IncludeUOMAndQuantity"].ToString();

            if (Request.QueryString["IncludeSpecifications"] != null)
                includeSpecifications = Request.QueryString["IncludeSpecifications"].ToString();

            if (Request.QueryString["IncludeContractPriceOnVariation"] != null)
                includeContractPriceOnVariation = Request.QueryString["IncludeContractPriceOnVariation"].ToString();

            if (Request.QueryString["docusignintegration"] != null)
                docusignintegration = Request.QueryString["docusignintegration"].ToString();

            if (docusignintegration == "1")// the following parameter only for docusign integration
            {
                if (Request.QueryString["documenttype"] != null)
                    documenttype = Request.QueryString["documenttype"].ToString();

                if (Request.QueryString["userid"] != null)
                    userid = Request.QueryString["userid"].ToString();

                if (Request.QueryString["action"] != null)
                    action = Request.QueryString["action"].ToString();

                if (Request.QueryString["routingorder"] != null)
                    routingorder = Request.QueryString["routingorder"].ToString();

                if (Request.QueryString["recipients"] != null)
                    recipients = Request.QueryString["recipients"].ToString();

                if (Request.QueryString["recipientsemail"] != null)
                    recipientsemail = Request.QueryString["recipientsemail"].ToString();

                if (Request.QueryString["sortorder"] != null)
                    recipientsortorder = Request.QueryString["sortorder"].ToString();

                if (Request.QueryString["envelopeid"] != null)
                    envelopeid = Request.QueryString["envelopeid"].ToString();

                if (Request.QueryString["methods"] != null)
                    methods = Request.QueryString["methods"].ToString();

                if (Request.QueryString["emailsubject"] != null)
                    emailsubject = Request.QueryString["emailsubject"].ToString();

                if (Request.QueryString["emailbody"] != null)
                    emailbody = Request.QueryString["emailbody"].ToString().Replace("~!~", Environment.NewLine);

            }


            estimateRevisionId = 0;
            Int32.TryParse(Request.QueryString["EstimateRevisionId"], out estimateRevisionId);

            GetEstimateRevisionHeaderDetails(estimateRevisionId);

            Session["AccountId"] = estimateRevision_accountId;
            Session["OpportunityId"] = estimateRevision_opportunityId;
            Session["OriginalLogOnState"] = estimateRevision_state;
            Session["SelectedRegionID"] = estimateRevision_regionId;



            bool isColourConsultant = false;
            if ((Session["OriginalLogOnState"] == null || Session["OriginalLogOnState"].ToString() == "" || Session["SelectedRegionID"] == null || Session["SelectedRegionID"].ToString() == "") && (Session["contractType"] != null && Session["contractType"].ToString() == "OPOL"))
            {
                string infos = @"<table><tr height='50'><td></td></tr>";

                infos = infos + @"<tr><td width='80'>&nbsp;</td><td><b>This estimate does not have valid suburb associated to it. 
                            <br><br>Please visit site tab to select valid suburb.</b></td></tr>";
                infos = infos + @"</table>";
                Response.Write(infos);
            }
            else
            {
                if (Session["selectedPriceRegionStateID"] != null)
                    selectedPriceRegionStateID = int.Parse(Session["selectedPriceRegionStateID"].ToString());

                //if (Session["contractType"] != null && Session["contractType"].ToString() == "PACKAGE")
                //    houseAndLandPkgContract = true;

                if (Session[Common.SELECTED_USER_SESSION_NAME] != null)
                {
                    consultantCode = Session[Common.SELECTED_USER_SESSION_NAME].ToString();

                    if (consultantCode != null)
                    {
                        DataSet userDs = Client.GetUserListFromUsercode(consultantCode);

                        if (userDs.Tables[0].Rows.Count > 0)
                        {
                            string userCatcode = userDs.Tables[0].Rows[0]["usercatcode"].ToString();

                            string[] singleUserCatcodes = userCatcode.ToUpper().Split(',');

                            foreach (string singleUserCatcode in singleUserCatcodes)
                            {
                                if (singleUserCatcode == @"CC")
                                {
                                    isColourConsultant = true;
                                }
                            }
                        }
                    }
                }

                int viewtype = 0;

                //if (Request.QueryString["surcharge"] != null)
                //    surcharge = double.Parse(Request.QueryString["surcharge"].ToString());

                if (Request.QueryString["merge"] != null)
                    merge = int.Parse(Request.QueryString["merge"].ToString());

                // ===== Modified - always shows a break down =====
                //if (Request.QueryString["breakdown"] != null)
                //    breakdown = int.Parse(Request.QueryString["breakdown"].ToString());
                breakdown = 1;
                // ==========

                //  ===== Modified to get Estimate ID from Estimate Revision =====
                EstimateID = estimateRevision_estimateId;
                // ==========

                if (Request.QueryString["printswitch"] != null)
                    viewtype = int.Parse(Request.QueryString["printswitch"].ToString());

                Doc theDoc = SetupPDF();

                // Uncomment it out to support blank customer details in Pdf for colour consultant.
                bool printCustomerDetails = isColourConsultant ? false : true;

                // Uncomment it out to print customer details regardless of Colour Consultant.
                // bool printCustomerDetails = true;

                if (EstimateID != 0)
                {
                    //  ===== Modified to get Estimate ID from Estimate Revision =====
                    pricedate = estimateRevision_effectiveDate;
                    // ==========

                    DataSet dtemp = GetPrintPDFTemplate(estimateRevisionId);
                    string PDFTemplate;
                    if (dtemp != null && dtemp.Tables[0].Rows.Count > 0)
                    {
                        PDFTemplate = dtemp.Tables[0].Rows[0]["PDFTemplate"].ToString();
                        headerimagename = dtemp.Tables[0].Rows[0]["headerimage"].ToString();
                        BC_Company = dtemp.Tables[0].Rows[0]["company"].ToString();
                        extentiondays = dtemp.Tables[0].Rows[0]["extendedday"].ToString();
                    }
                    else
                    {
                        PDFTemplate = "No valid template available.";
                    }

                    // ===========

                    // ===== Added to call Webservices without Parent Page =====
                    MS = new MetriconSales();
                    MS.Url = Utilities.GetMetriconSqsSalesWebServiceUrl();
                    MS.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    MS.Reconnect(BC_Company);
                    // ===========

                    string updatedpdfheader = PDFHeader(EstimateID, printCustomerDetails, PDFTemplate);
                    if (includeProductNameAndCode == "False")
                        updatedpdfheader = updatedpdfheader.Replace("Description", "").Replace("Summary", "Description");
                    if (includeUOMAndQuantity == "False")
                        updatedpdfheader = updatedpdfheader.Replace("QTY", "").Replace("UOM", "");
                    string Promotiontypeid = getPromotionType(EstimateID);
                    Doc BodyDoc = PrintEstimateBody(updatedpdfheader, EstimateID, theDoc, viewtype, Promotiontypeid);
                    SavePDF(BodyDoc);
                }
            }
        }

        public Doc SetupPDF()
        {
            // Set MediaBox size, and content rectangle
            // A4: w595 h842 
            Doc theDoc = new Doc();
            theDoc.MediaBox.SetRect(0, 0, 595, 842);
            theDoc.Rect.String = "30 35 565 812";
            //theDoc.Rect.Position(0, 0);
            return theDoc;
        }

        public string getPromotionType(int EstimateID)
        {
            string printheadervar = GetHeaderInformation(EstimateID);
            string[] headervar = printheadervar.Split('|');
            return headervar[17].ToString();
        }

        /// <summary>
        /// PDFs the header.
        /// </summary>
        /// <param name="EstimateID">The estimate ID.</param>
        /// <param name="printCustomerDetails">if set to <c>true</c> [print customer details].</param>
        /// <returns></returns>
        public string PDFHeader(int EstimateID, bool printCustomerDetails, string PDFTemplate)
        {
            string CustomerName = "";
            string CustomerName2 = "";
            string CustomerAddress = "";

            string EstimateCreatedDate = "";
            string EstimateActualDate = "";
            string BCCustomerid = "";
            string EstimateIDInHeader = "";
            string CartValue = "";
            string printheadervar = "";
            string houseAndLandPkg = "";

            printheadervar = GetHeaderInformation(EstimateID);

            string[] headervar = printheadervar.Split('|');

            string HouseName = estimateRevision_homeName; //headervar[6].ToString();
            
            // The followings are only printed if required, e.g. not a Colour Consultant.
            if (printCustomerDetails)
            {
                CustomerName = headervar[1].ToString();
                CustomerAddress = headervar[2].ToString();
                CustomerName2 = headervar[11].ToString();
                LotAddress = headervar[3].ToString();
                EstimateCreatedDate = headervar[4].ToString();
                EstimateActualDate = headervar[15].ToString();
                //EstimateExpiryDate = headervar[16].ToString();
                BCCustomerid = headervar[12].ToString();
                BCContractnumber = headervar[13].ToString();
                EstimateIDInHeader = EstimateID.ToString();
                CartValue = headervar[9].ToString();
                if (houseAndLandPkgContract)
                {
                    houseAndLandPkg = "Yes";
                }
                else
                {
                    houseAndLandPkg = "No";
                }
            }
            else
                houseAndLandPkg = "No";

            consultantName = headervar[20];

            PDFTemplate = PDFTemplate.Replace("$headerimagetoken$", Server.MapPath(@"~/images/" + headerimagename));
            PDFTemplate = PDFTemplate.Replace("$imagespacertoken$", Server.MapPath(@"~/images/spacer.gif"));
            PDFTemplate = PDFTemplate.Replace("$contactlisttoken$", CustomerName);
            PDFTemplate = PDFTemplate.Replace("$customernumbertoken$", BCCustomerid);
            PDFTemplate = PDFTemplate.Replace("$contactaddresstoken$", CustomerAddress);
            PDFTemplate = PDFTemplate.Replace("$lotaddresstoken$", LotAddress);
            PDFTemplate = PDFTemplate.Replace("$contractnumbertoken$", BCContractnumber);
            PDFTemplate = PDFTemplate.Replace("$estimatenumbertoken$", EstimateIDInHeader);
            PDFTemplate = PDFTemplate.Replace("$estimateactualdatetoken$", EstimateActualDate.ToString());
            PDFTemplate = PDFTemplate.Replace("$estimatecreatedtoken$", EstimateCreatedDate.ToString());
            PDFTemplate = PDFTemplate.Replace("$variationnumbertoken$", variationnumber);
            PDFTemplate = PDFTemplate.Replace("$brandimagetoken$", homebrandname);
            PDFTemplate = PDFTemplate.Replace("$createdbytoken$", estimateRevision_revisionOwner);
            PDFTemplate = PDFTemplate.Replace("$totallineimagetoken$", Server.MapPath(@"~/images/total-line.gif"));
            PDFTemplate = PDFTemplate.Replace("$housenametoken$", HouseName);
            PDFTemplate = PDFTemplate.Replace("$salesconsultanttoken$", consultantName);
            PDFTemplate = PDFTemplate.Replace("$displaylocationtoken$", displaycentre);
            PDFTemplate = PDFTemplate.Replace("$priceregiontoken$", region);
            PDFTemplate = PDFTemplate.Replace("$revisontypetoken$", estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")");
            PDFTemplate = PDFTemplate.Replace("$houseandlandpackagetoken$", houseAndLandPkg);

            if (includeUOMAndQuantity == "False" || estimateRevision_state.ToLower() == "qld" || estimateRevision_state.ToLower() == "nsw")
            {
                PDFTemplate = PDFTemplate.Replace("UOM", "");
                PDFTemplate = PDFTemplate.Replace("QTY", "");
            }

            return PDFTemplate;
        }

        public string GetHeaderInformation(int EstimateID)
        {
            string brand = ""; string homename = ""; int HOUSE_REGION;
            string CustomerName = ""; 
            string CustomerName2 = ""; 
            string CustomerAddress = "";
            //string brandlongdesc = "";
            string EstimateCreatedDate = "";
            string EstimateActualDate = "";
            string salesconsultant = "";

            string BCCustomerid = ""; string BCContractnumber = ""; string LotAddress = "";
            DataSet HPBHDS = Estimate.GetHomePriceBrandAndHomeName(EstimateID);
            if (HPBHDS != null)
                if (HPBHDS.Tables[0].Rows.Count > 0)
                {
                    DataRow HPBHRow = HPBHDS.Tables[0].Rows[0];
                    brand = HPBHRow["homebrandname"].ToString();
                    homebrandname = HPBHRow["homebrandname"].ToString();
                    BCContractnumber = HPBHRow["BCContractnumber"].ToString();
                    if (HPBHRow["displaylocation"] != null && HPBHRow["displaylocation"].ToString() != "")
                    {
                        displaycentre = HPBHRow["displaylocation"].ToString();
                    }
                    else
                    {
                        displaycentre = "";
                    }
                    homename = HPBHRow["homename"].ToString();
                    region = HPBHRow["regionname"].ToString();
                    HOUSE_REGION = int.Parse(HPBHRow["homeid"].ToString());
                    brandname = HPBHRow["brandname"].ToString().Trim();
                }
            try
            {
                //if (Session["AccountID"] != null)
                //{
                //    if (Session["AccountID"].ToString() != "")
                //    {
                        string accountid = Session["AccountID"].ToString();
                        string[] dataKeyNames = { "IVTU_SEQNUM" };
                        DataSet ds;

                //ds = MS.GetContactListForCustomerFromCRM(accountid, "", "1");
                CommonFunction cf = new CommonFunction();
                DataTable dt2 = cf.GetCustomerContactListFromACC(BCContractnumber);
                dt2.DefaultView.Sort = "isPrimary desc";
                //dt2.DefaultView.RowFilter = "RelationshipType='Current Owner'";
                DataView dv = dt2.DefaultView;

                DataTable customerDT = dv.ToTable();
                DataRow customerDR;

                if (customerDT.Rows.Count > 0)
                {
                    customerDR = customerDT.Rows[0];
                    CustomerAddress = customerDR["address"]
                                        + "," + customerDR["suburb"]
                                        + "," + customerDR["state"]
                                        + "," + customerDR["postcode"];
                }

                CustomerName = "";
                foreach (DataRow dr1 in customerDT.Rows)
                {
                    if (dr1["salutation"] != null && dr1["salutation"].ToString().ToUpper() != "ZZ")
                    {
                        CustomerName = CustomerName + dr1["salutation"] + " " + dr1["firstname"] + " " + dr1["lastname"] + "<br>";
                    }
                    else
                    {
                        CustomerName = CustomerName + dr1["firstname"] + " " + dr1["lastname"] + "<br>";
                    }
                }

                CustomerName = CustomerName.Length>4 ? CustomerName.Substring(0, CustomerName.Length - 4) : "";
                //    }
                //}
            }
            catch (NullReferenceException nex2)
            {
                Response.Write(nex2.Message.ToString() + @"<br>" + nex2.Source.ToString() + "<br>session AccountID problem.");
            }
            //get the estimate created date, bccontractnumber, bccustomernumber 
            DataSet EstimateHeaderDS = Estimate.GetEstimateDetailsForPrint(EstimateID);
            foreach (DataRow EstimateRow in EstimateHeaderDS.Tables[0].Rows)
            {

                if (EstimateRow["EstimateActualDate"] == DBNull.Value)
                    EstimateActualDate = "";
                else
                    EstimateActualDate = DateTime.Parse(EstimateRow["EstimateActualDate"].ToString()).ToString("dd/MM/yyyy");

                int estimateStatus = Estimate.CheckEstimateStatus(EstimateID);


                BCCustomerid = EstimateRow["BcCustomerid"].ToString();
                BCContractnumber = EstimateRow["BCContractnumber"].ToString();
                salesconsultant = EstimateRow["salesconsultant"].ToString();
            }
            // get estimate date and expirydate
            DBConnection DB = new DBConnection();
            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateHeaderForLogging");
            EstimateViewCmd.Parameters["@revisionId"].Value = estimateRevisionId;
            DataSet dstemp = DB.SelectSqlStoredProcedure(EstimateViewCmd);
            if (dstemp.Tables[0].Rows.Count > 0)
            {
                EstimateCreatedDate = DateTime.Parse(dstemp.Tables[0].Rows[0]["createdon"].ToString()).ToString("dd/MM/yyyy");
                variationnumber = dstemp.Tables[0].Rows[0]["VariationNumber"].ToString();
            }
            else
            {
                EstimateCreatedDate = "&nbsp;";
                //EstimateExpiryDate = "&nbsp;";
            }


            //get the lot address where the customer are building
            DataSet LotAddressds = null;
            try
            {
                string oppid = "";

                if (Session["OpportunityID"] != null)
                    oppid = Session["OpportunityID"].ToString();

                LotAddressds = MS.GetSiteDetailsForOpportunityContractInDataSet(oppid);

                if (Session["SelectedRegionID"] == null || Session["SelectedRegionID"].ToString() == "")
                {
                    Session["SelectedRegionID"] = LotAddressds.Tables[0].Rows[0]["fkidregion"].ToString();
                }

            }
            catch (NullReferenceException nex3)
            {
                Response.Write(nex3.Message.ToString() + @"<br>" + nex3.Source.ToString() + "<br>session OpportunityID problem.");
            }
            string houseAndLandPkg = "No";
            try
            {

                if (LotAddressds.Tables[0].Rows.Count > 0 && LotAddressds.Tables[0].Rows[0]["HouseAndLandPkg"].ToString() == "True")
                    houseAndLandPkg = "Yes";

                LotAddress = GetEstimateLotAddress(EstimateID);

            }
            catch (NullReferenceException nex4)
            {
                Response.Write(nex4.Message.ToString() + @"<br>" + nex4.Source.ToString() + "<br>LotAddressds problem.");
            }
            return string.Empty + "|" + CustomerName + "|" + CustomerAddress + "|" + LotAddress + "|" + EstimateCreatedDate + "|" +
           brand + "|" + homename + "|" + string.Empty + "|" + string.Empty + "|" +
           string.Empty + "|" + string.Empty + "|" + CustomerName2 + "|" + BCCustomerid + "|" + BCContractnumber + "|" + string.Empty + "|" +
           EstimateActualDate + "|" + string.Empty + "|" + string.Empty + "|" + string.Empty + "|" + houseAndLandPkg + "|" + salesconsultant + "|" + string.Empty;
        }

        /// <summary>
        /// Create an estimate body in html format, related to an Estimate.
        /// </summary>
        /// <param name="HTML">The HTML.</param>
        /// <param name="estimateid">estimate id.</param>
        /// <param name="theDoc">The doc.</param>
        /// <param name="printCustomerDetails">if set to <c>true</c> [print customer details].</param>
        /// <param name="promotiontypeid">The promotiontypeid.</param>
        /// <returns>Pdf document</returns>
        public Doc PrintEstimateBody(string pdftemplate, int estimateid, Doc theDoc, int viewtype, string promotiontypeid)
        {
            StringBuilder sb = new StringBuilder();
            string underReview = "";
            int fontsize = 100;
            StringBuilder tempdesc = new StringBuilder();
            decimal variationTotal = 0m;

            // Actual body contect starts from here.
            int RowIdentifier = 0;

            DBConnection DB = new DBConnection();
            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateDetailsForVariation");
            EstimateViewCmd.Parameters["@revisionId"].Value = estimateRevisionId;

            DataSet OptionDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);
            ArrayList AreaList = new ArrayList();


            foreach (DataRow OptionDR in OptionDS.Tables[0].Rows)
            {
                tempdesc.Clear();
                if (merge != 1 || (OptionDR["AREANAME"].ToString() != "Area Surcharge" && merge == 1))
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
                        PrintEstimateBodyDetailItem(OptionDR, RowIdentifier, ref variationTotal, ref sb);

                        sb.Append("</table></td></tr>");
                    }
                    else
                    {   // 1px row border seperator

                        sb.Append("<tr class='trline'><td colspan='6' class='trline'><img src='" + Server.MapPath(@"~/images/line.gif") + "' width='846' height='17' alt='' /></td></tr>");

                        PrintEstimateBodyDetailItem(OptionDR, RowIdentifier, ref variationTotal, ref sb);
                    }

                }
            }
            sb.Append("</table><div class='clear'>&nbsp;</div>");

            // replace the tokens
            string FinalHTML = pdftemplate.Replace("$estimatebodytoken$", sb.ToString());
            if (includeContractPriceOnVariation == "True")
                FinalHTML = FinalHTML.Replace("$totalpricelabeltoken$", "VARIATION<BR/>TOTAL ");
            else
                FinalHTML = FinalHTML.Replace("$totalpricelabeltoken$", "TOTAL ");
            FinalHTML = FinalHTML.Replace("$totalpricetoken$", variationTotal.ToString("c"));

            // replace the tokens
            if (includeContractPriceOnVariation == "True")
            {
                FinalHTML = FinalHTML.Replace("$totalpriceline2displaytoken$", "block");
                FinalHTML = FinalHTML.Replace("$totalpriceline2labeltoken$", "AMENDED<BR/>CONTRACT<BR/>PRICE ");

                double amendedContractTotal = 0.00;
                DataSet ds = GetEstimateHeaderInformation(estimateRevisionId.ToString());
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    double.TryParse(ds.Tables[0].Rows[0]["TotalPrice"].ToString(), out amendedContractTotal);
                }
                FinalHTML = FinalHTML.Replace("$totalpriceline2token$", amendedContractTotal.ToString("c"));
            }
            else
            {
                FinalHTML = FinalHTML.Replace("$totalpriceline2displaytoken$", "none");
                FinalHTML = FinalHTML.Replace("$totalpriceline2labeltoken$", "");
                FinalHTML = FinalHTML.Replace("$totalpriceline2token$", "");
            }

            int theID = theDoc.AddImageHtml(FinalHTML);

            string[] sortorderarray = recipientsortorder.Split(',');
            string[] recipientarray = recipients.Split(',');
            string[] methodarray = methods.Split(',');
            // Add the last page.
            while (true)
            {
                if (!theDoc.Chainable(theID))
                {
                    //if (includeSpecifications == "True")
                    //{
                        //string filename = getSpecification(estimateRevisionId.ToString());
                        //if (filename != "")
                        //{
                        //    string pdfPath = ConfigurationManager.AppSettings["PDFPath"].ToString();

                        //    if (File.Exists(pdfPath + filename))
                        //    {
                        //        Doc theDoc2 = new Doc();
                        //        theDoc2.Read(pdfPath + filename);

                        //        theDoc.Append(theDoc2);
                        //    }
                        //}
                    //}

                        string customerInfo = GetHeaderInformation(estimateid);
                                        string[] headervars = customerInfo.Split('|');
                                        string tempStr;
                                        string temp = headervars[1].Replace("<br>", "|");
                                        string[] contacts = temp.Split('|');


                                        tempStr = "<table width=600px cellspadding=0 cellspacing=0>";

                                        tempStr = "<table width='100%' cellspadding=0 cellspacing=0 border=0 style='font-weight:bold; font-family: arial; font-size:14;'>";
                                        for (int i = 0; i < contacts.Length; i++)
                                        {
                                            tempStr = tempStr + "<tr height='20px'><td>&nbsp;</td></tr>";
                                            if (docusignintegration == "0")
                                            {

                                                //tempStr = tempStr + "<tr><td width=400px align=left style='font-weight:bold; font-family: arial; font-size:12; '>" + contacts[i] + "</td><td width=380px align=right style='font-weight:bold; font-family: arial; font-size:12;'>Sign Here _______________&nbsp;Date __________</td></tr>";
                                                tempStr = tempStr + "<tr valign='top'><td  width='5%'>&nbsp;</td><td width='48%' align=left style='font-weight:bold; font-family: arial; font-size:14;'>" + contacts[i] + "</td><td width='47%' align=left style='font-weight:bold; font-family: arial; font-size:14;'>Sign Here " + "<span style='color:#FFFFFF'>/S" + (i + 1).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + "<span style='color:#FFFFFF'>/D" + (i + 1).ToString() + "/</span>" + "______________________________</td></tr>";
                                            }
                                            else
                                            {
                                                for (int k = 0; k < sortorderarray.Length; k++)
                                                {
                                                    if ((i + 1).ToString() == sortorderarray[k])
                                                    {
                                                        tempStr = tempStr + "<tr valign='top'><td  width='5%'>&nbsp;</td><td width='48%' align=left style='font-weight:bold; font-family: arial; font-size:14; '>" + contacts[i] + "</td><td width='47%' align=left style='font-weight:bold; font-family: arial; font-size:14;'>Sign Here " + "<span style='color:#FFFFFF'>/S" + (i + 1).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + "<span style='color:#FFFFFF;'>/D" + (i + 1).ToString() + "/</span>" + "______________________________</td></tr>";
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        // add buidler signer tab
                                        if (docusignintegration == "1")
                                        {
                                            for (int m = 0; m < recipientarray.Length; m++)
                                            {
                                                tempStr = tempStr + "<tr height='20px'><td>&nbsp;</td></tr>";
                                                if (int.Parse(sortorderarray[m]) > 100 && methodarray[m] != "3")
                                                {
                                                    tempStr = tempStr + "<tr><td  width='5%'>&nbsp;</td><td width='48%' align=left style='font-weight:bold; font-family: arial; font-size:14;'>" + recipientarray[m] + " (Builder)</td><td align=left style='font-weight:bold; font-family: arial; font-size:14;'>Sign Here " + "<span style='color:#FFFFFF'>/B" + (int.Parse(sortorderarray[m]) - 100).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; " + "<span style='color:#FFFFFF;'>/BD" + (int.Parse(sortorderarray[m]) - 100).ToString() + "/</span>" + "______________________________</td></tr>";
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (Session["OriginalLogOnState"].ToString() == "QLD" || Session["OriginalLogOnState"].ToString() == "NSW" || Session["OriginalLogOnState"].ToString() == "SA")
                                            {
                                                tempStr = tempStr + "<tr valign='top'><td  colspan='3'>&nbsp;</td></tr>";
                                                tempStr = tempStr + "<tr valign='top'><td  width='5%'>&nbsp;</td><td width='48%' align=left style='font-weight:bold; font-family: arial; font-size:14;'>Builder</td><td align=left style='font-weight:bold; font-family: arial; font-size:14;'>Sign Here <span style='color:#FFFFFF'>/B2/</span>______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style='color:#FFFFFF;'>/BD2/</span> ______________________________</td></tr>";
                                                //tempStr = tempStr + "<tr height='40px'><td>&nbsp;</td></tr>";
                                                //tempStr = tempStr + "<tr><td  width='2%'>&nbsp;</td><td align=left style='font-weight:bold; font-family: arial; font-size:14;'>Builder:</td><td align=left style='font-weight:bold; font-family: arial; font-size:14;'>&nbsp;</td></tr>";
                                                //tempStr = tempStr + "<tr height='20px'><td>&nbsp;</td></tr>";
                                                //tempStr = tempStr + "<tr><td width='100%' colspan='3' '>________________________________________________________________________________________________________________________</td></tr>";

                                                //tempStr = tempStr + "<tr height='40px'><td>&nbsp;</td></tr>";
                                                //tempStr = tempStr + "<tr><td  width='2%'>&nbsp;</td><td align=left style='font-weight:bold; font-family: arial; font-size:14;'>Date:</td><td align=left style='font-weight:bold; font-family: arial; font-size:14;'>&nbsp;</td></tr>";
                                                //tempStr = tempStr + "<tr height='20px'><td>&nbsp;</td></tr>";
                                                //tempStr = tempStr + "<tr><td width='100%' colspan='3' >________________________________________________________________________________________________________________________</td></tr>";
                                            }
                                        }
                                        tempStr = tempStr + "</table>";

                                        // Add the Disclaimer/Acknowledgements body text.
                                        Doc theDoc3 = new Doc();
                                        theDoc3.MediaBox.SetRect(0, 0, 595, 842);
                                        theDoc3.Rect.String = "30 35 565 812";


                                        theDoc3.Color.Color = System.Drawing.Color.Black;
                                        theDoc3.Font = theDoc3.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                                        theDoc3.TextStyle.Size = Common.PRINTPDF_DISCLAIMER_FONTSIZE;
                                        theDoc3.TextStyle.Bold = false;
                                        theDoc3.Rect.Pin = 0;
                                        theDoc3.Rect.Position(20, 20);
                                        theDoc3.Rect.Width = 500;
                                        theDoc3.Rect.Height = 750;
                                        theDoc3.TextStyle.LeftMargin = 5;

                                        string disclaimer = Common.getDisclaimer(estimateRevisionId.ToString(), Session["OriginalLogOnState"].ToString(), estimateRevision_internalVersion).Replace("$Token$", tempStr);
                                        disclaimer = disclaimer.Replace("$printdatetoken$", DateTime.Now.ToString("dd/MMM/yyyy"));
                                        disclaimer = disclaimer.Replace("$logoimagetoken$", Server.MapPath("~/images/metlog.jpg"));

                                        if (variationnumber == "--")
                                        {
                                            disclaimer = disclaimer.Replace("$DaysExtension$", "&nbsp;");
                                        }
                                        else
                                        {
                                            disclaimer = disclaimer.Replace("$DaysExtension$", string.Format("EXTENSION OF TIME {0} DAYS (Due to the above variation)", extentiondays));
                                        }

                                        // If QLD, then use a real deposit amount in the Agreement
                                        if (Session["OriginalLogOnStateID"] != null)
                                        {
                                            if (Session["OriginalLogOnStateID"].ToString() == "3")
                                            {
                                                DBConnection DBCon = new DBConnection();
                                                SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_checkIfContractDeposited");
                                                Cmd.Parameters["@contractNo"].Value = BCContractnumber;
                                                DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);
                                                double amount = 0;
                                                if (ds != null && ds.Tables[0].Rows.Count > 0)
                                                    amount = double.Parse(ds.Tables[0].Rows[0]["DepositAmount"].ToString());
                                                if (amount > 0.00)
                                                {
                                                    disclaimer = disclaimer.Replace("$DepositAmount$", "payment of " + String.Format("{0:C}", amount));
                                                }
                                                else
                                                {
                                                    disclaimer = disclaimer.Replace("$DepositAmount$", "payment as receipted");
                                                }
                                            }
                                        }

                                        if (disclaimer.Trim() != "")
                                        {
                                            //theDoc3.AddFont("Century Gothic");
                                            int docid = theDoc3.AddImageHtml(disclaimer);
                                            while (true)
                                            {
                                                if (!theDoc3.Chainable(docid))
                                                {
                                                    break;
                                                }

                                                theDoc3.Page = theDoc3.AddPage();
                                                docid = theDoc3.AddImageToChain(docid);
                                            }
                                        }

                                        theDoc.Append(theDoc3);



                    break;
                }

                theDoc.Page = theDoc.AddPage();
                theID = theDoc.AddImageToChain(theID);
 
            }

            // Add Page number footer to each page


            theDoc.Rect.String = Common.ORDERFORM_PAGENUMBER_COORDINATES; // "30 20 565 32"

            for (int i = 1; i <= theDoc.PageCount; i++)
            {
                theDoc.PageNumber = i;

                // Add the expired watermark.
                if (estimateRevision_internalVersion.ToUpper() == "INTERNAL" || estimateRevision_internalVersion.ToUpper() == "STUDIOM")
                {
                    theDoc.TextStyle.Bold = true;
                    theDoc.Font = theDoc.AddFont(Common.ORDERFORM_FONT);
                    theDoc.FontSize = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_FONTSIZE;
                    theDoc.Color.String = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR;
                    theDoc.Color.Alpha = Common.ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE;

                    theDoc.Pos.String = "50 300";

                    theDoc.Transform.Reset();
                    theDoc.Transform.Rotate(45, 50, 300);

                    theDoc.AddText(ESTIMATE_INTERNALCOPY_STAMP);
                }

                DBConnection DBCon = new DBConnection();
                SqlCommand sqlCmd = DBCon.ExecuteStoredProcedure("spw_GetHomePrintWatermark");
                sqlCmd.Parameters["@revisionId"].Value = estimateRevisionId;
                sqlCmd.Parameters["@printversion"].Value = estimateRevision_internalVersion;
                DataSet ds = DBCon.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null)
                {
                    underReview = ds.Tables[0].Rows[0]["watermark"].ToString();
                    fontsize = Int16.Parse(ds.Tables[0].Rows[0]["font"].ToString());
                }

                if (!string.IsNullOrWhiteSpace(underReview))
                {
                    theDoc.Font = theDoc.AddFont(Common.ORDERFORM_FONT);
                    theDoc.FontSize = fontsize;
                    theDoc.Color.String = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR;
                    theDoc.Color.Alpha = Common.ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE;
                    theDoc.Pos.String = "175 400";
                    theDoc.Transform.Reset();
                    theDoc.Transform.Rotate(45, 250, 400);

                    theDoc.AddHtml(underReview);
                }
                theDoc.TextStyle.Bold = false;

                theDoc.Rect.Pin = 0;
                theDoc.Rect.Position(50, 10);
                theDoc.Rect.Width = 450;
                theDoc.Rect.Height = 20;
                theDoc.TextStyle.LeftMargin = 15;
                theDoc.Color.String = "40 40 40";
                theDoc.TextStyle.Size = 6;
                theDoc.Transform.Reset();

                //if (Session["OriginalLogOnState"].ToString() == "QLD")
                //{
                //    theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Builder Initials:________________________&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer Initials:________________________");
                //}
                //else
                //{
                //    theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")");
                //}
                if (Session["OriginalLogOnState"].ToString() == "QLD")
                {
                    //theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Builder Initials:_____________________&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer Initials:________________________<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;E&OE");
                    theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   <br>E&EO&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Builder Initials:_____________________&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer Initials:________________________");
                    //if (estimateRevision_documenttype.ToLower().Trim().Contains("contract"))
                    //{
                    //    theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Builder Initials:_____________________&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer Initials:________________________<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;E&OE");
                    //}
                    //else
                    //{
                    //    theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;E&OE");
                    //}                 
                }
                else
                {
                    theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;E&OE");
                }
                theDoc.Rect.Pin = 0;
                theDoc.Rect.Position(450, 10);
                theDoc.Rect.Width = 110;
                theDoc.Rect.Height = 20;
                theDoc.TextStyle.LeftMargin = 20;
                theDoc.Color.String = "40 40 40";
                theDoc.TextStyle.Size = 6;

                theDoc.Transform.Reset();           // Default, no rotation.

                theDoc.AddHtml(DateTime.Today.Date.ToString("dd/MM/yyyy") + " | PAGE " + i.ToString() + " OF " + theDoc.PageCount.ToString());
                theDoc.Flatten();
            }

            #region add attachment
            if (includeSpecifications == "True")
            {
                string systemid = "4";
                if (estimateRevision_documenttype.ToUpper() == "CONTRACT")
                    systemid = "4";
                else if (estimateRevision_documenttype.ToUpper().StartsWith("PC"))
                    systemid = "3";
                else if (estimateRevision_documenttype.ToUpper() == "VARIATION")
                    systemid = "5";

                DataSet dattach = getAllAttachments(estimateid.ToString(), systemid, DateTime.Now, "0", "0"); //2 means "sales estimate"
                if (dattach != null)
                {
                    int idx = 0;
                    foreach (DataRow dr in dattach.Tables[0].Rows)
                    {
                        if (dr["attachmentname"].ToString() != "")
                        {
                            if (File.Exists(ConfigurationManager.AppSettings["PDFPath"].ToString() + dr["attachmentname"].ToString()))
                            {
                                idx = idx + 1;
                                Doc theDoc2 = new Doc();
                                theDoc2.Read(ConfigurationManager.AppSettings["PDFPath"].ToString() + dr["attachmentname"].ToString());

                                #region add attachment header for future splitting
                                theDoc2.FontSize = 1;
                                theDoc2.HPos = 1;
                                theDoc2.Color.String = "255 255 255";
                                theDoc2.Transform.Reset();

                                theDoc2.AddHtml(@"ATTACHMENT_" + idx.ToString() + "_" + dr["attachmenttypename"].ToString());
                                theDoc2.Flatten();
                                #endregion

                                theDoc.Append(theDoc2);
                            }
                        }
                    }
                }
            }
            #endregion

            return theDoc;

        }
        public DataSet getAllAttachments(string estimateid, string systemid, DateTime pricelistdate, string pricelistregionid, string pricebrandid)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_GetAllAttachmentsByEstimate");
                sqlCmd.Parameters["@estimateid"].Value = estimateid;
                sqlCmd.Parameters["@systemID"].Value = systemid;
                sqlCmd.Parameters["@pricelistdate"].Value = pricelistdate;
                sqlCmd.Parameters["@priceregionid"].Value = pricelistregionid;
                sqlCmd.Parameters["@pricebrandid"].Value = pricebrandid;
                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                return ds;
            }
            catch
            {
                return null;
            }
        }
        public void PrintEstimateBodyDetailItem(DataRow OptionDR, int RowIdentifier, ref decimal variationTotal, ref StringBuilder sb)
        {
            StringBuilder tempdesc = new StringBuilder();
            string tempimage = string.Empty;

            //check if the product is promotionproduct
            bool promotionProduct = Convert.ToBoolean(OptionDR["promotionproduct"]);

            bool isstudiomproduct;
                    if (OptionDR["isstudiomproduct"].ToString() == "0")
                        isstudiomproduct = false;
                    else
                        isstudiomproduct = true;

                    string ProductPrice = "";
                    double Qty = double.Parse(OptionDR["QUANTITY"].ToString());
                    if (OptionDR["totalprice"].ToString() != "TBA")
                    {
                        double RetailPrice = double.Parse(OptionDR["totalprice"].ToString());
                        double totalprice = RetailPrice;
                        ProductPrice = OptionDR["printprice"].ToString();//String.Format("{0:C}", totalprice);
                    }
                    else
                        ProductPrice = "TBA";

                    int packageType = 0;
                    if (houseAndLandPkgContract)
                    {
                        if (OptionDR["StandardPackageInclusion"].ToString() != "")
                            packageType = Int16.Parse(OptionDR["StandardPackageInclusion"].ToString());
                    }
                    string studiom = ExtractStudioMAnswer(OptionDR["studiomattributes"].ToString());

                    System.Drawing.Image img;
                    int originw = 150;
                    int originh = 120;
                    double w2 = 0;
                    double h2 = 0;


                    if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "") // there is a image selected
                    {
                        byte[] b = (byte[])OptionDR["image"];
                        MemoryStream ms = new MemoryStream(b);
                        //Bitmap bi = new Bitmap(ms);
                        img = (System.Drawing.Image)System.Drawing.Image.FromStream(ms);

                        if (!File.Exists(Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg")))
                        {
                            img.Save(Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                        }

                        if (img.Width > img.Height) // landscape
                        {
                            w2 = originw;
                            h2 = (img.Height* originw) / img.Width;
                        }
                        else // portrit image
                        {
                            w2 = (img.Width* originh) / img.Height;
                            h2 = originh;
                        }
                    }

                    if (OptionDR["oldproductdescription"] != DBNull.Value && OptionDR["oldproductdescription"].ToString() != "")
                    {
                        if (OptionDR["productdescription"] != DBNull.Value && OptionDR["productdescription"].ToString() != "" && OptionDR["productdescription"].ToString()!= OptionDR["oldproductdescription"].ToString())
                        {
                            tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["oldproductdescription"].ToString()) + "</span><br>");

                            if (OptionDR["oldadditionalinfo"] != DBNull.Value && OptionDR["oldadditionalinfo"].ToString() != "" && OptionDR["additionalinfo"] != DBNull.Value && OptionDR["additionalinfo"].ToString() != "" && OptionDR["additionalinfo"].ToString() != OptionDR["oldadditionalinfo"].ToString())
                            {
                                tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["oldadditionalinfo"].ToString()) + "</span><br>");
                            }

                        }
                    }
                    else if (OptionDR["oldadditionalinfo"] != DBNull.Value && OptionDR["oldadditionalinfo"].ToString() != "")
                    {
                        if (OptionDR["additionalinfo"] != DBNull.Value && OptionDR["additionalinfo"].ToString() != "" && OptionDR["additionalinfo"].ToString()!= OptionDR["oldadditionalinfo"].ToString())
                        {
                            tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["oldadditionalinfo"].ToString()) + "</span><br>");
                        }
                        //else if ((OptionDR["productdescription"] != DBNull.Value && OptionDR["productdescription"].ToString() != ""))
                        //{
                        //    tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["productdescription"].ToString()) + "</span><br>");
                        //}
                        //tempdesc.Append("<span style='text-decoration: line-through;'>" + Common.ReplaceCRLFByLineBreak(OptionDR["oldadditionalinfo"].ToString()) + "</span><br>");
                    }

                    if (OptionDR["productdescription"] != DBNull.Value && OptionDR["productdescription"].ToString() != "")
                    {
                        tempdesc.Append(Common.ReplaceCRLFByLineBreak(OptionDR["productdescription"].ToString()));
                    }
                    if (OptionDR["additionalinfo"] != DBNull.Value && OptionDR["additionalinfo"].ToString() != "")
                    {
                        tempdesc.Append("<br>" + Common.ReplaceCRLFByLineBreak(OptionDR["additionalinfo"].ToString()));
                    }

                    if (OptionDR["oldextradesc"] != DBNull.Value && OptionDR["oldextradesc"].ToString() != "")
                    {
                        tempdesc.Append("<span style='text-decoration: line-through;'><br><br><b>Extra Description:</b>" + Common.ReplaceCRLFByLineBreak(OptionDR["oldextradesc"].ToString()) + "</span>");
                        tempdesc.Append("<br><b>Extra Description:</b>" + OptionDR["ENTERDESC"].ToString());
                    }
                    else if (OptionDR["ENTERDESC"] != DBNull.Value && OptionDR["ENTERDESC"].ToString() != "")
                    {
                        tempdesc.Append("<br><br><b>Extra Description:</b>" + OptionDR["ENTERDESC"].ToString());
                    }


                        if (isstudiomproduct)
                        {
                            if (studiom != "")
                            {
                                tempdesc.Append("<br><br>" + studiom);
                            }
                        }
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


                        if (OptionDR["displayAt"] != null && OptionDR["displayAt"].ToString().Trim() != "")
                        {
                            if (temp2 == "")
                            {
                                temp2 = "<b>[" + OptionDR["displayAt"].ToString() + "]</b>";
                            }
                            else
                            {
                                temp2 = temp2 + ", <b>[" + OptionDR["displayAt"].ToString() + "]</b>";
                            }
                        }

                        if (temp2 != "")
                        {
                            tempdesc.Append("<br><br>" + temp2);
                        }

                        if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "")
                        {
                            tempimage = "<br><img src='" + Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg") + "' width='" + w2.ToString() + "' height='" + h2.ToString() + "'/>";
                        }


                        
                        if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "DELETED")
                        {

                            sb.Append("<tr><td valign='top' class='tdnumber'>" + RowIdentifier.ToString() + ".</td>");
                            if (includeProductNameAndCode == "True")
                                sb.Append("<td valign='top' style='word-wrap:break-word; page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</span></span></div></td>");
                            var colspanDesc = 1;
                            if (includeProductNameAndCode == "False")
                            {
                                colspanDesc += 1;
                            }
                            if (includeUOMAndQuantity == "False")
                            {
                                colspanDesc += 2;
                            }
                            if ( estimateRevision_revisionTypeId == 7 || estimateRevision_revisionTypeId == 8 || estimateRevision_revisionTypeId == 9 || estimateRevision_revisionTypeId == 10 || estimateRevision_revisionTypeId == 11 || estimateRevision_revisionTypeId == 12 || estimateRevision_revisionTypeId == 21 || estimateRevision_revisionTypeId == 22)
                            {
                                // if it's studio M today's change, event hide price, only show description
                                sb.Append("<td valign='top' colspan='" + colspanDesc + "' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'><b>DELETED PRODUCT</b><br>" + tempdesc.ToString() + "</span></span></div></td>");
                            }
                            else
                            {
                                sb.Append("<td valign='top' colspan='" + colspanDesc + "' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'><b>DELETED PRODUCT</b><br>" + tempdesc.ToString() + "</span></span></div></td>");
                            }
                            if (includeUOMAndQuantity == "True")
                            {
                                // SS - DELETED items we don't show QTY and UOM but leave empty on those TD's to be consistant aligned properly along with active items.
                                sb.Append("<td></td><td></td>");
                            }

                            if (promotionProduct)
                            {
                                sb.Append("<td valign='top'><span style='color:#666666;'>"+ OptionDR["promotionitemdisplaycode"].ToString().ToUpper()+@"<img src='" + Server.MapPath(@"~/images/star.gif") + "'/></span><br></td></tr>");
                            }
                            else
                            {
                                if (printversion == "LUMPSUM")
                                {
                                    sb.Append("<td valign='top'><span style='color:#666666;'><span style='color:black;'>&nbsp;</span></span><br></td></tr>");
                                }
                                else
                                {
                                    sb.Append("<td valign='top'><span style='color:#666666;'><span style='color:black;'>" + OptionDR["printprice"].ToString() + "</span></span></td></tr>");
                                }
                                try
                                {
                                    decimal itemPrice = decimal.Parse(OptionDR["totalprice"].ToString().Replace("$", ""));
variationTotal += itemPrice;
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                        else
                        {

                            sb.Append("<tr><td valign='top' class='tdnumber'>" + RowIdentifier.ToString() + ".</td>");
                            if (includeProductNameAndCode == "True")
                                sb.Append("<td valign='top' style='word-wrap:break-word; page-break-inside:avoid;'><div class='tdinner'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</div></td>");
                            if (estimateRevision_state.ToLower() != "qld" && estimateRevision_state.ToLower()!="nsw")
                            {
                                var colspanDesc = 1;
                                if (includeProductNameAndCode == "False")
                                    colspanDesc += 1;
                                if (includeUOMAndQuantity == "False")
                                    colspanDesc += 2;
                                if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "CHANGED")
                                    sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'><b>AMENDED PRODUCT</b><br>" + tempdesc.ToString() + "</div></td>");
                                else
                                    sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                                if (includeUOMAndQuantity == "True")
                                {
                                    sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["uom"].ToString() + "</div></td>");
                                    sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["quantity"].ToString() + "</div></td>");
                                }
                            }
                            else
                            {
                                if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "CHANGED")
                                {                                    
                                    if (includeProductNameAndCode == "False" || estimateRevision_revisionTypeId == 7 || estimateRevision_revisionTypeId == 8 || estimateRevision_revisionTypeId == 9 || estimateRevision_revisionTypeId == 10 || estimateRevision_revisionTypeId == 11 || estimateRevision_revisionTypeId == 12 || estimateRevision_revisionTypeId == 21 || estimateRevision_revisionTypeId == 22)
                                    {
                                        sb.Append("<td valign='top' colspan='4' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'><b>AMENDED PRODUCT</b><br>" + tempdesc.ToString() + "</span></span></div></td>");
                                    }
                                    else
                                    {
                                        sb.Append("<td valign='top' colspan='3' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;'><span style='color:black;'><b>AMENDED PRODUCT</b><br>" + tempdesc.ToString() + "</span></span></div></td>");
                                    }

                                }
                                else
                                {
                                    if (includeProductNameAndCode == "False" || estimateRevision_revisionTypeId == 7 || estimateRevision_revisionTypeId == 8 || estimateRevision_revisionTypeId == 9 || estimateRevision_revisionTypeId == 10 || estimateRevision_revisionTypeId == 11 || estimateRevision_revisionTypeId == 12 || estimateRevision_revisionTypeId == 21 || estimateRevision_revisionTypeId == 22)
                                    {
                                        sb.Append("<td valign='top' colspan='4' style='page-break-inside:avoid;'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                                    }
                                    else
                                    {
                                        sb.Append("<td valign='top' colspan='3' style='page-break-inside:avoid;'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                                    }
                                }
                            }


                            if (int.Parse(OptionDR["fkidstandardinclusions"].ToString()) > 0)
                            {

                                sb.Append("<td valign='top'><div class='tdinner'>INCLUDED</div></td></tr>");

                            }
                            else if (promotionProduct)
                            {
                                sb.Append("<td valign='top'><div class='tdinner'>"+ OptionDR["promotionitemdisplaycode"].ToString().ToUpper() + @"<img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                            }
                            else
                            {
                                decimal tempdecimal;
variationTotal += decimal.Parse(OptionDR["totalprice"].ToString().Replace("$", ""));

                            if (estimateRevision_state.ToLower() == "qld" && (estimateRevision_revisionTypeId == 7 || estimateRevision_revisionTypeId == 8 || estimateRevision_revisionTypeId == 9 || estimateRevision_revisionTypeId == 10 || estimateRevision_revisionTypeId == 11 || estimateRevision_revisionTypeId == 12 || estimateRevision_revisionTypeId == 21 || estimateRevision_revisionTypeId == 22))
                                {
                                }
                                else
                                {
                                    try
                                    {
                                        tempdecimal = decimal.Parse(OptionDR["printprice"].ToString().Replace("$", ""));

                                        if (printversion == "LUMPSUM")
                                        {
                                            sb.Append("<td valign='top'><div class='tdinner'>&nbsp;</div></td></tr>");
                                        }
                                        else
                                        {
                                            sb.Append("<td valign='top'><div class='tdinner'>" + String.Format("{0:C}", tempdecimal) + "</div></td></tr>");
                                        }

                                        //variationTotal += tempdecimal;
                                    }
                                    catch (Exception)
                                    {
                                        if (printversion == "LUMPSUM")
                                        {
                                            sb.Append("<td valign='top'><div class='tdinner'>&nbsp;</div></td></tr>");
                                        }
                                        else
                                        {
                                            sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["printprice"].ToString() + "</div></td></tr>");
                                        }
                                    }
                                }
                            }
                        }
                    //}
        }

        public void SavePDF(Doc theDoc)
        {
            Random R = new Random();
            byte[] theData = theDoc.GetData();

            //theDoc.Save("c:\\temp\\docusignvariation.pdf");

            CommonFunction cf = new CommonFunction();
            if (docusignintegration == "0")
            {
                Response.Clear();
                Response.AddHeader("content-type", "application/pdf");
                Response.AddHeader("content-disposition", "inline; filename='Brochure" + "_" + R.Next(1000).ToString() + ".pdf'");

                if (Context.Response.IsClientConnected)
                {
                    Session.Abandon();
                    Context.Response.OutputStream.Write(theData, 0, theData.Length);
                    Context.Response.Flush();
                }

                theDoc.Clear();
            }
            else
            {
                string returnurl = cf.ProcessDocuSign(action,
                    estimateRevisionId.ToString(),
                    estimateRevision_internalVersion,
                    printversion,
                    userid,
                    theData,
                    theDoc.PageNumber,
                    recipients,
                    recipientsemail,
                    routingorder,
                    documenttype,
                    recipientsortorder,
                    emailsubject,
                    emailbody.Replace(Environment.NewLine,"<br>"),
                    methods, envelopeid,
                    estimateRevision_revisionNumber,
                    EstimateID.ToString(),
                    BCContractnumber
                    );
                if (returnurl != null && returnurl != "")
                {
                    string newurl = "<div sytle='text-align: center;margin-top: 200px;'>Document is submitted to Docusign. <a href='" + ConfigurationManager.AppSettings["DocuSign_InofficeSigningURL"].ToString() + "'>Click here</a> to sign the document.</div>";
                    //Response.Redirect(returnurl);
                    Response.Write(newurl);
                }
            }

        }

        public string getTitleFromSalesType(string saletype)
        {
            string result = "";
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_GetEstimateTitleFromSaleType");
                sqlCmd.Parameters["@saletype"].Value = saletype;
                sqlCmd.Parameters["@state"].Value = Session["OriginalLogOnState"].ToString();
                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null)
                {
                    result = ds.Tables[0].Rows[0]["text1"].ToString();
                }
                return result;
            }
            catch
            {
                return result;
            }
        }

        public string getSpecification(string estimaterevisonid)
        {
            string result = "";
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetSpecificationByEstimate");
                sqlCmd.Parameters["@estimaterevisonid"].Value = Common.ConvertStringToIntIfFailToZero(estimaterevisonid);
                sqlCmd.Parameters["@printversion"].Value = printversion;

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = ds.Tables[0].Rows[0]["SpecificationFileName"].ToString();
                }
                return result;
            }
            catch
            {
                return result;
            }
        }

        public DataSet getLotAddressForPackage(string estimateid)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_PKG_Package_GetPackageLotAddress");
                sqlCmd.Parameters["@estimateid"].Value = Common.ConvertStringToIntIfFailToZero(estimateid);

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public string getPriceListDate()
        {
            string result = "";

            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetPriceListDateByEstimateID");
            Cmd.Parameters["@estimateid"].Value = EstimateID;
            DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

            if (ds.Tables[0].Rows.Count == 1 && ds.Tables[0].Rows[0]["effectivedate"] != null && ds.Tables[0].Rows[0]["effectivedate"].ToString() != "")
            {
                result = string.Format("{0:d}", DateTime.Parse(ds.Tables[0].Rows[0]["effectivedate"].ToString()));
            }

            return result;
        }

        public DataSet GetPackageHeaderInformation(string packageid)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_PKG_Package_getAllPackageItemsByID");
                sqlCmd.Parameters["@fkidpackage"].Value = Common.ConvertStringToIntIfFailToZero(packageid);

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Added 26/09/2011
        /// </summary>
        /// <param name="estimateRevisionId"></param>
        /// <returns></returns>
        public DataSet GetEstimateHeaderInformation(string estimateRevisionId)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateHeader");
                sqlCmd.Parameters["@revisionId"].Value = Common.ConvertStringToIntIfFailToZero(estimateRevisionId);

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public DataSet GetPrintPDFTemplate(int estimateRevisionId)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetPrintPDFTemplate");
                sqlCmd.Parameters["@revisionId"].Value = estimateRevisionId;
                sqlCmd.Parameters["@printtype"].Value = estimateRevision_internalVersion;

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                return ds;

            }
            catch (Exception)
            {
                //return "No template found!";
                return null;
            }
        }

        public string getpackageIDfromestimate(string estimateid)
        {
            string result = "0";
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_getPackageIDByEstimateID");
                sqlCmd.Parameters["@estimateid"].Value = Common.ConvertStringToIntIfFailToZero(estimateid);

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    result = ds.Tables[0].Rows[0]["fkidpackage"].ToString();
                }
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        public void GetEstimateRevisionHeaderDetails(int estimateRevisionId)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateHeaderForPrinting");
                sqlCmd.Parameters["@revisionId"].Value = estimateRevisionId;
                sqlCmd.Parameters["@printtype"].Value = estimateRevision_internalVersion;
                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    estimateRevision_estimateId = Convert.ToInt32(ds.Tables[0].Rows[0]["EstimateId"]);
                    estimateRevision_accountId = ds.Tables[0].Rows[0]["AccountId"].ToString();
                    estimateRevision_opportunityId = ds.Tables[0].Rows[0]["OpportunityId"].ToString();
                    estimateRevision_revisionNumber = ds.Tables[0].Rows[0]["RevisionNumber"].ToString();
                    estimateRevision_homeName = ds.Tables[0].Rows[0]["HomeName"].ToString();
                    estimateRevision_revisionType = ds.Tables[0].Rows[0]["RevisionType"].ToString();
                    estimateRevision_revisionTypeBrief = ds.Tables[0].Rows[0]["briefRevisonType"].ToString();
                    estimateRevision_revisionOwner = ds.Tables[0].Rows[0]["OwnerName"].ToString();
                    if (ds.Tables[0].Rows[0]["EffectiveDate"] != null && ds.Tables[0].Rows[0]["EffectiveDate"].ToString() != "")
                    {
                        estimateRevision_effectiveDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["EffectiveDate"]).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        estimateRevision_effectiveDate = "&nbsp;";
                    }
                    estimateRevision_landPrice = Convert.ToDouble(ds.Tables[0].Rows[0]["LandPrice"]);
                    estimateRevision_homePrice = Convert.ToDouble(ds.Tables[0].Rows[0]["HomePrice"]);
                    estimateRevision_siteworkPrice = Convert.ToDouble(ds.Tables[0].Rows[0]["SiteWorkValue"]);
                    estimateRevision_upgradePrice = Convert.ToDouble(ds.Tables[0].Rows[0]["UpgradeValue"]);
                    estimateRevision_promotionValue = Convert.ToDouble(ds.Tables[0].Rows[0]["PromotionValue"]);
                    estimateRevision_surcharge = Convert.ToDouble(ds.Tables[0].Rows[0]["Surcharge"]);
                    estimateRevision_state = ds.Tables[0].Rows[0]["State"].ToString();
                    estimateRevision_regionId = Convert.ToInt32(ds.Tables[0].Rows[0]["RegionID"]);
                    estimateRevision_statusId = Convert.ToInt32(ds.Tables[0].Rows[0]["StatusId"]);
                    estimateRevision_documenttype = ds.Tables[0].Rows[0]["documenttype"].ToString();
                    estimateRevision_revisionTypeId = Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionTypeId"]);
                    houseAndLandPkgContract = ds.Tables[0].Rows[0]["PackageId"] == DBNull.Value ? false : true;
                }
            }
            catch (Exception)
            {

            }
        }


        private string GetEstimateLotAddress(int estimateNumber)
        {
            string lotAddress = string.Empty;

            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateLotAddress");
                sqlCmd.Parameters["@estimateid"].Value = estimateNumber;

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    if (ds.Tables[0].Rows[0]["LotAddress"] != DBNull.Value)
                        lotAddress = ds.Tables[0].Rows[0]["LotAddress"].ToString();
                }
            }
            catch (Exception)
            {

            }

            return lotAddress;
        }

        private string ExtractStudioMAnswer(string studiomXML)
        {
            string result = "";
            try
            {
                XDocument doc = new XDocument();
                doc = XDocument.Parse(studiomXML);
                string selectedsupplierid, selectedsuppliername, selectedquestionid, selectedquestiontext;
                result = "<table cellspacing='0' cellpadding='0'><tr style='padding-top:0px'><td colspan='3'><span style='font-size:15px;'><b>Studio M:</b></span></td></tr>";
                IEnumerable<XElement> el = (from p in doc.Descendants("Brand") select p);
                string temp = ConfigurationManager.AppSettings["STUDIOM_QUESTION_TYPE"].ToString();
                string[] replacestring = temp.Split(',');
                foreach (XElement sup in el)
                {
                    selectedsupplierid = sup.Attribute("id").Value;
                    selectedsuppliername = sup.Attribute("name").Value;
                    result = result + "<tr style='padding-top:0px'><td colspan='3'><span style='font-size:15px;'><b>Brand: </b>" + selectedsuppliername + "</span></td></tr>";
                    IEnumerable<XElement> question = (from q in doc.Descendants("Question") where (string)q.Parent.Parent.Attribute("id") == selectedsupplierid select q);
                    foreach (XElement qu in question)
                    {

                        selectedquestionid = qu.Attribute("id").Value;
                        selectedquestiontext = qu.Attribute("text").Value;
                        //remove question type from question text
                        foreach (string s in replacestring)
                        {
                            selectedquestiontext = selectedquestiontext.Replace(s, "");
                        }
                        //selectedquestiontext = selectedquestiontext.Replace("(Multiple Selection)", "").Replace("(Single Selection)", "").Replace("(Free Text)", "").Replace("(Decimal)", "").Replace("(Integer)", "");

                        IEnumerable<XElement> answer = (from aw in doc.Descendants("Answer")
                                                        where (string)aw.Parent.Parent.Attribute("id") == selectedquestionid &&
                                                        (string)aw.Parent.Parent.Parent.Parent.Attribute("id") == selectedsupplierid
                                                        select aw);
                        if (answer.Count() > 0 && answer.ElementAt(0).Attribute("text").Value != "") //only print questions that've been answered                                                               
                        {
                            result = result + "<tr style='padding:0px; height:16px;'><td><span style='font-size:15px;'><b>Q: </b>" + selectedquestiontext + "</span></td></tr>";

                            //int index = 0;

                            foreach (XElement aw in answer)
                            {
                                //if (index == 0)
                                //{
                                result = result + "<tr style='padding:0px; height:16px;'><td><span style='font-size:15px;'><b>A: </b>" + aw.Attribute("text").Value + "</span></td></tr>";
                                //}
                                //else
                                //{
                                //    result = result + @"/" + aw.Attribute("text").Value;
                                //}
                                //index = index + 1;
                            }
                            //result = result + "</td></tr>";
                        }
                    }
                }
                if (result == "<table cellspacing='0' cellpadding='0'><tr style='padding-top:0px'><td colspan='3'><span style='font-size:15px;'><b>Studio M:</b></span></td></tr>")
                {
                    result = "";
                }
                else
                {
                    result = result + "</table>";
                }
            }
            catch (Exception)
            {
                result = "";
            }
            return result;
        }

        public DataSet GetAreaSurcharge(int estimateRevisionId)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetAreaSurcharge");
                sqlCmd.Parameters["@estimateRevisionId"].Value = estimateRevisionId;

                DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}