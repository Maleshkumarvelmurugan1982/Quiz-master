using System;
using System.Collections.Generic;
using QuizApp.Code;

namespace QuizApp.Admin
{
    public partial class QuizEditPage : BasePage
    {
        private int QuizId
        {
            get { return QueryInt("id"); }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack) return;

            CategoryService.Fill(ddlCategory, "-- Select Category --", "");

            if (QuizId > 0)
            {
                var quiz = QuizService.GetQuiz(QuizId);
                if (quiz == null)
                {
                    Flash("Quiz not found.", "error");
                    Go("~/Admin/Quizzes.aspx");
                    return;
                }

                litHeading.Text = "Edit Quiz";
                Title = "Edit: " + quiz.Title + " — QuizMaster";

                txtTitle.Text = quiz.Title;
                txtDescription.Text = quiz.Description;
                if (ddlCategory.Items.FindByValue(quiz.CategoryId.ToString()) != null)
                    ddlCategory.SelectedValue = quiz.CategoryId.ToString();
                ddlDifficulty.SelectedValue = quiz.Difficulty;
                txtDuration.Text = quiz.DurationMinutes.ToString();
                txtPassPct.Text = Fmt.Num(quiz.PassingPercentage);
                txtMaxQ.Text = quiz.MaxQuestionsPerAttempt.HasValue ? quiz.MaxQuestionsPerAttempt.Value.ToString() : "";
                txtNegValue.Text = Fmt.Num(quiz.NegativeMarkValue);
                chkNegative.Checked = quiz.NegativeMarking;
                chkRandQ.Checked = quiz.RandomizeQuestions;
                chkRandOpt.Checked = quiz.RandomizeOptions;
                chkActive.Checked = quiz.IsActive;

                ucQuestions.Questions = QuestionService.GetDrafts(QuizId);
            }
            else
            {
                // New quiz: start with one empty question to fill in.
                ucQuestions.Questions = new List<QuestionDraft> { new QuestionDraft() };
            }
        }

        protected void Save_Click(object sender, EventArgs e)
        {
            var input = new QuizInput
            {
                Title = (txtTitle.Text ?? "").Trim(),
                Description = (txtDescription.Text ?? "").Trim(),
                Difficulty = ddlDifficulty.SelectedValue,
                DurationMinutes = Parse.Int(txtDuration.Text, 15),
                PassingPercentage = Parse.Decimal(txtPassPct.Text, 50m),
                NegativeMarking = chkNegative.Checked,
                NegativeMarkValue = Parse.Decimal(txtNegValue.Text, 0.25m),
                RandomizeQuestions = chkRandQ.Checked,
                RandomizeOptions = chkRandOpt.Checked,
                MaxQuestionsPerAttempt = string.IsNullOrWhiteSpace(txtMaxQ.Text) ? (int?)null : Parse.Int(txtMaxQ.Text, 0),
                IsActive = chkActive.Checked
            };

            var questions = ucQuestions.Questions;

            // Validate everything before touching the database.
            var newCategory = (txtNewCategory.Text ?? "").Trim();
            int categoryId;
            input.CategoryId = int.TryParse(ddlCategory.SelectedValue, out categoryId) ? categoryId : 0;

            var error = QuizService.Validate(input, newCategory);
            if (error == null && questions.Count == 0) error = "Add at least one question before saving the quiz.";
            if (error == null) error = QuestionService.ValidateAll(questions);
            if (error != null)
            {
                ShowError(error);
                return;
            }

            try
            {
                if (newCategory.Length > 0)
                {
                    input.CategoryId = CategoryService.FindOrCreate(newCategory);
                }

                if (QuizId > 0)
                {
                    QuizService.UpdateWithQuestions(QuizId, input, questions);
                    Flash("Quiz updated with " + questions.Count + " question(s)!", "success");
                }
                else
                {
                    QuizService.CreateWithQuestions(input, questions, UserId);
                    Flash("Quiz saved with " + questions.Count + " question(s)!", "success");
                }
            }
            catch (Exception ex)
            {
                ShowError("Could not save the quiz: " + ex.Message);
                return;
            }

            Go("~/Admin/Quizzes.aspx");
        }

        private void ShowError(string message)
        {
            phError.Visible = true;
            litError.Text = message;
            Toast(message, "error");
        }
    }
}
