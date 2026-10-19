using System;
using System.Web.UI;
using QuizApp.Code;

namespace QuizApp
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!DbInitializer.Ready)
                DbInitializer.EnsureDatabase();

            if (!DbInitializer.Ready)
            {
                phDbError.Visible = true;
                litDbError.Text = DbInitializer.LastError;
                btnLogin.Enabled = false;
                return;
            }

            if (!IsPostBack && Auth.Current != null)
                Response.Redirect(Auth.HomeUrl(Auth.Current), true);
        }

        protected void Login_Click(object sender, EventArgs e)
        {
            string error;
            var user = AuthService.Login(txtEmail.Text, txtPassword.Text, out error);
            if (user == null)
            {
                phError.Visible = true;
                litError.Text = error;
                return;
            }

            Auth.SignIn(user, chkRemember.Checked);

            var returnUrl = Request.QueryString["ReturnUrl"];
            if (IsLocalUrl(returnUrl) && (user.IsAdmin || returnUrl.IndexOf("/Admin/", StringComparison.OrdinalIgnoreCase) < 0))
                Response.Redirect(returnUrl, true);
            else
                Response.Redirect(user.IsAdmin ? "~/Admin/Dashboard.aspx" : "~/Dashboard.aspx", true);
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\");
        }
    }
}
