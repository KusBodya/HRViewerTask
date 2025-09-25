using System;
using System.Data;
using System.Data.SqlClient;

namespace HRViewer
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public DataTable GetStatuses() => ExecToTable("dbo.usp_GetStatuses");
        public DataTable GetDepartments() => ExecToTable("dbo.usp_GetDepartments");
        public DataTable GetPositions() => ExecToTable("dbo.usp_GetPositions");

        public DataTable GetEmployees(int? statusId, int? departmentId, int? positionId, string lastNameFilter)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.usp_GetEmployees", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@StatusId", SqlDbType.Int).Value = (object?)statusId ?? DBNull.Value;
            cmd.Parameters.Add("@DepartmentId", SqlDbType.Int).Value = (object?)departmentId ?? DBNull.Value;
            cmd.Parameters.Add("@PositionId", SqlDbType.Int).Value = (object?)positionId ?? DBNull.Value;
            cmd.Parameters.Add("@LastNameFilter", SqlDbType.NVarChar, 100).Value =
                string.IsNullOrWhiteSpace(lastNameFilter) ? (object)DBNull.Value : lastNameFilter;

            var dt = new DataTable();
            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        public DataTable GetEmployeeStatistics(int statusId, DateTime from, DateTime to, bool isHiredStats)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("dbo.usp_GetEmployeeStatistics", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@StatusId", SqlDbType.Int).Value = statusId;
            cmd.Parameters.Add("@FromDate", SqlDbType.Date).Value = from.Date;
            cmd.Parameters.Add("@ToDate", SqlDbType.Date).Value = to.Date;
            cmd.Parameters.Add("@IsHiredStats", SqlDbType.Bit).Value = isHiredStats;

            var dt = new DataTable();
            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }

        private DataTable ExecToTable(string storedProcName)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(storedProcName, conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            var dt = new DataTable();
            using var da = new SqlDataAdapter(cmd);
            da.Fill(dt);
            return dt;
        }
    }
}
