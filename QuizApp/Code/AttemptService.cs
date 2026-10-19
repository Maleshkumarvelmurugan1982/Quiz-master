using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace QuizApp.Code
{
    public static class AttemptService
    {
        private static readonly string[] Letters = { "A", "B", "C", "D" };

        // ------------------------------------------------------------
        // START
        // ------------------------------------------------------------

        /// <summary>Creates a new attempt and decides question/option order. Returns null + error if not possible.</summary>
        public static AttemptLayout Start(int userId, int quizId, out string error)
        {
            error = null;
            var quiz = QuizService.GetQuiz(quizId);
            if (quiz == null || !quiz.IsActive)
            {
                error = "Quiz not found or inactive.";
                return null;
            }

            var questions = QuestionService.GetByQuiz(quizId, false);
            if (questions.Count == 0)
            {
                error = "This quiz has no questions yet.";
                return null;
            }

            var rnd = new Random(Guid.NewGuid().GetHashCode());
            if (quiz.RandomizeQuestions)
                questions = questions.OrderBy(q => rnd.Next()).ToList();

            if (quiz.MaxQuestionsPerAttempt.HasValue && quiz.MaxQuestionsPerAttempt.Value < questions.Count)
                questions = questions.Take(quiz.MaxQuestionsPerAttempt.Value).ToList();

            // Marks for the questions actually served (so a limited attempt is graded fairly).
            var totalMarks = questions.Sum(q => q.Marks);

            int attemptId;
            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                // Cancel any unfinished attempt of this quiz by this user.
                Db.Exec(conn, tx,
                    @"DELETE FROM AttemptAnswers WHERE AttemptId IN
                        (SELECT Id FROM Attempts WHERE UserId = @u AND QuizId = @q AND Status = 'InProgress');
                      DELETE FROM Attempts WHERE UserId = @u AND QuizId = @q AND Status = 'InProgress';",
                    Db.P("@u", userId), Db.P("@q", quizId));

                attemptId = Db.ScalarInt(conn, tx,
                    @"INSERT INTO Attempts (UserId, QuizId, Score, TotalMarks, Percentage, CorrectAnswers, WrongAnswers,
                          UnansweredQuestions, TimeTakenSeconds, Status, AttemptedAt)
                      VALUES (@u, @q, 0, @total, 0, 0, 0, 0, 0, 'InProgress', GETDATE());
                      SELECT CAST(SCOPE_IDENTITY() AS INT);",
                    Db.P("@u", userId), Db.P("@q", quizId), Db.P("@total", totalMarks));

                tx.Commit();
            }

            var layout = new AttemptLayout { AttemptId = attemptId, QuizId = quizId };
            foreach (var q in questions)
            {
                layout.QuestionIds.Add(q.Id);
                layout.OptionOrders.Add(quiz.RandomizeOptions
                    ? string.Concat(Letters.OrderBy(x => rnd.Next()))
                    : "ABCD");
            }
            return layout;
        }

        /// <summary>Used if the Session was lost mid-quiz: rebuilds a plain (non-random) layout.</summary>
        public static AttemptLayout RebuildLayout(AttemptInfo info)
        {
            var quiz = QuizService.GetQuiz(info.QuizId);
            var questions = QuestionService.GetByQuiz(info.QuizId, false);
            if (quiz != null && quiz.MaxQuestionsPerAttempt.HasValue && quiz.MaxQuestionsPerAttempt.Value < questions.Count)
                questions = questions.Take(quiz.MaxQuestionsPerAttempt.Value).ToList();

            var layout = new AttemptLayout { AttemptId = info.AttemptId, QuizId = info.QuizId };
            foreach (var q in questions)
            {
                layout.QuestionIds.Add(q.Id);
                layout.OptionOrders.Add("ABCD");
            }
            return layout;
        }

        public static AttemptInfo GetInfo(int attemptId)
        {
            return Db.First(
                @"SELECT a.Id, a.UserId, a.QuizId, a.TotalMarks, a.Status, a.AttemptedAt,
                         q.Title, q.DurationMinutes, q.PassingPercentage, q.NegativeMarking, q.NegativeMarkValue
                  FROM Attempts a JOIN Quizzes q ON q.Id = a.QuizId
                  WHERE a.Id = @id",
                r => new AttemptInfo
                {
                    AttemptId = r.Int("Id"),
                    UserId = r.Int("UserId"),
                    QuizId = r.Int("QuizId"),
                    TotalMarks = r.Int("TotalMarks"),
                    Status = r.Str("Status"),
                    AttemptedAt = r.Date("AttemptedAt"),
                    QuizTitle = r.Str("Title"),
                    DurationMinutes = r.Int("DurationMinutes"),
                    PassingPercentage = r.Dec("PassingPercentage"),
                    NegativeMarking = r.Bool("NegativeMarking"),
                    NegativeMarkValue = r.Dec("NegativeMarkValue")
                },
                Db.P("@id", attemptId));
        }

        /// <summary>Seconds since the attempt started, measured by the database clock.</summary>
        public static int GetElapsedSeconds(int attemptId)
        {
            return Db.ScalarInt("SELECT DATEDIFF(SECOND, AttemptedAt, GETDATE()) FROM Attempts WHERE Id = @id",
                Db.P("@id", attemptId));
        }

        private static Dictionary<int, QuestionItem> LoadQuestions(int quizId, IList<int> ids)
        {
            if (ids.Count == 0) return new Dictionary<int, QuestionItem>();
            var idList = string.Join(",", ids.Select(i => i.ToString())); // integers only, safe
            return Db.List(
                    "SELECT Id, QuizId, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption, Marks, " +
                    "Difficulty, Explanation, IsActive FROM Questions WHERE QuizId = @q AND Id IN (" + idList + ")",
                    QuestionService.Map, Db.P("@q", quizId))
                .ToDictionary(q => q.Id);
        }

        /// <summary>Questions for the Take Quiz page in the layout's order (no correct answers included).</summary>
        public static List<TakeQuestion> GetTakeQuestions(AttemptLayout layout, int userId)
        {
            var questions = LoadQuestions(layout.QuizId, layout.QuestionIds);
            var bookmarks = QuestionService.GetBookmarkedIds(userId);
            var result = new List<TakeQuestion>();

            for (int i = 0; i < layout.QuestionIds.Count; i++)
            {
                QuestionItem q;
                if (!questions.TryGetValue(layout.QuestionIds[i], out q)) continue;

                var order = i < layout.OptionOrders.Count ? layout.OptionOrders[i] : "ABCD";
                var take = new TakeQuestion
                {
                    Index = result.Count,
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Marks = q.Marks,
                    Difficulty = q.Difficulty,
                    IsBookmarked = bookmarks.Contains(q.Id),
                    Options = new List<TakeOption>()
                };
                for (int p = 0; p < 4; p++)
                {
                    var original = order[p].ToString();
                    take.Options.Add(new TakeOption
                    {
                        QuestionId = q.Id,
                        DisplayLetter = Letters[p],
                        Value = original,
                        Text = q.GetOption(original)
                    });
                }
                result.Add(take);
            }
            return result;
        }

        // ------------------------------------------------------------
        // SUBMIT
        // ------------------------------------------------------------

        /// <summary>
        /// Grades and completes the attempt. answers: questionId -> "A".."D" or null.
        /// Returns false if the attempt is not this user's or was already submitted.
        /// </summary>
        public static bool Submit(int userId, AttemptLayout layout, IDictionary<int, string> answers)
        {
            var info = GetInfo(layout.AttemptId);
            if (info == null || info.UserId != userId || info.Status != "InProgress") return false;

            var questions = LoadQuestions(info.QuizId, layout.QuestionIds);
            decimal totalScore = 0;
            int correct = 0, wrong = 0, unanswered = 0;

            var records = new List<Tuple<int, string, bool, decimal>>();
            foreach (var qid in layout.QuestionIds)
            {
                QuestionItem q;
                if (!questions.TryGetValue(qid, out q)) continue;

                string selected;
                answers.TryGetValue(qid, out selected);
                if (!Letters.Contains(selected)) selected = null;

                bool isCorrect = false;
                decimal marks = 0;

                if (selected == null)
                {
                    unanswered++;
                }
                else if (selected == q.CorrectOption)
                {
                    isCorrect = true;
                    marks = q.Marks;
                    correct++;
                    totalScore += marks;
                }
                else
                {
                    wrong++;
                    if (info.NegativeMarking)
                    {
                        marks = -info.NegativeMarkValue;
                        totalScore += marks;
                    }
                }
                records.Add(Tuple.Create(qid, selected, isCorrect, marks));
            }

            if (totalScore < 0) totalScore = 0;

            var totalMarks = info.TotalMarks > 0 ? info.TotalMarks : questions.Values.Sum(q => q.Marks);
            var percentage = totalMarks > 0 ? Math.Round(totalScore / totalMarks * 100m, 2) : 0m;
            if (percentage > 100m) percentage = 100m;
            var status = percentage >= info.PassingPercentage ? "Passed" : "Failed";

            using (var conn = Db.Open())
            using (var tx = conn.BeginTransaction())
            {
                var rows = Db.Exec(conn, tx,
                    @"UPDATE Attempts SET Score = @score, TotalMarks = @total, Percentage = @pct,
                          CorrectAnswers = @c, WrongAnswers = @w, UnansweredQuestions = @u,
                          TimeTakenSeconds = CASE
                              WHEN DATEDIFF(SECOND, AttemptedAt, GETDATE()) > @maxSecs THEN @maxSecs
                              WHEN DATEDIFF(SECOND, AttemptedAt, GETDATE()) < 0 THEN 0
                              ELSE DATEDIFF(SECOND, AttemptedAt, GETDATE()) END,
                          Status = @status, CompletedAt = GETDATE()
                      WHERE Id = @id AND UserId = @user AND Status = 'InProgress'",
                    Db.P("@score", totalScore), Db.P("@total", totalMarks), Db.P("@pct", percentage),
                    Db.P("@c", correct), Db.P("@w", wrong), Db.P("@u", unanswered),
                    Db.P("@maxSecs", info.DurationMinutes * 60), Db.P("@status", status),
                    Db.P("@id", info.AttemptId), Db.P("@user", userId));

                if (rows != 1)
                {
                    tx.Rollback();
                    return false;
                }

                foreach (var rec in records)
                {
                    Db.Exec(conn, tx,
                        @"INSERT INTO AttemptAnswers (AttemptId, QuestionId, SelectedOption, IsCorrect, MarksAwarded)
                          VALUES (@a, @q, @s, @ok, @m)",
                        Db.P("@a", info.AttemptId), Db.P("@q", rec.Item1), Db.P("@s", rec.Item2),
                        Db.P("@ok", rec.Item3), Db.P("@m", rec.Item4));
                }

                tx.Commit();
            }
            return true;
        }

        // ------------------------------------------------------------
        // RESULT
        // ------------------------------------------------------------

        public static AttemptResult GetResult(int attemptId)
        {
            var result = Db.First(
                @"SELECT a.Id, a.UserId, a.Score, a.TotalMarks, a.Percentage, a.CorrectAnswers, a.WrongAnswers,
                         a.UnansweredQuestions, a.TimeTakenSeconds, a.Status, a.AttemptedAt,
                         q.Title, q.PassingPercentage
                  FROM Attempts a JOIN Quizzes q ON q.Id = a.QuizId
                  WHERE a.Id = @id AND a.Status <> 'InProgress'",
                r => new AttemptResult
                {
                    AttemptId = r.Int("Id"),
                    UserId = r.Int("UserId"),
                    Score = r.Dec("Score"),
                    TotalMarks = r.Int("TotalMarks"),
                    Percentage = r.Dec("Percentage"),
                    CorrectAnswers = r.Int("CorrectAnswers"),
                    WrongAnswers = r.Int("WrongAnswers"),
                    UnansweredQuestions = r.Int("UnansweredQuestions"),
                    TimeTakenSeconds = r.Int("TimeTakenSeconds"),
                    Status = r.Str("Status"),
                    AttemptedAt = r.Date("AttemptedAt"),
                    QuizTitle = r.Str("Title"),
                    PassingPercentage = r.Dec("PassingPercentage")
                },
                Db.P("@id", attemptId));

            if (result == null) return null;

            int n = 0;
            result.Review = Db.List(
                @"SELECT aa.QuestionId, aa.SelectedOption, aa.IsCorrect, aa.MarksAwarded,
                         q.QuestionText, q.OptionA, q.OptionB, q.OptionC, q.OptionD, q.CorrectOption, q.Explanation
                  FROM AttemptAnswers aa JOIN Questions q ON q.Id = aa.QuestionId
                  WHERE aa.AttemptId = @id
                  ORDER BY aa.Id",
                r => new AnswerReview
                {
                    Number = ++n,
                    QuestionId = r.Int("QuestionId"),
                    SelectedOption = r.NStr("SelectedOption") == null ? null : r.Str("SelectedOption").Trim(),
                    IsCorrect = r.Bool("IsCorrect"),
                    MarksAwarded = r.Dec("MarksAwarded"),
                    QuestionText = r.Str("QuestionText"),
                    OptionA = r.Str("OptionA"),
                    OptionB = r.Str("OptionB"),
                    OptionC = r.Str("OptionC"),
                    OptionD = r.Str("OptionD"),
                    CorrectOption = r.Str("CorrectOption").Trim(),
                    Explanation = r.NStr("Explanation")
                },
                Db.P("@id", attemptId));

            return result;
        }

        // ------------------------------------------------------------
        // HISTORY / DASHBOARDS / LEADERBOARD
        // ------------------------------------------------------------

        public static List<AttemptHistoryItem> GetHistory(int userId, int top)
        {
            return Db.List(
                @"SELECT TOP (@top) a.Id, q.Title, c.Name AS CategoryName, q.Difficulty, a.Score, a.TotalMarks,
                         a.Percentage, a.Status, a.TimeTakenSeconds, a.AttemptedAt
                  FROM Attempts a
                  JOIN Quizzes q ON q.Id = a.QuizId
                  JOIN Categories c ON c.Id = q.CategoryId
                  WHERE a.UserId = @u AND a.Status <> 'InProgress'
                  ORDER BY a.AttemptedAt DESC, a.Id DESC",
                r => new AttemptHistoryItem
                {
                    AttemptId = r.Int("Id"),
                    QuizTitle = r.Str("Title"),
                    CategoryName = r.Str("CategoryName"),
                    Difficulty = r.Str("Difficulty"),
                    Score = r.Dec("Score"),
                    TotalMarks = r.Int("TotalMarks"),
                    Percentage = r.Dec("Percentage"),
                    Status = r.Str("Status"),
                    TimeTakenSeconds = r.Int("TimeTakenSeconds"),
                    AttemptedAt = r.Date("AttemptedAt")
                },
                Db.P("@top", top), Db.P("@u", userId));
        }

        public static UserDashboard GetUserDashboard(int userId)
        {
            var user = AuthService.GetById(userId);
            var attempts = GetHistory(userId, 100000);

            return new UserDashboard
            {
                WelcomeName = user != null ? user.Name : "",
                AvailableQuizzes = QuizService.CountActive(),
                QuizzesAttempted = attempts.Count,
                AverageScore = attempts.Count > 0 ? Math.Round(attempts.Average(a => a.Percentage), 2) : 0,
                HighestScore = attempts.Count > 0 ? attempts.Max(a => a.Percentage) : 0,
                RecentAttempts = attempts.Take(5).ToList(),
                Performance = attempts.Take(10)
                    .Select(a => new PerformancePoint { QuizTitle = a.QuizTitle, Percentage = a.Percentage, AttemptedAt = a.AttemptedAt })
                    .OrderBy(p => p.AttemptedAt)
                    .ToList()
            };
        }

        public static AdminDashboard GetAdminDashboard()
        {
            var d = new AdminDashboard
            {
                TotalUsers = Db.ScalarInt("SELECT COUNT(*) FROM Users WHERE Role = 'User'"),
                TotalQuizzes = Db.ScalarInt("SELECT COUNT(*) FROM Quizzes"),
                TotalQuestions = Db.ScalarInt("SELECT COUNT(*) FROM Questions"),
                TotalAttempts = Db.ScalarInt("SELECT COUNT(*) FROM Attempts WHERE Status <> 'InProgress'")
            };

            d.QuizStats = Db.List(
                @"SELECT TOP 10 q.Title, COUNT(a.Id) AS Total,
                         AVG(CAST(a.Percentage AS FLOAT)) AS AvgScore,
                         SUM(CASE WHEN a.Status = 'Passed' THEN 1 ELSE 0 END) * 100.0 / COUNT(a.Id) AS PassRate
                  FROM Quizzes q
                  JOIN Attempts a ON a.QuizId = q.Id AND a.Status <> 'InProgress'
                  GROUP BY q.Id, q.Title
                  ORDER BY Total DESC",
                r => new QuizStat
                {
                    QuizTitle = r.Str("Title"),
                    TotalAttempts = r.Int("Total"),
                    AverageScore = r.Dec("AvgScore"),
                    PassRate = r.Dec("PassRate")
                });

            d.RecentAttempts = Db.List(
                @"SELECT TOP 20 u.Name, q.Title, a.Percentage, a.Status, a.AttemptedAt
                  FROM Attempts a
                  JOIN Users u ON u.Id = a.UserId
                  JOIN Quizzes q ON q.Id = a.QuizId
                  WHERE a.Status <> 'InProgress'
                  ORDER BY a.AttemptedAt DESC, a.Id DESC",
                r => new RecentAdminAttempt
                {
                    UserName = r.Str("Name"),
                    QuizTitle = r.Str("Title"),
                    Percentage = r.Dec("Percentage"),
                    Status = r.Str("Status"),
                    AttemptedAt = r.Date("AttemptedAt")
                });

            return d;
        }

        public static List<LeaderboardEntry> GetLeaderboard(int currentUserId)
        {
            var rank = 0;
            return Db.List(
                @"SELECT TOP 50 a.UserId, u.Name,
                         AVG(CAST(a.Percentage AS FLOAT)) AS AvgScore,
                         MAX(a.Percentage) AS HighScore,
                         COUNT(*) AS Total,
                         SUM(CASE WHEN a.Status = 'Passed' THEN 1 ELSE 0 END) AS Passed
                  FROM Attempts a JOIN Users u ON u.Id = a.UserId
                  WHERE a.Status <> 'InProgress'
                  GROUP BY a.UserId, u.Name
                  ORDER BY AvgScore DESC, Total DESC",
                r => new LeaderboardEntry
                {
                    Rank = ++rank,
                    UserId = r.Int("UserId"),
                    UserName = r.Str("Name"),
                    AverageScore = Math.Round(r.Dec("AvgScore"), 2),
                    HighestScore = r.Dec("HighScore"),
                    TotalAttempts = r.Int("Total"),
                    PassedCount = r.Int("Passed"),
                    IsCurrentUser = r.Int("UserId") == currentUserId
                });
        }
    }
}
