using System;
using QuizApp.Code;

namespace QuizApp.Admin
{
    /// <summary>Add a question (?quizId=) or edit one (?id=).</summary>
    public partial class QuestionEditPage : BasePage
    {
        private int _quizId;
        private int _questionId;

        protected void Page_Load(object sender, EventArgs e)
        {
            _questionId = QueryInt("id");
            QuestionItem existing = null;

            if (_questionId > 0)
            {
                existing = QuestionService.Get(_questionId);
                if (existing == null)
                {
                    Flash("Question not found.", "error");
                    Go("~/Admin/Quizzes.aspx");
                    return;
                }
                _quizId = existing.QuizId;
            }
            else
            {
                _quizId = QueryInt("quizId");
            }

            var quiz = QuizService.GetQuiz(_quizId);
            if (quiz == null)
            {
                Flash("Quiz not found.", "error");
                Go("~/Admin/Quizzes.aspx");
                return;
            }

            var backUrl = "~/Admin/Questions.aspx?quizId=" + _quizId;
            lnkBack.NavigateUrl = backUrl;
            lnkCancel.NavigateUrl = backUrl;
            litQuizTitle.Text = quiz.Title;

            if (!IsPostBack && existing != null)
            {
                litHeading.Text = "Edit Question";
                txtText.Text = existing.QuestionText;
                txtA.Text = existing.OptionA;
                txtB.Text = existing.OptionB;
                txtC.Text = existing.OptionC;
                txtD.Text = existing.OptionD;
                ddlCorrect.SelectedValue = existing.CorrectOption;
                txtMarks.Text = existing.Marks.ToString();
                if (ddlDifficulty.Items.FindByValue(existing.Difficulty) != null)
                    ddlDifficulty.SelectedValue = existing.Difficulty;
                txtExplanation.Text = existing.Explanation;
            }
            else if (existing != null)
            {
                litHeading.Text = "Edit Question";
            }
        }

        protected void Save_Click(object sender, EventArgs e)
        {
            var draft = new QuestionDraft
            {
                QuestionText = txtText.Text ?? "",
                OptionA = txtA.Text ?? "",
                OptionB = txtB.Text ?? "",
                OptionC = txtC.Text ?? "",
                OptionD = txtD.Text ?? "",
                CorrectOption = ddlCorrect.SelectedValue,
                Marks = Parse.Int(txtMarks.Text, 1),
                Difficulty = ddlDifficulty.SelectedValue,
                Explanation = txtExplanation.Text ?? ""
            };

            var error = QuestionService.Validate(draft, null);
            if (error != null)
            {
                phError.Visible = true;
                litError.Text = char.ToUpper(error[0]) + error.Substring(1);
                Toast(litError.Text, "error");
                return;
            }

            if (_questionId > 0)
            {
                QuestionService.Save(_questionId, draft);
                Flash("Question updated!", "success");
            }
            else
            {
                QuestionService.Create(_quizId, draft);
                Flash("Question added!", "success");
            }

            Go("~/Admin/Questions.aspx?quizId=" + _quizId);
        }
    }
}
