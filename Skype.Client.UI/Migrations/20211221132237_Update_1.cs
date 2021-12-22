using Microsoft.EntityFrameworkCore.Migrations;

namespace Skype.Client.UI.Migrations
{
    public partial class Update_1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Profile_Filters_FilterId",
                table: "Profile");

            migrationBuilder.DropForeignKey(
                name: "FK_Profile_Filters_SourceProfileVM_FilterId",
                table: "Profile");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Profile",
                table: "Profile");

            migrationBuilder.DropIndex(
                name: "IX_Profile_SourceProfileVM_FilterId",
                table: "Profile");

            migrationBuilder.DropColumn(
                name: "SourceProfileVM_FilterId",
                table: "Profile");

            migrationBuilder.RenameTable(
                name: "Profile",
                newName: "SourceProfiles");

            migrationBuilder.RenameColumn(
                name: "Discriminator",
                table: "SourceProfiles",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "ProfileId",
                table: "SourceProfiles",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_Profile_FilterId",
                table: "SourceProfiles",
                newName: "IX_SourceProfiles_FilterId");

            migrationBuilder.AlterColumn<string>(
                name: "TargetLink",
                table: "SourceProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FilterId",
                table: "SourceProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "SourceProfiles",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SourceProfiles",
                table: "SourceProfiles",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DestinationProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    TargetLink = table.Column<string>(type: "TEXT", nullable: false),
                    FilterId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DestinationProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DestinationProfiles_Filters_FilterId",
                        column: x => x.FilterId,
                        principalTable: "Filters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DestinationProfiles_FilterId",
                table: "DestinationProfiles",
                column: "FilterId");

            migrationBuilder.AddForeignKey(
                name: "FK_SourceProfiles_Filters_FilterId",
                table: "SourceProfiles",
                column: "FilterId",
                principalTable: "Filters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SourceProfiles_Filters_FilterId",
                table: "SourceProfiles");

            migrationBuilder.DropTable(
                name: "DestinationProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SourceProfiles",
                table: "SourceProfiles");

            migrationBuilder.RenameTable(
                name: "SourceProfiles",
                newName: "Profile");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Profile",
                newName: "Discriminator");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Profile",
                newName: "ProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_SourceProfiles_FilterId",
                table: "Profile",
                newName: "IX_Profile_FilterId");

            migrationBuilder.AlterColumn<string>(
                name: "TargetLink",
                table: "Profile",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "FilterId",
                table: "Profile",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "Profile",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "SourceProfileVM_FilterId",
                table: "Profile",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Profile",
                table: "Profile",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Profile_SourceProfileVM_FilterId",
                table: "Profile",
                column: "SourceProfileVM_FilterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_Filters_FilterId",
                table: "Profile",
                column: "FilterId",
                principalTable: "Filters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Profile_Filters_SourceProfileVM_FilterId",
                table: "Profile",
                column: "SourceProfileVM_FilterId",
                principalTable: "Filters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
