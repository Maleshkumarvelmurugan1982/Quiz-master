using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using QuizApp.Code;

namespace QuizApp
{
    public partial class SiteMaster : MasterPage
    {
        public class NavItem
        {
            public string Url { get; set; }
            public string Icon { get; set; }
            public string Label { get; set; }
            public bool Active { get; set; }
        }

        // Pages that belong to a menu entry without being listed in the menu.
        private static readonly Dictionary<string, string> NavAliases =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "~/QuizDetail.aspx", "~/Quizzes.aspx" },
                { "~/TakeQuiz.aspx", "~/Quizzes.aspx" },
                { "~/Result.aspx", "~/History.aspx" },
                { "~/Admin/QuizEdit.aspx", "~/Admin/Quizzes.aspx" },
                { "~/Admin/GenerateQuiz.aspx", "~/Admin/Quizzes.aspx" },
                { "~/Admin/Questions.aspx", "~/Admin/Quizzes.aspx" },
                { "~/Admin/QuestionEdit.aspx", "~/Admin/Quizzes.aspx" },
                { "~/Admin/UserAttempts.aspx", "~/Admin/Users.aspx" }
            };

        protected void Page_Load(object sender, EventArgs e)
        {
            var user = Auth.Current;
            if (user != null)
            {
                litAvatar.Text = Fmt.Initial(user.DisplayName);
                litUserName.Text = user.DisplayName;
                litUserRole.Text = user.Role;
            }

            BindNav(user);
            ShowFlash();
        }

        private void BindNav(QuizPrincipal user)
        {
            var isAdmin = user != null && user.IsAdmin;

            var items = isAdmin
                ? new List<NavItem>
                {
                    new NavItem { Url = "~/Admin/Dashboard.aspx", Icon = "📊", Label = "Dashboard" },
                    new NavItem { Url = "~/Admin/Quizzes.aspx", Icon = "📚", Label = "Manage Quizzes" },
                    new NavItem { Url = "~/Admin/Users.aspx", Icon = "👥", Label = "Manage Users" },
                    new NavItem { Url = "~/Leaderboard.aspx", Icon = "🏆", Label = "Leaderboard" },
                    new NavItem { Url = "~/Quizzes.aspx", Icon = "🔍", Label = "Browse Quizzes" }
                }
                : new List<NavItem>
                {
                    new NavItem { Url = "~/Dashboard.aspx", Icon = "📊", Label = "Dashboard" },
                    new NavItem { Url = "~/Quizzes.aspx", Icon = "📚", Label = "Browse Quizzes" },
                    new NavItem { Url = "~/History.aspx", Icon = "📋", Label = "My Attempts" },
                    new NavItem { Url = "~/Leaderboard.aspx", Icon = "🏆", Label = "Leaderboard" },
                    new NavItem { Url = "~/Bookmarks.aspx", Icon = "⭐", Label = "Bookmarks" }
                };

            var current = Request.AppRelativeCurrentExecutionFilePath ?? "";
            string alias;
            if (NavAliases.TryGetValue(current, out alias)) current = alias;

            foreach (var item in items)
                item.Active = string.Equals(item.Url, current, StringComparison.OrdinalIgnoreCase);

            rptNav.DataSource = items;
            rptNav.DataBind();
        }

        private void ShowFlash()
        {
            if (Session == null) return;
            var flash = Session["qm_flash"] as string;
            if (string.IsNullOrEmpty(flash)) return;

            Session.Remove("qm_flash");
            var parts = flash.Split(new[] { '|' }, 2);
            BasePage.ShowToast(Page, parts.Length > 1 ? parts[1] : flash, parts[0]);
        }

        protected void SignOut_Click(object sender, EventArgs e)
        {
            Auth.SignOut();
            Response.Redirect("~/Login.aspx", true);
        }
    }
}
