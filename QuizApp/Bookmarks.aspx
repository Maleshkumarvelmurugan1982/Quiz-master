<%@ Page Title="Bookmarks — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Bookmarks.aspx.cs" Inherits="QuizApp.Bookmarks" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Bookmarked Questions</h1>
    <p>Questions you've marked for review</p>

    <div class="mt-16">
        <asp:Repeater ID="rptBookmarks" runat="server" OnItemCommand="Bookmarks_ItemCommand">
            <ItemTemplate>
                <div class="card" style="margin-bottom: 12px">
                    <div class="flex-between">
                        <span style="font-weight: 700; font-size: .85rem; color: var(--muted)">Q<%# Container.ItemIndex + 1 %> · <%# Fmt.H(Eval("Difficulty")) %></span>
                        <asp:LinkButton ID="lnkRemove" runat="server" CssClass="btn btn-outline btn-sm"
                            CommandName="RemoveBookmark" CommandArgument='<%# Eval("Id") %>'>Remove ⭐</asp:LinkButton>
                    </div>
                    <p style="font-weight: 600; color: var(--text); margin-top: 8px; white-space: pre-wrap"><%# Fmt.H(Eval("QuestionText")) %></p>
                    <%# ReviewHtml.ForQuestion(Container.DataItem) %>
                    <asp:PlaceHolder ID="phExplain" runat="server" Visible='<%# !string.IsNullOrEmpty((string)Eval("Explanation")) %>'>
                        <div class="explain">💡 <%# Fmt.H(Eval("Explanation")) %></div>
                    </asp:PlaceHolder>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
            <div class="card empty">No bookmarks yet. Bookmark questions while taking a quiz!</div>
        </asp:PlaceHolder>
    </div>
</asp:Content>
