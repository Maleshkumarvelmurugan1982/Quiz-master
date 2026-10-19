using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using QuizApp.Code;

namespace QuizApp.Controls
{
    /// <summary>
    /// Editable list of questions (add / remove / edit). The list is kept in ViewState
    /// between postbacks; what the admin typed is read back from the form before every change.
    /// </summary>
    public partial class QuestionEditor : UserControl
    {
        private List<QuestionDraft> Items
        {
            get
            {
                var list = ViewState["items"] as List<QuestionDraft>;
                if (list == null)
                {
                    list = new List<QuestionDraft>();
                    ViewState["items"] = list;
                }
                return list;
            }
            set { ViewState["items"] = value ?? new List<QuestionDraft>(); }
        }

        /// <summary>Gets the questions as currently typed, or loads a new set into the editor.</summary>
        public List<QuestionDraft> Questions
        {
            get
            {
                SyncFromUi();
                return Items;
            }
            set
            {
                Items = value;
                Rebind();
            }
        }

        public int Count
        {
            get { return Items.Count; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && rptQuestions.Items.Count == 0 && Items.Count == 0)
                Rebind();
        }

        public void AddBlank()
        {
            SyncFromUi();
            var list = Items;
            list.Add(new QuestionDraft());
            Items = list;
            Rebind();
        }

        protected void Add_Click(object sender, EventArgs e)
        {
            AddBlank();
        }

        protected void Questions_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "RemoveQuestion") return;

            SyncFromUi();
            int index;
            var list = Items;
            if (int.TryParse(Convert.ToString(e.CommandArgument), out index) && index >= 0 && index < list.Count)
                list.RemoveAt(index);
            Items = list;
            Rebind();
        }

        protected void Questions_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;

            var q = (QuestionDraft)e.Item.DataItem;
            ((HiddenField)e.Item.FindControl("hfId")).Value = q.Id.ToString();
            ((TextBox)e.Item.FindControl("txtText")).Text = q.QuestionText;
            ((TextBox)e.Item.FindControl("txtA")).Text = q.OptionA;
            ((TextBox)e.Item.FindControl("txtB")).Text = q.OptionB;
            ((TextBox)e.Item.FindControl("txtC")).Text = q.OptionC;
            ((TextBox)e.Item.FindControl("txtD")).Text = q.OptionD;
            ((TextBox)e.Item.FindControl("txtMarks")).Text = q.Marks.ToString();
            ((TextBox)e.Item.FindControl("txtExplanation")).Text = q.Explanation;

            var rbl = (RadioButtonList)e.Item.FindControl("rblCorrect");
            rbl.ClearSelection();
            var correct = rbl.Items.FindByValue(q.CorrectOption ?? "A");
            (correct ?? rbl.Items[0]).Selected = true;

            var ddl = (DropDownList)e.Item.FindControl("ddlDifficulty");
            ddl.ClearSelection();
            var diff = ddl.Items.FindByValue(q.Difficulty ?? "Medium");
            if (diff != null) diff.Selected = true;
        }

        /// <summary>Copies the values typed in the form back into the list.</summary>
        private void SyncFromUi()
        {
            var list = Items;
            if (rptQuestions.Items.Count != list.Count) return;

            for (int i = 0; i < rptQuestions.Items.Count; i++)
            {
                var item = rptQuestions.Items[i];
                var q = list[i];

                int id;
                q.Id = int.TryParse(((HiddenField)item.FindControl("hfId")).Value, out id) ? id : 0;
                q.QuestionText = ((TextBox)item.FindControl("txtText")).Text;
                q.OptionA = ((TextBox)item.FindControl("txtA")).Text;
                q.OptionB = ((TextBox)item.FindControl("txtB")).Text;
                q.OptionC = ((TextBox)item.FindControl("txtC")).Text;
                q.OptionD = ((TextBox)item.FindControl("txtD")).Text;
                q.Explanation = ((TextBox)item.FindControl("txtExplanation")).Text;

                int marks;
                q.Marks = int.TryParse(((TextBox)item.FindControl("txtMarks")).Text, out marks) ? marks : 1;

                var correct = ((RadioButtonList)item.FindControl("rblCorrect")).SelectedValue;
                q.CorrectOption = string.IsNullOrEmpty(correct) ? "A" : correct;
                q.Difficulty = ((DropDownList)item.FindControl("ddlDifficulty")).SelectedValue;
            }
            Items = list;
        }

        private void Rebind()
        {
            var list = Items;
            rptQuestions.DataSource = list;
            rptQuestions.DataBind();
            litCount.Text = list.Count.ToString();
            phEmpty.Visible = list.Count == 0;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            litCount.Text = Items.Count.ToString();
            phEmpty.Visible = Items.Count == 0;
        }
    }
}
