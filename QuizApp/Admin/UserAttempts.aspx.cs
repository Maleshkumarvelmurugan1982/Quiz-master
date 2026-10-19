using System;
using QuizApp.Code;

namespace QuizApp.Admin
{
    public partial class UserAttemptsPage : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var user = AuthService.GetById(QueryInt("id"));
            if (user == null)
            {
                Flash("User not found.", "error");
                Go("~/Admin/Users.aspx");
                return;
            }

            litName.Text = user.Name;
            litEmail.Text = user.Email + (string.IsNullOrEmpty(user.Section) ? "" : " · Section " + user.Section);
            Title = user.Name + " — Attempts — QuizMaster";

            gvAttempts.DataSource = AttemptService.GetHistory(user.Id, 1000);
            gvAttempts.DataBind();
        }
    }
}
