using System;
using System.Net;
using System.Web;
using QuizApp.Code;

namespace QuizApp
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // OpenRouter (AI generator) requires TLS 1.2+
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            // Creates QuizAppDB from App_Data/schema.sql the first time (if it does not exist).
            DbInitializer.EnsureDatabase();
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            // Turn the Forms Authentication cookie into our QuizPrincipal (user id + role).
            Auth.AttachPrincipal(Context);
        }
    }
}
