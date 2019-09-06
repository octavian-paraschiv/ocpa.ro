using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ProTONE
{
    public partial class oauth : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var qs = Request.QueryString.ToString();
            var redirectUrl = $"http://localhost:11000?{qs}";
            Response.Redirect(redirectUrl, true);
        }
    }
}