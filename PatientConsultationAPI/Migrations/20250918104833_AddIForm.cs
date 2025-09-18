using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientConsultationAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddIForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Consultations");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentFileName",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentFileName",
                table: "Consultations");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Consultations",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
