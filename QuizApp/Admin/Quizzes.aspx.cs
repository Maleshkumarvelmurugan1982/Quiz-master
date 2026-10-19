using System;
using System.Web.UI.WebControls;
using QuizApp.Code;

namespace QuizApp.Admin
{
    public partial class AdminQuizzesPage : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindQuizzes();
        }

        protected void Quizzes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int quizId;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out quizId)) return;

            if (e.CommandName == "ToggleQuiz")
            {
                QuizService.Toggle(quizId);
                Toast("Quiz status updated.", "success");
            }
            else if (e.CommandName == "DeleteQuiz")
            {
                if (QuizService.Delete(quizId))
                    Toast("Quiz deleted.", "success");
                else
                    Toast("Quiz not found.", "error");
            }

            BindQuizzes();
        }

        private void BindQuizzes()
        {
            gvQuizzes.DataSource = QuizService.GetQuizzes(null, null, null, false);
            gvQuizzes.DataBind();
        }
    }
}
