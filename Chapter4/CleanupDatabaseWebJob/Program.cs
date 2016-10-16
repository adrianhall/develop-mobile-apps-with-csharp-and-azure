using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CleanupDatabaseWebJob
{
    class Program
    {
        static void Main()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MS_TableConnectionString"].ConnectionString;

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    Console.WriteLine("[CleanupDatabaseWebJob] Initiating SQL Connection");
                    sqlConnection.Open();

                    Console.WriteLine("[CleanupDatabaseWebJob] Executing SQL Statement");
                    sqlCommand.CommandText = "DELETE FROM [dbo].[TodoItems] WHERE [deleted] = 1 AND [updatedAt] < DATEADD(day, -7, SYSDATETIMEOFFSET())";
                    var rowsAffected = sqlCommand.ExecuteNonQuery();
                    Console.WriteLine($"[CleanupDatabaseWebJob] {rowsAffected} rows deleted.");

                    sqlConnection.Close();
                }
            }
        }
    }
}
