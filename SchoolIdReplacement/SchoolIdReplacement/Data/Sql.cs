using System;
using System.Data;
using System.Data.SqlClient;

namespace SchoolIdReplacement.Data
{
    /// <summary>
    /// Parameterized data access helper that routes seamlessly to SQL Server / LocalDB
    /// or to the built-in portable database when SQL Server is not installed on the PC.
    /// </summary>
    public static class Sql
    {
        public static SqlConnection Open()
        {
            var connection = new SqlConnection(Database.ConnectionString);
            connection.Open();
            return connection;
        }

        public static DataTable Query(string commandText, params SqlParameter[] parameters)
        {
            if (Database.UseEmbeddedStore)
            {
                return EmbeddedDb.Query(commandText, parameters);
            }

            using (var connection = Open())
            using (var command = new SqlCommand(commandText, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static int Execute(string commandText, params SqlParameter[] parameters)
        {
            if (Database.UseEmbeddedStore)
            {
                return EmbeddedDb.Execute(commandText, parameters);
            }

            using (var connection = Open())
            using (var command = new SqlCommand(commandText, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }

        public static object Scalar(string commandText, params SqlParameter[] parameters)
        {
            if (Database.UseEmbeddedStore)
            {
                return EmbeddedDb.Scalar(commandText, parameters);
            }

            using (var connection = Open())
            using (var command = new SqlCommand(commandText, connection))
            {
                if (parameters != null && parameters.Length > 0)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteScalar();
            }
        }

        public static SqlParameter P(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }
    }
}
