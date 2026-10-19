<%@ Page Title="Manage Questions — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Questions.aspx.cs" Inherits="QuizApp.Admin.AdminQuestionsPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <a class="btn btn-outline btn-sm" href="Quizzes.aspx" style="margin-bottom: 8px">← Back</a>
            <h1>Manage Questions</h1>
            <p><asp:Literal ID="litQuizTitle" runat="server" Mode="Encode" /></p>
        </div>
        <asp:HyperLink ID="lnkNew" runat="server" CssClass="btn btn-primary">+ New Question</asp:HyperLink>
    </div>

    <div class="card">
        <div class="table-wrap">
            <asp:GridView ID="gvQuestions" runat="server" AutoGenerateColumns="false" CssClass="grid"
                GridLines="None" CellSpacing="0" OnRowCommand="Questions_RowCommand">
                <Columns>
                    <asp:TemplateField HeaderText="#">
                        <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Question">
                        <ItemTemplate>
                            <div style="max-width: 320px; font-size: .85rem"><%# Fmt.H(Fmt.Truncate(Eval("QuestionText"), 90)) %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Correct">
                        <ItemTemplate><span class="badge badge-green"><%# Fmt.H(Eval("CorrectOption")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Marks" DataField="Marks" />
                    <asp:TemplateField HeaderText="Difficulty">
                        <ItemTemplate><span class="badge <%# Fmt.DiffBadge(Eval("Difficulty")) %>"><%# Fmt.H(Eval("Difficulty")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class="badge <%# Fmt.ActiveBadge(Eval("IsActive")) %>"><%# (bool)Eval("IsActive") ? "Active" : "Hidden" %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="actions">
                                <a class="btn btn-outline btn-sm" href="QuestionEdit.aspx?id=<%# Eval("Id") %>">Edit</a>
                                <asp:LinkButton ID="lnkToggle" runat="server" CssClass="btn btn-outline btn-sm"
                                    CommandName="ToggleQuestion" CommandArgument='<%# Eval("Id") %>'
                                    Text='<%# (bool)Eval("IsActive") ? "Hide" : "Show" %>' />
                                <asp:LinkButton ID="lnkDelete" runat="server" CssClass="btn btn-danger btn-sm"
                                    CommandName="DeleteQuestion" CommandArgument='<%# Eval("Id") %>'
                                    OnClientClick="return qmConfirm(this, 'Delete Question?', 'This question will be permanently deleted.', 'Delete');">Del</asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <p class="empty">No questions yet. Add the first question!</p>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
