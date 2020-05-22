using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public partial class StartPage : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["ref"] == "SQS")
                Page.ClientScript.RegisterStartupScript(this.GetType(), "open", "openWindowFromSQS();", true);
            else
                Page.ClientScript.RegisterStartupScript(this.GetType(), "open", "openWindow();", true);
        }
    }
}