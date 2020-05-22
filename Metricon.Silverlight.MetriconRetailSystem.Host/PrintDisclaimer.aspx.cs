using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using WebSupergoo.ABCpdf9;
using Metricon.Silverlight.MetriconRetailSystem.Host.MetriconSalesWebService;
using Metricon.Silverlight.MetriconRetailSystem.Host.Internal;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class PrintDisclaimer : System.Web.UI.Page
    {
        private string BCContractnumber = "";
        private int EstimateID = 0;
        private MetriconSales MS;

        private int estimateRevisionId;
        private int estimateRevision_estimateId;
        private string estimateRevision_revisionNumber;
        private string estimateRevision_disclaimer_version;
        private int estimateRevision_revisionTypeId;
        private string estimateRevision_effectiveDate;
        private string estimateRevision_state;
        private int estimateRevision_statusId;
        private string estimateRevision_internalVersion;
        private string BC_Company = "METHOMES";
        private string extentiondays = "0";
        private string variationnumber = "&nbsp;";

        private string printversion = "";
        private string documenttype = "";
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
            estimateRevision_internalVersion = Request.QueryString["type"].ToString();
            estimateRevision_disclaimer_version = Request.QueryString["Version"].ToString();
            estimateRevisionId = 0;
            Int32.TryParse(Request.QueryString["EstimateRevisionId"], out estimateRevisionId);

            GetEstimateRevisionHeaderDetails(estimateRevisionId);

            Session["OriginalLogOnState"] = estimateRevision_state;

            //  ===== Modified to get Estimate ID from Estimate Revision =====
            EstimateID = estimateRevision_estimateId;

            Doc theDoc = SetupPDF();

            if (EstimateID != 0)
            {
                // ===== Added to call Webservices without Parent Page =====
                MS = new MetriconSales();
                MS.Url = Utilities.GetMetriconSqsSalesWebServiceUrl();
                MS.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                MS.Reconnect(BC_Company);
                // ===========

                string disclaimerTemplate =  @"$estimatebodytoken$";
                Doc BodyDoc = PrintEstimateBody(disclaimerTemplate, EstimateID, theDoc);
                SavePDF(BodyDoc);
            }
        }

        public Doc SetupPDF()
        {
            // Set MediaBox size, and content rectangle
            // A4: w595 h842 
            Doc theDoc = new Doc();
            theDoc.MediaBox.SetRect(0, 0, 595, 842);
            theDoc.Rect.String = "30 0 565 812";
            //theDoc.Rect.Position(0, 0);
            return theDoc;
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
        public Doc PrintEstimateBody(string pdftemplate, int estimateid, Doc theDoc)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder tempdesc = new StringBuilder();
            string tempStr = string.Empty;

            // Add the Disclaimer/Acknowledgements body text.
            Doc theDoc3 = new Doc();
            theDoc3.MediaBox.SetRect(0, 0, 595, 600);
            theDoc3.Rect.String = "0 0 565 600";

            theDoc3.Color.Color = System.Drawing.Color.Black;
            theDoc3.Font = theDoc3.AddFont(Common.PRINTPDF_DEFAULT_FONT);
            theDoc3.TextStyle.Size = Common.PRINTPDF_DISCLAIMER_FONTSIZE;
            theDoc3.TextStyle.Bold = false;
            theDoc3.Rect.Pin = 0;
            theDoc3.Rect.Position(20, 0);
            theDoc3.Rect.Width = 400;
            theDoc3.Rect.Height = 600;
            theDoc3.TextStyle.LeftMargin = 25;

            string disclaimer = Common.getDisclaimer(estimateRevisionId.ToString(), Session["OriginalLogOnState"].ToString(), estimateRevision_internalVersion, estimateRevision_disclaimer_version).Replace("$Token$", tempStr);
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
         
            return theDoc;

        }
        public void SavePDF(Doc theDoc)
        {
            Random R = new Random();
            byte[] theData = theDoc.GetData();
            CommonFunction cf = new CommonFunction();

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
                    estimateRevision_revisionNumber = ds.Tables[0].Rows[0]["RevisionNumber"].ToString();
                    if (ds.Tables[0].Rows[0]["EffectiveDate"] != null && ds.Tables[0].Rows[0]["EffectiveDate"].ToString() != "")
                    {
                        estimateRevision_effectiveDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["EffectiveDate"]).ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        estimateRevision_effectiveDate = "&nbsp;";
                    }
                    estimateRevision_state = ds.Tables[0].Rows[0]["State"].ToString();
                }
            }
            catch (Exception)
            {

            }
        }

    }
}