<%@ Page Title="Dashboard — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs" Inherits="QuizApp.Dashboard" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <h1>Welcome back, <asp:Literal ID="litName" runat="server" Mode="Encode" />!</h1>
            <p>Here's your learning progress</p>
        </div>
        <a class="btn btn-primary" href="Quizzes.aspx">📚 Browse Quizzes</a>
    </div>

    <div class="grid-4">
        <div class="stat-card">
            <div class="stat-value"><asp:Literal ID="litAvailable" runat="server" /></div>
            <div class="stat-label">Quizzes Available</div>
        </div>
        <div class="stat-card">
            <div class="stat-value"><asp:Literal ID="litAttempted" runat="server" /></div>
            <div class="stat-label">Quizzes Attempted</div>
        </div>
        <div class="stat-card">
            <div class="stat-value"><asp:Literal ID="litAverage" runat="server" />%</div>
            <div class="stat-label">Average Score</div>
        </div>
        <div class="stat-card">
            <div class="stat-value"><asp:Literal ID="litHighest" runat="server" />%</div>
            <div class="stat-label">Highest Score</div>
        </div>
    </div>

    <div class="grid-2 mt-24">
        <div class="card">
            <h3>Recent Attempts</h3>
            <asp:Repeater ID="rptRecent" runat="server">
                <ItemTemplate>
                    <a class="list-row" href="Result.aspx?id=<%# Eval("AttemptId") %>" style="color: inherit; text-decoration: none">
                        <div>
                            <div style="font-weight: 600; font-size: .88rem"><%# Fmt.H(Eval("QuizTitle")) %></div>
                            <div style="font-size: .75rem; color: var(--muted)"><%# Fmt.H(Eval("CategoryName")) %> · <%# Fmt.Date(Eval("AttemptedAt")) %></div>
                        </div>
                        <div style="text-align: right">
                            <span class="badge <%# Fmt.StatusBadge(Eval("Status")) %>"><%# Fmt.H(Eval("Status")) %></span>
                            <div class="font-mono" style="font-size: .85rem; font-weight: 700; margin-top: 4px">
                                <%# Fmt.Num(Eval("Score")) %>/<%# Eval("TotalMarks") %>
                            </div>
                        </div>
                    </a>
                </ItemTemplate>
            </asp:Repeater>
            <asp:PlaceHolder ID="phNoRecent" runat="server" Visible="false">
                <p style="text-align: center; padding: 20px">No attempts yet. <a href="Quizzes.aspx">Take a quiz!</a></p>
            </asp:PlaceHolder>
        </div>

        <div class="card">
            <h3>Performance Chart</h3>
            <asp:PlaceHolder ID="phChart" runat="server">
                <div class="chart-bar-wrap">
                    <asp:Repeater ID="rptChart" runat="server">
                        <ItemTemplate>
                            <div class="chart-bar-col" title="<%# Fmt.H(Eval("QuizTitle")) %>">
                                <div style="font-size: .7rem; color: var(--muted); margin-bottom: 4px"><%# Fmt.Round(Eval("Percentage")) %>%</div>
                                <div class="chart-bar" style="height: <%# Fmt.BarHeight(Eval("Percentage")) %>%"></div>
                                <div class="chart-bar-label"><%# Fmt.H(Fmt.FirstWord(Eval("QuizTitle"))) %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:PlaceHolder>
            <asp:PlaceHolder ID="phNoChart" runat="server" Visible="false">
                <p style="text-align: center; padding: 20px; color: var(--muted)">Attempt quizzes to see your performance trend</p>
            </asp:PlaceHolder>
        </div>
    </div>
</asp:Content>
