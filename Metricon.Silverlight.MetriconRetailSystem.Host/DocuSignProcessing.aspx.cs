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
using Metricon.Silverlight.MetriconRetailSystem.Host.MetriconSalesWebService;
using Metricon.Silverlight.MetriconRetailSystem.Host.DocuSignWebService;


namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class DocuSignProcessing : System.Web.UI.Page
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
        private DocuSignWebService.DocuSignWebService docusrv;

        private int estimateRevisionId;
        private int estimateRevision_estimateId;
        private string estimateRevision_accountId;
        private string estimateRevision_opportunityId;
        private string estimateRevision_revisionNumber;
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
        private double estimateRevision_siteworksurcharge;
        private double estimateRevision_nonsiteworksurcharge;
        private double estimateRevision_provisionalsums;
        private string estimateRevision_state;
        private int estimateRevision_regionId;
        private int estimateRevision_statusId;
        private bool merge_areaSurcharge;
        private string estimateRevision_internalVersion;
        private string ESTIMATE_INTERNALCOPY_STAMP = "Internal Copy";
        private string printversion = "FULLSUMMARY";
        private string BC_Company = "METHOMES";
        private string includestd = "0";
        private string primarycontact = "";
        private string primarycontactemail = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // ===== Added to retrieve Estimate Header from Estimate Revision =====
            estimateRevision_internalVersion = "";

            if (Request.QueryString["type"] != null)
                estimateRevision_internalVersion = Request.QueryString["type"].ToString().ToUpper();

            if (Request.QueryString["version"] != null && Request.QueryString["version"].ToString() != "")
                printversion = Request.QueryString["version"].ToString().ToUpper();

            if (Request.QueryString["includestd"] != null)
                includestd = Request.QueryString["includestd"].ToString();

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
                    }
                    else
                    {
                        PDFTemplate = "No valid template available.";
                    }

                    // ===========

                    // ===== Added to call Webservices without Parent Page =====
                    MS = new MetriconSales();
                    MS.Url = ConfigurationManager.AppSettings["MetriconSalesWebService"].ToString();
                    MS.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    MS.Reconnect(BC_Company);
                    // ===========

                    string updatedpdfheader = PDFHeader(EstimateID, printCustomerDetails, PDFTemplate);
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
            string EstimateExpiryDate = "&nbsp;";
            string BCCustomerid = "";
            string EstimateIDInHeader = "";
            string Baseprice = "";
            string siteworks = "";
            string CartValue = "";
            string depositDate = "";
            string houseAndLandPkg = "";
            string printheadervar = "";

            string HouseName = "";
            string landprice = "";
            string homeprice = "";
            string surcharge = "";

            if (houseAndLandPkgContract)
            {
                string packageid = getpackageIDfromestimate(EstimateID.ToString());
                dds = GetPackageHeaderInformation(packageid);
            }


            printheadervar = GetHeaderInformation(EstimateID);

            string[] headervar = printheadervar.Split('|');
            string SaleType = headervar[0].ToString();

            if (dds != null && dds.Tables[0].Rows.Count > 0)
            {
                //surcharge = String.Format("{0:C}", estimateRevision_surcharge);

                packagecreateddate = dds.Tables[0].Rows[0]["packagecreateddate"].ToString();
            }
            else
            {
                HouseName = headervar[6].ToString();
            }

            if (merge == 1)
            {
                DataSet surchargeDS = GetAreaSurcharge(estimateRevisionId);
                estimateRevision_surcharge = Convert.ToDouble(surchargeDS.Tables[0].Rows[0]["surcharge"]);
            }
            // The followings are only printed if required, e.g. not a Colour Consultant.
            if (printCustomerDetails)
            {
                CustomerName = headervar[1].ToString();
                CustomerAddress = headervar[2].ToString();
                CustomerName2 = headervar[11].ToString();
                LotAddress = headervar[3].ToString();
                EstimateCreatedDate = headervar[4].ToString();
                EstimateActualDate = headervar[15].ToString();
                EstimateExpiryDate = headervar[16].ToString();
                BCCustomerid = headervar[12].ToString();
                BCContractnumber = headervar[13].ToString();
                EstimateIDInHeader = EstimateID.ToString();
                Baseprice = headervar[7].ToString();
                siteworks = headervar[14].ToString();
                CartValue = headervar[9].ToString();
                depositDate = headervar[18].ToString();
                if (houseAndLandPkgContract)
                {
                    houseAndLandPkg = "Yes";

                    landprice = String.Format("{0:C}", estimateRevision_landPrice);
                }
                else
                {
                    houseAndLandPkg = "No";
                }
            }
            else
                houseAndLandPkg = "No";

            string Upgrades;
            Upgrades = headervar[8].ToString();

            string provisionalSums = "0";

            if (merge == 1)
            {
                homeprice = String.Format("{0:C}", estimateRevision_homePrice + estimateRevision_surcharge);
                siteworks = String.Format("{0:C}", (estimateRevision_siteworkPrice - estimateRevision_siteworksurcharge));
                Upgrades = String.Format("{0:C}", (estimateRevision_upgradePrice - estimateRevision_nonsiteworksurcharge));
            }
            else
            {
                homeprice = String.Format("{0:C}", estimateRevision_homePrice);
            }

            string Brand = headervar[5].ToString();


            string jobpromotion = headervar[10].ToString();
            consultantName = headervar[20];
            string longdesc = headervar[21];

            PDFTemplate = PDFTemplate.Replace("$headerimagetoken$", Server.MapPath(@"~/images/" + headerimagename));
            PDFTemplate = PDFTemplate.Replace("$imagespacertoken$", Server.MapPath(@"~/images/spacer.gif"));
            PDFTemplate = PDFTemplate.Replace("$houseandlandpackagetoken$", houseAndLandPkg);
            PDFTemplate = PDFTemplate.Replace("$contactlisttoken$", CustomerName);
            PDFTemplate = PDFTemplate.Replace("$customernumbertoken$", BCCustomerid);
            PDFTemplate = PDFTemplate.Replace("$contactaddresstoken$", CustomerAddress);
            PDFTemplate = PDFTemplate.Replace("$lotaddresstoken$", LotAddress);
            PDFTemplate = PDFTemplate.Replace("$contractnumbertoken$", BCContractnumber);
            PDFTemplate = PDFTemplate.Replace("$estimatenumbertoken$", EstimateIDInHeader);
            PDFTemplate = PDFTemplate.Replace("$estimateactualdatetoken$", EstimateActualDate.ToString());
            PDFTemplate = PDFTemplate.Replace("$estimatecreatedtoken$", EstimateCreatedDate.ToString());
            PDFTemplate = PDFTemplate.Replace("$depositdatetoken$", depositDate);
            PDFTemplate = PDFTemplate.Replace("$priceeffectivedatetoken$", pricedate);
            PDFTemplate = PDFTemplate.Replace("$createdbytoken$", estimateRevision_revisionOwner);
            //PDFTemplate = PDFTemplate.Replace("$expirydatetoken$", EstimateExpiryDate.ToString());
            //PDFTemplate = PDFTemplate.Replace("$brandimagetoken$", Server.MapPath(@"~/images/" + longdesc + "-logo.gif"));
            PDFTemplate = PDFTemplate.Replace("$brandimagetoken$", homebrandname);
            PDFTemplate = PDFTemplate.Replace("$revisontypetoken$", estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")");

            string tempfacade = "";
            if (homebrandname.ToLower() == "regional victoria")
            {
                tempfacade = GetFacadeUpgrade();
                if (tempfacade != "")
                {
                    HouseName = HouseName + "<br>Facade Upgrade: " + tempfacade;
                }
            }

            PDFTemplate = PDFTemplate.Replace("$housenametoken$", HouseName);
            PDFTemplate = PDFTemplate.Replace("$salesconsultanttoken$", consultantName);
            PDFTemplate = PDFTemplate.Replace("$displaylocationtoken$", displaycentre);
            PDFTemplate = PDFTemplate.Replace("$priceregiontoken$", region);
            //PDFTemplate = PDFTemplate.Replace("$homepricetoken$", Baseprice);
            if (printversion == "FULLSUMMARY")
            {
                PDFTemplate = PDFTemplate.Replace("$homepricetoken$", homeprice);
                PDFTemplate = PDFTemplate.Replace("$siteworkpricetoken$", siteworks);
                PDFTemplate = PDFTemplate.Replace("$upgradepricetoken$", Upgrades);

                if (estimateRevision_state.ToUpper() == "NSW")
                {
                    string temp = @"<tr><td valign='top' class='typetd'><h2>Provisional Sums</h2></td><td valign='top' class='valuetd'><p>" + String.Format("{0:C}", (estimateRevision_provisionalsums)) + @"</p></td></tr>";
                    PDFTemplate = PDFTemplate.Replace("$provisionalsumtoken$", temp);
                }
                else
                {
                    PDFTemplate = PDFTemplate.Replace("$provisionalsumtoken$", "");
                }
            }
            else
            {
                PDFTemplate = PDFTemplate.Replace("$homepricetoken$", "");
                PDFTemplate = PDFTemplate.Replace("$siteworkpricetoken$", "");
                PDFTemplate = PDFTemplate.Replace("$upgradepricetoken$", "");
                PDFTemplate = PDFTemplate.Replace("$provisionalsumtoken$", "");

                PDFTemplate = PDFTemplate.Replace("<h2>Home</h2>", "");
                PDFTemplate = PDFTemplate.Replace("<h2>Site Works</h2>", "");
                PDFTemplate = PDFTemplate.Replace("<h2>Upgrades</h2>", "");


            }
            PDFTemplate = PDFTemplate.Replace("$totalpricetoken$", CartValue);
            PDFTemplate = PDFTemplate.Replace("$totallineimagetoken$", Server.MapPath(@"~/images/total-line.gif"));


            if (estimateRevision_state.ToLower() == "qld")
            {
                PDFTemplate = PDFTemplate.Replace("UOM", "");
                PDFTemplate = PDFTemplate.Replace("QTY", "");
            }

            return PDFTemplate;
        }

        private string GetFacadeUpgrade()
        {
            string result = "";
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetFacadeUpgrade");
            Cmd.Parameters["@estimate_id"].Value = EstimateID;
            DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["facade"].ToString();
            }
            return result;
        }

        public string GetHeaderInformation(int EstimateID)
        {
            string brand = ""; string homename = ""; int HOUSE_CODE = 0; int HOUSE_REGION;
            string Upgrades; string baseprice; string cartvalue; string jobpromotion; string siteworksvalue;
            string CustomerName = ""; string CustomerName2 = ""; string CustomerAddress = "";
            string brandlongdesc = "";
            string EstimateCreatedDate = "";
            string EstimateActualDate = "";
            string EstimateExpiryDate = "&nbsp;";
            string promotiontype = "";
            string salesconsultant = "";
            double optionsSubTotal2;
            double optionsTotal;
            double housePrice;
            double packagePrice = 0.0;


            string BCCustomerid = ""; string BCContractnumber = ""; string LotAddress = "";
            DataSet HPBHDS = Estimate.GetHomePriceBrandAndHomeName(EstimateID);
            if (HPBHDS != null)
                if (HPBHDS.Tables[0].Rows.Count > 0)
                {
                    DataRow HPBHRow = HPBHDS.Tables[0].Rows[0];
                    brand = HPBHRow["homebrandname"].ToString();
                    homebrandname = HPBHRow["homebrandname"].ToString();
                    if (HPBHRow["displaylocation"] != null && HPBHRow["displaylocation"].ToString() != "")
                    {
                        displaycentre = HPBHRow["displaylocation"].ToString();
                    }
                    else
                    {
                        displaycentre = "";
                    }
                    homename = HPBHRow["homename"].ToString();
                    brandlongdesc = HPBHRow["longdescription"].ToString();
                    region = HPBHRow["regionname"].ToString();
                    HOUSE_CODE = int.Parse(HPBHRow["homeid"].ToString());
                    HOUSE_REGION = int.Parse(HPBHRow["homeid"].ToString());
                    brandname = HPBHRow["brandname"].ToString().Trim();
                    draft = bool.Parse(HPBHRow["draft"].ToString().Trim());
                    if (HPBHRow["PackagePrice"].ToString() != "")
                        packagePrice = double.Parse(HPBHRow["PackagePrice"].ToString());
                }

            // ===== Modified to get prices from Estimate Revision =====

            //double optionsSubTotal = Estimate.GetUpgradesTotal(EstimateID);

            //housePrice = Estimate.GetHomeBasePrice(EstimateID);

            housePrice = estimateRevision_homePrice;

            if (merge == 1)
            {
                housePrice = housePrice + surcharge;
                packagePrice = packagePrice + surcharge;
            }

            //double PromotionValue = Estimate.GetPromotionTotalForEstimate(EstimateID, HOUSE_CODE);
            //double siteworks = Estimate.GetSiteWorksTotalForEstimate(EstimateID);

            double PromotionValue = estimateRevision_promotionValue;
            double siteworks = estimateRevision_siteworkPrice;

            //Estimate.GetPromotionTotalForEstimate2(EstimateID, HOUSE_CODE, out PromotionValue, out optionsSubTotal2);

            //optionsTotal = optionsSubTotal + optionsSubTotal2;

            optionsTotal = estimateRevision_upgradePrice;

            if (merge == 1)
            {
                optionsTotal = optionsTotal - surcharge;
            }

            if (houseAndLandPkgContract)
            {
                //baseprice = string.Format("{0:C}", packagePrice);
                baseprice = string.Format("{0:C}", housePrice);
                Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice - estimateRevision_nonsiteworksurcharge);
                siteworks = siteworks - estimateRevision_siteworksurcharge;
            }
            else
            {
                baseprice = string.Format("{0:C}", housePrice);
                //Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice - estimateRevision_siteworkPrice);
                Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice);
            }

            //Upgrades = string.Format("{0:C}", optionsTotal - siteworks);

            // ===========

            //if (siteworks == 0.0)
            //    siteworksvalue = "TBA";
            //else

            siteworksvalue = string.Format("{0:C}", siteworks);

            if (houseAndLandPkgContract)
                //cartvalue = string.Format("{0:C}", packagePrice + optionsTotal);
                cartvalue = string.Format("{0:C}", housePrice + optionsTotal + siteworks + estimateRevision_provisionalsums);
            else
                cartvalue = string.Format("{0:C}", housePrice + optionsTotal + siteworks + estimateRevision_provisionalsums);
            jobpromotion = string.Format("{0:C}", PromotionValue);
            string title = "";
            string SaleTypeCode = "Default";
            string depositDate = "&nbsp;";
            try
            {
                //GET THE SALE TYPE FOR THE HEADING

                DataSet ContractDs = new DataSet();
                if (Session["SelectedContract"] != null && Session["SelectedContract"].ToString() != "")
                {
                    ContractDs = MS.GetDepositDetailsForContract(Session["SelectedContract"].ToString());
                }
                else
                    ContractDs = null;

                if (ContractDs != null && ContractDs.Tables.Count > 0 && ContractDs.Tables[0].Rows.Count > 0)
                {
                    SaleTypeCode = ContractDs.Tables[0].Rows[0]["DepositSaleType"].ToString();
                    if (ContractDs.Tables[0].Rows[0]["DepositDate"].ToString() != "")
                        depositDate = DateTime.Parse(ContractDs.Tables[0].Rows[0]["DepositDate"].ToString()).ToString("dd/MM/yyyy");
                }

                title = getTitleFromSalesType(SaleTypeCode);
            }
            catch (NullReferenceException nex1)
            {
                Response.Write(nex1.Message.ToString() + @"<br>" + nex1.Source.ToString() + "<br>session SelectedContract problem.");
            }
            //get the customer Details to be printed on the top of the Printout
            try
            {
                if (Session["AccountID"] != null)
                {
                    if (Session["AccountID"].ToString() != "")
                    {
                        //string customerCode = Session["SelectedCustomerCode"].ToString();
                        string accountid = Session["AccountID"].ToString();
                        string[] dataKeyNames = { "IVTU_SEQNUM" };
                        DataSet ds;

                        ds = MS.GetContactListForCustomerFromCRM(accountid, "", "1");



                        DataView dv = ds.Tables[0].DefaultView;

                        //dv.RowFilter = "IVTU_CONTACTTYPE = 'SQA' OR IVTU_CONTACTTYPE = 'SQD'";
                        //dv.Sort = "IVTU_SEQNUM";

                        DataTable customerDT = dv.ToTable();
                        DataRow customerDR;

                        if (customerDT.Rows.Count > 0)
                        {
                            customerDR = customerDT.Rows[0];
                            //CustomerName = customerDR["ivtu_title"] + " "
                            //                + customerDR["ivtu_firstname"] + " "
                            //                + customerDR["ivtu_surname"];

                            CustomerAddress = customerDR["ivtu_address"]
                                                + "," + customerDR["ivtu_suburb"]
                                                + "," + customerDR["ivtu_state"]
                                                + "," + customerDR["ivtu_zip"];
                        }

                        CustomerName = "";
                        foreach (DataRow dr1 in customerDT.Rows)
                        {
                            if (dr1["isPrimary"].ToString() == "1")
                            {
                                primarycontact = dr1["ivtu_title"] + " " + dr1["ivtu_firstname"] + " " + dr1["ivtu_surname"];
                                primarycontactemail = dr1["EMLA_ADDRESS"].ToString();
                            }

                            if (dr1["ivtu_title"] != null && dr1["ivtu_title"].ToString().ToUpper() != "ZZ")
                            {
                                CustomerName = CustomerName + dr1["ivtu_title"] + " " + dr1["ivtu_firstname"] + " " + dr1["ivtu_surname"] + "<br>";
                            }
                            else
                            {
                                CustomerName = CustomerName + dr1["ivtu_firstname"] + " " + dr1["ivtu_surname"] + "<br>";
                            }
                        }

                        CustomerName = CustomerName.Substring(0, CustomerName.Length - 4);
                    }
                }
            }
            catch (NullReferenceException nex2)
            {
                Response.Write(nex2.Message.ToString() + @"<br>" + nex2.Source.ToString() + "<br>session AccountID problem.");
            }
            //get the estimate created date, bccontractnumber, bccustomernumber 
            DataSet EstimateHeaderDS = Estimate.GetEstimateDetailsForPrint(EstimateID);
            foreach (DataRow EstimateRow in EstimateHeaderDS.Tables[0].Rows)
            {
                //EstimateCreatedDate = DateTime.Parse(EstimateRow["EstimateDate"].ToString()).ToString("dd/MM/yyyy");

                if (EstimateRow["EstimateActualDate"] == DBNull.Value)
                    EstimateActualDate = "";
                else
                    EstimateActualDate = DateTime.Parse(EstimateRow["EstimateActualDate"].ToString()).ToString("dd/MM/yyyy");

                int estimateStatus = Estimate.CheckEstimateStatus(EstimateID);
                // If the deposit is made, and not accepted yet, the Expiry Date is printed.
                if ((EstimateRow["Deposited"] != DBNull.Value && (bool)EstimateRow["Deposited"]))
                {
                    //EstimateExpiryDate = DateTime.Parse(EstimateRow["ExpiryDate"].ToString()).ToString("dd/MM/yyyy");
                    depositDate = DateTime.Parse(EstimateRow["depositdate"].ToString()).ToString("dd/MM/yyyy");
                }


                BCCustomerid = EstimateRow["BcCustomerid"].ToString();
                BCContractnumber = EstimateRow["BCContractnumber"].ToString();
                promotiontype = EstimateRow["promotiontypeid"].ToString();
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
                if (dstemp.Tables[0].Rows[0]["expirydate"] != null && dstemp.Tables[0].Rows[0]["expirydate"].ToString() != "")
                {
                    EstimateExpiryDate = DateTime.Parse(dstemp.Tables[0].Rows[0]["expirydate"].ToString()).ToString("dd/MM/yyyy");
                }
                else
                {
                    EstimateExpiryDate = "&nbsp;";
                }
            }
            else
            {
                EstimateCreatedDate = "&nbsp;";
                EstimateExpiryDate = "&nbsp;";
            }


            //get the lot address where the customer are building
            DataSet LotAddressds = null;
            try
            {
                string oppid = "";

                if (Session["OpportunityID"] != null)
                    oppid = Session["OpportunityID"].ToString();


                //if (houseAndLandPkgContract)
                //{
                //    LotAddressds = getLotAddressForPackage(EstimateID.ToString());
                //}
                //else
                //{
                LotAddressds = MS.GetSiteDetailsForOpportunityContractInDataSet(oppid);
                //}
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

                if (LotAddressds.Tables[0].Rows[0]["HouseAndLandPkg"].ToString() == "True")
                    houseAndLandPkg = "Yes";

                LotAddress = GetEstimateLotAddress(EstimateID);

            }
            catch (NullReferenceException nex4)
            {
                Response.Write(nex4.Message.ToString() + @"<br>" + nex4.Source.ToString() + "<br>LotAddressds problem.");
            }
            return title + "|" + CustomerName + "|" + CustomerAddress + "|" + LotAddress + "|" + EstimateCreatedDate + "|" +
           brand + "|" + homename + "|" + baseprice + "|" + Upgrades + "|" +
           cartvalue + "|" + jobpromotion + "|" + CustomerName2 + "|" + BCCustomerid + "|" + BCContractnumber + "|" + siteworksvalue + "|" +
           EstimateActualDate + "|" + EstimateExpiryDate + "|" + promotiontype + "|" + depositDate + "|" + houseAndLandPkg + "|" + salesconsultant + "|" + brandlongdesc;
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
            string tempimage = "";

            // Actual body contect starts from here.
            int RowIdentifier = 0;

            DBConnection DB = new DBConnection();
            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateDetailsForPrinting");
            EstimateViewCmd.Parameters["@revisionId"].Value = estimateRevisionId;
            EstimateViewCmd.Parameters["@printtype"].Value = estimateRevision_internalVersion;
            EstimateViewCmd.Parameters["@includestd"].Value = includestd;

            DataSet OptionDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);
            ArrayList AreaList = new ArrayList();


            foreach (DataRow OptionDR in OptionDS.Tables[0].Rows)
            {
                tempdesc.Clear();
                tempimage = "";
                if (merge != 1 || (OptionDR["AREANAME"].ToString() != "Area Surcharge" && merge == 1))
                {
                    if (!AreaList.Contains(OptionDR["AREANAME"].ToString()))
                    {
                        AreaList.Add(OptionDR["AREANAME"].ToString());
                        if (sb.ToString() != "")
                        {
                            sb.Append("</table>");
                        }

                        sb.Append("<table width='850' border='0' cellspacing='0' cellpadding='0' class='datatbl' style='width:850px;table-layout:fixed;'>");
                        sb.Append("<tr><td style='width:26px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + @"' width='26' height='1' alt='' /></td>");
                        sb.Append("<td style='width:158px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='158' height='1' alt='' /></td>");
                        sb.Append("<td style='width:466px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='466' height='1' alt='' /></td>");
                        sb.Append("<td style='width:50px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='50' height='1' alt='' /></td>");
                        sb.Append("<td style='width:50px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='50' height='1' alt='' /></td>");
                        sb.Append("<td style='width:100px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='100' height='1' alt='' /></td>");
                        sb.Append("</tr><tr class='rowheadingtr'><td colspan='6' class='rowheadingtd'>" + OptionDR["AREANAME"].ToString());
                        sb.Append("</td></tr><tr><td colspan='6'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='1' height='10' alt='' /></td></tr>");

                    }
                    else
                    {   // 1px row border seperator

                        sb.Append("<tr class='trline'><td colspan='6' class='trline'><img src='" + Server.MapPath(@"~/images/line.gif") + "' width='810' height='17' alt='' /></td></tr>");
                    }
                    RowIdentifier = RowIdentifier + 1;
                    //check if the product is promotionproduct
                    bool promotionProduct;
                    if (OptionDR["promotionproduct"].ToString() == "0")
                        promotionProduct = false;
                    else
                        promotionProduct = true;

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
                    string RowData = "";

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
                            h2 = (img.Height * originw) / img.Width;
                        }
                        else // portrit image
                        {
                            w2 = (img.Width * originh) / img.Height;
                            h2 = originh;
                        }
                    }



                    if (promotionProduct)
                    {
                        tempdesc.Append(Common.ReplaceCRLFByLineBreak(OptionDR["PRODUCTDESCRIPTION"].ToString()));

                        if ((OptionDR["ENTERDESC"].ToString().CompareTo("") != 0) && (OptionDR["ENTERDESC"].ToString().Trim().CompareTo("N/A") != 0))
                            tempdesc.Append("<br><br> <b>Extra Description:</b> " + OptionDR["ENTERDESC"].ToString());
                        if (estimateRevision_internalVersion == "INTERNAL" && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                            tempdesc.Append("<br><br> <b>Internal Description:</b> " + OptionDR["INTERNALDESC"].ToString());

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
                            sb.Append("<td valign='top' style='word-wrap:break-word;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</span></span></div></td>");

                            if (estimateRevision_state.ToLower() != "qld")
                            {
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["uom"].ToString() + "</span></div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["quantity"].ToString() + "</span></div></td>");
                            }
                            else
                            {
                                sb.Append("<td valign='top' colspan='3'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                            }

                            if (printversion == "LUMPSUM")
                            {
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>&nbsp;</span></span><br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                            }
                            else
                            {
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["printprice"].ToString() + "</span></span><br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                            }

                        }
                        else
                        {

                            sb.Append("<tr><td valign='top' class='tdnumber'>" + RowIdentifier.ToString() + ".</td>");
                            sb.Append("<td valign='top' style='word-wrap:break-word;'><div class='tdinner'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</div></td>");

                            if (estimateRevision_state.ToLower() != "qld")
                            {
                                sb.Append("<td valign='top'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["uom"].ToString() + "</div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["quantity"].ToString() + "</div></td>");
                            }
                            else
                            {
                                sb.Append("<td valign='top' colspan='3'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                            }
                            sb.Append("<td valign='top'><div class='tdinner'>INCLUDED<br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                        }

                    }
                    else
                    {

                        tempdesc.Append(Common.ReplaceCRLFByLineBreak(OptionDR["PRODUCTDESCRIPTION"].ToString()));
                        if ((OptionDR["ENTERDESC"].ToString().CompareTo("") != 0) && (OptionDR["ENTERDESC"].ToString().Trim().CompareTo("N/A") != 0))
                            tempdesc.Append("<br><br><b>Extra Description:</b> " + OptionDR["ENTERDESC"].ToString());
                        if (estimateRevision_internalVersion == "INTERNAL" && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                            tempdesc.Append("<br><br><b>Internal Description:</b> " + OptionDR["INTERNALDESC"].ToString());


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
                            sb.Append("<td valign='top' style='word-wrap:break-word;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</span></span></div></td>");

                            if (estimateRevision_state.ToLower() != "qld")
                            {
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["uom"].ToString() + "</span></div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["quantity"].ToString() + "</span></div></td>");
                            }
                            else
                            {
                                sb.Append("<td valign='top' colspan='3'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                            }

                            if (printversion == "LUMPSUM")
                            {
                                sb.Append("<td valign='top'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>&nbsp;</span></span><br><img src='" + Server.MapPath(@"~/images/remove.gif") + "'/></td></tr>");
                            }
                            else
                            {
                                sb.Append("<td valign='top'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["printprice"].ToString() + "</span></span><br><img src='" + Server.MapPath(@"~/images/remove.gif") + "'/></td></tr>");
                            }

                        }
                        else
                        {

                            sb.Append("<tr><td valign='top' class='tdnumber'>" + RowIdentifier.ToString() + ".</td>");
                            sb.Append("<td valign='top' style='word-wrap:break-word;'><div class='tdinner'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</div></td>");

                            if (estimateRevision_state.ToLower() != "qld")
                            {
                                sb.Append("<td valign='top'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["uom"].ToString() + "</div></td>");
                                sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["quantity"].ToString() + "</div></td>");
                            }
                            else
                            {
                                sb.Append("<td valign='top' colspan='3'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                            }


                            if (int.Parse(OptionDR["fkidstandardinclusions"].ToString()) > 0)
                            {

                                sb.Append("<td valign='top'><div class='tdinner'>INCLUDED</div></td></tr>");

                            }
                            else
                            {
                                decimal tempdecimal;
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


                }
            }
            sb.Append("</table><div class='clear'>&nbsp;</div>");

            // replace the tokens
            string FinalHTML = pdftemplate.Replace("$estimatebodytoken$", sb.ToString());
            int theID = theDoc.AddImageHtml(FinalHTML);

            // Add the last page.
            while (true)
            {
                if (!theDoc.Chainable(theID))
                {

                    // add specification based on state, brand and effectivedate
                    string filename = getSpecification(estimateRevisionId.ToString());
                    if (filename != "")
                    {
                        string pdfPath = ConfigurationManager.AppSettings["PDFPath"].ToString();

                        if (File.Exists(pdfPath + filename))
                        {
                            Doc theDoc2 = new Doc();
                            theDoc2.Read(pdfPath + filename);

                            theDoc.Append(theDoc2);
                        }
                    }

                    theDoc.Page = theDoc.AddPage();
                    if (Session["OriginalLogOnState"].ToString() != "QLD" && Session["OriginalLogOnState"].ToString() != "NSW")
                    {
                        theDoc.Color.Color = System.Drawing.Color.FromArgb(231, 232, 231);
                        theDoc.Rect.Bottom = 2;
                        theDoc.Rect.Left = 15;
                        theDoc.Rect.Width = 500;
                        theDoc.Rect.Height = 15;
                        theDoc.Rect.Move(16, 800);
                        theDoc.FillRect();

                        // Add the Disclaimer/Acknowledgements header.
                        theDoc.Color.Color = System.Drawing.Color.FromArgb(129, 130, 132);
                        theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                        theDoc.TextStyle.Size = 12;
                        theDoc.TextStyle.Bold = false;
                        theDoc.Rect.Pin = 0;
                        theDoc.Rect.Position(30, 800);
                        theDoc.Rect.Width = 200;
                        theDoc.Rect.Height = 15;
                        theDoc.TextStyle.LeftMargin = 5;
                        theDoc.AddText(Common.PRINTPDF_DISCLAIMER_HEADER);
                    }

                    string customerInfo = GetHeaderInformation(estimateid);
                    string[] headervars = customerInfo.Split('|');
                    string tempStr;
                    string temp = headervars[1].Replace("<br>", "|");
                    string[] contacts = temp.Split('|');

                    if (Session["OriginalLogOnState"].ToString() != "QLD" && Session["OriginalLogOnState"].ToString() != "NSW")
                    {
                        tempStr = "<table width=560px cellspadding=0 cellspacing=0>";
                        for (int i = 0; i < contacts.Length; i++)
                        {
                            tempStr = tempStr + "<tr><td width=400px align=left style='font-weight:bold; font-family: arial; font-size:12; '>" + contacts[i] + "</td><td width=380px align=right style='font-weight:bold; font-family: arial; font-size:12;'>Sign Here _______________&nbsp;Date __________</td></tr>";
                        }

                        tempStr = tempStr + "</table>";
                    }
                    else
                    {
                        tempStr = "";
                        for (int i = 0; i < contacts.Length; i++)
                        {
                            if (Session["OriginalLogOnState"].ToString() == "QLD")
                            {
                                tempStr = tempStr + @"
                                              <tr>
                                                <td width=57 valign=top style='width:42.55pt;border:solid windowtext 1.0pt;
                                                padding:0cm 5.4pt 0cm 5.4pt'>
                                                <p class=MsoNormal style='margin-top:8.0pt;margin-right:0cm;margin-bottom:
                                                3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                                font-family:Century Gothic,sans-serif'>Owner:</span></p>
                                                </td>
                                                <td width=321 valign=top style='width:240.95pt;border:solid windowtext 1.0pt;
                                                border-left:none;padding:0cm 5.4pt 0cm 5.4pt'>
                                                <div style='border:none;border-bottom:solid windowtext 1.0pt;padding:0cm 0cm 1.0pt 0cm'>
                                                <p class=MsoNormal style='margin-top:8.0pt;text-autospace:none;border:none;
                                                padding:0cm'><b><span style='font-size:8.0pt;font-family:Century Gothic,sans-serif;
                                                letter-spacing:-.25pt'>&nbsp;</span></b></p>
                                                </div>
                                                </td>
                                                <td width=57 valign=top style='width:42.55pt;border:solid windowtext 1.0pt;
                                                border-left:none;padding:0cm 5.4pt 0cm 5.4pt'>
                                                <p class=MsoNormal style='margin-top:8.0pt;margin-right:0cm;margin-bottom:
                                                3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                                font-family:Century Gothic,sans-serif'>Date:</span></p>
                                                </td>
                                                <td width=161 valign=top style='width:120.45pt;border:solid windowtext 1.0pt;
                                                border-left:none;padding:0cm 5.4pt 0cm 5.4pt'>
                                                <div style='border:none;border-bottom:solid windowtext 1.0pt;padding:0cm 0cm 1.0pt 0cm'>
                                                <p class=MsoNormal style='margin-top:8.0pt;text-autospace:none;border:none;
                                                padding:0cm'><b><span style='font-size:8.0pt;font-family:Arial,sans-serif;
                                                letter-spacing:-.25pt'>&nbsp;</span></b></p>
                                                </div>
                                                </td>
                                             </tr>


                                            <tr style='height:9.25pt'>
                                              <td width=57 valign=top style='width:42.55pt;border:solid windowtext 1.0pt;
                                              border-top:none;padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>&nbsp;</span></p>
                                              </td>
                                              <td width=321 valign=top style='width:240.95pt;border-top:none;border-left:
                                              none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;
                                              padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>" + contacts[i] + @"</span></p>
                                              </td>
                                              <td width=57 valign=top style='width:42.55pt;border-top:none;border-left:
                                              none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;
                                              padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>&nbsp;</span></p>
                                              </td>
                                              <td width=161 valign=top style='width:120.45pt;border-top:none;border-left:
                                              none;border-bottom:solid windowtext 1.0pt;border-right:solid windowtext 1.0pt;
                                              padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>&nbsp;</span></p>
                                              </td>
                                             </tr>";
                            }
                            else if (Session["OriginalLogOnState"].ToString() == "NSW")
                            {
                                tempStr = tempStr + @"
                                              <tr>
                                                <td width=57 valign=top style='width:42.55pt; windowtext 1.0pt;
                                                padding:0cm 5.4pt 0cm 5.4pt'>
                                                <p class=MsoNormal style='margin-top:8.0pt;margin-right:0cm;margin-bottom:
                                                3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                                font-family:Century Gothic,sans-serif'>Owner:</span></p>
                                                </td>
                                                <td width=321 valign=top style='width:240.95pt; windowtext 1.0pt;
                                                border-left:none;padding:0cm 5.4pt 0cm 5.4pt'>
                                                <div style='border:none;border-bottom:solid windowtext 1.0pt;padding:0cm 0cm 1.0pt 0cm'>
                                                <p class=MsoNormal style='margin-top:8.0pt;text-autospace:none;border:none;
                                                padding:0cm'><b><span style='font-size:8.0pt;font-family:Century Gothic,sans-serif;
                                                letter-spacing:-.25pt'>&nbsp;</span></b></p>
                                                </div>
                                                </td>
                                                <td width=57 valign=top style='width:42.55pt; windowtext 1.0pt;
                                                border-left:none;padding:0cm 5.4pt 0cm 5.4pt'>
                                                <p class=MsoNormal style='margin-top:8.0pt;margin-right:0cm;margin-bottom:
                                                3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                                font-family:Century Gothic,sans-serif'>Date:</span></p>
                                                </td>
                                                <td width=161 valign=top style='width:120.45pt; windowtext 1.0pt;
                                                border-left:none;padding:0cm 5.4pt 0cm 5.4pt'>
                                                <div style='border:none;border-bottom:solid windowtext 1.0pt;padding:0cm 0cm 1.0pt 0cm'>
                                                <p class=MsoNormal style='margin-top:8.0pt;text-autospace:none;border:none;
                                                padding:0cm'><b><span style='font-size:8.0pt;font-family:Arial,sans-serif;
                                                letter-spacing:-.25pt'>&nbsp;</span></b></p>
                                                </div>
                                                </td>
                                             </tr>


                                            <tr style='height:9.25pt'>
                                              <td width=57 valign=top style='width:42.55pt; windowtext 1.0pt;
                                              border-top:none;padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>&nbsp;</span></p>
                                              </td>
                                              <td width=321 valign=top style='width:240.95pt;border-top:none;border-left:
                                              none; windowtext 1.0pt; windowtext 1.0pt;
                                              padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>" + contacts[i] + @"</span></p>
                                              </td>
                                              <td width=57 valign=top style='width:42.55pt;border-top:none;border-left:
                                              none; windowtext 1.0pt; windowtext 1.0pt;
                                              padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>&nbsp;</span></p>
                                              </td>
                                              <td width=161 valign=top style='width:120.45pt;border-top:none;border-left:
                                              none; windowtext 1.0pt; windowtext 1.0pt;
                                              padding:0cm 5.4pt 0cm 5.4pt;height:9.25pt'>
                                              <p class=MsoNormal style='margin-top:2.0pt;margin-right:0cm;margin-bottom:
                                              3.0pt;margin-left:0cm;text-autospace:none'><span style='font-size:8.0pt;
                                              font-family:Century Gothic,sans-serif'>&nbsp;</span></p>
                                              </td>
                                             </tr>";
                            }
                        }
                    }

                    // Add the Disclaimer/Acknowledgements body text.
                    theDoc.Color.Color = System.Drawing.Color.Black;
                    theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                    theDoc.TextStyle.Size = Common.PRINTPDF_DISCLAIMER_FONTSIZE;
                    theDoc.TextStyle.Bold = false;
                    theDoc.Rect.Pin = 0;
                    theDoc.Rect.Position(40, 90);
                    theDoc.Rect.Width = 500;
                    theDoc.Rect.Height = 700;
                    theDoc.TextStyle.LeftMargin = 5;

                    string disclaimer = Common.getDisclaimer(estimateRevisionId.ToString(), Session["OriginalLogOnState"].ToString(), estimateRevision_internalVersion).Replace("$Token$", tempStr);
                    disclaimer = disclaimer.Replace("$printdatetoken$", DateTime.Now.ToString("dd/MMM/yyyy"));
                    disclaimer = disclaimer.Replace("$logoimagetoken$", Server.MapPath("~/images/metlog.jpg"));

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

                    //theDoc.AddHtml(disclaimer);
                    if (disclaimer.Trim() != "")
                    {
                        theDoc.AddFont("Century Gothic");
                        theDoc.AddImageHtml(disclaimer);
                    }


                    // Add customer name #1.
                    theDoc.Color.Color = System.Drawing.Color.Black;
                    theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                    theDoc.TextStyle.Size = Common.PRINTPDF_DEFAULT_FONTSIZE;
                    theDoc.TextStyle.Bold = true;

                    int longname = 0;
                    foreach (string s in contacts)
                    {
                        if (s.Length >= 42)
                        {
                            longname = 1;
                            break;
                        }
                    }

                    if (Session["OriginalLogOnState"].ToString() != "QLD" && tempStr.Trim() != "" && Session["OriginalLogOnState"].ToString() != "NSW")
                    {
                        theDoc.Rect.Pin = 0;
                        theDoc.Rect.Position(60, 60);
                        theDoc.Rect.Width = 700;
                        if (longname == 1)
                        {
                            theDoc.Rect.Height = contacts.Length * 60;
                        }
                        else
                        {
                            theDoc.Rect.Height = contacts.Length * 30;
                        }
                        theDoc.TextStyle.LeftMargin = 20;
                        theDoc.AddImageHtml(tempStr);
                    }


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
                else if (draft)
                {
                    DBConnection DBCon = new DBConnection();
                    SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetHomePrintWatermark");
                    DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);
                    if (ds != null)
                    {
                        underReview = ds.Tables[0].Rows[0]["watermark"].ToString();
                        fontsize = Int16.Parse(ds.Tables[0].Rows[0]["font"].ToString());
                        if (underReview == "")
                            underReview = "Draft";
                    }


                    theDoc.FontSize = fontsize;
                    theDoc.Color.String = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR;
                    theDoc.Color.Alpha = Common.ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE;

                    theDoc.Pos.String = "50 250";
                    theDoc.Transform.Reset();
                    theDoc.Transform.Rotate(50, 55, 300);

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

                if (Session["OriginalLogOnState"].ToString() == "QLD")
                {
                    theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Builder Initials:_____________________&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer Initials:________________________<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;E&OE");
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


            return theDoc;

        }

        public void SavePDF(Doc theDoc)
        {
            Random R = new Random();
            byte[] theData = theDoc.GetData();

            try
            {
                //docusrv = new DocuSignWebService.DocuSignWebService();
                ////DocuSignWebService.CreateEnvelopeResponse rs= docusrv.DocuSign_CreateAndSendEnvelope(theData, theDoc.PageNumber, EstimateID.ToString(), estimateRevision_internalVersion, printversion, estimateRevision_revisionNumber, primarycontactemail, primarycontact, ConfigurationManager.AppSettings["DocuSign_Customer_anchorXOffset"].ToString(), ConfigurationManager.AppSettings["DocuSign_Customer_anchorYOffset"].ToString());
                //DocuSignWebService.CreateEnvelopeResponse rs = docusrv.DocuSign_SignInPerson(theData, theDoc.PageNumber, EstimateID.ToString(), estimateRevision_internalVersion, printversion, estimateRevision_revisionNumber, primarycontactemail, primarycontact, ConfigurationManager.AppSettings["DocuSign_Customer_anchorXOffset"].ToString(), ConfigurationManager.AppSettings["DocuSign_Customer_anchorYOffset"].ToString());
                //Response.Write(rs.uri);
                //// log the send date
                //DBConnection DB = new DBConnection();
                //SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_LogDocuSignSendDate");
                //EstimateViewCmd.Parameters["@revisionId"].Value = estimateRevisionId;
                //EstimateViewCmd.Parameters["@estimateid"].Value = EstimateID;

                //DataSet dstemp = DB.SelectSqlStoredProcedure(EstimateViewCmd);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            //Response.Clear();
            //Response.AddHeader("content-type", "application/pdf");
            //Response.AddHeader("content-disposition", "inline; filename='Brochure" + "_" + R.Next(1000).ToString() + ".pdf'");

            //if (Context.Response.IsClientConnected)
            //{
            //    Session.Abandon();
            //    Context.Response.OutputStream.Write(theData, 0, theData.Length);
            //    Context.Response.Flush();
            //}

            //theDoc.Clear();
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
                    estimateRevision_siteworksurcharge = Convert.ToDouble(ds.Tables[0].Rows[0]["siteworksurcharge"]);
                    estimateRevision_nonsiteworksurcharge = Convert.ToDouble(ds.Tables[0].Rows[0]["nonsiteworksurcharge"]);
                    estimateRevision_state = ds.Tables[0].Rows[0]["State"].ToString();
                    estimateRevision_regionId = Convert.ToInt32(ds.Tables[0].Rows[0]["RegionID"]);
                    estimateRevision_statusId = Convert.ToInt32(ds.Tables[0].Rows[0]["StatusId"]);
                    estimateRevision_revisionTypeId = Convert.ToInt32(ds.Tables[0].Rows[0]["RevisionTypeId"]);
                    estimateRevision_provisionalsums = Convert.ToDouble(ds.Tables[0].Rows[0]["provisionalsums"]);
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