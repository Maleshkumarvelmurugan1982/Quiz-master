<%@ Page Title="Browse Quizzes — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Quizzes.aspx.cs" Inherits="QuizApp.Quizzes" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <h1>Browse Quizzes</h1>
            <p>Choose a quiz to test yourself</p>
        </div>
    </div>

    <asp:Panel ID="pnlFilters" runat="server" CssClass="filter-bar" DefaultButton="btnSearch">
        <asp:TextBox ID="txtSearch" runat="server" placeholder="🔍 Search quizzes..." />
        <asp:DropDownList ID="ddlCategory" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed" />
        <asp:DropDownList ID="ddlDifficulty" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
            <asp:ListItem Value="">All Difficulties</asp:ListItem>
            <asp:ListItem>Easy</asp:ListItem>
            <asp:ListItem>Medium</asp:ListItem>
            <asp:ListItem>Hard</asp:ListItem>
        </asp:DropDownList>
        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary btn-sm" OnClick="Filter_Changed" />
        <asp:LinkButton ID="lnkClear" runat="server" CssClass="btn btn-outline btn-sm" OnClick="Clear_Click">Clear</asp:LinkButton>
    </asp:Panel>

    <div class="grid-3">
        <asp:Repeater ID="rptQuizzes" runat="server">
            <ItemTemplate>
                <a class="quiz-card" href="QuizDetail.aspx?id=<%# Eval("Id") %>" style="color: inherit; text-decoration: none; display: block">
                    <div class="flex-between">
                        <span class="badge <%# Fmt.DiffBadge(Eval("Difficulty")) %>"><%# Fmt.H(Eval("Difficulty")) %></span>
                        <span style="font-size: .75rem; color: var(--muted)">
                            <%# (bool)Eval("IsActive") ? "" : "<span class='badge badge-red'>Inactive</span> " %><%# Fmt.H(Eval("CategoryName")) %>
                        </span>
                    </div>
                    <div class="quiz-title mt-16"><%# Fmt.H(Eval("Title")) %></div>
                    <div style="font-size: .82rem; color: var(--muted); margin-top: 4px; min-height: 36px"><%# Fmt.H(Fmt.Truncate(Eval("Description"), 80)) %></div>
                    <div class="quiz-meta">
                        <span class="quiz-meta-item">⏱ <%# Eval("DurationMinutes") %> min</span>
                        <span class="quiz-meta-item">❓ <%# Eval("QuestionCount") %> questions</span>
                        <span class="quiz-meta-item">⭐ <%# Eval("TotalMarks") %> marks</span>
                        <span class="quiz-meta-item">🎯 <%# Fmt.Num(Eval("PassingPercentage")) %>% to pass</span>
                    </div>
                </a>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
        <div class="card empty">No quizzes found. Try different filters.</div>
    </asp:PlaceHolder>
</asp:Content>
