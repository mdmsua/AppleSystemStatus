using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AppleSystemStatus.Migrations
{
    public partial class User : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var username = Environment.GetEnvironmentVariable("ApplicationUsername")?.Trim();
            var password = Environment.GetEnvironmentVariable("ApplicationPassword")?.Trim();
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return;
            }

            var sql = GenerateCreateUserSql(username, password);
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var username = Environment.GetEnvironmentVariable("ApplicationUsername")?.Trim();
            if (string.IsNullOrEmpty(username))
            {
                return;
            }

            var sql = GenerateDropUserSql(username);
            migrationBuilder.Sql(sql);
        }

        private static string GenerateCreateUserSql(string username, string password)
        {
            var roles = new[] { "db_datareader", "db_datawriter", "db_ddladmin" };
            var builder = new StringBuilder();
            builder.AppendFormat("IF NOT EXISTS(SELECT TOP(1) principal_id FROM sys.database_principals WHERE name = N'{0}')", username);
            builder.AppendLine();
            builder.AppendLine("BEGIN");
            builder.AppendFormat("CREATE USER {0} WITH PASSWORD = N'{1}'", username, password);
            builder.AppendLine();
            foreach (var role in roles)
            {
                builder.AppendFormat("ALTER ROLE {0} ADD MEMBER {1}", role, username);
                builder.AppendLine();
            }
            builder.AppendLine("END");
            return builder.ToString();
        }

        private static string GenerateDropUserSql(string username)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("IF EXISTS(SELECT TOP(1) principal_id FROM sys.database_principals WHERE name = N'{0}')", username);
            builder.AppendLine();
            builder.AppendLine("BEGIN");
            builder.AppendFormat("DROP USER {0}", username);
            builder.AppendLine();
            builder.AppendLine("END");
            return builder.ToString();
        }
    }
}
