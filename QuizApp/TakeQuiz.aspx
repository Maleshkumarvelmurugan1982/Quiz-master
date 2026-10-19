<%@ Page Title="Taking Quiz — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="TakeQuiz.aspx.cs" Inherits="QuizApp.TakeQuiz" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="quiz-header">
        <div>
            <div style="font-weight: 700"><asp:Literal ID="litTitle" runat="server" Mode="Encode" /></div>
            <div style="font-size: .8rem; color: var(--muted)">Q<span id="qmCurrentNo">1</span> of <asp:Literal ID="litTotal" runat="server" /></div>
        </div>
        <div class="timer" id="timerDisplay">--:--</div>
        <button type="button" class="btn btn-danger btn-sm" onclick="qmTake.confirmSubmit()">Submit Quiz</button>
    </div>

    <div class="quiz-wrap">
        <!-- Question navigation -->
        <div class="question-nav">
            <asp:Repeater ID="rptNav" runat="server" EnableViewState="false">
                <ItemTemplate>
                    <button type="button" class="q-nav-btn<%# (bool)Eval("IsBookmarked") ? " bookmarked" : "" %>"
                        data-index="<%# Eval("Index") %>" onclick="qmTake.show(<%# Eval("Index") %>)"><%# (int)Eval("Index") + 1 %></button>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <!-- Progress -->
        <div style="margin-bottom: 20px">
            <div class="flex-between text-sm text-muted mb-16">
                <span><span id="qmAnswered">0</span> of <%: QuestionCount %> answered</span>
                <span id="qmMarksInfo"></span>
            </div>
            <div class="progress-bar"><div class="progress-fill" id="qmProgress" style="width: 0%"></div></div>
        </div>

        <!-- Questions (one is visible at a time) -->
        <asp:Repeater ID="rptQuestions" runat="server" EnableViewState="false">
            <ItemTemplate>
                <div class="card q-block" data-index="<%# Eval("Index") %>" data-qid="<%# Eval("Id") %>"
                    data-info="<%# Fmt.H(Eval("Marks") + " mark" + ((int)Eval("Marks") > 1 ? "s" : "") + " · " + Eval("Difficulty")) %>">
                    <div style="margin-bottom: 20px">
                        <div style="font-size: .75rem; font-weight: 600; color: var(--muted); text-transform: uppercase; letter-spacing: .06em; margin-bottom: 8px">
                            Question <%# (int)Eval("Index") + 1 %>
                        </div>
                        <div style="font-size: 1.05rem; font-weight: 600; line-height: 1.5; white-space: pre-wrap"><%# Fmt.H(Eval("QuestionText")) %></div>
                    </div>

                    <asp:Repeater ID="rptOptions" runat="server" DataSource='<%# Eval("Options") %>'>
                        <ItemTemplate>
                            <label class="option-btn">
                                <input type="radio" name="ans_<%# Eval("QuestionId") %>" value="<%# Fmt.H(Eval("Value")) %>" />
                                <span class="option-label"><%# Fmt.H(Eval("DisplayLetter")) %></span>
                                <span><%# Fmt.H(Eval("Text")) %></span>
                            </label>
                        </ItemTemplate>
                    </asp:Repeater>

                    <div style="display: flex; justify-content: space-between; gap: 8px; margin-top: 16px; flex-wrap: wrap">
                        <button type="button" class="btn btn-outline btn-sm" onclick="qmTake.prev()">← Previous</button>
                        <button type="button" class="btn btn-outline btn-sm qm-bookmark" data-qid="<%# Eval("Id") %>"
                            data-bookmarked="<%# (bool)Eval("IsBookmarked") ? "1" : "0" %>" onclick="qmTake.bookmark(this)">
                            ⭐ <%# (bool)Eval("IsBookmarked") ? "Bookmarked" : "Bookmark" %>
                        </button>
                        <button type="button" class="btn btn-outline btn-sm" onclick="qmTake.next()">Next →</button>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <!-- The real (server) submit button; clicked by the confirm dialog or the timer -->
        <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="Submit_Click" Style="display: none" UseSubmitBehavior="true" />
    </div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
        var QM_TAKE = {
            remainingSeconds: <%= RemainingSeconds %>,
            submitButtonId: '<%= btnSubmit.ClientID %>',
            bookmarkUrl: '<%= ResolveUrl("~/Handlers/Bookmark.ashx") %>',
            storageKey: 'qm_answers_<%= AttemptId %>'
        };
    </script>
    <script src="<%: ResolveUrl("~/Scripts/takequiz.js") %>"></script>
</asp:Content>
