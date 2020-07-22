<%@ Assembly Name="$SharePoint.Project.AssemblyFullName$" %>
<%@ Assembly Name="Microsoft.Web.CommandUI, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="SharePoint" Namespace="Microsoft.SharePoint.WebControls" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %> 
<%@ Register Tagprefix="Utilities" Namespace="Microsoft.SharePoint.Utilities" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Register Tagprefix="asp" Namespace="System.Web.UI" Assembly="System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" %>
<%@ Import Namespace="Microsoft.SharePoint" %> 
<%@ Register Tagprefix="WebPartPages" Namespace="Microsoft.SharePoint.WebPartPages" Assembly="Microsoft.SharePoint, Version=15.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c" %>
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="User_ManagementUserControl.ascx.cs" Inherits="Wdw.UserManagement.v2.User_Management.User_ManagementUserControl" %>

<link rel="Stylesheet" type="text/css" href="../Style Library/MAP_UM_Configs/map_um.css" />
<script type="text/javascript" src="../Style Library/MAP_UM_Configs/jquery-3.5.1.min.js"></script>
<script type="text/javascript">

    function OnTreeClick(evt) {
        var src = window.event != window.undefined ? window.event.srcElement : evt.target;
        var isChkBoxClick = (src.tagName.toLowerCase() == "input" && src.type == "checkbox");
        var ddlDefaultDept = document.getElementById('<%= ddlDefaultDept.ClientID %>');
        var txtCheckedDeptText = document.getElementById('<%= txtCheckedDeptText.ClientID %>');
        var txtCheckedDeptValue = document.getElementById('<%= txtCheckedDeptValue.ClientID %>');
        
        if (isChkBoxClick) {            
            var nodeText = getNextSibling(src).innerText || getNextSibling(src).innerHTML;
            txtCheckedDeptText.value = nodeText;
            var nodeValue = GetNodeValue(getNextSibling(src));
            txtCheckedDeptValue.value = nodeValue;

            var parentTable = GetParentByTagName("table", src);            
            var nxtSibling = parentTable.nextSibling;            
            //check if nxt sibling is not null & is an element node
            if (nxtSibling && nxtSibling.nodeType == 1) {
                //if node has children   
                if (nxtSibling.tagName.toLowerCase() == "div") {
                    //check or uncheck children at all levels                    
                    CheckUncheckChildren(parentTable.nextSibling, src.checked, ddlDefaultDept);
            }
        }
        //check or uncheck parents at all levels
        CheckUncheckParents(src, src.checked);
        }
        var o = window.event.srcElement;
        if (o.tagName == "INPUT" && o.type == "checkbox") {
            __doPostBack('<%=btnHiddenPostBack.ClientID %>', null);            
        }

        if (!src.checked) {
            var chkSelectAll = document.getElementById('<%= chkSelectAll.ClientID %>');
            chkSelectAll.checked = false;
        }
    } 

    function CheckUncheckChildren(childContainer, check, ddlDefaultDept) {
    var childChkBoxes = childContainer.getElementsByTagName("input");
    var childChkBoxCount = childChkBoxes.length;
    for (var i = 0; i < childChkBoxCount; i++) {
        childChkBoxes[i].checked = check;        
        if (check) AddToDropdown(ddlDefaultDept, getNextSibling(childChkBoxes[i]).innerText, GetNodeValue(getNextSibling(childChkBoxes[i])));
        else RemoveFromDropdown(ddlDefaultDept, getNextSibling(childChkBoxes[i]).innerText, GetNodeValue(getNextSibling(childChkBoxes[i])));
    }
} 

function CheckUncheckParents(srcChild, check) {   
    var parentDiv = GetParentByTagName("div", srcChild);
    var parentNodeTable = parentDiv.previousSibling;
    if (parentNodeTable) {
        var checkUncheckSwitch;
        //checkbox checked
        if (check) {
            var isAllSiblingsChecked = AreAllSiblingsChecked(srcChild);
            if (isAllSiblingsChecked)
                checkUncheckSwitch = true;
          else
            return; //do not need to check parent if any(one or more) child not checked
        }
        else //checkbox unchecked
        {
            checkUncheckSwitch = false;
        } 
        var inpElemsInParentTable = parentNodeTable.getElementsByTagName("input");
        if (inpElemsInParentTable.length > 0) {
            var parentNodeChkBox = inpElemsInParentTable[0];
            parentNodeChkBox.checked = checkUncheckSwitch;
            //do the same recursively
            CheckUncheckParents(parentNodeChkBox, checkUncheckSwitch);
        }
    }
}    

function AreAllSiblingsChecked(chkBox) {
    var parentDiv = GetParentByTagName("div", chkBox);
    var childCount = parentDiv.childNodes.length;
    for (var i = 0; i < childCount; i++) {
        if (parentDiv.childNodes[i].nodeType == 1) {
            //check if the child node is an element node
          if (parentDiv.childNodes[i].tagName.toLowerCase() == "table") {
          var prevChkBox = parentDiv.childNodes[i].getElementsByTagName("input")[0];
                //if any of sibling nodes are not checked, return false
                if (!prevChkBox.checked) {
                    return false;
                }
            }
        }
    }
    return true;
}

 //utility function to get the container of an element by tagname
function GetParentByTagName(parentTagName, childElementObj) {
    var parent = childElementObj.parentNode;
    while (parent.tagName.toLowerCase() != parentTagName.toLowerCase()) {
        parent = parent.parentNode;
    }
    return parent;
} 

function getNextSibling(element) {
    var n = element;
    do n = n.nextSibling;
    while (n && n.nodeType != 1);
    return n;
}

function getFirstChild(element) {
    var n = element; var index = 0;
    var child = null;
    do child = n.children[index++];
    while (child && child.nodeType != 1 && index < 10);
    return child;
}

 //returns NodeValue
function GetNodeValue(node) {
    var nodeValue = "";
    var nodePath = node.href.substring(node.href.indexOf(",") + 2, node.href.length - 2);
    var nodeValues = nodePath.split("\\");
    if (nodeValues.length > 1)
        nodeValue = nodeValues[nodeValues.length - 1];
    else
        nodeValue = nodeValues[0].substr(1);
    return nodeValue;
}

function CheckIfExistsInDropdown(obj, value) {
    var IsExists = false;
    var ddloption = obj.options;
    for (var i = 0; i < ddloption.length; i++) {
        if (ddloption[i].value === value) {
            IsExists = true;
            break;
        }
    }
    return IsExists;
}

function AddToDropdown(obj, text, value) {
    if (!CheckIfExistsInDropdown(obj, value)) {
        var option = document.createElement('option');
        option.text = text;
        option.value = value;
        obj.add(option, 0);
        if(value != '-1')
            RemoveFromDropdown(obj, 'No Department Selected', '-1');
        sortSelectDropdown(obj);
    }
}

function RemoveFromDropdown(obj, text, value) {
    var ddloption = obj.options;
    for (var i = 0; i < ddloption.length; i++) {
        if (ddloption[i].value === value) {            
            obj.remove(i);
            if (obj.options.length === 0) {
                var option = document.createElement('option');
                option.text = 'No Department Selected';
                option.value = -1;
                obj.add(option, 0);
            }
            break;
        }
    }    
}

function sortSelectDropdown(selElem) {
    var selectedValue = selElem.options[selElem.selectedIndex].value;
    var tmpAry = new Array();
    for (var i = 0; i < selElem.options.length; i++) {
        tmpAry[i] = new Array();
        tmpAry[i][0] = selElem.options[i].text;
        tmpAry[i][1] = selElem.options[i].value;
    }
    tmpAry.sort();
    while (selElem.options.length > 0) {
        selElem.options[0] = null;
    }
    for (var i = 0; i < tmpAry.length; i++) {
        var op = new Option(tmpAry[i][0], tmpAry[i][1]);
        selElem.options[i] = op;
    }

    setSelectedIndex(selElem, selectedValue);
    return;
}

function isNumeric(value)
{
    var numbers = /^[0-9]+$/;
    if (value.match(numbers)) return true;
    return false;
}

function setSelectedIndex(s, v) {
    for (var i = 0; i < s.options.length; i++) {
        if (s.options[i].value == v) {
            s.options[i].selected = true;
            return;
        }
    }
}

function btnSubmit_ClientClick(){
    var ddlSelectUser = document.getElementById('<%= ddlSelectUser.ClientID %>');
    if (null != ddlSelectUser) {
        var selectedValue = ddlSelectUser.options[ddlSelectUser.selectedIndex].value;
        if ('-1' === selectedValue) {
            alert('Please select an existing or a new user to proceed..');
            return false;
        }
    }    
    
    if (!isAtleast1ModuleSelected()) {
        alert('Please select a module to proceed..');
        return false;
    }
    
    var chkSelectAll = document.getElementById('<%= chkSelectAll.ClientID %>');
    console.log(chkSelectAll.checked);
    var ddlDefaultDept = document.getElementById('<%= ddlDefaultDept.ClientID %>');
    if (null != ddlDefaultDept) {
        var selectedValue = ddlDefaultDept.options[ddlDefaultDept.selectedIndex].value;
        if ('-1' === selectedValue) {
            alert('Please choose the Default Department before clicking Submit.');
            return false;
        }
    }
    return true;    
}

    function isAtleast1ModuleSelected()
    {        
        var chklstModules = document.getElementById('<%= chklstModules.ClientID %>');
        if (null != chklstModules) {
            var allModules = chklstModules.getElementsByTagName('input');
            if (null != allModules) {
                for (var index = 0 ; index < allModules.length; index++) {
                    if (allModules[index].checked) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    function btnDelete_ClientClick() {
    var ddlSelectUser = document.getElementById('<%= ddlSelectUser.ClientID %>');
    if (null != ddlSelectUser) {
        var selectedValue = ddlSelectUser.options[ddlSelectUser.selectedIndex].value;
        if ('-1' === selectedValue) {
            alert('Please select an existing or a new user to proceed..');
            return false;
        }
    }
    if (!confirm('Are you sure you want to delete the selected user  '+ ddlSelectUser.options[ddlSelectUser.selectedIndex].text + ' ?' )) return false;
    return true;
    }

     var xPos, yPos;
      var prm = Sys.WebForms.PageRequestManager.getInstance();

      function BeginRequestHandler(sender, args) {
        if ($get('<%=dvDeptSelection.ClientID%>') != null) {
          // Get X and Y positions of scrollbar before the partial postback
          xPos = $get('<%=dvDeptSelection.ClientID%>').scrollLeft;
          yPos = $get('<%=dvDeptSelection.ClientID%>').scrollTop;
        }
     }

     function EndRequestHandler(sender, args) {
         if ($get('<%=dvDeptSelection.ClientID%>') != null) {
           // Set X and Y positions back to the scrollbar
           // after partial postback
           $get('<%=dvDeptSelection.ClientID%>').scrollLeft = xPos;
           $get('<%=dvDeptSelection.ClientID%>').scrollTop = yPos;
         }
     }

     prm.add_beginRequest(BeginRequestHandler);
     prm.add_endRequest(EndRequestHandler);

     function NewUser_PostBack() {
         var ppNewUser = document.getElementById('<%= ppNewUser.ClientID %>');
         if (null != ppNewUser) {
             var allDivs = ppNewUser.getElementsByTagName('div');
             if (null != allDivs && allDivs.length > 0) {
                 for (var index = 0; index < allDivs.length; index++) {
                     if (allDivs[index].id === 'divEntityData') {
                         var newUser = allDivs[index].getAttribute("displaytext");
                         var txtNewUserName = document.getElementById('<%= txtNewUserName.ClientID %>');
                         if (null != txtNewUserName) {
                             txtNewUserName.value = newUser;                             
                         }                         
                     }
                 }
             }
         }                    
         __doPostBack('<%=btnHiddenNewUserPostBack.ClientID %>', null);
     }
</script>

<asp:UpdatePanel ID="upManageUsers" runat="server">
    <ContentTemplate>
        <asp:Label ID="lblMessage" runat="server" Text="" Visible="false" Font-Bold="true"></asp:Label><br /><br />
        <asp:Button ID="btnReset" Text="Reset" Visible="false" runat="server" OnClick="btnReset_Click" CssClass="btnGeneral" />
        <table id="tblMain" class="tblMain" runat="server">
            <tr>
                <td><b>Manage User</b>&nbsp;&nbsp;&nbsp;
                    <asp:DropDownList ID="ddlSelectUser" Width="314px" runat="server" OnSelectedIndexChanged="ddlSelectUser_SelectedIndexChanged" AutoPostBack="true"></asp:DropDownList>
                </td>
                <td>&nbsp;</td>
                <td colspan="2" style="text-align: left;"><asp:Button ID="btnNewUser" Text="Add New User" CssClass="btnGeneral btnDelete" runat="server" /></td>
            </tr>
            <tr id="trAddNewUser" runat="server" style="display: none;">
                <td><nobr>Add a new user: </nobr></td>
                <td colspan="3">
                     <SharePoint:PeopleEditor ID="ppNewUser" runat="server" CssClass="ppAddNewUser" MaximumEntities="1" OnValueChangedClientScript="NewUser_PostBack();" />
                </td>
            </tr>
            <tr>
                <td colspan="2" style="vertical-align: top;">
                    <div class="dvDeptSelection" runat="server" id="dvDeptSelection">
                        <b>MAP Departments</b><br /><br />
                        <asp:CheckBox ID="chkSelectAll" Text="All Departments" OnCheckedChanged="chkSelectAll_CheckedChanged" CssClass="chkSelectAll" AutoPostBack="true" runat="server" />                        
                        <asp:TreeView ID="tvDepts"  runat="server" ForeColor="Black" Font-Size="12pt" ShowCheckBoxes="All" OnClick="OnTreeClick(event)"></asp:TreeView>
                        <span style="display: none;">
                            <asp:Button ID="btnHiddenPostBack" runat="server" Text="Hidden" OnClick="btnHiddenPostBack_Click" />
                            <asp:TextBox ID="txtCheckedDeptText" runat="server" Text=""></asp:TextBox>
                            <asp:TextBox ID="txtCheckedDeptValue" runat="server" Text=""></asp:TextBox>
                            <asp:Button ID="btnHiddenNewUserPostBack" runat="server" Text="HiddenNewUser" OnClick="btnHiddenNewUserPostBack_Click" />
                        </span>
                    </div>    
                </td>
                <td colspan="2" style="vertical-align: top;">
                    <div class="dvModules">
                        <b>MAP Modules</b><br /><br /><asp:CheckBoxList ID="chklstModules" runat="server"></asp:CheckBoxList>
                    </div>
                </td>
            </tr>
            <tr>
                <td><b>Default Department</b>&nbsp;&nbsp;&nbsp;
                    <asp:DropDownList ID="ddlDefaultDept" runat="server" Width="261px"></asp:DropDownList>
                </td>
                <td colspan="3">                                        &nbsp;
                </td>                
            </tr>
            <tr>
                <td colspan="2">
                      <asp:Button ID="btnDelete" Text="Delete" OnClick="btnDelete_Click" OnClientClick="javascript: return btnDelete_ClientClick();" CssClass="btnGeneral btnDelete" runat="server" />&nbsp;&nbsp;&nbsp;
                      <asp:Button ID="btnCancel" Text="Reset" OnClick="btnCancel_Click"  CssClass="btnGeneral" runat="server" />
                </td>
                <td colspan="2" style="text-align: left;">                      
                     <asp:Button ID="btnSubmit" Text="Submit" OnClick="btnSubmit_Click" OnClientClick="javascript: return btnSubmit_ClientClick();" CssClass="btnGeneral btnDelete" runat="server" />                    
                </td>
            </tr>            
        </table><br />
</ContentTemplate>
</asp:UpdatePanel>
<asp:UpdateProgress ID="uProg" runat="server" DisplayAfter="0" AssociatedUpdatePanelID="upManageUsers">
        <ProgressTemplate>
            <div class="overlay" />
            <div class="overlayContent">
                <h2>Loading.....Please wait..</h2>
                <img src="../Style Library/MAP_UM_Configs/ajax-loader-gif-6.gif" alt="Loading.....Please wait.."  />
            </div>
        </ProgressTemplate>
    </asp:UpdateProgress>

<span style="display: none;">
    <asp:TextBox ID="txtNewUserName" runat="server" Text=""></asp:TextBox>
</span>

<script type="text/javascript">
    function runAfterEverythingElse() {
        var ppNewUser = document.getElementById('<%= ppNewUser.ClientID %>');
        
        if (null != ppNewUser) {
            var allAnchors = ppNewUser.getElementsByTagName('a');
            if(null != allAnchors && allAnchors.length > 0)
            {
                for(var index = 0; index <allAnchors.length; index++)
                {
                    if(allAnchors[index].title === 'Browse')
                    {
                        var btnAddNewUser = document.getElementById('<%= btnNewUser.ClientID %>');
                        if (null != btnAddNewUser)       btnAddNewUser.onclick = allAnchors[index].onclick;                        
                    }
                }
            }
        }
    }
    _spBodyOnLoadFunctionNames.push("runAfterEverythingElse");
</script>