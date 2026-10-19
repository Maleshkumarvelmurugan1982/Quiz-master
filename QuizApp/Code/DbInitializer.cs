using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace QuizApp.Code
{
    /// <summary>
    /// Creates the QuizAppDB database automatically the first time the site runs,
    /// using App_Data/schema.sql. If the database already exists it is left untouched.
    /// Turn this off with &lt;add key="AutoCreateDatabase" value="false" /&gt; in Web.config.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>Set when the database could not be reached or created. Shown on the login page.</summary>
        public static string LastError { get; private set; }

        public static bool Ready { get; private set; }

        private static readonly object Sync = new object();

        public static void EnsureDatabase()
        {
            lock (Sync)
            {
                if (Ready) return;

                try
                {
                    var auto = ConfigurationManager.AppSettings["AutoCreateDatabase"];
                    var builder = new SqlConnectionStringBuilder(Db.ConnectionString);
                    var dbName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "QuizAppDB" : builder.InitialCatalog;

                    if (!string.Equals(auto, "false", StringComparison.OrdinalIgnoreCase) && !DatabaseExists(builder, dbName))
                    {
                        CreateDatabase(builder, dbName);
                    }

                    // Final check: can we query the Users table?
                    Db.ScalarInt("SELECT COUNT(*) FROM Users");

                    // Upgrade databases created by older versions of the project.
                    Upgrade();
                    LastError = null;
                    Ready = true;
                }
                catch (Exception ex)
                {
                    LastError = BuildFriendlyMessage(ex);
                    Ready = false;
                }
            }
        }

        /// <summary>Adds columns that newer versions need (safe to run every time).</summary>
        private static void Upgrade()
        {
            // Student section (A / B / C)
            if (!ColumnExists("Users", "Section"))
                Db.Exec("ALTER TABLE Users ADD Section NVARCHAR(5) NULL");
        }

        private static bool ColumnExists(string table, string column)
        {
            try
            {
                Db.Scalar("SELECT TOP 1 " + column + " FROM " + table); // names are constants, not user input
                return true;
            }
            catch (System.Data.Common.DbException)
            {
                return false;
            }
        }

        private static bool DatabaseExists(SqlConnectionStringBuilder builder, string dbName)
        {
            var master = new SqlConnectionStringBuilder(builder.ConnectionString) { InitialCatalog = "master" };
            using (var conn = new SqlConnection(master.ConnectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sys.databases WHERE name = @n", conn))
                {
                    cmd.Parameters.AddWithValue("@n", dbName);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        private static void CreateDatabase(SqlConnectionStringBuilder builder, string dbName)
        {
            var path = HostingEnvironment.MapPath("~/App_Data/schema.sql");
            if (path == null || !File.Exists(path))
                throw new FileNotFoundException("App_Data/schema.sql was not found, so the database could not be created.");

            var script = File.ReadAllText(path);
            if (!string.Equals(dbName, "QuizAppDB", StringComparison.OrdinalIgnoreCase))
                script = script.Replace("QuizAppDB", dbName);

            var batches = Regex.Split(script, @"^\s*GO\s*;?\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            var master = new SqlConnectionStringBuilder(builder.ConnectionString) { InitialCatalog = "master" };
            using (var conn = new SqlConnection(master.ConnectionString))
            {
                conn.Open();
                foreach (var batch in batches)
                {
                    if (string.IsNullOrWhiteSpace(batch)) continue;
                    using (var cmd = new SqlCommand(batch, conn))
                    {
                        cmd.CommandTimeout = 120;
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // New database: make sure the next connection does not reuse a pooled "master" context.
            SqlConnection.ClearAllPools();
        }

        private static string BuildFriendlyMessage(Exception ex)
        {
            var server = "(unknown)";
            try { server = new SqlConnectionStringBuilder(Db.ConnectionString).DataSource; }
            catch { }

            var msg = ex.Message;
            if (ex is SqlException)
            {
                return "Cannot connect to SQL Server \"" + server + "\". " +
                       "Make sure SQL Server Express is installed and running, or change the " +
                       "DefaultConnection connection string in Web.config. Details: " + msg;
            }
            return "Database setup failed: " + msg;
        }
    }
}
