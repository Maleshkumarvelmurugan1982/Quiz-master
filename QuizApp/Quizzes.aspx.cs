using System;
using QuizApp.Code;

namespace QuizApp
{
    public partial class Quizzes : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                CategoryService.Fill(ddlCategory, "All Categories", "");

                // Allow links like Quizzes.aspx?category=3
                var cat = QueryInt("category");
                if (cat > 0 && ddlCategory.Items.FindByValue(cat.ToString()) != null)
                    ddlCategory.SelectedValue = cat.ToString();

                BindQuizzes();
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            BindQuizzes();
        }

        protected void Clear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            ddlCategory.SelectedIndex = 0;
            ddlDifficulty.SelectedIndex = 0;
            BindQuizzes();
        }

        private void BindQuizzes()
        {
            int catId;
            int? category = int.TryParse(ddlCategory.SelectedValue, out catId) ? catId : (int?)null;

            // Admins also see inactive quizzes (like the original app).
            var quizzes = QuizService.GetQuizzes(txtSearch.Text, category, ddlDifficulty.SelectedValue, !IsAdmin);

            rptQuizzes.DataSource = quizzes;
            rptQuizzes.DataBind();
            phEmpty.Visible = quizzes.Count == 0;
        }
    }
}
