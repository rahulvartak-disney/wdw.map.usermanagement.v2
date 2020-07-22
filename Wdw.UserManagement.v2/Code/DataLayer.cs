using Microsoft.SharePoint;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Wdw.UserManagement.v2.Code
{
    public class DataLayer
    {
        internal static DataTable getExistingUsers()
        {
            DataTable dtUsers = new DataTable();            
            SqlConnection conn = new SqlConnection();
            try
            {
                conn.ConnectionString = Get_Connection_String();
                SqlCommand cmd = new SqlCommand("USP_SP_UM_GET_EXISTING_USERS", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter oAdapter = new SqlDataAdapter(cmd);
                oAdapter.Fill(dtUsers);
            }
            catch (Exception ex)
            {
                BusinessLayer.LogMessage(ex, "DataLayer.getExistingUsers");
            }
            finally
            {
                if (null != conn) conn.Close();
            }

            DataRow newRow = dtUsers.NewRow();
            newRow[Constants.Users.USER_LOGIN] = "-1";
            newRow[Constants.Users.USER_NAME] = "Choose User";
            dtUsers.Rows.InsertAt(newRow, 0);
            dtUsers.AcceptChanges();

            return dtUsers; 
        }        

        internal static DataSet  getGroupsAndDepts()
        {
            DataSet dsResult = new DataSet();
            SqlConnection conn = new SqlConnection();
            try
            {
                conn.ConnectionString = Get_Connection_String();
                SqlCommand cmd = new SqlCommand("USP_SP_UM_GET_DEPT_HIER", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter oAdapter = new SqlDataAdapter(cmd);
                oAdapter.Fill(dsResult);
            }
            catch (Exception ex)
            {
                BusinessLayer.LogMessage(ex, "DataLayer.getGroupsAndDepts");
            }
            finally
            {
                if (null != conn) conn.Close();
            }
            return dsResult;
        }

        internal static DataTable getMAPModules()
        {
            DataTable dtModules = new DataTable();
            SqlConnection conn = new SqlConnection();
            try
            {
                conn.ConnectionString = Get_Connection_String();
                SqlCommand cmd = new SqlCommand("USP_SP_UM_GET_ALL_MODULES", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter oAdapter = new SqlDataAdapter(cmd);
                oAdapter.Fill(dtModules);
            }
            catch (Exception ex)
            {
                BusinessLayer.LogMessage(ex, "DataLayer.getMAPModules");
            }
            finally
            {
                if (null != conn) conn.Close();
            }
            return dtModules;
        }

        internal static DataSet getUserAllocations(string userLogin)
        {
            DataSet dsResult = new DataSet();
            SqlConnection conn = new SqlConnection();
            try
            {
                conn.ConnectionString = Get_Connection_String();
                SqlCommand cmd = new SqlCommand("USP_SP_UM_GET_USER_ALLOCATIONS", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@userLogin", SqlDbType.VarChar)).Value = userLogin;
                SqlDataAdapter oAdapter = new SqlDataAdapter(cmd);
                oAdapter.Fill(dsResult);
            }
            catch (Exception ex)
            {
                BusinessLayer.LogMessage(ex, "DataLayer.getUserAllocations");
            }
            finally
            {
                if (null != conn) conn.Close();
            }
            return dsResult;
        }

        #region => Get connection string <=

        public static string Get_Connection_String()
        {
            SPWeb oWeb = SPContext.Current.Web;
            SPList oList = oWeb.Lists.TryGetList(Constants.Configuration.NAME);
            if (null != oList)
            {
                SPQuery oQuery = new SPQuery();
                oQuery.Query = Constants.Configuration.GET_ALL_CONNSTR;
                oQuery.ViewFields = Constants.Configuration.VF_CONNSTR;
                SPListItemCollection items = oList.GetItems(oQuery);
                if (null != items && items.Count > 0)                
                    return Convert.ToString(items[0][Constants.Configuration.FLD_VALUE]);                
            }
            return string.Empty;
        }

        #endregion

        internal static int UpdateUserAllocations(string userLogin, string selectedModules, string selectedDepts, Int32 defaultDept, string userName, string userEmail)
        {
            int result = -1;
            SqlConnection conn = new SqlConnection();
            try
            {
                conn.ConnectionString = Get_Connection_String();
                conn.Open();
                SqlCommand cmd = new SqlCommand("USP_SP_UM_UPDATE_USER_ALLOCATIONS", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@userLogin", SqlDbType.VarChar)).Value = userLogin;
                cmd.Parameters.Add(new SqlParameter("@userModules", SqlDbType.VarChar)).Value = selectedModules;
                cmd.Parameters.Add(new SqlParameter("@userDepts", SqlDbType.VarChar)).Value = selectedDepts;
                cmd.Parameters.Add(new SqlParameter("@defaultDept", SqlDbType.Int)).Value = defaultDept;
                cmd.Parameters.Add(new SqlParameter("@userName", SqlDbType.VarChar)).Value = userName;
                cmd.Parameters.Add(new SqlParameter("@userEmail", SqlDbType.VarChar)).Value = userEmail;
                result = cmd.ExecuteNonQuery();                
                result = 1;
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "DataLayer.UpdateUserAllocations");
            }
            finally
            {
                if (null != conn) conn.Close();
            }
            return result;
        }

        internal static int DeleteUserAllocations(string userLogin)
        {
            int result = -1;
            SqlConnection conn = new SqlConnection();
            try
            {
                conn.ConnectionString = Get_Connection_String();
                conn.Open();
                SqlCommand cmd = new SqlCommand("USP_DeleteUsers_AllTables", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.VarChar)).Value = userLogin;                
                result = cmd.ExecuteNonQuery();                
                result = 1;
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "DataLayer.DeleteUserAllocations");
            }
            finally
            {
                if (null != conn) conn.Close();
            }
            return result;
        }
    }
}
