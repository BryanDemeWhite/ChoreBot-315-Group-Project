using System;
using System.Collections.Generic;
using Npgsql;

namespace ChoreBot
{
    public class DBConnection
    {
        private static string host = "ec2-54-159-244-207.compute-1.amazonaws.com";
        private static string database = "d1vk5aq0vjqvl0";
        private static string user = "tgobwhkzsbmmpx";
        private static string password = "7ba3e034c1e123fcf2c484f56d7179b7456dd8d80990472b163a9be483474a98";

        private static string connStr = $"Host={host};Username={user};Password={password};Database={database};SSL Mode=Prefer;Trust Server Certificate=true";
        NpgsqlConnection conn;

        /// <summary>
        /// Connects to the database
        /// </summary>
        public DBConnection()
        {
            try
            {
                conn = new NpgsqlConnection(connStr);
                conn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Runs the query and returns the full table output as a <see cref="NpgsqlDataReader">NpgsqlDataReader</see>
        /// </summary>
        /// <param name="sql">The SQL command</param>
        public NpgsqlDataReader runQuery(string sql)
        {
            NpgsqlDataReader reader; 
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                reader = cmd.ExecuteReader();
            }

            return reader;
        }

        /// <summary>
        /// Runs the command and returns the number of columns affected
        /// </summary>
        /// <param name="sql">The SQL command</param>
        public int runCommand(string sql)
        {
            int cols;
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cols = cmd.ExecuteNonQuery();
            }

            return cols;
        }

        /// <summary>
        /// Runs the SQL query and returns the first cell from the output
        /// </summary>
        /// <param name="sql">The SQL command</param>
        public string runScalar(string sql)
        {
            try
            {
                string output;
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    output = (string)cmd.ExecuteScalar();
                }
                return output ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}
