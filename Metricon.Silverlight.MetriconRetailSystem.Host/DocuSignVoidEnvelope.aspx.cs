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

using Metricon.Silverlight.MetriconRetailSystem.Host.MetriconSalesWebService;
using Metricon.Silverlight.MetriconRetailSystem.Host.DocuSignWebService;
using Metricon.Silverlight.MetriconRetailSystem.Host.Internal;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class DocuSignVoidEnvelope : System.Web.UI.Page
    {
        private string integrationid = "";
        private string envelopeid = "";
        private string username = "";
        private string voidreason = "";

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Request.QueryString["integrationid"] != null)
                integrationid = Request.QueryString["integrationid"].ToString();

            if (Request.QueryString["envelopeid"] != null)
                envelopeid = Request.QueryString["envelopeid"].ToString();

            if (Request.QueryString["username"] != null)
                username = Request.QueryString["username"].ToString();

            if (Request.QueryString["voidreason"] != null)
                voidreason = Request.QueryString["voidreason"].ToString().Replace("~!~", "<br>");

            Envelope env = new Envelope();
            env.envelopeid = envelopeid;
            env.status = "voided";
            env.voidedreason = "<br>" + voidreason + "<br><br>Voided by " + username;

            var docusrv = new DocuSignWebService.DocuSignWebService();
            docusrv.Url = Utilities.GetMetriconSqsDocuSignWebServiceUrl();

            DocuSignWebService.EnvelopeResult rs = docusrv.DocuSign_VoidEnvelopeRest(env);

            //DocuSignWebService.GetViewResponse rs = docusrv.DocuSign_VoidEnvelope(envelopeid, username);


        }
    }
}