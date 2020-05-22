using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Configuration;

using Metricon.WCF.MetriconRetailSystem.Contracts;

namespace Metricon.DAL.MetriconRetailSystem
{
    public class BcDataAccess
    {
        string ConnectionString;

        public BcDataAccess()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["BusinessCraftConnection"].ToString();
        }

        public bool IsConstructionCommenced(string contractNumber)
        {
            SqlDataAccess dataAccess = new SqlDataAccess();
            List<EstimateGridItem> estimates = dataAccess.SearchSpecificJob(string.Empty, contractNumber, "0", string.Empty, string.Empty, string.Empty, string.Empty);
            if (estimates.Count > 0)
            {
                string queryCommand = string.Empty;
                string eventNumber = "6000";

                if (estimates[0].MRSGroup == "Metro" && estimates[0].RegionID == 42) // Metro Central
                {
                    string eventNumberRegion42 = "5005";
                    queryCommand = string.Format("SELECT CLE_CONTRACT FROM CONLINE WHERE (CLE_EVENT = '{0}' OR CLE_EVENT = '{1}') AND CLE_CONTRACT = '{2}' AND CLE_REGDATE IS NOT NULL", eventNumber, eventNumberRegion42, contractNumber);
                }
                else
                {
                    if (estimates[0].MRSGroup == "QLD")
                        eventNumber = "5109";
                    else if (estimates[0].MRSGroup == "Regional West")
                        eventNumber = "5000";
                    queryCommand = string.Format("SELECT CLE_CONTRACT FROM CONLINE WHERE CLE_EVENT = '{0}' AND CLE_CONTRACT = '{1}' AND CLE_REGDATE IS NOT NULL", eventNumber, contractNumber);
                }

                OdbcConnection bcConn = new OdbcConnection(ConnectionString);
                OdbcCommand cmd = new OdbcCommand(queryCommand, bcConn);
                bcConn.Open();
                if (cmd.ExecuteScalar() == null)
                    return false;
                else
                    return true;
            }
            else
                throw new Exception("Estimate Not Found");
        }

        public string GetBCForecastDate(string contractNumber)
        {
            string bcForecastDate = null;
            string eventNumber = "5000";
            string bcForecastDateQuery = string.Format("SELECT CLE_FOREDATE FROM CONLINE WHERE CLE_CONTRACT = '{0}' and CLE_EVENT = '{1}'", contractNumber, eventNumber);

            try
            { 
                using (OdbcConnection bcConn = new OdbcConnection(ConnectionString))
                {
                    using (OdbcCommand bcForecastDateQueryCmd = new OdbcCommand(bcForecastDateQuery, bcConn))
                    {
                        bcConn.Open();

                        object result = bcForecastDateQueryCmd.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            bcForecastDate = result.ToString();
                        }

                        bcConn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                bcForecastDate = string.Empty;
            }

            return bcForecastDate;
        }

        public DataTable GetContactsOfCustomer(string customercode)
        {
            DataTable contacts = new DataTable();
            contacts.Columns.Add("Salutation");
            contacts.Columns.Add("LastName");
            contacts.Columns.Add("FirstName");
            contacts.Columns.Add("Address");
            contacts.Columns.Add("Suburb");
            contacts.Columns.Add("State");
            contacts.Columns.Add("Postcode");
            contacts.Columns.Add("Phone");
            contacts.Columns.Add("Mobile");
            contacts.Columns.Add("EmailAddress");
            contacts.Columns.Add("Primary");

            string customerQuery = string.Format(@"SELECT IVTU_CUSTNO, IVTU_SEQNUM, IVTU_TITLE, IVTU_SURNAME, IVTU_FIRSTNAME, IVTU_ADDRESS, IVTU_SUBURB, 
	                                IVTU_STATE, IVTU_ZIP, IVTU_PHHOME, IVTU_MOBILE
                                    FROM IVCTBLU
                                    WHERE IVTU_CUSTNO = '{0}'
                                    ORDER BY IVTU_CUSTNO, IVTU_SEQNUM", customercode);

            using (OdbcConnection bcConn = new OdbcConnection(ConnectionString))
            {
                using (OdbcCommand customerCmd = new OdbcCommand(customerQuery, bcConn))
                {
                    bcConn.Open();

                    using (OdbcDataReader customerDataReader = customerCmd.ExecuteReader())
                    {
                        bool firstContact = true;

                        while (customerDataReader.Read())
                        {
                            DataRow contact = contacts.NewRow();

                            if (customerDataReader["IVTU_TITLE"] != DBNull.Value)
                                contact["Salutation"] = customerDataReader["IVTU_TITLE"].ToString();

                            if (customerDataReader["IVTU_SURNAME"] != DBNull.Value)
                                contact["LastName"] = customerDataReader["IVTU_SURNAME"].ToString();

                            if (customerDataReader["IVTU_FIRSTNAME"] != DBNull.Value)
                                contact["FirstName"] = customerDataReader["IVTU_FIRSTNAME"].ToString();

                            if (customerDataReader["IVTU_ADDRESS"] != DBNull.Value)
                                contact["Address"] = customerDataReader["IVTU_ADDRESS"].ToString();

                            if (customerDataReader["IVTU_SUBURB"] != DBNull.Value)
                                contact["Suburb"] = customerDataReader["IVTU_SUBURB"].ToString();

                            if (customerDataReader["IVTU_STATE"] != DBNull.Value)
                                contact["State"] = customerDataReader["IVTU_STATE"].ToString();

                            if (customerDataReader["IVTU_ZIP"] != DBNull.Value)
                                contact["Postcode"] = customerDataReader["IVTU_ZIP"].ToString();

                            if (customerDataReader["IVTU_PHHOME"] != DBNull.Value)
                                contact["Phone"] = customerDataReader["IVTU_PHHOME"].ToString();

                            if (customerDataReader["IVTU_MOBILE"] != DBNull.Value)
                                contact["Mobile"] = customerDataReader["IVTU_MOBILE"].ToString();

                            if (customerDataReader["IVTU_SEQNUM"] != DBNull.Value)
                            {
                                string referenceNumber = string.Format("{0}00{1}", customercode, customerDataReader["IVTU_SEQNUM"].ToString().Trim());
                                contact["EmailAddress"] = GetCustomerEmailAddress(referenceNumber);
                            }

                            contact["Primary"] = firstContact;

                            contacts.Rows.Add(contact);

                            firstContact = false;
                        }
                        customerDataReader.Close();
                    }

                    bcConn.Close();
                }
            }

            return contacts;
        }

        private string GetCustomerEmailAddress(string referenceNumber)
        {
            string emailAddress = null;
            string emailAddressQuery = string.Format("SELECT EMLA_ADDRESS FROM EMLADDB WHERE EMLA_REFCODE = '{0}'", referenceNumber);

            using (OdbcConnection bcConn = new OdbcConnection(ConnectionString))
            {
                using (OdbcCommand emailQueryCmd = new OdbcCommand(emailAddressQuery, bcConn))
                {
                    bcConn.Open();

                    object result = emailQueryCmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        emailAddress = (string)result;
                        if (emailAddress != null && emailAddress.ToString() != "")
                        {
                            if (emailAddress.IndexOf(";") > 0)
                            {
                                emailAddress = emailAddress.Substring(0, emailAddress.IndexOf(";"));
                            }

                            emailAddress = emailAddress.Replace("þ", "");
                            emailAddress = emailAddress.Replace("ÿ", "");
                        }
                    }

                    bcConn.Close();
                }
            }

            return emailAddress;
        }

        public DataTable GetContractDetails(string contractNumber)
        {
            DataTable contract = new DataTable();
            contract.Columns.Add("CustomerNumber");
            contract.Columns.Add("LotNumber");
            contract.Columns.Add("StreetNumber");
            contract.Columns.Add("StreetName");
            contract.Columns.Add("City");
            contract.Columns.Add("State");
            contract.Columns.Add("Postcode");
            contract.Columns.Add("HouseCode");
            contract.Columns.Add("HouseName");
            contract.Columns.Add("ContractStatus");

            string contractQuery = string.Format(@"SELECT 
                CON.CO_CUSNUM, 
                CON.CO_LOTNO, 
                CON.CO_STREETNO, 
                CON.CO_ADDR, 
                CON.CO_CITY, 
                CON.CO_STATE, 
                CON.CO_ZIP, 
                CON.CO_HOUSE, 
                CON.CO_STATUS, 
                ITM.DESCR 
                FROM CONHDRA CON
                LEFT JOIN ITMMASA ITM ON CON.CO_HOUSE = ITM.ITEMNO
                WHERE CO_CONTRACT = '{0}'", contractNumber);

            using (OdbcConnection bcConn = new OdbcConnection(ConnectionString))
            {
                using (OdbcCommand customerCmd = new OdbcCommand(contractQuery, bcConn))
                {
                    bcConn.Open();

                    using (OdbcDataReader contractDataReader = customerCmd.ExecuteReader())
                    {
                        if (contractDataReader.Read())
                        {
                            DataRow contractRow = contract.NewRow();

                            if (contractDataReader["CO_CUSNUM"] != DBNull.Value)
                                contractRow["CustomerNumber"] = contractDataReader["CO_CUSNUM"].ToString();

                            if (contractDataReader["CO_LOTNO"] != DBNull.Value)
                                contractRow["LotNumber"] = contractDataReader["CO_LOTNO"].ToString();

                            if (contractDataReader["CO_STREETNO"] != DBNull.Value)
                                contractRow["StreetNumber"] = contractDataReader["CO_STREETNO"].ToString();

                            if (contractDataReader["CO_ADDR"] != DBNull.Value)
                                contractRow["StreetName"] = contractDataReader["CO_ADDR"].ToString();

                            if (contractDataReader["CO_CITY"] != DBNull.Value)
                                contractRow["City"] = contractDataReader["CO_CITY"].ToString();

                            if (contractDataReader["CO_STATE"] != DBNull.Value)
                                contractRow["State"] = contractDataReader["CO_STATE"].ToString();

                            if (contractDataReader["CO_ZIP"] != DBNull.Value)
                                contractRow["Postcode"] = contractDataReader["CO_ZIP"].ToString();

                            if (contractDataReader["CO_HOUSE"] != DBNull.Value)
                                contractRow["HouseCode"] = contractDataReader["CO_HOUSE"].ToString();

                            if (contractDataReader["DESCR"] != DBNull.Value)
                                contractRow["HouseName"] = contractDataReader["DESCR"].ToString();

                            if (contractDataReader["CO_STATUS"] != DBNull.Value)
                                contractRow["ContractStatus"] = contractDataReader["CO_STATUS"].ToString();

                            contract.Rows.Add(contractRow);
                        }
                        contractDataReader.Close();
                    }

                    bcConn.Close();
                }
            }

            return contract;
        }

        public DateTime GetStudioMAppointmentTime(string contractNumber, string eventNumber)
        {
            DateTime appointmentTime = DateTime.MinValue;

            string eventQuery = string.Format(@"SELECT CLE_FOREDATE, CLE_REF FROM CONLINE WHERE CLE_CONTRACT = '{0}' AND CLE_EVENT = '{1}'", contractNumber, eventNumber);

            using (OdbcConnection bcConn = new OdbcConnection(ConnectionString))
            {
                using (OdbcCommand eventCmd = new OdbcCommand(eventQuery, bcConn))
                {
                    bcConn.Open();

                    using (OdbcDataReader eventDataReader = eventCmd.ExecuteReader())
                    {
                        if (eventDataReader.Read())
                        {
                            if (eventDataReader["CLE_FOREDATE"] != DBNull.Value)
                            {
                                DateTime appDate = Convert.ToDateTime(eventDataReader["CLE_FOREDATE"]);
                                string time = "12:00 AM";

                                if (eventDataReader["CLE_REF"] != DBNull.Value)
                                {
                                    DateTime appTime = DateTime.MinValue;
                                    string appRef = eventDataReader["CLE_REF"].ToString().ToUpper().Replace(" ","");
                                    appRef = appRef.Replace(".M.","M"); //A.M. = AM
                                    appRef = appRef.Replace(".M","M"); //A.M = AM

                                    appRef = appRef.Replace("AM", " AM");
                                    appRef = appRef.Replace("PM", " PM");

                                    appRef = appRef.Replace(".", ":");

                                    if (DateTime.TryParse(appRef, out appTime))
                                    {
                                        time = appTime.ToShortTimeString();
                                    }
                                }

                                appointmentTime = Convert.ToDateTime(appDate.ToString("yyyy-MM-dd") + " " + time);
                            }
                        }
                        eventDataReader.Close();
                    }

                    bcConn.Close();
                }
            }

            return appointmentTime;
        }

        public string GetStaffCodeFromContractNumberAndEvent(string contractnumber, string eventcode)
        {
            string staffcode = "";

            string Query = string.Format(@"SELECT CLE_EMPLOYEE
                                    FROM conline 
                                    WHERE CLE_CONTRACT = '{0}' AND CLE_EVENT='{1}'
                                    ORDER BY CLE_EMPLOYEE", contractnumber, eventcode);

            using (OdbcConnection bcConn = new OdbcConnection(ConnectionString))
            {
                using (OdbcCommand emailQueryCmd = new OdbcCommand(Query, bcConn))
                {
                    bcConn.Open();

                    object result = emailQueryCmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        staffcode = (string)result;
                    }
                    bcConn.Close();
                }
            }
            return staffcode;
        }
    }
}
