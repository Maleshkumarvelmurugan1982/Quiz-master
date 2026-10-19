using System;
using QuizApp.Code;

namespace QuizApp
{
    public partial class ResultPage : BasePage
    {
        protected AttemptResult Result { get; private set; }
        protected string StudentName { get; private set; }
        protected string BackUrl { get; private set; }
        protected string BackText { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Result = AttemptService.GetResult(QueryInt("id"));

            // Users can only see their own results; admins can see everyone's.
            if (Result == null || (Result.UserId != UserId && !IsAdmin))
            {
                Result = null;
                phResult.Visible = false;
                phNotFound.Visible = true;
                return;
            }

            if (Result.UserId != UserId)
            {
                var student = AuthService.GetById(Result.UserId);
                StudentName = student != null ? student.Name : "";
                BackUrl = "~/Admin/UserAttempts.aspx?id=" + Result.UserId;
                BackText = "← Back to " + StudentName + "'s attempts";
            }
            else
            {
                BackUrl = "~/Quizzes.aspx";
                BackText = "← Browse Quizzes";
            }

            Title = "Result: " + Result.QuizTitle + " — QuizMaster";

            if (!IsPostBack)
            {
                rptReview.DataSource = Result.Review;
                rptReview.DataBind();
            }
        }
    }
}
