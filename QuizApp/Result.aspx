<%@ Page Title="Result — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Result.aspx.cs" Inherits="QuizApp.ResultPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <asp:PlaceHolder ID="phNotFound" runat="server" Visible="false">
        <a class="btn btn-outline btn-sm mb-16" href="Quizzes.aspx">← Browse Quizzes</a>
        <div class="card empty">Result not found.</div>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phResult" runat="server">
        <div class="flex gap-8 mb-16" style="flex-wrap: wrap">
            <a class="btn btn-outline btn-sm" href="<%: ResolveUrl(BackUrl) %>"><%: BackText %></a>
        </div>

        <div class="card result-header">
            <div class="result-score-ring <%: Result.Passed ? "passed" : "failed" %>">
                <div class="result-score-value"><%: Fmt.Num(Result.Percentage) %>%</div>
                <div class="result-score-label">Score</div>
            </div>
            <h1><%: Result.QuizTitle %></h1>
            <% if (!string.IsNullOrEmpty(StudentName)) { %>
            <p>Attempt by <strong><%: StudentName %></strong></p>
            <% } %>
            <span class="badge <%: Result.Passed ? "badge-green" : "badge-red" %>" style="font-size: 1rem; padding: 6px 20px">
                <%: Result.Passed ? "✅ PASSED" : "❌ FAILED" %>
            </span>

            <div class="grid-4 mt-24" style="text-align: left">
                <div class="stat-card"><div class="stat-value"><%: Fmt.Num(Result.Score) %>/<%: Result.TotalMarks %></div><div class="stat-label">Score</div></div>
                <div class="stat-card"><div class="stat-value" style="color: var(--green)"><%: Result.CorrectAnswers %></div><div class="stat-label">Correct</div></div>
                <div class="stat-card"><div class="stat-value" style="color: var(--red)"><%: Result.WrongAnswers %></div><div class="stat-label">Wrong</div></div>
                <div class="stat-card"><div class="stat-value" style="color: var(--amber)"><%: Result.UnansweredQuestions %></div><div class="stat-label">Unanswered</div></div>
            </div>

            <div style="margin-top: 16px; color: var(--muted); font-size: .88rem">
                Time taken: <%: Fmt.Time(Result.TimeTakenSeconds) %> · Pass mark: <%: Fmt.Num(Result.PassingPercentage) %>% · <%: Fmt.DateAndTime(Result.AttemptedAt) %>
            </div>

            <div style="margin-top: 24px; display: flex; gap: 12px; justify-content: center; flex-wrap: wrap">
                <button type="button" class="btn btn-primary" onclick="var r=document.getElementById('reviewSection'); r.style.display = r.style.display === 'none' ? 'block' : 'none';">📖 Review Answers</button>
                <% if (!IsAdmin) { %>
                <a class="btn btn-outline" href="History.aspx">📋 View History</a>
                <% } %>
            </div>
        </div>

        <div id="reviewSection" style="display: none; margin-top: 16px">
            <h2>Answer Review</h2>
            <asp:Repeater ID="rptReview" runat="server">
                <ItemTemplate>
                    <div class="review-item <%# ReviewHtml.ReviewClass(Container.DataItem) %>">
                        <div class="flex-between">
                            <span style="font-weight: 700; font-size: .85rem">Q<%# Eval("Number") %></span>
                            <%# ReviewHtml.ReviewBadge(Container.DataItem) %>
                        </div>
                        <p style="margin-top: 8px; font-weight: 600; color: var(--text); white-space: pre-wrap"><%# Fmt.H(Eval("QuestionText")) %></p>
                        <%# ReviewHtml.ForReview(Container.DataItem) %>
                        <asp:PlaceHolder ID="phExplain" runat="server" Visible='<%# !string.IsNullOrEmpty((string)Eval("Explanation")) %>'>
                            <div class="explain">💡 <%# Fmt.H(Eval("Explanation")) %></div>
                        </asp:PlaceHolder>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </asp:PlaceHolder>
</asp:Content>
