<%@ Page Title="Quiz — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="QuizDetail.aspx.cs" Inherits="QuizApp.QuizDetailPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <a class="btn btn-outline btn-sm mb-16" href="Quizzes.aspx">← Back to Quizzes</a>

    <asp:PlaceHolder ID="phNotFound" runat="server" Visible="false">
        <div class="card empty">Quiz not found or no longer available.</div>
    </asp:PlaceHolder>

    <asp:PlaceHolder ID="phQuiz" runat="server">
        <div class="card">
            <div class="flex-between mb-16">
                <div>
                    <span class="badge <%: Fmt.DiffBadge(Quiz.Difficulty) %>"><%: Quiz.Difficulty %></span>
                    <span style="font-size: .85rem; color: var(--muted); margin-left: 8px"><%: Quiz.CategoryName %></span>
                </div>
                <% if (!Quiz.IsActive) { %><span class="badge badge-red">Inactive</span><% } %>
            </div>

            <h1><%: Quiz.Title %></h1>
            <p><%: Quiz.Description %></p>

            <div class="grid-4 mt-24">
                <div class="stat-card"><div class="stat-value"><%: Quiz.DurationMinutes %></div><div class="stat-label">Minutes</div></div>
                <div class="stat-card"><div class="stat-value"><%: QuestionsPerAttempt %></div><div class="stat-label">Questions</div></div>
                <div class="stat-card"><div class="stat-value"><%: Quiz.TotalMarks %></div><div class="stat-label">Total Marks</div></div>
                <div class="stat-card"><div class="stat-value"><%: Fmt.Num(Quiz.PassingPercentage) %>%</div><div class="stat-label">Pass Mark</div></div>
            </div>

            <div style="margin-top: 20px; padding: 16px; background: var(--bg); border-radius: 8px">
                <% if (Quiz.NegativeMarking) { %>
                <p style="color: var(--red); font-size: .88rem; margin-bottom: 4px">⚠️ Negative marking: -<%: Fmt.Num(Quiz.NegativeMarkValue) %> per wrong answer</p>
                <% } %>
                <% if (Quiz.RandomizeQuestions) { %>
                <p style="font-size: .88rem; margin-bottom: 4px">🔀 Questions are randomized</p>
                <% } %>
                <% if (Quiz.RandomizeOptions) { %>
                <p style="font-size: .88rem; margin-bottom: 4px">🔀 Answer options are shuffled</p>
                <% } %>
                <p style="font-size: .88rem; margin-bottom: 4px">⏱ The timer starts when you press Start and the quiz is submitted automatically when time runs out.</p>
                <p style="font-size: .88rem; margin-bottom: 0">Created by <%: Quiz.CreatedByName %></p>
            </div>

            <div style="margin-top: 24px; display: flex; gap: 12px; flex-wrap: wrap; align-items: center">
                <asp:Button ID="btnStart" runat="server" Text="🚀 Start Quiz" CssClass="btn btn-primary" OnClick="Start_Click" />
                <asp:PlaceHolder ID="phCannotStart" runat="server" Visible="false">
                    <span class="badge badge-red" style="font-size: .9rem; padding: 10px 20px"><asp:Literal ID="litCannotStart" runat="server" Mode="Encode" /></span>
                </asp:PlaceHolder>
                <a class="btn btn-outline" href="Quizzes.aspx">← Back</a>
                <% if (IsAdmin) { %>
                <a class="btn btn-outline" href="Admin/QuizEdit.aspx?id=<%: Quiz.Id %>">✏️ Edit Quiz</a>
                <% } %>
            </div>
        </div>
    </asp:PlaceHolder>
</asp:Content>
