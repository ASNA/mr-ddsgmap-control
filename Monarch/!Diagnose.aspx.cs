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
    public partial class Diagnose : System.Web.UI.Page
    {
        protected override void OnLoad( EventArgs e )
        {
            ASNA.Monarch.WebDevice Device;
            string msg;
            string Stk;
            string[] lines;

            msg = base.Request.QueryString[ "m" ];
            if( msg != null )
            {
                lblMessage.Text = msg;
                Stk = base.Request.QueryString[ "s" ];
                if( msg != null )
                    lblStackTrace.Text = Stk;
            }
            else
            {
                Device = Session[ "Device" ] as ASNA.Monarch.WebDevice;
                if( Device == null )
                    Response.Redirect( "!ExpiredSession.htm" );
                lblMessage.Text = Device.AbEndMessage;
                lblStackTrace.Text = Device.AbEndStack;
            }

            if( lblStackTrace.Text != null )
            {
                lines = lblStackTrace.Text.Split( '\x0A' );
                Stk = "";
                foreach( string line in lines )
                    Stk = Stk + "<div>" + line + "</div>\n";
                lblStackTrace.Text = Stk;
            }

            Session.Abandon();
        }
    }
}