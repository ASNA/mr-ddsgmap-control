using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace ASNA.Monarch.Support
{
    partial class iResumeFullScreen : System.Web.UI.Page
    {
        protected override void OnPreRender(EventArgs e)
        {
            string script = @"
var previousSession = '';
var sid = localStorage.getItem('ASNA_iOS_Last_ASP_SID');

if (sid) {
    previousSession = '(S(' + sid + '))/';
}
window.location = '";

            script = script + Request.ApplicationPath.TrimEnd('/') + "/' + previousSession + 'Monarch/Resumer.aspx';";
            Page.ClientScript.RegisterStartupScript(typeof(iResumeFullScreen), "_Redirect", script, true);
            Session.Abandon();
        }

    }
}
