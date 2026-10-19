using System;
using QuizApp.Code;

namespace QuizApp.Admin
{
    public partial class AdminDashboardPage : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var d = AttemptService.GetAdminDashboard();

            litUsers.Text = d.TotalUsers.ToString();
            litQuizzes.Text = d.TotalQuizzes.ToString();
            litQuestions.Text = d.TotalQuestions.ToString();
            litAttempts.Text = d.TotalAttempts.ToString();

            gvQuizStats.DataSource = d.QuizStats;
            gvQuizStats.DataBind();

            rptRecent.DataSource = d.RecentAttempts;
            rptRecent.DataBind();
            phNoRecent.Visible = d.RecentAttempts.Count == 0;
        }
    }
}
