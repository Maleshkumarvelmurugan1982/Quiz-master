using System;
using System.Web.UI;
using QuizApp.Code;

namespace QuizApp
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
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

            if (!IsPostBack && Auth.Current != null)
                Response.Redirect(Auth.HomeUrl(Auth.Current), true);
        }

        protected void Register_Click(object sender, EventArgs e)
        {
            string error;
            var role = rblRole.SelectedValue == "Admin" ? "Admin" : "User";
            var user = AuthService.Register(txtName.Text, txtEmail.Text, txtPassword.Text, role,
                role == "Admin" ? null : ddlSection.SelectedValue, out error);
            if (user == null)
            {
                phError.Visible = true;
                litError.Text = error;
                return;
            }

            Auth.SignIn(user, false);
            Session["qm_flash"] = "success|Welcome to QuizMaster, " + user.Name + "!";
            Response.Redirect(user.IsAdmin ? "~/Admin/Dashboard.aspx" : "~/Dashboard.aspx", true);
        }
    }
}
