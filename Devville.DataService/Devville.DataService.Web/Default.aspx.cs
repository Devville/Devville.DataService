using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Devville.DataService.Web
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var s = Request.Url;

            var d = string.Format("{0}://{1}{2}", s.Scheme, s.Authority, s.PathAndQuery);
        }
    }
}