using System;
using QuizApp.Code;

namespace QuizApp
{
    public partial class Dashboard : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            var d = AttemptService.GetUserDashboard(UserId);

            litName.Text = d.WelcomeName;
            litAvailable.Text = d.AvailableQuizzes.ToString();
            litAttempted.Text = d.QuizzesAttempted.ToString();
            litAverage.Text = Fmt.Num(d.AverageScore);
            litHighest.Text = Fmt.Num(d.HighestScore);

            rptRecent.DataSource = d.RecentAttempts;
            rptRecent.DataBind();
            phNoRecent.Visible = d.RecentAttempts.Count == 0;

            rptChart.DataSource = d.Performance;
            rptChart.DataBind();
            phChart.Visible = d.Performance.Count > 0;
            phNoChart.Visible = d.Performance.Count == 0;
        }
    }
}
