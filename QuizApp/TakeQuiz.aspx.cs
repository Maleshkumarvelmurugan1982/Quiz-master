using System;
using System.Collections.Generic;
using QuizApp.Code;

namespace QuizApp
{
    public partial class TakeQuiz : BasePage
    {
        protected int AttemptId { get; private set; }
        protected int RemainingSeconds { get; private set; }
        protected int QuestionCount { get; private set; }

        private AttemptLayout _layout;

        private string SessionKey
        {
            get { return "qm_attempt_" + AttemptId; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AttemptId = QueryInt("attempt");
            var info = AttemptService.GetInfo(AttemptId);

            if (info == null || info.UserId != UserId)
            {
                Flash("Quiz attempt not found.", "error");
                Go("~/Quizzes.aspx");
                return;
            }

            if (info.Status != "InProgress")
            {
                // Already submitted (e.g. Back button after submitting) -> show the result.
                Go("~/Result.aspx?id=" + AttemptId);
                return;
            }

            _layout = Session[SessionKey] as AttemptLayout;
            if (_layout == null || _layout.AttemptId != AttemptId)
            {
                // Session expired / server restarted: rebuild the question list.
                _layout = AttemptService.RebuildLayout(info);
                Session[SessionKey] = _layout;
            }

            // Remaining time is calculated on the server, so refreshing the page does not reset the timer.
            var elapsed = AttemptService.GetElapsedSeconds(AttemptId);
            RemainingSeconds = Math.Max(0, info.DurationMinutes * 60 - elapsed);

            if (!IsPostBack)
            {
                // More than 30s past the deadline (e.g. user came back much later): close the attempt.
                if (info.DurationMinutes * 60 - elapsed < -30)
                {
                    FinishAttempt();
                    return;
                }

                Title = info.QuizTitle + " — QuizMaster";
                litTitle.Text = info.QuizTitle;

                var questions = AttemptService.GetTakeQuestions(_layout, UserId);
                if (questions.Count == 0)
                {
                    Flash("This quiz has no questions available.", "error");
                    Go("~/Quizzes.aspx");
                    return;
                }

                QuestionCount = questions.Count;
                litTotal.Text = questions.Count.ToString();

                rptNav.DataSource = questions;
                rptNav.DataBind();
                rptQuestions.DataSource = questions;
                rptQuestions.DataBind();
            }
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            FinishAttempt();
        }

        private void FinishAttempt()
        {
            // Answers are plain radio buttons named "ans_<questionId>".
            var answers = new Dictionary<int, string>();
            foreach (var qid in _layout.QuestionIds)
                answers[qid] = Request.Form["ans_" + qid];

            var ok = AttemptService.Submit(UserId, _layout, answers);
            Session.Remove(SessionKey);

            if (!ok)
                Flash("This attempt was already submitted.", "error");

            Go("~/Result.aspx?id=" + AttemptId);
        }
    }
}
