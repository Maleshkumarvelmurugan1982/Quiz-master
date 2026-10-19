using System;
using System.Web.UI.WebControls;
using QuizApp.Code;

namespace QuizApp
{
    public partial class Bookmarks : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindBookmarks();
        }

        protected void Bookmarks_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int questionId;
            if (e.CommandName == "RemoveBookmark" && int.TryParse(Convert.ToString(e.CommandArgument), out questionId))
            {
                QuestionService.Unbookmark(UserId, questionId);
                Toast("Bookmark removed.", "success");
            }
            BindBookmarks();
        }

        private void BindBookmarks()
        {
            var list = QuestionService.GetBookmarks(UserId);
            rptBookmarks.DataSource = list;
            rptBookmarks.DataBind();
            phEmpty.Visible = list.Count == 0;
        }
    }
}
