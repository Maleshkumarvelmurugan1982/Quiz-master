<%@ Page Title="Manage Quizzes — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Quizzes.aspx.cs" Inherits="QuizApp.Admin.AdminQuizzesPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <h1>Manage Quizzes</h1>
            <p>Create, edit and manage quizzes</p>
        </div>
        <div class="flex gap-8" style="flex-wrap: wrap">
            <a class="btn btn-ai" href="GenerateQuiz.aspx">✨ Generate Quiz with AI</a>
            <a class="btn btn-outline" href="QuizEdit.aspx">+ New Quiz (manual)</a>
        </div>
    </div>

    <div class="card">
        <div class="table-wrap">
            <asp:GridView ID="gvQuizzes" runat="server" AutoGenerateColumns="false" CssClass="grid"
                GridLines="None" CellSpacing="0" DataKeyNames="Id" OnRowCommand="Quizzes_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="Title">
                        <ItemTemplate>
                            <a href="../QuizDetail.aspx?id=<%# Eval("Id") %>" style="font-weight: 600; color: var(--text)"><%# Fmt.H(Eval("Title")) %></a>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Category">
                        <ItemTemplate><%# Fmt.H(Eval("CategoryName")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Difficulty">
                        <ItemTemplate><span class="badge <%# Fmt.DiffBadge(Eval("Difficulty")) %>"><%# Fmt.H(Eval("Difficulty")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Questions" DataField="QuestionCount" />
                    <asp:BoundField HeaderText="Attempts" DataField="TotalAttempts" />
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class="badge <%# Fmt.ActiveBadge(Eval("IsActive")) %>"><%# (bool)Eval("IsActive") ? "Active" : "Inactive" %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="actions">
                                <a class="btn btn-outline btn-sm" href="Questions.aspx?quizId=<%# Eval("Id") %>">Questions</a>
                                <a class="btn btn-outline btn-sm" href="QuizEdit.aspx?id=<%# Eval("Id") %>">Edit</a>
                                <asp:LinkButton ID="lnkToggle" runat="server" CssClass="btn btn-outline btn-sm"
                                    CommandName="ToggleQuiz" CommandArgument='<%# Eval("Id") %>'
                                    Text='<%# (bool)Eval("IsActive") ? "Deactivate" : "Activate" %>' />
                                <asp:LinkButton ID="lnkDelete" runat="server" CssClass="btn btn-danger btn-sm"
                                    CommandName="DeleteQuiz" CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return qmConfirm(this, 'Delete Quiz?', 'This deletes the quiz, all its questions and all attempts at it.', 'Delete');">Delete</asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <p class="empty">No quizzes yet. Use <strong>✨ Generate Quiz with AI</strong> or <strong>+ New Quiz</strong> to create one.</p>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
