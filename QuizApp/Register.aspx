<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="QuizApp.Register" %>

<!DOCTYPE html>
<html lang="en" data-theme="light">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Create Account — QuizMaster</title>
    <script>
        try { if (localStorage.getItem('qm_theme') === 'dark') document.documentElement.setAttribute('data-theme', 'dark'); } catch (e) { }
    </script>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&amp;family=JetBrains+Mono:wght@400;600&amp;display=swap" rel="stylesheet" />
    <link href="~/Content/site.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server" defaultbutton="btnRegister">
        <div class="auth-container">
            <div class="auth-box">
                <div class="auth-logo">
                    <span class="auth-logo-icon">🧠</span>
                    <div class="auth-logo-text">QuizMaster</div>
                    <div class="auth-logo-sub">Test your knowledge. Track your growth.</div>
                </div>

                <asp:PlaceHolder ID="phError" runat="server" Visible="false">
                    <div class="alert alert-error"><asp:Literal ID="litError" runat="server" Mode="Encode" /></div>
                </asp:PlaceHolder>

                <h2>Create Account</h2>

                <!-- Student / Admin toggle -->
                <div class="input-group">
                    <label>Register as</label>
                    <asp:RadioButtonList ID="rblRole" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow"
                        CssClass="role-toggle" onclick="qmRoleChanged()">
                        <asp:ListItem Value="User" Selected="True">🎓 Student</asp:ListItem>
                        <asp:ListItem Value="Admin">🛡️ Admin</asp:ListItem>
                    </asp:RadioButtonList>
                </div>
                <div class="input-group">
                    <label for="<%= txtName.ClientID %>">Full Name</label>
                    <asp:TextBox ID="txtName" runat="server" placeholder="Your name" MaxLength="100" />
                </div>
                <div class="input-group">
                    <label for="<%= txtEmail.ClientID %>">Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="you@example.com" MaxLength="200" />
                </div>
                <div class="input-group" id="sectionBox">
                    <label for="<%= ddlSection.ClientID %>">Section</label>
                    <asp:DropDownList ID="ddlSection" runat="server">
                        <asp:ListItem Value="">-- Select your section --</asp:ListItem>
                        <asp:ListItem Value="A">Section A</asp:ListItem>
                        <asp:ListItem Value="B">Section B</asp:ListItem>
                        <asp:ListItem Value="C">Section C</asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div class="input-group">
                    <label for="<%= txtPassword.ClientID %>">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Min 6 characters" />
                </div>

                <asp:Button ID="btnRegister" runat="server" Text="Create Account" CssClass="btn btn-primary"
                    Style="width: 100%; justify-content: center" OnClick="Register_Click" />

                <p style="text-align: center; margin-top: 16px; font-size: .85rem">
                    Have an account? <a href="Login.aspx" style="font-weight: 600">Sign In</a>
                </p>
            </div>
        </div>
    </form>
    <script>
        // Show the Section field only when registering as a Student
        function qmRoleChanged() {
            var admin = document.querySelector('#<%= rblRole.ClientID %> input[value="Admin"]');
            document.getElementById('sectionBox').style.display = admin && admin.checked ? 'none' : 'block';
        }
        qmRoleChanged();
    </script>
</body>
</html>
