<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="QuizApp.Login" %>

<!DOCTYPE html>
<html lang="en" data-theme="light">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Sign In — QuizMaster</title>
    <script>
        try { if (localStorage.getItem('qm_theme') === 'dark') document.documentElement.setAttribute('data-theme', 'dark'); } catch (e) { }
    </script>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&amp;family=JetBrains+Mono:wght@400;600&amp;display=swap" rel="stylesheet" />
    <link href="~/Content/site.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server" defaultbutton="btnLogin">
        <div class="auth-container">
            <div class="auth-box">
                <div class="auth-logo">
                    <span class="auth-logo-icon">🧠</span>
                    <div class="auth-logo-text">QuizMaster</div>
                    <div class="auth-logo-sub">Test your knowledge. Track your growth.</div>
                </div>

                <asp:PlaceHolder ID="phDbError" runat="server" Visible="false">
                    <div class="alert alert-error">
                        <strong>Database problem</strong><br />
                        <asp:Literal ID="litDbError" runat="server" Mode="Encode" />
                    </div>
                </asp:PlaceHolder>

                <asp:PlaceHolder ID="phError" runat="server" Visible="false">
                    <div class="alert alert-error"><asp:Literal ID="litError" runat="server" Mode="Encode" /></div>
                </asp:PlaceHolder>

                <h2>Sign In</h2>
                <div class="input-group">
                    <label for="<%= txtEmail.ClientID %>">Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="you@example.com" MaxLength="200" />
                </div>
                <div class="input-group">
                    <label for="<%= txtPassword.ClientID %>">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Your password" />
                </div>
                <div class="input-group check-row">
                    <asp:CheckBox ID="chkRemember" runat="server" Text="Keep me signed in" />
                </div>

                <asp:Button ID="btnLogin" runat="server" Text="Sign In" CssClass="btn btn-primary"
                    Style="width: 100%; justify-content: center" OnClick="Login_Click" />

                <p style="text-align: center; margin-top: 16px; font-size: .85rem">
                    No account? <a href="Register.aspx" style="font-weight: 600">Register</a>
                </p>
                <p style="text-align: center; font-size: .75rem; margin-top: 8px; color: var(--muted)">
                    Demo: admin@quizapp.com / Admin@123
                </p>
            </div>
        </div>
    </form>
</body>
</html>
