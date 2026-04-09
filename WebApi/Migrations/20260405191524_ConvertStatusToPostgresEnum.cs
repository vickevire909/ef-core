using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApi.Migrations
{
    /// <inheritdoc />
    public partial class ConvertStatusToPostgresEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:order_status", "pending,processing,completed,cancelled");

            migrationBuilder.Sql(
                """
                    ALTER TABLE orders
                    ALTER COLUMN status TYPE order_status USING status::order_status
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                    ALTER TABLE orders
                    ALTER COLUMN status TYPE text USING status::text
                """
            );

            migrationBuilder
                .AlterDatabase()
                .OldAnnotation(
                    "Npgsql:Enum:order_status",
                    "pending,processing,completed,cancelled"
                );
        }
    }
}
