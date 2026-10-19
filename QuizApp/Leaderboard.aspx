<%@ Page Title="Leaderboard — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Leaderboard.aspx.cs" Inherits="QuizApp.Leaderboard" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Leaderboard</h1>
    <p>Top performers across all quizzes</p>

    <div class="card mt-16">
        <h3 style="margin-bottom: 16px">🏆 Top Performers</h3>
        <asp:Repeater ID="rptBoard" runat="server" EnableViewState="false">
            <ItemTemplate>
                <div class="lb-row<%# (bool)Eval("IsCurrentUser") ? " current-user" : "" %>">
                    <div class="lb-rank <%# MedalClass((int)Eval("Rank")) %>"><%# MedalText((int)Eval("Rank")) %></div>
                    <div class="lb-name">
                        <%# Fmt.H(Eval("UserName")) %>
                        <%# (bool)Eval("IsCurrentUser") ? "<span style='font-size:.75rem;color:var(--muted)'>(You)</span>" : "" %>
                    </div>
                    <div style="text-align: right">
                        <div class="lb-score"><%# Fmt.Num(Eval("AverageScore")) %>%</div>
                        <div style="font-size: .72rem; color: var(--muted)"><%# Eval("TotalAttempts") %> attempts · <%# Eval("PassedCount") %> passed · best <%# Fmt.Num(Eval("HighestScore")) %>%</div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:PlaceHolder ID="phEmpty" runat="server" Visible="false">
            <p style="text-align: center; padding: 20px; color: var(--muted)">No entries yet</p>
        </asp:PlaceHolder>
    </div>
</asp:Content>
