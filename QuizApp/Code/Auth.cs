using System;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace QuizApp.Code
{
    /// <summary>
    /// The logged-in user, rebuilt from the Forms Authentication cookie on every request
    /// (see Global.asax). Replaces the JWT token used by the old project.
    /// </summary>
    public class QuizPrincipal : GenericPrincipal
    {
        public int UserId { get; private set; }
        public string DisplayName { get; private set; }
        public string Role { get; private set; }
        public bool IsAdmin { get { return Role == "Admin"; } }

        public QuizPrincipal(IIdentity identity, int userId, string name, string role)
            : base(identity, new[] { role })
        {
            UserId = userId;
            DisplayName = name;
            Role = role;
        }
    }

    public static class Auth
    {
        /// <summary>Writes the login cookie. UserData holds "id|role|name".</summary>
        public static void SignIn(AppUser user, bool remember)
        {
            var timeout = remember ? TimeSpan.FromDays(7) : FormsAuthentication.Timeout;
            var userData = user.Id + "|" + user.Role + "|" + (user.Name ?? "").Replace("|", " ");

            var ticket = new FormsAuthenticationTicket(
                2,
                user.Email,
                DateTime.Now,
                DateTime.Now.Add(timeout),
                remember,
                userData,
                FormsAuthentication.FormsCookiePath);

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
            {
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath,
                Secure = FormsAuthentication.RequireSSL
            };
            if (remember) cookie.Expires = ticket.Expiration;

            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
            var ctx = HttpContext.Current;
            if (ctx.Session != null) ctx.Session.Abandon();
        }

        /// <summary>Called from Global.asax PostAuthenticateRequest.</summary>
        public static void AttachPrincipal(HttpContext ctx)
        {
            var cookie = ctx.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie == null || string.IsNullOrEmpty(cookie.Value)) return;

            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(cookie.Value);
            }
            catch
            {
                return;
            }
            if (ticket == null || ticket.Expired) return;

            var parts = (ticket.UserData ?? "").Split(new[] { '|' }, 3);
            int id;
            if (parts.Length < 3 || !int.TryParse(parts[0], out id)) return;

            var principal = new QuizPrincipal(new FormsIdentity(ticket), id, parts[2], parts[1]);
            ctx.User = principal;
            System.Threading.Thread.CurrentPrincipal = principal;
        }

        public static QuizPrincipal Current
        {
            get
            {
                var ctx = HttpContext.Current;
                return ctx == null ? null : ctx.User as QuizPrincipal;
            }
        }

        public static string HomeUrl(QuizPrincipal user)
        {
            if (user == null) return "~/Login.aspx";
            return user.IsAdmin ? "~/Admin/Dashboard.aspx" : "~/Dashboard.aspx";
        }
    }
}
