using System;
using QuizApp.Code;

namespace QuizApp
{
    public partial class QuizDetailPage : BasePage
    {
        /// <summary>Used by the &lt;%: %&gt; expressions in the markup.</summary>
        protected QuizDetail Quiz { get; private set; }

        protected int QuestionsPerAttempt
        {
            get
            {
                if (Quiz == null) return 0;
                return Quiz.MaxQuestionsPerAttempt.HasValue && Quiz.MaxQuestionsPerAttempt.Value < Quiz.QuestionCount
                    ? Quiz.MaxQuestionsPerAttempt.Value
                    : Quiz.QuestionCount;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Quiz = QuizService.GetQuiz(QueryInt("id"));

            // Users cannot open inactive quizzes; admins can preview them.
            if (Quiz == null || (!Quiz.IsActive && !IsAdmin))
            {
                Quiz = null;
                phQuiz.Visible = false;
                phNotFound.Visible = true;
                return;
            }

            Title = Quiz.Title + " — QuizMaster";

            string reason = null;
            if (!Quiz.IsActive) reason = "Quiz Inactive";
            else if (Quiz.QuestionCount == 0) reason = "No questions yet";

            btnStart.Visible = reason == null;
            phCannotStart.Visible = reason != null;
            litCannotStart.Text = reason;
        }

        protected void Start_Click(object sender, EventArgs e)
        {
            if (Quiz == null) return;

            string error;
            var layout = AttemptService.Start(UserId, Quiz.Id, out error);
            if (layout == null)
            {
                Toast(error, "error");
                return;
            }

            Session["qm_attempt_" + layout.AttemptId] = layout;
            Go("~/TakeQuiz.aspx?attempt=" + layout.AttemptId);
        }
    }
}
