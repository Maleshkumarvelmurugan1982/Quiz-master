using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace QuizApp.Code
{
    public static class QuizService
    {
        public static readonly string[] Difficulties = { "Easy", "Medium", "Hard" };

        private const string ListSelect = @"
            SELECT q.Id, q.Title, q.Description, q.CategoryId, c.Name AS CategoryName, q.Difficulty,
                   q.DurationMinutes, q.TotalMarks, q.PassingPercentage, q.IsActive, q.CreatedAt,
                   q.NegativeMarking, q.NegativeMarkValue, q.RandomizeQuestions, q.RandomizeOptions,
                   q.MaxQuestionsPerAttempt, u.Name AS CreatedByName,
                   (SELECT COUNT(*) FROM Questions x WHERE x.QuizId = q.Id AND x.IsActive = 1) AS QuestionCount,
                   (SELECT COUNT(*) FROM Attempts a WHERE a.QuizId = q.Id AND a.Status <> 'InProgress') AS TotalAttempts
            FROM Quizzes q
            JOIN Categories c ON c.Id = q.CategoryId
            LEFT JOIN Users u ON u.Id = q.CreatedBy";

        private static QuizDetail Map(IDataRecord r)
        {
            return new QuizDetail
            {
                Id = r.Int("Id"),
                Title = r.Str("Title"),
                Description = r.Str("Description"),
                CategoryId = r.Int("CategoryId"),
                CategoryName = r.Str("CategoryName"),
                Difficulty = r.Str("Difficulty"),
                DurationMinutes = r.Int("DurationMinutes"),
                TotalMarks = r.Int("TotalMarks"),
                PassingPercentage = r.Dec("PassingPercentage"),
                IsActive = r.Bool("IsActive"),
                CreatedAt = r.Date("CreatedAt"),
                NegativeMarking = r.Bool("NegativeMarking"),
                NegativeMarkValue = r.Dec("NegativeMarkValue"),
                RandomizeQuestions = r.Bool("RandomizeQuestions"),
                RandomizeOptions = r.Bool("RandomizeOptions"),
                MaxQuestionsPerAttempt = r.NInt("MaxQuestionsPerAttempt"),
                CreatedByName = r.Str("CreatedByName"),
                QuestionCount = r.Int("QuestionCount"),
                TotalAttempts = r.Int("TotalAttempts")
            };
        }

        public static List<QuizDetail> GetQuizzes(string search, int? categoryId, string difficulty, bool activeOnly)
        {
            search = (search ?? "").Trim();
            var like = "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";

            var sql = ListSelect + @"
            WHERE (@activeOnly = 0 OR q.IsActive = 1)
              AND (@search = '' OR q.Title LIKE @like OR q.Description LIKE @like)
              AND (@cat IS NULL OR q.CategoryId = @cat OR c.ParentId = @cat)
              AND (@diff = '' OR q.Difficulty = @diff)
            ORDER BY q.CreatedAt DESC, q.Id DESC";

            return Db.List(sql, Map,
                Db.P("@activeOnly", activeOnly ? 1 : 0),
                Db.P("@search", search),
                Db.P("@like", like),
                Db.P("@cat", categoryId),
                Db.P("@diff", difficulty ?? ""));
        }

        public static QuizDetail GetQuiz(int id)
        {
            return Db.First(ListSelect + " WHERE q.Id = @id", Map, Db.P("@id", id));
        }

        // ------------------------------------------------------------
        // VALIDATION
        // ------------------------------------------------------------

        /// <param name="newCategoryName">A category typed by the admin that will be created on save (skips the dropdown check).</param>
        public static string Validate(QuizInput input, string newCategoryName = null)
        {
            if (string.IsNullOrWhiteSpace(input.Title)) return "Quiz title is required.";
            if (input.Title.Trim().Length > 200) return "Quiz title must be 200 characters or fewer.";
            if (input.Description != null && input.Description.Length > 1000) return "Description must be 1000 characters or fewer.";
            if (string.IsNullOrWhiteSpace(newCategoryName) && input.CategoryId <= 0) return "Please select a category (or type a new one).";
            if (newCategoryName != null && newCategoryName.Trim().Length > 100) return "Category name must be 100 characters or fewer.";
            if (!Difficulties.Contains(input.Difficulty)) return "Please choose a difficulty.";
            if (input.DurationMinutes < 1 || input.DurationMinutes > 180) return "Duration must be between 1 and 180 minutes.";
            if (input.PassingPercentage < 0 || input.PassingPercentage > 100) return "Passing % must be between 0 and 100.";
            if (input.NegativeMarkValue < 0 || input.NegativeMarkValue > 10) return "Negative mark value must be between 0 and 10.";
            if (input.MaxQuestionsPerAttempt.HasValue && input.MaxQuestionsPerAttempt.Value < 1) return "Max questions must be at least 1 (or blank for all).";
            return null;
        }

        // ------------------------------------------------------------
        // CREATE / UPDATE
        // ------------------------------------------------------------

        /// <summary>Saves a quiz and all its questions in ONE transaction. Returns the new quiz id.</summary>
        public static int CreateWithQuestions(QuizInput input, IList<QuestionDraft> questions, int createdBy)
        {
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                var quizId = Db.ScalarInt(conn, tx,
                    @"INSERT INTO Quizzes (Title, Description, CategoryId, Difficulty, DurationMinutes, TotalMarks,
                          PassingPercentage, NegativeMarking, NegativeMarkValue, RandomizeQuestions, RandomizeOptions,
                          MaxQuestionsPerAttempt, IsActive, CreatedBy, CreatedAt)
                      VALUES (@t, @d, @c, @diff, @dur, 0, @pass, @neg, @negv, @rq, @ro, @maxq, @act, @by, GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    QuizParams(input, Db.P("@by", createdBy)));

                foreach (var q in questions)
                    QuestionService.Insert(conn, tx, quizId, q);

                QuestionService.RecalcTotalMarks(conn, tx, quizId);
                tx.Commit();
                return quizId;
            }
        }

        /// <summary>
        /// Updates quiz settings and its (active) questions in ONE transaction.
        /// Questions removed in the editor are deleted, or hidden if users already answered them.
        /// </summary>
        public static void UpdateWithQuestions(int quizId, QuizInput input, IList<QuestionDraft> questions)
        {
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                Db.Exec(conn, tx,
                    @"UPDATE Quizzes SET Title = @t, Description = @d, CategoryId = @c, Difficulty = @diff,
                          DurationMinutes = @dur, PassingPercentage = @pass, NegativeMarking = @neg,
                          NegativeMarkValue = @negv, RandomizeQuestions = @rq, RandomizeOptions = @ro,
                          MaxQuestionsPerAttempt = @maxq, IsActive = @act, UpdatedAt = GETDATE()
                      WHERE Id = @id",
                    QuizParams(input, Db.P("@id", quizId)));

                var existing = new HashSet<int>(Db.List(conn, tx,
                    "SELECT Id FROM Questions WHERE QuizId = @id AND IsActive = 1",
                    r => r.Int("Id"), Db.P("@id", quizId)));

                var kept = new HashSet<int>();
                foreach (var q in questions)
                {
                    if (q.Id > 0 && existing.Contains(q.Id))
                    {
                        QuestionService.Update(conn, tx, q.Id, q);
                        kept.Add(q.Id);
                    }
                    else
                    {
                        QuestionService.Insert(conn, tx, quizId, q);
                    }
                }

                foreach (var removedId in existing.Where(id => !kept.Contains(id)))
                {
                    string ignored;
                    if (!QuestionService.DeleteOrHide(conn, tx, removedId, out ignored))
                        throw new InvalidOperationException("Could not remove question " + removedId);
                }

                QuestionService.RecalcTotalMarks(conn, tx, quizId);
                tx.Commit();
            }
        }

        private static DbParameter[] QuizParams(QuizInput i, DbParameter extra)
        {
            return new[]
            {
                Db.P("@t", i.Title.Trim()),
                Db.P("@d", string.IsNullOrWhiteSpace(i.Description) ? null : i.Description.Trim()),
                Db.P("@c", i.CategoryId),
                Db.P("@diff", i.Difficulty),
                Db.P("@dur", i.DurationMinutes),
                Db.P("@pass", i.PassingPercentage),
                Db.P("@neg", i.NegativeMarking),
                Db.P("@negv", i.NegativeMarkValue),
                Db.P("@rq", i.RandomizeQuestions),
                Db.P("@ro", i.RandomizeOptions),
                Db.P("@maxq", i.MaxQuestionsPerAttempt),
                Db.P("@act", i.IsActive),
                extra
            };
        }

        // ------------------------------------------------------------
        // DELETE / TOGGLE
        // ------------------------------------------------------------

        /// <summary>Deletes the quiz with its questions, attempts and bookmarks.</summary>
        public static bool Delete(int quizId)
        {
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                var p = new Func<DbParameter>(() => Db.P("@id", quizId));
                Db.Exec(conn, tx, "DELETE FROM BookmarkedQuestions WHERE QuestionId IN (SELECT Id FROM Questions WHERE QuizId = @id)", p());
                Db.Exec(conn, tx, "DELETE FROM AttemptAnswers WHERE AttemptId IN (SELECT Id FROM Attempts WHERE QuizId = @id)", p());
                Db.Exec(conn, tx, "DELETE FROM AttemptAnswers WHERE QuestionId IN (SELECT Id FROM Questions WHERE QuizId = @id)", p());
                Db.Exec(conn, tx, "DELETE FROM Attempts WHERE QuizId = @id", p());
                Db.Exec(conn, tx, "DELETE FROM Questions WHERE QuizId = @id", p());
                var rows = Db.Exec(conn, tx, "DELETE FROM Quizzes WHERE Id = @id", p());
                tx.Commit();
                return rows > 0;
            }
        }

        public static bool Toggle(int quizId)
        {
            return Db.Exec("UPDATE Quizzes SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END, UpdatedAt = GETDATE() WHERE Id = @id",
                Db.P("@id", quizId)) > 0;
        }

        public static int CountActive()
        {
            return Db.ScalarInt("SELECT COUNT(*) FROM Quizzes WHERE IsActive = 1");
        }
    }
}
