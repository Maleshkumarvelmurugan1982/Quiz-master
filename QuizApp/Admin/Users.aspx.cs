using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using QuizApp.Code;

namespace QuizApp.Admin
{
    public partial class AdminUsersPage : BasePage
    {
        public class SectionCard
        {
            public string Value { get; set; }
            public string Label { get; set; }
            public int Count { get; set; }
            public bool Active { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Allow links like Users.aspx?section=B
                var section = (Request.QueryString["section"] ?? "").ToUpperInvariant();
                if (ddlSectionFilter.Items.FindByValue(section) != null)
                    ddlSectionFilter.SelectedValue = section;
                BindAll();
            }
        }

        protected void Filter_Changed(object sender, EventArgs e)
        {
            BindAll();
        }

        protected void Sections_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "FilterSection") return;
            var value = Convert.ToString(e.CommandArgument);
            if (ddlSectionFilter.Items.FindByValue(value) != null)
                ddlSectionFilter.SelectedValue = value;
            BindAll();
        }

        protected void Users_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var user = (UserListItem)e.Row.DataItem;
            var ddl = (DropDownList)e.Row.FindControl("ddlRowSection");
            ddl.SelectedValue = UserService.IsValidSection(user.Section) ? user.Section : "";
        }

        /// <summary>Admin picked another section for a student in the grid.</summary>
        protected void RowSection_Changed(object sender, EventArgs e)
        {
            var ddl = (DropDownList)sender;
            var row = (GridViewRow)ddl.NamingContainer;
            var userId = (int)gvUsers.DataKeys[row.RowIndex].Value;

            if (UserService.SetSection(userId, ddl.SelectedValue))
                Toast("Moved to Section " + ddl.SelectedValue + ".", "success");
            else
                Toast("Please choose section A, B or C.", "error");

            BindAll();
        }

        protected void Users_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int id;
            if (e.CommandName == "ToggleUser" && int.TryParse(Convert.ToString(e.CommandArgument), out id))
            {
                string message;
                var ok = UserService.Toggle(id, UserId, out message);
                Toast(message, ok ? "success" : "error");
            }
            BindAll();
        }

        private void BindAll()
        {
            var selected = ddlSectionFilter.SelectedValue;

            // Summary cards
            var counts = UserService.CountBySection();
            var total = 0;
            foreach (var c in counts.Values) total += c;

            var cards = new List<SectionCard>
            {
                new SectionCard { Value = "", Label = "All Students", Count = total, Active = selected == "" }
            };
            foreach (var s in UserService.Sections)
                cards.Add(new SectionCard { Value = s, Label = "Section " + s, Count = counts[s], Active = selected == s });
            if (counts[""] > 0)
                cards.Add(new SectionCard { Value = "NONE", Label = "No section", Count = counts[""], Active = selected == "NONE" });

            rptSections.DataSource = cards;
            rptSections.DataBind();

            // Student list
            var users = UserService.GetUsers(txtSearch.Text, selected);
            gvUsers.DataSource = users;
            gvUsers.DataBind();

            var where = selected == "" ? "all sections" : selected == "NONE" ? "no section" : "Section " + selected;
            litShowing.Text = "Showing " + users.Count + " student(s) in " + where;
        }
    }
}
