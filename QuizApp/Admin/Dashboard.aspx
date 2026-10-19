<%@ Page Title="Admin Dashboard — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Dashboard.aspx.cs" Inherits="QuizApp.Admin.AdminDashboardPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <h1>Admin Dashboard</h1>
            <p>Platform overview and statistics</p>
        </div>
        <div class="flex gap-8" style="flex-wrap: wrap">
            <a class="btn btn-ai" href="GenerateQuiz.aspx">✨ Generate Quiz with AI</a>
            <a class="btn btn-outline" href="QuizEdit.aspx">+ New Quiz</a>
        </div>
    </div>

    <div class="grid-4">
        <div class="stat-card">
            <div style="font-size: 1.5rem; margin-bottom: 8px">👥</div>
            <div class="stat-value"><asp:Literal ID="litUsers" runat="server" /></div>
            <div class="stat-label">Total Users</div>
        </div>
        <div class="stat-card">
            <div style="font-size: 1.5rem; margin-bottom: 8px">📚</div>
            <div class="stat-value"><asp:Literal ID="litQuizzes" runat="server" /></div>
            <div class="stat-label">Total Quizzes</div>
        </div>
        <div class="stat-card">
            <div style="font-size: 1.5rem; margin-bottom: 8px">❓</div>
            <div class="stat-value"><asp:Literal ID="litQuestions" runat="server" /></div>
            <div class="stat-label">Total Questions</div>
        </div>
        <div class="stat-card">
            <div style="font-size: 1.5rem; margin-bottom: 8px">📊</div>
            <div class="stat-value"><asp:Literal ID="litAttempts" runat="server" /></div>
            <div class="stat-label">Total Attempts</div>
        </div>
    </div>

    <div class="grid-2 mt-24">
        <div class="card">
            <h3>Quiz Performance</h3>
            <div class="table-wrap">
                <asp:GridView ID="gvQuizStats" runat="server" AutoGenerateColumns="false" CssClass="grid"
                    GridLines="None" CellSpacing="0" EnableViewState="false">
                    <Columns>
                        <asp:TemplateField HeaderText="Quiz">
                            <ItemTemplate><strong><%# Fmt.H(Eval("QuizTitle")) %></strong></ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField HeaderText="Attempts" DataField="TotalAttempts" />
                        <asp:TemplateField HeaderText="Avg Score">
                            <ItemTemplate><span class="font-mono"><%# Fmt.Round(Eval("AverageScore")) %>%</span></ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Pass Rate">
                            <ItemTemplate>
                                <span class="badge <%# (decimal)Eval("PassRate") >= 50 ? "badge-green" : "badge-red" %>"><%# Fmt.Round(Eval("PassRate")) %>%</span>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate><p class="empty">No attempts yet.</p></EmptyDataTemplate>
                </asp:GridView>
            </div>
        </div>

        <div class="card">
            <h3>Recent Activity</h3>
            <asp:Repeater ID="rptRecent" runat="server" EnableViewState="false">
                <ItemTemplate>
                    <div class="list-row" style="font-size: .85rem">
                        <div>
                            <strong><%# Fmt.H(Eval("UserName")) %></strong> took <em><%# Fmt.H(Eval("QuizTitle")) %></em>
                            <div style="font-size: .75rem; color: var(--muted)"><%# Fmt.DateAndTime(Eval("AttemptedAt")) %></div>
                        </div>
                        <span class="badge <%# Fmt.StatusBadge(Eval("Status")) %>"><%# Fmt.Round(Eval("Percentage")) %>%</span>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
            <asp:PlaceHolder ID="phNoRecent" runat="server" Visible="false">
                <p class="empty">No activity yet.</p>
            </asp:PlaceHolder>
        </div>
    </div>
</asp:Content>
