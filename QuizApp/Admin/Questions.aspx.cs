using System;
using System.Web.UI.WebControls;
using QuizApp.Code;

namespace QuizApp.Admin
{
    public partial class AdminQuestionsPage : BasePage
    {
        private int QuizId
        {
            get { return QueryInt("quizId"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var quiz = QuizService.GetQuiz(QuizId);
            if (quiz == null)
            {
                Flash("Quiz not found.", "error");
                Go("~/Admin/Quizzes.aspx");
                return;
            }

            litQuizTitle.Text = quiz.Title;
            Title = "Questions: " + quiz.Title + " — QuizMaster";
            lnkNew.NavigateUrl = "~/Admin/QuestionEdit.aspx?quizId=" + QuizId;

            if (!IsPostBack) BindQuestions();
        }

        protected void Questions_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

            // Make sure the question belongs to this quiz.
            var question = QuestionService.Get(id);
            if (question == null || question.QuizId != QuizId)
            {
                Toast("Question not found.", "error");
                BindQuestions();
                return;
            }

            if (e.CommandName == "ToggleQuestion")
            {
                QuestionService.Toggle(id);
                Toast(question.IsActive ? "Question hidden." : "Question visible again.", "success");
            }
            else if (e.CommandName == "DeleteQuestion")
            {
                string message;
                QuestionService.Delete(id, out message);
                Toast(message, "success");
            }

            BindQuestions();
        }

        private void BindQuestions()
        {
            // Admins see hidden questions too, so they can show them again.
            gvQuestions.DataSource = QuestionService.GetByQuiz(QuizId, true);
            gvQuestions.DataBind();
        }
    }
}
