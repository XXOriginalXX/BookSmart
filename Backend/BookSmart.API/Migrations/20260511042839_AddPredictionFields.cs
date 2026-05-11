using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookSmart.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPredictionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Alcoholism",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Diabetes",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Gender",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Handicap",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Hypertension",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHighRisk",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Neighbourhood",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "NoShowProbability",
                table: "Appointments",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scholarship",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SmsReceived",
                table: "Appointments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Alcoholism",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Diabetes",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Handicap",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Hypertension",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "IsHighRisk",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Neighbourhood",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "NoShowProbability",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "Scholarship",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "SmsReceived",
                table: "Appointments");
        }
    }
}
