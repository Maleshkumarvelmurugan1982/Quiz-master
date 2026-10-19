using System;
using System.Web;
using System.Web.UI;

namespace QuizApp.Code
{
    /// <summary>
    /// Base class for every logged-in page: current user, database check, toast messages.
    /// (Access rules themselves are in Web.config &lt;authorization&gt;.)
    /// </summary>
    public class BasePage : Page
    {
        protected QuizPrincipal CurrentUser
        {
            get { return Auth.Current; }
        }

        protected int UserId
        {
            get { return CurrentUser != null ? CurrentUser.UserId : 0; }
        }

        protected bool IsAdmin
        {
            get { return CurrentUser != null && CurrentUser.IsAdmin; }
        }

        protected override void OnInit(EventArgs e)
        {
            if (!DbInitializer.Ready)
            {
                DbInitializer.EnsureDatabase();
                if (!DbInitializer.Ready)
                {
                    Response.Redirect("~/Login.aspx", true);
                    return;
                }
            }

            if (CurrentUser == null)
            {
                // Should not happen thanks to Web.config, but never run a page without a user.
                Response.Redirect("~/Login.aspx?ReturnUrl=" + HttpUtility.UrlEncode(Request.RawUrl), true);
                return;
            }

            base.OnInit(e);
        }

        protected int QueryInt(string name)
        {
            int value;
            return int.TryParse(Request.QueryString[name], out value) ? value : 0;
        }

        /// <summary>Shows a toast message on this response.</summary>
        protected void Toast(string message, string type = "")
        {
            ShowToast(this, message, type);
        }

        /// <summary>Stores a toast to show after the next redirect.</summary>
        protected void Flash(string message, string type = "")
        {
            Session["qm_flash"] = (type ?? "") + "|" + message;
        }

        /// <summary>Redirects and ends the request (do not call inside try/catch(Exception)).</summary>
        protected void Go(string url)
        {
            Response.Redirect(url, true);
        }

        public static void ShowToast(Page page, string message, string type)
        {
            var script = "qmToast('" + HttpUtility.JavaScriptStringEncode(message ?? "") + "','" +
                         HttpUtility.JavaScriptStringEncode(type ?? "") + "');";
            page.ClientScript.RegisterStartupScript(page.GetType(), "qmToast" + Guid.NewGuid().ToString("N"), script, true);
        }
    }
}
