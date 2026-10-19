using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace QuizApp.Code
{
    public static class QuestionService
    {
        private const string Select = @"
            SELECT Id, QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption,
                   Marks, Difficulty, Explanation, IsActive
            FROM Questions";

        public static QuestionItem Map(IDataRecord r)
        {
            return new QuestionItem
            {
                Id = r.Int("Id"),
                QuizId = r.Int("QuizId"),
                QuestionText = r.Str("QuestionText"),
                OptionA = r.Str("OptionA"),
                OptionB = r.Str("OptionB"),
                OptionC = r.Str("OptionC"),
                OptionD = r.Str("OptionD"),
                CorrectOption = r.Str("CorrectOption").Trim(),
                Marks = r.Int("Marks"),
                Difficulty = r.Str("Difficulty"),
                Explanation = r.NStr("Explanation"),
                IsActive = r.Bool("IsActive")
            };
        }

        public static List<QuestionItem> GetByQuiz(int quizId, bool includeInactive)
        {
            return Db.List(Select + " WHERE QuizId = @q AND (@all = 1 OR IsActive = 1) ORDER BY Id",
                Map, Db.P("@q", quizId), Db.P("@all", includeInactive ? 1 : 0));
        }

        public static QuestionItem Get(int id)
        {
            return Db.First(Select + " WHERE Id = @id", Map, Db.P("@id", id));
        }

        public static List<QuestionDraft> GetDrafts(int quizId)
        {
            return GetByQuiz(quizId, false).Select(q => new QuestionDraft
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                OptionA = q.OptionA,
                OptionB = q.OptionB,
                OptionC = q.OptionC,
                OptionD = q.OptionD,
                CorrectOption = q.CorrectOption,
                Marks = q.Marks,
                Difficulty = q.Difficulty,
                Explanation = q.Explanation ?? ""
            }).ToList();
        }

        // ------------------------------------------------------------
        // VALIDATION
        // ------------------------------------------------------------

        /// <summary>Returns an error message, or null when the question is valid.</summary>
        public static string Validate(QuestionDraft q, string label)
        {
            label = string.IsNullOrEmpty(label) ? "" : label + ": ";
            var fields = new[] { q.QuestionText, q.OptionA, q.OptionB, q.OptionC, q.OptionD };
            if (fields.Any(string.IsNullOrWhiteSpace))
                return label + "fill in the question and all four options.";
            if (q.QuestionText.Trim().Length > 2000)
                return label + "question text must be 2000 characters or fewer.";
            if (new[] { q.OptionA, q.OptionB, q.OptionC, q.OptionD }.Any(o => o.Trim().Length > 500))
                return label + "each option must be 500 characters or fewer.";
            if (!new[] { "A", "B", "C", "D" }.Contains(q.CorrectOption))
                return label + "choose the correct answer.";
            if (q.Marks < 1 || q.Marks > 100)
                return label + "marks must be between 1 and 100.";
            if (!QuizService.Difficulties.Contains(q.Difficulty))
                return label + "choose a difficulty.";
            if (q.Explanation != null && q.Explanation.Length > 2000)
                return label + "explanation must be 2000 characters or fewer.";
            return null;
        }

        public static string ValidateAll(IList<QuestionDraft> questions)
        {
            for (int i = 0; i < questions.Count; i++)
            {
                var err = Validate(questions[i], "Question " + (i + 1));
                if (err != null) return err;
            }
            return null;
        }

        // ------------------------------------------------------------
        // CRUD
        // ------------------------------------------------------------

        private static DbParameter[] Params(QuestionDraft q, params DbParameter[] extra)
        {
            var list = new List<DbParameter>
            {
                Db.P("@text", q.QuestionText.Trim()),
                Db.P("@a", q.OptionA.Trim()),
                Db.P("@b", q.OptionB.Trim()),
                Db.P("@c", q.OptionC.Trim()),
                Db.P("@d", q.OptionD.Trim()),
                Db.P("@correct", q.CorrectOption),
                Db.P("@marks", q.Marks),
                Db.P("@diff", q.Difficulty),
                Db.P("@exp", string.IsNullOrWhiteSpace(q.Explanation) ? null : q.Explanation.Trim())
            };
            list.AddRange(extra);
            return list.ToArray();
        }

        public static int Insert(DbConnection conn, DbTransaction tx, int quizId, QuestionDraft q)
        {
            return Db.ScalarInt(conn, tx,
                @"INSERT INTO Questions (QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption,
                      Marks, Difficulty, Explanation, IsActive, CreatedAt)
                  VALUES (@quiz, @text, @a, @b, @c, @d, @correct, @marks, @diff, @exp, 1, GETDATE());
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                Params(q, Db.P("@quiz", quizId)));
        }

        public static void Update(DbConnection conn, DbTransaction tx, int id, QuestionDraft q)
        {
            Db.Exec(conn, tx,
                @"UPDATE Questions SET QuestionText = @text, OptionA = @a, OptionB = @b, OptionC = @c, OptionD = @d,
                      CorrectOption = @correct, Marks = @marks, Difficulty = @diff, Explanation = @exp
                  WHERE Id = @id",
                Params(q, Db.P("@id", id)));
        }

        public static int Create(int quizId, QuestionDraft q)
        {
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                var id = Insert(conn, tx, quizId, q);
                RecalcTotalMarks(conn, tx, quizId);
                tx.Commit();
                return id;
            }
        }

        public static void Save(int id, QuestionDraft q)
        {
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                Update(conn, tx, id, q);
                var quizId = Db.ScalarInt(conn, tx, "SELECT QuizId FROM Questions WHERE Id = @id", Db.P("@id", id));
                RecalcTotalMarks(conn, tx, quizId);
                tx.Commit();
            }
        }

        /// <summary>
        /// Deletes a question. If it was already answered in past attempts it is hidden instead,
        /// so users' result history stays intact. Returns false if not found.
        /// </summary>
        public static bool Delete(int id, out string message)
        {
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                var quizId = Db.ScalarInt(conn, tx, "SELECT QuizId FROM Questions WHERE Id = @id", Db.P("@id", id));
                var ok = DeleteOrHide(conn, tx, id, out message);
                if (ok) RecalcTotalMarks(conn, tx, quizId);
                tx.Commit();
                return ok;
            }
        }

        internal static bool DeleteOrHide(DbConnection conn, DbTransaction tx, int id, out string message)
        {
            var answered = Db.ScalarInt(conn, tx, "SELECT COUNT(*) FROM AttemptAnswers WHERE QuestionId = @id", Db.P("@id", id));
            if (answered > 0)
            {
                var rows = Db.Exec(conn, tx, "UPDATE Questions SET IsActive = 0 WHERE Id = @id", Db.P("@id", id));
                message = "This question appears in past attempts, so it was hidden instead of deleted.";
                return rows > 0;
            }

            Db.Exec(conn, tx, "DELETE FROM BookmarkedQuestions WHERE QuestionId = @id", Db.P("@id", id));
            var deleted = Db.Exec(conn, tx, "DELETE FROM Questions WHERE Id = @id", Db.P("@id", id));
            message = "Question deleted.";
            return deleted > 0;
        }

        public static bool Toggle(int id)
        {
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                var rows = Db.Exec(conn, tx,
                    "UPDATE Questions SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END WHERE Id = @id",
                    Db.P("@id", id));
                var quizId = Db.ScalarInt(conn, tx, "SELECT QuizId FROM Questions WHERE Id = @id", Db.P("@id", id));
                if (quizId > 0) RecalcTotalMarks(conn, tx, quizId);
                tx.Commit();
                return rows > 0;
            }
        }

        public static void RecalcTotalMarks(DbConnection conn, DbTransaction tx, int quizId)
        {
            Db.Exec(conn, tx,
                @"UPDATE Quizzes
                  SET TotalMarks = ISNULL((SELECT SUM(Marks) FROM Questions WHERE QuizId = @q AND IsActive = 1), 0)
                  WHERE Id = @q",
                Db.P("@q", quizId));
        }

        // ------------------------------------------------------------
        // BOOKMARKS
        // ------------------------------------------------------------

        public static void Bookmark(int userId, int questionId)
        {
            Db.Exec(
                @"IF EXISTS (SELECT 1 FROM Questions WHERE Id = @q)
                  AND NOT EXISTS (SELECT 1 FROM BookmarkedQuestions WHERE UserId = @u AND QuestionId = @q)
                      INSERT INTO BookmarkedQuestions (UserId, QuestionId, BookmarkedAt) VALUES (@u, @q, GETDATE());",
                Db.P("@u", userId), Db.P("@q", questionId));
        }

        public static void Unbookmark(int userId, int questionId)
        {
            Db.Exec("DELETE FROM BookmarkedQuestions WHERE UserId = @u AND QuestionId = @q",
                Db.P("@u", userId), Db.P("@q", questionId));
        }

        public static List<QuestionItem> GetBookmarks(int userId)
        {
            return Db.List(
                @"SELECT q.Id, q.QuizId, q.QuestionText, q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.CorrectOption,
                         q.Marks, q.Difficulty, q.Explanation, q.IsActive
                  FROM BookmarkedQuestions b
                  JOIN Questions q ON q.Id = b.QuestionId
                  WHERE b.UserId = @u
                  ORDER BY b.BookmarkedAt DESC",
                Map, Db.P("@u", userId));
        }

        public static HashSet<int> GetBookmarkedIds(int userId)
        {
            return new HashSet<int>(Db.List("SELECT QuestionId FROM BookmarkedQuestions WHERE UserId = @u",
                r => r.Int("QuestionId"), Db.P("@u", userId)));
        }
    }
}
