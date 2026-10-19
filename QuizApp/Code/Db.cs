using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace QuizApp.Code
{
    /// <summary>
    /// Small ADO.NET helper (replaces Entity Framework Core from the old project).
    /// Connects to SQL Server with SqlClient. Every query uses SQL parameters,
    /// so user input is never concatenated into SQL.
    /// </summary>
    public static class Db
    {
        public static string ConnectionString
        {
            get
            {
                var cs = ConfigurationManager.ConnectionStrings["DefaultConnection"];
                if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                    throw new ConfigurationErrorsException(
                        "Connection string 'DefaultConnection' is missing in Web.config.");
                return cs.ConnectionString;
            }
        }

        public static DbConnection Open()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static DbParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        private static DbCommand Cmd(DbConnection conn, DbTransaction tx, string sql, DbParameter[] ps)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = tx;
            cmd.CommandTimeout = 60;
            if (ps != null)
            {
                foreach (var p in ps)
                    cmd.Parameters.Add(p);
            }
            return cmd;
        }

        // ------------------------------------------------------------
        // Simple (own connection)
        // ------------------------------------------------------------

        public static List<T> List<T>(string sql, Func<IDataRecord, T> map, params DbParameter[] ps)
        {
            using (var conn = Open())
            {
                return List(conn, null, sql, map, ps);
            }
        }

        public static T First<T>(string sql, Func<IDataRecord, T> map, params DbParameter[] ps) where T : class
        {
            var list = List(sql, map, ps);
            return list.Count > 0 ? list[0] : null;
        }

        public static object Scalar(string sql, params DbParameter[] ps)
        {
            using (var conn = Open())
            {
                return Scalar(conn, null, sql, ps);
            }
        }

        public static int Exec(string sql, params DbParameter[] ps)
        {
            using (var conn = Open())
            {
                return Exec(conn, null, sql, ps);
            }
        }

        public static int ScalarInt(string sql, params DbParameter[] ps)
        {
            var v = Scalar(sql, ps);
            return v == null ? 0 : Convert.ToInt32(v);
        }

        // ------------------------------------------------------------
        // With an existing connection / transaction
        // ------------------------------------------------------------

        public static List<T> List<T>(DbConnection conn, DbTransaction tx, string sql,
            Func<IDataRecord, T> map, params DbParameter[] ps)
        {
            var result = new List<T>();
            using (var cmd = Cmd(conn, tx, sql, ps))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                    result.Add(map(reader));
            }
            return result;
        }

        public static object Scalar(DbConnection conn, DbTransaction tx, string sql, params DbParameter[] ps)
        {
            using (var cmd = Cmd(conn, tx, sql, ps))
            {
                var value = cmd.ExecuteScalar();
                return value == DBNull.Value ? null : value;
            }
        }

        public static int Exec(DbConnection conn, DbTransaction tx, string sql, params DbParameter[] ps)
        {
            using (var cmd = Cmd(conn, tx, sql, ps))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static int ScalarInt(DbConnection conn, DbTransaction tx, string sql, params DbParameter[] ps)
        {
            var v = Scalar(conn, tx, sql, ps);
            return v == null ? 0 : Convert.ToInt32(v);
        }
    }

    /// <summary>Null-safe column readers.</summary>
    public static class ReaderExtensions
    {
        public static string Str(this IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? "" : Convert.ToString(v);
        }

        public static string NStr(this IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? null : Convert.ToString(v);
        }

        public static int Int(this IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? 0 : Convert.ToInt32(v);
        }

        public static int? NInt(this IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? (int?)null : Convert.ToInt32(v);
        }

        public static decimal Dec(this IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? 0m : Convert.ToDecimal(v);
        }

        public static bool Bool(this IDataRecord r, string col)
        {
            var v = r[col];
            return v != DBNull.Value && Convert.ToBoolean(v);
        }

        public static DateTime Date(this IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(v);
        }

        public static DateTime? NDate(this IDataRecord r, string col)
        {
            var v = r[col];
            return v == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(v);
        }
    }
}
