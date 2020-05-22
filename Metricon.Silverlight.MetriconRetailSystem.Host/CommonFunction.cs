using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using RestSharp;
using Newtonsoft.Json;
using Metricon.Silverlight.MetriconRetailSystem.Host.DocuSignWebService;
using Metricon.Silverlight.MetriconRetailSystem.Host.Internal;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    public class CommonFunction
    {
        private SqlConnection connection = null;
        private int timeout = 120;
        private SqlDataAdapter Adapter;
        private DataSet ds;

        public string ProcessDocuSign(string action,
            string estimateRevisionId,
            string estimateRevision_internalVersion,
            string printversion,
            string userid,
            byte[] theData,
            int pagenumber,
            string recipients,
            string recipientsemail,
            string routingorder,
            string documenttype,
            string recipientsortorder,
            string emailsubject,
            string emailbody,
            string methods,
            string envelopeid,
            string estimateRevision_revisionNumber,
            string estimateid,
            string bccontractnumber

            )
        {
            string returnstring = "";
            if (action.ToLower() == "sendviadocusign")
            {
                try
                {

                    DBConnection DBCon = new DBConnection();
                    SqlCommand SqlCmd = DBCon.ExecuteStoredProcedure("sp_salesestimate_DocuSignPushDocumentToTheProcessQueue");
                    SqlCmd.Parameters["@revisionid"].Value = estimateRevisionId;
                    SqlCmd.Parameters["@versiontype"].Value = estimateRevision_internalVersion;
                    SqlCmd.Parameters["@printtype"].Value = printversion;
                    SqlCmd.Parameters["@userid"].Value = userid;
                    SqlCmd.Parameters["@file"].Value = theData;
                    SqlCmd.Parameters["@pagenumber"].Value = pagenumber;
                    SqlCmd.Parameters["@primarycontact"].Value = recipients;
                    SqlCmd.Parameters["@primarycontactemail"].Value = recipientsemail;
                    SqlCmd.Parameters["@routingorder"].Value = routingorder;
                    SqlCmd.Parameters["@documenttype"].Value = documenttype;
                    SqlCmd.Parameters["@sortorder"].Value = recipientsortorder;
                    SqlCmd.Parameters["@emailsubject"].Value = emailsubject;
                    SqlCmd.Parameters["@emailbody"].Value = emailbody;
                    SqlCmd.Parameters["@methods"].Value = methods;

                    DataSet ds = DBCon.SelectSqlStoredProcedure(SqlCmd);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else // sign in person
            {
                DocuSignWebService.SignerParameter[] sp = GetSignerParameters(methods, recipients, recipientsemail, recipientsortorder, routingorder);
                string url = "";
                string embededsignername = "";
                string embededsigneremail = "";

                foreach (SignerParameter pp in sp)
                {
                    if (pp.Action == "2")
                    {
                        embededsignername = pp.SignerName;
                        embededsigneremail = pp.SignerEmail;
                        break;
                    }
                }
                //url = docusrv.DocuSign_GetRecipientView(envelopeid, embededsignername, embededsigneremail);


                EmbeddedSignerRequest rq = new EmbeddedSignerRequest();
                rq.AuthenticationMethod = "Email";
                rq.ClientUserId = embededsigneremail;
                rq.ReturnUrl = "http://www.docusign.com/devcenter";
                rq.UserName = embededsignername;
                rq.Email = embededsigneremail;

                var docusrv = new DocuSignWebService.DocuSignWebService();
                docusrv.Url = Utilities.GetMetriconSqsDocuSignWebServiceUrl();

                if (envelopeid != null && envelopeid != "" && envelopeid != Guid.Empty.ToString())
                {
                    url = docusrv.DocuSign_GetRecipientViewRest(envelopeid, rq);
                }
                else
                {
                    EnvelopeResult result = docusrv.DocuSign_SignInOffice(theData, pagenumber, estimateid, estimateRevision_internalVersion, printversion, estimateRevision_revisionNumber, sp, ConfigurationManager.AppSettings["DocuSign_Customer_anchorXOffset"].ToString(), ConfigurationManager.AppSettings["DocuSign_Customer_anchorYOffset"].ToString(), "MRS", documenttype, emailsubject, emailbody, bccontractnumber);
                    url = docusrv.DocuSign_GetRecipientViewRest(result.EnvelopeId, rq);
                }
                returnstring = url;
            }


            return returnstring;
        }

        private DocuSignWebService.SignerParameter[] GetSignerParameters(string action, string recipientname, string recipientemail, string sortorders, string routingorder)
        {

            string[] tempname = recipientname.Split(',');
            string[] tempemail = recipientemail.Split(',');
            string[] temporder = routingorder.Split(',');
            string[] tempsortorder = sortorders.Split(',');
            string[] tempaction = action.Split(',');
            DocuSignWebService.SignerParameter[] ls = new DocuSignWebService.SignerParameter[tempname.Length];

            for (int i = 0; i < tempname.Length; i++)
            {
                DocuSignWebService.SignerParameter sp = new DocuSignWebService.SignerParameter();
                sp.SignerEmail = tempemail[i];
                sp.SignerName = tempname[i].Replace("Miss", "").Replace("Mr", "").Replace("Mrs", "").Replace("Ms", "").Replace("Dr", "");
                sp.RoutingOrder = int.Parse(temporder[i]);
                sp.SortOrder = int.Parse(tempsortorder[i]);
                sp.Action = tempaction[i];
                ls[i] = sp;
            }


            return ls;
        }


        public DataTable GetCustomerContactListFromACC(string contractnumber)
        {
            DataTable contactTable = new DataTable();
            contactTable.Columns.Add("Name");
            contactTable.Columns.Add("HomePhone");
            contactTable.Columns.Add("MobilePhone");
            contactTable.Columns.Add("BusinessPhone");
            contactTable.Columns.Add("EmailAddress");
            contactTable.Columns.Add("PreferredContact");
            contactTable.Columns.Add("PreferredTime");
            contactTable.Columns.Add("isPrimary");
            contactTable.Columns.Add("relationshiptype");
            contactTable.Columns.Add("address");
            contactTable.Columns.Add("firstname");
            contactTable.Columns.Add("lastname");
            contactTable.Columns.Add("suburb");
            contactTable.Columns.Add("postcode");
            contactTable.Columns.Add("state");
            contactTable.Columns.Add("salutation");

            try
            {
                string url = ConfigurationManager.AppSettings["CRMContractApiUrl"].ToString() + ConfigurationManager.AppSettings["WEBAPIGetContactListMethodName"].ToString() + "?contractnumber=" + contractnumber;
                var request = new RestRequest("", Method.GET);
                RestClient rst = new RestClient(new Uri(url));
                var response = rst.Execute(request);

                List<NewCRMContact> contactlist = JsonConvert.DeserializeObject<List<NewCRMContact>>(response.Content);
                foreach (NewCRMContact c in contactlist)
                {
                    DataRow contactRow = contactTable.NewRow();
                    contactRow["Name"] = c.FirstName + " " + c.LastName;
                    contactRow["HomePhone"] = c.HomePhone;
                    contactRow["MobilePhone"] = c.MobilePhone;
                    contactRow["EmailAddress"] = c.EmailAddress;
                    contactRow["isPrimary"] = c.IsPrimary;
                    contactRow["relationshiptype"] = c.RelationshipType;

                    contactRow["address"] = c.Address1;
                    contactRow["firstname"] = c.FirstName;
                    contactRow["lastname"] = c.LastName;
                    contactRow["suburb"] = c.Suburb;
                    contactRow["postcode"] = c.Postalcode;
                    contactRow["state"] = c.StateorProvince;
                    contactRow["salutation"] = c.Salutation;
                    contactTable.Rows.Add(contactRow);
                }
                return contactTable;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        #region sql help fucntion
        public SqlCommand ConstructStoredProcedure(string SP)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ToString();
            SqlCommand Cmd = null;

            try
            {
                if (connectionstring != null)
                {
                    connection = new SqlConnection(connectionstring);
                    connection.Open();
                }
                Cmd = new SqlCommand(SP, connection);
                Cmd.CommandTimeout = timeout;
                SqlDataAdapter Adapter = new SqlDataAdapter(Cmd);
                Cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(Cmd);
            }
            catch (Exception)
            {
                if (connection != null)
                    connection.Dispose();
                if (Cmd != null)
                    Cmd.Dispose();

                throw;
            }
            return Cmd;
        }

        public DataSet ExcuteSqlStoredProcedure(SqlCommand SelectCommand)
        {
            try
            {
                Adapter = new SqlDataAdapter(SelectCommand);
                SelectCommand.CommandTimeout = timeout;
                ds = new DataSet();
                Adapter.Fill(ds);

                connection.Close();

                return ds;
            }
            //catch (Exception ex)
            //{
            //    throw ex;
            //}
            finally
            {
                if (connection != null)
                    connection.Dispose();
            }
        }
        #endregion
    }

    public class NewCRMContact
    {
        public Guid AccountID { get; set; }
        public Guid ContactID { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string BusinessPhone { get; set; }
        public string Country { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string HomePhone { get; set; }
        public bool IsPrimary { get; set; }
        public string JobTitle { get; set; }
        public string LastName { get; set; }
        public string Fullname { get; set; }
        public string MobilePhone { get; set; }
        public string Postalcode { get; set; }
        public string Postcode { get; set; }
        public string Suburb { get; set; }
        public string StateorProvince { get; set; }
        public string StateProvince { get; set; }
        public string SalesCode { get; set; }
        public string Salutation { get; set; }
        public int SalutationID { get; set; }
        public int BCSequenceNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string RelationshipType { get; set; }
        public string PreferredContactMethod { get; set; }
        public string PreferredContactTime { get; set; }
    }
}