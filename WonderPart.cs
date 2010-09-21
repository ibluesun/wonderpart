using System;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Serialization;

using System.Xml;
using System.Web.UI.WebControls;
using System.Text;



namespace WonderSolutions.SharePoint.WebParts
{
    /// <summary>
    /// WonderPart: Host usercontrols.
    /// </summary>
    public class WonderPart : System.Web.UI.WebControls.WebParts.WebPart
    {

        /* Note for reader:
         * To use the assembly do the following:
         *  1) Open web.config of SharePoint Web Site
         *  2) Find <SafeControl>  tags
         *  3) Add       <SafeControl Assembly="Bluebridge.Intranet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=37cd2a6b423271d5"
                            Namespace="Bluebridge.Intranet"
                            TypeName="*" Safe="True" />
         * 4) Add this assembly to the _app_bin\  directory or in the GAC :)
         * Enjoy the webpart.
         */


        /* Note: About using the class
         * This class was intended to have the functionality of selecting folder which have all the user
         * controls that form an application
         * but due to time limits the testing reveals something wrong by using it this way
         * so please don't use it like that instead inherit a class from it
         * and override the folders locations and specify explictly the usercontrol
         * then navigation will be used freely without problems.
         */

        /* Note: About RenderCount
         * I am storing render count in current view state
         * this way I know how many renders happens in the webpart
         * and to implement a custom WonderPartPostBack.
         * Why I am using custom PostBack??
         * because when navigating to another user control is occured in the PostBack Back event
         * that's why I need a custom propery to tell me exactly if the user control loaded for
         * first time or not
         * -----------------------------
         * the functionality is running without a problem and I have no exceptions assumptions about it untill now
         */


        #region Fields

        //MainHolder is a Place Holder of loaded controls.
        protected System.Web.UI.WebControls.PlaceHolder MainHolder;

        //Built in the application
        protected string _WonderPartApplicationsFolder = "~/WonderPartApplications/";

        //modified by the WonderPartApplication
        protected string _WonderPartApplicationFolder = "DefaultApplication/";

        //modified by the WonderPartDefaultUserControl
        protected string _WonderPartDefaultUserControl = "Default.ascx";

        #endregion


        #region WonderPart Properties

        /// <summary>
        /// The folder in which all WonderPart applications exist
        /// </summary>
        public virtual string WonderPartApplicationsFolder
        {
            get
            {
                return _WonderPartApplicationsFolder;
            }
        }


        /// <summary>
        /// This property will be set from the sharepoint 
        /// when choosing the WonderPart application
        /// </summary>
        [Personalizable(PersonalizationScope.Shared)]
        [WebBrowsable(true)]
        [WebDisplayName("Application Folder")]
        [WebDescription("The WonderPart Application Folder Contains the application user controls")]
        public virtual string WonderPartApplicationFolder
        {
            get
            {
                return _WonderPartApplicationFolder;
            }
            set
            {
                _WonderPartApplicationFolder = value;
            }
        }


        /// <summary>
        /// Default User control that rendered every time the webpart run.
        /// </summary>
        [Personalizable(PersonalizationScope.Shared)]
        [WebBrowsable(true)]
        [WebDisplayName("Default User Control")]
        [WebDescription("The WonderPart Default user control page.")]
        public virtual string WonderPartDefaultUserControl
        {
            get
            {
                return _WonderPartDefaultUserControl;
            }
            set
            {
                _WonderPartDefaultUserControl = value;
            }
        }


        /// <summary>
        /// The current user control which will be displayed in the webpart.
        /// Have no interfere with the default user control.
        /// Default user control is used when no view state information found.
        /// </summary>
        public string CurrentUserControl
        {
            get
            {
                if (ViewState["WonderPartCurrentUserControl"] != null)
                {
                    return (string)ViewState["WonderPartCurrentUserControl"];
                }
                else
                {
                    ViewState["WonderPartCurrentUserControl"] = WonderPartDefaultUserControl;

                    return WonderPartDefaultUserControl;
                }
            }
            set
            {
                //Store the current user control in view state
                ViewState["WonderPartCurrentUserControl"] = value;

                RenderCount = -1;   //first time to load.


                //clear the child controls.
                this.Controls.Clear();

                //reset the value of created childs.
                this.ChildControlsCreated = false;

                //let the framework re-render its child controls.
                this.EnsureChildControls();

            }
        }

        public int RenderCount
        {
            get
            {
                if (ViewState["RenderCount"] == null)
                {
                    ViewState["RenderCount"] = (int)-1;
                }
                return (int)ViewState["RenderCount"];
            }
            set
            {
                ViewState["RenderCount"] = value;
            }
        }


        /// <summary>
        /// Custom post back because the WonderPart change its usercontrol during the page post back.
        /// </summary>
        public bool WonderPartIsPostBack
        {
            get
            {
                if (RenderCount > 0) return true;
                else return false;

            }
        }


        private string _FloatingData = string.Empty;

        /// <summary>
        /// FloatingData is a property which hold string 
        /// this string could be usefull in sharing data between transfering of user controls
        /// The string selection come from the same idea of using strings the sharepoint
        /// workflow ASPX negotiation
        /// </summary>
        public string FloatingData
        {
            get
            {
                return _FloatingData;
            }
            set
            {
                _FloatingData = value;
            }
        }


        #endregion


        #region Rendering

        /// <summary>
        /// The control that will be rendered
        /// </summary>
        protected Control MainControl
        {

            get
            {
                string ControlToLoad = "";
                try
                {
                    //try to load the control and display it
                    ControlToLoad = WonderPartApplicationsFolder + WonderPartApplicationFolder + CurrentUserControl;

                    Control TargetUserControl = this.Page.LoadControl(ControlToLoad);

                    return TargetUserControl;
                }
                catch (Exception ex)
                {
                    Label ExceptionLablel = new Label();

                    StringBuilder strException = new StringBuilder();
                    strException.AppendLine("Current Control: " + ControlToLoad);
                    strException.AppendLine(ex.Message);
                    strException.AppendLine(ex.Source);
                    strException.AppendLine(ex.StackTrace);


                    if (ex.InnerException != null) strException.AppendLine("InnerException: " + ex.InnerException.Message);

                    ExceptionLablel.Text = strException.ToString();

                    return ExceptionLablel;
                }
            }
        }


        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            RenderCount++;   //first time = -1 ==> 0, 1, 2 etc.


#if DEBUG
            //write information about current usercontrol.
            Label lblInfo = new Label();
            lblInfo.Text = "<" + WonderPartApplicationsFolder + WonderPartApplicationFolder + CurrentUserControl + ": RenderCount= " + RenderCount.ToString() + ">";

            lblInfo.Attributes.Add("Width", "100%");
            this.Controls.Add(lblInfo);
#endif


            //render the main control
            MainHolder = new PlaceHolder();

            this.Controls.Add(MainHolder);

            //load the control after loading placeholder in the webpart
            //because I need the properties of webpart to be available to the 
            //enclosed usercontrol
            MainHolder.Controls.Add(MainControl);

        }

        #endregion

        # region "AJAX Support Code"

        private ScriptManager _ajaxManager;
        private int _asyncPostBackTimeout;
        private bool _isAjaxEnabled;

        public ScriptManager AjaxManager
        {
            get { return _ajaxManager; }
            set { _ajaxManager = value; }
        }

        public int AsyncPostBackTimeout
        {
            get { return _asyncPostBackTimeout; }
            set { _asyncPostBackTimeout = value; }
        }

        public bool IsAjaxEnabled
        {
            get { return _isAjaxEnabled; }
            set { _isAjaxEnabled = value; }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if (IsAjaxEnabled)
            {
                _ajaxManager = ScriptManager.GetCurrent(this.Page);
                if (_ajaxManager == null)
                {
                    _ajaxManager = new ScriptManager();
                    _ajaxManager.EnablePartialRendering = true;
                    _ajaxManager.AsyncPostBackTimeout = _asyncPostBackTimeout == 0 ? 300 : _asyncPostBackTimeout;
                    Page.ClientScript.RegisterStartupScript(this.GetType(), ID, "_spOriginalFormAction = document.forms[0].action;", true);
                    if (Page.Form != null)
                    {
                        string str = Page.Form.Attributes["onsubmit"];
                        if (!(string.IsNullOrEmpty(str) || !(str == "return _spFormOnSubmitWrapper();")))
                        {
                            Page.Form.Attributes["onsubmit"] = "_spFormOnSubmitWrapper();";
                        }
                        Page.Form.Controls.AddAt(0, this._ajaxManager);
                    }
                }
            }
        }

        # endregion


    }

}
