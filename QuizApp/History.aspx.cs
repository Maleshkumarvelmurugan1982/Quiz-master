using System;
using System.Web.UI.WebControls;
using QuizApp.Code;

namespace QuizApp
{
    public partial class History : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            gvHistory.DataSource = AttemptService.GetHistory(UserId, 1000);
            gvHistory.DataBind();
            if (gvHistory.HeaderRow != null)
                gvHistory.HeaderRow.TableSection = TableRowSection.TableHeader;
        }
    }
}
