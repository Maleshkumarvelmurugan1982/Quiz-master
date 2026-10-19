<%@ Page Title="Manage Users — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Users.aspx.cs" Inherits="QuizApp.Admin.AdminUsersPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Manage Users</h1>
    <p>View students by section and manage their accounts</p>

    <!-- Section summary: click a card to filter -->
    <div class="grid-4 mt-16">
        <asp:Repeater ID="rptSections" runat="server" OnItemCommand="Sections_ItemCommand">
            <ItemTemplate>
                <asp:LinkButton ID="lnkSection" runat="server" CommandName="FilterSection" CommandArgument='<%# Eval("Value") %>'
                    CssClass='<%# "stat-card section-card" + ((bool)Eval("Active") ? " active" : "") %>'>
                    <div class="stat-value"><%# Eval("Count") %></div>
                    <div class="stat-label"><%# Fmt.H(Eval("Label")) %></div>
                </asp:LinkButton>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:Panel ID="pnlSearch" runat="server" CssClass="filter-bar mt-24" DefaultButton="btnSearch">
        <asp:TextBox ID="txtSearch" runat="server" placeholder="🔍 Search by name or email..." />
        <asp:DropDownList ID="ddlSectionFilter" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Filter_Changed">
            <asp:ListItem Value="">All Sections</asp:ListItem>
            <asp:ListItem Value="A">Section A</asp:ListItem>
            <asp:ListItem Value="B">Section B</asp:ListItem>
            <asp:ListItem Value="C">Section C</asp:ListItem>
            <asp:ListItem Value="NONE">No section</asp:ListItem>
        </asp:DropDownList>
        <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary btn-sm" OnClick="Filter_Changed" />
        <span class="text-sm text-muted"><asp:Literal ID="litShowing" runat="server" Mode="Encode" /></span>
    </asp:Panel>

    <div class="card">
        <div class="table-wrap">
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" CssClass="grid"
                GridLines="None" CellSpacing="0" DataKeyNames="Id" OnRowCommand="Users_RowCommand"
                OnRowDataBound="Users_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate><strong><%# Fmt.H(Eval("Name")) %></strong></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Email">
                        <ItemTemplate><span style="font-size: .85rem; color: var(--muted)"><%# Fmt.H(Eval("Email")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Section">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlRowSection" runat="server" AutoPostBack="true"
                                OnSelectedIndexChanged="RowSection_Changed" CssClass="section-select">
                                <asp:ListItem Value="">—</asp:ListItem>
                                <asp:ListItem>A</asp:ListItem>
                                <asp:ListItem>B</asp:ListItem>
                                <asp:ListItem>C</asp:ListItem>
                            </asp:DropDownList>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField HeaderText="Attempts" DataField="TotalAttempts" />
                    <asp:TemplateField HeaderText="Avg Score">
                        <ItemTemplate><span class="font-mono"><%# Fmt.Round(Eval("AverageScore")) %>%</span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <span class="badge <%# Fmt.ActiveBadge(Eval("IsActive")) %>"><%# (bool)Eval("IsActive") ? "Active" : "Disabled" %></span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Joined">
                        <ItemTemplate><span style="font-size: .8rem; color: var(--muted)"><%# Fmt.Date(Eval("CreatedAt")) %></span></ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <div class="actions">
                                <a class="btn btn-outline btn-sm" href="UserAttempts.aspx?id=<%# Eval("Id") %>">Attempts</a>
                                <asp:LinkButton ID="lnkToggle" runat="server" CssClass="btn btn-outline btn-sm"
                                    CommandName="ToggleUser" CommandArgument='<%# Eval("Id") %>'
                                    Text='<%# (bool)Eval("IsActive") ? "Disable" : "Enable" %>' />
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate><p class="empty">No students found.</p></EmptyDataTemplate>
            </asp:GridView>
        </div>
    </div>
</asp:Content>
