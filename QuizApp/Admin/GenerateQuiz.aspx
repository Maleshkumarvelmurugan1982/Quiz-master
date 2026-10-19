<%@ Page Title="Generate Quiz with AI — QuizMaster" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="GenerateQuiz.aspx.cs" Inherits="QuizApp.Admin.GenerateQuizPage" %>
<%@ MasterType VirtualPath="~/Site.Master" %>
<%@ Register Src="~/Controls/QuestionEditor.ascx" TagPrefix="uc" TagName="QuestionEditor" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-head">
        <div>
            <a class="btn btn-outline btn-sm" href="Quizzes.aspx" style="margin-bottom: 8px">← Back</a>
            <h1>✨ Generate Quiz with AI</h1>
            <p>Create a quiz from a topic or from your own study material. You can review and edit everything before saving.</p>
        </div>
    </div>

    <asp:PlaceHolder ID="phError" runat="server" Visible="false">
        <div class="alert alert-error"><asp:Literal ID="litError" runat="server" Mode="Encode" /></div>
    </asp:PlaceHolder>

    <!-- ========== STEP 1: INPUT ========== -->
    <asp:Panel ID="pnlInput" runat="server" CssClass="card" Style="max-width: 780px">
        <div class="gen-tabs">
            <button type="button" class="gen-tab active" id="genTabTopic" onclick="qmGenMode('topic')">💡 From a Topic</button>
            <button type="button" class="gen-tab" id="genTabFile" onclick="qmGenMode('file')">📄 From a File</button>
        </div>
        <asp:HiddenField ID="hfMode" runat="server" Value="topic" />

        <div id="genTopicBox">
            <div class="input-group">
                <label>Topic</label>
                <asp:TextBox ID="txtTopic" runat="server" MaxLength="300"
                    placeholder="e.g. Java OOP concepts, SQL joins, Indian Constitution" />
                <div class="gen-hint">The AI writes the questions from its own knowledge. Be specific for better quizzes.</div>
            </div>
        </div>

        <div id="genFileBox" style="display: none">
            <div class="input-group">
                <label>Study material</label>
                <asp:FileUpload ID="fuFile" runat="server" CssClass="file-input" />
                <div class="gen-hint">PDF, DOCX, PPTX, TXT, MD, CSV, JSON, PNG, JPG, WEBP · max 10 MB</div>
            </div>
            <div class="input-group">
                <label>Focus on (optional)</label>
                <asp:TextBox ID="txtFocus" runat="server" MaxLength="300" placeholder="e.g. Chapter 3 only, or 'only the definitions'" />
            </div>
        </div>

        <div class="grid-2">
            <div class="input-group">
                <label>Number of questions</label>
                <asp:TextBox ID="txtCount" runat="server" TextMode="Number" min="3" max="30" Text="10" />
            </div>
            <div class="input-group">
                <label>Difficulty</label>
                <asp:DropDownList ID="ddlGenDifficulty" runat="server">
                    <asp:ListItem>Easy</asp:ListItem>
                    <asp:ListItem Selected="True">Medium</asp:ListItem>
                    <asp:ListItem>Hard</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="input-group span-2">
                <label>AI model</label>
                <div class="gen-hint" style="font-size: .85rem">
                    <span class="badge badge-blue"><asp:Literal ID="litModel" runat="server" Mode="Encode" /></span>
                    — set in Web.config (OpenRouter:Model). Temporary failures are retried automatically.
                </div>
            </div>
            <div class="input-group span-2">
                <label>Extra instructions (optional)</label>
                <asp:TextBox ID="txtInstructions" runat="server" TextMode="MultiLine" Rows="2"
                    placeholder="e.g. Include a few code-output questions. Keep options short." />
            </div>
        </div>

        <div class="flex gap-8">
            <asp:Button ID="btnGenerate" runat="server" Text="✨ Generate Quiz" CssClass="btn btn-ai"
                OnClientClick="return qmGenStart();" OnClick="Generate_Click" />
            <a class="btn btn-outline" href="Quizzes.aspx">Cancel</a>
        </div>
    </asp:Panel>

    <!-- ========== STEP 2: REVIEW ========== -->
    <asp:Panel ID="pnlReview" runat="server" CssClass="card" Visible="false">
        <asp:PlaceHolder ID="phWarning" runat="server" Visible="false">
            <div class="gen-warning"><asp:Literal ID="litWarning" runat="server" Mode="Encode" /></div>
        </asp:PlaceHolder>
        <div class="gen-hint" style="margin-bottom: 12px"><asp:Literal ID="litModelInfo" runat="server" Mode="Encode" /></div>

        <div class="grid-2">
            <div class="input-group span-2">
                <label>Quiz Title</label>
                <asp:TextBox ID="txtQuizTitle" runat="server" MaxLength="200" />
            </div>
            <div class="input-group span-2">
                <label>Description</label>
                <asp:TextBox ID="txtQuizDescription" runat="server" TextMode="MultiLine" Rows="2" MaxLength="1000" />
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
                <asp:DropDownList ID="ddlQuizDifficulty" runat="server">
                    <asp:ListItem>Easy</asp:ListItem>
                    <asp:ListItem>Medium</asp:ListItem>
                    <asp:ListItem>Hard</asp:ListItem>
                </asp:DropDownList>
            </div>
            <div class="input-group">
                <label>Duration (minutes)</label>
                <asp:TextBox ID="txtDuration" runat="server" TextMode="Number" min="1" max="180" />
            </div>
            <div class="input-group">
                <label>Passing %</label>
                <asp:TextBox ID="txtPassPct" runat="server" TextMode="Number" min="0" max="100" step="any" Text="50" />
            </div>
            <div class="input-group">
                <div class="check-row"><asp:CheckBox ID="chkRandQ" runat="server" Text="Randomize question order" /></div>
                <div class="check-row" style="margin-top: 8px"><asp:CheckBox ID="chkPublish" runat="server" Text="Publish now (visible to users)" Checked="true" /></div>
            </div>
        </div>

        <uc:QuestionEditor ID="ucQuestions" runat="server" />

        <div class="flex gap-8 mt-16" style="flex-wrap: wrap">
            <asp:Button ID="btnSave" runat="server" Text="💾 Save Quiz" CssClass="btn btn-primary" OnClick="Save_Click" />
            <asp:Button ID="btnBack" runat="server" Text="← Back / Regenerate" CssClass="btn btn-outline"
                OnClick="Back_Click" CausesValidation="false" />
            <a class="btn btn-outline" href="Quizzes.aspx">Discard</a>
        </div>
    </asp:Panel>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ScriptContent" runat="server">
    <script>
        var QM_GEN = {
            modeField: '<%= hfMode.ClientID %>',
            topic: '<%= txtTopic.ClientID %>',
            file: '<%= fuFile.ClientID %>',
            count: '<%= txtCount.ClientID %>',
            exts: ['pdf', 'docx', 'pptx', 'txt', 'md', 'markdown', 'csv', 'json', 'png', 'jpg', 'jpeg', 'webp', 'gif'],
            maxBytes: 10 * 1024 * 1024
        };

        function qmGenMode(mode) {
            var field = document.getElementById(QM_GEN.modeField);
            if (field) field.value = mode;
            document.getElementById('genTabTopic').classList.toggle('active', mode === 'topic');
            document.getElementById('genTabFile').classList.toggle('active', mode === 'file');
            document.getElementById('genTopicBox').style.display = mode === 'topic' ? 'block' : 'none';
            document.getElementById('genFileBox').style.display = mode === 'file' ? 'block' : 'none';
        }

        function qmGenStart() {
            var mode = document.getElementById(QM_GEN.modeField).value;
            var count = parseInt(document.getElementById(QM_GEN.count).value, 10);
            if (!count || count < 3 || count > 30) { qmToast('Number of questions must be between 3 and 30.', 'error'); return false; }

            if (mode === 'file') {
                var input = document.getElementById(QM_GEN.file);
                var f = input.files && input.files[0];
                if (!f) { qmToast('Choose a file first.', 'error'); return false; }
                var ext = (f.name.split('.').pop() || '').toLowerCase();
                if (QM_GEN.exts.indexOf(ext) < 0) { qmToast('Unsupported file type. Use PDF, DOCX, PPTX, TXT, MD, CSV, JSON or an image.', 'error'); return false; }
                if (f.size > QM_GEN.maxBytes) { qmToast('File is too large (max 10 MB).', 'error'); return false; }
                qmShowLoading('Generating quiz from your file…', 'Reading your material and creating questions. This usually takes 10–60 seconds.');
            } else {
                if (!document.getElementById(QM_GEN.topic).value.trim()) { qmToast('Enter a topic first.', 'error'); return false; }
                qmShowLoading('Generating quiz from your topic…', 'Using AI knowledge to create questions. This usually takes 10–60 seconds.');
            }
            return true;
        }

        document.addEventListener('DOMContentLoaded', function () {
            var field = document.getElementById(QM_GEN.modeField);
            if (field) qmGenMode(field.value === 'file' ? 'file' : 'topic');
        });
    </script>
</asp:Content>
