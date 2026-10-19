using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuizApp.Code;

namespace QuizApp.Admin
{
    /// <summary>
    /// Step 1: admin enters a topic or uploads a file -> AI creates a draft (nothing saved yet).
    /// Step 2: admin reviews/edits the draft in the question editor and saves it.
    /// </summary>
    public partial class GenerateQuizPage : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                litModel.Text = QuizGeneratorService.ConfiguredModel;
                CategoryService.Fill(ddlCategory, "-- Select Category --", "");
            }
        }

        // ------------------------------------------------------------
        // STEP 1 -> generate
        // ------------------------------------------------------------
        protected void Generate_Click(object sender, EventArgs e)
        {
            var count = Parse.Int(txtCount.Text, 0);
            if (count < 3 || count > 30)
            {
                ShowError("Number of questions must be between 3 and 30.");
                return;
            }

            var request = new GenerateRequest
            {
                NumQuestions = count,
                Difficulty = ddlGenDifficulty.SelectedValue,
                Instructions = txtInstructions.Text
            };

            if (hfMode.Value == "file")
            {
                if (!fuFile.HasFile)
                {
                    ShowError("Choose a file first.");
                    return;
                }

                var ext = (Path.GetExtension(fuFile.FileName) ?? "").ToLowerInvariant();
                if (!QuizGeneratorService.AllowedExtensions.Split(',').Contains(ext))
                {
                    ShowError("Unsupported file type. Upload PDF, DOCX, PPTX, TXT, MD, CSV, JSON, PNG, JPG, WEBP or GIF.");
                    return;
                }
                if (fuFile.PostedFile.ContentLength > QuizGeneratorService.MaxFileBytes)
                {
                    ShowError("File is too large. Maximum size is 10 MB.");
                    return;
                }

                request.FileName = fuFile.FileName;
                request.FileBytes = fuFile.FileBytes;
                request.Topic = txtFocus.Text;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(txtTopic.Text))
                {
                    ShowError("Enter a topic first.");
                    return;
                }
                request.Topic = txtTopic.Text;
            }

            // The AI call can take 10-60 seconds. Task.Run runs it on a background thread
            // (no ASP.NET context, so it cannot deadlock) and we wait here for the result.
            try
            {
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4)))
                {
                    var draft = Task.Run(() => QuizGeneratorService.GenerateAsync(request, cts.Token))
                                    .GetAwaiter().GetResult();
                    ShowReview(draft);
                }
            }
            catch (QuizGenerationException ex)
            {
                ShowError(ex.Message);
            }
            catch (OperationCanceledException)
            {
                ShowError("The AI took too long to respond. Please try again.");
            }
            catch (Exception ex)
            {
                ShowError("An unexpected error occurred while generating the quiz: " + ex.Message);
            }
        }

        private void ShowReview(GeneratedQuiz draft)
        {
            pnlInput.Visible = false;
            pnlReview.Visible = true;
            phError.Visible = false;

            txtQuizTitle.Text = draft.Title;
            txtQuizDescription.Text = draft.Description;
            ddlQuizDifficulty.SelectedValue = draft.Difficulty;
            txtDuration.Text = draft.SuggestedDurationMinutes.ToString();
            txtPassPct.Text = "50";
            chkRandQ.Checked = false;
            chkPublish.Checked = true;

            phWarning.Visible = !string.IsNullOrEmpty(draft.Warning);
            litWarning.Text = draft.Warning;
            litModelInfo.Text = string.IsNullOrEmpty(draft.ModelUsed)
                ? ""
                : "Generated with " + draft.ModelUsed +
                  (draft.ModelsTried > 1 ? " (after " + (draft.ModelsTried - 1) + " retry/retries)" : "") +
                  ". Review and edit anything, then save.";

            ucQuestions.Questions = draft.Questions;
            Toast("Quiz generated with " + draft.Questions.Count + " questions. Review and save.", "success");
        }

        // ------------------------------------------------------------
        // STEP 2 -> back / save
        // ------------------------------------------------------------
        protected void Back_Click(object sender, EventArgs e)
        {
            pnlReview.Visible = false;
            pnlInput.Visible = true;
        }

        protected void Save_Click(object sender, EventArgs e)
        {
            var input = new QuizInput
            {
                Title = (txtQuizTitle.Text ?? "").Trim(),
                Description = (txtQuizDescription.Text ?? "").Trim(),
                Difficulty = ddlQuizDifficulty.SelectedValue,
                DurationMinutes = Parse.Int(txtDuration.Text, 15),
                PassingPercentage = Parse.Decimal(txtPassPct.Text, 50m),
                NegativeMarking = false,
                NegativeMarkValue = 0.25m,
                RandomizeQuestions = chkRandQ.Checked,
                RandomizeOptions = false,
                MaxQuestionsPerAttempt = null,
                IsActive = chkPublish.Checked
            };

            int categoryId;
            input.CategoryId = int.TryParse(ddlCategory.SelectedValue, out categoryId) ? categoryId : 0;
            var newCategory = (txtNewCategory.Text ?? "").Trim();

            var questions = ucQuestions.Questions;

            var error = QuizService.Validate(input, newCategory);
            if (error == null && questions.Count == 0) error = "Keep at least one question.";
            if (error == null) error = QuestionService.ValidateAll(questions);
            if (error != null)
            {
                ShowError(error);
                return;
            }

            try
            {
                if (newCategory.Length > 0)
                    input.CategoryId = CategoryService.FindOrCreate(newCategory);

                QuizService.CreateWithQuestions(input, questions, UserId);
            }
            catch (Exception ex)
            {
                ShowError("Could not save the quiz: " + ex.Message);
                return;
            }

            Flash("Quiz saved with " + questions.Count + " questions!", "success");
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
