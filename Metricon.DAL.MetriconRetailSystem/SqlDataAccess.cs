using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.ComponentModel;
using Elmah;
using System.Diagnostics;

using Metricon.WCF.MetriconRetailSystem.Contracts;

namespace Metricon.DAL.MetriconRetailSystem
{

    public class SqlDataAccess
    {
        //private static string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        private SqlConnection connection = null;
        private int timeout = 120;
        private SqlDataAdapter Adapter;
        private DataSet ds;

        public int AcceptOriginalEstimate(int estimateId)
        {
            int newEstimateHeaderId = 0;
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_AcceptOriginalEstimate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));

                    conn.Open();
                    newEstimateHeaderId = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();
                }
            }

            return newEstimateHeaderId;
        }


        public void CompleteEstimate(int revisionId, int userId, int statusId, int statusReasonId, int revisionTypeId, int ownerId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_SalesEstimate_CompleteEstimateRevision";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));
                        cmd.Parameters.Add(new SqlParameter("@userId", userId));
                        cmd.Parameters.Add(new SqlParameter("@statusId", statusId));
                        cmd.Parameters.Add(new SqlParameter("@statusReasonId", statusReasonId));
                        cmd.Parameters.Add(new SqlParameter("@nextRevisionTypeId", revisionTypeId));
                        cmd.Parameters.Add(new SqlParameter("@nextRevisionOwnerId", ownerId));

                        conn.Open();
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
        }


        public int AssignQueuedEstimate(int queueId, int userId, int ownerId)
        {
            int estimateRevisionId = 0;
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_AssignSalesEstimateFromQueue";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@queueId", queueId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    cmd.Parameters.Add(new SqlParameter("@ownerId", ownerId));
                    SqlParameter param = cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", SqlDbType.Int));
                    param.Direction = ParameterDirection.Output;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    estimateRevisionId = (int)param.Value;

                    conn.Close();
                }
            }

            return estimateRevisionId;
        }


        public void AssignWorkingEstimate(int estimateRevisionId, int userId, int ownerId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_AssignWorkingSalesEstimate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    cmd.Parameters.Add(new SqlParameter("@ownerId", ownerId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public List<EstimateGridItem> GetQueuedEstimates(
            int revisionTypeId,
            int regionId,
            int roleId,
            string customerNumber,
            string contractNumber,
            int salesConsultantId,
            string lotNumber,
            string streetName,
            string suburb, string businessUnit)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateGridItem> estimates = new List<EstimateGridItem>();

                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_SalesEstimate_GetQueuedEstimates";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@revisionTypeId", revisionTypeId));
                        cmd.Parameters.Add(new SqlParameter("@regionId", regionId));
                        cmd.Parameters.Add(new SqlParameter("@roleId", roleId));
                        cmd.Parameters.Add(new SqlParameter("@customerNumber", customerNumber));
                        cmd.Parameters.Add(new SqlParameter("@contractNumber", contractNumber));
                        cmd.Parameters.Add(new SqlParameter("@salesConsultantId", salesConsultantId));
                        cmd.Parameters.Add(new SqlParameter("@lotNumber", lotNumber));
                        cmd.Parameters.Add(new SqlParameter("@streetName", streetName));
                        cmd.Parameters.Add(new SqlParameter("@suburb", suburb));
                        cmd.Parameters.Add(new SqlParameter("@businessUnit", businessUnit));

                        conn.Open();

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                EstimateGridItem estimate = new EstimateGridItem();
                                estimate.CustomerName = dr["CustomerName"].ToString();
                                estimate.CustomerNumber = Convert.ToInt32(dr["CustomerNumber"]);
                                estimate.ContractNumber = Convert.ToInt32(dr["ContractNumber"]);
                                estimate.EstimateId = Convert.ToInt32(dr["EstimateNumber"]);
                                estimate.HomeName = dr["HomeName"].ToString();
                                estimate.SalesConsultantName = dr["SalesConsultantName"].ToString();
                                estimate.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                                estimate.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]) + 1;
                                estimate.RevisionTypeId = Convert.ToInt32(dr["RevisionTypeId"]);
                                estimate.RevisionTypeCode = dr["RevisionTypeCode"].ToString();
                                estimate.DifficultyRating = dr["DifficultyRatingName"] != DBNull.Value ? dr["DifficultyRatingName"].ToString() : string.Empty;
                                estimate.DifficultyRatingId = dr["DifficultyRatingId"] != DBNull.Value ? Convert.ToInt32(dr["DifficultyRatingId"]) : 0;
                                estimate.OwnerName = dr["OwnerName"].ToString();
                                estimate.RecordId = Convert.ToInt32(dr["QueueId"]);
                                estimate.RecordType = "Queue";
                                estimate.RevisionDetails = estimate.RevisionNumber.ToString() + " (" + estimate.RevisionTypeCode + ")";
                                estimate.ContractID = dr["contractID"].ToString();
                                estimate.PreviousRevisionId = Convert.ToInt32(dr["PreviousRevisionId"]);
                                estimate.ContractType = dr["ContractType"].ToString();
                                estimate.JobFlowType = dr["JobFlowType"].ToString();

                                if (dr["DueDate"] != DBNull.Value)
                                    estimate.DueDate = Convert.ToDateTime(dr["DueDate"]);

                                if (dr["ContractStatus"] != DBNull.Value)
                                {
                                    switch (dr["ContractStatus"].ToString())
                                    {
                                        case "1":
                                            estimate.ContractStatusName = "Pending";
                                            break;
                                        case "2":
                                            estimate.ContractStatusName = "Cancelled";
                                            break;
                                        case "3":
                                            estimate.ContractStatusName = "Work In Progress";
                                            break;
                                        case "4":
                                            estimate.ContractStatusName = "On Hold";
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                estimate.BusinessUnit = dr["BusinessUnit"].ToString();

                                estimates.Add(estimate);
                            }

                            dr.Close();
                        }

                        conn.Close();
                    }
                }

            return estimates;
        }

        public List<EstimateGridItem> GetAssignedEstimates(
            int revisionTypeId,
            int roleId,
            int statusId,
            int userId,
            int regionId,
            string customerNumber,
            string contractNumber,
            int salesConsultantId,
            string lotNumber,
            string streetName,
            string suburb, string businessUnit)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateGridItem> estimates = new List<EstimateGridItem>();
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();
            //try
            //{
                /*SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetAssignedEstimates");
                SqlCmd.Parameters["@revisionTypeId"].Value = revisionTypeId;
                SqlCmd.Parameters["@roleId"].Value = roleId;
                SqlCmd.Parameters["@statusId"].Value = statusId;
                SqlCmd.Parameters["@userId"].Value = userId;
                SqlCmd.Parameters["@regionId"].Value = regionId;
                SqlCmd.Parameters["@customerNumber"].Value = customerNumber;
                SqlCmd.Parameters["@contractNumber"].Value = contractNumber;
                SqlCmd.Parameters["@salesConsultantId"].Value = salesConsultantId;
                SqlCmd.Parameters["@lotNumber"].Value = lotNumber;
                SqlCmd.Parameters["@streetName"].Value = streetName;
                SqlCmd.Parameters["@suburb"].Value = suburb;
                SqlCmd.Parameters["@district"].Value = District;
                SqlCmd.Parameters["@opCentre"].Value = OpCentre;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    EstimateGridItem estimate = new EstimateGridItem();
                    estimate.CustomerName = dr["CustomerName"].ToString();
                    estimate.CustomerNumber = Convert.ToInt32(dr["CustomerNumber"]);
                    estimate.ContractNumber = Convert.ToInt32(dr["ContractNumber"]);
                    estimate.EstimateId = Convert.ToInt32(dr["EstimateNumber"]);
                    estimate.HomeName = dr["HomeName"].ToString();
                    estimate.SalesConsultantName = dr["SalesConsultantName"].ToString();
                    estimate.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                    estimate.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]);
                    estimate.RevisionTypeId = Convert.ToInt32(dr["RevisionTypeId"]);
                    estimate.RevisionTypeCode = dr["RevisionTypeCode"].ToString();
                    estimate.DifficultyRating = dr["DifficultyRatingName"] != DBNull.Value ? dr["DifficultyRatingName"].ToString() : string.Empty;
                    estimate.DifficultyRatingId = dr["DifficultyRatingId"] != DBNull.Value ? Convert.ToInt32(dr["DifficultyRatingId"]) : 0;
                    estimate.OwnerName = dr["OwnerName"].ToString();
                    estimate.OwnerId = Convert.ToInt32(dr["OwnerId"]);
                    estimate.RecordId = Convert.ToInt32(dr["EstimateHeaderId"]);
                    estimate.RecordType = "EstimateHeader";
                    estimate.Opportunityid = dr["fkidopportunity"].ToString();
                    estimate.ContractID = dr["contractID"].ToString();
                    estimate.Phone = dr["phone"].ToString();
                    estimate.Accountid = dr["accountid"].ToString();

                    if (dr["DocumentType"].ToString() != "")
                        estimate.RevisionDetails = estimate.RevisionNumber.ToString() + " (" + estimate.RevisionTypeCode + ") - " + dr["DocumentType"].ToString();
                    else
                        estimate.RevisionDetails = estimate.RevisionNumber.ToString() + " (" + estimate.RevisionTypeCode + ")";

                    estimate.AllowToAddNSR = bool.Parse(dr["allowtoaddNSR"].ToString());
                    estimate.ValidateAcceptedFlag = bool.Parse(dr["validateaccept"].ToString());
                    estimate.ValidateStandardInclusion = bool.Parse(dr["ValidateStandardInclusion"].ToString());
                    estimate.ReadOnly = bool.Parse(dr["readonly"].ToString());
                    estimate.AllowToAcceptItem = bool.Parse(dr["AllowToAcceptItem"].ToString());
                    estimate.AllowToViewStudioMTab = bool.Parse(dr["AllowToViewStudioMTab"].ToString());
                    estimate.AllowToViewStudioMDocuSign = bool.Parse(dr["AllowToViewStudioMDocuSign"].ToString());
                    estimate.ContractType = dr["ContractType"].ToString();
                    estimate.JobFlowType = dr["JobFlowType"].ToString();
                    estimate.MRSGroup = dr["MRSGroup"].ToString();
                    estimate.AllowToResetCurrentMilestone = bool.Parse(dr["allowtoresetcurrentmilestone"].ToString());
                    estimate.AllowUndoSetContract = bool.Parse(dr["allowtoundosetcontract"].ToString());

                    if (dr["DueDate"] != DBNull.Value)
                        estimate.DueDate = Convert.ToDateTime(dr["DueDate"]);

                    if (dr["AppointmentDateTime"] != DBNull.Value)
                        estimate.AppointmentDate = Convert.ToDateTime(dr["AppointmentDateTime"]);

                    estimate.ContractStatusName = dr["ContractStatus"].ToString();
                    //if (dr["ContractStatus"] != DBNull.Value)
                    //{
                    //    switch (dr["ContractStatus"].ToString())
                    //    {
                    //        case "1":
                    //            estimate.ContractStatusName = "Pending";
                    //            break;
                    //        case "2":
                    //            estimate.ContractStatusName = "Cancelled";
                    //            break;
                    //        case "3":
                    //            estimate.ContractStatusName = "Work In Progress";
                    //            break;
                    //        case "4":
                    //            estimate.ContractStatusName = "On Hold";
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //}
                    estimate.District = dr["District"].ToString();
                    estimate.OpCentre = dr["OpCentre"].ToString();
                    if (dr.Table.Columns.Contains("EditEstimateUserID")) // Table.Columns["EditEstimateUserID"] != null
                        estimate.EditEstimateUserID = Convert.ToInt32(dr["EditEstimateUserID"].ToString());
                    if (dr.Table.Columns.Contains("EditEstimateUserName"))
                        estimate.EditEstimateUserName = dr["EditEstimateUserName"].ToString();

                    estimates.Add(estimate);
                }
                    */
                                    using (SqlConnection conn = new SqlConnection(connectionstring))
                                    {
                                        using (SqlCommand cmd = conn.CreateCommand())
                                        {
                                            cmd.CommandText = "sp_SalesEstimate_GetAssignedEstimates";
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            cmd.Parameters.Add(new SqlParameter("@revisionTypeId", revisionTypeId));
                                            cmd.Parameters.Add(new SqlParameter("@roleId", roleId));
                                            cmd.Parameters.Add(new SqlParameter("@statusId", statusId));
                                            cmd.Parameters.Add(new SqlParameter("@userId", userId));
                                            cmd.Parameters.Add(new SqlParameter("@regionId", regionId));
                                            cmd.Parameters.Add(new SqlParameter("@customerNumber", customerNumber));
                                            cmd.Parameters.Add(new SqlParameter("@contractNumber", contractNumber));
                                            cmd.Parameters.Add(new SqlParameter("@salesConsultantId", salesConsultantId));
                                            cmd.Parameters.Add(new SqlParameter("@lotNumber", lotNumber));
                                            cmd.Parameters.Add(new SqlParameter("@streetName", streetName));
                                            cmd.Parameters.Add(new SqlParameter("@suburb", suburb));
                                            cmd.Parameters.Add(new SqlParameter("@businessUnit", businessUnit));

                                            conn.Open();

                                            using (SqlDataReader dr = cmd.ExecuteReader())
                                            {
                                                while (dr.Read())
                                                {
                                                    EstimateGridItem estimate = new EstimateGridItem();
                                                    estimate.CustomerName = dr["CustomerName"].ToString();
                                                    estimate.CustomerNumber = Convert.ToInt32(dr["CustomerNumber"]);
                                                    estimate.ContractNumber = Convert.ToInt32(dr["ContractNumber"]);
                                                    estimate.EstimateId = Convert.ToInt32(dr["EstimateNumber"]);
                                                    estimate.HomeName = dr["HomeName"].ToString();
                                                    estimate.SalesConsultantName = dr["SalesConsultantName"].ToString();
                                                    estimate.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                                                    estimate.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]);
                                                    estimate.RevisionTypeId = Convert.ToInt32(dr["RevisionTypeId"]);
                                                    estimate.RevisionTypeCode = dr["RevisionTypeCode"].ToString();
                                                    estimate.DifficultyRating = dr["DifficultyRatingName"] != DBNull.Value ? dr["DifficultyRatingName"].ToString() : string.Empty;
                                                    estimate.DifficultyRatingId = dr["DifficultyRatingId"] != DBNull.Value ? Convert.ToInt32(dr["DifficultyRatingId"]) : 0;
                                                    estimate.OwnerName = dr["OwnerName"].ToString();
                                                    estimate.OwnerId = Convert.ToInt32(dr["OwnerId"]);
                                                    estimate.RecordId = Convert.ToInt32(dr["EstimateHeaderId"]);
                                                    estimate.RecordType = "EstimateHeader";
                                                    estimate.Opportunityid = dr["fkidopportunity"].ToString();
                                                    estimate.ContractID = dr["contractID"].ToString();
                                                    estimate.Phone = dr["phone"].ToString();
                                                    estimate.Accountid = dr["accountid"].ToString();

                                                    if (dr["DocumentType"].ToString() != "")
                                                        estimate.RevisionDetails = estimate.RevisionNumber.ToString() + " (" + estimate.RevisionTypeCode + ") - " + dr["DocumentType"].ToString();
                                                    else
                                                        estimate.RevisionDetails = estimate.RevisionNumber.ToString() + " (" + estimate.RevisionTypeCode + ")";

                                                    estimate.AllowToAddNSR = bool.Parse(dr["allowtoaddNSR"].ToString());
                                                    estimate.ValidateAcceptedFlag = bool.Parse(dr["validateaccept"].ToString());
                                                    estimate.ValidateStandardInclusion = bool.Parse(dr["ValidateStandardInclusion"].ToString());
                                                    estimate.ReadOnly = bool.Parse(dr["readonly"].ToString());
                                                    estimate.AllowToAcceptItem = bool.Parse(dr["AllowToAcceptItem"].ToString());
                                                    estimate.AllowToViewStudioMTab = bool.Parse(dr["AllowToViewStudioMTab"].ToString());
                                                    estimate.AllowToViewStudioMDocuSign = bool.Parse(dr["AllowToViewStudioMDocuSign"].ToString());
                                                    estimate.ContractType = dr["ContractType"].ToString();
                                                    estimate.JobFlowType = dr["JobFlowType"].ToString();
                                                    estimate.MRSGroup = dr["MRSGroup"].ToString();
                                                    estimate.EditVisible = bool.Parse(dr["allowtoedit"].ToString());
                                                    estimate.AllowToResetCurrentMilestone = bool.Parse(dr["allowtoresetcurrentmilestone"].ToString());
                                                    estimate.AllowUndoSetContract = bool.Parse(dr["allowtoundosetcontract"].ToString());
                                                    estimate.AllowUndoCurrentRevision = bool.Parse(dr["AllowUndoCurrentRevision"].ToString());

                                                    if (dr["DueDate"] != DBNull.Value)
                                                        estimate.DueDate = Convert.ToDateTime(dr["DueDate"]);

                                                    if (dr["AppointmentDateTime"] != DBNull.Value)
                                                        estimate.AppointmentDate = Convert.ToDateTime(dr["AppointmentDateTime"]);

                                                    estimate.ContractStatusName = dr["ContractStatus"].ToString();
                                                    //if (dr["ContractStatus"] != DBNull.Value)
                                                    //{
                                                    //    switch (dr["ContractStatus"].ToString())
                                                    //    {
                                                    //        case "1":
                                                    //            estimate.ContractStatusName = "Pending";
                                                    //            break;
                                                    //        case "2":
                                                    //            estimate.ContractStatusName = "Cancelled";
                                                    //            break;
                                                    //        case "3":
                                                    //            estimate.ContractStatusName = "Work In Progress";
                                                    //            break;
                                                    //        case "4":
                                                    //            estimate.ContractStatusName = "On Hold";
                                                    //            break;
                                                    //        default:
                                                    //            break;
                                                    //    }
                                                    //}
                                                    estimate.BusinessUnit = dr["BusinessUnit"].ToString();
                                                    
                                                    //if (dr.GetSchemaTable().Columns["EditEstimateUserID"] != null) // Table.Columns["EditEstimateUserID"] != null
                                                    estimate.EditEstimateUserID = Convert.ToInt32(dr["EditEstimateUserID"].ToString());
                                                    estimate.EditEstimateUserName = dr["EditEstimateUserName"].ToString();

                                                    estimates.Add(estimate);
                                                }

                                                dr.Close();
                                            }

                                            conn.Close();
                                        }
                                    }
                

            //    stopwatch.Stop();
            //}
            //catch(Exception ex)
            //{
            //    stopwatch.Stop();
            //    string data = "Paameters are: revisionTypeId=" + revisionTypeId.ToString();
            //    data = data + ",roleId=" + roleId.ToString();
            //    data = data + ",statusId=" + statusId.ToString();
            //    data = data + ",userId=" + userId.ToString();
            //    data = data + ",regionId=" + regionId.ToString();
            //    data = data + ",customerNumber=" + customerNumber;
            //    data = data + ",contractNumber=" + contractNumber;
            //    data = data + ",salesConsultantId=" + salesConsultantId.ToString();
            //    data = data + ",lotNumber=" + lotNumber;
            //    data = data + ",streetName=" + streetName;
            //    data = data + ",suburb=" + suburb;
            //    data = data + ",district=" + district;
            //    data = data + ",opCentre=" + opCentre;
            //    data = data + ",Time elapsed=" + stopwatch.Elapsed.ToString();

            //    Trace.TraceError(ex.Message + Environment.NewLine + "SqlDataAccess-GetAssignedEstimates - Error - revisionTypeId={0}, roleId={1}, regionId={2}, customerNumber={3}, contractNumber={4}, salesConsultantId={5}, lotNumber={6}, streetName={7}, suburb={8}, district={9}, opCentre={10}", revisionTypeId, roleId, regionId, customerNumber, contractNumber, salesConsultantId, lotNumber, streetName, suburb, district, opCentre);

            //    Exception newException = new Exception(data, ex);
            //    throw newException;
            //}
            return estimates;
        }


        public EstimateHeader GetEstimateHeader(int revisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            EstimateHeader estimateHeader = new EstimateHeader();
            decimal gst = decimal.Parse("1.1");

                using (SqlConnection conn = new SqlConnection(connectionstring))
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateHeader";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows && dr.Read())
                        {
                            estimateHeader.CustomerName = dr["CustomerName"].ToString();
                            estimateHeader.CustomerNumber = Convert.ToInt32(dr["CustomerNumber"]);
                            estimateHeader.ContractNumber = Convert.ToInt32(dr["ContractNumber"]);
                            estimateHeader.EstimateId = Convert.ToInt32(dr["EstimateNumber"]);
                            estimateHeader.HomeName = dr["HomeName"].ToString();
                            estimateHeader.HomeRange = dr["brandname"].ToString();
                            estimateHeader.HomePrice = Convert.ToDecimal(dr["HomePrice"]);
                            estimateHeader.SalesConsultantName = dr["SalesConsultantName"].ToString();
                            estimateHeader.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            estimateHeader.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]);
                            estimateHeader.OwnerName = dr["OwnerName"].ToString();
                            estimateHeader.RecordId = Convert.ToInt32(dr["EstimateHeaderId"]);
                            estimateHeader.Region = dr["regionname"].ToString();
                            estimateHeader.UpgradeValue = Convert.ToDecimal(dr["UpgradeValue"]);
                            //if (dr.GetSchemaTable().Columns["PromotionValue"] != null)
                            //    estimateHeader.PromotionValue = Convert.ToDecimal(dr["PromotionValue"]);
                            estimateHeader.PromotionName = dr["currentpromotion"].ToString();
                            estimateHeader.SiteWorkValue = Convert.ToDecimal(dr["SiteWorkValue"]);
                            //estimateHeader.TotalPrice = estimateHeader.HomePrice + estimateHeader.UpgradeValue + estimateHeader.SiteWorkValue;
                            estimateHeader.TotalPrice = Convert.ToDecimal(dr["TotalPrice"]);
                            estimateHeader.TotalCostBTP = Convert.ToDecimal(dr["totalcostbtp"]);
                            estimateHeader.TotalCostDBC = Convert.ToDecimal(dr["totalcostdbc"]);
                            estimateHeader.TotalVariation = Convert.ToDecimal(dr["variationtotal"]);
                            estimateHeader.TotalPriceExc = Convert.ToDecimal(dr["TotalPrice"]) / gst;
                            estimateHeader.TotalMargin = estimateHeader.TotalPriceExc - estimateHeader.TotalCostDBC;
                            estimateHeader.Margin = Convert.ToDecimal(dr["margin"]);
                            estimateHeader.MarginString = dr["margin"].ToString() + "%";
                            estimateHeader.TargetMargin = Convert.ToDecimal(dr["TargetMargin"]);
                            estimateHeader.Comments = dr["Comments"].ToString();
                            estimateHeader.RevisionTypeId = Convert.ToInt32(dr["RevisionTypeId"]);
                            estimateHeader.RevisionTypeCode = dr["RevisionTypeCode"].ToString();
                            estimateHeader.LotNumber = dr["LotNumber"].ToString();
                            estimateHeader.StreetNumber = dr["StreetNumber"].ToString();
                            estimateHeader.StreetAddress = dr["StreetAddress"].ToString();
                            estimateHeader.Suburb = dr["Suburb"].ToString();
                            estimateHeader.PostCode = dr["Postcode"].ToString();
                            estimateHeader.State = dr["State"].ToString();
                            estimateHeader.SalesAcceptor = dr["SalesAcceptor"].ToString();
                            estimateHeader.DraftPerson = dr["DraftPerson"].ToString();
                            estimateHeader.SalesEstimator = dr["Salesestimator"].ToString();
                            estimateHeader.CSC = dr["CSC"].ToString();
                            estimateHeader.OwnerId = Convert.ToInt32(dr["OwnerId"]);
                            estimateHeader.StatusId = Convert.ToInt32(dr["StatusId"]);
                            estimateHeader.CustomerDocumentName = dr["CustomerDocumentName"].ToString();
                            estimateHeader.CustomerDocumentDesc = dr["CustomerDocumentDesc"].ToString();

                            if (dr["DifficultyRatingId"] != DBNull.Value)
                                estimateHeader.DifficultyRatingId = Convert.ToInt32(dr["DifficultyRatingId"]);
                            else
                                estimateHeader.DifficultyRatingId = 0;

                            if (dr["DueDate"] != DBNull.Value)
                                estimateHeader.DueDate = Convert.ToDateTime(dr["DueDate"]);

                            if (dr["AppointmentDateTime"] != DBNull.Value)
                                estimateHeader.AppointmentDate = Convert.ToDateTime(dr["AppointmentDateTime"]);

                            if (dr["EffectiveDate"] != DBNull.Value)
                                estimateHeader.EffectiveDate = Convert.ToDateTime(dr["EffectiveDate"]);

                            if (dr["DepositDate"] != DBNull.Value)
                                estimateHeader.DepositDate = Convert.ToDateTime(dr["DepositDate"]);
                            estimateHeader.HomeAndLandPackage = dr["HouseAndLandPackage"].ToString();

                            estimateHeader.ContractType = dr["ContractType"].ToString();
                            estimateHeader.JobFlowType = dr["JobFlowType"].ToString();
                            estimateHeader.Opportunityid = dr["fkidopportunity"].ToString();
                        }

                        dr.Close();
                    }

                    conn.Close();
                }

            return estimateHeader;
        }

        public List<EstimateDetails> GetEstimateDetails(int revisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateDetails> estimateDetails = new List<EstimateDetails>();

            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetEstimateDetails");
            SqlCmd.Parameters["@revisionId"].Value = revisionId;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateEstimateDetailsListFromDataSet(Sdr);
        }

        //public List<string[]> GetEstimateDetailsAsArray(int revisionId)
        //{
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        //    string[] resultarray = new string[13];
        //    List<string[]> estimateDetails = new List<string[]>();
        //    using (SqlConnection conn = new SqlConnection(connectionstring))
        //    {
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "sp_SalesEstimate_GetEstimateDetails";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));

        //            conn.Open();

        //            using (SqlDataReader dr = cmd.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    resultarray[0] = dr["AreaName"].ToString();
        //                    if (dr["NonStandardAreaName"] != DBNull.Value && dr["NonStandardAreaName"].ToString().Trim() != "")
        //                        resultarray[0] += " (" + dr["NonStandardAreaName"].ToString() + ")";

        //                    resultarray[1] = dr["GroupName"].ToString();
        //                    resultarray[2] = dr["ProductID"].ToString();
        //                    resultarray[3] = dr["ProductName"].ToString();
        //                    resultarray[4] = dr["ProductDescription"].ToString();
        //                    resultarray[5] = dr["ProductDescriptionshort"].ToString();
        //                    resultarray[6] = dr["ExtraDescription"].ToString();
        //                    resultarray[7] = dr["InternalDescription"].ToString();
        //                    resultarray[8] = dr["additionalinfo"].ToString();

        //                    resultarray[9] = dr["UOM"].ToString();
        //                    resultarray[10] = dr["Quantity"].ToString();
        //                    resultarray[11] = dr["ItemPrice"].ToString();
        //                    resultarray[11] = dr["ismasterpromotion"].ToString();

        //                    estimateDetails.Add(resultarray);
        //                }

        //                dr.Close();
        //            }

        //            conn.Close();
        //        }
        //    }
        //    return estimateDetails;
        //}

        public string CheckEstimateLockStatus(int estimaterevisionid)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string message = "";
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CheckEstimateLockStatus";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimaterevisionid", estimaterevisionid));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            message = dr["message"].ToString();
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return message;
        }

        public List<EstimateHeader> GetEstimateRevisions(int estimateId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            decimal gst = decimal.Parse("1.1");
            List<EstimateHeader> estimates = new List<EstimateHeader>();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateRevisions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            EstimateHeader estimate = new EstimateHeader();
                            estimate.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            estimate.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]);
                            estimate.OwnerName = dr["OwnerName"].ToString();
                            estimate.StatusName = dr["StatusName"].ToString();
                            estimate.RecordId = Convert.ToInt32(dr["EstimateHeaderID"]);
                            estimate.Comments = dr["Comments"].ToString();
                            estimate.RevisionTypeCode = dr["RevisionTypeCode"].ToString();
                            estimate.EstimateId = int.Parse(dr["fkidestimate"].ToString());
                            estimate.Margin = Convert.ToDecimal(dr["margin"]);
                            estimate.MarginString = Convert.ToDecimal(dr["margin"]) + "%";
                            estimate.TotalPrice = Convert.ToDecimal(dr["contractvalue"]);
                            estimate.TotalCostDBC = Convert.ToDecimal(dr["TotalCost"]);
                            estimate.TotalPriceExc = estimate.TotalPrice / gst;
                            estimate.TotalMargin = estimate.TotalPriceExc - estimate.TotalCostDBC;
                            estimate.MarginString = estimate.Margin.ToString() + "%";
                            estimate.RevisionTypeId = int.Parse(dr["revisiontypeid"].ToString());

                            estimates.Add(estimate);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return estimates;
        }

        public string UndoThisRevision(int bcContractNumber, int estimateId, int revisionId, int userId, string reasonComment)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string return_value = string.Empty;

                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_SalesEstimate_UndoThisRevision";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@bcContractNumber", bcContractNumber));
                        cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));
                        cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", revisionId));
                        cmd.Parameters.Add(new SqlParameter("@userId", userId));
                        cmd.Parameters.Add(new SqlParameter("@reasonComment", reasonComment));

                        conn.Open();

                        object response = cmd.ExecuteScalar();

                        if (response != DBNull.Value && response != null)
                            return_value = response.ToString();

                        conn.Close();
                    }
                }

            return return_value;
        }

        public string UndoCurrentMilestone(int revisionId, int userId, string reasonComment)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string return_value = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UndoCurrentMilestone";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", revisionId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    cmd.Parameters.Add(new SqlParameter("@reasonComment", reasonComment));

                    conn.Open();

                    object response = cmd.ExecuteScalar();

                    if (response != DBNull.Value && response != null)
                        return_value = response.ToString();

                    conn.Close();
                }
            }
            return return_value;
        }

        public string UndoSetAsContract(int revisionId, int userId, string reasonComment)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string return_value = string.Empty;

                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_SalesEstimate_UndoSetContract";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", revisionId));
                        cmd.Parameters.Add(new SqlParameter("@userId", userId));
                        cmd.Parameters.Add(new SqlParameter("@reasonComment", reasonComment));

                        conn.Open();

                        object response = cmd.ExecuteScalar();

                        if (response != DBNull.Value && response != null)
                            return_value = response.ToString();

                        conn.Close();
                    }
                }

            return return_value;
        }
        public List<EstimateHeader> UndoThisRevisionValidate(int estimateId, int bcContractNumber, int revisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            decimal gst = decimal.Parse("1.1");
            List<EstimateHeader> estimates = new List<EstimateHeader>();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UndoThisRevisionValidate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));
                    cmd.Parameters.Add(new SqlParameter("@bcContractNumber", bcContractNumber));
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", revisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            EstimateHeader estimate = new EstimateHeader();
                            estimate.CreatedOn = Convert.ToDateTime(dr["CreatedOn"]);
                            estimate.RevisionNumber = Convert.ToInt32(dr["RevisionNumber"]);
                            estimate.OwnerName = dr["OwnerName"].ToString();
                            estimate.StatusName = dr["StatusName"].ToString();
                            estimate.RecordId = Convert.ToInt32(dr["EstimateHeaderID"]);
                            estimate.Comments = dr["Comments"].ToString();
                            estimate.RevisionTypeCode = dr["RevisionTypeCode"].ToString();
                            estimate.EstimateId = int.Parse(dr["fkidestimate"].ToString());
                            estimate.Margin = Convert.ToDecimal(dr["margin"]);
                            estimate.MarginString = Convert.ToDecimal(dr["margin"]) + "%";
                            estimate.TotalPrice = Convert.ToDecimal(dr["contractvalue"]);
                            estimate.TotalCostDBC = Convert.ToDecimal(dr["TotalCost"]);
                            estimate.TotalPriceExc = estimate.TotalPrice / gst;
                            estimate.TotalMargin = estimate.TotalPriceExc - estimate.TotalCostDBC;
                            estimate.MarginString = estimate.Margin.ToString() + "%";
                            estimate.RevisionTypeId = int.Parse(dr["revisiontypeid"].ToString());

                            estimates.Add(estimate);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return estimates;
        }

        public string GetManagersByUserAndRevisionId(int userId, int revisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string managerIds = string.Empty;
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetManagersByUserAndRevisionId";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            if (managerIds == string.Empty)
                            {
                                managerIds = dr["ManagerUserId"].ToString();
                            }
                            else
                            {
                                managerIds += "," + dr["ManagerUserId"].ToString();
                            }
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return managerIds;
        }

        //public int InsertProduct(int revisionId, int estimateDetailsId, int userId)
        //{
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        //    int revisionDetailsId = 0;

        //    using (SqlConnection conn = new SqlConnection(connectionstring))
        //    {
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "sp_SalesEstimate_InsertEstimateDetails";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));
        //            cmd.Parameters.Add(new SqlParameter("@estimateDetailsId", estimateDetailsId));
        //            cmd.Parameters.Add(new SqlParameter("@userId", userId));
        //            SqlParameter param = cmd.Parameters.Add(new SqlParameter("@estimateRevisionDetailsId", SqlDbType.Int));
        //            param.Direction = ParameterDirection.Output;

        //            conn.Open();
        //            cmd.ExecuteNonQuery();

        //            revisionDetailsId = (int)param.Value;

        //            conn.Close();
        //        }
        //    }

        //    return revisionDetailsId;
        //}

        public void UpdateEstimateDetails(
            int revisionDetailsId,
            decimal price,
            decimal quantity,
            decimal totalprice,
            string productDescription,
            string extraDescription,
            string internalDescription,
            string additionalnotes,
            string studioManswer,
            int itemaccepted,
            int categoryid,
            int groupid,
            int pricedisplayid,
            int userId,
            int applyanswertoallgroup,
            string selectedimageid,
            bool issiteworkitem,
            string costbtp,
            string costdbc)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            int sitework = 0;
            if (issiteworkitem) sitework = 1;
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionDetailsId", revisionDetailsId));
                    cmd.Parameters.Add(new SqlParameter("@price", price));
                    cmd.Parameters.Add(new SqlParameter("@quantity", quantity));
                    cmd.Parameters.Add(new SqlParameter("@productDescription", productDescription));
                    cmd.Parameters.Add(new SqlParameter("@extraDescription", extraDescription));
                    cmd.Parameters.Add(new SqlParameter("@internalDescription", internalDescription));
                    cmd.Parameters.Add(new SqlParameter("@studioManswer", studioManswer));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    cmd.Parameters.Add(new SqlParameter("@itemaccepted", itemaccepted));
                    cmd.Parameters.Add(new SqlParameter("@categoryid", categoryid));
                    cmd.Parameters.Add(new SqlParameter("@groupid", groupid));
                    cmd.Parameters.Add(new SqlParameter("@pricedisplayid", pricedisplayid));
                    cmd.Parameters.Add(new SqlParameter("@applyanswertoallgroup", applyanswertoallgroup));
                    cmd.Parameters.Add(new SqlParameter("@additionalnotes", additionalnotes));
                    cmd.Parameters.Add(new SqlParameter("@selectedimageid", selectedimageid));
                    cmd.Parameters.Add(new SqlParameter("@issiteworkitem", sitework));
                    cmd.Parameters.Add(new SqlParameter("@costbtp", costbtp));
                    cmd.Parameters.Add(new SqlParameter("@costdbc", costdbc));
                    cmd.Parameters.Add(new SqlParameter("@totalprice", totalprice));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public EstimateDetails GetPagByID(int estimaterevisionid, int optionid)
        {
            EstimateDetails details = new EstimateDetails();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetPagByID");
                SqlCmd.Parameters["@revisionId"].Value = estimaterevisionid;
                SqlCmd.Parameters["@optionId"].Value = optionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                DataRow dr = Sdr.Tables[0].Rows[0];

                details.AreaName = dr["AreaName"].ToString();
                if (dr["NonStandardAreaName"] != DBNull.Value && dr["NonStandardAreaName"].ToString().Trim() != "")
                    details.AreaName += " (" + dr["NonStandardAreaName"].ToString() + ")";

                details.GroupName = dr["GroupName"].ToString();
                details.AreaId = Convert.ToInt32(dr["AreaId"]);
                details.GroupId = Convert.ToInt32(dr["GroupId"]);
                details.ProductId = dr["ProductID"].ToString();
                details.ProductName = dr["ProductName"].ToString();
                details.ProductDescription = dr["ProductDescription"].ToString();
                details.ProductDescriptionShort = dr["ProductDescriptionshort"].ToString();
                details.ExtraDescription = dr["ExtraDescription"].ToString();
                details.InternalDescription = dr["InternalDescription"].ToString();
                details.UpdatedProductDescription = dr["ProductDescription"].ToString();
                details.UpdatedExtraDescription = dr["ExtraDescription"].ToString();
                details.UpdatedInternalDescription = dr["InternalDescription"].ToString();
                details.AdditionalNotes = dr["additionalinfo"].ToString();
                details.UpdatedAdditionalNotes = dr["additionalinfo"].ToString();
                details.Uom = dr["UOM"].ToString();
                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                details.Price = Convert.ToDecimal(dr["ItemPrice"]);
                details.UpdatedQuantity = Convert.ToDecimal(dr["Quantity"]);
                details.UpdatedPrice = Convert.ToDecimal(dr["ItemPrice"]);
                details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                details.HomeDisplayOptionId = Convert.ToInt32(dr["OptionId"]);
                //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
                details.EstimateRevisionDetailsId = Convert.ToInt32(dr["EstimateRevisionDetailsId"]);
                details.TotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.UpdatedTotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.CreatedByUserId = Convert.ToInt32(dr["CreatedBy"]);
                details.CreatedByUserManagerIds = "";
                details.ItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
                details.UpdatedItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
                details.NonstandardCategoryID = Convert.ToInt32(dr["NonstandardCategoryID"]);
                details.NonstandardGroupID = Convert.ToInt32(dr["nonstandardgroupid"]);
                details.UpdatedNonstandardCategoryID = Convert.ToInt32(dr["NonstandardCategoryID"]);
                details.UpdatedNonstandardGroupID = Convert.ToInt32(dr["NonstandardCategoryID"]);
                details.StandardInclusionId = Convert.ToInt32(dr["idstandardinclusions"]);

                details.StudioMProduct = Convert.ToBoolean(dr["isstudiomproduct"]);
                details.ProductPhotoCount = int.Parse(dr["imagecount"].ToString());
                details.StudioMQuestion = dr["studiomquestion"].ToString();
                details.StudioMAnswer = dr["studiomanswer"].ToString();
                details.SelectedImageID = dr["selectedimageid"].ToString();
                details.SiteWorkItem = bool.Parse(dr["SiteWorkItem"].ToString());
                details.SOSI = dr["SOSI"].ToString();
                details.SOSIToolTips = dr["SOSIToolTips"].ToString();
                details.StudioMIcon = dr["StudioMIcon"].ToString();
                details.StudioMTooltips = dr["StudioMTooltips"].ToString();
                details.Changed = bool.Parse(dr["changed"].ToString());
                details.PreviousChanged = bool.Parse(dr["PreviousChanged"].ToString());
                details.StudioMAnswerMandatory = bool.Parse(dr["qandamandatory"].ToString());
                if (dr["changeprice"].ToString() == "0")
                {
                    details.ItemAllowToChangePrice = false;
                }
                else
                {
                    details.ItemAllowToChangePrice = true;
                }
                if (dr["changeqty"].ToString() == "0")
                {
                    details.ItemAllowToChangeQuantity = false;
                }
                else
                {
                    details.ItemAllowToChangeQuantity = true;
                }

                if (dr["changedisplaycode"].ToString() == "0")
                {
                    details.ItemAllowToChangeDisplayCode = false;
                }
                else
                {
                    details.ItemAllowToChangeDisplayCode = true;
                }

                if (dr["changeproductstandarddescription"].ToString() == "0")
                {
                    details.ItemAllowToChangeDescription = false;
                }
                else
                {
                    details.ItemAllowToChangeDescription = true;
                }


                details.ItemAllowToRemove = bool.Parse(dr["allowtoremove"].ToString());

                if (dr["studiomsortorder"] != DBNull.Value)
                    details.StudioMSortOrder = Convert.ToInt32(dr["studiomsortorder"]);
                else
                    details.StudioMSortOrder = 99999;

                if (dr["derivedcost"].ToString() == "0" || dr["derivedcost"].ToString().ToUpper() == "FALSE")
                {
                    details.DerivedCost = false;
                }
                else
                {
                    details.DerivedCost = true;
                }

                if (dr["CostExcGST"] != null)
                {
                    details.CostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedBTPCostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedDBCCostExcGST = dr["DBCCostExcGST"].ToString();
                }
                else
                {
                    details.CostExcGST = "";
                    details.UpdatedBTPCostExcGST = "";
                    details.UpdatedDBCCostExcGST = "";
                }
            }
            catch (Exception)
            {
                details = null;
            }
            return details;
        }

        public List<OptionTreeProducts> GetOptionTreeFromAllProducts(string regionid, string searchText)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetProducts");
            SqlCmd.Parameters["@regionid"].Value = regionid;
            SqlCmd.Parameters["@searchText"].Value = searchText;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateOptionTreeProductsListFromDataSet(Sdr);
        }

        public List<OptionTreeProducts> GetOptionTreeFromAllProductsExtended(int stateid, string regionid, int homeid, string productname, string productdesc, int areaid, int groupid)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_OnlinePriceBook");
            SqlCmd.Parameters["@stateID"].Value = stateid;
            SqlCmd.Parameters["@regionID"].Value = regionid;
            SqlCmd.Parameters["@homeID"].Value = homeid;
            SqlCmd.Parameters["@productname"].Value = productname;
            SqlCmd.Parameters["@productdesc"].Value = productdesc;
            SqlCmd.Parameters["@areaID"].Value = areaid;
            SqlCmd.Parameters["@groupID"].Value = groupid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateOptionTreeProductsListOnlinePriceBookFromDataSet(Sdr);
        }

        public EstimateDetails DeleteProduct(int revisionDetailsId, string reason, int reasonid, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            EstimateDetails details = new EstimateDetails();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_SalesEstimate_DeleteEstimateDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@revisionDetailsId", revisionDetailsId));
                        cmd.Parameters.Add(new SqlParameter("@userId", userId));
                        cmd.Parameters.Add(new SqlParameter("@reason", reason));
                        cmd.Parameters.Add(new SqlParameter("@reasonid", reasonid));

                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                details.AreaName = dr["AreaName"].ToString();
                                details.GroupName = dr["GroupName"].ToString();
                                details.AreaId = Convert.ToInt32(dr["AreaId"]);
                                details.GroupId = Convert.ToInt32(dr["GroupId"]);
                                details.ProductId = dr["ProductID"].ToString();
                                details.ProductName = dr["ProductName"].ToString();
                                details.ProductDescription = dr["ProductDescription"].ToString();
                                details.ProductDescriptionShort = dr["ProductDescriptionShort"].ToString();
                                details.ExtraDescription = dr["EnterDesc"].ToString();
                                details.Uom = dr["UOM"].ToString();
                                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                                details.UpdatedQuantity = details.Quantity;
                                details.Price = Convert.ToDecimal(dr["SellPrice"]);
                                details.UpdatedPrice = details.Price;
                                details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                                details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                                //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
                                details.TotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                                details.UpdatedTotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                                details.ItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
                            }

                            dr.Close();
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return details;
        }

        //public EstimateDetails DeleteProducts(int revisionDetailsId, string estimatedetailsidstring, string areaidstring, string groupidstring, string reason, int reasonid, int userId)
        //{
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        //    EstimateDetails details = new EstimateDetails();

        //    try
        //    {
        //        using (SqlConnection conn = new SqlConnection(connectionstring))
        //        {
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = "sp_SalesEstimate_DeleteEstimateDetails_Multiple";
        //                cmd.CommandType = CommandType.StoredProcedure;
        //                cmd.Parameters.Add(new SqlParameter("@revisionDetailsId", revisionDetailsId));
        //                cmd.Parameters.Add(new SqlParameter("@estimatedetailsidstring", estimatedetailsidstring));
        //                cmd.Parameters.Add(new SqlParameter("@areaidstring", areaidstring));
        //                cmd.Parameters.Add(new SqlParameter("@groupidstring", groupidstring));
        //                cmd.Parameters.Add(new SqlParameter("@userId", userId));
        //                cmd.Parameters.Add(new SqlParameter("@reason", reason));
        //                cmd.Parameters.Add(new SqlParameter("@reasonid", reasonid));

        //                conn.Open();
        //                using (SqlDataReader dr = cmd.ExecuteReader())
        //                {
        //                    while (dr.Read())
        //                    {
        //                        details.AreaName = dr["AreaName"].ToString();
        //                        details.GroupName = dr["GroupName"].ToString();
        //                        details.AreaId = Convert.ToInt32(dr["AreaId"]);
        //                        details.GroupId = Convert.ToInt32(dr["GroupId"]);
        //                        details.ProductId = dr["ProductID"].ToString();
        //                        details.ProductName = dr["ProductName"].ToString();
        //                        details.ProductDescription = dr["ProductDescription"].ToString();
        //                        details.ProductDescriptionShort = dr["ProductDescriptionShort"].ToString();
        //                        details.ExtraDescription = dr["EnterDesc"].ToString();
        //                        details.Uom = dr["UOM"].ToString();
        //                        details.Quantity = Convert.ToDecimal(dr["Quantity"]);
        //                        details.UpdatedQuantity = details.Quantity;
        //                        details.Price = Convert.ToDecimal(dr["SellPrice"]);
        //                        details.UpdatedPrice = details.Price;
        //                        details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
        //                        details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
        //                        //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
        //                        details.TotalPrice = details.Quantity * details.Price;
        //                        details.UpdatedTotalPrice = details.Quantity * details.Price;
        //                        details.ItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
        //                    }

        //                    dr.Close();
        //                }
        //                conn.Close();
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //    return details;
        //}

        public void InsertComment(int revisionId, string comment, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_InsertComment";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));
                    cmd.Parameters.Add(new SqlParameter("@comment", comment));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void UpdateComment(int estimateRevisionId, string comments, int userid, int variationnumber, string variationsummary)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateComment";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@comments", comments));
                    cmd.Parameters.Add(new SqlParameter("@variationnumber", variationnumber));
                    cmd.Parameters.Add(new SqlParameter("@variationsummary", variationsummary));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }


        public void UpdateEstimateDifficultyRating(int estimateRevisionId, int difficultyRatingId, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateDifficultyRating";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@difficultyRatingId", difficultyRatingId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }


        public void UpdateQueueDifficultyRating(int queueId, int difficultyRatingId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateQueueDifficultyRating";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@queueId", queueId));
                    cmd.Parameters.Add(new SqlParameter("@difficultyRatingId", difficultyRatingId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void MarginReport_SaveDetails(int estimateRevisionId, int titledLand, int titledLandDays, int basePriceExtensionDays, DateTime effectiveDate, double bpeCharge, int requiredBPEChargeType, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_MarginReport_SaveDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@titledLand", titledLand));
                    cmd.Parameters.Add(new SqlParameter("@titledLandDays", titledLandDays));
                    cmd.Parameters.Add(new SqlParameter("@basePriceExtensionDays", basePriceExtensionDays));
                    cmd.Parameters.Add(new SqlParameter("@effectiveDate", effectiveDate));
                    cmd.Parameters.Add(new SqlParameter("@bpeCharge", bpeCharge));
                    cmd.Parameters.Add(new SqlParameter("@requiredBPEChargeType", requiredBPEChargeType));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public MarginReportDetail MarginReport_GetDetails(int estimateRevisionId)
        {
            MarginReportDetail marginRptDet = new MarginReportDetail();

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            EstimateDetails details = new EstimateDetails();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_SalesEstimate_MarginReport_GetDetails";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));

                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                marginRptDet.RevisionID = estimateRevisionId;
                                marginRptDet.BPECharge = double.Parse(dr["BPECharge"].ToString());
                                marginRptDet.DaysFrom = int.Parse(dr["DaysFrom"].ToString());
                                if (dr["TitledLand"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["TitledLand"].ToString()))
                                    marginRptDet.TitledLand = bool.Parse(dr["TitledLand"].ToString());
                                if (dr["TitledLandDays"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["TitledLandDays"].ToString()))
                                    marginRptDet.TitledLandDays = int.Parse(dr["TitledLandDays"].ToString());
                                if (dr["BasePriceExtensionDays"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["BasePriceExtensionDays"].ToString()))
                                    marginRptDet.BasePriceExtensionDays = int.Parse(dr["BasePriceExtensionDays"].ToString());
                                if (dr["RequiredBPEChargeType"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["RequiredBPEChargeType"].ToString()))
                                    marginRptDet.RequiredBPEChargeType = int.Parse(dr["RequiredBPEChargeType"].ToString());
                                if (dr["RequiredBPECharge"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["RequiredBPECharge"].ToString()))
                                    marginRptDet.RequiredBPECharge = double.Parse(dr["RequiredBPECharge"].ToString());
                                if (dr["TodaysPrice"] != DBNull.Value && !string.IsNullOrWhiteSpace(dr["TodaysPrice"].ToString()))
                                    marginRptDet.TodaysPrice = double.Parse(dr["TodaysPrice"].ToString());
                                marginRptDet.BCForecastDate = GetBCForecastDate(dr["BCContractNumber"].ToString());
                                marginRptDet.PriceEffectiveDates = GetHomePrices(estimateRevisionId);
                                break;
                            }

                            dr.Close();
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // throw;
            }

            return marginRptDet;
        }

        public string GetBCForecastDate(string bcContractNumber)
        {
            string bcForecastDate = string.Empty;

            BcDataAccess bcData = new BcDataAccess();
            bcForecastDate = bcData.GetBCForecastDate(bcContractNumber);

            return bcForecastDate;
        }

        public void UpdateEstimateDueDate(int estimateRevisionId, DateTime duedate, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateDueDate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@duedate", duedate));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }


        public void UpdateEstimateAppointmentTime(int estimateRevisionId, DateTime appointmentTime, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateAppointmentTime";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@appointmentTime", appointmentTime));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void UpdateQueueDueDate(int queueId, DateTime duedate, int userid)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateQueueDueDate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@queueId", queueId));
                    cmd.Parameters.Add(new SqlParameter("@duedate", duedate));
                    cmd.Parameters.Add(new SqlParameter("@userid", userid));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }


        public void UpdateEstimateStatus(int estimateRevisionId, int statusId, int statusReasonId, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateStatus";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@statusId", statusId));
                    cmd.Parameters.Add(new SqlParameter("@statusReasonId", statusReasonId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }


        public void UpdateEstimateEffectiveDate(int estimateRevisionId, int priceId, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateEffectiveDate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@priceId", priceId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        //public bool CheckEstimateUpdatability(int revisionId)
        //{
        ////    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        ////    bool updateable = false;

        ////    using (SqlConnection conn = new SqlConnection(connectionstring))
        ////    {
        ////        using (SqlCommand cmd = conn.CreateCommand())
        ////        {
        ////            cmd.CommandText = "sp_SalesEstimate_CheckEstimateUpdatability";
        ////            cmd.CommandType = CommandType.StoredProcedure;
        ////            cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));

        ////            SqlParameter updatability = new SqlParameter("@updatable", updateable);
        ////            updatability.Direction = ParameterDirection.Output;
        ////            cmd.Parameters.Add(updatability);

        ////            conn.Open();
        ////            cmd.ExecuteNonQuery();

        ////            updateable = Boolean.Parse(cmd.Parameters["@updatable"].Value.ToString());

        ////            conn.Close();
        ////        }
        ////    }
        ////    return updateable;

        //    return false;
        //}

        //public void ReEditEstimate(int revisionId, int userId)
        //{
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

        //    using (SqlConnection conn = new SqlConnection(connectionstring))
        //    {
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "sp_SalesEstimate_ReEditSalesEstimate";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));
        //            cmd.Parameters.Add(new SqlParameter("@userId", userId));

        //            conn.Open();
        //            cmd.ExecuteNonQuery();
        //            conn.Close();
        //        }
        //    }
        //}

        public List<User> GetUsersByRegionAndRole(int regionId, int roleId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<User> users = new List<User>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetUsersByRegionAndRole";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@regionId", regionId));
                    cmd.Parameters.Add(new SqlParameter("@roleId", roleId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            User user = new User();
                            user.UserId = Convert.ToInt32(dr["UserID"]);
                            user.FullName = dr["FullName"].ToString();

                            users.Add(user);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return users;
        }


        public List<User> GetUsersByRegionAndRevisionType(int regionId, int revisionTypeId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<User> users = new List<User>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetUsersByRegionAndRevisionType";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@regionId", regionId));
                    cmd.Parameters.Add(new SqlParameter("@revisionTypeId", revisionTypeId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            User user = new User();
                            user.UserId = Convert.ToInt32(dr["UserID"]);
                            user.FullName = dr["FullName"].ToString();

                            users.Add(user);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return users;
        }

        public User GetUserById(int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            User user = null;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetUserById";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            user = new User();
                            user.UserId = Convert.ToInt32(dr["UserID"]);
                            user.FullName = dr["FullName"].ToString();
                            user.RegionId = Convert.ToInt32(dr["RegionID"]);
                            user.StateId = Convert.ToInt32(dr["StateID"]);
                            user.RegionName = dr["RegionName"].ToString();

                            if (dr["EmailAddress"] != DBNull.Value)
                                user.EmailAddress = dr["EmailAddress"].ToString();

                            if (dr["PrimaryRoleId"] != DBNull.Value)
                                user.PrimaryRoleId = Convert.ToInt32(dr["PrimaryRoleId"]);
                            else
                                user.PrimaryRoleId = 0;
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return user;
        }

        public User GetUserByLoginName(string loginName)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            User user = null;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetUserByLoginName";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@loginName", loginName));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            user = new User();
                            user.UserId = Convert.ToInt32(dr["UserID"]);
                            user.FullName = dr["FullName"].ToString();
                            user.RegionId = Convert.ToInt32(dr["RegionID"]);
                            user.LoginPriceRegionId = Convert.ToInt32(dr["PriceRegionID"]);
                            user.StateId = Convert.ToInt32(dr["StateID"]);
                            user.RegionName = dr["RegionName"].ToString();
                            if (dr["PrimaryRoleId"] != DBNull.Value)
                                user.PrimaryRoleId = Convert.ToInt32(dr["PrimaryRoleId"]);
                            else
                                user.PrimaryRoleId = 0;
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return user;
        }

        public List<UserRole> GetUserRoles(int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<UserRole> roles = new List<UserRole>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetUserRoles";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            UserRole role = new UserRole();
                            role.RoleId = Convert.ToInt32(dr["RoleID"]);
                            role.RoleName = dr["RoleName"].ToString();
                            role.IsManager = Convert.ToBoolean(dr["IsManager"]);

                            roles.Add(role);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return roles;
        }

        public List<StatusReason> GetStatusReasons(int statusId, int revisionTypeId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<StatusReason> reasons = new List<StatusReason>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetStatusReasons";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@statusId", statusId));
                    cmd.Parameters.Add(new SqlParameter("@revisionTypeId", revisionTypeId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            StatusReason reason = new StatusReason();
                            reason.StatusReasonId = Convert.ToInt32(dr["StatusReasonId"]);
                            reason.StatusReasonName = dr["StatusReasonName"].ToString();


                            reasons.Add(reason);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return reasons;
        }

        public List<SQSConfiguration> GetSQSConfiguration(string configCode, string codeValue = "")
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<SQSConfiguration> sqsConfigurations = new List<SQSConfiguration>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_GetSQSConfiguration";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@configCode", configCode));
                    cmd.Parameters.Add(new SqlParameter("@codeValue", codeValue));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            SQSConfiguration config = new SQSConfiguration();
                            config.CodeValue = dr["CodeValue"].ToString();
                            config.CodeText = dr["CodeText"].ToString();


                            sqsConfigurations.Add(config);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return sqsConfigurations;
        }


        public List<RevisionType> GetRevisionTypeAccess(int roleId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<RevisionType> types = new List<RevisionType>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetRevisionTypeAccess";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@roleId", roleId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            RevisionType type = new RevisionType();
                            type.RevisionTypeId = Convert.ToInt32(dr["RevisionTypeID"]);
                            type.RevisionTypeName = dr["RevisionTypeName"].ToString();

                            types.Add(type);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return types;
        }

        public List<EstimateStatus> GetEstimateStatuses()
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateStatus> statuses = new List<EstimateStatus>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateStatuses";
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            EstimateStatus status = new EstimateStatus();
                            status.StatusId = Convert.ToInt32(dr["StatusID"]);
                            status.StatusName = dr["StatusName"].ToString();

                            statuses.Add(status);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return statuses;
        }

        public List<DifficultyRating> GetDifficultyRatings()
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<DifficultyRating> ratings = new List<DifficultyRating>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetDifficultyRatings";
                    cmd.CommandType = CommandType.StoredProcedure;

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DifficultyRating rating = new DifficultyRating();
                            rating.DifficultyRatingId = Convert.ToInt32(dr["DifficultyRatingId"]);
                            rating.DifficultyRatingName = dr["DifficultyRatingName"].ToString() + " (" + dr["DifficultyRatingDescription"].ToString() + ")";

                            ratings.Add(rating);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return ratings;
        }

        public List<HomePrice> GetHomePrices(int estimateRevisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<HomePrice> prices = new List<HomePrice>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetHomePrices";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            HomePrice price = new HomePrice();
                            price.PriceId = Convert.ToInt32(dr["PriceID"]);
                            price.EffectiveDate = Convert.ToDateTime(dr["EffectiveDate"]);
                            price.EffectivePrice = Convert.ToDecimal(dr["PromotionPrice"]);
                            price.EffectiveDateOptionName = price.EffectiveDate.ToString("dd/MM/yyyy") + " (" + price.EffectivePrice.ToString("c") + ")";
                            prices.Add(price);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return prices;
        }

        public List<AuditLog> GetAuditTrail(int estimateId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<AuditLog> logs = new List<AuditLog>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetAuditTrail";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            AuditLog log = new AuditLog();
                            log.Action = dr["Action"].ToString();
                            log.Description = dr["Description"] != null ? dr["Description"].ToString() : string.Empty;
                            log.LogId = dr["LogId"].ToString();
                            log.LogTime = Convert.ToDateTime(dr["LogTime"]).ToLocalTime(); // CRM stores DateTime in GMT 
                            log.RevisionNumber = dr["RevisionNumber"].ToString();
                            log.RevisionType = dr["RevisionType"].ToString();
                            log.User = dr["User"].ToString();
                            log.EstimateNumber = dr["estimatenumber"].ToString();
                            logs.Add(log);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return logs;
        }

        public List<EstimateDetailsComparison> CompareEstimateDetails(int estimateRevisionIdA, int estimateRevisionIdB)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateDetailsComparison> estimateDetails = new List<EstimateDetailsComparison>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CompareSaleEstimateDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionIdA", estimateRevisionIdA));
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionIdB", estimateRevisionIdB));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            EstimateDetailsComparison estimateDetail = new EstimateDetailsComparison();
                            estimateDetail.PagID = dr["pagid"].ToString();
                            estimateDetail.AreaName = dr["AreaName"] != DBNull.Value ? dr["AreaName"].ToString() : null;
                            estimateDetail.GroupName = dr["GroupName"] != DBNull.Value ? dr["GroupName"].ToString() : null;

                            estimateDetail.ProductNameA = dr["ProductNameA"] != DBNull.Value ? dr["ProductNameA"].ToString() : null;
                            estimateDetail.UomA = dr["UomA"] != DBNull.Value ? dr["UomA"].ToString() : null;
                            estimateDetail.PriceA = dr["PriceA"] != DBNull.Value ? Convert.ToDecimal(dr["PriceA"]).ToString("c") : null;
                            estimateDetail.QuantityA = dr["QuantityA"] != DBNull.Value ? Convert.ToDecimal(dr["QuantityA"]).ToString() : null;

                            estimateDetail.ProductNameB = dr["ProductNameB"] != DBNull.Value ? dr["ProductNameB"].ToString() : null;
                            estimateDetail.UomB = dr["UomB"] != DBNull.Value ? dr["UomB"].ToString() : null;
                            estimateDetail.PriceB = dr["PriceB"] != DBNull.Value ? Convert.ToDecimal(dr["PriceB"]).ToString("c") : null;
                            estimateDetail.QuantityB = dr["QuantityB"] != DBNull.Value ? Convert.ToDecimal(dr["QuantityB"]).ToString() : null;
                            estimateDetail.Reason = dr["reason"] != DBNull.Value ? dr["reason"].ToString() : null;

                            //product A
                            if (dr["ProductDescriptionA"] != null && dr["ProductDescriptionA"].ToString().Trim() != "")
                                estimateDetail.ProductDescriptionA = dr["ProductDescriptionA"].ToString();

                            if (dr["ExtraDescriptionA"] != null && dr["ExtraDescriptionA"].ToString().Trim() != "")
                                estimateDetail.ProductDescriptionA = estimateDetail.ProductDescriptionA +  Environment.NewLine+ "Extra Desc:" + Environment.NewLine  + dr["ExtraDescriptionA"].ToString();

                            if (dr["AdditionalInfoA"] != null && dr["AdditionalInfoA"].ToString().Trim() != "")
                                estimateDetail.ProductDescriptionA = estimateDetail.ProductDescriptionA + Environment.NewLine + "Additional Notes:"+ Environment.NewLine + dr["AdditionalInfoA"].ToString();

                            // product B
                            if (dr["ProductDescriptionB"] != null && dr["ProductDescriptionB"].ToString().Trim() != "")
                                estimateDetail.ProductDescriptionB = dr["ProductDescriptionB"].ToString();

                            if (dr["ExtraDescriptionB"] != null && dr["ExtraDescriptionB"].ToString().Trim() != "")
                                estimateDetail.ProductDescriptionB = estimateDetail.ProductDescriptionB + Environment.NewLine + "Extra Desc:" + Environment.NewLine + dr["ExtraDescriptionB"].ToString();

                            if (dr["AdditionalInfoB"] != null && dr["AdditionalInfoB"].ToString().Trim() != "")
                                estimateDetail.ProductDescriptionB = estimateDetail.ProductDescriptionB + Environment.NewLine + "Additional Notes:"+ Environment.NewLine + dr["AdditionalInfoB"].ToString();

                            if (estimateDetail.ProductNameA != null && estimateDetail.ProductNameB != null)
                            {
                                estimateDetail.Changes = "";

                                if (dr["PriceA"].ToString() != dr["PriceB"].ToString())
                                    estimateDetail.Changes = "PRC";

                                if (dr["QuantityA"].ToString() != dr["QuantityB"].ToString())
                                    estimateDetail.Changes = estimateDetail.Changes == "" ? "QTY" : estimateDetail.Changes + ", QTY";

                                if (dr["ProductDescriptionA"].ToString() != dr["ProductDescriptionB"].ToString() ||
                                    dr["ExtraDescriptionA"].ToString() != dr["ExtraDescriptionB"].ToString() ||
                                    dr["InternalDescriptionA"].ToString() != dr["InternalDescriptionB"].ToString() ||
                                    dr["AdditionalInfoA"].ToString() != dr["AdditionalInfoB"].ToString())
                                    estimateDetail.Changes = estimateDetail.Changes == "" ? "DESC" : estimateDetail.Changes + ", DESC";

                                if (dr["StudioMAttributesA"].ToString() != dr["StudioMAttributesB"].ToString())
                                    estimateDetail.Changes = estimateDetail.Changes == "" ? "STUDIO M" : estimateDetail.Changes + ", STUDIO M";

                                if (dr["NonStandardAreaIdA"].ToString() != dr["NonStandardAreaIdB"].ToString())
                                    estimateDetail.Changes = estimateDetail.Changes == "" ? "AREA" : estimateDetail.Changes + ", AREA";

                                if (dr["NonStandardGroupIdA"].ToString() != dr["NonStandardGroupIdB"].ToString())
                                    estimateDetail.Changes = estimateDetail.Changes == "" ? "GROUP" : estimateDetail.Changes + ", GROUP";

                                if (dr["IsPromotionProductA"].ToString() != dr["IsPromotionProductB"].ToString())
                                    estimateDetail.Changes = estimateDetail.Changes == "" ? "PROMO" : estimateDetail.Changes + ", PROMO";

                            }
                            else
                                estimateDetail.Changes = "*";

                            estimateDetails.Add(estimateDetail);
                        }

                        dr.Close();
                    }
                }
                conn.Close();
            }

            return estimateDetails;
        }

        public void GetEstimateForLogging(int estimateRevisionId, out Guid contractId, out string revisionType, out int revisionNumber, out int estimateNumber)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateHeaderForLogging";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Guid.TryParse(dr["ContractID"].ToString(), out contractId);
                            revisionType = dr["RevisionType"].ToString();
                            Int32.TryParse(dr["RevisionNumber"].ToString(), out revisionNumber);
                            Int32.TryParse(dr["EstimateNumber"].ToString(), out estimateNumber);
                        }
                        else
                        {
                            contractId = Guid.Empty;
                            revisionType = string.Empty;
                            revisionNumber = 0;
                            estimateNumber = 0;
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
        }


        public Guid GetEstimateContractId(int estimateRevisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            Guid contractId = Guid.Empty;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateHeaderForLogging";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Guid.TryParse(dr["ContractID"].ToString(), out contractId);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return contractId;
        }


        public List<NextRevision> GetNextEstimateRevision(int estimateRevisionId, int statusId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            List<NextRevision> revisionList = new List<NextRevision>();

            //using (SqlConnection conn = new SqlConnection(connectionstring))
            //{
            //    using (SqlCommand cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = "sp_SalesEstimate_GetNextEstimateRevisionType";
            //        cmd.CommandType = CommandType.StoredProcedure;

            //        cmd.Parameters.Add(new SqlParameter("@revisionId", estimateRevisionId));
            //        cmd.Parameters.Add(new SqlParameter("@statusId", statusId));

            //        //SqlParameter nextRevisionTypeParam = new SqlParameter("@nextRevisionTypeId", 0);
            //        //nextRevisionTypeParam.Direction = ParameterDirection.Output;
            //        //cmd.Parameters.Add(nextRevisionTypeParam);

            //        //SqlParameter nextRevisionOwnerParam = new SqlParameter("@nextRevisionOwnerId", 0);
            //        //nextRevisionOwnerParam.Direction = ParameterDirection.Output;
            //        //cmd.Parameters.Add(nextRevisionOwnerParam);

            //        conn.Open();

            //        using (SqlDataReader dr = cmd.ExecuteReader())
            //        {
            //            while (dr.Read())
            //            {
            //                NextRevision nextRevision = new NextRevision();

            //                if (dr["NextRevisionTypeId"] != DBNull.Value)
            //                    nextRevision.RevisionTypeId = Convert.ToInt32(dr["NextRevisionTypeId"]);

            //                if (dr["NewRevisionTypeName"] != DBNull.Value)
            //                    nextRevision.RevisionTypeName = dr["NewRevisionTypeName"].ToString();

            //                if (dr["NextRevisionOwnerId"] != DBNull.Value)
            //                    nextRevision.OwnerId = Convert.ToInt32(dr["NextRevisionOwnerId"]);

            //                if (dr["Notes"] != DBNull.Value)
            //                    nextRevision.Notes = dr["Notes"].ToString();

            //                revisionList.Add(nextRevision);
            //            }

            //            dr.Close();
            //        }

            //        conn.Close();

            //    }
            //}
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetNextEstimateRevisionType");
                SqlCmd.Parameters["@revisionId"].Value = estimateRevisionId;
                SqlCmd.Parameters["@statusId"].Value = statusId;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    NextRevision nextRevision = new NextRevision();

                    if (dr["NextRevisionTypeId"] != DBNull.Value)
                        nextRevision.RevisionTypeId = Convert.ToInt32(dr["NextRevisionTypeId"]);

                    if (dr["NewRevisionTypeName"] != DBNull.Value)
                        nextRevision.RevisionTypeName = dr["NewRevisionTypeName"].ToString();

                    if (dr["NextRevisionOwnerId"] != DBNull.Value)
                        nextRevision.OwnerId = Convert.ToInt32(dr["NextRevisionOwnerId"]);

                    if (dr["Notes"] != DBNull.Value)
                        nextRevision.Notes = dr["Notes"].ToString();

                    revisionList.Add(nextRevision);
                }
                stopwatch.Stop();
            }
            catch(Exception ex)
            {
                stopwatch.Stop();

                Trace.TraceError(ex.Message + Environment.NewLine + "SqlDataAccess-GetNextEstimateRevision - Error - estimateRevisionId={0}, statusId={1}", estimateRevisionId, statusId);

                Exception newException = new Exception("revisonid="+estimateRevisionId.ToString()+",statusid="+statusId.ToString()+", time elapsed="+ stopwatch.Elapsed.ToString(), ex);
                throw newException;
            }

            return revisionList;
        }

        /// <summary>
        /// Get the latest id_SalesEstimate_EstimateHeader from tbl_SalesEstimate_EstimateHeader for a specific Estimate Id
        /// </summary>
        /// <param name="estimateId"></param>
        /// <returns></returns>
        public int GetLatestEstimateRevisionId(int estimateId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            int estimateRevisionId = 0;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetLatestEstimateRevisionId";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));

                    conn.Open();

                    object result = cmd.ExecuteScalar();

                    conn.Close();

                    if (result != DBNull.Value)
                        estimateRevisionId = (int)result;
                }
            }
            return estimateRevisionId;
        }

        public int GetResubmittedEstimateCount(int userId, int regionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            int estimateCount = 0;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetResubmittedEstimateCount";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    cmd.Parameters.Add(new SqlParameter("@regionId", regionId));

                    conn.Open();

                    object result = cmd.ExecuteScalar();

                    conn.Close();

                    if (result != DBNull.Value)
                        estimateCount = (int)result;
                }
            }
            return estimateCount;
        }

        //public int GetOnHoldEstimateCount(string revisionTypeIds, //comma separated i.e. '1, 2'
        //                                int userId, 
        //                                int regionId)
        //{
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        //    int estimateCount = 0;

        //    using (SqlConnection conn = new SqlConnection(connectionstring))
        //    {
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "sp_SalesEstimate_GetOnHoldEstimateCount";
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            cmd.Parameters.Add(new SqlParameter("@revisionTypeIds", revisionTypeIds));
        //            cmd.Parameters.Add(new SqlParameter("@userId", userId));
        //            cmd.Parameters.Add(new SqlParameter("@regionId", regionId));

        //            conn.Open();

        //            object result = cmd.ExecuteScalar();

        //            conn.Close();

        //            if (result != DBNull.Value)
        //                estimateCount = (int)result;
        //        }
        //    }
        //    return estimateCount;
        //}

        //public List<PAG> GetOptionTree(string revisonid)
        //{
        //    List<PAG> l = new List<PAG>();
        //    decimal qty, sellprice;
        //    bool standardoption, promotionproduct, issiteworkitem, ismasterpromotion;
        //    int estimatedetailsid;
        //    try
        //    {
        //        SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_PopulateTreeForEstimate");
        //        SqlCmd.Parameters["@revisionid"].Value = revisonid;
        //        DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

        //        if (Sdr.Tables[0].Rows.Count > 0)
        //        {
        //            foreach (DataRow dr in Sdr.Tables[0].Rows)
        //            {
        //                try
        //                {
        //                    qty = decimal.Parse(dr["quantity"].ToString());
        //                }
        //                catch
        //                {
        //                    qty = 0;
        //                }
        //                try
        //                {
        //                    sellprice = decimal.Parse(dr["sellprice"].ToString());
        //                }
        //                catch
        //                {
        //                    sellprice = 0;
        //                }

        //                try
        //                {
        //                    estimatedetailsid = int.Parse(dr["estimatedetailsid"].ToString());
        //                }
        //                catch
        //                {
        //                    estimatedetailsid = 0;
        //                }
        //                try
        //                {
        //                    promotionproduct = bool.Parse(dr["promotionproduct"].ToString());
        //                }
        //                catch
        //                {
        //                    promotionproduct = false;
        //                }
        //                try
        //                {
        //                    standardoption = bool.Parse(dr["standardoption"].ToString());
        //                }
        //                catch
        //                {
        //                    standardoption = false;
        //                }
        //                try
        //                {
        //                    issiteworkitem = bool.Parse(dr["siteworkitem"].ToString());
        //                }
        //                catch
        //                {
        //                    issiteworkitem = false;
        //                }
        //                try
        //                {
        //                    ismasterpromotion = bool.Parse(dr["ismasterpromotion"].ToString());
        //                }
        //                catch
        //                {
        //                    ismasterpromotion = false;
        //                }


        //                l.Add(new PAG(dr["areaname"].ToString(), dr["groupname"].ToString(), dr["productname"].ToString(), qty, sellprice, estimatedetailsid, dr["enterdesc"].ToString(), "", promotionproduct, standardoption, issiteworkitem, ismasterpromotion));
        //            }
        //        }

        //    }
        //    catch (Exception)
        //    {
        //        l.Add(new PAG("", "", "", 0, 0, 0, "", "", false, false, false, false));
        //    }

        //    return l;
        //}

        //public List<EstimateDetails> GetOptionTreeAsEstimateDetails(string revisionId)
        //{
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        //    List<EstimateDetails> estimateDetails = new List<EstimateDetails>();

        //    SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_PopulateTreeForEstimate");
        //    SqlCmd.Parameters["@revisionId"].Value = revisionId;

        //    DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

        //    return PopulateEstimateDetailsListFromDataSet(Sdr);
        //}

        public List<OptionTreeProducts> GetOptionTreeFromMasterHome(string regionid, string homeid)
        {
            //string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            //List<OptionTreeProducts> list = new List<OptionTreeProducts>();

            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_PopulateTreeFromMasterHome");
            SqlCmd.Parameters["@regionid"].Value = regionid;
            SqlCmd.Parameters["@homeid"].Value = homeid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateOptionTreeProductsListFromDataSet(Sdr);
        }

        //public List<string[]> GetOptionTreeAsArray(string revisionId)
        //{
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
        //    List<string[]> estimateDetails = new List<string[]>();
        //    SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_PopulateTreeForEstimate");
        //    SqlCmd.Parameters["@revisionid"].Value = revisionId;
        //    DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

        //    string[] result = new string[10];
        //    if (Sdr.Tables[0].Rows.Count > 0)
        //    {

        //        foreach (DataRow dr in Sdr.Tables[0].Rows)
        //        {
        //            result[0] = dr["AreaName"].ToString();
        //            result[1] = dr["GroupName"].ToString();
        //            result[2] = dr["EstimateDetailsId"].ToString();
        //            result[3] = dr["ProductName"].ToString();
        //            result[4] = dr["ProductDescription"].ToString();

        //            result[5] = dr["uom"].ToString();
        //            result[6] = dr["Quantity"].ToString();
        //            result[7] = dr["sellprice"].ToString();
        //            result[8] = dr["isstudiomproduct"].ToString();
        //            result[9] = dr["StandardOption"].ToString();
        //            //result[5] = dr["ProductDescription"].ToString().Length > 100 ? dr["ProductDescription"].ToString().Substring(0, 100) + "..." : dr["ProductDescription"].ToString();
        //            //result[6] = dr["enterdesc"].ToString();

        //            //result[7] = dr["internaldescription"].ToString();

        //            //result[8] = dr["additionalinfo"].ToString();

        //            //result[9] = dr["uom"].ToString();
        //            //result[10] = dr["Quantity"].ToString();
        //            //result[11] = dr["sellprice"].ToString();
        //            //result[12] = dr["PromotionProduct"].ToString();
        //            //result[13] = dr["StandardOption"].ToString();
        //            //result[14] = dr["EstimateDetailsId"].ToString();
        //            //result[15] = dr["ProductAreaGroupID"].ToString();
        //            //result[16] = (decimal.Parse(dr["Quantity"].ToString()) * decimal.Parse(dr["Quantity"].ToString())).ToString();

        //            //result[17] = dr["ItemAccepted"].ToString();
        //            //result[18] = dr["isstudiomproduct"].ToString();
        //            //result[19] = dr["imagecount"].ToString();
        //            //result[20] = dr["studiomquestion"].ToString();
        //            //result[21] = dr["SiteWorkItem"].ToString();
        //            //result[22] = dr["idStandardInclusions"].ToString();
        //            //result[23] = dr["qandamandatory"].ToString();

        //            //result[24] = dr["SOSI"].ToString();
        //            //result[25] = dr["SOSIToolTips"].ToString();
        //            //result[26] = dr["StudioMIcon"].ToString();
        //            //result[27] = dr["StudioMTooltips"].ToString();
        //            //result[28] = "1"; // allow to remove

        //            //result[29] = dr["changeprice"].ToString();
        //            //result[30] = dr["changeqty"].ToString();


        //            //if (dr["studiomsortorder"] != DBNull.Value)
        //            //    result[31] = dr["studiomsortorder"].ToString();
        //            //else
        //            //     result[31] = "99999";

        //            //    //if (details.StandardOption)
        //            //    //{
        //            //    //    details.SOSI = "./images/upgrade.png";
        //            //    //    details.SOSIToolTips = "Standard Option.";
        //            //    //}
        //            //    //else
        //            //    //{
        //            //    //    details.SOSI = "./images/inclusion.png";
        //            //    //    details.SOSIToolTips = "Standard Inclusion.";
        //            //    //}
        //            //    //if (details.StudioMProduct)
        //            //    //{
        //            //    //    details.StudioMIcon = "./images/color_swatch.png";
        //            //    //    details.StudioMTooltips = "Studio M Product.";
        //            //    //}
        //            estimateDetails.Add(result);
        //        }

        //    }
        //    return estimateDetails;

        //}

        public List<OptionTreeProducts> GetOptionTreeAsOptionTreeProductsForEstimateItemReplace(string revisionId, string areaname, string groupname)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_PopulateTreeForEstimateItemReplace");
            SqlCmd.Parameters["@revisionId"].Value = revisionId;
            SqlCmd.Parameters["@areaname"].Value = areaname;
            SqlCmd.Parameters["@groupname"].Value = groupname;
            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateOptionTreeProductsListFromDataSet(Sdr);
        }

        public List<OptionTreeProducts> GetOptionTreeAsOptionTreeProducts(string revisionId)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_PopulateTreeForEstimate");
            SqlCmd.Parameters["@revisionId"].Value = revisionId;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateOptionTreeProductsListFromDataSet(Sdr);

            //string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            //List<OptionTreeProducts> estimateDetails = new List<OptionTreeProducts>();

            //using (SqlConnection conn = new SqlConnection(connectionstring))
            //{
            //    using (SqlCommand cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = "sp_SalesEstimate_PopulateTreeForEstimate";
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));

            //        conn.Open();

            //        using (SqlDataReader dr = cmd.ExecuteReader())
            //        {
            //            while (dr.Read())
            //            {
            //                OptionTreeProducts details = new OptionTreeProducts();
            //                details.AreaName = dr["AreaName"].ToString();
            //                details.GroupName = dr["GroupName"].ToString();
            //                details.AreaId = Convert.ToInt32(dr["AreaId"]);
            //                details.GroupId = Convert.ToInt32(dr["GroupId"]);
            //                details.ProductName = dr["ProductName"].ToString();
            //                details.ProductDescription = dr["ProductDescription"].ToString();
            //                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
            //                details.Price = Convert.ToDecimal(dr["sellprice"]);
            //                details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
            //                details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
            //                if (dr["siteworkitem"] != DBNull.Value)
            //                    details.IsSiteWork = Convert.ToBoolean(dr["siteworkitem"]);
            //                details.StudioMSortOrder = Convert.ToInt32(dr["StudioMSortOrder"]);
            //                details.UOM = dr["uom"].ToString();

            //                bool isStudioM = Convert.ToBoolean(dr["isstudiomproduct"]);
            //                bool isMandatory = Convert.ToBoolean(dr["qandamandatory"]);
            //                string studioQandA = dr["studiomquestion"] == DBNull.Value ? string.Empty : dr["studiomquestion"].ToString();

            //                if (isStudioM)
            //                {
            //                    if (isMandatory)
            //                        details.StudioMProduct = 1; // Studio M Mandatory
            //                    else
            //                    {
            //                        if (string.IsNullOrEmpty(studioQandA))
            //                            details.StudioMProduct = 2; // Studio M No question
            //                        else
            //                            details.StudioMProduct = 3; // Studio M Non-mandatory
            //                    }
            //                }
            //                else
            //                    details.StudioMProduct = 0; // Non Studio M

            //                estimateDetails.Add(details);
            //            }

            //            dr.Close();
            //        }

            //        conn.Close();
            //    }
            //}

            //return estimateDetails;
        }

        public List<EstimateDetails> GetAdditionalNotesTemplateAndProducts(int revisionId, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateDetails> estimateDetails = new List<EstimateDetails>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetAdditionalNotesAndProducts";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            EstimateDetails details = new EstimateDetails();
                            details.TemplateID = dr["Templateid"].ToString();
                            details.TemplateName = dr["TemplateName"].ToString();
                            details.AreaName = dr["AreaName"].ToString();
                            details.GroupName = dr["GroupName"].ToString();
                            details.ProductId = dr["ProductID"].ToString();
                            details.ProductName = dr["ProductName"].ToString();
                            details.ProductDescription = dr["ProductDescription"].ToString();
                            details.ExtraDescription = dr["enterdesc"].ToString();
                            details.InternalDescription = dr["InternalDescription"].ToString();
                            details.AdditionalNotes = dr["AdditionalInfo"].ToString();
                            details.Uom = "";
                            details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                            details.Price = Convert.ToDecimal(dr["sellprice"]);
                            details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                            details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                            //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
                            details.HomeDisplayOptionId = Convert.ToInt32(dr["optionid"]);
                            details.TotalPrice = Convert.ToDecimal(dr["totalprice"]);
                            details.ItemAccepted = false;
                            if (dr["DerivedCost"].ToString() == "0" || dr["derivedcost"].ToString().ToUpper() == "FALSE")
                            {
                                details.DerivedCost = false;
                                details.DerivedCostIcon = "./images/spacer.gif";
                                details.DerivedCostTooltips = "";
                            }
                            else
                            {
                                details.DerivedCost = true;
                                details.DerivedCostIcon = "./images/link.png";
                                details.DerivedCostTooltips = "Derived cost.";
                            }

                            if (dr["CostExcGST"] != null)
                            {
                                details.CostExcGST = dr["CostExcGST"].ToString();
                                details.UpdatedBTPCostExcGST = dr["CostExcGST"].ToString();
                                details.UpdatedDBCCostExcGST = dr["DBCCostExcGST"].ToString();
                            }
                            else
                            {
                                details.CostExcGST = "";
                                details.UpdatedBTPCostExcGST = "";
                                details.UpdatedDBCCostExcGST = "";
                            }
                            if (dr["changeprice"].ToString() == "0")
                            {
                                details.ItemAllowToChangePrice = false;
                            }
                            else
                            {
                                details.ItemAllowToChangePrice = true;
                            }
                            if (dr["changeqty"].ToString() == "0")
                            {
                                details.ItemAllowToChangeQuantity = false;
                            }
                            else
                            {
                                details.ItemAllowToChangeQuantity = true;
                            }


                            if (dr["changedisplaycode"].ToString() == "0")
                            {
                                details.ItemAllowToChangeDisplayCode = false;
                            }
                            else
                            {
                                details.ItemAllowToChangeDisplayCode = true;
                            }

                            if (dr["changeproductstandarddescription"].ToString() == "0")
                            {
                                details.ItemAllowToChangeDescription = false;
                            }
                            else
                            {
                                details.ItemAllowToChangeDescription = true;


                            }

                            if (dr["UseDefaultQuantity"].ToString() == "1" || dr["UseDefaultQuantity"].ToString().ToUpper() == "TRUE")
                            {
                                details.UseDefaultQuantity = true;
                            }
                            else
                            {
                                details.UseDefaultQuantity = false;
                            }
                            estimateDetails.Add(details);
                        }

                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return estimateDetails;
        }

        public List<PAG> GetSelectedPAG(string estimateid, string revisionnumber)
        {
            List<PAG> l = new List<PAG>();
            //decimal qty, sellprice;
            //bool standardoption, promotionproduct;
            //int estimatedetailsid;
            //try
            //{
            //    SqlCommand SqlCmd = ConstructStoredProcedure("temp_test_getestimatedetails");
            //    SqlCmd.Parameters["@estimateId"].Value = estimateid;
            //    SqlCmd.Parameters["@revision"].Value = revisionnumber;
            //    DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            //    if (Sdr.Tables[0].Rows.Count > 0)
            //    {
            //        foreach (DataRow dr in Sdr.Tables[0].Rows)
            //        {
            //            try
            //            {
            //                qty = decimal.Parse(dr["quantity"].ToString());
            //            }
            //            catch
            //            {
            //                qty = 0;
            //            }
            //            try
            //            {
            //                sellprice = decimal.Parse(dr["itemprice"].ToString());
            //            }
            //            catch
            //            {
            //                sellprice = 0;
            //            }

            //            try
            //            {
            //                estimatedetailsid = int.Parse(dr["estimatedetailsid"].ToString());
            //            }
            //            catch
            //            {
            //                estimatedetailsid = 0;
            //            }
            //            try
            //            {
            //                promotionproduct = bool.Parse(dr["promotionproduct"].ToString());
            //            }
            //            catch
            //            {
            //                promotionproduct = false;
            //            }
            //            try
            //            {
            //                standardoption = bool.Parse(dr["standardoption"].ToString());
            //            }
            //            catch
            //            {
            //                standardoption = false;
            //            }
            //            l.Add(new PAG(dr["areaname"].ToString(), dr["groupname"].ToString(), dr["productname"].ToString(), qty, sellprice, estimatedetailsid, dr["extradescription"].ToString(), dr["internaldescription"].ToString(), promotionproduct, standardoption));
            //        }
            //    }

            //}
            //catch (Exception ex)
            //{
            //    l.Add(new PAG("", "", "", 0, 0, 0, "", "", false, false));
            //}

            return l;
        }

        public List<EstimateComments> GetCommentsForAnEstimate(string revisionid)
        {
            List<EstimateComments> l = new List<EstimateComments>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetEstimateComments");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                if (Sdr.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in Sdr.Tables[0].Rows)
                    {
                        l.Add(new EstimateComments(dr["comments"].ToString()));
                    }
                }

            }
            catch (Exception)
            {
                l.Add(new EstimateComments(""));
            }

            return l;
        }

        public bool GetAccessPermission(string revisionid, string userid, string roleid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_AccessPermission");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                SqlCmd.Parameters["@userid"].Value = userid;
                SqlCmd.Parameters["@roleid"].Value = roleid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                if (Sdr.Tables[0].Rows[0]["result"].ToString() == "1" || Sdr.Tables[0].Rows[0]["result"].ToString().ToUpper()=="TRUE")
                {
                    result = true;
                }

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public List<int> GetEstimateCount(int userId, int roleId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<int> estimateCount = new List<int>();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateCount";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    cmd.Parameters.Add(new SqlParameter("@roleId", roleId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            estimateCount.Add(Convert.ToInt32(dr["QueueCount"]));
                            estimateCount.Add(Convert.ToInt32(dr["WIPCount"]));
                            estimateCount.Add(Convert.ToInt32(dr["AcceptedCount"]));
                            estimateCount.Add(Convert.ToInt32(dr["RejectedCount"]));
                            estimateCount.Add(Convert.ToInt32(dr["OnHoldCount"]));
                            estimateCount.Add(Convert.ToInt32(dr["CancelledCount"]));
                            estimateCount.Add(Convert.ToInt32(dr["AppointmentCount"]));
                        }
                        else
                        {
                            estimateCount.Add(0);
                            estimateCount.Add(0);
                            estimateCount.Add(0);
                            estimateCount.Add(0);
                            estimateCount.Add(0);
                            estimateCount.Add(0);
                            estimateCount.Add(0);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return estimateCount;

        }

        public int SaveSelectedItem(int selectedid, int revisionid, int pagid, int userid)
        {
            //int revisionDetailsId;
            ////try
            ////{
            //    SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_InsertEstimateDetails");
            //    SqlCmd.Parameters["@revisionId"].Value = revisionid;
            //    SqlCmd.Parameters["@estimateDetailsId"].Value = selectedid;
            //    SqlCmd.Parameters["@userId"].Value = userid;
            //    SqlParameter param = SqlCmd.Parameters.Add(new SqlParameter("@estimateRevisionDetailsId", SqlDbType.Int));
            //    param.Direction = ParameterDirection.Output;

            //    DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            //    revisionDetailsId = (int)param.Value;
            ////}
            ////catch (Exception ex)
            ////{
            ////    revisionDetailsId = -1;
            ////}
            //return revisionDetailsId;

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            int revisionDetailsId = 0;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_InsertEstimateDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionid));
                    cmd.Parameters.Add(new SqlParameter("@estimateDetailsId", selectedid));
                    cmd.Parameters.Add(new SqlParameter("@pagId", pagid));
                    cmd.Parameters.Add(new SqlParameter("@userId", userid));
                    SqlParameter param = cmd.Parameters.Add(new SqlParameter("@estimateRevisionDetailsId", SqlDbType.Int));
                    param.Direction = ParameterDirection.Output;

                    conn.Open();
                    cmd.ExecuteNonQuery();

                    revisionDetailsId = (int)param.Value;

                    conn.Close();
                }
            }
            return revisionDetailsId;

        }

        public bool RemoveItem(int selectedid, int estimateid)
        {
            bool result = false;
            //try
            //{
            //    SqlCommand SqlCmd = ConstructStoredProcedure("temp_test_removeselecteditem");
            //    SqlCmd.Parameters["@estimateId"].Value = estimateid;
            //    SqlCmd.Parameters["@estimatedetailsid"].Value = selectedid;
            //    DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            //    result = true;
            //}
            //catch (Exception ex)
            //{
            //    result = false;
            //}
            return result;

        }

        public bool CheckValidProductByRevision(int revisionId, string productId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            bool valid = true;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CheckValidProductByRevision";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));
                    cmd.Parameters.Add(new SqlParameter("@productId", productId));
                    conn.Open();

                    object returnValue = cmd.ExecuteScalar();

                    if (returnValue != DBNull.Value)
                        valid = Convert.ToBoolean(returnValue);

                    conn.Close();
                }
            }
            return valid;
        }

        public bool SaveEditItemDetails(int selectedid, int revsionid, decimal qty, decimal sellprice, string productdescription, string extradescription, string internaldescription)
        {

            bool result = false;
            //try
            //{
            //    SqlCommand SqlCmd = ConstructStoredProcedure("temp_test_UpdateItemDetail");
            //    SqlCmd.Parameters["@estimateId"].Value = revsionid;
            //    SqlCmd.Parameters["@estimatedetailsid"].Value = selectedid;
            //    SqlCmd.Parameters["@qty"].Value = qty;
            //    SqlCmd.Parameters["@sellprice"].Value = sellprice;
            //    SqlCmd.Parameters["@productdescription"].Value = productdescription;
            //    SqlCmd.Parameters["@extradescription"].Value = extradescription;
            //    SqlCmd.Parameters["@internaldescription"].Value = internaldescription;

            //    DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            //    result = true;
            //}
            //catch (Exception ex)
            //{
            //    result = false;
            //}
            return result;
        }

        public string GetEstimateSalesConsultantLoginName(int revisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string loginName = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateSalesConsultantLoginName";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionId));

                    conn.Open();

                    object returnValue = cmd.ExecuteScalar();

                    if (returnValue != DBNull.Value)
                        loginName = returnValue.ToString();

                    conn.Close();
                }
            }
            return loginName;
        }

        public EstimateDetails CopyItemFromOptionTreeToEstimate(int homedisplayoptionid, int revisiondetailsid, int revisionid, int productareagroupid, int userid)
        {
            EstimateDetails details = new EstimateDetails();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CopyDescriptionToNonstandardOption");
                SqlCmd.Parameters["@HomeDisplayOptionId"].Value = homedisplayoptionid;
                SqlCmd.Parameters["@revisiondetailsid"].Value = revisiondetailsid;
                SqlCmd.Parameters["@revisionId"].Value = revisionid;
                SqlCmd.Parameters["@productareagroupid"].Value = productareagroupid;
                SqlCmd.Parameters["@userId"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                DataRow dr = Sdr.Tables[0].Rows[0];

                //insert the item and return the item to add to the list on view.
                details.AreaName = dr["AreaName"].ToString();
                if (dr["NonStandardAreaName"] != DBNull.Value && dr["NonStandardAreaName"].ToString().Trim() != "")
                {
                    if (dr["NonStandardGroupName"] != DBNull.Value && dr["NonStandardGroupName"].ToString().Trim() != "")
                        details.AreaName += "\r\n[" + dr["NonStandardAreaName"].ToString() + " - " + dr["NonStandardGroupName"].ToString() + "]";
                    else
                        details.AreaName += "\r\n[" + dr["NonStandardAreaName"].ToString() + "]";
                }

                details.GroupName = dr["GroupName"].ToString();
                details.AreaId = Convert.ToInt32(dr["AreaId"]);
                details.GroupId = Convert.ToInt32(dr["GroupId"]);
                details.ProductId = dr["ProductID"].ToString();
                details.ProductName = dr["ProductName"].ToString();

                details.ProductDescription = dr["ProductDescription"].ToString();
                details.UpdatedProductDescription = dr["ProductDescription"].ToString();
                details.ExtraDescription = dr["EnterDesc"].ToString();
                details.UpdatedExtraDescription = dr["EnterDesc"].ToString();
                details.ProductDescriptionShort = dr["ProductDescriptionShort"].ToString();
                details.InternalDescription = dr["internalDescription"].ToString();
                details.UpdatedInternalDescription = dr["internalDescription"].ToString();
                details.AdditionalNotes = dr["additionalinfo"].ToString();
                details.UpdatedAdditionalNotes = dr["additionalinfo"].ToString();

                details.Uom = dr["UOM"].ToString();
                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                details.Price = Convert.ToDecimal(dr["SellPrice"]);
                details.UpdatedQuantity = Convert.ToDecimal(dr["Quantity"]);
                details.UpdatedPrice = Convert.ToDecimal(dr["SellPrice"]);
                details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
                details.HomeDisplayOptionId = Convert.ToInt32(dr["OptionId"]);
                details.EstimateRevisionDetailsId = Convert.ToInt32(dr["EstimateRevisionDetailsId"]);
                details.TotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.UpdatedTotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.ItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
                details.NonstandardCategoryID = Convert.ToInt32(dr["fkid_NonstandardArea"]);
                details.NonstandardGroupID = Convert.ToInt32(dr["fkid_NonstandardGroup"]);
                details.UpdatedNonstandardCategoryID = Convert.ToInt32(dr["fkid_NonstandardArea"]);
                details.UpdatedNonstandardGroupID = Convert.ToInt32(dr["fkid_NonstandardGroup"]);

                details.PriceDisplayCodeDesc = dr["pricedisplaydesc"].ToString();
                details.PriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());
                details.UpdatedPriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());

                if (dr["CostExcGST"] != null)
                {
                    details.CostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedBTPCostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedDBCCostExcGST = dr["DBCCostExcGST"].ToString();
                }
                else
                {
                    details.CostExcGST = "";
                    details.UpdatedBTPCostExcGST = "";
                    details.UpdatedDBCCostExcGST = "";
                }
                details.Margin = dr["margin"].ToString();
                details.MarginDBCCost = dr["marginDBCCost"].ToString();

                if (dr["derivedcost"].ToString() == "0" || dr["derivedcost"].ToString().ToUpper() == "FALSE")
                {
                    details.DerivedCost = false;
                    details.DerivedCostIcon = "./images/spacer.gif";
                    details.DerivedCostTooltips = "";
                }
                else
                {
                    details.DerivedCost = true;
                    details.DerivedCostIcon = "./images/link.png";
                    details.DerivedCostTooltips = "Derived cost.";
                }

                if (dr["changeprice"].ToString() == "0")
                {
                    details.ItemAllowToChangePrice = false;
                }
                else
                {
                    details.ItemAllowToChangePrice = true;
                }

                if (dr["changeqty"].ToString() == "0")
                {
                    details.ItemAllowToChangeQuantity = false;
                }
                else
                {
                    details.ItemAllowToChangeQuantity = true;
                }

                if (dr["changedisplaycode"].ToString() == "0")
                {
                    details.ItemAllowToChangeDisplayCode = false;
                }
                else
                {
                    details.ItemAllowToChangeDisplayCode = true;
                }

                if (dr["changeproductstandarddescription"].ToString() == "0")
                {
                    details.ItemAllowToChangeDescription = false;
                }
                else
                {
                    details.ItemAllowToChangeDescription = true;
                }

            }
            catch (Exception)
            {
                details = null;
            }
            return details;

        }

        public EstimateDetails CopyItemFromMasterHomeToEstimate(int regionid, int optionid, int revisionid, int userid)
        {
            EstimateDetails details = new EstimateDetails();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CopyDescriptionFromOtherHomeToNonstandardOption");
                SqlCmd.Parameters["@regionid"].Value = regionid;
                SqlCmd.Parameters["@optionid"].Value = optionid;
                SqlCmd.Parameters["@revisionId"].Value = revisionid;
                SqlCmd.Parameters["@userId"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                DataRow dr = Sdr.Tables[0].Rows[0];

                //insert the item and return the item to add to the list on view.
                details.AreaName = dr["AreaName"].ToString();
                if (dr["NonStandardAreaName"] != DBNull.Value && dr["NonStandardAreaName"].ToString().Trim() != "")
                {
                    if (dr["NonStandardGroupName"] != DBNull.Value && dr["NonStandardGroupName"].ToString().Trim() != "")
                        details.AreaName += "\r\n[" + dr["NonStandardAreaName"].ToString() + " - " + dr["NonStandardGroupName"].ToString() + "]";
                    else
                        details.AreaName += "\r\n[" + dr["NonStandardAreaName"].ToString() + "]";
                }

                details.GroupName = dr["GroupName"].ToString();
                details.AreaId = Convert.ToInt32(dr["AreaId"]);
                details.GroupId = Convert.ToInt32(dr["GroupId"]);
                details.ProductId = dr["ProductID"].ToString();
                details.ProductName = dr["ProductName"].ToString();

                details.ProductDescription = dr["ProductDescription"].ToString();
                details.UpdatedProductDescription = dr["ProductDescription"].ToString();
                details.ExtraDescription = dr["EnterDesc"].ToString();
                details.UpdatedExtraDescription = dr["EnterDesc"].ToString();
                details.ProductDescriptionShort = dr["ProductDescriptionShort"].ToString();
                details.InternalDescription = dr["internalDescription"].ToString();
                details.UpdatedInternalDescription = dr["internalDescription"].ToString();
                details.AdditionalNotes = dr["additionalinfo"].ToString();
                details.UpdatedAdditionalNotes = dr["additionalinfo"].ToString();

                details.Uom = dr["UOM"].ToString();
                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                details.Price = Convert.ToDecimal(dr["SellPrice"]);
                details.UpdatedQuantity = Convert.ToDecimal(dr["Quantity"]);
                details.UpdatedPrice = Convert.ToDecimal(dr["SellPrice"]);
                details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
                details.HomeDisplayOptionId = Convert.ToInt32(dr["OptionId"]);
                details.ProductAreaGroupID = Convert.ToInt32(dr["ProductAreaGroupID"]);
                details.EstimateRevisionDetailsId = Convert.ToInt32(dr["EstimateRevisionDetailsId"]);
                details.TotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.UpdatedTotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.ItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
                details.NonstandardCategoryID = Convert.ToInt32(dr["fkid_NonstandardArea"]);
                details.NonstandardGroupID = Convert.ToInt32(dr["fkid_NonstandardGroup"]);
                details.UpdatedNonstandardCategoryID = Convert.ToInt32(dr["fkid_NonstandardArea"]);
                details.UpdatedNonstandardGroupID = Convert.ToInt32(dr["fkid_NonstandardGroup"]);

                details.PriceDisplayCodeDesc = dr["pricedisplaydesc"].ToString();
                details.PriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());
                details.UpdatedPriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());

                if (dr["CostExcGST"] != null)
                {
                    details.CostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedBTPCostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedDBCCostExcGST = dr["DBCCostExcGST"].ToString();
                }
                else
                {
                    details.CostExcGST = "";
                    details.UpdatedBTPCostExcGST = "";
                    details.UpdatedDBCCostExcGST = "";
                }
                details.Margin = dr["margin"].ToString();

                if (dr["derivedcost"].ToString() == "0" || dr["derivedcost"].ToString().ToUpper() == "FALSE")
                {
                    details.DerivedCost = false;
                    details.DerivedCostIcon = "./images/spacer.gif";
                    details.DerivedCostTooltips = "";
                }
                else
                {
                    details.DerivedCost = true;
                    details.DerivedCostIcon = "./images/link.png";
                    details.DerivedCostTooltips = "Derived cost.";
                }

                if (dr["changeprice"].ToString() == "0")
                {
                    details.ItemAllowToChangePrice = false;
                }
                else
                {
                    details.ItemAllowToChangePrice = true;
                }

                if (dr["changeqty"].ToString() == "0")
                {
                    details.ItemAllowToChangeQuantity = false;
                }
                else
                {
                    details.ItemAllowToChangeQuantity = true;
                }

                if (dr["changedisplaycode"].ToString() == "0")
                {
                    details.ItemAllowToChangeDisplayCode = false;
                }
                else
                {
                    details.ItemAllowToChangeDisplayCode = true;
                }

                if (dr["changeproductstandarddescription"].ToString() == "0")
                {
                    details.ItemAllowToChangeDescription = false;
                }
                else
                {
                    details.ItemAllowToChangeDescription = true;
                }

            }
            catch (Exception)
            {
                details = null;
            }
            return details;

        }
        public EstimateDetails CopyItemFromAllProductsToEstimate(int regionid, string productid, int revisionid, int userid)
        {
            EstimateDetails details = new EstimateDetails();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CopyDescriptionFromAllProductsToNonstandardOption");
                SqlCmd.Parameters["@regionid"].Value = regionid;
                SqlCmd.Parameters["@productid"].Value = productid;
                SqlCmd.Parameters["@revisionId"].Value = revisionid;
                SqlCmd.Parameters["@userId"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                DataRow dr = Sdr.Tables[0].Rows[0];

                //insert the item and return the item to add to the list on view.
                details.AreaName = dr["AreaName"].ToString();
                if (dr["NonStandardAreaName"] != DBNull.Value && dr["NonStandardAreaName"].ToString().Trim() != "")
                {
                    if (dr["NonStandardGroupName"] != DBNull.Value && dr["NonStandardGroupName"].ToString().Trim() != "")
                        details.AreaName += "\r\n[" + dr["NonStandardAreaName"].ToString() + " - " + dr["NonStandardGroupName"].ToString() + "]";
                    else
                        details.AreaName += "\r\n[" + dr["NonStandardAreaName"].ToString() + "]";
                }

                details.GroupName = dr["GroupName"].ToString();
                details.AreaId = Convert.ToInt32(dr["AreaId"]);
                details.GroupId = Convert.ToInt32(dr["GroupId"]);
                details.ProductId = dr["ProductID"].ToString();
                details.ProductName = dr["ProductName"].ToString();

                details.ProductDescription = dr["ProductDescription"].ToString();
                details.UpdatedProductDescription = dr["ProductDescription"].ToString();
                details.ExtraDescription = dr["EnterDesc"].ToString();
                details.UpdatedExtraDescription = dr["EnterDesc"].ToString();
                details.ProductDescriptionShort = dr["ProductDescriptionShort"].ToString();
                details.InternalDescription = dr["internalDescription"].ToString();
                details.UpdatedInternalDescription = dr["internalDescription"].ToString();
                details.AdditionalNotes = dr["additionalinfo"].ToString();
                details.UpdatedAdditionalNotes = dr["additionalinfo"].ToString();

                details.Uom = dr["UOM"].ToString();
                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                details.Price = Convert.ToDecimal(dr["SellPrice"]);
                details.UpdatedQuantity = Convert.ToDecimal(dr["Quantity"]);
                details.UpdatedPrice = Convert.ToDecimal(dr["SellPrice"]);
                details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
                details.HomeDisplayOptionId = Convert.ToInt32(dr["OptionId"]);
                details.ProductAreaGroupID = Convert.ToInt32(dr["ProductAreaGroupID"]);
                details.EstimateRevisionDetailsId = Convert.ToInt32(dr["EstimateRevisionDetailsId"]);
                details.TotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.UpdatedTotalPrice = Convert.ToDecimal(dr["totalPrice"]);
                details.ItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
                details.NonstandardCategoryID = Convert.ToInt32(dr["fkid_NonstandardArea"]);
                details.NonstandardGroupID = Convert.ToInt32(dr["fkid_NonstandardGroup"]);
                details.UpdatedNonstandardCategoryID = Convert.ToInt32(dr["fkid_NonstandardArea"]);
                details.UpdatedNonstandardGroupID = Convert.ToInt32(dr["fkid_NonstandardGroup"]);

                details.PriceDisplayCodeDesc = dr["pricedisplaydesc"].ToString();
                details.PriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());
                details.UpdatedPriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());

                if (dr["CostExcGST"] != null)
                {
                    details.CostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedBTPCostExcGST = dr["CostExcGST"].ToString();
                    details.UpdatedDBCCostExcGST = dr["DBCCostExcGST"].ToString();
                }
                else
                {
                    details.CostExcGST = "";
                    details.UpdatedBTPCostExcGST = "";
                    details.UpdatedDBCCostExcGST = "";
                }
                details.Margin = dr["margin"].ToString();

                if (dr["derivedcost"].ToString() == "0" || dr["derivedcost"].ToString().ToUpper() == "FALSE")
                {
                    details.DerivedCost = false;
                    details.DerivedCostIcon = "./images/spacer.gif";
                    details.DerivedCostTooltips = "";
                }
                else
                {
                    details.DerivedCost = true;
                    details.DerivedCostIcon = "./images/link.png";
                    details.DerivedCostTooltips = "Derived cost.";
                }

                if (dr["changeprice"].ToString() == "0")
                {
                    details.ItemAllowToChangePrice = false;
                }
                else
                {
                    details.ItemAllowToChangePrice = true;
                }

                if (dr["changeqty"].ToString() == "0")
                {
                    details.ItemAllowToChangeQuantity = false;
                }
                else
                {
                    details.ItemAllowToChangeQuantity = true;
                }

                if (dr["changedisplaycode"].ToString() == "0")
                {
                    details.ItemAllowToChangeDisplayCode = false;
                }
                else
                {
                    details.ItemAllowToChangeDisplayCode = true;
                }

                if (dr["changeproductstandarddescription"].ToString() == "0")
                {
                    details.ItemAllowToChangeDescription = false;
                }
                else
                {
                    details.ItemAllowToChangeDescription = true;
                }

            }
            catch (Exception)
            {
                details = null;
            }
            return details;

        }

       public bool SynchronizeNewOptionToEstimate(int revisionid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_SynchronizeNewOptions");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;

        }

        public bool AddAdditonalNotesTemplate(string templatename, int revisionid, int userid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_AddAddtionalNotesTemplate");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                SqlCmd.Parameters["@templatename"].Value = templatename;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;

        }

        public bool AddItemsToNotesTemplate(string templateid, string selecteditemids, int userid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_AddItemsToNotesTemplate");
                SqlCmd.Parameters["@templateid"].Value = templateid;
                SqlCmd.Parameters["@selecteditemids"].Value = selecteditemids;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;

        }

        public bool AddNewNotesTemplate(string templatename, string regionid, int userid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_AddNewNotesTemplate");
                SqlCmd.Parameters["@templatename"].Value = templatename;
                SqlCmd.Parameters["@regionid"].Value = regionid;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;

        }

        public bool RemoveNotesTemplate(string templateid, int userid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_RemoveNotesTemplate");
                SqlCmd.Parameters["@templateid"].Value = templateid;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public bool CopyNotesTemplate(string templatename, string regionid, int userid, string templateid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CopyNotesTemplate");
                SqlCmd.Parameters["@templatename"].Value = templatename;
                SqlCmd.Parameters["@regionid"].Value = regionid;
                SqlCmd.Parameters["@userid"].Value = userid;
                SqlCmd.Parameters["@templateid"].Value = templateid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public List<EstimateDetails> GetAvailableItemsForNotesTemplate(string templateid, string searchtext)
        {
            List<EstimateDetails> result = new List<EstimateDetails>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetAvailableItemsForNotesTemplate");
                SqlCmd.Parameters["@templateid"].Value = templateid;
                SqlCmd.Parameters["@searchtext"].Value = searchtext;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    EstimateDetails details = new EstimateDetails();
                    details.AreaName = dr["AreaName"].ToString();
                    details.GroupName = dr["GroupName"].ToString();
                    details.ProductId = dr["ProductID"].ToString();
                    details.ProductName = dr["ProductName"].ToString();
                    details.ProductDescription = dr["ProductDescription"].ToString();
                    details.ProductAreaGroupID = int.Parse(dr["productareagroupid"].ToString());

                    result.Add(details);
                }

            }
            catch (Exception)
            {
                result = null;
            }
            return result;

        }

        public bool RemoveItemFromNotesTemplate(string templateid, string productareagroupid, int userid)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_RemoveItemFromNotesTemplate");
                SqlCmd.Parameters["@templateid"].Value = templateid;
                SqlCmd.Parameters["@productareagroupid"].Value = productareagroupid;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;

        }

        public bool UpdateNotesTemplateItem(string templateid, string productareagroupid, decimal quantity, decimal price, string extradescription, string internaldescription, string additionalinfo, int userid, bool userdefaultquantity)
        {
            bool result = false;
            int defaultqty = 0;
            if (userdefaultquantity) defaultqty = 1;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_UpdateNotesTemplateItem");
                SqlCmd.Parameters["@templateid"].Value = templateid;
                SqlCmd.Parameters["@productareagroupid"].Value = productareagroupid;
                SqlCmd.Parameters["@quantity"].Value = quantity;
                SqlCmd.Parameters["@price"].Value = price;
                SqlCmd.Parameters["@extraDesc"].Value = extradescription;
                SqlCmd.Parameters["@internalDesc"].Value = internaldescription;
                SqlCmd.Parameters["@additonalinfo"].Value = additionalinfo;
                SqlCmd.Parameters["@userid"].Value = userid;
                SqlCmd.Parameters["@usedefaultqty"].Value = defaultqty;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public bool UpdateNotesTemplateStatus(string templateid, int userid, int status, string templatename, string action)
        {
            bool result = false;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_UpdateNotesTemplate");
                SqlCmd.Parameters["@templateid"].Value = templateid;
                SqlCmd.Parameters["@templatename"].Value = templatename;
                SqlCmd.Parameters["@status"].Value = status;
                SqlCmd.Parameters["@userid"].Value = userid;
                SqlCmd.Parameters["@action"].Value = action;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }



        public string CheckNewNotesTemplateNameExists(int templateid, string templatename)
        {
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CheckNotesTemplateNameExists");
                SqlCmd.Parameters["@templateid"].Value = templateid;
                SqlCmd.Parameters["@templatename"].Value = templatename;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                return Sdr.Tables[0].Rows[0]["result"].ToString();
            }
            catch (Exception)
            {
                return "Error occurs when check duplication.";
            }


        }
        public List<EstimateDetails> GetAdditionalNotesTemplateAndProductsByRegion(string templatename, string subregionid, int userid, int active, int selectedroleid)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateDetails> estimateDetails = new List<EstimateDetails>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionstring))
                {
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "sp_SalesEstimate_GetAdditionalNotesAndProductsByRegion";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@templatename", templatename));
                        cmd.Parameters.Add(new SqlParameter("@subregionid", subregionid));
                        cmd.Parameters.Add(new SqlParameter("@userid", userid));
                        cmd.Parameters.Add(new SqlParameter("@active", active));
                        cmd.Parameters.Add(new SqlParameter("@loginroleid", selectedroleid));
                        conn.Open();


                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                EstimateDetails details = new EstimateDetails();
                                details.TemplateID = dr["Templateid"].ToString();
                                details.TemplateName = dr["TemplateName"].ToString();
                                details.RegionName = dr["RegionName"].ToString();
                                details.AreaId = Convert.ToInt32(dr["AreaId"]);
                                details.GroupId = Convert.ToInt32(dr["GroupId"]);
                                details.AreaName = dr["AreaName"].ToString();
                                details.GroupName = dr["GroupName"].ToString();
                                details.ProductId = dr["ProductID"].ToString();
                                details.ProductName = dr["ProductName"].ToString();
                                details.ProductDescription = dr["ProductDescription"].ToString();
                                details.ExtraDescription = dr["enterdesc"].ToString();
                                details.InternalDescription = dr["internaldesc"].ToString();
                                details.AdditionalNotes = dr["additionalinfo"].ToString();
                                details.Uom = "";
                                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                                details.Price = Convert.ToDecimal(dr["sellprice"]);
                                details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                                details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                                //details.EstimateDetailsId = 0;
                                details.ProductAreaGroupID = int.Parse(dr["productareagroupid"].ToString());
                                details.TotalPrice = Convert.ToDecimal(dr["totalprice"]);
                                details.TemplateActive = bool.Parse(dr["active"].ToString());
                                details.IsPrivate = bool.Parse(dr["IsPrivate"].ToString());
                                details.OwnerName = dr["ownername"].ToString();
                                if (dr["derivedcost"].ToString() == "0" || dr["derivedcost"].ToString().ToUpper() == "FALSE")
                                {
                                    details.DerivedCost = false;
                                    details.DerivedCostIcon = "./images/spacer.gif";
                                    details.DerivedCostTooltips = "";
                                }
                                else
                                {
                                    details.DerivedCost = true;
                                    details.DerivedCostIcon = "./images/link.png";
                                    details.DerivedCostTooltips = "Derived cost.";
                                }

                                if (dr["CostExcGST"] != null)
                                {
                                    details.CostExcGST = dr["CostExcGST"].ToString();
                                    details.UpdatedBTPCostExcGST = dr["CostExcGST"].ToString();
                                    details.UpdatedDBCCostExcGST = dr["DBCCostExcGST"].ToString();
                                }
                                else
                                {
                                    details.CostExcGST = "";
                                    details.UpdatedBTPCostExcGST = "";
                                    details.UpdatedDBCCostExcGST = "";
                                }

                                if (dr["changeprice"].ToString() == "0")
                                {
                                    details.ItemAllowToChangePrice = false;
                                }
                                else
                                {
                                    details.ItemAllowToChangePrice = true;
                                }
                                if (dr["changeqty"].ToString() == "0")
                                {
                                    details.ItemAllowToChangeQuantity = false;
                                }
                                else
                                {
                                    details.ItemAllowToChangeQuantity = true;
                                }
                                if (dr["changedisplaycode"].ToString() == "0")
                                {
                                    details.ItemAllowToChangeDisplayCode = false;
                                }
                                else
                                {
                                    details.ItemAllowToChangeDisplayCode = true;
                                }

                                if (dr["changeproductstandarddescription"].ToString() == "0")
                                {
                                    details.ItemAllowToChangeDescription = false;
                                }
                                else
                                {
                                    details.ItemAllowToChangeDescription = true;
                                }

                                if (dr["createdby"]!=null)
                                {
                                    details.CreatedBy = dr["createdby"].ToString();
                                }
                                else
                                {
                                    details.CreatedBy = "";
                                }

                                if (dr["createdon"] != null && dr["createdon"].ToString()!="")
                                {
                                    details.CreatedOn = DateTime.Parse(dr["createdon"].ToString()).ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    details.CreatedOn = "";
                                }

                                if (dr["modifiedby"] != null)
                                {
                                    details.ModifiedBy = dr["modifiedby"].ToString();
                                }
                                else
                                {
                                    details.ModifiedBy = "";
                                }

                                if (dr["modifiedon"] != null && dr["modifiedon"].ToString() != "")
                                {
                                    details.ModifiedOn = DateTime.Parse(dr["modifiedon"].ToString()).ToString("dd/MM/yyyy");
                                }
                                else
                                {
                                    details.ModifiedOn = "";
                                }

                                if (dr["UseDefaultQuantity"] != null && (dr["UseDefaultQuantity"].ToString() == "1" || dr["UseDefaultQuantity"].ToString().ToUpper() == "TRUE"))
                                {
                                    details.UseDefaultQuantity = true;
                                }
                                else
                                {
                                    details.UseDefaultQuantity = false;
                                }

                                estimateDetails.Add(details);
                            }

                            dr.Close();
                        }

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                EstimateDetails details = new EstimateDetails();
                details.ProductAreaGroupID = -1;
                details.ProductDescription = ex.Message.ToString();
                estimateDetails.Add(details);
            }
            return estimateDetails;
        }
        public List<SQSSalesRegion> GetSaleRegionByState(string stateid)
        {
            List<SQSSalesRegion> l = new List<SQSSalesRegion>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetSalesRegion");
                SqlCmd.Parameters["@stateid"].Value = stateid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    SQSSalesRegion sr = new SQSSalesRegion();
                    sr.RegionId = int.Parse(dr["regionid"].ToString());
                    sr.RegionName = dr["regionname"].ToString();
                    l.Add(sr);
                }
            }
            catch (Exception)
            {

            }
            return l;

        }
        public List<SQSSalesRegion> GetPriceRegionByState(string stateid)
        {
            List<SQSSalesRegion> l = new List<SQSSalesRegion>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetPriceRegion");
                SqlCmd.Parameters["@stateid"].Value = stateid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    SQSSalesRegion sr = new SQSSalesRegion();
                    sr.RegionId = int.Parse(dr["regionid"].ToString());
                    sr.RegionName = dr["regionname"].ToString();
                    l.Add(sr);
                }
            }
            catch (Exception)
            {

            }
            return l;

        }
        public List<SQSArea> GetAreaNameWithAll()
        {
            List<SQSArea> l = new List<SQSArea>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("spw_getAreaNameWithAll");
                SqlCmd.Parameters["@tempID"].Value = 1;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    SQSArea sr = new SQSArea();
                    sr.AreaID = int.Parse(dr["areaid"].ToString());
                    sr.AreaName = dr["areaname"].ToString();
                    l.Add(sr);
                }
            }
            catch (Exception)
            {

            }
            return l;
        }

        public List<SQSGroup> GetGroupNameWithAll()
        {
            List<SQSGroup> l = new List<SQSGroup>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("spw_getGroupNameWithAll");
                SqlCmd.Parameters["@tempID"].Value = 1;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    SQSGroup sr = new SQSGroup();
                    sr.GroupID = int.Parse(dr["groupid"].ToString());
                    sr.GroupName = dr["groupname"].ToString();
                    l.Add(sr);
                }
            }
            catch (Exception)
            {

            }
            return l;
        }
        public List<OptionTreeProducts> GetPagsPriceForEstimator(int stateID, int regionid, int homeID, int areaID, int groupID, string productname, string productdesc)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("spw_getPagsPriceForEstimator");
            SqlCmd.Parameters["@stateID"].Value = stateID;
            SqlCmd.Parameters["@regionID"].Value = regionid;
            SqlCmd.Parameters["@homeID"].Value = homeID;
            SqlCmd.Parameters["@areaID"].Value = areaID;
            SqlCmd.Parameters["@groupID"].Value = groupID;
            SqlCmd.Parameters["@productname"].Value = productname;
            SqlCmd.Parameters["@productdesc"].Value = productdesc;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateOptionTreeProductsListFromDataSet(Sdr);
        }

        public List<NonStandardCategory> GetNonstandardCategory()
        {
            List<NonStandardCategory> l = new List<NonStandardCategory>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("spw_GetAreaAsCategory");
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    NonStandardCategory sr = new NonStandardCategory();
                    sr.CategoryId = int.Parse(dr["areaid"].ToString());
                    sr.CategoryName = dr["areaname"].ToString();
                    l.Add(sr);
                }
            }
            catch (Exception)
            {

            }
            return l;
        }

        public List<NonStandardCategory> GetNonstandardCategoryByState(int stateid, int selectedareaid)
        {
            List<NonStandardCategory> l = new List<NonStandardCategory>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetAreasByState");
                SqlCmd.Parameters["@stateid"].Value = stateid;
                SqlCmd.Parameters["@selectedareaid"].Value = selectedareaid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    NonStandardCategory sr = new NonStandardCategory();
                    sr.CategoryId = int.Parse(dr["areaid"].ToString());
                    sr.CategoryName = dr["areaname"].ToString();
                    l.Add(sr);
                }
            }
            catch (Exception)
            {

            }
            return l;
        }

        public List<PriceDisplayCode> GetPriceDisplayCodes()
        {
            List<PriceDisplayCode> l = new List<PriceDisplayCode>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetPriceDisplayCodes");
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    PriceDisplayCode pdc = new PriceDisplayCode();
                    pdc.PriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());
                    pdc.PriceDisplayCodeDescription = dr["PriceDisplayDesc"].ToString();
                    l.Add(pdc);
                }
            }
            catch (Exception)
            {

            }
            return l;
        }

        public List<NonStandardGroup> GetNonstandardGroups(int selectedareaid, int stateid, int selectedgroupid)
        {
            List<NonStandardGroup> groups = new List<NonStandardGroup>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetProductGroups");
                SqlCmd.Parameters["@selectedareaid"].Value = selectedareaid;
                SqlCmd.Parameters["@stateid"].Value = stateid;
                SqlCmd.Parameters["@selectedgroupid"].Value = selectedgroupid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                NonStandardGroup defaultGroup = new NonStandardGroup();
                defaultGroup.GroupId = 0;
                defaultGroup.GroupName = "Please Select";
                groups.Add(defaultGroup);

                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    NonStandardGroup group = new NonStandardGroup();
                    group.GroupId = int.Parse(dr["groupID"].ToString());
                    group.GroupName = dr["groupName"].ToString();
                    groups.Add(group);
                }
            }
            catch (Exception)
            {

            }
            return groups;
        }

        public void GetEstimateCustomerInformation(int estimateId, out int customerNumber, out string accountId, out string opportunityId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            customerNumber = 0;
            accountId = string.Empty;
            opportunityId = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateCustomer";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["CustomerNumber"] != DBNull.Value)
                                customerNumber = Convert.ToInt32(dr["CustomerNumber"]);
                            if (dr["AccountId"] != DBNull.Value)
                                accountId = dr["AccountId"].ToString();
                            if (dr["OpportunityId"] != DBNull.Value)
                                opportunityId = dr["OpportunityId"].ToString();
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }

        }

        public void GetEstimateContractInformation(int estimateId, out int contractNumber, out string contractId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            contractNumber = 0;
            contractId = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetEstimateContract";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateId", estimateId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["ContractNumber"] != DBNull.Value)
                                contractNumber = Convert.ToInt32(dr["ContractNumber"]);
                            if (dr["ContractId"] != DBNull.Value)
                                contractId = dr["ContractId"].ToString();
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }

        }

        public List<ProductImage> GetProductImages(string pproductid, int supplierid)
        {
            DataSet Sdr;
            List<ProductImage> imglist = new List<ProductImage>();
            try
            {

                SqlCommand SqlCmd = ConstructStoredProcedure("spa_Admin_StudioM_GetImageForProduct");
                SqlCmd.Parameters["@ProductID"].Value = pproductid;
                SqlCmd.Parameters["@supplierid"].Value = supplierid;
                Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    ProductImage pi = new ProductImage();
                    pi.image = (byte[])dr["image"];
                    pi.suppliername = dr["supplierbrandname"].ToString();
                    pi.imagename = dr["imagename"].ToString();
                    pi.imageID = int.Parse(dr["id_studiom_productimage"].ToString());
                    imglist.Add(pi);
                }

                return imglist;

            }
            catch
            {
                return null;
            }

        }

        public void GetStandardinclusionAndUpgradeOptionsList(int estimaterevisionid)
        {
            List<StandardInlusionAndUpgradeOption> l = new List<StandardInlusionAndUpgradeOption>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetStandardinclusionAndUpgradeOptionsList");
                SqlCmd.Parameters["@estimaterevisionid"].Value = estimaterevisionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                if (Sdr.Tables[0].Rows.Count > 0)
                {
                    foreach (DataRow dr in Sdr.Tables[0].Rows)
                    {
                        StandardInlusionAndUpgradeOption si = new StandardInlusionAndUpgradeOption();
                        si.Standardinclusion = dr["fkidinclusionproduct"].ToString();
                        si.UpgradeOption = dr["fkidupgradeproduct"].ToString();
                    }
                }
            }
            catch
            {
                throw;
            }

        }

        public void UnlockEstimate(int estimaterevisionid, int type)
        {
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_UnlockEstimate");
                SqlCmd.Parameters["@estimaterevisionid"].Value = estimaterevisionid;
                SqlCmd.Parameters["@type"].Value = type;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            }
            catch
            {
                throw;
            }

        }

        public List<ValidationErrorMessage> ValidateStudioMEstimate(int estimaterevisionid)
        {
            List<ValidationErrorMessage> l = new List<ValidationErrorMessage>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_StudioMValidateEstimate");
                SqlCmd.Parameters["@estimaterevisionid"].Value = estimaterevisionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    ValidationErrorMessage s = new ValidationErrorMessage();
                    //s.EstimateDetailsID = int.Parse(dr["siestimatedetailsid"].ToString());
                    s.HomeDisplayOptionId = int.Parse(dr["sioptionid"].ToString());
                    s.PagID = int.Parse(dr["sipagid"].ToString());
                    s.Area = dr["areaname"].ToString();
                    s.Group = dr["groupname"].ToString();
                    s.ErrorMessage = dr["errormessage"].ToString();
                    s.PossibleUpgrade = dr["upgrade"].ToString();
                    s.Reason = dr["reason"].ToString();
                    if (dr["sortorder"].ToString() == "0")
                    {
                        s.ErrorIcon = "../images/add.png";
                        s.ErrorIconToolTips = "This item should be added to estimate.";
                        s.AddVisible = true;
                        s.UpgradeVisible = true;
                        s.AnswerVisible = false;
                        s.AddImageOpacity = 1;
                        s.UpgradeImageOpacity = 1;
                        s.AnswerImageOpacity = 0.3;

                    }
                    else if (dr["sortorder"].ToString() == "1")
                    {
                        s.AnswerVisible = true;
                        s.AddVisible = false;
                        s.UpgradeVisible = false;
                        s.AddImageOpacity = 0.3;
                        s.UpgradeImageOpacity = 0.3;
                        s.AnswerImageOpacity = 1;
                        s.ErrorIcon = "../images/help.png";
                        s.ErrorIconToolTips = "Need answer for Studio M product.";
                    }
                    else
                    {
                        s.ErrorIcon = "../images/delete.png";
                        s.ErrorIconToolTips = "This item should be removed from estimate.";
                        s.AnswerVisible = false;
                        s.AddVisible = false;
                        s.UpgradeVisible = false;
                        s.AddImageOpacity = 0.3;
                        s.UpgradeImageOpacity = 0.3;
                        s.AnswerImageOpacity = 0.3;
                    }

                    if (dr["allowgoahead"].ToString() == "1")
                    {
                        s.AllowGoAhead = true;
                    }
                    else
                    {
                        s.AllowGoAhead = false;
                    }

                    l.Add(s);
                }

            }
            catch
            {
                throw;
            }
            return l;

        }

        public List<ValidationErrorMessage> ValidateAcceptFlagForRevision(int estimaterevisionid, int userroleid)
        {
            List<ValidationErrorMessage> l = new List<ValidationErrorMessage>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_ValidateAcceptFlagForRevision");
                SqlCmd.Parameters["@estimaterevisionid"].Value = estimaterevisionid;
                SqlCmd.Parameters["@userroleid"].Value = userroleid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    ValidationErrorMessage s = new ValidationErrorMessage();
                    s.Area = dr["areaname"].ToString();
                    s.Group = dr["groupname"].ToString();
                    s.ErrorMessage = dr["errormessage"].ToString();
                    s.PossibleUpgrade = dr["upgrade"].ToString();
                    s.Reason = dr["reason"].ToString();
                    s.AllowGoAhead = bool.Parse(dr["AllowGoAhead"].ToString());
                    s.ErrorIconToolTips = dr["ErrorIconToolTips"].ToString();
                    s.ErrorIcon = dr["ErrorIconpath"].ToString();
                    l.Add(s);
                }

            }
            catch
            {
                throw;
            }
            return l;

        }
        public List<SimplePAG> GetUpgradeOptionListForStandardInclusion(int estimaterevisionid, int optionid)
        {
            List<SimplePAG> l = new List<SimplePAG>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetUpgradeOptionsForStandardInclusion");
                SqlCmd.Parameters["@revisionid"].Value = estimaterevisionid;
                SqlCmd.Parameters["@originateoptionid"].Value = optionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    SimplePAG s = new SimplePAG();
                    s.AreaName = dr["areaname"].ToString();
                    s.AreaID = 0;
                    s.GroupName = dr["groupname"].ToString();
                    s.GroupID = 0;
                    s.ProductAreaGroupID = int.Parse(dr["ProductAreaGroupID"].ToString());
                    s.Selected = bool.Parse(dr["selected"].ToString());
                    s.ProductID = dr["productid"].ToString();
                    s.ProductName = dr["ProductName"].ToString();
                    //s.EstimateDetailsID = int.Parse(dr["EstimateDetailsID"].ToString());
                    s.HomeDisplayOptionID = int.Parse(dr["OptionId"].ToString());

                    l.Add(s);
                }

            }
            catch
            {
                l = null;
            }
            return l;

        }
        public List<ValidationErrorMessage> ValidateStudioMRevisions(int estimaterevisionid)
        {
            List<ValidationErrorMessage> l = new List<ValidationErrorMessage>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_ValidateStudioMRevisions");
                SqlCmd.Parameters["@estimateRevisionId"].Value = estimaterevisionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    ValidationErrorMessage s = new ValidationErrorMessage();
                    s.Area = dr["AreaName"].ToString();
                    s.Group = dr["GroupName"].ToString();
                    s.ErrorMessage = dr["ProductID"].ToString() + " requires Studio M answers";
                    s.Reason = "Studio M answer is required";
                    s.PossibleUpgrade = "-";

                    l.Add(s);
                }

            }
            catch
            {
                throw;
            }
            return l;

        }

        public bool ValidateAppointmentDate(int estimaterevisionid)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            bool valid = false;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_ValidateAppointmentDate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimaterevisionid));

                    conn.Open();
                    int counter = (int)cmd.ExecuteScalar();

                    if (counter > 0)
                        valid = true;

                    conn.Close();
                }
            }

            return valid;
        }

        public DataSet GetContactsOfCustomer(string customercode)
        {
            DataSet Sdr;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetContactsOfCustomer");
                SqlCmd.Parameters["@customercode"].Value = customercode;
                Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            }
            catch
            {
                return null;
            }
            return Sdr;
        }

        public List<StudioMItem> GetItemsNeedSetDefaultAnswer(string revisionid)
        {
            List<StudioMItem> l = new List<StudioMItem>();
            DataSet Sdr;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetItemsNeedSetDefaultAnswer");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    StudioMItem item = new StudioMItem();
                    item.MRSEstimateDetailsId = int.Parse(dr["id_salesestimate_estimatedetails"].ToString());
                    item.StudioMDefaultAnswer = "";
                    item.StudioMQestion = dr["studiomqanda"].ToString();

                    l.Add(item);
                }

            }
            catch
            {
                return null;
            }
            return l;
        }

        public bool SetDefaultAnswerForEstimateRevision(string idstring, string studiomstring, string usercode)
        {
            DataSet Sdr;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_SetDefaultAnswerForEstimateRevision");
                SqlCmd.Parameters["@idstring"].Value = idstring;
                SqlCmd.Parameters["@studiomstring"].Value = studiomstring;
                SqlCmd.Parameters["@usercode"].Value = usercode;
                Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                return true;

            }
            catch
            {
                return false;
            }

        }

        public bool UpdateItemAcceptance(string revisionestimatedetailsid, int accepted, int userid)
        {
            DataSet Sdr;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_UpdateItemAcceptance");
                SqlCmd.Parameters["@revisionestimatedetailsid"].Value = revisionestimatedetailsid;
                SqlCmd.Parameters["@accepted"].Value = accepted;
                SqlCmd.Parameters["@userid"].Value = userid;
                Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public ContractDraftActionAvailability GetContractDraftActions(int estimateRevisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            ContractDraftActionAvailability availability = new ContractDraftActionAvailability();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetContractDraftActions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (Convert.ToBoolean(dr["StudioMExists"]))
                            {
                                availability.ColourSelectionAvailable = false;
                                availability.CarpetSelectionAvailable = false;
                                availability.DeckingSelectionAvailable = false;
                                availability.ElectricalSelectionAvailable = false;
                                availability.StudioMAvailable = false;
                                availability.PavingSelectionAvailable = false;
                                availability.TilesSelectionAvailable = false;
                                availability.CurtainSelectionAvailable = false;
                                availability.FloorSelectionAvailable = false;
                                availability.ApplianceSelectionAvailable = false;
                                availability.LandscapingSelectionAvailable = false;
                            }
                            else
                            {
                                if (Convert.ToBoolean(dr["ColourSelectionExists"]))
                                {
                                    availability.ColourSelectionAvailable = false;

                                    if (Convert.ToBoolean(dr["ElectricalSelectionExists"]))
                                    {
                                        availability.ColourSelectionAvailable = false;
                                        availability.ElectricalSelectionAvailable = false;

                                        if (Convert.ToBoolean(dr["PavingSelectionExists"]))
                                            availability.PavingSelectionAvailable = false;
                                        else
                                            availability.PavingSelectionAvailable = true;

                                        if (Convert.ToBoolean(dr["TileSelectionExists"]))
                                            availability.TilesSelectionAvailable = false;
                                        else
                                            availability.TilesSelectionAvailable = true;

                                        if (Convert.ToBoolean(dr["DeckingSelectionExists"]))
                                            availability.DeckingSelectionAvailable = false;
                                        else
                                            availability.DeckingSelectionAvailable = true;

                                        if (Convert.ToBoolean(dr["CarpetSelectionExists"]))
                                            availability.CarpetSelectionAvailable = false;
                                        else
                                            availability.CarpetSelectionAvailable = true;

                                        if (Convert.ToBoolean(dr["CurtainSelectionExists"]))
                                            availability.CurtainSelectionAvailable = false;
                                        else
                                            availability.CurtainSelectionAvailable = true;

                                        if (Convert.ToBoolean(dr["FloorSelectionExists"]))
                                            availability.FloorSelectionAvailable = false;
                                        else
                                            availability.FloorSelectionAvailable = true;

                                        if (Convert.ToBoolean(dr["ApplianceSelectionExists"]))
                                            availability.ApplianceSelectionAvailable = false;
                                        else
                                            availability.ApplianceSelectionAvailable = true;

                                        if (Convert.ToBoolean(dr["LandscapingSelectionExists"]))
                                            availability.LandscapingSelectionAvailable = false;
                                        else
                                            availability.LandscapingSelectionAvailable = true;

                                        //If no Studio M revision is In Progress
                                        if (!Convert.ToBoolean(dr["ColourSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["ElectricalSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["PavingSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["TileSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["DeckingSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["CarpetSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["CurtainSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["FloorSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["ApplianceSelectionInProgress"]) &&
                                            !Convert.ToBoolean(dr["LandscapingSelectionInProgress"]))
                                        {
                                            availability.StudioMAvailable = true;
                                        }
                                    }
                                    else
                                    {
                                        availability.ColourSelectionAvailable = false;
                                        availability.CarpetSelectionAvailable = false;
                                        availability.DeckingSelectionAvailable = false;
                                        availability.ElectricalSelectionAvailable = true;
                                        availability.StudioMAvailable = false;
                                        availability.PavingSelectionAvailable = false;
                                        availability.TilesSelectionAvailable = false;
                                        availability.CurtainSelectionAvailable = false;
                                        availability.FloorSelectionAvailable = false;
                                        availability.ApplianceSelectionAvailable = false;
                                        availability.LandscapingSelectionAvailable = false;
                                    }
                                }
                                else
                                {
                                    availability.ColourSelectionAvailable = true;
                                    availability.CarpetSelectionAvailable = false;
                                    availability.DeckingSelectionAvailable = false;
                                    availability.ElectricalSelectionAvailable = false;
                                    availability.StudioMAvailable = false;
                                    availability.PavingSelectionAvailable = false;
                                    availability.TilesSelectionAvailable = false;
                                    availability.CurtainSelectionAvailable = false;
                                    availability.FloorSelectionAvailable = false;
                                    availability.ApplianceSelectionAvailable = false;
                                    availability.LandscapingSelectionAvailable = false;
                                }
                            }

                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return availability;
        }

        public FinalContractActionAvailability GetFinalContractActions(int estimateRevisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            FinalContractActionAvailability availability = new FinalContractActionAvailability();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetFinalContractActions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            availability.PreSiteVariationAvailable = !Convert.ToBoolean(dr["PresiteVariationInProgress"]);
                            availability.BuildingVariationAvailable = !Convert.ToBoolean(dr["BuildingVariationInProgress"]);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return availability;
        }

        public CustomerSupportActionAvailability GetCustomerSupportActions(int estimateRevisionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            CustomerSupportActionAvailability availability = new CustomerSupportActionAvailability();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetCustomerSupportActions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            availability.CustomerSupportAvailable = Convert.ToBoolean(dr["ValidToCreateCustomerSupport"]);
                            availability.PreStudioVariationAvailable = Convert.ToBoolean(dr["ValidToCreatePreStudioMVariation"]);
                            availability.ContractDraftAvailable = Convert.ToBoolean(dr["ValidToCreateContractDraft"]);
                            availability.AssignSTMSplitAvailable = Convert.ToBoolean(dr["ValidToCreateAssignSTMSplit"]);
                            availability.FinalContractAvailable = Convert.ToBoolean(dr["ValidToCreateFinalContract"]);
                            availability.PreSiteVariationAvailable = Convert.ToBoolean(dr["ValidToCreatePreSiteVariation"]);
                            availability.BuildingVariationAvailable = Convert.ToBoolean(dr["ValidToCreateBuildingVariation"]);
                            availability.ChangeContractTypeAvailable = Convert.ToBoolean(dr["ValidToChangeContractType"]);
                            availability.ChangeJobFlowTypeAvailable = Convert.ToBoolean(dr["ValidToChangeJobFlowType"]);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return availability;
        }

        public SalesEstimatorActionAvailability GetSalesEstimatorActions(int estimateRevisionId, int userid)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            SalesEstimatorActionAvailability availability = new SalesEstimatorActionAvailability();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetSalesEstimatorActions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@userid", userid));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            availability.ChangeFacadeAvailable = Convert.ToBoolean(dr["ValidToChangeFacade"]);
                            availability.ChangeContractTypeAvailable = Convert.ToBoolean(dr["ValidToChangeContractType"]);
                            availability.ChangeJobFlowTypeAvailable = Convert.ToBoolean(dr["ValidToChangeJobFlowType"]);
                            availability.ChangePriceEffectiveDateAvailable = Convert.ToBoolean(dr["ValidToChangePriceEffectiveDate"]);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return availability;
        }

        public bool GetContractDraftCreationVisibility(int estimateRevisionId)
        {
            bool validToCreate = false;

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetContractDraftCreationVisibility";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            validToCreate = Convert.ToBoolean(dr["ValidToCreateContractDraft"]);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return validToCreate;
        }

        public bool GetFinalContractCreationVisibility(int estimateRevisionId)
        {
            bool validToCreate = false;

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetFinalContractCreationVisibility";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            validToCreate = Convert.ToBoolean(dr["ValidToCreateFinalContract"]);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return validToCreate;
        }

        public void CreateSplitStudioMRevisions(int estimateRevisionId, string revisionTypeIds, string assignedToUserIds, int createdbyId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CreateSplitStudioMRevisions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@revisionTypeIds", revisionTypeIds));
                    cmd.Parameters.Add(new SqlParameter("@assignedToUserIds", assignedToUserIds));
                    cmd.Parameters.Add(new SqlParameter("@createdbyId", createdbyId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void MergeStudioMRevisions(int estimateRevisionId, int createdbyId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_MergeStudioMRevisions";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@createdbyId", createdbyId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void CreateContractDraft(int estimateRevisionId, int createdbyId/*, DateTime appointment*/)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CreateContractDraft";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@createdbyId", createdbyId));
                    //cmd.Parameters.Add(new SqlParameter("@appointment", appointment));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void CreateFinalContract(int estimateRevisionId, int createdbyId/*, DateTime appointment*/)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CreateFinalContract";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@createdbyId", createdbyId));
                    //cmd.Parameters.Add(new SqlParameter("@appointment", appointment));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void CreateCscVariation(int estimateRevisionId, int createdbyId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CreateCscVariation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@createdbyId", createdbyId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public string CreateStudioMRevision(int estimateRevisionId, int ownerId, DateTime appointmentDateTime, int revisionTypeId, int createdbyId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string result = "0";

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CreateStudioMEstimate";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@ownerId", ownerId));
                    cmd.Parameters.Add(new SqlParameter("@appointmentDate", appointmentDateTime));
                    cmd.Parameters.Add(new SqlParameter("@revisionTypeId", revisionTypeId));
                    cmd.Parameters.Add(new SqlParameter("@createdbyId", createdbyId));
                    conn.Open();
                    //cmd.ExecuteNonQuery();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["newEstimateRevisionId"] != DBNull.Value)
                                result = dr["newEstimateRevisionId"].ToString();
                        }
                        dr.Close();
                    }
                    conn.Close();
                }
            }

            return result;
        }

        public void CreateVariation(int estimateRevisionId, int revisionTypeId, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CreateVariation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@revisionTypeId", revisionTypeId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public void RejectVariation(int estimateRevisionId, int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_RejectVariation";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public string ValidateSetEstimateStatus(int estimateRevisionId, int nextRevisionTypeId)
        {
            string errorMessage = null;

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_ValidateSetEstimateStatus";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    cmd.Parameters.Add(new SqlParameter("@nextRevisionTypeId", nextRevisionTypeId));
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["ErrorMessage"] != DBNull.Value)
                                errorMessage = dr["ErrorMessage"].ToString();
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return errorMessage;
        }

        public string GetCustomerDocumentType(int estimateRevisionId)
        {
            string documentType = null;

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetCustomerDocumentType";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            if (dr["DocumentType"] != DBNull.Value)
                                documentType = dr["DocumentType"].ToString();
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return documentType;
        }

        public CustomerDocumentDetails GetCustomerDocumentDetails(int estimateRevisionId)
        {
            CustomerDocumentDetails document = null;

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetCustomerDocumentDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", estimateRevisionId));
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            document = new CustomerDocumentDetails();

                            document.Active = Convert.ToBoolean(dr["Active"]);

                            if (dr["SentDate"] != DBNull.Value)
                                document.SentDate = Convert.ToDateTime(dr["SentDate"]);

                            if (dr["AcceptedDate"] != DBNull.Value)
                                document.AcceptedDate = Convert.ToDateTime(dr["AcceptedDate"]);

                            if (dr["DocumentNumber"] != DBNull.Value)
                                document.DocumentNumber = Convert.ToInt32(dr["DocumentNumber"]);

                            if (dr["ExtensionDays"] != DBNull.Value)
                                document.ExtensionDays = Convert.ToInt32(dr["ExtensionDays"]);

                            document.DocumentType = dr["DocumentType"].ToString();

                            document.CustomerDocumentID = Convert.ToInt32(dr["CustomerDocumentId"]);
                            document.DocumentSummary = dr["summary"].ToString();
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }
            return document;
        }

        public int UpdateCustomerDocumentDetails(CustomerDocumentDetails document)
        {
            int customerDocumentId = 0;
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            if (document.DocumentSummary == null) document.DocumentSummary = "";

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateCustomerDocumentDetails";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@estimateRevisionId", document.EstimateRevisionID));
                    cmd.Parameters.Add(new SqlParameter("@active", document.Active));
                    cmd.Parameters.Add(new SqlParameter("@sentDate", document.SentDate));
                    cmd.Parameters.Add(new SqlParameter("@acceptedDate", document.AcceptedDate));
                    cmd.Parameters.Add(new SqlParameter("@documentNumber", document.DocumentNumber));
                    cmd.Parameters.Add(new SqlParameter("@documentType", document.DocumentType));
                    cmd.Parameters.Add(new SqlParameter("@extensionDays", document.ExtensionDays));
                    cmd.Parameters.Add(new SqlParameter("@customerDocumentId", document.CustomerDocumentID));
                    cmd.Parameters.Add(new SqlParameter("@userId", document.UserId));
                    cmd.Parameters.Add(new SqlParameter("@summary", document.DocumentSummary));
                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            customerDocumentId = Convert.ToInt32(dr["CustomerDocumentId"]);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return customerDocumentId;
        }

        public List<SimplePAG> GetRelevantPAGFromOnePAG(string optionid, string revisionid)
        {
            List<SimplePAG> l = new List<SimplePAG>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetAllRelevantPAGFromOnePAG");
                SqlCmd.Parameters["@HomeDisplayOptionId"].Value = optionid;
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    SimplePAG pag = new SimplePAG();
                    pag.AreaID = int.Parse(dr["areaid"].ToString());
                    pag.AreaName = dr["areaname"].ToString();
                    pag.GroupID = int.Parse(dr["groupid"].ToString());
                    pag.ProductAreaGroupID = int.Parse(dr["productareagroupid"].ToString());
                    pag.StandardInclusionsID = int.Parse(dr["idStandardInclusions"].ToString());
                    pag.GroupName = dr["groupname"].ToString();
                    pag.ProductID = dr["productid"].ToString();
                    //pag.EstimateDetailsID = int.Parse(dr["estimatedetailsid"].ToString());
                    pag.HomeDisplayOptionID = int.Parse(dr["OptionId"].ToString());
                    pag.Selected = bool.Parse(dr["selected"].ToString());
                    pag.IsSiteWork = bool.Parse(dr["IsSiteWork"].ToString());
                    pag.IsStandardOption = bool.Parse(dr["StandardOption"].ToString());
                    pag.DisplayAt = dr["DisplayAt"].ToString();
                    if (dr["DerivedCost"].ToString() == "1")
                    {
                        pag.DerivedCost = true;
                    }
                    else
                    {
                        pag.DerivedCost = false;
                    }
                    pag.CostBTPExcGST = decimal.Parse(dr["CostExcGST"].ToString());
                    pag.CostDBCExcGST = decimal.Parse(dr["DBCCostExcGST"].ToString());
                    pag.ProductDescription = dr["ProductDescription"].ToString();
                    pag.AdditionalNotes = dr["AdditionalInfo"].ToString();
                    pag.ExtraDescription = dr["ExtraDescription"].ToString();
                    pag.InternalDescription = dr["InternalDescription"].ToString();
                    pag.Quantity = decimal.Parse(dr["Quantity"].ToString());
                    pag.Price = decimal.Parse(dr["Price"].ToString());
                    pag.Margin = dr["margin"].ToString();
                    pag.Uom = dr["Uom"].ToString();
                    pag.PriceDisplayCodeDesc = dr["PriceDisplayDescription"].ToString();
                    pag.PriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeId"].ToString());
                    if (dr["changeprice"].ToString() == "0")
                    {
                        pag.ItemAllowToChangePrice = false;
                    }
                    else
                    {
                        pag.ItemAllowToChangePrice = true;
                    }
                    if (dr["changeqty"].ToString() == "0")
                    {
                        pag.ItemAllowToChangeQuantity = false;
                    }
                    else
                    {
                        pag.ItemAllowToChangeQuantity = true;
                    }

                    if (dr["changedisplaycode"].ToString() == "0")
                    {
                        pag.ItemAllowToChangeDisplayCode = false;
                    }
                    else
                    {
                        pag.ItemAllowToChangeDisplayCode = true;
                    }

                    if (dr["changeproductstandarddescription"].ToString() == "0")
                    {
                        pag.ItemAllowToChangeDescription = false;
                    }
                    else
                    {
                        pag.ItemAllowToChangeDescription = true;
                    }
                    pag.IsInLieuExistStandard = dr["InLieuExistStandard"].ToString() == "0" ? false : true;
                    pag.IsInLieuExistPromo = dr["InLieuExistPromo"].ToString() == "0" ? false : true;
                    pag.IsInLieuExist = pag.IsInLieuExistStandard || pag.IsInLieuExistPromo;
                    pag.PriceStandard = decimal.Parse(dr["PriceStandard"].ToString());
                    pag.PricePromo = decimal.Parse(dr["PricePromo"].ToString());
                    pag.CostStandardExcGST = decimal.Parse(dr["CostStandardExcGST"].ToString());
                    pag.CostPromoExcGST = decimal.Parse(dr["CostPromoExcGST"].ToString());
                    pag.PriceStandardSelected = true;

                    l.Add(pag);
                }

            }
            catch (Exception)
            {
                l = null;
            }
            return l;

        }

        public List<EstimateDetails> PopulateEstimateDetailsListFromDataSet(DataSet Sdr)
        {
            List<EstimateDetails> result = new List<EstimateDetails>();

            if (Sdr != null && Sdr.Tables.Count > 0)
            {
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    EstimateDetails details = new EstimateDetails();
                    details.EstimateRevisionDetailsId = Convert.ToInt32(dr["id_SalesEstimate_EstimateDetails"]);
                    details.AreaId = Convert.ToInt32(dr["AreaId"]);
                    details.GroupId = Convert.ToInt32(dr["GroupId"]);
                    details.AreaName = dr["AreaName"].ToString();
                    details.GroupName = dr["GroupName"].ToString();
                    details.ProductId = dr["ProductID"].ToString();
                    details.ProductName = dr["ProductName"].ToString();
                    details.ProductDescription = dr["ProductDescription"].ToString();
                    details.UpdatedProductDescription = dr["ProductDescription"].ToString();
                    details.ProductDescriptionShort = details.ProductDescription.Length > 100 ? details.ProductDescription.Substring(0, 100) + "..." : details.ProductDescription;
                    details.ExtraDescription = dr["enterdesc"].ToString();
                    details.UpdatedExtraDescription = dr["enterdesc"].ToString();
                    details.InternalDescription = dr["internaldescription"].ToString();
                    details.UpdatedInternalDescription = dr["internaldescription"].ToString();
                    details.AdditionalNotes = dr["additionalinfo"].ToString();
                    details.UpdatedAdditionalNotes = dr["additionalinfo"].ToString();
                    details.Uom = dr["uom"].ToString();
                    details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                    details.Price = Convert.ToDecimal(dr["sellprice"]);
                    details.UpdatedQuantity = Convert.ToDecimal(dr["Quantity"]);
                    details.UpdatedPrice = Convert.ToDecimal(dr["sellprice"]);
                    details.PromotionProduct = Convert.ToBoolean(dr["PromotionProduct"]);
                    details.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                    //details.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]);
                    details.HomeDisplayOptionId = Convert.ToInt32(dr["fkidHomeDisplayOption"] is DBNull ? 0 : dr["fkidHomeDisplayOption"]);
                    details.ProductAreaGroupID = Convert.ToInt32(dr["ProductAreaGroupID"]);
                    details.TotalPrice = Convert.ToDecimal(dr["totalprice"]);
                    details.UpdatedTotalPrice = Convert.ToDecimal(dr["totalprice"]);
                    details.ItemAccepted = Convert.ToBoolean(dr["ItemAccepted"]);
                    details.StudioMProduct = Convert.ToBoolean(dr["isstudiomproduct"]);
                    details.ProductPhotoCount = int.Parse(dr["imagecount"].ToString());
                    details.StudioMQuestion = dr["studiomquestion"].ToString();
                    details.StudioMAnswer = dr["StudioMAttributes"].ToString();
                    details.SiteWorkItem = bool.Parse(dr["SiteWorkItem"].ToString());
                    details.UpdatedSiteWorkItem = bool.Parse(dr["SiteWorkItem"].ToString());
                    details.StandardInclusionId = Convert.ToInt32(dr["idStandardInclusions"]);
                    details.StudioMAnswerMandatory = bool.Parse(dr["qandamandatory"].ToString());

                    details.NonstandardCategoryID = Convert.ToInt32(dr["NonstandardCategoryID"]);
                    details.NonstandardGroupID = Convert.ToInt32(dr["nonstandardgroupid"]);
                    details.UpdatedNonstandardCategoryID = Convert.ToInt32(dr["NonstandardCategoryID"]);
                    details.UpdatedNonstandardGroupID = Convert.ToInt32(dr["nonstandardgroupid"]);

                    details.SOSI = dr["SOSI"].ToString();
                    details.SOSIToolTips = dr["SOSIToolTips"].ToString();
                    details.StudioMIcon = dr["StudioMIcon"].ToString();
                    details.StudioMTooltips = dr["StudioMTooltips"].ToString();

                    details.CreatedByUserId = Convert.ToInt32(dr["CreatedBy"]);
                    details.CreatedByUserManagerIds = GetManagersByUserAndRevisionId(details.CreatedByUserId, Convert.ToInt32(dr["revisionId"]));

                    details.PriceDisplayCodeDesc = dr["pricedisplaydesc"].ToString();
                    details.PriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());
                    if (dr["allowtoremove"].ToString().ToUpper() == "TRUE" || dr["allowtoremove"].ToString() == "1")
                    {
                        details.ItemAllowToRemove = true;
                    }
                    else
                    {
                        details.ItemAllowToRemove = false;
                    }

                    if (dr["changeprice"].ToString() == "0")
                    {
                        details.ItemAllowToChangePrice = false;
                    }
                    else
                    {
                        details.ItemAllowToChangePrice = true;
                    }
                    if (dr["changeqty"].ToString() == "0")
                    {
                        details.ItemAllowToChangeQuantity = false;
                    }
                    else
                    {
                        details.ItemAllowToChangeQuantity = true;
                    }

                    if (dr["changedisplaycode"].ToString() == "0")
                    {
                        details.ItemAllowToChangeDisplayCode = false;
                    }
                    else
                    {
                        details.ItemAllowToChangeDisplayCode = true;
                    }

                    if (dr["changeproductstandarddescription"].ToString() == "0")
                    {
                        details.ItemAllowToChangeDescription = false;
                    }
                    else
                    {
                        details.ItemAllowToChangeDescription = true;
                    }

                    if (dr["studiomsortorder"] != DBNull.Value)
                        details.StudioMSortOrder = Convert.ToInt32(dr["studiomsortorder"]);
                    else
                        details.StudioMSortOrder = 99999;

                    details.PriceDisplayCodeDesc = dr["pricedisplaydesc"].ToString();
                    details.PriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());
                    details.UpdatedPriceDisplayCodeId = int.Parse(dr["PriceDisplayCodeID"].ToString());

                    details.PreviousChanged = bool.Parse(dr["PreviousChanged"].ToString());
                    details.Changed = bool.Parse(dr["changed"].ToString());
                    details.SelectedImageID = dr["selectedimageid"].ToString();

                    if (dr["derivedcost"].ToString() == "0" || dr["derivedcost"].ToString().ToUpper() == "FALSE")
                    {
                        details.DerivedCost = false;
                        details.DerivedCostIcon = "./images/spacer.gif";
                        details.DerivedCostTooltips = "";
                    }
                    else
                    {
                        details.DerivedCost = true;
                        details.DerivedCostIcon = "./images/link.png";
                        details.DerivedCostTooltips = "Derived cost.";
                    }

                    if (dr["CostExcGST"] != null)
                    {
                        details.CostExcGST = dr["CostExcGST"].ToString();
                        details.UpdatedBTPCostExcGST = dr["CostExcGST"].ToString();
                        details.UpdatedDBCCostExcGST = dr["DBCCostExcGST"].ToString();
                    }
                    else
                    {
                        details.CostExcGST = "";
                        details.UpdatedBTPCostExcGST = "";
                        details.UpdatedDBCCostExcGST = "";

                    }
                    details.DerivedCostIcon = dr["DerivedCostIcon"].ToString();
                    details.DerivedCostTooltips = dr["DerivedCostTooltips"].ToString();
                    details.Margin = dr["margin"].ToString();
                    details.MarginDBCCost =dr["margindbccost"].ToString();
                    details.MarginString = details.Margin;
                    if (!string.IsNullOrWhiteSpace(details.MarginString))
                        details.MarginString += "%";
                    details.Changes = dr["changetype"].ToString();
                    if (dr["ismasterpromotion"].ToString() == "0" || dr["ismasterpromotion"].ToString().ToUpper() == "FALSE")
                    {
                        details.IsMasterPromotion = false;
                    }
                    else
                    {
                        details.IsMasterPromotion = true;
                    }
                    if (dr.Table.Columns.Contains("homeid"))
                    {
                        details.Homeid = int.Parse(dr["homeid"].ToString());
                    }
                    if (dr.Table.Columns.Contains("homedisplayid"))
                    {
                        details.HomeDisplayID = int.Parse(dr["homedisplayid"].ToString());
                    }
                    Int16 standardPackageInclusion = -1;
                    Int16.TryParse(dr["StandardPackageInclusion"].ToString(), out standardPackageInclusion);
                    if (dr["StandardPackageInclusion"] == DBNull.Value || standardPackageInclusion < 0 || standardPackageInclusion > 4)
                    {
                        details.IsPrePackageItem = false;
                    }
                    else
                    {
                        details.IsPrePackageItem = true;
                    }
                    details.PrePackageItemDescription = dr["StandardPackageInclusionComment"].ToString();
                    details.IsColMarginPercentAvailable = false;

                    result.Add(details);
                }
            }

            return result;
        }

        public List<OptionTreeProducts> PopulateOptionTreeProductsListFromDataSet(DataSet Sdr)
        {
            List<OptionTreeProducts> products = new List<OptionTreeProducts>();

            if (Sdr != null && Sdr.Tables.Count > 0)
            {
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    OptionTreeProducts product = new OptionTreeProducts();

                    product.AreaName = dr["AreaName"].ToString();
                    product.GroupName = dr["GroupName"].ToString();
                    product.AreaId = Convert.ToInt32(dr["AreaId"]);
                    product.GroupId = Convert.ToInt32(dr["GroupId"]);
                    product.ProductId = dr["ProductId"].ToString();
                    product.ProductName = dr["ProductName"].ToString();
                    product.ProductDescription = dr["ProductDescription"].ToString();
                    product.Quantity = Convert.ToDecimal(dr["Quantity"]);
                    product.Price = Convert.ToDecimal(dr["sellprice"]);
                    product.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                    //product.EstimateDetailsId = Convert.ToInt32(dr["EstimateDetailsId"]); //Estimate Details ID or Option ID 
                    product.HomeDisplayOptionId = Convert.ToInt32(dr["OptionId"]);
                    if (dr["siteworkitem"] != DBNull.Value)
                        product.IsSiteWork = Convert.ToBoolean(dr["siteworkitem"]);
                    product.StudioMSortOrder = Convert.ToInt32(dr["StudioMSortOrder"]);
                    product.UOM = dr["uom"].ToString();

                    bool isStudioM = Convert.ToBoolean(dr["isstudiomproduct"]);
                    bool isMandatory = Convert.ToBoolean(dr["qandamandatory"]);
                    string studioQandA = dr["studiomquestion"] == DBNull.Value ? string.Empty : dr["studiomquestion"].ToString();

                    if (isStudioM)
                    {
                        if (isMandatory)
                            product.StudioMProduct = 1; // Studio M Mandatory
                        else
                        {
                            if (string.IsNullOrEmpty(studioQandA))
                                product.StudioMProduct = 2; // Studio M No question
                            else
                                product.StudioMProduct = 3; // Studio M Non-mandatory
                        }
                    }
                    else
                        product.StudioMProduct = 0; // Non Studio M

                    products.Add(product);
                }
            }

            return products;
        }

        public List<OptionTreeProducts> PopulateOptionTreeProductsListOnlinePriceBookFromDataSet(DataSet Sdr)
        {
            List<OptionTreeProducts> products = new List<OptionTreeProducts>();

            if (Sdr != null && Sdr.Tables.Count > 0)
            {
                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    OptionTreeProducts product = new OptionTreeProducts();

                    product.AreaName = dr["AreaName"].ToString();
                    product.GroupName = dr["GroupName"].ToString();
                    if (Sdr.Tables[0].Columns["AreaId"] != null)
                        product.AreaId = Convert.ToInt32(dr["AreaId"]);
                    if (Sdr.Tables[0].Columns["GroupId"] != null)
                        product.GroupId = Convert.ToInt32(dr["GroupId"]);
                    product.ProductId = dr["ProductId"].ToString();
                    product.ProductName = dr["ProductName"].ToString();
                    product.ProductDescription = dr["ProductDescription"].ToString();
                    product.Quantity = Convert.ToDecimal(dr["Quantity"]);
                    if (Sdr.Tables[0].Columns["unitprice"] != null)
                        product.Price = Convert.ToDecimal(dr["unitprice"]);
                    if (Sdr.Tables[0].Columns["UOM"] != null)
                        product.UOM = dr["UOM"].ToString();
                    if (Sdr.Tables[0].Columns["StandardOption"] != null)
                        product.StandardOption = Convert.ToBoolean(dr["StandardOption"]);
                    if (Sdr.Tables[0].Columns["OptionId"] != null)
                        product.HomeDisplayOptionId = Convert.ToInt32(dr["OptionId"]);

                    products.Add(product);
                }
            }

            return products;
        }

        public List<EstimateDetails> SaveSelectedItemsFromOptionTreeToEstimate(string optionidstring,
            string standardinclusionidstring,
            string revisionid,
            string studiomanswer,
            string userid,
            string action,
            string derivedcost,
            string costbtpexcgststring,
            string costbtpoverwriteflagstring,
            string costdbcexcgststring,
            string costdbcoverwriteflagstring,
            string quantitystring,
            string pricestring,
            string isacceptedstring,
            string areaidstring,
            string groupidstring,
            string pricedisplaycodestring,
            string issiteworkstring,
            string productdescriptionstring,
            string additionalnotestring,
            string extradescriptionstring,
            string internaldescriptionstring)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_SaveSelectedItemsFromOptionTreeToEstimate");
            SqlCmd.Parameters["@optionidstring"].Value = optionidstring;
            SqlCmd.Parameters["@standardinclusionidstring"].Value = standardinclusionidstring;
            SqlCmd.Parameters["@revisionid"].Value = revisionid;
            SqlCmd.Parameters["@studiomanswer"].Value = studiomanswer;
            SqlCmd.Parameters["@derivedcoststring"].Value = derivedcost;
            SqlCmd.Parameters["@costbtpexcgststring"].Value = costbtpexcgststring;
            SqlCmd.Parameters["@costbtpoverwriteflagstring"].Value = costbtpoverwriteflagstring;
            SqlCmd.Parameters["@costdbcexcgststring"].Value = costdbcexcgststring;
            SqlCmd.Parameters["@costdbcoverwriteflagstring"].Value = costdbcoverwriteflagstring;
            SqlCmd.Parameters["@quantitystring"].Value = quantitystring;
            SqlCmd.Parameters["@pricestring"].Value = pricestring;
            SqlCmd.Parameters["@isacceptedstring"].Value = isacceptedstring;
            SqlCmd.Parameters["@areaidstring"].Value = areaidstring;
            SqlCmd.Parameters["@groupidstring"].Value = groupidstring;
            SqlCmd.Parameters["@pricedisplaycodestring"].Value = pricedisplaycodestring;
            SqlCmd.Parameters["@issiteworkstring"].Value = issiteworkstring;
            SqlCmd.Parameters["@productdescriptionstring"].Value = productdescriptionstring;
            SqlCmd.Parameters["@additionalnotestring"].Value = additionalnotestring;
            SqlCmd.Parameters["@extradescriptionstring"].Value = extradescriptionstring;
            SqlCmd.Parameters["@internaldescriptionstring"].Value = internaldescriptionstring;
            SqlCmd.Parameters["@userid"].Value = userid;
            SqlCmd.Parameters["@action"].Value = action;
            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateEstimateDetailsListFromDataSet(Sdr);
        }

        public List<EstimateDetails> ReplaceSaveSelectedItemsFromOptionTreeToEstimate(string sourceEstimateRevisionDetailsId, string optionidstring,
              string standardinclusionidstring,
              string revisionid,
              string studiomanswer,
              string userid,
              string action,
              string derivedcost,
              string costbtpexcgst,
              string costdbcexcgst,
              string quantitystring,
              string pricestring,
              string isacceptedstring,
              string areaidstring,
              string groupidstring,
              string pricedisplaycodestring,
              string issiteworkstring,
              string productdescriptionstring,
              string additionalnotestring,
              string extradescriptionstring,
              string internaldescriptionstring,
              string copyquantity,
              string copyadditionalnotes,
              string copyextradescription,
              string copyinternalnotes
            )
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_ReplaceSaveSelectedItemsFromOptionTreeToEstimate");
            SqlCmd.Parameters["@sourceEstimateRevisionDetailsId"].Value = sourceEstimateRevisionDetailsId;
            SqlCmd.Parameters["@optionidstring"].Value = optionidstring;
            SqlCmd.Parameters["@standardinclusionidstring"].Value = standardinclusionidstring;
            SqlCmd.Parameters["@revisionid"].Value = revisionid;
            SqlCmd.Parameters["@studiomanswer"].Value = studiomanswer;
            SqlCmd.Parameters["@derivedcoststring"].Value = derivedcost;
            SqlCmd.Parameters["@costbtpexcgststring"].Value = costbtpexcgst;
            SqlCmd.Parameters["@costdbcexcgststring"].Value = costdbcexcgst;
            SqlCmd.Parameters["@quantitystring"].Value = quantitystring;
            SqlCmd.Parameters["@pricestring"].Value = pricestring;
            SqlCmd.Parameters["@isacceptedstring"].Value = isacceptedstring;
            SqlCmd.Parameters["@areaidstring"].Value = areaidstring;
            SqlCmd.Parameters["@groupidstring"].Value = groupidstring;
            SqlCmd.Parameters["@pricedisplaycodestring"].Value = pricedisplaycodestring;
            SqlCmd.Parameters["@issiteworkstring"].Value = issiteworkstring;
            SqlCmd.Parameters["@productdescriptionstring"].Value = productdescriptionstring;
            SqlCmd.Parameters["@additionalnotestring"].Value = additionalnotestring;
            SqlCmd.Parameters["@extradescriptionstring"].Value = extradescriptionstring;
            SqlCmd.Parameters["@internaldescriptionstring"].Value = internaldescriptionstring;
            SqlCmd.Parameters["@copyquantity"].Value = @copyquantity;
            SqlCmd.Parameters["@copyadditionalnotes"].Value = @copyadditionalnotes;
            SqlCmd.Parameters["@copyextradescription"].Value = @copyextradescription;
            SqlCmd.Parameters["@copyinternalnotes"].Value = @copyinternalnotes;
            SqlCmd.Parameters["@userid"].Value = userid;
            SqlCmd.Parameters["@action"].Value = action;
            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateEstimateDetailsListFromDataSet(Sdr);
        }

        public List<SQSHome> GetAllFacadeFromRevisonID(int revisionid)
        {
            List<SQSHome> hlist = new List<SQSHome>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetFacadeForHomeByRevisionID");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow row in Sdr.Tables[0].Rows)
                {
                    SQSHome h = new SQSHome();
                    h.HomeID = Convert.ToInt32(row["homeid"].ToString());
                    h.HomeName = row["homename"].ToString();

                    hlist.Add(h);
                }

            }
            catch (Exception)
            {
                hlist = null;
            }

            return hlist;
        }

        public List<SQSHome> GetAllAvailableHomesByState(int stateid, string searchText, bool showdisplayhomes)
        {
            List<SQSHome> hlist = new List<SQSHome>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetAvailableHomesByState");
                SqlCmd.Parameters["@stateid"].Value = stateid;
                SqlCmd.Parameters["@homename"].Value = searchText;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow row in Sdr.Tables[0].Rows)
                {
                    SQSHome h = new SQSHome();
                    h.HomeID = Convert.ToInt32(row["homeid"].ToString());
                    h.HomeName = row["homename"].ToString();
                    if (row["displayhome"].ToString().ToUpper() == "TRUE" || row["displayhome"].ToString() == "1")
                    {
                        h.Display = true;
                    }
                    else
                    {
                        h.Display = false;
                    }
                    // simon selva - EstimateViewModel passes true for showdisplayhomes to get full list but changehome contextmenu only need non-display homes
                    if (showdisplayhomes || !h.Display)
                        hlist.Add(h);
                }

            }
            catch (Exception ex)
            {
                hlist = null;
            }

            return hlist;
        }
        public List<SQSHome> GetHomeFullNameByState(int stateid, int userId)
        {
            List<SQSHome> hlist = new List<SQSHome>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("spw_getHomeFullNameByState");
                SqlCmd.Parameters["@stateid"].Value = stateid;
                SqlCmd.Parameters["@usercode"].Value = userId;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow row in Sdr.Tables[0].Rows)
                {
                    SQSHome h = new SQSHome();
                    h.HomeID = Convert.ToInt32(row["homeid"].ToString());
                    h.HomeName = row["homename"].ToString();
                    hlist.Add(h);
                }

            }
            catch (Exception ex)
            {
                hlist = null;
            }

            return hlist;
        }
        public RoleAccessModule GetRoleAccessModule(int roleid)
        {
            RoleAccessModule ram = new RoleAccessModule();
            ram.RoleId = roleid;

            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetRoleAccessModule");
            SqlCmd.Parameters["@roleid"].Value = roleid;
            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            if (Sdr.Tables[0].Rows.Count > 0)
            {
                if (Sdr.Tables[0].Rows[0]["viewMarginmodule"].ToString().ToUpper() == "TRUE" || Sdr.Tables[0].Rows[0]["viewMarginmodule"].ToString().ToUpper() == "1")
                {
                    ram.AccessMarginModule = true;
                }
                if (Sdr.Tables[0].Rows[0]["viewstudiommodule"].ToString().ToUpper() == "TRUE" || Sdr.Tables[0].Rows[0]["viewstudiommodule"].ToString().ToUpper() == "1")
                {
                    ram.AccessStudioMModule = true;
                }
            }
            else
            {
                ram.AccessMarginModule = false;
                ram.AccessStudioMModule = false;
            }

            return ram;
        }

        public List<ValidationErrorMessage> CheckFacadeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            List<ValidationErrorMessage> hlist = new List<ValidationErrorMessage>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CheckFacadeConfigurationDifference");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                SqlCmd.Parameters["@newfacadehomeid"].Value = newfacadehomeid;
                SqlCmd.Parameters["@date"].Value = effectivedate;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow row in Sdr.Tables[0].Rows)
                {
                    ValidationErrorMessage h = new ValidationErrorMessage();
                    h.PagID = Convert.ToInt32(row["PagID"].ToString());
                    h.Area = row["areaname"].ToString();
                    h.Group = row["groupname"].ToString();
                    h.PossibleUpgrade = row["productname"].ToString();
                    h.ErrorMessage = row["error"].ToString();
                    h.SellPrice = Convert.ToDecimal(row["sellprice"].ToString());
                    h.Reason = row["reason"].ToString();
                    if (h.Reason == "0") // Not available in new facade.
                    {
                        h.ErrorIcon = "../images/cancel.png";
                        h.CopyAsNSR = true;
                    }
                    else if (h.Reason == "1") // Quantity changed.
                    {
                        h.ErrorIcon = "../images/accept.png";
                        h.QuantityUseCurrent = true;
                        h.QuantityUseNew = false;
                    }
                    else if (h.Reason == "2") // Unit price changed.
                    {
                        h.ErrorIcon = "../images/accept.png";
                        h.PriceUseCurrent = true;
                        h.PriceUseNew = false;
                    }
                    hlist.Add(h);
                }

            }
            catch (Exception)
            {
                hlist = null;
            }

            return hlist;
        }
        public List<ValidationErrorMessage> CheckHomeConfigurationDifference(int revisionid, int newfacadehomeid, string effectivedate)
        {
            List<ValidationErrorMessage> hlist = new List<ValidationErrorMessage>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CheckHomeConfigurationDifference");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                SqlCmd.Parameters["@newfacadehomeid"].Value = newfacadehomeid;
                SqlCmd.Parameters["@date"].Value = effectivedate;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow row in Sdr.Tables[0].Rows)
                {
                    ValidationErrorMessage h = new ValidationErrorMessage();
                    h.PagID = Convert.ToInt32(row["PagID"].ToString());
                    h.Area = row["areaname"].ToString();
                    h.Group = row["groupname"].ToString();
                    h.PossibleUpgrade = row["productname"].ToString();
                    h.ErrorMessage = row["error"].ToString();
                    h.SellPrice = Convert.ToDecimal(row["sellprice"].ToString());
                    h.Reason = row["reason"].ToString();
                    if (h.Reason == "0") // Not available in new facade.
                    {
                        h.ErrorIcon = "../images/cancel.png";
                        h.CopyAsNSR = true;
                    }
                    else if (h.Reason == "1") // Quantity changed.
                    {
                        h.ErrorIcon = "../images/accept.png";
                        h.QuantityUseCurrent = true;
                        h.QuantityUseNew = false;
                    }
                    else if (h.Reason == "2") // Unit price changed.
                    {
                        h.ErrorIcon = "../images/accept.png";
                        h.PriceUseCurrent = true;
                        h.PriceUseNew = false;
                    }
                    hlist.Add(h);
                }

            }
            catch (Exception)
            {
                hlist = null;
            }

            return hlist;
        }
        public bool ChangeFacade(int revisionid, int newfacadehomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid)
        {
            bool result = true;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_ChangeFacade");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                SqlCmd.Parameters["@newfacadehomeid"].Value = newfacadehomeid;
                SqlCmd.Parameters["@detailids"].Value = detailIDsSelected;
                SqlCmd.Parameters["@detailoptions"].Value = detailOptionsSeleced;
                SqlCmd.Parameters["@detailprices"].Value = detailPricesSeleced;
                SqlCmd.Parameters["@date"].Value = effectivedate;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;

            }
            catch (Exception ex)
            {
                result = false;
                Trace.TraceError(ex.Message);
            }

            return result;
        }

        public bool ChangeHome(int revisionid, int newhomeid, string detailIDsSelected, string detailOptionsSeleced, string detailPricesSeleced, string effectivedate, int userid)
        {
            bool result = true;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_ChangeHome");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                SqlCmd.Parameters["@newfacadehomeid"].Value = newhomeid;
                SqlCmd.Parameters["@detailids"].Value = detailIDsSelected;
                SqlCmd.Parameters["@detailoptions"].Value = detailOptionsSeleced;
                SqlCmd.Parameters["@detailprices"].Value = detailPricesSeleced;
                SqlCmd.Parameters["@date"].Value = effectivedate;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;

            }
            catch (Exception ex)
            {
                result = false;
                Trace.TraceError(ex.Message);
            }

            return result;
        }

        public bool UpdateContractType(int revisionid, string contracttype, string jobflowtype, int userid)
        {
            bool result = true;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_UpdateContractType");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                if (!string.IsNullOrWhiteSpace(contracttype))
                    SqlCmd.Parameters["@contracttype"].Value = contracttype;
                if (!string.IsNullOrWhiteSpace(jobflowtype))
                    SqlCmd.Parameters["@jobflowtype"].Value = jobflowtype;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool UpdateHomeName(int revisionid, string homename, int userid)
        {
            bool result = true;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_UpdateHomeDisplayName");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;
                SqlCmd.Parameters["@homename"].Value = homename;
                SqlCmd.Parameters["@userid"].Value = userid;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                result = true;

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public List<ValidationErrorMessage> CopyEstimateCheckDifference(string sourceEstimatenumber, string destinationEstimatenumber)
        {
            List<ValidationErrorMessage> hlist = new List<ValidationErrorMessage>();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("[sp_SalesEstimate_CopySourceEstimatetoTargetEstimate_CheckDifference]");
                SqlCmd.Parameters["@sourceEstimateNumber"].Value = sourceEstimatenumber;
                SqlCmd.Parameters["@targetEstimateNumber"].Value = destinationEstimatenumber;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow row in Sdr.Tables[0].Rows)
                {
                    ValidationErrorMessage h = new ValidationErrorMessage();
                    h.Area = row["areaname"].ToString();
                    h.Group = row["groupname"].ToString();
                    h.PossibleUpgrade = row["productname"].ToString();
                    h.ErrorMessage = row["error"].ToString();

                    hlist.Add(h);
                }

            }
            catch (Exception)
            {
                hlist = null;
            }

            return hlist;
        }

        public bool CopyEstimate(string sourceEstimatenumber, string destinationEstimatenumber)
        {
            bool result = true;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("[sp_SalesEstimate_CopySourceEstimatetoTargetEstimate]");
                SqlCmd.Parameters["@sourceEstimateNumber"].Value = sourceEstimatenumber;
                SqlCmd.Parameters["@targetEstimateNumber"].Value = destinationEstimatenumber;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public string GetHomeName(int revisionid)
        {
            string homeName = string.Empty;
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetHomeDisplayName";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionid));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            homeName = dr["HomeDisplayName"].ToString();
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return homeName;
        }

        public RevisionTypePermission CheckRevisionTypeAllowToAddNSR(int revisiontypeid)
        {
            RevisionTypePermission rtp = new RevisionTypePermission();
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_CheckRevisionTypePermission");
                SqlCmd.Parameters["@revisiontypeid"].Value = revisiontypeid;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                rtp.AllowToAddNSR = bool.Parse(Sdr.Tables[0].Rows[0]["allowtoaddNSR"].ToString());
                rtp.ValidateAccept = bool.Parse(Sdr.Tables[0].Rows[0]["validateAccept"].ToString());
                rtp.ValidateStandardInclusion = bool.Parse(Sdr.Tables[0].Rows[0]["validateStandardInclusion"].ToString());

                return rtp;

            }
            catch (Exception)
            {
                return rtp;
            }

        }

        public List<AuditLog> GetAuditLogs(int revisionid, int estimatedetailid)
        {
            List<AuditLog> auditLogs = new List<AuditLog>();

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetAuditLogs";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionid", revisionid));
                    cmd.Parameters.Add(new SqlParameter("@estimatedetailId", estimatedetailid));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            AuditLog item = new AuditLog();

                            if (dr["fkidEstimate"] != DBNull.Value)
                                item.EstimateNumber = dr["fkidEstimate"].ToString();

                            if (dr["LogID"] != DBNull.Value)
                                item.LogId = dr["LogID"].ToString();

                            if (dr["RevisionNumber"] != DBNull.Value)
                                item.RevisionNumber = dr["RevisionNumber"].ToString();

                            if (dr["RevisionType"] != DBNull.Value)
                                item.RevisionType = dr["RevisionType"].ToString();

                            if (dr["LogMessage"] != DBNull.Value)
                                item.Description = dr["LogMessage"].ToString();

                            if (dr["CreatedBy"] != DBNull.Value)
                                item.User = dr["CreatedBy"].ToString();

                            if (dr["CreatedDate"] != DBNull.Value)
                                item.LogTime = Convert.ToDateTime(dr["CreatedDate"].ToString());

                            auditLogs.Add(item);
                        }
                        if (auditLogs.Count < 1)
                        {
                            AuditLog item = new AuditLog();
                            item.Description = "There is no Audit information available for this product";
                            auditLogs.Add(item);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }

            return auditLogs;
        }

        public List<DeletedItem> GetDeletedItems(int revisionid, int resulttype)
        {
            List<DeletedItem> deletedItems = new List<DeletedItem>();

            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetDeletedItems";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionid));
                    cmd.Parameters.Add(new SqlParameter("@resulttype", resulttype));

                    conn.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            DeletedItem item = new DeletedItem();

                            if (dr["RevisionId"] != DBNull.Value)
                                item.RevisionId = Convert.ToInt32(dr["RevisionId"]);

                            if (dr["RevisionName"] != DBNull.Value)
                                item.RevisionName = dr["RevisionName"].ToString();

                            if (dr["fkidHomeDisplayOption"] != DBNull.Value)
                                item.HomeDisplayOptionId = Convert.ToInt32(dr["fkidHomeDisplayOption"]);

                            if (dr["AreaName"] != DBNull.Value)
                                item.AreaName = dr["AreaName"].ToString();

                            if (dr["GroupName"] != DBNull.Value)
                                item.GroupName = dr["GroupName"].ToString();

                            if (dr["ProductName"] != DBNull.Value)
                                item.ProductName = dr["ProductName"].ToString();

                            if (dr["ProductID"] != DBNull.Value)
                                item.ProductName += "[" + dr["ProductID"].ToString() + "]";

                            if (dr["ProductDescription"] != DBNull.Value)
                                item.ProductDescription = dr["ProductDescription"].ToString();

                            if (dr["AdditionalInfo"] != DBNull.Value)
                                item.AdditionalNotes = dr["AdditionalInfo"].ToString();
                            Decimal decQuantity = 0;
                            if (dr["Quantity"] != DBNull.Value)
                                Decimal.TryParse(dr["Quantity"].ToString(), out decQuantity);
                            item.Quantity = decQuantity;
                            Decimal decPrice = 0;
                            if (dr["ItemPrice"] != DBNull.Value)
                                Decimal.TryParse(dr["ItemPrice"].ToString(), out decPrice);
                            item.Price = decPrice;

                            Decimal totalPrice = 0;
                            if (dr["TotalPrice"] != DBNull.Value)
                                Decimal.TryParse(dr["TotalPrice"].ToString(), out totalPrice);
                            item.TotalPrice = totalPrice;
                            if (dr["Uom"] != DBNull.Value)
                                item.Uom = dr["Uom"].ToString();

                            if (dr["username"] != DBNull.Value)
                                item.DeletedBy = dr["username"].ToString();

                            if (dr["RemovedDate"] != DBNull.Value)
                                item.DeletedOn = Convert.ToDateTime(dr["RemovedDate"]).ToString("dd/MM/yyyy hh:mm tt");

                            if (dr["Reason"] != DBNull.Value)
                                item.Reason = dr["Reason"].ToString();

                            if (dr["Comment"] != DBNull.Value)
                                item.Comment = dr["Comment"].ToString();

                            if (dr["IsPromotionProduct"] != DBNull.Value)
                                item.PromotionProduct = Convert.ToBoolean(dr["IsPromotionProduct"]);

                            if (dr["IsMasterPromotion"] != DBNull.Value)
                                item.IsMasterPromotion = Convert.ToBoolean(dr["IsMasterPromotion"]);

                            deletedItems.Add(item);
                        }
                        dr.Close();
                    }

                    conn.Close();
                }
            }


            return deletedItems;
        }

        public bool ReAddDeletedEstimateItem(int sourceEstimateRevisionId, int targetEstimateRevisionId, int optionId, int userId)
        {
            bool result = true;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("[sp_SalesEstimate_ReAddDeletedEstimateItem]");
                SqlCmd.Parameters["@sourceRevisionId"].Value = sourceEstimateRevisionId;
                SqlCmd.Parameters["@targetRevisionId"].Value = targetEstimateRevisionId;
                SqlCmd.Parameters["@optionId"].Value = optionId;
                SqlCmd.Parameters["@userId"].Value = userId;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public bool ReAddDeletedMasterPromotionEstimateItem(int estimateRevisionId, int optionId, int userId)
        {
            bool result = true;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("[sp_SalesEstimate_ReAddDeletedMasterPromotionEstimateItem]");
                SqlCmd.Parameters["@estimateRevisionId"].Value = estimateRevisionId;
                SqlCmd.Parameters["@optionId"].Value = optionId;
                SqlCmd.Parameters["@userId"].Value = userId;
                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        public List<decimal> GetAreaSurcharge(int revisionid)
        {
            List<decimal> surcharge = new List<decimal>();
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetAreaSurcharge");
            SqlCmd.Parameters["@estimateRevisionId"].Value = revisionid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            surcharge.Add(Convert.ToDecimal(Sdr.Tables[0].Rows[0]["result"]));
            surcharge.Add(Convert.ToDecimal(Sdr.Tables[0].Rows[0]["surcharge"]));

            return surcharge;
        }

        public List<ItemRemoveReason> GetItemRemoveReason()
        {
            List<ItemRemoveReason> reason = new List<ItemRemoveReason>();
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetItemRemoveReason");

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                ItemRemoveReason ir = new ItemRemoveReason();
                ir.RemoveReasonID = int.Parse(dr["reasonid"].ToString());
                ir.RemoveReason = dr["deletionreason"].ToString();
                reason.Add(ir);
            }
            return reason;

        }

        public List<PAG> GetInLieuStandardPromotionItems(int estimaterevisionid, int originateoptionid)
        {
            List<PAG> itemsStdPromo = new List<PAG>();
            PAG item = new PAG("", "", "In lieu of Standard", 1, 400, 1, "", "", false, true, true, false);
            itemsStdPromo.Add(item);
            item = new PAG("", "", "In lieu of Promotion", 1, 360, 2, "", "", true, false, false, false);
            itemsStdPromo.Add(item);
            //SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetItemRemoveReason");

            //DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            //foreach (DataRow dr in Sdr.Tables[0].Rows)
            //{
            //    ItemRemoveReason ir = new ItemRemoveReason();
            //    ir.RemoveReasonID = int.Parse(dr["reasonid"].ToString());
            //    ir.RemoveReason = dr["deletionreason"].ToString();
            //    reason.Add(ir);
            //}
            return itemsStdPromo;

        }

        public List<GenericClassCodeName> GetBusinessUnits(int regionid)
        {
            List<GenericClassCodeName> itemsGeneric = new List<GenericClassCodeName>();

            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_Business Units");
            SqlCmd.Parameters["@regionId"].Value = regionid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            if (Sdr.Tables[0].Rows.Count < 2)
            {
                GenericClassCodeName ir = new GenericClassCodeName();
                ir.CodeValue = string.Empty;
                ir.CodeText = string.Empty;
                itemsGeneric.Add(ir);
            }
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                GenericClassCodeName ir = new GenericClassCodeName();
                ir.CodeValue = dr["CodeValue"].ToString();
                ir.CodeText = dr["CodeText"].ToString();
                itemsGeneric.Add(ir);
            }
            return itemsGeneric;

        }

        public string LoadStudioMQuestionForAProduct(string pproductid)
        {
            string l = "";
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetStudioMQuestionForAProduct");
            SqlCmd.Parameters["@productid"].Value = pproductid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                l = dr["studiomquestion"].ToString();
            }
            return l;
        }

        public List<SharepointDocumentType> GetSalesDocumentType()
        {
            List<SharepointDocumentType> l = new List<SharepointDocumentType>();
            SqlCommand SqlCmd = ConstructStoredProcedure("spw_SalesPaperWork_GetDocumentType");

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                SharepointDocumentType dt = new SharepointDocumentType();
                dt.DocumentCategory = dr["documentcategory"].ToString();
                dt.DocumentType = dr["documettype"].ToString();
                dt.DocumentTypeID = int.Parse(dr["id_salespaperwork_documettype"].ToString());
                l.Add(dt);

            }
            return l;
        }

        public DataSet GetContactGuidFromFullName(string fullname, string contacttype)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("spw_salesestimate_GetContactGuidFromName");
            SqlCmd.Parameters["@fullname"].Value = fullname;
            SqlCmd.Parameters["@contacttype"].Value = contacttype;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            return Sdr;
        }

        public DateTime GetDocuSignSendDate(string revisionid, string estimateid)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetDocuSignSendDate");
            SqlCmd.Parameters["@revisionid"].Value = revisionid;
            SqlCmd.Parameters["@estimateid"].Value = estimateid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            return DateTime.Parse(Sdr.Tables[0].Rows[0]["senddate"].ToString());
        }

        public List<CRMContact> GetDocuSignSigner(string accountid)
        {
            List<CRMContact> l = new List<CRMContact>();

            SqlCommand SqlCmd = ConstructStoredProcedure("spw_SQSCRM_GetAccountContactListForAccountFromCRM");
            SqlCmd.Parameters["@accountid"].Value = accountid;
            SqlCmd.Parameters["@contactid"].Value = "";
            SqlCmd.Parameters["@type"].Value = "1";

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            int i = 1;
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                CRMContact cc = new CRMContact();
                cc.FullName = dr["IVTU_NAME"].ToString();
                cc.Email = dr["EMLA_ADDRESS"].ToString();
                cc.SortOrder = i;
                i = i + 1;

                l.Add(cc);
            }


            return l;
        }

        public List<DocuSignDocStatusInfo> GetDocuSignEnvelopStatus(string revisionid, string estimateid)
        {
            List<DocuSignDocStatusInfo> lv = new List<DocuSignDocStatusInfo>();
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetDocuSignEnvelopStatus");
            SqlCmd.Parameters["@revisionid"].Value = revisionid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                DocuSignDocStatusInfo si = new DocuSignDocStatusInfo();
                si.integrationid = int.Parse(dr["integrationid"].ToString());
                si.envelopeId = dr["envelopeid"].ToString();
                si.deleted = "0";
                si.documenttype = dr["documenttype"].ToString();
                si.printtype = dr["printtype"].ToString();
                si.status = dr["status"].ToString();
                si.statusChangedDateTime = dr["statusdate"].ToString();
                si.revisionnumber = dr["revisionnumber"].ToString();
                si.estimateid = dr["estimateid"].ToString();
                si.accountid = dr["accountid"].ToString();
                si.versiontype = dr["versiontype"].ToString();
                si.Selected = false;
                si.documentnumber = dr["documentnumber"].ToString();
                if (dr["EnableSendViaDocuSign"] != null && (dr["EnableSendViaDocuSign"].ToString().ToUpper() == "TRUE" || dr["EnableSendViaDocuSign"].ToString() == "1"))
                {
                    si.EnableSendViaDocuSign = true;
                }
                else
                {
                    si.EnableSendViaDocuSign = false;
                }

                if (dr["EnableSignInPerson"] != null && (dr["EnableSignInPerson"].ToString().ToUpper() == "TRUE" || dr["EnableSignInPerson"].ToString() == "1"))
                {
                    si.EnableSignInPerson = true;
                }
                else
                {
                    si.EnableSignInPerson = false;
                }

                if (dr["EnableVoid"] != null && (dr["EnableVoid"].ToString().ToUpper() == "TRUE" || dr["EnableVoid"].ToString() == "1"))
                {
                    si.EnableVoid = true;
                }
                else
                {
                    si.EnableVoid = false;
                }

                lv.Add(si);
            }

            return lv;
        }

        public List<DocuSignHistory> GetDocuSignEnvelopeHistory(string envelopeid)
        {
            List<DocuSignHistory> lv = new List<DocuSignHistory>();
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetDocuSignEnvelopHistoy");
            SqlCmd.Parameters["@envelopeid"].Value = envelopeid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                DocuSignHistory si = new DocuSignHistory();
                si.UserName = dr["UserName"].ToString();
                si.ActionStatus = dr["ActionStatus"].ToString();
                si.ActionTime = DateTime.Parse(dr["ActionTime"].ToString());
                si.EnvelopeStatus = dr["LastStatusCode"].ToString();
                lv.Add(si);
            }

            return lv;
        }


        public List<DocuSignHistory> GetDocuSignEnvelopeHistoryByRevision(string revisionid, string versiontype, string printtype)
        {
            List<DocuSignHistory> lv = new List<DocuSignHistory>();
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetDocuSignEnvelopHistoyByRevision");
            SqlCmd.Parameters["@revisionid"].Value = revisionid;
            SqlCmd.Parameters["@versiontype"].Value = versiontype;
            SqlCmd.Parameters["@printtype"].Value = printtype;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                DocuSignHistory si = new DocuSignHistory();
                si.UserName = dr["UserName"].ToString();
                si.ActionStatus = dr["ActionStatus"].ToString();
                si.ActionTime = DateTime.Parse(dr["ActionTime"].ToString());
                si.EnvelopeStatus = dr["LastStatusCode"].ToString().Replace("&lt;", "<").Replace("&gt;",">").Replace("<br>", Environment.NewLine);
                lv.Add(si);
            }

            return lv;
        }
        public string GetStudioMQandA(int optionId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            string questions = string.Empty;

            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_GetStudioMQandA";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@optionId", optionId));

                    conn.Open();

                    object returnValue = cmd.ExecuteScalar();

                    if (returnValue != DBNull.Value)
                        questions = returnValue.ToString();

                    conn.Close();
                }
            }
            return questions;
        }

        public void RegisterEvent(string action, int revisionid, int userid)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_CreateEstimateEventRegister";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@action", action));
                    cmd.Parameters.Add(new SqlParameter("@revisionId", revisionid));
                    cmd.Parameters.Add(new SqlParameter("@userId", userid));

                    conn.Open();

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
        }

        public void UpdateEstimateDetailsDescription(
            int revisionDetailsId,
            string productDescription,
            string additionalnotes,
            string extraDescription,
            int userId)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_UpdateEstimateDetailsDescription";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionDetailsId", revisionDetailsId));
                    cmd.Parameters.Add(new SqlParameter("@productDescription", productDescription.Trim()));
                    cmd.Parameters.Add(new SqlParameter("@additionalnotes", additionalnotes.Trim()));
                    cmd.Parameters.Add(new SqlParameter("@extraDescription", extraDescription.Trim()));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
        }

        public bool MoveEstimateDetailItem(
            int revisionDetailsIdSource,
            int revisionDetailsIdTarget,
            int userId)
        {
            bool return_value = true;
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            using (SqlConnection conn = new SqlConnection(connectionstring))
            {
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SalesEstimate_MoveEstimateDetailItem";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@revisionDetailsIdSource", revisionDetailsIdSource));
                    cmd.Parameters.Add(new SqlParameter("@revisionDetailsIdTarget", revisionDetailsIdTarget));
                    cmd.Parameters.Add(new SqlParameter("@userId", userId));
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            return return_value;
        }

        public List<EstimateDetails> GetPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid)
        {
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetPromotionProductByMasterPromotionRevisionDetailsID");
            SqlCmd.Parameters["@revisiondetailsid"].Value = revisiondetailsid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            return PopulateEstimateDetailsListFromDataSet(Sdr);
        }

        public List<PromotionPAG> GetExistingPromotionProductByMasterPromotionRevisionDetailsID(string revisiondetailsid)
        {
            List<PromotionPAG> promolist = new List<PromotionPAG>();
            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetExistingPromotionProductByMasterPromotionRevisionDetailsID");
            SqlCmd.Parameters["@revisiondetailsid"].Value = revisiondetailsid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            foreach (DataRow dr in Sdr.Tables[0].Rows)
            {
                PromotionPAG details = new PromotionPAG();
                details.RevisionDetailsID = Convert.ToInt32(dr["EstimateRevisionDetailsId"]);
                details.AreaName = dr["AreaName"].ToString();
                details.GroupName = dr["GroupName"].ToString();
                details.ProductName = dr["ProductName"].ToString();
                details.Quantity = Convert.ToDecimal(dr["Quantity"]);
                details.SellPrice = Convert.ToDecimal(dr["ItemPrice"]);
                if (dr["isinmultiplepromotion"].ToString() == "1" || dr["isinmultiplepromotion"].ToString().ToUpper() == "TRUE")
                {
                    details.IsInMultiplePromotion = true;
                }
                else
                {
                    details.IsInMultiplePromotion = false;
                }

                details.MultiplePromotionName = dr["multiplepromotionname"].ToString();
                details.IconImage = dr["iconimage"].ToString();
                details.Selected = true;

                promolist.Add(details);
            }
            return promolist;
        }

        public bool DeleteMasterPromotionItem(string masterpromotionitemid, string selectedpromotionitemids, int userid)
        {
            try
            {
                List<PromotionPAG> promolist = new List<PromotionPAG>();
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_DeleteMasterPromotion");
                SqlCmd.Parameters["@masterpromotionitemid"].Value = masterpromotionitemid;
                SqlCmd.Parameters["@selectedpromotionitemids"].Value = selectedpromotionitemids;
                SqlCmd.Parameters["@userid"].Value = userid;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public List<EstimateDetails> GetEstimateDetailsByIDString(string selectedrevisiondetailsid)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
            List<EstimateDetails> estimateDetails = new List<EstimateDetails>();

            SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetEstimateDetailsByIDString");
            SqlCmd.Parameters["@idstring"].Value = selectedrevisiondetailsid;

            DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

            return PopulateEstimateDetailsListFromDataSet(Sdr);
        }
        public bool DocuSign_PushDocumentToTheProcessQueue(string revisionid, string printtype, string documenttype, int userid)
        {
            //try
            //{
            //    SqlCommand SqlCmd = ConstructStoredProcedure("sp_salesestimate_DocuSignPushDocumentToTheProcessQueue");
            //    SqlCmd.Parameters["@revisionid"].Value = revisionid;
            //    SqlCmd.Parameters["@documenttype"].Value = documenttype;
            //    SqlCmd.Parameters["@printtype"].Value = printtype;
            //    SqlCmd.Parameters["@userid"].Value = userid;

            //    DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
            //    return true;
            //}
            //catch (Exception)
            //{
            //    return false;
            //}
            return true;
        }

        public bool RemoveDocuSignDocumentFromTheProcessQueue(string integrationid)
        {
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_salesestimate_DocuSignRemoveDocumentFromTheProcessQueue");
                SqlCmd.Parameters["@integrationid"].Value = integrationid;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string ValidateSignerAndDocuemnt(string estimateid, string versionnumber, string recipientname, string recipienttype, string recipientaction)
        {
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_ValidateDocuSignRecipientActionAndDocument");
                SqlCmd.Parameters["@estimateid"].Value = estimateid;
                SqlCmd.Parameters["@versionnumber"].Value = versionnumber;
                SqlCmd.Parameters["@recipientname"].Value = recipientname;
                SqlCmd.Parameters["@recipienttype"].Value = recipienttype;
                SqlCmd.Parameters["@recipientaction"].Value = recipientaction;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                return Sdr.Tables[0].Rows[0]["validationMessage"].ToString();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public List<EstimateGridItem> SearchSpecificJob(string customernumber, string contractnumber, string SelectedSalesConsultantId, string LotNumber, string StreetName, string Suburb, string BusinessUnit)
        {
            if (customernumber==null) 
                customernumber="";
            if (contractnumber == null)
                contractnumber = "";
           
            List<EstimateGridItem> le = new List<EstimateGridItem>();

                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_SearchSpecificJob");
                SqlCmd.Parameters["@customernumber"].Value = customernumber;
                SqlCmd.Parameters["@contractnumber"].Value = contractnumber;
                SqlCmd.Parameters["@salesConsultantId"].Value = SelectedSalesConsultantId;
                SqlCmd.Parameters["@lotNumber"].Value = LotNumber;
                SqlCmd.Parameters["@streetName"].Value = StreetName;
                SqlCmd.Parameters["@suburb"].Value = Suburb;
                SqlCmd.Parameters["@businessUnit"].Value = BusinessUnit;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                foreach (DataRow dr in Sdr.Tables[0].Rows)
                {
                    EstimateGridItem estimate = new EstimateGridItem();
                    estimate.RecordId = int.Parse(dr["RecordId"].ToString());
                    estimate.CustomerName = dr["CustomerName"].ToString();
                    estimate.CustomerNumber = Convert.ToInt32(dr["CustomerNumber"]);
                    estimate.ContractNumber = Convert.ToInt32(dr["ContractNumber"]);
                    estimate.EstimateId = Convert.ToInt32(dr["EstimateNumber"]);
                    estimate.HomeName = dr["HomeName"].ToString();
                    estimate.SalesConsultantName = dr["SalesConsultantName"].ToString();

                    estimate.RevisionDetails = dr["RevisionNumber"].ToString();

                    estimate.OwnerName = dr["OwnerName"].ToString();
                    estimate.MRSGroup = dr["region"].ToString();
                    estimate.RegionID = Convert.ToInt32(dr["RegionID"]);
                    estimate.JobLocation = dr["location"].ToString();
                    estimate.RevisionTypeId = int.Parse(dr["RevisionTypeID"].ToString());
                    estimate.RevisionTypeCode = dr["RevisionTypeCode"].ToString();
                    estimate.PreviousRevisionId = int.Parse(dr["PreviousRevisionId"].ToString());
                    estimate.AllowToResetCurrentMilestone = dr["allowtoresetcurrentmilestone"].ToString() == "0" || dr["allowtoresetcurrentmilestone"].ToString() == "false" ? false : true;
                    estimate.AllowUndoSetContract = bool.Parse(dr["allowtoundosetcontract"].ToString());

                    estimate.JobFlowType = dr["JobFlowType"].ToString();
                    estimate.ContractType = dr["ContractType"].ToString();
                    estimate.ContractStatusName = dr["ContractStatus"].ToString();
                    //if (dr["ContractStatus"] != DBNull.Value)
                    //{
                    //    switch (dr["ContractStatus"].ToString())
                    //    {
                    //        case "1":
                    //            estimate.ContractStatusName = "Pending";
                    //            break;
                    //        case "2":
                    //            estimate.ContractStatusName = "Cancelled";
                    //            break;
                    //        case "3":
                    //            estimate.ContractStatusName = "Work In Progress";
                    //            break;
                    //        case "4":
                    //            estimate.ContractStatusName = "On Hold";
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //}
                    estimate.BusinessUnit = dr["BusinessUnit"].ToString();

                    if (estimate.JobLocation == "SQS")
                        estimate.RecordType = "SQS";
                    else if (estimate.PreviousRevisionId > 0)
                        estimate.RecordType = "Queue";
                    else
                        estimate.RecordType = "EstimateHeader";

                    le.Add(estimate);
                }

            return le;
        }


        public void GetContractEventFromRevisonID(string revisionid, out string contractnumber, out string eventcode)
        {
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetContractEventFromRevisonID");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                if (Sdr.Tables[0].Rows.Count > 0)
                {
                    contractnumber = Sdr.Tables[0].Rows[0]["contractnumber"].ToString();
                    eventcode = Sdr.Tables[0].Rows[0]["eventcode"].ToString(); 
                }
                else
                {
                    contractnumber="";
                    eventcode = "";
                }
                 
            }
            catch (Exception ex)
            {
                contractnumber = "";
                eventcode = "";                
            }
        }

        public bool ApplyRounding(int revisionid)
        {
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_ApplyRoundingToRevision");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public int GetUserIDFromUserCode(string usercode)
        {
            int userid = 0;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetUserIDFromUserCode");
                SqlCmd.Parameters["@usercode"].Value = usercode;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                if (Sdr.Tables[0].Rows.Count > 0)
                {
                    userid = int.Parse(Sdr.Tables[0].Rows[0]["userid"].ToString());
                }

            }
            catch (Exception ex)
            {
                userid = 0;
            }

            return userid;
        }
        public int ResetEditEstimateUserID(int estimateRevisionId, int editEstimateUserID)
        {
            int return_value = 0;
            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_EditEstimateResetUserEditLock");
                SqlCmd.Parameters["@estimateRevisionId"].Value = estimateRevisionId;
                SqlCmd.Parameters["@editEstimateUserID"].Value = editEstimateUserID;

                ExcuteSqlStoredProcedure(SqlCmd);

                return_value = 1;
            }
            catch (Exception ex)
            {
                return_value = 0;
            }

            return return_value;
        }

        #region sql help fucntion
        public SqlCommand ConstructStoredProcedure(string SP)
        {
            string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();
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
        public EstimateDisclaimerUpdateDetail GetEstimateDisclaimerUpdateDetails(int revisionid)
        {
            EstimateDisclaimerUpdateDetail disclaimerDetail = new EstimateDisclaimerUpdateDetail();

            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_GetDisclaimerUpdateDetails");
                SqlCmd.Parameters["@revisionid"].Value = revisionid;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                if (Sdr.Tables[0].Rows.Count > 0)
                {
                    disclaimerDetail.DisclaimerCurrentId = int.Parse(Sdr.Tables[0].Rows[0]["disclaimerCurrentId"].ToString());
                    disclaimerDetail.DisclaimerNewId = int.Parse(Sdr.Tables[0].Rows[0]["disclaimerNewId"].ToString());
                    disclaimerDetail.DisclaimerVariationCurrentId = int.Parse(Sdr.Tables[0].Rows[0]["disclaimerVariationCurrentId"].ToString());
                    disclaimerDetail.DisclaimerVariationNewId = int.Parse(Sdr.Tables[0].Rows[0]["disclaimerVariationNewId"].ToString());
                }

            }
            catch (Exception ex)
            {
                // handle erros
            }

            return disclaimerDetail;
        }

        public bool SaveDisclaimerUpdateDetails(int revisionId, int typeId, int disclaimerNewId, int userId)
        {
            bool return_value = true;

            try
            {
                SqlCommand SqlCmd = ConstructStoredProcedure("sp_SalesEstimate_SaveDisclaimerUpdateDetails");
                SqlCmd.Parameters["@revisionid"].Value = revisionId;
                SqlCmd.Parameters["@typeId"].Value = typeId;
                SqlCmd.Parameters["@disclaimerNewId"].Value = disclaimerNewId;
                SqlCmd.Parameters["@modifiedbyId"].Value = userId;

                DataSet Sdr = ExcuteSqlStoredProcedure(SqlCmd);

                if (Sdr.Tables[0].Rows.Count > 0)
                {
                }

            }
            catch (Exception ex)
            {
                // handle erros
                return_value = false;
            }

            return return_value;
        }

        //public List<District> GetDistrictList()
        //{
        //    List<District> return_value = new List<District>();
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

        //    using (SqlConnection conn = new SqlConnection(connectionstring))
        //    {
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "Select * from District";
        //            cmd.CommandType = CommandType.Text;
        //            conn.Open();

        //            using (SqlDataReader dr = cmd.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    District dist = new District();
        //                    dist.DistrictCode = dr["dis_bk_cde"].ToString();
        //                    dist.DistrictName = dr["dis_dsc"].ToString();
        //                    return_value.Add(dist);
        //                }

        //                dr.Close();
        //            }

        //            conn.Close();
        //        }
        //    }

        //    return return_value;
        //}

        //public List<OperatingCenter> GetOperatingCenterList()
        //{
        //    List<OperatingCenter> return_value = new List<OperatingCenter>();
        //    string connectionstring = ConfigurationManager.ConnectionStrings["SqlConnection"].ToString();

        //    using (SqlConnection conn = new SqlConnection(connectionstring))
        //    {
        //        using (SqlCommand cmd = conn.CreateCommand())
        //        {
        //            cmd.CommandText = "Select * from Operating_Centre";
        //            cmd.CommandType = CommandType.Text;
        //            conn.Open();

        //            using (SqlDataReader dr = cmd.ExecuteReader())
        //            {
        //                while (dr.Read())
        //                {
        //                    OperatingCenter opsCenter = new OperatingCenter();
        //                    opsCenter.OpsCenterCode = dr["op_cent_bk_cde"].ToString();
        //                    opsCenter.OpsCenterName = dr["op_cent_nme"].ToString();
        //                    return_value.Add(opsCenter);
        //                }

        //                dr.Close();
        //            }

        //            conn.Close();
        //        }
        //    }

        //    return return_value;
        //}
        #endregion

    }
}
