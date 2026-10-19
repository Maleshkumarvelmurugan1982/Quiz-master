using System;
using QuizApp.Code;

namespace QuizApp
{
    public partial class Leaderboard : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var board = AttemptService.GetLeaderboard(UserId);
            rptBoard.DataSource = board;
            rptBoard.DataBind();
            phEmpty.Visible = board.Count == 0;
        }

        protected string MedalClass(int rank)
        {
            return rank == 1 ? "gold" : rank == 2 ? "silver" : rank == 3 ? "bronze" : "";
        }

        protected string MedalText(int rank)
        {
            return rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : "#" + rank;
        }
    }
}
