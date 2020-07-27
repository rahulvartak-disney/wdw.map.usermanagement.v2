using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace Wdw.UserManagement.v2.Code
{
    public class BusinessLayer
    {
        #region => Log Message <=
        public static void LogMessage(Exception ex, string component)
        {
            try
            {
                SPWeb oWeb = SPContext.Current.Web;
                SPList oList = oWeb.Lists.TryGetList(Constants.ExceptionLogs.NAME);
                if (null != oList)
                {
                    oWeb.AllowUnsafeUpdates = true;
                    SPListItem newLog = oList.Items.Add();
                    newLog[Constants.ExceptionLogs.COL_COMPONENT] = component;
                    newLog[Constants.ExceptionLogs.COL_DETAILS] = string.Format("Message -> {0} *** StackTrace -> {1} *** ", ex.Message, ex.StackTrace);
                    newLog.Update();
                    oWeb.AllowUnsafeUpdates = false;
                }
            }
            catch (Exception e) { }
        }
        #endregion

        #region => Get String from database object <=
        public static string GetString(object obj)
        {
            if (obj.GetType().FullName == "System.DBNull")
                return string.Empty;
            else if (null == obj)
                return string.Empty;
            else
                return Convert.ToString(obj);
        }
        #endregion

        public static Int32 GetInteger(object obj)
        {
            if (obj.GetType().FullName == "System.DBNull")
                return -1;
            else
            {
                try
                {
                    return Convert.ToInt32(obj);
                }
                catch (SystemException ex)
                {
                    LogMessage(ex, "BusinessLayer.GetInteger");
                    return -1;
                }
            }
        }

        public static void UncheckAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Checked = false;
                CheckChildren(node, false);
            }
        }

        private static void CheckChildren(TreeNode rootNode, bool isChecked)
        {
            foreach (TreeNode node in rootNode.ChildNodes)
            {
                CheckChildren(node, isChecked);
                node.Checked = isChecked;
            }
        }

        public static void SortDropdown(DropDownList ddlToBeSorted)
        {
            string selectedValue = ddlToBeSorted.SelectedValue;

            List<ListItem> list = new List<ListItem>();
            foreach (ListItem li in ddlToBeSorted.Items) list.Add(li);
            
            //sort list items alphabetically/ascending
            List<ListItem> sorted = list.OrderBy(b => Convert.ToInt32(b.Value)).ToList();

            //empty dropdownlist
            ddlToBeSorted.Items.Clear();

            //repopulate dropdownlist with sorted items.
            foreach (ListItem li in sorted) ddlToBeSorted.Items.Add(li);

            ddlToBeSorted.SelectedValue = selectedValue;
        }

        public static SPFieldUserValue GetSelectedUser(PeopleEditor pEditor)
        {
            SPFieldUserValue user = new SPFieldUserValue();
            try
            {
                string[] userarray = pEditor.CommaSeparatedAccounts.ToString().Split(',');
                if(userarray.Length > 0 && Convert.ToString(userarray[0]).Length > 0)
                {
                    return Convert2Account(userarray[0]);
                }
            }
            catch(Exception ex)
            {
                LogMessage(ex, "BusinessLayer.GetSelectedUser");
            }
            return user;
        }

        public static SPFieldUserValue Convert2Account(string userId)
        {
            SPFieldUserValue uservalue = null;
            try
            {
                using (SPSite site = new SPSite(SPContext.Current.Site.Url))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPUser requireduser = web.EnsureUser(userId);
                        uservalue = new SPFieldUserValue(web, requireduser.ID, requireduser.LoginName);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex, string.Format("Unable to resolve the user with ID {0} in Methods.Convert2Account", userId));
            }
            return uservalue;
        }

        public static void ReorderAlphabetized(DropDownList ddl)
        {
            List<ListItem> listCopy = new List<ListItem>();
            foreach (ListItem item in ddl.Items)
                listCopy.Add(item);
            ddl.Items.Clear();
            foreach (ListItem item in listCopy.OrderBy(item => item.Text))
                ddl.Items.Add(item);
        }
    }
}
