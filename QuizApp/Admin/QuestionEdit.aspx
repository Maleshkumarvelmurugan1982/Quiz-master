<%@ Page Title="Question — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="QuestionEdit.aspx.cs" Inherits="QuizApp.Admin.QuestionEditPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <asp:HyperLink ID="lnkBack" runat="server" CssClass="btn btn-outline btn-sm" Style="margin-bottom: 8px">← Back</asp:HyperLink>
            <h1><asp:Literal ID="litHeading" runat="server" Text="New Question" /></h1>
            <p><asp:Literal ID="litQuizTitle" runat="server" Mode="Encode" /></p>
        </div>
    </div>

    <asp:PlaceHolder ID="phError" runat="server" Visible="false">
        <div class="alert alert-error"><asp:Literal ID="litError" runat="server" Mode="Encode" /></div>
    </asp:PlaceHolder>

    <div class="card" style="max-width: 760px">
        <div class="input-group">
            <label>Question Text</label>
            <asp:TextBox ID="txtText" runat="server" TextMode="MultiLine" Rows="3" placeholder="Enter the question..." />
        </div>
        <div class="input-group">
            <label>Option A</label>
            <asp:TextBox ID="txtA" runat="server" MaxLength="500" placeholder="Option A" />
        </div>
        <div class="input-group">
            <label>Option B</label>
            <asp:TextBox ID="txtB" runat="server" MaxLength="500" placeholder="Option B" />
        </div>
        <div class="input-group">
            <label>Option C</label>
            <asp:TextBox ID="txtC" runat="server" MaxLength="500" placeholder="Option C" />
        </div>
        <div class="input-group">
            <label>Option D</label>
            <asp:TextBox ID="txtD" runat="server" MaxLength="500" placeholder="Option D" />
        </div>
        <div class="grid-3">
            <div class="input-group">
                <label>Correct Answer</label>
                <asp:DropDownList ID="ddlCorrect" runat="server">
                    <asp:ListItem>A</asp:ListItem>
                    <asp:ListItem>B</asp:ListItem>
                    <asp:ListItem>C</asp:ListItem>
                    <asp:ListItem>D</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="input-group">
                <label>Marks</label>
                <asp:TextBox ID="txtMarks" runat="server" TextMode="Number" min="1" max="100" Text="1" />
            </div>
            <div class="input-group">
                <label>Difficulty</label>
                <asp:DropDownList ID="ddlDifficulty" runat="server">
                    <asp:ListItem>Easy</asp:ListItem>
                    <asp:ListItem Selected="True">Medium</asp:ListItem>
                    <asp:ListItem>Hard</asp:ListItem>
                </asp:DropDownList>
            </div>
        </div>
        <div class="input-group">
            <label>Explanation (optional)</label>
            <asp:TextBox ID="txtExplanation" runat="server" TextMode="MultiLine" Rows="3" placeholder="Explain why this answer is correct..." />
        </div>

        <div class="flex gap-8 mt-16">
            <asp:Button ID="btnSave" runat="server" Text="Save Question" CssClass="btn btn-primary" OnClick="Save_Click" />
            <asp:HyperLink ID="lnkCancel" runat="server" CssClass="btn btn-outline">Cancel</asp:HyperLink>
        </div>
    </div>
</asp:Content>
