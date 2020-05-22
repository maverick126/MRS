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
    public class Common
    {
        public const string SELECTED_USER_SESSION_NAME = "";
        public const string PRINTPDF_DEFAULT_FONT = "Arial";
        public const double PRINTPDF_DEFAULT_FONTSIZE = 10;
        public const string PRINTPDF_DISCLAIMER_HEADER = "Agreement:";
        public const double PRINTPDF_DISCLAIMER_FONTSIZE = 8.3;
        public const string PRINTPDF_CUSTOMER_LABEL = "Customer:";
        public const string PRINTPDF_SIGNATURE_LABEL = "Sign Here : ________________";
        public const int ESTIMATE_STATUS_EXPIRED = 5;
        public const string ORDERFORM_FONT = "Arial";
        public const int ORDERFORM_ESTIMATE_EXPIRED_STAMP_FONTSIZE = 70;
        public const int ORDERFORM_ESTIMATE_FINAL_STAMP_FONTSIZE = 50;
        public const int ESTIMATE_FINAL_CONSTRUCTION_COPY_STAMP_FONTSIZE = 45;
        public const int ESTIMATE_DATE_TIME_STAMP_FONTSIZE = 32;
        public const string ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR = "255 0 0"; // Red
        public const int ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE = 60;
        public const string ORDERFORM_ESTIMATE_EXPIRED_STAMP = "Expired";
        public const string ORDERFORM_PAGENUMBER_COORDINATES = "25 20 505 32";
        public const int ORDERFORM_PAGENUMBER_FONTSIZE = 10;
        public static int ESTIMATE_STATUS_ACCEPTED = 3;
        
        public static int ConvertStringToIntIfFailToZero(string inputstring)
        {
            int outputint = 0;
            try
            {
                outputint = Int32.Parse(inputstring);
            }
            catch (Exception)
            {
                outputint = 0;
            }
            return outputint;
        }


        public static string getDisclaimer(string revisionid, string state, string printtype, string disclaimer_version="Current")
        {
            string result = "";
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_SalesEstimate_GetMRSDisclaimer");
            Cmd.Parameters["@revisionid"].Value = revisionid;
            Cmd.Parameters["@state"].Value = state;
            Cmd.Parameters["@printtype"].Value = printtype;
            Cmd.Parameters["@version"].Value = disclaimer_version;
            DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

            if (ds.Tables[0].Rows.Count == 1)
            {
                result = ds.Tables[0].Rows[0]["agreement"].ToString();
            }

            return result;
        }

        /// <summary>
        /// Replaces the CRLF by line break before it can be displayed properly in the browser.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static string ReplaceCRLFByLineBreak(string text)
        {
            StringBuilder newText = new StringBuilder();

            try
            {
                newText.Append(text);

                newText.Replace("\r\n", @"<br />");
                newText.Replace("\n", @"<br />");
            }
            catch
            {
                // Nothing.
            }

            return newText.ToString();
        }
    }

    public class DBConnection
    {
        public SqlConnection conn = null;
        public SqlDataAdapter Adapter;
        public string ConnectionString = "";
        public DataSet DS;
        public int timeout;

        public DBConnection()
        {
            timeout = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["SQLCommandTimeOut"]);
        }
        public DataSet ExecuteQuery(String Query)
        {
            try
            {

                string ConnectionString = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ConnectionString;
                conn = new SqlConnection(ConnectionString);
                conn.Open();

                Adapter = new SqlDataAdapter(Query, conn);
                DS = new DataSet();
                Adapter.Fill(DS);
                CloseDBConnection();
                return DS;
            }
            catch (Exception)
            {
                throw;
            }
        }
        // this function return a dataset when user call a stored procdure with parameters

        public DataSet ExecuteSQLQuery(string sqlStoredProcedure, SqlParameter[] myParameters)
        {

            try
            {
                string ConnectionString = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ConnectionString;
                conn = new SqlConnection(ConnectionString);
                conn.Open();

                Adapter = new SqlDataAdapter(sqlStoredProcedure, conn);
                DS = new DataSet();

                //mySQLAdapter.SelectCommand = new SqlCommand(sqlStoredProcedure);
                Adapter.SelectCommand.Connection = conn;
                Adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                Adapter.SelectCommand.CommandTimeout = timeout;


                foreach (SqlParameter tempParam in myParameters)
                {

                    Adapter.SelectCommand.Parameters.Add(tempParam);
                }

                Adapter.Fill(DS);


            }
            catch (Exception)
            {
                conn.Close();
            }
            finally
            {
                //if there are no tables add default table
                if (DS.Tables.Count == 0)
                {
                    DS.Tables.Add(new DataTable("NoRecords"));
                }
                conn.Close();
                conn.Dispose();
            }
            return DS;
        }
        /// <summary>
        /// this method is used to setup the connection string and the storedprocedure to be executed by the user
        /// </summary>
        /// <param name="SP"></param>
        /// <returns></returns>
        public SqlCommand ExecuteStoredProcedure(string SP)
        {
            SqlCommand Cmd = null;
            try
            {
                if (conn == null)
                {
                    string ConnectionString = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ConnectionString;
                    conn = new SqlConnection(ConnectionString);
                    conn.Open();
                }
                Cmd = new SqlCommand(SP, conn);
                Cmd.CommandTimeout = timeout;
                SqlDataAdapter Adapter = new SqlDataAdapter(Cmd);
                Cmd.CommandType = CommandType.StoredProcedure;
                SqlCommandBuilder.DeriveParameters(Cmd);
            }
            catch (Exception)
            {

            }
            return Cmd;
        }
        /// <summary>
        /// this method will only be called when the user want to insert a new record in the database
        /// </summary>
        /// <param name="InsertCommand"></param>
        /// <returns></returns>
        public int InsertStoredProcedure(SqlCommand InsertCommand)
        {
            InsertCommand.CommandTimeout = timeout;
            int RowsAffected = InsertCommand.ExecuteNonQuery();
            CloseDBConnection();
            return RowsAffected;
        }
        /// <summary>
        /// this method will be called to update the values in the database
        /// </summary>
        /// <param name="UpdateCommand"></param>
        /// <returns></returns>
        public int UpdateStoredProcedure(SqlCommand UpdateCommand)
        {
            UpdateCommand.CommandTimeout = timeout;
            int RowsAffected = UpdateCommand.ExecuteNonQuery();
            CloseDBConnection();
            return RowsAffected;
        }

        public DataSet SelectSqlStoredProcedure(SqlCommand SelectCommand)
        {
            try
            {
                Adapter = new SqlDataAdapter(SelectCommand);
                SelectCommand.CommandTimeout = timeout;
                DS = new DataSet();
                Adapter.Fill(DS);
                CloseConnection();
                return DS;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DisposeObjects()
        {
            Adapter.Dispose();
            Adapter = null;
            DS.Dispose();
            DS = null;
            CloseDBConnection();
        }

        public void CloseConnection()
        {
            CloseDBConnection();
        }

        public void CloseDBConnection()
        {
            try
            {
                conn.Close();
            }
            catch (Exception)
            {
            }
        }

        ~DBConnection()
        {
            CloseDBConnection();
        }
    }

    public class Client
    {
        public Client()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static DataSet GetEstimateAcceptedForCustomer(int BCCustomerCode, int contractNo)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_CheckIsEstimateAcceptedForCustomer2");
            Cmd.Parameters["@customerCode"].Value = BCCustomerCode;
            Cmd.Parameters["@contractNo"].Value = contractNo;
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }
        public static DataSet GetEstimateAcceptedByOpportunity(string opportunityid)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_SQSCRM_CheckIsEstimateAcceptedByOpportunity");
            Cmd.Parameters["@opportunityid"].Value = opportunityid;
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }
        public static DataSet GetUserList(string username)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserList");
            Cmd.Parameters["@currentusername"].Value = username;
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }

        public static DataSet GetUserListFromUsercode(string usercode)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserListFromUsercode");
            Cmd.Parameters["@currentusercode"].Value = usercode;
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }

        public static DataSet GetCustomerClientHouseDetails(string bccontractnumber, string bccustomernumber)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetCustomerClientHouseDetails");
            Cmd.Parameters["@bccontractnumber"].Value = Common.ConvertStringToIntIfFailToZero(bccontractnumber);
            Cmd.Parameters["@bccustomernumber"].Value = Common.ConvertStringToIntIfFailToZero(bccustomernumber);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }

        public static DataSet GetUserListForAdmin()
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserListForAdmin");
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }

        public static DataSet GetUserDefaultRoleID(string fullname)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetUserDefautRoleID");
            Cmd.Parameters["@username"].Value = fullname;
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }
        public static DataSet GetUserListForManagers(string userid, string roleid)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserListForManagers");
            Cmd.Parameters["@userid"].Value = userid;
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }

        public static DataSet GetAllSalesConsultants(string stateid)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetAllSalesConsultants");
            Cmd.Parameters["@fkstateid"].Value = Int16.Parse(stateid);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }
        public static DataSet GetUserRoles(string userid)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetUserRoles");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            return DS;
        }
        public static bool userInMultipleRoles(string userid)
        {
            bool result = false;
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserInMultipleRoles");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
        public static bool isUserAdmin(string userid)
        {
            bool result = false;
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserAdmin");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
        public static bool isUserRoleAdmin(string userid, string roleid)
        {
            bool result = false;
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserRoleAdmin");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
            Cmd.Parameters["@roleid"].Value = Common.ConvertStringToIntIfFailToZero(roleid);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
        public static bool isUserRoleManager(string userid, string roleid)
        {
            bool result = false;
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserRoleManager");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
            Cmd.Parameters["@roleid"].Value = Common.ConvertStringToIntIfFailToZero(roleid);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
        public static bool isUserRoleCanSeeAllConsultant(string userid, string roleid)
        {
            bool result = false;
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserRoleCanSeeAllConsultant");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
            Cmd.Parameters["@roleid"].Value = Common.ConvertStringToIntIfFailToZero(roleid);
            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
        public static bool isUserPackageAdmin(string userid)
        {
            bool result = false;
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserPackageAdmin");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);

            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
        public static bool isUserSalesEstimator(string userid)
        {
            bool result = false;
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserSalesEstimator");
            Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);

            DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
            if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return result;
        }
    }

    public class Estimate
    {
        public Estimate()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static double GetUpgradesTotal(int Estimateid)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand UpgradesTotalCmd = DB.ExecuteStoredProcedure("sp_GetUpgradeTotalForEstimate");
                UpgradesTotalCmd.Parameters["@estimateid"].Value = Estimateid;
                DataSet UpgradesTotalDS = DB.SelectSqlStoredProcedure(UpgradesTotalCmd);
                if (UpgradesTotalDS != null)
                    if (UpgradesTotalDS.Tables[0].Rows.Count > 0)
                    {
                        DataRow UpgradeRow = UpgradesTotalDS.Tables[0].Rows[0];
                        if ((UpgradeRow["upgradetotal"] != null) && (UpgradeRow["upgradetotal"].ToString() != string.Empty))
                            return double.Parse(UpgradeRow["upgradetotal"].ToString());
                        else return 0;
                    }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static double GetHomeBasePrice(int Estimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand HomePriceCmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
            HomePriceCmd.Parameters["@estimateid"].Value = Estimateid;
            DataSet HomePriceDS = DB.SelectSqlStoredProcedure(HomePriceCmd);
            if (HomePriceDS != null)
                if (HomePriceDS.Tables[0].Rows.Count > 0)
                {
                    DataRow HomePriceRow = HomePriceDS.Tables[0].Rows[0];
                    return double.Parse(HomePriceRow["homesellprice"].ToString());
                }
            return 0;
        }

        public static double GetLandPrice(int Estimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand HomePriceCmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
            HomePriceCmd.Parameters["@estimateid"].Value = Estimateid;
            DataSet HomePriceDS = DB.SelectSqlStoredProcedure(HomePriceCmd);
            if (HomePriceDS != null)
                if (HomePriceDS.Tables[0].Rows.Count > 0)
                {
                    DataRow HomePriceRow = HomePriceDS.Tables[0].Rows[0];
                    return double.Parse(HomePriceRow["landprice"].ToString());
                }
            return 0;
        }
        public static double GetSiteWorksTotalForEstimate(int Estimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand SiteWorksCmd = DB.ExecuteStoredProcedure("sp_GetSiteWorksTotalForEstimate");
            SiteWorksCmd.Parameters["@estimateid"].Value = Estimateid;
            DataSet SiteWorksDS = DB.SelectSqlStoredProcedure(SiteWorksCmd);
            if (SiteWorksDS != null)
                if (SiteWorksDS.Tables[0].Rows.Count > 0)
                {
                    DataRow SiteWorksRow = SiteWorksDS.Tables[0].Rows[0];
                    if (SiteWorksRow["siteworktotal"] != null && SiteWorksRow["siteworktotal"].ToString() != string.Empty)
                        return double.Parse(SiteWorksRow["siteworktotal"].ToString());
                    else
                        return 0;
                }
            return 0;
        }


        public static DataSet GetHomePriceBrandAndHomeName(int Estimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand HPBHCmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
            HPBHCmd.Parameters["@estimateid"].Value = Estimateid;
            DataSet HPBHDS = DB.SelectSqlStoredProcedure(HPBHCmd);
            return HPBHDS;
        }

        public static double GetDOLOptionTotal(int Estimateid, int HomeDisplayid)
        {
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand DolOptionTotalCmd = DB.ExecuteStoredProcedure("sp_GetDOLOptionTotal");
                DolOptionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
                DolOptionTotalCmd.Parameters["@homedisplayid"].Value = HomeDisplayid;

                DataSet DolOptionTotalDS = DB.SelectSqlStoredProcedure(DolOptionTotalCmd);
                if (DolOptionTotalDS != null)
                    if (DolOptionTotalDS.Tables[0].Rows.Count > 0)
                    {
                        DataRow DolOptionTotalRow = DolOptionTotalDS.Tables[0].Rows[0];
                        if (DolOptionTotalRow["DOLTotal"].ToString() != String.Empty || DolOptionTotalRow["DOLTotal"] != null || DolOptionTotalRow["DOLTotal"].ToString() != "")
                            return double.Parse(DolOptionTotalRow["DOLTotal"].ToString());
                    }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public static double GetPromotionTotalForEstimate(int Estimateid,
                                                          int EstimateHouseid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand PromotionTotalCmd = DB.ExecuteStoredProcedure("sp_GetPrmotionTotalForEstimate");
            PromotionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
            PromotionTotalCmd.Parameters["@estimatehouseid"].Value = EstimateHouseid;
            DataSet PromotionTotalDS = DB.SelectSqlStoredProcedure(PromotionTotalCmd);

            if (PromotionTotalDS != null)
                if (PromotionTotalDS.Tables[0].Rows.Count > 0)
                {
                    DataRow PromotionRow = PromotionTotalDS.Tables[0].Rows[0];

                    if ((PromotionRow["promotiontotal"] != null) && (PromotionRow["promotiontotal"].ToString() != string.Empty))
                        return double.Parse(PromotionRow["promotiontotal"].ToString());
                    else return 0;
                }

            return 0;
        }

        public static DataSet GetPromotionTotalForEstimateAsDataSet(int Estimateid,
                                                      int EstimateHouseid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand PromotionTotalCmd = DB.ExecuteStoredProcedure("sp_GetPrmotionTotalForEstimate");
            PromotionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
            PromotionTotalCmd.Parameters["@estimatehouseid"].Value = EstimateHouseid;
            DataSet PromotionTotalDS = DB.SelectSqlStoredProcedure(PromotionTotalCmd);

            return PromotionTotalDS;
        }
        /// <summary>
        /// This method returns the promotional total and upgrade total for a particular estimate.
        /// </summary>
        /// <param name="Estimateid">Estimate Id</param>
        /// <param name="EstimateHouseid">HouseId for the Estimate</param>
        /// <param name="promotionTotal">Returns the total promotional value</param>
        /// <param name="upgradeTotal">Returns the total upgrade value</param>
        /// <returns>Returns void</returns>
        /// <remarks>
        /// </remarks>
        public static void GetPromotionTotalForEstimate2(int Estimateid,
                                                            int EstimateHouseid,
                                                            out double promotionTotal,
                                                            out double upgradeTotal)
        {
            promotionTotal = 0;
            upgradeTotal = 0;

            DBConnection DB = new DBConnection();
            SqlCommand PromotionTotalCmd = DB.ExecuteStoredProcedure("sp_GetPrmotionTotalForEstimate");
            PromotionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
            PromotionTotalCmd.Parameters["@estimatehouseid"].Value = EstimateHouseid;
            DataSet PromotionTotalDS = DB.SelectSqlStoredProcedure(PromotionTotalCmd);

            if (PromotionTotalDS != null)
            {
                if (PromotionTotalDS.Tables[0].Rows.Count > 0)
                {
                    DataRow PromotionRow = PromotionTotalDS.Tables[0].Rows[0];

                    if ((PromotionRow["upgradetotal"] != null) && (PromotionRow["upgradetotal"].ToString() != string.Empty))
                        upgradeTotal = double.Parse(PromotionRow["upgradetotal"].ToString());

                    if ((PromotionRow["promotiontotal"] != null) && (PromotionRow["promotiontotal"].ToString() != string.Empty))
                        promotionTotal = double.Parse(PromotionRow["promotiontotal"].ToString());
                }
            }

            return;
        }

        public static DataSet GetItemsForSelectedAreaGroup(int areaid, int groupid, int estimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand GIFSAGCmd = DB.ExecuteStoredProcedure("sp_GetItemsForSelectedAreaGroup");
            GIFSAGCmd.Parameters["@areaid"].Value = areaid;
            GIFSAGCmd.Parameters["@groupid"].Value = groupid;
            GIFSAGCmd.Parameters["@estimateid"].Value = estimateid;
            DataSet GIFSAGDS = DB.SelectSqlStoredProcedure(GIFSAGCmd);
            return GIFSAGDS;
        }

        public static string GetArea(int areaid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand AreaCmd = DB.ExecuteStoredProcedure("sp_GetArea");
            AreaCmd.Parameters["@areaid"].Value = areaid;
            DataSet AreaDS = DB.SelectSqlStoredProcedure(AreaCmd);
            if (AreaDS != null)
                if (AreaDS.Tables[0].Rows.Count > 0)
                {
                    DataRow AreaRow = AreaDS.Tables[0].Rows[0];
                    if ((AreaRow["areaname"] != null) && (AreaRow["areaname"].ToString() != string.Empty))
                        return AreaRow["areaname"].ToString();
                    else return "";
                }
            return "";
        }

        public static string GetGroup(int groupid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand GroupCmd = DB.ExecuteStoredProcedure("sp_GetGroup");
            GroupCmd.Parameters["@groupid"].Value = groupid;
            DataSet GroupDS = DB.SelectSqlStoredProcedure(GroupCmd);
            if (GroupDS != null)
                if (GroupDS.Tables[0].Rows.Count > 0)
                {
                    DataRow GroupRow = GroupDS.Tables[0].Rows[0];
                    if ((GroupRow["Groupname"] != null) && (GroupRow["Groupname"].ToString() != string.Empty))
                        return GroupRow["Groupname"].ToString();
                    else return "";
                }
            return "";
        }

        public static DataSet GetEstimateView(int estimateid, int viewtype)
        {
            DBConnection DB = new DBConnection();
            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_GetEstimateView");
            EstimateViewCmd.Parameters["@estimateid"].Value = estimateid;
            EstimateViewCmd.Parameters["@viewtype"].Value = viewtype;
            DataSet EstimateViewDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);
            return EstimateViewDS;
        }

        public static void SetEstimateToCompleted(int estimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand EstimateCmd = DB.ExecuteStoredProcedure("sp_MarkEstimateToCompleted");
            EstimateCmd.Parameters["@estimateid"].Value = estimateid;
            int rowsAffected = DB.InsertStoredProcedure(EstimateCmd);
        }

        public static int CheckEstimateStatus(int sourceestimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand EstimateStatusCmd = DB.ExecuteStoredProcedure("sp_GetEstimateStatus");
            EstimateStatusCmd.Parameters["@sourceestimateid"].Value = sourceestimateid;
            DataSet EstimateStatusDS = DB.SelectSqlStoredProcedure(EstimateStatusCmd);
            if (EstimateStatusDS != null)
                if (EstimateStatusDS.Tables[0].Rows.Count > 0)
                {
                    DataRow EstimateStatusRow = EstimateStatusDS.Tables[0].Rows[0];
                    return int.Parse(EstimateStatusRow["fkestimatestatusid"].ToString());
                }
            return 0;
        }

        public static bool IsEstimateHasDeposit(int sourceestimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand EstimateDepositCmd = DB.ExecuteStoredProcedure("spw_GetEstimateDepositFlag");
            EstimateDepositCmd.Parameters["@sourceestimateid"].Value = sourceestimateid;
            DataSet EstimateDepositDS = DB.SelectSqlStoredProcedure(EstimateDepositCmd);
            if (EstimateDepositDS != null)
            {
                if (EstimateDepositDS.Tables[0].Rows.Count > 0)
                {
                    DataRow EstimateDepositRow = EstimateDepositDS.Tables[0].Rows[0];
                    if (EstimateDepositRow["Deposited"].ToString() == "1")
                        return true;
                }
            }
            return false;
        }

        public static DataSet GetInitialInformationForWIPEstimate(int sourceestimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand GIIFWIPECmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
            GIIFWIPECmd.Parameters["@estimateid"].Value = sourceestimateid;
            DataSet GIIFWIPEDS = DB.SelectSqlStoredProcedure(GIIFWIPECmd);
            return GIIFWIPEDS;
        }

        public static DataSet GetEstimateDetailsForPrint(int estimateid)
        {
            DBConnection DB = new DBConnection();
            SqlCommand GEDFPCmd = DB.ExecuteStoredProcedure("sp_GetEstimateDetailsForPrint");
            GEDFPCmd.Parameters["@estimateid"].Value = estimateid;
            DataSet GEDFPDS = DB.SelectSqlStoredProcedure(GEDFPCmd);
            return GEDFPDS;
        }

        public static DataSet GetEstimateDetails(int estimateId)
        {
            DBConnection DB = new DBConnection();
            SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_GetEstimateDetails");
            sqlCmd.Parameters["@estimateid"].Value = estimateId;

            DataSet estimateDetailsDS = DB.SelectSqlStoredProcedure(sqlCmd);

            return estimateDetailsDS;
        }

        public static void SetEsimateActiveStatus(int estimateId, bool active)
        {
            DBConnection DB = new DBConnection();
            SqlCommand EstimateCmd = DB.ExecuteStoredProcedure("sp_SetEstimateActiveStatus");
            EstimateCmd.Parameters["@estimateid"].Value = estimateId;
            EstimateCmd.Parameters["@active"].Value = active;

            int rowsAffected = DB.InsertStoredProcedure(EstimateCmd);
        }

        public static string GetEffectiveDateForHomePrice(int estimateId)
        {
            string date = "";
            DBConnection DB = new DBConnection();
            SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_GetEffectiveDateForHomePrice");
            sqlCmd.Parameters["@estimateId"].Value = estimateId;
            DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
                date = ds.Tables[0].Rows[0]["PriceDate"].ToString();
            return date;
        }

        public static string GetFacadeUpgrade(int estimate_id, string return_column)
        {
            string facade = "";
            DBConnection DB = new DBConnection();
            SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_GetFacadeUpgrade");
            sqlCmd.Parameters["@estimate_id"].Value = estimate_id;
            DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                if (return_column != "")
                    facade = ds.Tables[0].Rows[0][return_column].ToString();
                else
                    facade = ds.Tables[0].Rows[0]["ProductName"].ToString();
            }
            return facade;
        }

        public static string getRegionIDFromEstimate(string estimateid)
        {
            string result = "";
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
            Cmd.Parameters["@estimateid"].Value = Common.ConvertStringToIntIfFailToZero(estimateid);
            DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["regionid"].ToString();
            }

            return result;
        }
        public static string getHomeStoreyFromEstimate(string estimateid)
        {
            string result = "";
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetHomeStoreyFromEstimate");
            Cmd.Parameters["@estimateid"].Value = estimateid;
            DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

            if (ds.Tables[0].Rows.Count > 0)
            {
                result = ds.Tables[0].Rows[0]["stories"].ToString();
            }

            return result;
        }
        public static DataSet getPromotionProductsForEstiamte(string estimateid, string idmultiplepromotion)
        {
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetPromotionProductsForEstimate");
            Cmd.Parameters["@estimateid"].Value = estimateid;
            Cmd.Parameters["@idmultiplepromotion"].Value = idmultiplepromotion;
            DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);
            return ds;
        }
        public static string GetPromotionType(string estimateid)
        {
            string promotiontype = "7";
            DBConnection DBCon = new DBConnection();
            SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetPromotionFromEstimate");
            Cmd.Parameters["@estimateid"].Value = estimateid;
            DataSet DisplayDS = DBCon.SelectSqlStoredProcedure(Cmd);

            if (DisplayDS.Tables[0].Rows.Count > 0)
            {
                promotiontype = DisplayDS.Tables[0].Rows[0]["promotiontypeid"].ToString();
            }
            return promotiontype;
        }
    }

    public class Validation
    {
        public Validation()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string NullToString(object o)
        {
            if (o == null)
                return "";
            else
                return o.ToString();
        }

        public static string MakeFirstCharCapital(string s)
        {
            string result;
            char[] stringArray;
            s = s.ToLower();
            stringArray = s.ToCharArray();
            char first = stringArray[0];
            first = System.Convert.ToChar(first.ToString().ToUpper());
            stringArray[0] = first;
            result = new String(stringArray);
            return result;
        }

        public static string MakeCapitalAfterSpace(string s)
        {
            string result;
            char[] stringArray;
            s = s.Trim();
            s = s.ToLower();
            stringArray = s.ToCharArray();

            for (int x = 0; x < stringArray.Length; x++)
            {
                if (x == 0)
                {
                    stringArray[x] = stringArray[x].ToString().ToUpper().ToCharArray()[0];
                }
                else if (stringArray[x] == ' ')
                {
                    x++;
                    stringArray[x] = stringArray[x].ToString().ToUpper().ToCharArray()[0];

                }
            }

            result = new String(stringArray);
            return result;
        }

        /// <summary>
        /// Makes sure:
        ///		Returned an uppercase string
        ///		Returns "VIC" if parsed in string is empty or null
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string AbbreviatedState(string s)
        {
            if (s == null || s == string.Empty)
            {
                return "VIC";
            }
            return s.ToUpper();
        }

        public static string CleanEmail(string s)
        {
            if (s.Length > 0)
            {
                string result;
                result = NullToString(s.Substring(0, s.IndexOf('þ')));
                result.Replace("ÿ", "");
                return result;
            }
            else
            {
                return "";
            }
        }

        public static string GetStringFromColumn(DataRow dr, string columnName)
        {
            if (dr == null)
            {
                return "";
            }

            return dr[columnName].ToString();
        }

        public static string GetStringFromColumn(DataTable dt, int rowNumber, string columnName)
        {
            string result = "";
            try
            {
                DataRow dr = dt.Rows[rowNumber];
                result = NullToString(dr[columnName]);
            }
            catch
            {
                //Do nothing and return an empty string
            }

            return result;
        }
    }

    public partial class PrintEstimateNewTemplate : System.Web.UI.Page
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
        private string estimateRevision_homeName;
        private string estimateRevision_revisionType;
        private string estimateRevision_revisionTypeBrief;
        private int estimateRevision_revisionTypeId;
        private string estimateRevision_revisionOwner;
        private string estimateRevision_effectiveDate;
        private string estimateRevision_documenttype;
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
        private string ESTIMATE_FINALCONSTRUCTIONCOPY_STAMP = "Final Construction Copy";
        private string printversion = "FULLSUMMARY";
        private string BC_Company = "METHOMES";
        private string includestd = "1";
        private string includeProductNameAndCode = "False";
        private string includeUOMAndQuantity = "False";
        private string includeSpecifications = "False";

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
        private string pestimatecreatedate = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // ===== Added to retrieve Estimate Header from Estimate Revision =====
            estimateRevision_internalVersion = "";

            if (Request.QueryString["type"] != null)
                estimateRevision_internalVersion = Request.QueryString["type"].ToString().ToUpper();

            if (Request.QueryString["version"] != null && Request.QueryString["version"].ToString() != "")
                printversion = Request.QueryString["version"].ToString().ToUpper();

            if (Request.QueryString["IncludeProductNameAndCode"] != null)
                includeProductNameAndCode = Request.QueryString["IncludeProductNameAndCode"].ToString();

            if (Request.QueryString["IncludeUOMAndQuantity"] != null)
                includeUOMAndQuantity = Request.QueryString["IncludeUOMAndQuantity"].ToString();

            if (Request.QueryString["IncludeSpecifications"] != null)
                includeSpecifications = Request.QueryString["IncludeSpecifications"].ToString();

            if (Request.QueryString["includestd"] != null)
                includestd = Request.QueryString["includestd"].ToString();

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
                    emailbody = Request.QueryString["emailbody"].ToString().Replace("~!~",Environment.NewLine);

            }

            //recipientsortorder = "1,2,101,102";
            //recipients = "s1,s2,Vishal Choksi,builder2";
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

                Response.Write("<div sytle='width:100%; height: 100%; text-align: center;margin-top: 200px;'><img src='" + Server.MapPath(@"~/images/loading_blue1.gif") + "' alt=''/><br><br>Processing document...</div>");


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
                    if (docusignintegration == "0" || (docusignintegration == "1" && (action.ToLower() == "sendviadocusign" || (action.ToLower() != "sendviadocusign" && (envelopeid == "" || envelopeid == Guid.Empty.ToString())))))
                    {
                        // ===== Added to call Webservices without Parent Page =====
                        MS = new MetriconSales();
                        MS.Url = Utilities.GetMetriconSqsSalesWebServiceUrl();
                        MS.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                        MS.Reconnect(BC_Company);
                        // ===========

                        string updatedpdfheader = PDFHeader(EstimateID, printCustomerDetails, PDFTemplate);
                        if (estimateRevision_internalVersion == "FINAL" || printversion == "LUMPSUM")
                        {
                            updatedpdfheader = updatedpdfheader.Replace("<td>&nbsp;<br />Description</td>", "<td colspan='2'>&nbsp;<br />Description</td>");
                            updatedpdfheader = updatedpdfheader.Replace("Total Price<br />(INC. GST)", "");
                            if (estimateRevision_internalVersion == "FINAL")
                                includeProductNameAndCode = "True";
                        }
                        if (includeProductNameAndCode == "False")
                            updatedpdfheader = updatedpdfheader.Replace("Description", "").Replace("Summary", "Description");
                        if (includeUOMAndQuantity == "False")
                            updatedpdfheader = updatedpdfheader.Replace("QTY", "").Replace("UOM", "");
                        string Promotiontypeid = getPromotionType(EstimateID);
                        Doc BodyDoc = PrintEstimateBody(updatedpdfheader, EstimateID, theDoc, viewtype, Promotiontypeid);
                        SavePDF(BodyDoc);
                    }
                    else
                    {
                        SavePDF(new Doc());
                    }
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

            if (merge == 1)
            {
                DataSet surchargeDS = GetAreaSurcharge(estimateRevisionId);
                estimateRevision_surcharge = Convert.ToDouble(surchargeDS.Tables[0].Rows[0]["surcharge"]);
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
                HouseName = estimateRevision_homeName; //headervar[6].ToString();
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

                pestimatecreatedate = EstimateCreatedDate;
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
            if (printversion == "FULLSUMMARY" && estimateRevision_internalVersion != "FINAL")
            {
                PDFTemplate = PDFTemplate.Replace("$homepricetoken$", homeprice);
                PDFTemplate = PDFTemplate.Replace("$siteworkpricetoken$", siteworks);
                PDFTemplate = PDFTemplate.Replace("$upgradepricetoken$", Upgrades);

                if (estimateRevision_state.ToUpper() == "NSW")
                {
                    string temp = @"<tr><td valign='top' class='typetd'><h2>Provisional Sums</h2></td><td valign='top' class='valuetd'><p>"+String.Format("{0:C}", (estimateRevision_provisionalsums))+@"</p></td></tr>";
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
            if (estimateRevision_internalVersion == "FINAL")
            {
                PDFTemplate = PDFTemplate.Replace("<p></p>", "");
                PDFTemplate = PDFTemplate.Replace("<h2>Total</h2>", "");
                PDFTemplate = PDFTemplate.Replace("<p>$totalpricetoken$</p>", "");
                PDFTemplate = PDFTemplate.Replace("<img src=\"$totallineimagetoken$\" width=\"225\" height=\"18\" alt=\"\" />", "<img src=\"$totallineimagetoken$\" width=\"225\" height=\"0\" alt=\"\" />");
                PDFTemplate = PDFTemplate.Replace("class=\"totaltbl\"", "class=\"\"");
            }
            else
            {
                PDFTemplate = PDFTemplate.Replace("$totalpricetoken$", CartValue);
            }
            PDFTemplate = PDFTemplate.Replace("$totallineimagetoken$", Server.MapPath(@"~/images/total-line.gif"));

            if (includeUOMAndQuantity == "False" ||
                    (estimateRevision_state.ToLower() == "qld" && estimateRevision_internalVersion != "FINAL"))
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
                    BCContractnumber= HPBHRow["BCContractnumber"].ToString();
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
                housePrice = housePrice + estimateRevision_surcharge;
                packagePrice = packagePrice + estimateRevision_surcharge;
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
                optionsTotal = optionsTotal - estimateRevision_nonsiteworksurcharge;
                siteworks = siteworks - estimateRevision_siteworksurcharge;
            }

            if (houseAndLandPkgContract)
            {
                //baseprice = string.Format("{0:C}", packagePrice);
                baseprice = string.Format("{0:C}", housePrice);
                Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice - estimateRevision_nonsiteworksurcharge);

            }
            else
            {
                baseprice = string.Format("{0:C}", housePrice);
                //Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice - estimateRevision_siteworkPrice);
                Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice );
            }

            //Upgrades = string.Format("{0:C}", optionsTotal - siteworks);

            // ===========

            //if (siteworks == 0.0)
            //    siteworksvalue = "TBA";
            //else

            siteworksvalue = string.Format("{0:C}", siteworks);

            if (houseAndLandPkgContract)
                //cartvalue = string.Format("{0:C}", packagePrice + optionsTotal);
                cartvalue = string.Format("{0:C}", housePrice + optionsTotal + siteworks+estimateRevision_provisionalsums);
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
                //if (Session["AccountID"] != null)
                //{
                //    if (Session["AccountID"].ToString() != "")
                //    {
                        //string customerCode = Session["SelectedCustomerCode"].ToString();
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

                        if (CustomerName.Length>4)
                            CustomerName = CustomerName.Substring(0, CustomerName.Length - 4);
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

                if (LotAddressds.Tables[0].Rows.Count>0 && LotAddressds.Tables[0].Rows[0]["HouseAndLandPkg"].ToString() == "True")
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

            // Actual body contect starts from here.
            int RowIdentifier = 0;
            int colspanDesc = 1;

            DBConnection DB = new DBConnection();
            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateDetailsForPrinting");
            EstimateViewCmd.Parameters["@revisionId"].Value = estimateRevisionId;
            EstimateViewCmd.Parameters["@printtype"].Value = estimateRevision_internalVersion;
            EstimateViewCmd.Parameters["@includestd"].Value = includestd;

            DataSet OptionDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);
            ArrayList AreaList = new ArrayList();

            //**** Simon Selva 2016-07-28 - rewritten this logic with comments to avoid alignment issue happening on various combinations through out the entire procedure - ss - start
            if (estimateRevision_state.ToLower() != "qld" || estimateRevision_internalVersion == "FINAL")
            {
                if (printversion == "LUMPSUM")
                {
                    if (includeProductNameAndCode == "False")
                        colspanDesc += 2; // use columns productnameandcode and itemprice
                    else
                        colspanDesc += 1; // use column itemprice 
                }
                else if (estimateRevision_internalVersion == "FINAL")
                    colspanDesc += 1; // use the column itemprice
                else if (includeProductNameAndCode == "False")
                {
                    if(estimateRevision_internalVersion != "INTERNAL" && estimateRevision_internalVersion != "FINAL")
                        colspanDesc += 1; // use column productnameandcode

                }
                if (includeUOMAndQuantity == "False" && (printversion == "LUMPSUM" || printversion == "FULLTOTAL" || printversion == "FULLSUMMARY" || estimateRevision_internalVersion == "FINAL"))
                    colspanDesc += 2;
            }
            else
            {
                if (includeProductNameAndCode == "False")
                    if (estimateRevision_internalVersion != "INTERNAL" && estimateRevision_internalVersion != "FINAL")
                    {
                        colspanDesc += 3; // use columns productnameandcode, uom and quantity
                    }
                    else
                    { 
                        colspanDesc += 2; // if internal and don't print product code
                    }
                else
                { 
                    colspanDesc += 2; // use columns uom and quantity
                }
                if (includeUOMAndQuantity == "False" && (printversion == "LUMPSUM"))
                    colspanDesc += 2;
            }
            //**** ss - end

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
                        sb.Append("<td style='width:100px;'><img src='" + Server.MapPath(@"~/images/spacer.gif") + "' width='100' height='1' alt='' /></td></tr>");

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
                        PrintEstimateBodyDetailItem(OptionDR, RowIdentifier, colspanDesc, ref sb);

                        sb.Append("</table></td></tr>");
                    }
                    else
                    {   // 1px row border seperator

                        sb.Append("<tr class='trline'><td colspan='6' class='trline'><img src='" + Server.MapPath(@"~/images/line.gif") + "' width='845' height='17' alt='' /></td></tr>");

                        PrintEstimateBodyDetailItem(OptionDR, RowIdentifier, colspanDesc, ref sb);
                    }

                }
            }
            sb.Append("</table><div class='clear'>&nbsp;</div>");

            // replace the tokens
            string FinalHTML = pdftemplate.Replace("$estimatebodytoken$", sb.ToString());
            int theID = theDoc.AddImageHtml(FinalHTML);
            string[] sortorderarray = recipientsortorder.Split(',');
            string[] recipientarray = recipients.Split(',');
            string[] methodarray = methods.Split(',');

            // Add the last page.
            while (true)
            {
                if (!theDoc.Chainable(theID))
                {
                    // add contract notes based on state, brand and effectivedate-- old contract notes code
                    string filename = getContractNotes(estimateRevisionId.ToString());
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

                    string customerInfo = GetHeaderInformation(estimateid);
                    string[] headervars = customerInfo.Split('|');
                    string tempStr;
                    string temp = headervars[1].Replace("<br>", "|");
                    string[] contacts = temp.Split('|');

                    //theDoc.Page = theDoc.AddPage();

                    //if (Session["OriginalLogOnState"].ToString() != "QLD" && Session["OriginalLogOnState"].ToString() != "NSW")
                    //{
                        tempStr = string.Empty; //"<table width='100%' cellspadding=0 cellspacing=0 border=0>";
                        for (int i = 0; i < contacts.Length; i++)
                        {
                            tempStr = tempStr + "<tr height='20px'><td></td></tr>";
                            if (docusignintegration == "0")
                            {
                                   
                                if (Session["OriginalLogOnState"].ToString() == "QLD")
                                {
                                    tempStr = tempStr + "<tr valign='top'><td  width='0%'>&nbsp;</td><td width='43%' align=left>" + contacts[i] + "</td><td width='57%' align=left>Sign Here " + "<span style='color:#FFFFFF'>/S" + (i + 1).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + "<span style='color:#FFFFFF'>/D" + (i + 1).ToString() + "/</span>" + "______________________________</td></tr>";
                                }
                                else
                                {
                                    tempStr = tempStr + "<tr valign='top'><td  width='5%'>&nbsp;</td><td width='43%' align=left>" + contacts[i] + "</td><td width='57%' align=left>Sign Here " + "<span style='color:#FFFFFF'>/S" + (i + 1).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + "<span style='color:#FFFFFF'>/D" + (i + 1).ToString() + "/</span>" + "______________________________</td></tr>";
                                }
                            }
                            else
                            {
                                for (int k = 0; k < sortorderarray.Length; k++)
                                {
                                    if ((i + 1).ToString() == sortorderarray[k] && methodarray[i] != "3")
                                    {
                                        if (Session["OriginalLogOnState"].ToString() == "QLD")
                                        {
                                            tempStr = tempStr + "<tr valign='top'><td  width='0%'>&nbsp;</td><td width='43%' align=left>" + contacts[i] + "</td><td width='57%' align=left>Sign Here " + "<span style='color:#FFFFFF'>/S" + (i + 1).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + "<span style='color:#FFFFFF;'>/D" + (i + 1).ToString() + "/</span>" + "______________________________</td></tr>";
                                        }
                                        else
                                        {
                                            tempStr = tempStr + "<tr valign='top'><td  width='5%'>&nbsp;</td><td width='43%' align=left>" + contacts[i] + "</td><td width='57%' align=left>Sign Here " + "<span style='color:#FFFFFF'>/S" + (i + 1).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;" + "<span style='color:#FFFFFF;'>/D" + (i + 1).ToString() + "/</span>" + "______________________________</td></tr>";
                                        }
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
                                tempStr = tempStr + "<tr height='20px'><td></td></tr>";
                                if (int.Parse(sortorderarray[m]) > 100 && methodarray[m] != "3")
                                {
                                    if (Session["OriginalLogOnState"].ToString() == "QLD" || Session["OriginalLogOnState"].ToString() == "SA")
                                    {
                                        tempStr = tempStr + "<tr valign='top'><td  width='0%'></td><td align=left>" + recipientarray[m] + " (Builder)</td><td align=left>Sign Here " + "<span style='color:#FFFFFF'>/B" + (int.Parse(sortorderarray[m]) - 100).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; " + "<span style='color:#FFFFFF;'>/BD" + (int.Parse(sortorderarray[m]) - 100).ToString() + "/</span>" + "______________________________</td></tr>";
                                    }
                                    else
                                    {
                                        tempStr = tempStr + "<tr valign='top'><td  width='5%'></td><td align=left>" + recipientarray[m] + " (Builder)</td><td align=left>Sign Here " + "<span style='color:#FFFFFF'>/B" + (int.Parse(sortorderarray[m]) - 100).ToString() + "/</span>" + "______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; " + "<span style='color:#FFFFFF;'>/BD" + (int.Parse(sortorderarray[m]) - 100).ToString() + "/</span>" + "______________________________</td></tr>";
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (((Session["OriginalLogOnState"].ToString() == "QLD" || Session["OriginalLogOnState"].ToString() == "SA") && (estimateRevision_documenttype.ToLower().Trim().Contains("contract") || estimateRevision_documenttype.ToLower().Trim().Contains("variation"))))
                            {
                                tempStr = tempStr + "<tr valign='top'><td  colspan='3'></td></tr>";
                                tempStr = tempStr + "<tr height='20px'><td></td></tr>";
                                if (Session["OriginalLogOnState"].ToString() == "QLD" )
                                {
                                    tempStr = tempStr + "<tr valign='top'><td  width='0%'></td><td align=left>Builder</td><td align=left>Sign Here <span style='color:#FFFFFF'>/B2/</span>______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style='color:#FFFFFF;'>/BD2/</span> ______________________________</td></tr>";
                                }
                                else
                                {
                                    tempStr = tempStr + "<tr valign='top'><td  width='5%'></td><td align=left>Builder</td><td align=left>Sign Here <span style='color:#FFFFFF'>/B2/</span>______________________________&nbsp;<br><br>Date&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<span style='color:#FFFFFF;'>/BD2/</span> ______________________________</td></tr>";
                                }
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

                        //tempStr = tempStr + "</table>";
                      

                    // Add the Disclaimer/Acknowledgements body text.
                    Doc theDoc3 = new Doc();
                    theDoc3.MediaBox.SetRect(0, 0, 595, 842);
                    theDoc3.Rect.String = "80 35 575 822";
                    if (Session["OriginalLogOnState"].ToString() =="VIC")
                    {
                        theDoc3.Color.Color = System.Drawing.Color.FromArgb(231, 232, 231);
                        theDoc3.Rect.Bottom = 2;
                        theDoc3.Rect.Left = 50;
                        theDoc3.Rect.Width = 500;
                        theDoc3.Rect.Height = 15;
                        theDoc3.Rect.Move(16, 800);
                        theDoc3.FillRect();

                        // Add the Disclaimer/Acknowledgements header.
                        theDoc3.Color.Color = System.Drawing.Color.FromArgb(129, 130, 132);
                        theDoc3.Font = theDoc3.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                        theDoc3.TextStyle.Size = 12;
                        theDoc3.TextStyle.Bold = false;
                        theDoc3.Rect.Pin = 0;
                        theDoc3.Rect.Position(80, 800);
                        theDoc3.Rect.Width = 200;
                        theDoc3.Rect.Height = 15;
                        theDoc3.TextStyle.LeftMargin = 50;
                        theDoc3.AddText(Common.PRINTPDF_DISCLAIMER_HEADER);
                    }

                    theDoc3.Color.Color = System.Drawing.Color.Black;
                    theDoc3.Font = theDoc3.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                    theDoc3.TextStyle.Size = Common.PRINTPDF_DISCLAIMER_FONTSIZE;
                    theDoc3.TextStyle.Bold = false;
                    theDoc3.Rect.Pin = 0;
                    theDoc3.Rect.Position(20, 35);
                    theDoc3.Rect.Width = 500;
                    theDoc3.Rect.Height = 760;
                    theDoc3.TextStyle.LeftMargin = 50;

                    string disclaimer = Common.getDisclaimer(estimateRevisionId.ToString(), Session["OriginalLogOnState"].ToString(), estimateRevision_internalVersion).Replace("$Token$", tempStr);
                            disclaimer = disclaimer.Replace("$printdatetoken$", DateTime.Now.ToString("dd/MMM/yyyy"));
                            disclaimer = disclaimer.Replace("$logoimagetoken$", Server.MapPath("~/images/metlog.jpg"));

                            DateTime dd;
                            try
                            {
                                dd = DateTime.Parse(pestimatecreatedate);
                                pestimatecreatedate = dd.ToString("dd MMMM yyyy");
                            }
                            catch
                            {
                                pestimatecreatedate = DateTime.Now.ToString("dd MMMM yyyy");
                            }
                            disclaimer = disclaimer.Replace("$estimatecreatedtoken$", pestimatecreatedate);
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
                                //theDoc3.AddFont("Century Gothic");
                                int docid=theDoc3.AddImageHtml(disclaimer);
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
                        theDoc.Font = theDoc.AddFont(Common.ORDERFORM_FONT);
                        if (estimateRevision_internalVersion.ToUpper() == "INTERNAL" || estimateRevision_internalVersion.ToUpper() == "FINAL" || estimateRevision_internalVersion.ToUpper() == "STUDIOM")
                        {
                            theDoc.TextStyle.Bold = true;
                            theDoc.Color.String = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR;
                            theDoc.Color.Alpha = Common.ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE;

                            if (estimateRevision_internalVersion.ToUpper() == "FINAL")
                            {
                                theDoc.FontSize = Common.ORDERFORM_ESTIMATE_FINAL_STAMP_FONTSIZE;
                                theDoc.Pos.String = "55 400";
                                theDoc.Transform.Reset();
                                theDoc.Transform.Rotate(55, 75, 400);
                                theDoc.AddText("");
                                theDoc.FontSize = Common.ESTIMATE_FINAL_CONSTRUCTION_COPY_STAMP_FONTSIZE;
                                theDoc.Pos.String = "0 400";
                                theDoc.Transform.Reset();
                                theDoc.Transform.Rotate(45, 200, 400);
                                theDoc.AddText(ESTIMATE_FINALCONSTRUCTIONCOPY_STAMP);
                            }
                            else
                            {
                                theDoc.FontSize = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_FONTSIZE;
                                theDoc.Pos.String = "30 300";
                                theDoc.Transform.Reset();
                                theDoc.Transform.Rotate(45, 50, 300);
                                theDoc.AddText(ESTIMATE_INTERNALCOPY_STAMP);
                            }
                            theDoc.FontSize = Common.ESTIMATE_DATE_TIME_STAMP_FONTSIZE;
                            theDoc.Pos.String = "25 400";
                            theDoc.Transform.Reset();
                            theDoc.Transform.Rotate(45, 275, 400);
                            theDoc.AddText(DateTime.Now.ToString("dd/MM/yyyy hh:mm tt"));
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
                        theDoc.Font = theDoc.AddFont(Common.ORDERFORM_FONT);
                        if (!string.IsNullOrWhiteSpace(underReview))
                        {
                            int anchorXRotate = 0;
                            theDoc.Color.String = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR;
                            theDoc.Color.Alpha = Common.ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE;
                            if (estimateRevision_internalVersion.ToUpper() == "FINAL")
                            {
                                theDoc.FontSize = Common.ESTIMATE_FINAL_CONSTRUCTION_COPY_STAMP_FONTSIZE;
                                theDoc.Pos.String = "40 400";
                                anchorXRotate = 345;        
                            }
                            else if (estimateRevision_internalVersion.ToUpper() == "INTERNAL")
                            {
                                theDoc.FontSize = fontsize;
                                theDoc.Pos.String = "40 400";
                                anchorXRotate = 345;
                            }
                            else
                            {
                                theDoc.FontSize = fontsize;
                                theDoc.Pos.String = "200 400";
                                anchorXRotate = 300;
                            }

                            theDoc.Transform.Reset();
                            theDoc.Transform.Rotate(45, anchorXRotate, 400);

                            theDoc.AddHtml(underReview);
                        }
                        theDoc.TextStyle.Bold = false;

                        theDoc.Rect.Pin = 0;
                        theDoc.Rect.Position(50, 10);
                        theDoc.Rect.Width = 700;
                        theDoc.Rect.Height = 20;
                        theDoc.TextStyle.LeftMargin = 15;
                        theDoc.Color.String = "40 40 40";
                        theDoc.TextStyle.Size = 6;
                        theDoc.Transform.Reset();

                        if (Session["OriginalLogOnState"].ToString() == "QLD")
                        {
                            if (estimateRevision_documenttype.ToLower().Trim().Contains("contract"))
                                theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   <br>E&EO&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Builder Initials:_____________________&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer Initials:________________________");
                            else
                                theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   <br>E&EO&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;");
                            //theDoc.AddHtml(LotAddress.ToUpper() + "<br>Contract No: " + BCContractnumber + " Revision: " + estimateRevision_revisionNumber.ToString() + "(" + estimateRevision_revisionTypeBrief + ")   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Builder Initials:_____________________&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer Initials:________________________<br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;E&OE");
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
                        theDoc.Rect.Height = 22;
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
        public void PrintEstimateBodyDetailItem(DataRow OptionDR, int RowIdentifier, int colspanDesc, ref StringBuilder sb)
        {
            StringBuilder tempdesc = new StringBuilder();
            string tempimage = string.Empty;

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

            int packageType = -1;
            if (houseAndLandPkgContract)
            {
                if (OptionDR["StandardPackageInclusion"].ToString() != "")
                {
                    packageType = Int16.Parse(OptionDR["StandardPackageInclusion"].ToString());
                }
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
                if ((estimateRevision_internalVersion == "INTERNAL" || estimateRevision_internalVersion == "FINAL") && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                    tempdesc.Append("<br><br> <b>Internal Notes:</b> " + OptionDR["INTERNALDESC"].ToString());

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

                    sb.Append("<tr><td valign='top' class='tdnumber' style='page-break-inside:avoid;'>" + RowIdentifier.ToString() + ".</td>");
                    if (estimateRevision_internalVersion == "FINAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>" + tempimage + "</span></span></div></td>");
                    else if (includeProductNameAndCode == "True" || estimateRevision_internalVersion == "INTERNAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</span></span></div></td>");


                    if (estimateRevision_state.ToLower() != "qld" || estimateRevision_internalVersion == "FINAL")
                    {
                        sb.Append("<td colspan='" + colspanDesc + "' valign='top'style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                        if (includeUOMAndQuantity == "True")
                        {
                            sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["uom"].ToString() + "</span></div></td>");
                            sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["quantity"].ToString() + "</span></div></td>");
                        }
                    }
                    else
                    {
                        sb.Append("<td valign='top' colspan='3' style='page-break-inside:avoid;'>><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                    }
                    if (estimateRevision_internalVersion != "FINAL")
                    {
                        if (printversion == "LUMPSUM")
                        {
                            sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>&nbsp;</span></span><br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                        }
                        else
                        {
                            sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["printprice"].ToString() + "</span></span><br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                        }
                    }
                }
                else
                {

                    sb.Append("<tr><td valign='top' class='tdnumber' style='page-break-inside:avoid;'>" + RowIdentifier.ToString() + ".</td>");
                    if (estimateRevision_internalVersion == "FINAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>" + tempimage + "</div></td>");
                    else if (includeProductNameAndCode == "True" || estimateRevision_internalVersion == "INTERNAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</div></td>");

                    if (estimateRevision_state.ToLower() != "qld" || estimateRevision_internalVersion == "FINAL")
                    {
                        if (includeProductNameAndCode == "False" || estimateRevision_internalVersion == "FINAL")
                            sb.Append("<td valign='top' style='page-break-inside:avoid;' colspan=" + colspanDesc + "><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                        else
                            sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");

                        if (includeUOMAndQuantity == "True")
                        {
                            sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["uom"].ToString() + "</div></td>");
                            sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["quantity"].ToString() + "</div></td>");
                        }
                    }
                    else
                    {
                        if (includeProductNameAndCode == "True")
                            sb.Append("<td valign='top' style='page-break-inside:avoid;' colspan='" + colspanDesc + "'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                        else
                            sb.Append("<td valign='top' style='page-break-inside:avoid;' colspan='" + colspanDesc + "'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                    }

                    if (estimateRevision_internalVersion != "FINAL")
                    {
                        if (printversion != "LUMPSUM")
                        {
                            if (estimateRevision_state.ToLower() != "qld")
                            {
                                sb.Append("<td valign='top'><div class='tdinner'>"+ OptionDR["promotionitemdisplaycode"].ToString().ToUpper() + @"<br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                            }
                            else
                            {
                                sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["promotionitemdisplaycode"].ToString().ToUpper() + @"<br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                            }
                        }

                    }
                }

            }
            else
            {

                tempdesc.Append(Common.ReplaceCRLFByLineBreak(OptionDR["PRODUCTDESCRIPTION"].ToString()));
                if ((OptionDR["ENTERDESC"].ToString().CompareTo("") != 0) && (OptionDR["ENTERDESC"].ToString().Trim().CompareTo("N/A") != 0))
                    tempdesc.Append("<br><br><b>Extra Description:</b> " + OptionDR["ENTERDESC"].ToString());
                if ((estimateRevision_internalVersion == "INTERNAL" || estimateRevision_internalVersion == "FINAL") && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                    tempdesc.Append("<br><br><b>Internal Notes:</b> " + OptionDR["INTERNALDESC"].ToString());


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
                    sb.Append("<tr><td valign='top' class='tdnumber' style='page-break-inside:avoid;'>" + RowIdentifier.ToString() + ".</td>");
                    if (estimateRevision_internalVersion == "FINAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>" + tempimage + "</span></span></div></td>");
                    else if (includeProductNameAndCode == "True" || estimateRevision_internalVersion == "INTERNAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</span></span></div></td>");

                    if (estimateRevision_state.ToLower() != "qld" || estimateRevision_internalVersion == "FINAL")
                    {
                        if (estimateRevision_internalVersion == "FINAL")
                        {
                            sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                        }
                        else
                            sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                        if (includeUOMAndQuantity == "True")
                        {
                            sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["uom"].ToString() + "</span></div></td>");
                            sb.Append("<td valign='top'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["quantity"].ToString() + "</span></div></td>");
                        }
                    }
                    else
                    {
                        sb.Append("<td valign='top' colspan='" + colspanDesc + "' style='page-break-inside:avoid;'><div class='tdinner'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + tempdesc.ToString() + "</span></span></div></td>");
                    }

                    if (printversion == "LUMPSUM")
                    {
                        sb.Append("<td valign='top'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>&nbsp;</span></span><br><img src='" + Server.MapPath(@"~/images/remove.gif") + "'/></td></tr>");
                    }
                    else
                    {
                        if (includeProductNameAndCode == "True" && estimateRevision_internalVersion != "FINAL")
                            sb.Append("<td valign='top'><span style='color:#666666;text-decoration:line-through'><span style='color:black;'>" + OptionDR["printprice"].ToString() + "</span></span><br><img src='" + Server.MapPath(@"~/images/remove.gif") + "'/></td></tr>");
                    }

                }
                else
                {

                    sb.Append("<tr><td valign='top' class='tdnumber' style='page-break-inside:avoid;'>" + RowIdentifier.ToString() + ".</td>");
                    if (estimateRevision_internalVersion == "FINAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>" + tempimage + "</div></td>");
                    else if (includeProductNameAndCode == "True" || estimateRevision_internalVersion == "INTERNAL")
                        sb.Append("<td valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]<br>" + tempimage + "</div></td>");

                    if (estimateRevision_state.ToLower() != "qld" || estimateRevision_internalVersion == "FINAL")
                    {
                        sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                        if (includeUOMAndQuantity == "True")
                        {
                            sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["uom"].ToString() + "</div></td>");
                            sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["quantity"].ToString() + "</div></td>");
                        }
                    }
                    else
                    {
                        sb.Append("<td colspan='" + colspanDesc + "' valign='top' style='page-break-inside:avoid;'><div class='tdinner'>" + tempdesc.ToString() + "</div></td>");
                    }

                    if (estimateRevision_internalVersion != "FINAL")
                    {
                        //if (int.Parse(OptionDR["fkidstandardinclusions"].ToString()) > 0)
                        //{

                        //    sb.Append("<td valign='top'><div class='tdinner'>INCLUDED</div></td></tr>");

                        //}
                        //else
                        //{
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
                                if (estimateRevision_state.ToLower() != "qld")
                                {
                                    string priceDisplay = tempdecimal.ToString("C");
                                    //commented out becasue victoria users want to see the price, we felt every state other than QLD may want to see the price as wel instead of included
                                    //if (packageType >= 0 && packageType <= 4)
                                    //    priceDisplay = "INCLUDED";
                                    sb.Append("<td valign='top'><div class='tdinner'>" + priceDisplay + "</div></td></tr>");
                                }
                                else
                                {
                                    if (OptionDR["productpricedisplaycode"].ToString().ToUpper() == "PR")
                                    {
                                        sb.Append("<td valign='top'><div class='tdinner'>PROMOTION<br><img src='" + Server.MapPath(@"~/images/star.gif") + "'/></div></td></tr>");
                                    }
                                    else
                                    {
                                        //if (packageType >= 0 && packageType <= 4)
                                        //    sb.Append("<td valign='top'><div class='tdinner'>INCLUDED</div></td></tr>");
                                        //else
                                        sb.Append("<td valign='top'><div class='tdinner'>" + String.Format("{0:C}", tempdecimal) + "</div></td></tr>");
                                    }
                                }
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
                                if (estimateRevision_internalVersion != "FINAL")
                                {
                                    if (OptionDR["printprice"].ToString().ToUpper() == "PROMOTION")
                                    {
                                        sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["printprice"].ToString() + "<br><img src='" + Server.MapPath(@"~/images/star.gif") + "'</div></td></tr>");
                                    }
                                    else
                                    {
                                        sb.Append("<td valign='top'><div class='tdinner'>" + OptionDR["printprice"].ToString() + "</div></td></tr>");
                                    }
                                }
                            }
                        }

                        //}
                    }
                }
            }
        }

        public void SavePDF(Doc theDoc)
        {
            Random R = new Random();
            byte[] theData = theDoc.GetData();
            //theDoc.Save("c:\\temp\\docusign.pdf");
            CommonFunction cf= new CommonFunction();

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
                    emailbody,
                    methods,envelopeid,
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
        //private DocuSignWebService.SignerParameter[] GetSignerParameters(string action, string recipientname, string recipientemail, string sortorders, string routingorder)
        //{

        //    string[] tempname = recipientname.Split(',');
        //    string[] tempemail = recipientemail.Split(',');
        //    string[] temporder = routingorder.Split(',');
        //    string[] tempsortorder = sortorders.Split(',');
        //    string[] tempaction = action.Split(',');
        //    DocuSignWebService.SignerParameter[] ls = new DocuSignWebService.SignerParameter[tempname.Length];

        //    for (int i = 0; i < tempname.Length; i++)
        //    {
        //        DocuSignWebService.SignerParameter sp = new DocuSignWebService.SignerParameter();
        //        sp.SignerEmail = tempemail[i];
        //        sp.SignerName = tempname[i];
        //        sp.RoutingOrder = int.Parse(temporder[i]);
        //        sp.SortOrder = int.Parse(tempsortorder[i]);
        //        sp.Action = tempaction[i];
        //        ls[i] = sp;
        //    }


        //    return ls;
        //}
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

        public string getContractNotes(string estimaterevisonid)
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
                    estimateRevision_homeName = ds.Tables[0].Rows[0]["HomeName"].ToString();
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
                    estimateRevision_documenttype=ds.Tables[0].Rows[0]["documenttype"].ToString();
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