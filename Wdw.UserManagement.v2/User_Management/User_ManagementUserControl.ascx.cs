using Microsoft.SharePoint;
using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Wdw.UserManagement.v2.Code;

namespace Wdw.UserManagement.v2.User_Management
{
    public partial class User_ManagementUserControl : UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Load_UserNames();
                    Load_MAPModules();
                    Load_Selected_Departments("onload");
                    Populate_DeptTreeView(); 
                }
                else
                {                   
                        string newUser = txtNewUserName.Text;
                        if (newUser.Length > 0)
                        {
                            ddlSelectUser.ClearSelection();
                            if (!ddlSelectUser.Items.Contains(new ListItem(newUser, "-2")))
                                ddlSelectUser.Items.Add(new ListItem(newUser, "-2"));
                            ddlSelectUser.SelectedValue = "-2";
                            ddlSelectUser.Enabled = false;
                            btnNewUser.Enabled = false;
                        }                    
                }
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.Page_Load");
            }            
        }

        private void Populate_DeptTreeView()
        {
            try
            {
                DataTable dtparent = new DataTable();
                DataTable dtchild = new DataTable();
                DataSet dsFromSQL = new DataSet();

                dsFromSQL = DataLayer.getGroupsAndDepts();
                if(dsFromSQL.Tables.Count > 1)
                {
                    dtparent = dsFromSQL.Tables[0];
                    dtparent.TableName = "A";
                    dtchild = dsFromSQL.Tables[1];
                    dtchild.TableName = "B";
                    dsFromSQL.Relations.Add("children", dtparent.Columns[Constants.Group.GROUP_NAME], dtchild.Columns[Constants.Group.GROUP_NAME]);

                    if (dsFromSQL.Tables[0].Rows.Count > 0)
                    {
                        tvDepts.Nodes.Clear();
                        foreach (DataRow masterRow in dsFromSQL.Tables[0].Rows)
                        {
                            TreeNode masterNode = new TreeNode((string)masterRow[Constants.Group.GROUP_NAME], Convert.ToString(masterRow[Constants.Group.GROUP_NAME]));
                            tvDepts.Nodes.Add(masterNode);
                            tvDepts.CollapseAll();
                            foreach (DataRow childRow in masterRow.GetChildRows("Children"))
                            {
                                TreeNode childNode = new TreeNode((string)childRow[Constants.Department.DEPT_NAME], Convert.ToString(childRow[Constants.Department.DEPT_NBR]));
                                masterNode.ChildNodes.Add(childNode);
                                childNode.Value = Convert.ToString(childRow[Constants.Department.DEPT_NBR]);
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.Populate_DeptTreeView");
            }            
        }

        private void Load_Selected_Departments(string eventName)
        {
            if (eventName.Equals("onload"))
            {
                DataTable dtSelectedDepts = new DataTable();

                DataColumn newColumn = new DataColumn(Constants.Department.DEPT_NAME);
                dtSelectedDepts.Columns.Add(newColumn);
                newColumn = new DataColumn(Constants.Department.DEPT_NBR);
                dtSelectedDepts.Columns.Add(newColumn);
                dtSelectedDepts.AcceptChanges();

                DataRow newRow = dtSelectedDepts.NewRow();
                newRow[Constants.Department.DEPT_NAME] = "No Department Selected";
                newRow[Constants.Department.DEPT_NBR] = -1;
                dtSelectedDepts.Rows.Add(newRow);
                dtSelectedDepts.AcceptChanges();

                ddlDefaultDept.DataSource = dtSelectedDepts;
                ddlDefaultDept.DataTextField = Constants.Department.DEPT_NAME;
                ddlDefaultDept.DataValueField = Constants.Department.DEPT_NBR;
                ddlDefaultDept.DataBind();

            }
        }

        private void Load_MAPModules()
        {
            DataTable dtModules = DataLayer.getMAPModules();
            if(dtModules.Rows.Count > 0)
            {
                chklstModules.DataTextField = Constants.Modules.NAME;
                chklstModules.DataValueField = Constants.Modules.ID;
                chklstModules.DataSource = dtModules;
                chklstModules.DataBind();
            }
        }

        private void Load_UserNames()
        {
            DataTable dtUsers = DataLayer.getExistingUsers();
            if (dtUsers.Rows.Count > 0)
            {
                ddlSelectUser.DataTextField = Constants.Users.USER_NAME;
                ddlSelectUser.DataValueField = Constants.Users.USER_LOGIN;
                ddlSelectUser.DataSource = dtUsers;
                ddlSelectUser.DataBind();
            }
        }        

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                string userLogin = ddlSelectUser.SelectedValue;
                if ((userLogin == "-1" || userLogin == "-2"))
                {
                    // Only existing users can be deleted
                }
                else
                {
                    if(userLogin.Length > 0)
                    {
                        string userName = ddlSelectUser.SelectedItem.Text;
                        int updatedSuccessFlag = DataLayer.DeleteUserAllocations(userLogin);
                        if (updatedSuccessFlag > 0)
                        {
                            lblMessage.Text = string.Format("The user {0} was succesfully deleted. ", userName);
                            lblMessage.ForeColor = Color.Green;
                            tblMain.Visible = false;
                            lblMessage.Visible = true;
                            btnReset.Visible = true;
                        }
                        else
                        {
                            lblMessage.Text = string.Format("Some error occured during deletion of the user {0}. Please contact the site administrator.", userName);
                            lblMessage.ForeColor = Color.Red;
                            tblMain.Visible = false;
                            lblMessage.Visible = true;
                            btnReset.Visible = true;
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.btnDelete_Click");
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl);
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string userLogin = ddlSelectUser.SelectedValue;
                string userEmail = string.Empty;
                string userName = string.Empty;
                if((userLogin == "-1" || userLogin == "-2") && ppNewUser.Entities.Count > 0)
                {
                    SPFieldUserValue user = BusinessLayer.GetSelectedUser(ppNewUser);
                    userLogin = user.User.LoginName;
                    userLogin = userLogin.Substring(userLogin.LastIndexOf("\\") + 1);
                    userEmail = user.User.Email;
                    userName = user.User.Name;
                }
                else
                {
                    SPFieldUserValue user = BusinessLayer.Convert2Account(userLogin);
                    userEmail = user.User.Email;
                    userName = ddlSelectUser.SelectedItem.Text;
                }

                string selectedModules = string.Empty;
                bool firstModule = true;
                foreach(ListItem oItem in chklstModules.Items)
                {
                    if (oItem.Selected)
                    {
                        if (firstModule) { selectedModules = oItem.Text; firstModule = false; }
                        else   selectedModules = (selectedModules + "," + oItem.Text);                          
                    }
                }

                Int32 defaultDept = BusinessLayer.GetInteger(ddlDefaultDept.SelectedValue);

                string selectedDepts = string.Empty;
                bool firstDept = true;

                if (chkSelectAll.Checked)
                {
                    selectedDepts = "0";
                    //defaultDept = 0;
                }
                    
                else
                {
                    foreach (TreeNode oNode in tvDepts.Nodes)
                    {
                        foreach (TreeNode childNode in oNode.ChildNodes)
                        {
                            if (childNode.Checked)
                            {
                                if (firstDept) { selectedDepts = childNode.Value; firstDept = false; }
                                else selectedDepts = (selectedDepts + "," + childNode.Value);
                            }
                        }
                    }
                }
                                

                int updatedSuccessFlag = DataLayer.UpdateUserAllocations(userLogin, selectedModules, selectedDepts, defaultDept, userName, userEmail);
                if(updatedSuccessFlag > 0)
                {
                    lblMessage.Text = string.Format("The user {0} was succesfully updated. ", userName);
                    lblMessage.ForeColor = Color.Green;
                    tblMain.Visible = false;
                    lblMessage.Visible = true;
                    btnReset.Visible = true;
                }
                else
                {
                    lblMessage.Text = string.Format("Some error occured during updation of the user {0}. Please contact the site administrator.", userName);
                    lblMessage.ForeColor = Color.Red;
                    tblMain.Visible = false;
                    lblMessage.Visible = true;
                    btnReset.Visible = true;
                }
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "DataLayer.btnSubmit_Click");
            }
        }

        protected void ddlSelectUser_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                chklstModules.ClearSelection();
                DataSet dsResult = DataLayer.getUserAllocations(ddlSelectUser.SelectedValue);
                if (dsResult.Tables.Count > 2)
                {
                    // Set selected Modules
                    foreach (DataRow oRow in dsResult.Tables[1].Rows) 
                        foreach(ListItem oItem in chklstModules.Items)                        
                            if (oItem.Text.Equals(BusinessLayer.GetString(oRow[Constants.Modules.NAME]), StringComparison.InvariantCultureIgnoreCase))
                                oItem.Selected = true;

                    chkSelectAll.Checked = false;
                    BusinessLayer.UncheckAllNodes(tvDepts.Nodes);


                    foreach(DataRow oRow in dsResult.Tables[0].Rows)
                    {
                        string deptName = BusinessLayer.GetString(oRow[Constants.Department.DEPT_NAME]);
                        string deptNbr = BusinessLayer.GetString(oRow[Constants.Department.DEPT_NBR]);
                        string deptNbrName = BusinessLayer.GetString(oRow[Constants.Department.DEPT_NBR_NAME]);
                        if(deptNbr.Equals("0"))
                        {
                            chkSelectAll.Checked = true;
                            foreach(TreeNode oNode in tvDepts.Nodes)
                            {
                                oNode.Checked = true;
                                CheckUncheckChildItems(oNode, chkSelectAll.Checked);
                            }                            
                            break;
                        }
                        else
                        {
                            string childPath = string.Format("{0} /{1}", BusinessLayer.GetString(oRow[Constants.Group.GROUP_NAME]), deptNbr); 
                            if (deptName.Length > 0 && deptNbr.Length > 0)
                            {
                                TreeNode toBeSelected = tvDepts.FindNode(childPath);
                                if (null != toBeSelected)
                                {
                                    toBeSelected.Checked = true;
                                    toBeSelected.Expanded = true;
                                    toBeSelected.Parent.Expanded = true;

                                    if (!ddlDefaultDept.Items.Contains(new ListItem(toBeSelected.Text, toBeSelected.Value)))
                                    {
                                        ddlDefaultDept.Items.Add(new ListItem(toBeSelected.Text, toBeSelected.Value));
                                        if (ddlDefaultDept.Items.Contains(new ListItem("No Department Selected", "-1")))
                                        {
                                            ddlDefaultDept.Items.Remove(new ListItem("No Department Selected", "-1"));
                                        }
                                    }
                                }
                            }
                        }                       
                    }

                    CheckUncheckParentNodes();
                    BusinessLayer.SortDropdown(ddlDefaultDept);

                    string defaultDeptName =  BusinessLayer.GetString( dsResult.Tables[2].Rows[0][Constants.Users.DEFAULT_DEPT_NAME]);
                    string defaultDeptNbr = BusinessLayer.GetString(dsResult.Tables[2].Rows[0][Constants.Users.DEFAULT_DEPT_NBR]);
                    ddlDefaultDept.ClearSelection();
                    if(ddlDefaultDept.Items.Contains(new ListItem(defaultDeptName, defaultDeptNbr)))                    
                        ddlDefaultDept.Items.FindByValue(defaultDeptNbr).Selected = true;                    
                }
                btnNewUser.Enabled = false;
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.ddlSelectUser_SelectedIndexChanged");
            }
        }        

        private void UncheckSelectAll()
        {
            foreach (TreeNode oNode in tvDepts.Nodes)
            {
                if(!oNode.Checked)
                {
                    chkSelectAll.Checked = false;
                    return;
                }
                foreach(TreeNode cNode in oNode.ChildNodes)
                {
                    if (!cNode.Checked)
                    {
                        chkSelectAll.Checked = false;
                        return;
                    }
                }
            }
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            Response.Redirect(Request.RawUrl);
        }

        protected void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {                
                //foreach (TreeNode oNode in tvDepts.Nodes)
                //{
                //    oNode.Checked = chkSelectAll.Checked;
                //    CheckUncheckChildItems(oNode, chkSelectAll.Checked);
                //}
                // BusinessLayer.SortDropdown(ddlDefaultDept);
            }
            catch (Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.chkSelectAll_CheckedChanged");                
            }
        }

        private void CheckUncheckChildItems(TreeNode oNode, bool toBeChecked)
        {
            foreach (TreeNode childNode in oNode.ChildNodes)
            {
                childNode.Checked = toBeChecked;
                if (toBeChecked)
                {
                    if (!ddlDefaultDept.Items.Contains(new ListItem(childNode.Text, childNode.Value)))
                    ddlDefaultDept.Items.Add(new ListItem(childNode.Text, childNode.Value));
                    if (childNode.Value != "-1" && ddlDefaultDept.Items.Contains(new ListItem("No Department Selected", "-1")))
                    {
                        ddlDefaultDept.Items.Remove(new ListItem("No Department Selected", "-1"));
                    }
                }
                else
                {
                    ddlDefaultDept.Items.Remove(new ListItem(childNode.Text, childNode.Value));
                    if (ddlDefaultDept.Items.Count == 0)
                    {
                        ddlDefaultDept.Items.Add(new ListItem("No Department Selected", "-1"));
                    }
                }
            }
        }

        protected void btnHiddenPostBack_Click(object sender, EventArgs e)
        {
            try
            {
                //TreeNode nodeChecked = new TreeNode();
                //bool nodeFound = false;
                //foreach(TreeNode pNode in tvDepts.Nodes)
                //{
                //    if(pNode.Text.Equals(txtCheckedDeptText.Text) && pNode.Value.Equals(txtCheckedDeptValue.Text))
                //    {
                //        nodeChecked = pNode;
                //        nodeFound = true;
                //        break;
                //    }
                //    foreach(TreeNode cNode in pNode.ChildNodes)
                //    {
                //        if (cNode.Text.Equals(txtCheckedDeptText.Text) && cNode.Value.Equals(txtCheckedDeptValue.Text))
                //        {
                //            nodeChecked = cNode;
                //            nodeFound = true;
                //            break;
                //        }
                //    }
                //    if (nodeFound)
                //        break;

                //}

                //if (nodeFound)
                //{
                //    if (nodeChecked.Checked && nodeChecked.Depth > 0)
                //    {
                //        if (!ddlDefaultDept.Items.Contains(new ListItem(nodeChecked.Text, nodeChecked.Value)))
                //            ddlDefaultDept.Items.Add(new ListItem(nodeChecked.Text, nodeChecked.Value));
                //        if (nodeChecked.Value != "-1" && ddlDefaultDept.Items.Contains(new ListItem("No Department Selected", "-1")))
                //        {
                //            ddlDefaultDept.Items.Remove(new ListItem("No Department Selected", "-1"));
                //        }
                //    }
                //    else if (!nodeChecked.Checked && nodeChecked.Depth > 0)
                //    {
                //        ddlDefaultDept.Items.Remove(new ListItem(nodeChecked.Text, nodeChecked.Value));
                //        if (ddlDefaultDept.Items.Count == 0)
                //        {
                //            ddlDefaultDept.Items.Add(new ListItem("No Department Selected", "-1"));
                //        }
                //    }
                //    BusinessLayer.SortDropdown(ddlDefaultDept);
                //    UncheckSelectAll();
                //} 
                
                UpdateDeptSelections();

            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.btnHiddenPostBack_Click");
            }
        }

        protected void btnHiddenNewUserPostBack_Click(object sender, EventArgs e)
        {
            try
            {
                btnNewUser.Enabled = false;
            }
            catch(Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.btnHiddenNewUserPostBack_Click");
            }
        }

        protected void btnHiddenSelectAllPostBack_Click(object sender, EventArgs e)
        {
            try
            {
                BusinessLayer.LogMessage(new Exception("testing"), "testing");
                foreach (TreeNode oNode in tvDepts.Nodes)
                {
                    oNode.Checked = chkSelectAll.Checked;
                    CheckUncheckChildItems(oNode, chkSelectAll.Checked);
                }
                // BusinessLayer.SortDropdown(ddlDefaultDept);
            }
            catch (Exception ex)
            {
                BusinessLayer.LogMessage(ex, "User_ManagementUserControl.btnHiddenSelectAllPostBack_Click");
            }
        }

        protected void UpdateDeptSelections()
        {
            try
            {
                string selectedDefaultDept = ddlDefaultDept.SelectedValue;

                foreach (TreeNode parentNode in tvDepts.Nodes)
                {
                    if (parentNode.Checked && parentNode.Value.Equals(txtCheckedDeptValue.Text))
                    {
                        foreach (TreeNode childNode in parentNode.ChildNodes)
                        {
                            if (!childNode.Checked) 
                            {
                                childNode.Checked = true;
                                if (!ddlDefaultDept.Items.Contains(new ListItem(childNode.Text, childNode.Value)))
                                    ddlDefaultDept.Items.Add(new ListItem(childNode.Text, childNode.Value));
                                if (childNode.Value != "-1" && ddlDefaultDept.Items.Contains(new ListItem("No Department Selected", "-1")))
                                {
                                    ddlDefaultDept.Items.Remove(new ListItem("No Department Selected", "-1"));
                                }
                            }
                        }                           
                    }
                    else if (!parentNode.Checked && parentNode.Value.Equals(txtCheckedDeptValue.Text))
                    {
                        foreach (TreeNode childNode in parentNode.ChildNodes)
                        {
                            if (childNode.Checked)
                            {
                                childNode.Checked = false;
                                ddlDefaultDept.Items.Remove(new ListItem(childNode.Text, childNode.Value));
                                if (ddlDefaultDept.Items.Count == 0)
                                {
                                    ddlDefaultDept.Items.Add(new ListItem("No Department Selected", "-1"));
                                }
                            }                            
                        }
                    }
                    else
                    {
                        foreach (TreeNode childNode in parentNode.ChildNodes)
                        {
                            if (childNode.Checked && childNode.Value.Equals(txtCheckedDeptValue.Text))
                            {
                                if (!ddlDefaultDept.Items.Contains(new ListItem(childNode.Text, childNode.Value)))
                                    ddlDefaultDept.Items.Add(new ListItem(childNode.Text, childNode.Value));
                                if (childNode.Value != "-1" && ddlDefaultDept.Items.Contains(new ListItem("No Department Selected", "-1")))
                                {
                                    ddlDefaultDept.Items.Remove(new ListItem("No Department Selected", "-1"));
                                }
                            }
                            else if(!childNode.Checked && childNode.Value.Equals(txtCheckedDeptValue.Text))
                            {
                                ddlDefaultDept.Items.Remove(new ListItem(childNode.Text, childNode.Value));
                                if (ddlDefaultDept.Items.Count == 0)
                                {
                                    ddlDefaultDept.Items.Add(new ListItem("No Department Selected", "-1"));
                                }
                            }
                        }
                    }                   
                }

                BusinessLayer.SortDropdown(ddlDefaultDept);

                ListItem selectedItem = ddlDefaultDept.Items.FindByValue(selectedDefaultDept);
                if(null != selectedItem)
                {
                    ddlDefaultDept.ClearSelection();
                    selectedItem.Selected = true;
                }


            }
            catch (Exception ex)
            {
                BusinessLayer.LogMessage(ex, "");
            }
        }

        protected void CheckUncheckParentNodes()
        {
            foreach(TreeNode pNode in tvDepts.Nodes)
            {
                bool toBeChecked = true;
                foreach (TreeNode cNode in pNode.ChildNodes)
                    if (!cNode.Checked) toBeChecked = false;
                pNode.Checked = toBeChecked;
            }
        }
    }
}
