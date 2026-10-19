<%@ Page Title="Quiz Editor — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="QuizEdit.aspx.cs" Inherits="QuizApp.Admin.QuizEditPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>
<%@ Register Src="~/Controls/QuestionEditor.ascx" TagPrefix="uc" TagName="QuestionEditor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <a class="btn btn-outline btn-sm" href="Quizzes.aspx" style="margin-bottom: 8px">← Back</a>
            <h1><asp:Literal ID="litHeading" runat="server" Text="New Quiz" /></h1>
            <p>Quiz settings and questions are saved together.</p>
        </div>
    </div>

    <asp:PlaceHolder ID="phError" runat="server" Visible="false">
        <div class="alert alert-error"><asp:Literal ID="litError" runat="server" Mode="Encode" /></div>
    </asp:PlaceHolder>

    <div class="card">
        <div class="grid-2">
            <div class="input-group span-2">
                <label>Quiz Title</label>
                <asp:TextBox ID="txtTitle" runat="server" MaxLength="200" placeholder="e.g. Java Fundamentals" />
            </div>
            <div class="input-group span-2">
                <label>Description</label>
                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" MaxLength="1000"
                    placeholder="Brief description of what this quiz covers..." />
            </div>
            <div class="input-group">
                <label>Category</label>
                <asp:DropDownList ID="ddlCategory" runat="server" />
            </div>
            <div class="input-group">
                <label>…or create a new category</label>
                <asp:TextBox ID="txtNewCategory" runat="server" MaxLength="100" placeholder="Leave blank to use the selected one" />
            </div>
            <div class="input-group">
                <label>Difficulty</label>
                <asp:DropDownList ID="ddlDifficulty" runat="server">
                    <asp:ListItem>Easy</asp:ListItem>
                    <asp:ListItem Selected="True">Medium</asp:ListItem>
                    <asp:ListItem>Hard</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="input-group">
                <label>Duration (minutes)</label>
                <asp:TextBox ID="txtDuration" runat="server" TextMode="Number" min="1" max="180" Text="15" />
            </div>
            <div class="input-group">
                <label>Passing %</label>
                <asp:TextBox ID="txtPassPct" runat="server" TextMode="Number" min="0" max="100" step="any" Text="50" />
            </div>
            <div class="input-group">
                <label>Max Questions per attempt (blank = all)</label>
                <asp:TextBox ID="txtMaxQ" runat="server" TextMode="Number" min="1" placeholder="Leave blank for all" />
            </div>
            <div class="input-group">
                <label>Negative Mark Value</label>
                <asp:TextBox ID="txtNegValue" runat="server" TextMode="Number" min="0" max="10" step="0.25" Text="0.25" />
            </div>
            <div class="input-group">
                <div class="check-row"><asp:CheckBox ID="chkNegative" runat="server" Text="Negative Marking" /></div>
                <div class="check-row" style="margin-top: 8px"><asp:CheckBox ID="chkRandQ" runat="server" Text="Randomize Questions" /></div>
                <div class="check-row" style="margin-top: 8px"><asp:CheckBox ID="chkRandOpt" runat="server" Text="Shuffle Answer Options" /></div>
                <div class="check-row" style="margin-top: 8px"><asp:CheckBox ID="chkActive" runat="server" Text="Active (visible to users)" Checked="true" /></div>
            </div>
        </div>

        <uc:QuestionEditor ID="ucQuestions" runat="server" />

        <div class="flex gap-8 mt-16">
            <asp:Button ID="btnSave" runat="server" Text="💾 Save Quiz" CssClass="btn btn-primary" OnClick="Save_Click" />
            <a class="btn btn-outline" href="Quizzes.aspx">Cancel</a>
        </div>
    </div>
</asp:Content>
