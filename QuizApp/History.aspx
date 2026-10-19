<%@ Page Title="My Attempts — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="History.aspx.cs" Inherits="QuizApp.History" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Attempt History</h1>
    <p>All your past quiz attempts</p>

    <div class="card mt-16">
        <div class="table-wrap">
            <asp:GridView ID="gvHistory" runat="server" AutoGenerateColumns="false" CssClass="grid"
                GridLines="None" CellSpacing="0" UseAccessibleHeader="true" EnableViewState="false">
                <Columns>
                    <asp:TemplateField HeaderText="Quiz">
                        <ItemTemplate><strong><%# Fmt.H(Eval("QuizTitle")) %></strong></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Category">
                        <ItemTemplate><span class="badge badge-blue"><%# Fmt.H(Eval("CategoryName")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Score">
                        <ItemTemplate><span class="font-mono"><%# Fmt.Num(Eval("Score")) %>/<%# Eval("TotalMarks") %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="%">
                        <ItemTemplate><span class="font-mono"><%# Fmt.Num(Eval("Percentage")) %>%</span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Time">
                        <ItemTemplate><%# Fmt.Time(Eval("TimeTakenSeconds")) %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate><span class="badge <%# Fmt.StatusBadge(Eval("Status")) %>"><%# Fmt.H(Eval("Status")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Date">
                        <ItemTemplate><span style="font-size: .8rem; color: var(--muted)"><%# Fmt.Date(Eval("AttemptedAt")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:HyperLinkField Text="View" DataNavigateUrlFields="AttemptId"
                        DataNavigateUrlFormatString="~/Result.aspx?id={0}" ControlStyle-CssClass="btn btn-outline btn-sm" />
                </Columns>
                <EmptyDataTemplate>
                    <p class="empty">No attempts yet. <a href="Quizzes.aspx">Take a quiz</a> to get started!</p>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
