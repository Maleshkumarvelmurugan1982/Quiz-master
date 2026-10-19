<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="QuestionEditor.ascx.cs" Inherits="QuizApp.Controls.QuestionEditor" %>

<div class="manual-q-section mt-16">
    <div class="flex-between" style="align-items: center; margin-bottom: 12px; gap: 12px; flex-wrap: wrap">
        <div>
            <h3 style="margin: 0">Questions (<asp:Literal ID="litCount" runat="server" />)</h3>
            <div class="gen-hint">Each question needs four options. Pick the correct answer with the A/B/C/D buttons.</div>
        </div>
        <asp:Button ID="btnAdd" runat="server" Text="+ Add Question" CssClass="btn btn-outline btn-sm"
            OnClick="Add_Click" CausesValidation="false" />
    </div>

    <asp:Repeater ID="rptQuestions" runat="server" OnItemCommand="Questions_ItemCommand" OnItemDataBound="Questions_ItemDataBound">
        <ItemTemplate>
            <div class="manual-q-card">
                <div class="manual-q-head">
                    <strong>Question <%# Container.ItemIndex + 1 %></strong>
                    <asp:LinkButton ID="lnkRemove" runat="server" CssClass="btn btn-danger btn-sm"
                        CommandName="RemoveQuestion" CommandArgument='<%# Container.ItemIndex %>' CausesValidation="false">Remove</asp:LinkButton>
                </div>
                <asp:HiddenField ID="hfId" runat="server" />

                <div class="input-group">
                    <label>Question Text</label>
                    <asp:TextBox ID="txtText" runat="server" TextMode="MultiLine" Rows="3" placeholder="Enter the question..." />
                </div>

                <div class="q-option-row"><b>A</b><asp:TextBox ID="txtA" runat="server" placeholder="Option A" MaxLength="500" /></div>
                <div class="q-option-row"><b>B</b><asp:TextBox ID="txtB" runat="server" placeholder="Option B" MaxLength="500" /></div>
                <div class="q-option-row"><b>C</b><asp:TextBox ID="txtC" runat="server" placeholder="Option C" MaxLength="500" /></div>
                <div class="q-option-row"><b>D</b><asp:TextBox ID="txtD" runat="server" placeholder="Option D" MaxLength="500" /></div>

                <div class="grid-3" style="margin-top: 10px">
                    <div class="input-group">
                        <label>Correct Answer</label>
                        <asp:RadioButtonList ID="rblCorrect" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="correct-picker">
                            <asp:ListItem>A</asp:ListItem>
                            <asp:ListItem>B</asp:ListItem>
                            <asp:ListItem>C</asp:ListItem>
                            <asp:ListItem>D</asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <div class="input-group">
                        <label>Marks</label>
                        <asp:TextBox ID="txtMarks" runat="server" TextMode="Number" min="1" max="100" />
                    </div>
                    <div class="input-group">
                        <label>Difficulty</label>
                        <asp:DropDownList ID="ddlDifficulty" runat="server">
                            <asp:ListItem>Easy</asp:ListItem>
                            <asp:ListItem>Medium</asp:ListItem>
                            <asp:ListItem>Hard</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <div class="input-group" style="margin-bottom: 0">
                    <label>Explanation (optional, shown after the quiz)</label>
                    <asp:TextBox ID="txtExplanation" runat="server" TextMode="MultiLine" Rows="2" placeholder="Explain why the correct answer is correct..." />
                </div>
            </div>
        </ItemTemplate>
    </asp:Repeater>

    <asp:PlaceHolder ID="phEmpty" runat="server">
        <div style="text-align: center; padding: 18px; border: 1px dashed var(--border); border-radius: 10px; color: var(--muted)">
            No questions yet. Click <strong>+ Add Question</strong> to start.
        </div>
    </asp:PlaceHolder>
</div>
