using System.Text;
using System.Web;

namespace QuizApp.Code
{
    /// <summary>Builds the 4 coloured option boxes used on the Result and Bookmarks pages.</summary>
    public static class ReviewHtml
    {
        private static readonly string[] Letters = { "A", "B", "C", "D" };

        public static string Options(string a, string b, string c, string d, string correct, string selected, bool showSelection)
        {
            var texts = new[] { a, b, c, d };
            var sb = new StringBuilder("<div class=\"review-opts\">");

            for (int i = 0; i < 4; i++)
            {
                var letter = Letters[i];
                var isCorrect = letter == correct;
                var isWrongPick = showSelection && letter == selected && !isCorrect;

                sb.Append("<div class=\"review-opt");
                if (isCorrect) sb.Append(" is-correct");
                else if (isWrongPick) sb.Append(" is-wrong");
                sb.Append("\"><strong>").Append(letter).Append(".</strong> ");
                sb.Append(HttpUtility.HtmlEncode(texts[i] ?? ""));
                if (isCorrect) sb.Append(" ✓");
                if (isWrongPick) sb.Append(" ✗");
                if (showSelection && letter == selected) sb.Append(" <em style=\"color:var(--muted)\">(your answer)</em>");
                sb.Append("</div>");
            }

            sb.Append("</div>");
            return sb.ToString();
        }

        public static string ForReview(object item)
        {
            var r = (AnswerReview)item;
            return Options(r.OptionA, r.OptionB, r.OptionC, r.OptionD, r.CorrectOption, r.SelectedOption, true);
        }

        public static string ForQuestion(object item)
        {
            var q = (QuestionItem)item;
            return Options(q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.CorrectOption, null, false);
        }

        public static string ReviewClass(object item)
        {
            var r = (AnswerReview)item;
            return r.IsCorrect ? "correct" : r.SelectedOption != null ? "wrong" : "unanswered";
        }

        public static string ReviewBadge(object item)
        {
            var r = (AnswerReview)item;
            if (r.IsCorrect)
                return "<span class=\"badge badge-green\">✓ Correct (+" + Fmt.Num(r.MarksAwarded) + ")</span>";
            if (r.SelectedOption != null)
                return "<span class=\"badge badge-red\">✗ Wrong (" + Fmt.Num(r.MarksAwarded) + ")</span>";
            return "<span class=\"badge badge-amber\">— Unanswered</span>";
        }
    }
}
