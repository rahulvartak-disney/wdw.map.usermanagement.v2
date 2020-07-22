namespace Wdw.UserManagement.v2.Code
{
    public class Constants
    {
        #region => Modules <=
        public struct Modules
        {
            public const string NAME = "Module_Nm";
            public const string ID = "Module_Id";
        }
        #endregion

        #region => Users <=
        public struct Users
        {
            public const string USER_NAME = "userName";
            public const string USER_LOGIN = "userLogin";
            public const string DEFAULT_DEPT_NAME = "defaultDeptName";
            public const string DEFAULT_DEPT_NBR = "defaultDeptNbr";
        }
        #endregion

        #region => Exception Logs <=
        public struct ExceptionLogs
        {
            public const string NAME = "Exception Logs";
            public const string COL_TITLE = "Title";
            public const string COL_COMPONENT = "Component";
            public const string COL_MODIFIED = "Modified";
            public const string COL_DETAILS = "Details";
        }
        #endregion

        #region => Department <=
        public struct Department
        {
            public const string GROUP_NAME = "groupName";
            public const string GROUP_NBR = "groupNbr";
            public const string DEPT_NAME = "deptName";
            public const string DEPT_NBR = "deptNbr";
            public const string DEPT_NBR_NAME = "deptNbrName";
        }
        #endregion

        #region => Group <=
        public  struct Group
        {
            public const string GROUP_NAME = "groupName";
            public const string GROUP_NBR = "groupNbr";
        }
        #endregion

        #region => Configuration List <=
        public struct Configuration
        {
            public const string NAME = "Configuration";
            public const string GET_ALL_CONNSTR = "<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>ConnectionString</Value></Eq></Where>";
            public const string VF_CONNSTR = "<FieldRef Name='Value' />";
            public const string FLD_VALUE = "Value";
        }
        #endregion
    }
}
