using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CleanupDatabaseWebJob
{
    class Program
    {
        static void Main()
        {
            var connectionString = Environment.GetEnvironmentVariable("SQLCONNSTR_MS_TableConnectionString");

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand sqlCommand = sqlConnection.CreateCommand())
                {
                    Debug.WriteLine("[CleanupDatabaseWebJob] Initiating SQL Connection");
                    sqlConnection.Open();

                    Debug.WriteLine("[CleanupDatabaseWebJob] Executing SQL Statement");
                    sqlCommand.CommandText = "DELETE FROM [dbo].[TodoItems] WHERE [deleted] AND[updatedAt] < DATEADD(day, -7, SYSDATETIMEOFFSET())";
                    var rowsAffected = sqlCommand.ExecuteNonQuery();
                    Debug.WriteLine($"[CleanupDatabaseWebJob] {rowsAffected} rows deleted.");

                    sqlConnection.Close();
                }
            }
        }
    }
}
