<%@ Page Title="User Attempts — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="UserAttempts.aspx.cs" Inherits="QuizApp.Admin.UserAttemptsPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <a class="btn btn-outline btn-sm mb-16" href="Users.aspx">← Back to Users</a>
    <h1><asp:Literal ID="litName" runat="server" Mode="Encode" />'s Attempt History</h1>
    <p><asp:Literal ID="litEmail" runat="server" Mode="Encode" /></p>

    <div class="card mt-16">
        <div class="table-wrap">
            <asp:GridView ID="gvAttempts" runat="server" AutoGenerateColumns="false" CssClass="grid"
                GridLines="None" CellSpacing="0" EnableViewState="false">
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
                        <ItemTemplate><span style="font-size: .8rem"><%# Fmt.Date(Eval("AttemptedAt")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:HyperLinkField Text="View" DataNavigateUrlFields="AttemptId"
                        DataNavigateUrlFormatString="~/Result.aspx?id={0}" ControlStyle-CssClass="btn btn-outline btn-sm" />
                </Columns>
                <EmptyDataTemplate><p class="empty">No attempts yet.</p></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
