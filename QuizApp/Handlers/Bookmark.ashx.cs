using System.Web;
using QuizApp.Code;

namespace QuizApp.Handlers
{
    /// <summary>
    /// AJAX endpoint used while taking a quiz to add/remove a bookmark without leaving the page.
    /// POST questionId=..&amp;action=add|remove  ->  {"bookmarked":true|false}
    /// </summary>
    public class BookmarkHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);

            var user = Auth.Current;
            if (user == null)
            {
                context.Response.StatusCode = 401;
                context.Response.Write("{\"message\":\"Please sign in again.\"}");
                return;
            }

            if (context.Request.HttpMethod != "POST")
            {
                context.Response.StatusCode = 405;
                context.Response.Write("{\"message\":\"POST only.\"}");
                return;
            }

            int questionId;
            if (!int.TryParse(context.Request.Form["questionId"], out questionId) || questionId <= 0)
            {
                context.Response.StatusCode = 400;
                context.Response.Write("{\"message\":\"Invalid question.\"}");
                return;
            }

            var add = context.Request.Form["action"] != "remove";
            if (add)
                QuestionService.Bookmark(user.UserId, questionId);
            else
                QuestionService.Unbookmark(user.UserId, questionId);

            context.Response.Write("{\"bookmarked\":" + (add ? "true" : "false") + "}");
        }
    }
}
