using System;
using System.Web.UI;
using QuizApp.Code;

namespace QuizApp
{
    /// <summary>Start page: sends the user to the right dashboard (or to the login page).</summary>
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect(Auth.HomeUrl(Auth.Current), true);
        }
    }
}
