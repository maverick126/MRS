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

using WebSupergoo.ABCpdf5;
using WebSupergoo.ABCpdf5.Objects;
using WebSupergoo.ABCpdf5.Atoms;

using Metricon.Silverlight.MetriconRetailSystem.Host.MetriconSalesWebService;
using Metricon.Silverlight.MetriconRetailSystem.Host.Internal;

namespace Metricon.Silverlight.MetriconRetailSystem.Host
{
    //public class Common
    //{
    //    public const string SELECTED_USER_SESSION_NAME = "";
    //    public const string PRINTPDF_DEFAULT_FONT = "Arial";
    //    public const double PRINTPDF_DEFAULT_FONTSIZE = 10;
    //    public const string PRINTPDF_DISCLAIMER_HEADER = "Agreement:";
    //    public const double PRINTPDF_DISCLAIMER_FONTSIZE = 8.3;
    //    public const string PRINTPDF_CUSTOMER_LABEL = "Customer:";
    //    public const string PRINTPDF_SIGNATURE_LABEL = "Sign Here : ________________";
    //    public const int ESTIMATE_STATUS_EXPIRED = 5;
    //    public const string ORDERFORM_FONT = "Arial";
    //    public const int ORDERFORM_ESTIMATE_EXPIRED_STAMP_FONTSIZE = 80;
    //    public const string ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR = "255 0 0"; // Red
    //    public const int ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE = 200;
    //    public const string ORDERFORM_ESTIMATE_EXPIRED_STAMP = "Expired";
    //    public const string ORDERFORM_PAGENUMBER_COORDINATES = "30 20 565 32";
    //    public const int ORDERFORM_PAGENUMBER_FONTSIZE = 10;
    //    public static int ESTIMATE_STATUS_ACCEPTED = 3;


    //    public static int ConvertStringToIntIfFailToZero(string inputstring)
    //    {
    //        int outputint = 0;
    //        try
    //        {
    //            outputint = Int32.Parse(inputstring);
    //        }
    //        catch (Exception)
    //        {
    //            outputint = 0;
    //        }
    //        return outputint;
    //    }


    //    public static string getDisclaimer(string revisiontypeid, string state, string regionid)
    //    {
    //        string result = "";
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_SalesEstimate_GetMRSDisclaimer");
    //        Cmd.Parameters["@revisiontypeid"].Value = revisiontypeid;
    //        Cmd.Parameters["@state"].Value = state;
    //        Cmd.Parameters["@regionid"].Value = regionid;
    //        DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

    //        if (ds.Tables[0].Rows.Count == 1)
    //        {
    //            result = ds.Tables[0].Rows[0]["agreement"].ToString();
    //        }

    //        return result;
    //    }

    //    /// <summary>
    //    /// Replaces the CRLF by line break before it can be displayed properly in the browser.
    //    /// </summary>
    //    /// <param name="text">The text.</param>
    //    /// <returns></returns>
    //    public static string ReplaceCRLFByLineBreak(string text)
    //    {
    //        StringBuilder newText = new StringBuilder();

    //        try
    //        {
    //            newText.Append(text);

    //            newText.Replace("\r\n", @"<br />");
    //            newText.Replace("\n", @"<br />");
    //        }
    //        catch
    //        {
    //            // Nothing.
    //        }

    //        return newText.ToString();
    //    }
    //}

    //public class DBConnection
    //{
    //    public SqlConnection conn = null;
    //    public SqlDataAdapter Adapter;
    //    public string ConnectionString = "";
    //    public DataSet DS;
    //    public int timeout;

    //    public DBConnection()
    //    {
    //        timeout = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["SQLCommandTimeOut"]);
    //    }
    //    public DataSet ExecuteQuery(String Query)
    //    {
    //        try
    //        {

    //            string ConnectionString = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ConnectionString;
    //            conn = new SqlConnection(ConnectionString);
    //            conn.Open();

    //            Adapter = new SqlDataAdapter(Query, conn);
    //            DS = new DataSet();
    //            Adapter.Fill(DS);
    //            CloseDBConnection();
    //            return DS;
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }
    //    }
    //    // this function return a dataset when user call a stored procdure with parameters

    //    public DataSet ExecuteSQLQuery(string sqlStoredProcedure, SqlParameter[] myParameters)
    //    {

    //        try
    //        {
    //            string ConnectionString = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ConnectionString;
    //            conn = new SqlConnection(ConnectionString);
    //            conn.Open();

    //            Adapter = new SqlDataAdapter(sqlStoredProcedure, conn);
    //            DS = new DataSet();

    //            //mySQLAdapter.SelectCommand = new SqlCommand(sqlStoredProcedure);
    //            Adapter.SelectCommand.Connection = conn;
    //            Adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
    //            Adapter.SelectCommand.CommandTimeout = timeout;


    //            foreach (SqlParameter tempParam in myParameters)
    //            {

    //                Adapter.SelectCommand.Parameters.Add(tempParam);
    //            }

    //            Adapter.Fill(DS);


    //        }
    //        catch (Exception)
    //        {
    //            conn.Close();
    //        }
    //        finally
    //        {
    //            //if there are no tables add default table
    //            if (DS.Tables.Count == 0)
    //            {
    //                DS.Tables.Add(new DataTable("NoRecords"));
    //            }
    //            conn.Close();
    //            conn.Dispose();
    //        }
    //        return DS;
    //    }
    //    /// <summary>
    //    /// this method is used to setup the connection string and the storedprocedure to be executed by the user
    //    /// </summary>
    //    /// <param name="SP"></param>
    //    /// <returns></returns>
    //    public SqlCommand ExecuteStoredProcedure(string SP)
    //    {
    //        SqlCommand Cmd = null;
    //        try
    //        {
    //            if (conn == null)
    //            {
    //                string ConnectionString = ConfigurationManager.ConnectionStrings["PMO006ConnectionString"].ConnectionString;
    //                conn = new SqlConnection(ConnectionString);
    //                conn.Open();
    //            }
    //            Cmd = new SqlCommand(SP, conn);
    //            Cmd.CommandTimeout = timeout;
    //            SqlDataAdapter Adapter = new SqlDataAdapter(Cmd);
    //            Cmd.CommandType = CommandType.StoredProcedure;
    //            SqlCommandBuilder.DeriveParameters(Cmd);
    //        }
    //        catch (Exception)
    //        {

    //        }
    //        return Cmd;
    //    }
    //    /// <summary>
    //    /// this method will only be called when the user want to insert a new record in the database
    //    /// </summary>
    //    /// <param name="InsertCommand"></param>
    //    /// <returns></returns>
    //    public int InsertStoredProcedure(SqlCommand InsertCommand)
    //    {
    //        InsertCommand.CommandTimeout = timeout;
    //        int RowsAffected = InsertCommand.ExecuteNonQuery();
    //        CloseDBConnection();
    //        return RowsAffected;
    //    }
    //    /// <summary>
    //    /// this method will be called to update the values in the database
    //    /// </summary>
    //    /// <param name="UpdateCommand"></param>
    //    /// <returns></returns>
    //    public int UpdateStoredProcedure(SqlCommand UpdateCommand)
    //    {
    //        UpdateCommand.CommandTimeout = timeout;
    //        int RowsAffected = UpdateCommand.ExecuteNonQuery();
    //        CloseDBConnection();
    //        return RowsAffected;
    //    }

    //    public DataSet SelectSqlStoredProcedure(SqlCommand SelectCommand)
    //    {
    //        try
    //        {
    //            Adapter = new SqlDataAdapter(SelectCommand);
    //            SelectCommand.CommandTimeout = timeout;
    //            DS = new DataSet();
    //            Adapter.Fill(DS);
    //            CloseConnection();
    //            return DS;
    //        }
    //        catch (Exception)
    //        {
    //            throw;
    //        }
    //    }

    //    public void DisposeObjects()
    //    {
    //        Adapter.Dispose();
    //        Adapter = null;
    //        DS.Dispose();
    //        DS = null;
    //        CloseDBConnection();
    //    }

    //    public void CloseConnection()
    //    {
    //        CloseDBConnection();
    //    }

    //    public void CloseDBConnection()
    //    {
    //        try
    //        {
    //            conn.Close();
    //        }
    //        catch (Exception)
    //        {
    //        }
    //    }

    //    ~DBConnection()
    //    {
    //        CloseDBConnection();
    //    }
    //}

    //public class Client
    //{
    //    public Client()
    //    {
    //        //
    //        // TODO: Add constructor logic here
    //        //
    //    }

    //    public static DataSet GetEstimateAcceptedForCustomer(int BCCustomerCode, int contractNo)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_CheckIsEstimateAcceptedForCustomer2");
    //        Cmd.Parameters["@customerCode"].Value = BCCustomerCode;
    //        Cmd.Parameters["@contractNo"].Value = contractNo;
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }
    //    public static DataSet GetEstimateAcceptedByOpportunity(string opportunityid)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_SQSCRM_CheckIsEstimateAcceptedByOpportunity");
    //        Cmd.Parameters["@opportunityid"].Value = opportunityid;
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }
    //    public static DataSet GetUserList(string username)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserList");
    //        Cmd.Parameters["@currentusername"].Value = username;
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }

    //    public static DataSet GetUserListFromUsercode(string usercode)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserListFromUsercode");
    //        Cmd.Parameters["@currentusercode"].Value = usercode;
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }

    //    public static DataSet GetCustomerClientHouseDetails(string bccontractnumber, string bccustomernumber)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetCustomerClientHouseDetails");
    //        Cmd.Parameters["@bccontractnumber"].Value = Common.ConvertStringToIntIfFailToZero(bccontractnumber);
    //        Cmd.Parameters["@bccustomernumber"].Value = Common.ConvertStringToIntIfFailToZero(bccustomernumber);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }

    //    public static DataSet GetUserListForAdmin()
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserListForAdmin");
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }

    //    public static DataSet GetUserDefaultRoleID(string fullname)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetUserDefautRoleID");
    //        Cmd.Parameters["@username"].Value = fullname;
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }
    //    public static DataSet GetUserListForManagers(string userid, string roleid)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetUserListForManagers");
    //        Cmd.Parameters["@userid"].Value = userid;
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }

    //    public static DataSet GetAllSalesConsultants(string stateid)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetAllSalesConsultants");
    //        Cmd.Parameters["@fkstateid"].Value = Int16.Parse(stateid);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }
    //    public static DataSet GetUserRoles(string userid)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetUserRoles");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return DS;
    //    }
    //    public static bool userInMultipleRoles(string userid)
    //    {
    //        bool result = false;
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserInMultipleRoles");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
    //        {
    //            result = false;
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //    public static bool isUserAdmin(string userid)
    //    {
    //        bool result = false;
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserAdmin");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
    //        {
    //            result = false;
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //    public static bool isUserRoleAdmin(string userid, string roleid)
    //    {
    //        bool result = false;
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserRoleAdmin");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
    //        Cmd.Parameters["@roleid"].Value = Common.ConvertStringToIntIfFailToZero(roleid);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
    //        {
    //            result = false;
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //    public static bool isUserRoleManager(string userid, string roleid)
    //    {
    //        bool result = false;
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserRoleManager");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
    //        Cmd.Parameters["@roleid"].Value = Common.ConvertStringToIntIfFailToZero(roleid);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
    //        {
    //            result = false;
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //    public static bool isUserRoleCanSeeAllConsultant(string userid, string roleid)
    //    {
    //        bool result = false;
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserRoleCanSeeAllConsultant");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);
    //        Cmd.Parameters["@roleid"].Value = Common.ConvertStringToIntIfFailToZero(roleid);
    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
    //        {
    //            result = false;
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //    public static bool isUserPackageAdmin(string userid)
    //    {
    //        bool result = false;
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserPackageAdmin");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);

    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
    //        {
    //            result = false;
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //    public static bool isUserSalesEstimator(string userid)
    //    {
    //        bool result = false;
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_isUserSalesEstimator");
    //        Cmd.Parameters["@userid"].Value = Common.ConvertStringToIntIfFailToZero(userid);

    //        DataSet DS = DBCon.SelectSqlStoredProcedure(Cmd);
    //        if (DS.Tables[0].Rows[0]["result"].ToString() == "0")
    //        {
    //            result = false;
    //        }
    //        else
    //        {
    //            result = true;
    //        }
    //        return result;
    //    }
    //}

    //public class Estimate
    //{
    //    public Estimate()
    //    {
    //        //
    //        // TODO: Add constructor logic here
    //        //
    //    }

    //    public static double GetUpgradesTotal(int Estimateid)
    //    {
    //        try
    //        {
    //            DBConnection DB = new DBConnection();
    //            SqlCommand UpgradesTotalCmd = DB.ExecuteStoredProcedure("sp_GetUpgradeTotalForEstimate");
    //            UpgradesTotalCmd.Parameters["@estimateid"].Value = Estimateid;
    //            DataSet UpgradesTotalDS = DB.SelectSqlStoredProcedure(UpgradesTotalCmd);
    //            if (UpgradesTotalDS != null)
    //                if (UpgradesTotalDS.Tables[0].Rows.Count > 0)
    //                {
    //                    DataRow UpgradeRow = UpgradesTotalDS.Tables[0].Rows[0];
    //                    if ((UpgradeRow["upgradetotal"] != null) && (UpgradeRow["upgradetotal"].ToString() != string.Empty))
    //                        return double.Parse(UpgradeRow["upgradetotal"].ToString());
    //                    else return 0;
    //                }
    //            return 0;
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    public static double GetHomeBasePrice(int Estimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand HomePriceCmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
    //        HomePriceCmd.Parameters["@estimateid"].Value = Estimateid;
    //        DataSet HomePriceDS = DB.SelectSqlStoredProcedure(HomePriceCmd);
    //        if (HomePriceDS != null)
    //            if (HomePriceDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow HomePriceRow = HomePriceDS.Tables[0].Rows[0];
    //                return double.Parse(HomePriceRow["homesellprice"].ToString());
    //            }
    //        return 0;
    //    }

    //    public static double GetLandPrice(int Estimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand HomePriceCmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
    //        HomePriceCmd.Parameters["@estimateid"].Value = Estimateid;
    //        DataSet HomePriceDS = DB.SelectSqlStoredProcedure(HomePriceCmd);
    //        if (HomePriceDS != null)
    //            if (HomePriceDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow HomePriceRow = HomePriceDS.Tables[0].Rows[0];
    //                return double.Parse(HomePriceRow["landprice"].ToString());
    //            }
    //        return 0;
    //    }
    //    public static double GetSiteWorksTotalForEstimate(int Estimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand SiteWorksCmd = DB.ExecuteStoredProcedure("sp_GetSiteWorksTotalForEstimate");
    //        SiteWorksCmd.Parameters["@estimateid"].Value = Estimateid;
    //        DataSet SiteWorksDS = DB.SelectSqlStoredProcedure(SiteWorksCmd);
    //        if (SiteWorksDS != null)
    //            if (SiteWorksDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow SiteWorksRow = SiteWorksDS.Tables[0].Rows[0];
    //                if (SiteWorksRow["siteworktotal"] != null && SiteWorksRow["siteworktotal"].ToString() != string.Empty)
    //                    return double.Parse(SiteWorksRow["siteworktotal"].ToString());
    //                else
    //                    return 0;
    //            }
    //        return 0;
    //    }


    //    public static DataSet GetHomePriceBrandAndHomeName(int Estimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand HPBHCmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
    //        HPBHCmd.Parameters["@estimateid"].Value = Estimateid;
    //        DataSet HPBHDS = DB.SelectSqlStoredProcedure(HPBHCmd);
    //        return HPBHDS;
    //    }

    //    public static double GetDOLOptionTotal(int Estimateid, int HomeDisplayid)
    //    {
    //        try
    //        {
    //            DBConnection DB = new DBConnection();
    //            SqlCommand DolOptionTotalCmd = DB.ExecuteStoredProcedure("sp_GetDOLOptionTotal");
    //            DolOptionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
    //            DolOptionTotalCmd.Parameters["@homedisplayid"].Value = HomeDisplayid;

    //            DataSet DolOptionTotalDS = DB.SelectSqlStoredProcedure(DolOptionTotalCmd);
    //            if (DolOptionTotalDS != null)
    //                if (DolOptionTotalDS.Tables[0].Rows.Count > 0)
    //                {
    //                    DataRow DolOptionTotalRow = DolOptionTotalDS.Tables[0].Rows[0];
    //                    if (DolOptionTotalRow["DOLTotal"].ToString() != String.Empty || DolOptionTotalRow["DOLTotal"] != null || DolOptionTotalRow["DOLTotal"].ToString() != "")
    //                        return double.Parse(DolOptionTotalRow["DOLTotal"].ToString());
    //                }
    //            return 0;
    //        }
    //        catch
    //        {
    //            return 0;
    //        }
    //    }

    //    public static double GetPromotionTotalForEstimate(int Estimateid,
    //                                                      int EstimateHouseid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand PromotionTotalCmd = DB.ExecuteStoredProcedure("sp_GetPrmotionTotalForEstimate");
    //        PromotionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
    //        PromotionTotalCmd.Parameters["@estimatehouseid"].Value = EstimateHouseid;
    //        DataSet PromotionTotalDS = DB.SelectSqlStoredProcedure(PromotionTotalCmd);

    //        if (PromotionTotalDS != null)
    //            if (PromotionTotalDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow PromotionRow = PromotionTotalDS.Tables[0].Rows[0];

    //                if ((PromotionRow["promotiontotal"] != null) && (PromotionRow["promotiontotal"].ToString() != string.Empty))
    //                    return double.Parse(PromotionRow["promotiontotal"].ToString());
    //                else return 0;
    //            }

    //        return 0;
    //    }

    //    public static DataSet GetPromotionTotalForEstimateAsDataSet(int Estimateid,
    //                                                  int EstimateHouseid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand PromotionTotalCmd = DB.ExecuteStoredProcedure("sp_GetPrmotionTotalForEstimate");
    //        PromotionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
    //        PromotionTotalCmd.Parameters["@estimatehouseid"].Value = EstimateHouseid;
    //        DataSet PromotionTotalDS = DB.SelectSqlStoredProcedure(PromotionTotalCmd);

    //        return PromotionTotalDS;
    //    }
    //    /// <summary>
    //    /// This method returns the promotional total and upgrade total for a particular estimate.
    //    /// </summary>
    //    /// <param name="Estimateid">Estimate Id</param>
    //    /// <param name="EstimateHouseid">HouseId for the Estimate</param>
    //    /// <param name="promotionTotal">Returns the total promotional value</param>
    //    /// <param name="upgradeTotal">Returns the total upgrade value</param>
    //    /// <returns>Returns void</returns>
    //    /// <remarks>
    //    /// </remarks>
    //    public static void GetPromotionTotalForEstimate2(int Estimateid,
    //                                                        int EstimateHouseid,
    //                                                        out double promotionTotal,
    //                                                        out double upgradeTotal)
    //    {
    //        promotionTotal = 0;
    //        upgradeTotal = 0;

    //        DBConnection DB = new DBConnection();
    //        SqlCommand PromotionTotalCmd = DB.ExecuteStoredProcedure("sp_GetPrmotionTotalForEstimate");
    //        PromotionTotalCmd.Parameters["@estimateid"].Value = Estimateid;
    //        PromotionTotalCmd.Parameters["@estimatehouseid"].Value = EstimateHouseid;
    //        DataSet PromotionTotalDS = DB.SelectSqlStoredProcedure(PromotionTotalCmd);

    //        if (PromotionTotalDS != null)
    //        {
    //            if (PromotionTotalDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow PromotionRow = PromotionTotalDS.Tables[0].Rows[0];

    //                if ((PromotionRow["upgradetotal"] != null) && (PromotionRow["upgradetotal"].ToString() != string.Empty))
    //                    upgradeTotal = double.Parse(PromotionRow["upgradetotal"].ToString());

    //                if ((PromotionRow["promotiontotal"] != null) && (PromotionRow["promotiontotal"].ToString() != string.Empty))
    //                    promotionTotal = double.Parse(PromotionRow["promotiontotal"].ToString());
    //            }
    //        }

    //        return;
    //    }

    //    public static DataSet GetItemsForSelectedAreaGroup(int areaid, int groupid, int estimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand GIFSAGCmd = DB.ExecuteStoredProcedure("sp_GetItemsForSelectedAreaGroup");
    //        GIFSAGCmd.Parameters["@areaid"].Value = areaid;
    //        GIFSAGCmd.Parameters["@groupid"].Value = groupid;
    //        GIFSAGCmd.Parameters["@estimateid"].Value = estimateid;
    //        DataSet GIFSAGDS = DB.SelectSqlStoredProcedure(GIFSAGCmd);
    //        return GIFSAGDS;
    //    }

    //    public static string GetArea(int areaid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand AreaCmd = DB.ExecuteStoredProcedure("sp_GetArea");
    //        AreaCmd.Parameters["@areaid"].Value = areaid;
    //        DataSet AreaDS = DB.SelectSqlStoredProcedure(AreaCmd);
    //        if (AreaDS != null)
    //            if (AreaDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow AreaRow = AreaDS.Tables[0].Rows[0];
    //                if ((AreaRow["areaname"] != null) && (AreaRow["areaname"].ToString() != string.Empty))
    //                    return AreaRow["areaname"].ToString();
    //                else return "";
    //            }
    //        return "";
    //    }

    //    public static string GetGroup(int groupid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand GroupCmd = DB.ExecuteStoredProcedure("sp_GetGroup");
    //        GroupCmd.Parameters["@groupid"].Value = groupid;
    //        DataSet GroupDS = DB.SelectSqlStoredProcedure(GroupCmd);
    //        if (GroupDS != null)
    //            if (GroupDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow GroupRow = GroupDS.Tables[0].Rows[0];
    //                if ((GroupRow["Groupname"] != null) && (GroupRow["Groupname"].ToString() != string.Empty))
    //                    return GroupRow["Groupname"].ToString();
    //                else return "";
    //            }
    //        return "";
    //    }

    //    public static DataSet GetEstimateView(int estimateid, int viewtype)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_GetEstimateView");
    //        EstimateViewCmd.Parameters["@estimateid"].Value = estimateid;
    //        EstimateViewCmd.Parameters["@viewtype"].Value = viewtype;
    //        DataSet EstimateViewDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);
    //        return EstimateViewDS;
    //    }

    //    public static void SetEstimateToCompleted(int estimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand EstimateCmd = DB.ExecuteStoredProcedure("sp_MarkEstimateToCompleted");
    //        EstimateCmd.Parameters["@estimateid"].Value = estimateid;
    //        int rowsAffected = DB.InsertStoredProcedure(EstimateCmd);
    //    }

    //    public static int CheckEstimateStatus(int sourceestimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand EstimateStatusCmd = DB.ExecuteStoredProcedure("sp_GetEstimateStatus");
    //        EstimateStatusCmd.Parameters["@sourceestimateid"].Value = sourceestimateid;
    //        DataSet EstimateStatusDS = DB.SelectSqlStoredProcedure(EstimateStatusCmd);
    //        if (EstimateStatusDS != null)
    //            if (EstimateStatusDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow EstimateStatusRow = EstimateStatusDS.Tables[0].Rows[0];
    //                return int.Parse(EstimateStatusRow["fkestimatestatusid"].ToString());
    //            }
    //        return 0;
    //    }

    //    public static bool IsEstimateHasDeposit(int sourceestimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand EstimateDepositCmd = DB.ExecuteStoredProcedure("spw_GetEstimateDepositFlag");
    //        EstimateDepositCmd.Parameters["@sourceestimateid"].Value = sourceestimateid;
    //        DataSet EstimateDepositDS = DB.SelectSqlStoredProcedure(EstimateDepositCmd);
    //        if (EstimateDepositDS != null)
    //        {
    //            if (EstimateDepositDS.Tables[0].Rows.Count > 0)
    //            {
    //                DataRow EstimateDepositRow = EstimateDepositDS.Tables[0].Rows[0];
    //                if (EstimateDepositRow["Deposited"].ToString() == "1")
    //                    return true;
    //            }
    //        }
    //        return false;
    //    }

    //    public static DataSet GetInitialInformationForWIPEstimate(int sourceestimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand GIIFWIPECmd = DB.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
    //        GIIFWIPECmd.Parameters["@estimateid"].Value = sourceestimateid;
    //        DataSet GIIFWIPEDS = DB.SelectSqlStoredProcedure(GIIFWIPECmd);
    //        return GIIFWIPEDS;
    //    }

    //    public static DataSet GetEstimateDetailsForPrint(int estimateid)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand GEDFPCmd = DB.ExecuteStoredProcedure("sp_GetEstimateDetailsForPrint");
    //        GEDFPCmd.Parameters["@estimateid"].Value = estimateid;
    //        DataSet GEDFPDS = DB.SelectSqlStoredProcedure(GEDFPCmd);
    //        return GEDFPDS;
    //    }

    //    public static DataSet GetEstimateDetails(int estimateId)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand sqlCmd = DB.ExecuteStoredProcedure("sp_GetEstimateDetails");
    //        sqlCmd.Parameters["@estimateid"].Value = estimateId;

    //        DataSet estimateDetailsDS = DB.SelectSqlStoredProcedure(sqlCmd);

    //        return estimateDetailsDS;
    //    }

    //    public static void SetEsimateActiveStatus(int estimateId, bool active)
    //    {
    //        DBConnection DB = new DBConnection();
    //        SqlCommand EstimateCmd = DB.ExecuteStoredProcedure("sp_SetEstimateActiveStatus");
    //        EstimateCmd.Parameters["@estimateid"].Value = estimateId;
    //        EstimateCmd.Parameters["@active"].Value = active;

    //        int rowsAffected = DB.InsertStoredProcedure(EstimateCmd);
    //    }

    //    public static string GetEffectiveDateForHomePrice(int estimateId)
    //    {
    //        string date = "";
    //        DBConnection DB = new DBConnection();
    //        SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_GetEffectiveDateForHomePrice");
    //        sqlCmd.Parameters["@estimateId"].Value = estimateId;
    //        DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
    //        if (ds != null && ds.Tables[0].Rows.Count > 0)
    //            date = ds.Tables[0].Rows[0]["PriceDate"].ToString();
    //        return date;
    //    }

    //    public static string GetFacadeUpgrade(int estimate_id, string return_column)
    //    {
    //        string facade = "";
    //        DBConnection DB = new DBConnection();
    //        SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_GetFacadeUpgrade");
    //        sqlCmd.Parameters["@estimate_id"].Value = estimate_id;
    //        DataSet ds = DB.SelectSqlStoredProcedure(sqlCmd);
    //        if (ds != null && ds.Tables[0].Rows.Count > 0)
    //        {
    //            if (return_column != "")
    //                facade = ds.Tables[0].Rows[0][return_column].ToString();
    //            else
    //                facade = ds.Tables[0].Rows[0]["ProductName"].ToString();
    //        }
    //        return facade;
    //    }

    //    public static string getRegionIDFromEstimate(string estimateid)
    //    {
    //        string result = "";
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("sp_GetCurrentEstimateIDDetails");
    //        Cmd.Parameters["@estimateid"].Value = Common.ConvertStringToIntIfFailToZero(estimateid);
    //        DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

    //        if (ds.Tables[0].Rows.Count > 0)
    //        {
    //            result = ds.Tables[0].Rows[0]["regionid"].ToString();
    //        }

    //        return result;
    //    }
    //    public static string getHomeStoreyFromEstimate(string estimateid)
    //    {
    //        string result = "";
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetHomeStoreyFromEstimate");
    //        Cmd.Parameters["@estimateid"].Value = estimateid;
    //        DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);

    //        if (ds.Tables[0].Rows.Count > 0)
    //        {
    //            result = ds.Tables[0].Rows[0]["stories"].ToString();
    //        }

    //        return result;
    //    }
    //    public static DataSet getPromotionProductsForEstiamte(string estimateid, string idmultiplepromotion)
    //    {
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetPromotionProductsForEstimate");
    //        Cmd.Parameters["@estimateid"].Value = estimateid;
    //        Cmd.Parameters["@idmultiplepromotion"].Value = idmultiplepromotion;
    //        DataSet ds = DBCon.SelectSqlStoredProcedure(Cmd);
    //        return ds;
    //    }
    //    public static string GetPromotionType(string estimateid)
    //    {
    //        string promotiontype = "7";
    //        DBConnection DBCon = new DBConnection();
    //        SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetPromotionFromEstimate");
    //        Cmd.Parameters["@estimateid"].Value = estimateid;
    //        DataSet DisplayDS = DBCon.SelectSqlStoredProcedure(Cmd);

    //        if (DisplayDS.Tables[0].Rows.Count > 0)
    //        {
    //            promotiontype = DisplayDS.Tables[0].Rows[0]["promotiontypeid"].ToString();
    //        }
    //        return promotiontype;
    //    }
    //}

    //public class Validation
    //{
    //    public Validation()
    //    {
    //        //
    //        // TODO: Add constructor logic here
    //        //
    //    }

    //    public static string NullToString(object o)
    //    {
    //        if (o == null)
    //            return "";
    //        else
    //            return o.ToString();
    //    }

    //    public static string MakeFirstCharCapital(string s)
    //    {
    //        string result;
    //        char[] stringArray;
    //        s = s.ToLower();
    //        stringArray = s.ToCharArray();
    //        char first = stringArray[0];
    //        first = System.Convert.ToChar(first.ToString().ToUpper());
    //        stringArray[0] = first;
    //        result = new String(stringArray);
    //        return result;
    //    }

    //    public static string MakeCapitalAfterSpace(string s)
    //    {
    //        string result;
    //        char[] stringArray;
    //        s = s.Trim();
    //        s = s.ToLower();
    //        stringArray = s.ToCharArray();

    //        for (int x = 0; x < stringArray.Length; x++)
    //        {
    //            if (x == 0)
    //            {
    //                stringArray[x] = stringArray[x].ToString().ToUpper().ToCharArray()[0];
    //            }
    //            else if (stringArray[x] == ' ')
    //            {
    //                x++;
    //                stringArray[x] = stringArray[x].ToString().ToUpper().ToCharArray()[0];

    //            }
    //        }

    //        result = new String(stringArray);
    //        return result;
    //    }

    //    /// <summary>
    //    /// Makes sure:
    //    ///		Returned an uppercase string
    //    ///		Returns "VIC" if parsed in string is empty or null
    //    /// </summary>
    //    /// <param name="s"></param>
    //    /// <returns></returns>
    //    public static string AbbreviatedState(string s)
    //    {
    //        if (s == null || s == string.Empty)
    //        {
    //            return "VIC";
    //        }
    //        return s.ToUpper();
    //    }

    //    public static string CleanEmail(string s)
    //    {
    //        if (s.Length > 0)
    //        {
    //            string result;
    //            result = NullToString(s.Substring(0, s.IndexOf('þ')));
    //            result.Replace("ÿ", "");
    //            return result;
    //        }
    //        else
    //        {
    //            return "";
    //        }
    //    }

    //    public static string GetStringFromColumn(DataRow dr, string columnName)
    //    {
    //        if (dr == null)
    //        {
    //            return "";
    //        }

    //        return dr[columnName].ToString();
    //    }

    //    public static string GetStringFromColumn(DataTable dt, int rowNumber, string columnName)
    //    {
    //        string result = "";
    //        try
    //        {
    //            DataRow dr = dt.Rows[rowNumber];
    //            result = NullToString(dr[columnName]);
    //        }
    //        catch
    //        {
    //            //Do nothing and return an empty string
    //        }

    //        return result;
    //    }
    //}

    public partial class PrintEstimate : System.Web.UI.Page
    {
        private string consultantCode;
        private string consultantName;
        private string brandname;
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
        private string estimateRevision_revisionType;
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
        private int estimateRevision_regionId;
        private int estimateRevision_statusId;
        private string estimateRevision_internalVersion;
        private string ESTIMATE_INTERNALCOPY_STAMP = "Internal Copy";


        protected void Page_Load(object sender, EventArgs e)
        {
            // ===== Added to retrieve Estimate Header from Estimate Revision =====
            estimateRevision_internalVersion = "";

            if (Request.QueryString["type"] != null)
                estimateRevision_internalVersion = Request.QueryString["type"].ToString().ToUpper();

            estimateRevisionId = 0;
            Int32.TryParse(Request.QueryString["EstimateRevisionId"], out estimateRevisionId);

            GetEstimateRevisionHeaderDetails(estimateRevisionId);

            Session["AccountId"] = estimateRevision_accountId;
            Session["OpportunityId"] = estimateRevision_opportunityId;
            Session["OriginalLogOnState"] = estimateRevision_state;
            Session["SelectedRegionID"] = estimateRevision_regionId;

            // ===========

            // ===== Added to call Webservices without Parent Page =====
            MS = new MetriconSales();
            MS.Url = Utilities.GetMetriconSqsSalesWebServiceUrl();
            MS.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
            // ===========

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

                    string PageHtml = PDFHeader(EstimateID, printCustomerDetails);
                    string Promotiontypeid = getPromotionType(EstimateID);
                    Doc BodyDoc = PrintEstimateBody(PageHtml, EstimateID, theDoc, viewtype, Promotiontypeid);
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
        public string PDFHeader(int EstimateID, bool printCustomerDetails)
        {
            string CustomerName = "";
            string CustomerName2 = "";
            string CustomerAddress = "";
            string LotAddress = "";
            string EstimateCreatedDate = "";
            string EstimateActualDate = "";
            string EstimateExpiryDate = "";
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
            //string inclusionType = "";
            //string inclusionvalue = "";
            //string devguideline = "";
            //string extraoption = "";
            //string Facade = "";


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
                //LotAddress = dds.Tables[0].Rows[0]["lotaddress"].ToString();
                //HouseName = dds.Tables[0].Rows[0]["homename"].ToString();
                //landprice = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["landprice"].ToString()));
                //if (merge == 1)
                //{
                //    homeprice = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["homeprice"].ToString()) + double.Parse(dds.Tables[0].Rows[0]["surcharge"].ToString()));
                //}
                //else
                //{
                //    homeprice = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["homeprice"].ToString()));
                //}
                surcharge = String.Format("{0:C}", estimateRevision_surcharge);
                //inclusionType = dds.Tables[0].Rows[0]["inclusionType"].ToString();
                //inclusionvalue = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["totalinclusion"].ToString()));
                //devguideline = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["totaldevguideline"].ToString()));
                //extraoption = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["totalextra"].ToString()));
                //siteworks = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["sitecost"].ToString()));
                //CartValue = String.Format("{0:C}", double.Parse(dds.Tables[0].Rows[0]["cartvalue"].ToString()));
                //Facade = dds.Tables[0].Rows[0]["Facade"].ToString();
                packagecreateddate = dds.Tables[0].Rows[0]["packagecreateddate"].ToString();
            }
            else
            {
                HouseName = headervar[6].ToString();
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
                    if (merge == 1)
                    {
                        homeprice = String.Format("{0:C}", estimateRevision_homePrice + estimateRevision_surcharge);
                    }
                    else
                    {
                        homeprice = String.Format("{0:C}", estimateRevision_homePrice);
                    }
                }
                else
                {
                    houseAndLandPkg = "No";
                }
            }
            else
                houseAndLandPkg = "No";

            string Brand = headervar[5].ToString();
            //        string HouseName = headervar[6].ToString();
            string Upgrades;
            //if (houseAndLandPkgContract)
            //{
            //    // Upgrades = String.Format("{0:C}", double.Parse(siteworks.ToString().Replace("$", "")) + double.Parse((headervar[8].ToString()).Replace("$", "")));
            //    double temp1 = 0.0;
            //    double temp2 = 0.0;
            //    try
            //    {
            //        temp1 = double.Parse(siteworks.ToString().Replace("$", ""));
            //    }
            //    catch (Exception ex)
            //    {
            //        temp1 = 0.0;
            //    }
            //    try
            //    {
            //        temp2 = double.Parse(headervar[8].ToString().Replace("$", ""));
            //    }
            //    catch (Exception ex)
            //    {
            //        temp2 = 0.0;
            //    }
            //    Upgrades = String.Format("{0:C}", temp1 + temp2);
            //}
            //else
            //{
                Upgrades = headervar[8].ToString();
            //}
            string jobpromotion = headervar[10].ToString();

            if (Session[Common.SELECTED_USER_SESSION_NAME] != null)
            {
                consultantCode = Session[Common.SELECTED_USER_SESSION_NAME].ToString();

                DataSet userDs = Client.GetUserListFromUsercode(consultantCode);

                if (userDs.Tables[0].Rows.Count > 0)
                {
                    if (userDs.Tables[0].Rows[0]["username"] != null && userDs.Tables[0].Rows[0]["username"].ToString() != "")
                    {
                        consultantName = userDs.Tables[0].Rows[0]["username"].ToString() + "&nbsp;&nbsp;" + userDs.Tables[0].Rows[0]["phone"].ToString();
                    }
                    else
                    {
                        consultantName = userDs.Tables[0].Rows[0]["username"].ToString();
                    }
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(@"<style type='text/css'>
            th {
                font-weight: bold; 
                font-size: 13px; 
                color: #000000;
                font-family: arial; 
                padding-left:3px; 
                padding-right:10px
            }
            td {
                font-family: arial; 
                font-size: 11px;
                padding: 0px;
            }
            </style>");

            sb.Append(@"
            <body style='margin:0px' valign='top'>
            <table border='0' width='100%' cellspacing=0 cellpadding=0>
            <tr  bgcolor='#76bdeb'>
                <td>
                    <table border='0' style='margin-top:10px; margin-left:10px; color:white'>
                    <col align=right><col align=left>
                    <tr>
                        <td valign='top' height='100px' style='font-size:18pt' colspan='2' align='left'>" + SaleType + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt;' colspan='2' align='left' ><b>Customer/Job Information</b></td>
                    </tr>
                    <tr>
                        <td style='font-size:11pt' valign='top'><b>Name:&nbsp;</b></td>
                        <td style='font-size:11pt' valign='top'>
                        <table width='100%' cellspacing=0 cellpadding=0>
                            <tr><td style='font-size:11pt;color:white;' valign='top'>&nbsp;&nbsp;" + CustomerName + "</td></tr><tr><td style='font-size:11pt;color:white;'>&nbsp;&nbsp;" + CustomerName2 + @" </td>
                        </table>
                        </td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt' valign='top'><b>Correspondence Address:</b></td>
                        <td style='font-size:11pt'>&nbsp;&nbsp;" + CustomerAddress + @"</td>
                    <tr>
                    
                    <tr>
                        <td style='font-size:11pt' valign='top'><b>Lot Address:</b></td>
                        <td style='font-size:11pt'>&nbsp;&nbsp;" + LotAddress + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Estimate Actual Date:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + EstimateActualDate.ToString() + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Estimate Created:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + EstimateCreatedDate.ToString() + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Deposit Date:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + depositDate + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Price List Effective Date:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + pricedate + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Expiry Date:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + EstimateExpiryDate.ToString() + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Customer Number:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + BCCustomerid + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Contract Number:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + BCContractnumber + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Estimate Number:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + EstimateIDInHeader + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>House and Land Package:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + houseAndLandPkg + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Package Created Date:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + packagecreateddate + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Brand:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + homebrandname + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>House Name:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + HouseName + @"</td>
                    <tr>");
            if (homebrandname.ToLower() == "regional victoria")
            {
                sb.Append(@"<tr>
                        <td style='font-size:11pt'><b>Facade Upgrade:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + Estimate.GetFacadeUpgrade(EstimateID, "ProductName") + @"</td>
                    <tr>");
            }
            sb.Append(@"<tr>
                        <td style='font-size:11pt'><b>Sales Consultant:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + consultantName + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Display:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + displaycentre + @"</td>
                    <tr>
                    <tr>
                        <td style='font-size:11pt'><b>Price Region:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + region + @"</td>
                    <tr>
                    </table>
                </td>
                <td align=right style='padding:0px'>
                    <table border = '0' color:white'>
                    <tr>
                        <td valign='top' height='400px' colspan='2'><img src='" + Server.MapPath(@"~/images/EstimateHeader.gif") + @"' /></td>
                    <tr>
                      <tr> 
                        <td align='right'>  
                            <table border='0' cellspacing=0 cellpading=0 style='margin-top:10px; margin-left:10px; color:white;'>
                                <col align=right><col align=left>");
            if (!houseAndLandPkgContract)
            {
                if (estimateRevision_internalVersion.ToUpper() != "CHANGESONLY")
                {
                    sb.Append(@"<tr>
                                    <td style='font-size:11pt'><b>Home:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + Baseprice + @"</td>
                                </tr>");
                }
            }
            // No need to display this for House and Land package
            //                            else
            //                            {
            //                                sb.Append(@"<tr>
            //                                    <td style='font-size:11pt'><b>Package:</b></td><td style='font-size:11pt'>" + Baseprice + @"</td>
            //                                </tr>");
            //                            }

            if (houseAndLandPkgContract)
            {
                if (breakdown == 0)
                {
                    sb.Append(@"<tr>
                                                  <td style='font-size:11pt'></td><td style='font-size:11pt'></td>
                                                </tr>
                                                <tr>
                                                    <td style='font-size:11pt'></td><td style='font-size:11pt'></td>
                                                </tr>
                                                <tr>
                                                    <td style='font-size:11pt'><b>Package Price:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + CartValue + @"</td>
                                                </tr>");
                }
                else
                {
                    if (estimateRevision_internalVersion.ToUpper() != "CHANGESONLY")
                    {
                        sb.Append(@"<tr>
                                                  <td style='font-size:11pt'><b>Land Price:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + landprice + @"</td>
                                                </tr>");
                        if (merge == 1)
                        {
                            sb.Append(@"<tr>
                                                  <td style='font-size:11pt'><b>Home Price:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + homeprice + @"</td>
                                                </tr>");
                        }
                        else
                        {
                            sb.Append(@"<tr>
                                                  <td style='font-size:11pt'><b>Home Price:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + homeprice + @"</td>
                                                </tr>
                                                <tr>
                                                  <td style='font-size:11pt'><b>Surcharge:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + surcharge + @"</td>
                                                </tr>");
                        }

                        sb.Append(@"
                                                <tr>
                                                    <td style='font-size:11pt'><b>Upgrades:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + Upgrades + @"</td>
                                                </tr>
                                                <tr>
                                                    <td style='font-size:11pt'><b>Total:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + CartValue + @"</td>

                                                </tr>");
                    }
                    else
                    {
                        sb.Append(@"
                                                <tr>
                                                    <td style='font-size:11pt'><b>Total:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + Upgrades + @"</td>
                                                </tr>");
                    }
                }
            }
            else
            {
                if ((estimateRevision_internalVersion.ToUpper() != "CHANGESONLY"))
                {
                    sb.Append(@"<tr>
                                    <td style='font-size:11pt'><b>Provisional Site Works:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + siteworks + @"</td>
                                </tr>
                                <tr>
                                    <td style='font-size:11pt'><b>Upgrades:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + Upgrades + @"</td>
                                </tr>
                                <tr>
                                    <td style='font-size:11pt'><b>Total:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + CartValue + @"</td>
                                </tr>");
                }
                else
                {
                    sb.Append(@"
                                                <tr>
                                                    <td style='font-size:11pt'><b>Total:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + Upgrades + @"</td>
                                                </tr>");
                }
            }
            sb.Append(@"
                                <tr>
                                    <td style='font-size:11pt'><b>Revision No.:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + estimateRevision_revisionNumber.ToString() + @"</td>
                                </tr>
                                <tr>
                                    <td style='font-size:11pt'><b>Revision Type:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + estimateRevision_revisionType + @"</td>
                                </tr>
                                <tr>
                                    <td style='font-size:11pt'><b>Revision Owner:</b></td><td style='font-size:11pt'>&nbsp;&nbsp;" + estimateRevision_revisionOwner + @"</td>
                                </tr>
                            </table>
                        </td> 
                    </tr>
                    </table>
                </td>
            </tr>
            </table>
            ");
            return sb.ToString();
        }

        public string GetHeaderInformation(int EstimateID)
        {
            string brand = ""; string homename = ""; int HOUSE_CODE = 0; int HOUSE_REGION;
            string Upgrades; string baseprice; string cartvalue; string jobpromotion; string siteworksvalue;
            string CustomerName = ""; string CustomerName2 = ""; string CustomerAddress = "";
            string EstimateCreatedDate = "";
            string EstimateActualDate = "";
            string EstimateExpiryDate = "";
            string promotiontype = "";
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
                baseprice = string.Format("{0:C}", packagePrice);
                Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice - estimateRevision_surcharge);
            }
            else
            {
                baseprice = string.Format("{0:C}", housePrice);
                Upgrades = string.Format("{0:C}", estimateRevision_upgradePrice - estimateRevision_siteworkPrice);
            }

            //Upgrades = string.Format("{0:C}", optionsTotal - siteworks);

            // ===========

            if (siteworks == 0.0)
                siteworksvalue = "TBA";
            else
                siteworksvalue = string.Format("{0:C}", siteworks);
            if (houseAndLandPkgContract)
                cartvalue = string.Format("{0:C}", packagePrice + optionsTotal);
            else
                cartvalue = string.Format("{0:C}", housePrice + optionsTotal);
            jobpromotion = string.Format("{0:C}", PromotionValue);
            string title = "";
            string SaleTypeCode = "Default";
            string depositDate = "";
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
                            CustomerName = customerDR["ivtu_title"] + " "
                                            + customerDR["ivtu_firstname"] + " "
                                            + customerDR["ivtu_surname"];

                            CustomerAddress = customerDR["ivtu_address"]
                                                + "," + customerDR["ivtu_suburb"]
                                                + "," + customerDR["ivtu_state"]
                                                + "," + customerDR["ivtu_zip"];
                        }

                        if (customerDT.Rows.Count > 1)
                        {
                            customerDR = customerDT.Rows[1];
                            CustomerName2 = customerDR["ivtu_title"] + " "
                                            + customerDR["ivtu_firstname"] + " "
                                            + customerDR["ivtu_surname"];
                        }
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
                EstimateCreatedDate = DateTime.Parse(EstimateRow["EstimateDate"].ToString()).ToString("dd/MM/yyyy");

                if (EstimateRow["EstimateActualDate"] == DBNull.Value)
                    EstimateActualDate = "";
                else
                    EstimateActualDate = DateTime.Parse(EstimateRow["EstimateActualDate"].ToString()).ToString("dd/MM/yyyy");

                int estimateStatus = Estimate.CheckEstimateStatus(EstimateID);
                // If the deposit is made, and not accepted yet, the Expiry Date is printed.
                if ((EstimateRow["Deposited"] != DBNull.Value && (bool)EstimateRow["Deposited"]) &&
                    (estimateStatus != Common.ESTIMATE_STATUS_ACCEPTED))
                    EstimateExpiryDate = DateTime.Parse(EstimateRow["ExpiryDate"].ToString()).ToString("dd/MM/yyyy");

                BCCustomerid = EstimateRow["BcCustomerid"].ToString();
                BCContractnumber = EstimateRow["BCContractnumber"].ToString();
                promotiontype = EstimateRow["promotiontypeid"].ToString();
            }
            //get the lot address where the customer are building
            DataSet LotAddressds = null;
            try
            {
                string oppid = "";
                if (Session["OpportunityID"] != null)
                    oppid = Session["OpportunityID"].ToString();


                if (houseAndLandPkgContract)
                {
                    LotAddressds = getLotAddressForPackage(EstimateID.ToString());
                }
                else
                {
                    LotAddressds = MS.GetSiteDetailsForOpportunityContractInDataSet(oppid);
                }
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
                /*
                DataTable dt = LotAddressds.Tables[0];
                DataRow dr = LotAddressds.Tables[0].Rows[0];
                if (LotAddressds != null && LotAddressds.Tables[0].Rows.Count > 0)
                {
                    if (dt.Rows[0]["lotaddress"] != null && dt.Rows[0]["lotaddress"].ToString() != "")
                    {
                        LotAddress = "Lot " + Validation.GetStringFromColumn(dt, 0, "lotaddress");
                    }

                    string addrPart = Validation.GetStringFromColumn(dt, 0, "streetno");

                    // Add Addr1
                    if ((LotAddress.Length > 0)
                        && (LotAddress.LastIndexOf(',') != LotAddress.Length - 1)
                        && (addrPart.Length > 0))
                        LotAddress += ",";

                    LotAddress += addrPart + " ";

                    addrPart = Validation.GetStringFromColumn(dt, 0, "streetname");

                    // Add Addr2
                    //if ((LotAddress.Length > 0) 
                    //    && (LotAddress.LastIndexOf(',') != LotAddress.Length-1)
                    //    && (addrPart.Length > 0))
                    //    LotAddress += ",";

                    LotAddress += addrPart;

                    addrPart = Validation.GetStringFromColumn(dt, 0, "suburb");

                    if (addrPart.ToUpper().IndexOf("SELECT") != -1)
                        addrPart = String.Empty;

                    // Add City
                    if ((LotAddress.Length > 0)
                        && (LotAddress.LastIndexOf(',') != LotAddress.Length - 1)
                        && (addrPart.Length > 0))
                        LotAddress += ",";

                    LotAddress += addrPart;

                    addrPart = Validation.GetStringFromColumn(dt, 0, "state");

                    if (addrPart.ToUpper().IndexOf("SELECT") != -1)
                        addrPart = String.Empty;

                    // Add State
                    if ((LotAddress.Length > 0)
                        && (LotAddress.LastIndexOf(',') != LotAddress.Length - 1)
                        && (addrPart.Length > 0))
                        LotAddress += ",";

                    LotAddress += addrPart;

                    addrPart = Validation.GetStringFromColumn(dt, 0, "postcode");

                    // Add City
                    if ((LotAddress.Length > 0)
                        && (LotAddress.LastIndexOf(',') != LotAddress.Length - 1)
                        && (addrPart.Length > 0))
                        LotAddress += ",";

                    LotAddress += addrPart;
                 
                    
                }
                */
                // Check houseAndLandPkg in site details

                //DBConnection DBCon = new DBConnection();
                //SqlCommand Cmd = DBCon.ExecuteStoredProcedure("spw_GetSiteDetailsRecord");
                //Cmd.Parameters["@contract"].Value = BCContractnumber;
                //DataSet sitedetails = DBCon.SelectSqlStoredProcedure(Cmd);
                //if (sitedetails != null && sitedetails.Tables[0].Rows.Count > 0)
                //{
                if (LotAddressds.Tables[0].Rows[0]["HouseAndLandPkg"].ToString() == "True")
                    houseAndLandPkg = "Yes";
                //}

                LotAddress = GetEstimateLotAddress(EstimateID);

            }
            catch (NullReferenceException nex4)
            {
                Response.Write(nex4.Message.ToString() + @"<br>" + nex4.Source.ToString() + "<br>LotAddressds problem.");
            }
            return title + "|" + CustomerName + "|" + CustomerAddress + "|" + LotAddress + "|" + EstimateCreatedDate + "|" +
           brand + "|" + homename + "|" + baseprice + "|" + Upgrades + "|" +
           cartvalue + "|" + jobpromotion + "|" + CustomerName2 + "|" + BCCustomerid + "|" + BCContractnumber + "|" + siteworksvalue + "|" +
           EstimateActualDate + "|" + EstimateExpiryDate + "|" + promotiontype + "|" + depositDate + "|" + houseAndLandPkg;
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
        public Doc PrintEstimateBody(string HTML, int estimateid, Doc theDoc, int viewtype, string promotiontypeid)
        {
            StringBuilder sb = new StringBuilder();
            string underReview = "";
            int fontsize = 100;
            sb.Append(HTML);

            // Actual body contect starts from here.
            int RowIdentifier = 0;
            sb.Append(@"
            <table border='0' width='100%' cellspacing=0 cellpadding=0>
            <col valign=top><col valign=top><col valign=top align=left><col valign=top align=right ><col align=center valign=top><col align=center valign=top><col align=right valign=top>
            <tr valign='top' style='text-align:left;padding-top:5px'>
                <th colspan=2>Summary</th>
                <th colspan=2>Description</th>");
            if (selectedPriceRegionStateID != 3)
            {
                sb.Append("<th>UOM</th>");
                sb.Append(@"<th>Qty</th>");
            }
            sb.Append(@"<th nowrap style='padding-right:0px'>Total Price<br>(Inc GST)</th>
            </tr>");

            //DataSet OptionDS = Estimate.GetEstimateView(estimateid, viewtype);

            DBConnection DB = new DBConnection();

            // ===== Modified to select data from Estimate Revision Details =====
            //SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("spw_GetEstimateForPrint");
            //EstimateViewCmd.Parameters["@estimateid"].Value = estimateid;

            SqlCommand EstimateViewCmd = DB.ExecuteStoredProcedure("sp_SalesEstimate_GetEstimateDetailsForPrinting");
            EstimateViewCmd.Parameters["@revisionId"].Value = estimateRevisionId;
            EstimateViewCmd.Parameters["@printtype"].Value = estimateRevision_internalVersion;

            // ===========
            
            DataSet OptionDS = DB.SelectSqlStoredProcedure(EstimateViewCmd);

            ArrayList AreaList = new ArrayList();

            foreach (DataRow OptionDR in OptionDS.Tables[0].Rows)
            {
                if (merge != 1 || (OptionDR["AREANAME"].ToString() != "Area Surcharge" && merge == 1))
                {
                    if (!AreaList.Contains(OptionDR["AREANAME"].ToString()))
                    {
                        AreaList.Add(OptionDR["AREANAME"].ToString());
                        // 5px pre-heading seperator
                        sb.Append("<tr><td colspan='7' style='font-size:5px;padding:0px'>&nbsp;</td></tr>");
                        // Heading
                        sb.Append("<tr bgcolor=#2A2F70>");
                        sb.Append("<td colspan='7' style='font-size:13px; font-weight:bold; color:#ffffff;'> " + OptionDR["AREANAME"].ToString() + "</td>");
                        sb.Append("</tr>");
                    }
                    else
                    {   // 1px row border seperator
                        sb.Append("<tr><td colspan='7' style='font-size:1px;padding:0px;border-bottom:1px solid black'>&nbsp;</td></tr>");
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
                    double w2=0;
                    double h2 = 0;
  

                    if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "") // there is a image selected
                    {
                        byte[] b = (byte[])OptionDR["image"];
                        MemoryStream ms = new MemoryStream(b);
                        //Bitmap bi = new Bitmap(ms);
                        img = (System.Drawing.Image)System.Drawing.Image.FromStream(ms);

                        if (!File.Exists(Server.MapPath(@"~/images/temp/"+OptionDR["selectedimageid"].ToString()+".jpg")))
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
                            w2 = (img.Width*originh)/img.Height;
                            h2 = originh;
                        }
                    }

                    

                    if (promotionProduct)
                    {
                        if (promotiontypeid == "5")
                        {
                            //Highlight that it is a promotion product
                            RowData =
                                    RowIdentifier + ".|" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]" + "|" + OptionDR["PRODUCTDESCRIPTION"].ToString();
                            if ((OptionDR["ENTERDESC"].ToString().CompareTo("") != 0) && (OptionDR["ENTERDESC"].ToString().Trim().CompareTo("N/A") != 0))
                                RowData = RowData + "<br> <b>Extra Description</b> " + OptionDR["ENTERDESC"].ToString();
                            if (estimateRevision_internalVersion=="INTERNAL" && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                                RowData = RowData + "<br> <b>Internal Notes</b> " + OptionDR["INTERNALDESC"].ToString();
                            if (isstudiomproduct)
                                RowData = RowData + "<br>"+studiom;

                            if (packageType == 1)
                                RowData = RowData + "<br><b>[Package Inclusion]</b>";
                            if (packageType == 2)
                                RowData = RowData + "<br><b>[Developer Guideline]</b>";

                            if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "")
                            {
                                RowData = RowData + "|" + "<br><img src='" + Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg") + "' width='" + w2.ToString() + "' height='" + h2.ToString() + "'/>";
                            }
                            else
                            {
                                RowData = RowData + "|";
                            }

                            if (selectedPriceRegionStateID != 3)
                                RowData = RowData + "|" + OptionDR["UOM"].ToString() + "|" + Qty.ToString();
                            RowData = RowData + "|" + ProductPrice + "<br/><b>(M-Style)</b>";
                        }
                        else if (promotiontypeid == "11")
                        {
                            //Highlight that it is a promotion product
                            RowData = RowIdentifier + ".|" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]" + "|" + Common.ReplaceCRLFByLineBreak(OptionDR["PRODUCTDESCRIPTION"].ToString());
                            if ((OptionDR["ENTERDESC"].ToString().CompareTo("") != 0) && (OptionDR["ENTERDESC"].ToString().Trim().CompareTo("N/A") != 0))
                                RowData = RowData + "<br> <b>Extra Description</b> " + OptionDR["ENTERDESC"].ToString();
                            if (estimateRevision_internalVersion == "INTERNAL" && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                                RowData = RowData + "<br> <b>Internal Notes</b> " + OptionDR["INTERNALDESC"].ToString();
                            if (isstudiomproduct)
                                RowData = RowData + "<br>" + studiom;
                            if (packageType == 1)
                                RowData = RowData + "<br><b>[Package Inclusion]</b>";
                            if (packageType == 2)
                                RowData = RowData + "<br><b>[Developer Guideline]</b>";

                            if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "")
                            {
                                RowData = RowData + "|" + "<br><img src='" + Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg") + "' width='" + w2.ToString() + "' height='" + h2.ToString() + "'/>";
                            }
                            else
                            {
                                RowData = RowData + "|";
                            }

                            if (selectedPriceRegionStateID != 3)
                                RowData = RowData + "|" + OptionDR["UOM"].ToString() + "|" + Qty.ToString();

                            if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "DELETED")
                            {
                                RowData = RowData + "|<b><img src='" + Server.MapPath(@"~/images/close.gif") + "'/>" + OptionDR["printprice"].ToString() + "</B>";
                            }
                            else
                            {
                                if (OptionDR["StandardPackageInclusion"].ToString() == "8")
                                {
                                    RowData = RowData + "|<b><img src='" + Server.MapPath(@"~/images/promotion.gif") + "'/>" + OptionDR["printprice"].ToString() + "</B>";
                                }
                                else
                                {
                                    RowData = RowData + "|<b><img src='" + Server.MapPath(@"~/images/promotion_red.ico") + "'/>" + OptionDR["printprice"].ToString() + "</B>";
                                }
                            }
                        }
                        else
                        {
                            //Highlight that it is a promotion product
                            RowData = RowIdentifier + ".|" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]" + "|" + Common.ReplaceCRLFByLineBreak(OptionDR["PRODUCTDESCRIPTION"].ToString());
                            if ((OptionDR["ENTERDESC"].ToString().CompareTo("") != 0) && (OptionDR["ENTERDESC"].ToString().Trim().CompareTo("N/A") != 0))
                                RowData = RowData + "<br> <b>Extra Description</b> " + OptionDR["ENTERDESC"].ToString();
                            if (estimateRevision_internalVersion == "INTERNAL" && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                                RowData = RowData + "<br> <b>Internal Notes</b> " + OptionDR["INTERNALDESC"].ToString();
                            if (isstudiomproduct)
                                RowData = RowData + "<br>" + studiom;
                            if (packageType == 1)
                                RowData = RowData + "<br><b>[Package Inclusion]</b>";
                            if (packageType == 2)
                                RowData = RowData + "<br><b>[Developer Guideline]</b>";

                            if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "")
                            {
                                RowData = RowData + "|" + "<br><img src='" + Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg") + "' width='" + w2.ToString() + "' height='" + h2.ToString() + "'/>";
                            }
                            else
                            {
                                RowData = RowData + "|";
                            }

                            if (selectedPriceRegionStateID != 3)
                                RowData = RowData + "|" + OptionDR["UOM"].ToString() + "|" + Qty.ToString();

                            if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "DELETED")
                            {
                                RowData = RowData + "|<b><img src='" + Server.MapPath(@"~/images/close.gif") + "'/>" + OptionDR["printprice"].ToString() + "</B>";
                            }
                            else
                            {
                                RowData = RowData + "|<b><img src='" + Server.MapPath(@"~/images/promotion.gif") + "'/>INCLUDED</B>";
                            }
                        }
                    }
                    else
                    {
                        //if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "" && File.Exists(Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg")))
                        //{
                        //    RowData = RowIdentifier + ".|" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]" + "|" + Common.ReplaceCRLFByLineBreak(OptionDR["PRODUCTDESCRIPTION"].ToString());
                        //}
                        //else
                        //{
                        RowData = RowIdentifier + ".|" + OptionDR["PRODUCTNAME"].ToString() + "<br>[" + OptionDR["PRODUCTID"].ToString() + "]" + "|" + Common.ReplaceCRLFByLineBreak(OptionDR["PRODUCTDESCRIPTION"].ToString()) ;
                        //}


                        if ((OptionDR["ENTERDESC"].ToString().CompareTo("") != 0) && (OptionDR["ENTERDESC"].ToString().Trim().CompareTo("N/A") != 0))
                            RowData = RowData + "<br> <b>Extra Description</b> " + OptionDR["ENTERDESC"].ToString();
                        if (estimateRevision_internalVersion == "INTERNAL" && (OptionDR["INTERNALDESC"].ToString().CompareTo("") != 0) && (OptionDR["INTERNALDESC"].ToString().Trim().CompareTo("N/A") != 0))
                            RowData = RowData + "<br> <b>Internal Notes</b> " + OptionDR["INTERNALDESC"].ToString();
                        if (isstudiomproduct)
                            RowData = RowData + "<br>" + studiom;
                        if (packageType == 1)
                            RowData = RowData + "<br><b>[Package Inclusion]</b>";
                        if (packageType == 2)
                            RowData = RowData + "<br><b>[Developer Guideline]</b>";

                        if (OptionDR["displayAt"] != null && OptionDR["displayAt"].ToString().Trim() != "")
                        {
                            RowData = RowData + "<br><b>[" + OptionDR["displayAt"].ToString() + "]</b>";
                        }



                        if (OptionDR["selectedimageid"] != null && OptionDR["selectedimageid"].ToString() != "" && File.Exists(Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg")))
                        {
                            RowData = RowData + "|" + "<img src='" + Server.MapPath(@"~/images/temp/" + OptionDR["selectedimageid"].ToString() + ".jpg") + "' width='" + w2.ToString() + "' height='" + h2.ToString() + "'/>";
                        }
                        else
                        {
                            RowData = RowData + "|";
                        }

                        if (selectedPriceRegionStateID != 3)
                            RowData = RowData + "|" + OptionDR["UOM"].ToString() + "|" + Qty.ToString();

                        if (packageType == 1)
                            RowData = RowData + "|<b>INCLUDED</b>";
                        else
                            RowData = RowData + "|" + ProductPrice;

                        if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "DELETED")
                        {
                            RowData = RowData + "<br><img src='" + Server.MapPath(@"~/images/close.gif") + "'>&nbsp;&nbsp;";
                        }
                    }

                    if (OptionDR["change"] != null && OptionDR["change"].ToString().ToUpper() == "DELETED")
                    {    
                          sb.Append("<tr bgcolor='#999999'>");
                    }
                    else
                    {
                        if (promotionProduct)
                            sb.Append("<tr bgcolor='#FFF8DC'>");
                        else if (packageType == 1)
                            sb.Append("<tr bgcolor='#CCCCFF'>");
                        else
                            sb.Append("<tr>");
                    }


                        if ((estimateRevision_revisionTypeId != 4  // Not Sales Estimator revision
                            || (estimateRevision_revisionTypeId == 4 && OptionDR["ItemAccepted"] != DBNull.Value && !Convert.ToBoolean(OptionDR["ItemAccepted"]))) // Not Accepted by Sales Estimator 
                            && OptionDR["areaid"].ToString() == "43") // Non-standard Request
                        {
                            RowData = RowData + "<br><img src='" + Server.MapPath(@"~/images/nonstandard.jpg") + "'>";
                        }


                    string[] theCols = RowData.ToString().Split(new char[] { '|' });

                    for (int i = 0; i < theCols.Length; i++)
                    {
                        if (i==2 && theCols[3].Trim()=="")
                        {                        
                            sb.Append("<td colspan=2>" + theCols[i].ToString() + "</td>");
                        }
                        else if (i == 3)
                        {
                            if (theCols[i].ToString().Trim() != "")
                            {
                                sb.Append("<td style='margin-top:5px; valign:top;'>" + theCols[i].ToString() + "</td>");
                            }
                        }
                        else
                        {
                            sb.Append("<td>" + theCols[i].ToString() + "</td>");
                        }
                    }

                    sb.Append("</tr>");
                }
            }
            // final row border seperator
            sb.Append("<tr><td colspan='6' style='font-size:1px;padding:0px;border-bottom:1px solid black'>&nbsp;</td></tr>");
            sb.Append("</table></body>");
            string FinalHTML = sb.ToString();
            int theID = theDoc.AddImageHtml(FinalHTML);

            // Add the last page.
            while (true)
            {
                if (!theDoc.Chainable(theID))
                {
                    theDoc.Page = theDoc.AddPage();
                    theDoc.Color.Color = System.Drawing.Color.FromArgb(42, 47, 112);
                    theDoc.Rect.Bottom = 2;
                    theDoc.Rect.Left = 15;
                    theDoc.Rect.Width = 500;
                    theDoc.Rect.Height = 15;
                    theDoc.Rect.Move(16, 800);
                    theDoc.FillRect();

                    // Add the Disclaimer/Acknowledgements header.
                    theDoc.Color.Color = System.Drawing.Color.White;
                    theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                    theDoc.TextStyle.Size = 12;
                    theDoc.TextStyle.Bold = false;
                    theDoc.Rect.Pin = 0;
                    theDoc.Rect.Position(30, 800);
                    theDoc.Rect.Width = 200;
                    theDoc.Rect.Height = 15;
                    theDoc.TextStyle.LeftMargin = 5;
                    theDoc.AddText(Common.PRINTPDF_DISCLAIMER_HEADER);

                    string customerInfo = GetHeaderInformation(estimateid);
                    string[] headervars = customerInfo.Split('|');
                    string tempStr;

                    tempStr = "Customer: " + headervars[1] + @"&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Sign Here ____________________";
                    if (customerInfo != null)
                    {
                        if (headervars.Length > 11 && headervars[11].Length > 0)
                        {
                            tempStr = tempStr + @"<br><br>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Customer: " + headervars[11] + @"&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Sign Here ____________________";
                        }
                    }



                    // Add the Disclaimer/Acknowledgements body text.
                    theDoc.Color.Color = System.Drawing.Color.Black;
                    theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                    theDoc.TextStyle.Size = Common.PRINTPDF_DISCLAIMER_FONTSIZE;
                    theDoc.TextStyle.Bold = false;
                    theDoc.Rect.Pin = 0;
                    theDoc.Rect.Position(30, 90);
                    theDoc.Rect.Width = 500;
                    theDoc.Rect.Height = 700;
                    theDoc.TextStyle.LeftMargin = 5;

                    string disclaimer = Common.getDisclaimer(estimateRevision_revisionTypeId.ToString(), Session["OriginalLogOnState"].ToString(), Session["SelectedRegionID"].ToString()).Replace("$Token$", tempStr);

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
                    theDoc.AddHtml(disclaimer);


                    // Add customer name #1.
                    theDoc.Color.Color = System.Drawing.Color.Black;
                    theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                    theDoc.TextStyle.Size = Common.PRINTPDF_DEFAULT_FONTSIZE;
                    theDoc.TextStyle.Bold = true;
                    //String Sig1 = "Customer : " + Session["Firstname"] + " " + Session["LastName"];
                    String Sig1 = Common.PRINTPDF_CUSTOMER_LABEL;

                    if (customerInfo != null)
                    {
                        Sig1 += headervars[1];
                    }

                    theDoc.Rect.Pin = 0;
                    theDoc.Rect.Position(30, 20);
                    theDoc.Rect.Width = 350;
                    theDoc.Rect.Height = 70;
                    theDoc.TextStyle.LeftMargin = 5;
                    theDoc.AddHtml(Sig1);

                    // Add signature field for Customer #1 on the same line.
                    theDoc.Color.Color = System.Drawing.Color.Black;
                    theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                    theDoc.TextStyle.Size = Common.PRINTPDF_DEFAULT_FONTSIZE;
                    theDoc.TextStyle.Bold = true;
                    theDoc.Rect.Pin = 0;
                    theDoc.Rect.Position(300, 20);
                    theDoc.Rect.Width = 350;
                    theDoc.Rect.Height = 70;
                    theDoc.TextStyle.LeftMargin = 5;
                    theDoc.AddHtml(Common.PRINTPDF_SIGNATURE_LABEL);

                    // Add customer name 2.
                    if (customerInfo != null)
                    {
                        if (headervars.Length > 11 && headervars[11].Length > 0)
                        {
                            Sig1 = Common.PRINTPDF_CUSTOMER_LABEL + headervars[11];
                            theDoc.Color.Color = System.Drawing.Color.Black;
                            theDoc.Font = theDoc.AddFont(Common.PRINTPDF_DEFAULT_FONT);
                            theDoc.TextStyle.Size = Common.PRINTPDF_DEFAULT_FONTSIZE;
                            theDoc.TextStyle.Bold = true;
                            theDoc.Rect.Pin = 0;
                            theDoc.Rect.Position(30, 0);
                            theDoc.Rect.Width = 350;
                            theDoc.Rect.Height = 70;
                            theDoc.TextStyle.LeftMargin = 5;
                            theDoc.AddHtml(Sig1);

                            // Add customer signature.
                            theDoc.Rect.Pin = 0;
                            theDoc.Rect.Position(300, 0);
                            theDoc.Rect.Width = 350;
                            theDoc.Rect.Height = 70;
                            theDoc.TextStyle.LeftMargin = 5;
                            theDoc.AddHtml(Common.PRINTPDF_SIGNATURE_LABEL);
                        }
                    }

                    break;
                }

                theDoc.Page = theDoc.AddPage();
                theID = theDoc.AddImageToChain(theID);
            }

            // Add Page number footer to each page
            //int status = Estimate.CheckEstimateStatus(estimateid);

            //bool addExpiryWatermark = false;

            //if (status == Common.ESTIMATE_STATUS_EXPIRED)
            //    addExpiryWatermark = true;

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
                DataSet ds = DBCon.SelectSqlStoredProcedure(sqlCmd);
                if (ds != null)
                {
                    underReview = ds.Tables[0].Rows[0]["watermark"].ToString();
                    fontsize = Int16.Parse(ds.Tables[0].Rows[0]["font"].ToString());
                }

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

                //theDoc.TextStyle.Bold = true;
                //theDoc.Font = theDoc.AddFont(Common.ORDERFORM_FONT);

                //theDoc.FontSize = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_FONTSIZE;
                //theDoc.Color.String = Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP_COLOUR;
                //theDoc.Color.Alpha = Common.ORDERFORM_ESTIMATE_EXPIRED_ALPHA_VALUE;

                //theDoc.Pos.String = "50 300";

                //theDoc.Transform.Reset();
                //theDoc.Transform.Rotate(45, 50, 300);

                //theDoc.AddText(Common.ORDERFORM_ESTIMATE_EXPIRED_STAMP);


                theDoc.TextStyle.Bold = false;
                theDoc.Rect.String = Common.ORDERFORM_PAGENUMBER_COORDINATES;
                theDoc.Color.String = "0 0 0";

                theDoc.FontSize = Common.ORDERFORM_PAGENUMBER_FONTSIZE;
                theDoc.HPos = 1;
                theDoc.Transform.Reset();           // Default, no rotation.

                theDoc.AddHtml(@"  <table border='0' width='100%' height='25px'>
                    <tr> <td>
                    <font size='1' face='arial' color='black'>
                        Date: " + DateTime.Today.Date.ToString("dd/MM/yyyy") +
                          @"&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td> 
                    <td> 
                        Page " + i.ToString() + " of " + theDoc.PageCount.ToString() +
                            "</font></td></tr> </table>");
                theDoc.Flatten();
            }
            // add specification based on state, brand and effectivedate
            string filename = getSpecification(estimateid.ToString());
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
         
            return theDoc;
        }

        public void SavePDF(Doc theDoc)
        {
            Random R = new Random();
            byte[] theData = theDoc.GetData();
            Response.Clear();
            Response.AddHeader("content-type", "application/pdf");
            Response.AddHeader("content-disposition", "inline; filename='Brochure" + "_" + R.Next(1000).ToString() + ".pdf'");

            if (Context.Response.IsClientConnected)
            {
                Context.Response.OutputStream.Write(theData, 0, theData.Length);
                Context.Response.Flush();
            }

            theDoc.Clear();
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

        public string getSpecification(string estimateid)
        {
            string result = "";
            try
            {
                DBConnection DB = new DBConnection();
                SqlCommand sqlCmd = DB.ExecuteStoredProcedure("spw_GetSpecificationByEstimate");
                sqlCmd.Parameters["@estimateid"].Value = Common.ConvertStringToIntIfFailToZero(estimateid);

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
            catch (Exception ex)
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
            catch (Exception ex)
            {
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
            catch (Exception ex)
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
                    estimateRevision_revisionOwner = ds.Tables[0].Rows[0]["OwnerName"].ToString();
                    estimateRevision_effectiveDate = Convert.ToDateTime(ds.Tables[0].Rows[0]["EffectiveDate"]).ToString("dd/MM/yyyy");
                    estimateRevision_landPrice = Convert.ToDouble(ds.Tables[0].Rows[0]["LandPrice"]);
                    estimateRevision_homePrice = Convert.ToDouble(ds.Tables[0].Rows[0]["HomePrice"]);
                    estimateRevision_siteworkPrice = Convert.ToDouble(ds.Tables[0].Rows[0]["SiteWorkValue"]);
                    estimateRevision_upgradePrice = Convert.ToDouble(ds.Tables[0].Rows[0]["UpgradeValue"]);
                    estimateRevision_promotionValue = Convert.ToDouble(ds.Tables[0].Rows[0]["PromotionValue"]);
                    estimateRevision_surcharge = Convert.ToDouble(ds.Tables[0].Rows[0]["Surcharge"]);
                    estimateRevision_state = ds.Tables[0].Rows[0]["State"].ToString();
                    estimateRevision_regionId = Convert.ToInt32(ds.Tables[0].Rows[0]["RegionID"]);
                    estimateRevision_statusId = Convert.ToInt32(ds.Tables[0].Rows[0]["StatusId"]);
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
                string selectedsupplierid,selectedsuppliername, selectedquestionid, selectedquestiontext;
                result = "<table cellspacing='0' cellpadding='0'><tr style='padding-top:0px'><td colspan='3'><b>Studio M:</b></td></tr>";
                IEnumerable<XElement> el = (from p in doc.Descendants("Brand") select p);
                string temp = ConfigurationManager.AppSettings["STUDIOM_QUESTION_TYPE"].ToString();
                string[] replacestring = temp.Split(',');
                foreach (XElement sup in el)
                {
                    selectedsupplierid = sup.Attribute("id").Value;
                    selectedsuppliername = sup.Attribute("name").Value;
                    result = result + "<tr style='padding-top:0px'><td colspan='3'><b>Brand: </b>" + selectedsuppliername + "</td></tr>";
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

                        result = result + "<tr style='padding:0px; height:16px;'><td><b>Q: </b>" + selectedquestiontext + "</td><td width='10px'>&nbsp;</td><td><b>A: </b>";
                        IEnumerable<XElement> answer = (from aw in doc.Descendants("Answer")
                                                        where (string)aw.Parent.Parent.Attribute("id") == selectedquestionid &&
                                                        (string)aw.Parent.Parent.Parent.Parent.Attribute("id") == selectedsupplierid
                                                        select aw);
                        int index = 0;
                        foreach (XElement aw in answer)
                        {
                            if (index == 0)
                            {
                                result = result + aw.Attribute("text").Value;
                            }
                            else
                            {
                                result = result + @"/" + aw.Attribute("text").Value;
                            }
                            index = index + 1;
                        }
                        result = result + "</td></tr>";

                    }
                }
                if (result == "<table cellspacing='0' cellpadding='0'><tr style='padding-top:0px'><td colspan='3'><b>Studio M:</b></td></tr>")
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
                result="";
            }
            return result;
        }
    }
}