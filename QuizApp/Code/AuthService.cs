using System;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace QuizApp.Code
{
    public static class AuthService
    {
        private static AppUser Map(IDataRecord r)
        {
            return new AppUser
            {
                Id = r.Int("Id"),
                Name = r.Str("Name"),
                Email = r.Str("Email"),
                Role = r.Str("Role"),
                Section = r.NStr("Section"),
                IsActive = r.Bool("IsActive"),
                CreatedAt = r.Date("CreatedAt"),
                LastLogin = r.NDate("LastLogin")
            };
        }

        /// <summary>Returns the user, or null with an error message.</summary>
        public static AppUser Register(string name, string email, string password, string role, string section, out string error)
        {
            error = null;
            name = (name ?? "").Trim();
            email = (email ?? "").Trim();

            if (name.Length == 0 || email.Length == 0 || string.IsNullOrEmpty(password))
            {
                error = "Please fill in all fields.";
                return null;
            }
            if (name.Length > 100)
            {
                error = "Name must be 100 characters or fewer.";
                return null;
            }
            if (email.Length > 200 || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                error = "Please enter a valid email address.";
                return null;
            }
            if (password.Length < 6)
            {
                error = "Password must be at least 6 characters.";
                return null;
            }
            if (role != "Admin") role = "User";
            if (role == "User" && !UserService.IsValidSection(section))
            {
                error = "Please choose your section.";
                return null;
            }
            if (role == "Admin") section = null; // admins do not belong to a section

            var exists = Db.ScalarInt("SELECT COUNT(*) FROM Users WHERE Email = @e", Db.P("@e", email));
            if (exists > 0)
            {
                error = "Email already registered.";
                return null;
            }

            var id = Db.ScalarInt(
                @"INSERT INTO Users (Name, Email, PasswordHash, Role, Section, IsActive, CreatedAt, LastLogin)
                  VALUES (@n, @e, @h, @role, @sec, 1, GETDATE(), GETDATE());
                  SELECT CAST(SCOPE_IDENTITY() AS INT);",
                Db.P("@n", name), Db.P("@e", email), Db.P("@h", BCrypt.HashPassword(password)), Db.P("@sec", section), Db.P("@role", role));

            return GetById(id);
        }

        /// <summary>Returns the user when email + password are valid and the account is active.</summary>
        public static AppUser Login(string email, string password, out string error)
        {
            error = null;
            email = (email ?? "").Trim();
            if (email.Length == 0 || string.IsNullOrEmpty(password))
            {
                error = "Please fill in all fields.";
                return null;
            }

            string hash = null;
            var user = Db.First(
                "SELECT Id, Name, Email, Role, Section, IsActive, CreatedAt, LastLogin, PasswordHash FROM Users WHERE Email = @e",
                r => { hash = r.Str("PasswordHash"); return Map(r); },
                Db.P("@e", email));

            if (user == null || !user.IsActive || !BCrypt.Verify(password, hash))
            {
                error = "Invalid credentials or account is disabled.";
                return null;
            }

            Db.Exec("UPDATE Users SET LastLogin = GETDATE() WHERE Id = @id", Db.P("@id", user.Id));
            return user;
        }

        public static AppUser GetById(int id)
        {
            return Db.First(
                "SELECT Id, Name, Email, Role, Section, IsActive, CreatedAt, LastLogin FROM Users WHERE Id = @id",
                Map, Db.P("@id", id));
        }
    }
}
