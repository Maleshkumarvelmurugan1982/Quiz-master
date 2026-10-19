using System.Collections.Generic;
using System.Linq;

namespace QuizApp.Code
{
    public static class UserService
    {
        /// <summary>The class sections a student can belong to.</summary>
        public static readonly string[] Sections = { "A", "B", "C" };

        public static bool IsValidSection(string section)
        {
            return Sections.Contains(section);
        }

        /// <summary>
        /// Students, optionally filtered by search text and section.
        /// section: "" = all sections, "A"/"B"/"C", or "NONE" = students with no section yet.
        /// </summary>
        public static List<UserListItem> GetUsers(string search, string section)
        {
            search = (search ?? "").Trim();
            section = (section ?? "").Trim();
            var like = "%" + search.Replace("[", "[[]").Replace("%", "[%]").Replace("_", "[_]") + "%";

            return Db.List(
                @"SELECT TOP 200 u.Id, u.Name, u.Email, u.Section, u.IsActive, u.CreatedAt, u.LastLogin,
                         (SELECT COUNT(*) FROM Attempts a WHERE a.UserId = u.Id AND a.Status <> 'InProgress') AS TotalAttempts,
                         ISNULL((SELECT AVG(CAST(a.Percentage AS FLOAT)) FROM Attempts a
                                 WHERE a.UserId = u.Id AND a.Status <> 'InProgress'), 0) AS AvgScore
                  FROM Users u
                  WHERE u.Role = 'User'
                    AND (@s = '' OR u.Name LIKE @like OR u.Email LIKE @like)
                    AND (@sec = '' OR (@sec = 'NONE' AND u.Section IS NULL) OR u.Section = @sec)
                  ORDER BY u.Section, u.Name",
                r => new UserListItem
                {
                    Id = r.Int("Id"),
                    Name = r.Str("Name"),
                    Email = r.Str("Email"),
                    Section = r.NStr("Section"),
                    IsActive = r.Bool("IsActive"),
                    CreatedAt = r.Date("CreatedAt"),
                    LastLogin = r.NDate("LastLogin"),
                    TotalAttempts = r.Int("TotalAttempts"),
                    AverageScore = System.Math.Round(r.Dec("AvgScore"), 2)
                },
                Db.P("@s", search), Db.P("@like", like), Db.P("@sec", section));
        }

        /// <summary>Number of students in each section, e.g. {"A": 12, "B": 9, "C": 10, "": 2 (no section)}.</summary>
        public static Dictionary<string, int> CountBySection()
        {
            var counts = Sections.ToDictionary(s => s, s => 0);
            counts[""] = 0;
            foreach (var row in Db.List(
                "SELECT ISNULL(Section, '') AS Section, COUNT(*) AS Total FROM Users WHERE Role = 'User' GROUP BY Section",
                r => new { Section = r.Str("Section"), Total = r.Int("Total") }))
            {
                var key = counts.ContainsKey(row.Section) ? row.Section : "";
                counts[key] += row.Total;
            }
            return counts;
        }

        /// <summary>Moves a student to another section.</summary>
        public static bool SetSection(int userId, string section)
        {
            if (!IsValidSection(section)) return false;
            return Db.Exec("UPDATE Users SET Section = @sec WHERE Id = @id AND Role = 'User'",
                Db.P("@sec", section), Db.P("@id", userId)) > 0;
        }

        /// <summary>Activates / deactivates a user. Admins cannot disable themselves.</summary>
        public static bool Toggle(int userId, int currentUserId, out string message)
        {
            if (userId == currentUserId)
            {
                message = "Cannot disable your own account.";
                return false;
            }

            var user = AuthService.GetById(userId);
            if (user == null)
            {
                message = "User not found.";
                return false;
            }

            Db.Exec("UPDATE Users SET IsActive = @a WHERE Id = @id",
                Db.P("@a", !user.IsActive), Db.P("@id", userId));
            message = "User " + (user.IsActive ? "deactivated" : "activated") + ".";
            return true;
        }
    }
}
