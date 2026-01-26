using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPrefixedTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_020_Bookings_Courts_CourtId",
                table: "020_Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tournaments_TournamentId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Participants_020_Members_MemberId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_Participants_Tournaments_TournamentId",
                table: "Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatches_Matches_MatchId",
                table: "TournamentMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_TournamentMatches_Tournaments_TournamentId",
                table: "TournamentMatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TournamentMatches",
                table: "TournamentMatches");

            migrationBuilder.DropIndex(
                name: "IX_TournamentMatches_TournamentId",
                table: "TournamentMatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Participants",
                table: "Participants");

            migrationBuilder.DropIndex(
                name: "IX_Participants_TournamentId",
                table: "Participants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_News",
                table: "News");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Matches",
                table: "Matches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Courts",
                table: "Courts");

            migrationBuilder.RenameTable(
                name: "Tournaments",
                newName: "020_Tournaments");

            migrationBuilder.RenameTable(
                name: "TournamentMatches",
                newName: "020_TournamentMatches");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "020_RefreshTokens");

            migrationBuilder.RenameTable(
                name: "Participants",
                newName: "020_Participants");

            migrationBuilder.RenameTable(
                name: "News",
                newName: "020_News");

            migrationBuilder.RenameTable(
                name: "Matches",
                newName: "020_Matches");

            migrationBuilder.RenameTable(
                name: "Courts",
                newName: "020_Courts");

            migrationBuilder.RenameIndex(
                name: "IX_TournamentMatches_MatchId",
                table: "020_TournamentMatches",
                newName: "IX_020_TournamentMatches_MatchId");

            migrationBuilder.RenameIndex(
                name: "IX_Participants_MemberId",
                table: "020_Participants",
                newName: "IX_020_Participants_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_Matches_TournamentId",
                table: "020_Matches",
                newName: "IX_020_Matches_TournamentId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "020_Tournaments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "020_Tournaments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "020_Tournaments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Format",
                table: "020_Tournaments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "020_Tournaments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "020_Tournaments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "BracketGroup",
                table: "020_TournamentMatches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "020_RefreshTokens",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "020_RefreshTokens",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "JwtId",
                table: "020_RefreshTokens",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "020_Participants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "020_Participants",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "020_News",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPinned",
                table: "020_News",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "020_News",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "020_News",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "020_Matches",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Result",
                table: "020_Matches",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MatchFormat",
                table: "020_Matches",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "020_Courts",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "020_Courts",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "020_Courts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_020_Tournaments",
                table: "020_Tournaments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_020_TournamentMatches",
                table: "020_TournamentMatches",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_020_RefreshTokens",
                table: "020_RefreshTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_020_Participants",
                table: "020_Participants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_020_News",
                table: "020_News",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_020_Matches",
                table: "020_Matches",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_020_Courts",
                table: "020_Courts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_020_Tournaments_Status",
                table: "020_Tournaments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_020_TournamentMatches_TournamentId_Round_Position",
                table: "020_TournamentMatches",
                columns: new[] { "TournamentId", "Round", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_020_RefreshTokens_Token",
                table: "020_RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_020_RefreshTokens_UserId",
                table: "020_RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_020_Participants_TournamentId_MemberId",
                table: "020_Participants",
                columns: new[] { "TournamentId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_020_News_IsPinned",
                table: "020_News",
                column: "IsPinned");

            migrationBuilder.AddForeignKey(
                name: "FK_020_Bookings_020_Courts_CourtId",
                table: "020_Bookings",
                column: "CourtId",
                principalTable: "020_Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_020_Matches_020_Tournaments_TournamentId",
                table: "020_Matches",
                column: "TournamentId",
                principalTable: "020_Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_020_Participants_020_Members_MemberId",
                table: "020_Participants",
                column: "MemberId",
                principalTable: "020_Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_020_Participants_020_Tournaments_TournamentId",
                table: "020_Participants",
                column: "TournamentId",
                principalTable: "020_Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_020_TournamentMatches_020_Matches_MatchId",
                table: "020_TournamentMatches",
                column: "MatchId",
                principalTable: "020_Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_020_TournamentMatches_020_Tournaments_TournamentId",
                table: "020_TournamentMatches",
                column: "TournamentId",
                principalTable: "020_Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_020_Bookings_020_Courts_CourtId",
                table: "020_Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_020_Matches_020_Tournaments_TournamentId",
                table: "020_Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_020_Participants_020_Members_MemberId",
                table: "020_Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_020_Participants_020_Tournaments_TournamentId",
                table: "020_Participants");

            migrationBuilder.DropForeignKey(
                name: "FK_020_TournamentMatches_020_Matches_MatchId",
                table: "020_TournamentMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_020_TournamentMatches_020_Tournaments_TournamentId",
                table: "020_TournamentMatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_020_Tournaments",
                table: "020_Tournaments");

            migrationBuilder.DropIndex(
                name: "IX_020_Tournaments_Status",
                table: "020_Tournaments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_020_TournamentMatches",
                table: "020_TournamentMatches");

            migrationBuilder.DropIndex(
                name: "IX_020_TournamentMatches_TournamentId_Round_Position",
                table: "020_TournamentMatches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_020_RefreshTokens",
                table: "020_RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_020_RefreshTokens_Token",
                table: "020_RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_020_RefreshTokens_UserId",
                table: "020_RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_020_Participants",
                table: "020_Participants");

            migrationBuilder.DropIndex(
                name: "IX_020_Participants_TournamentId_MemberId",
                table: "020_Participants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_020_News",
                table: "020_News");

            migrationBuilder.DropIndex(
                name: "IX_020_News_IsPinned",
                table: "020_News");

            migrationBuilder.DropPrimaryKey(
                name: "PK_020_Matches",
                table: "020_Matches");

            migrationBuilder.DropPrimaryKey(
                name: "PK_020_Courts",
                table: "020_Courts");

            migrationBuilder.RenameTable(
                name: "020_Tournaments",
                newName: "Tournaments");

            migrationBuilder.RenameTable(
                name: "020_TournamentMatches",
                newName: "TournamentMatches");

            migrationBuilder.RenameTable(
                name: "020_RefreshTokens",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "020_Participants",
                newName: "Participants");

            migrationBuilder.RenameTable(
                name: "020_News",
                newName: "News");

            migrationBuilder.RenameTable(
                name: "020_Matches",
                newName: "Matches");

            migrationBuilder.RenameTable(
                name: "020_Courts",
                newName: "Courts");

            migrationBuilder.RenameIndex(
                name: "IX_020_TournamentMatches_MatchId",
                table: "TournamentMatches",
                newName: "IX_TournamentMatches_MatchId");

            migrationBuilder.RenameIndex(
                name: "IX_020_Participants_MemberId",
                table: "Participants",
                newName: "IX_Participants_MemberId");

            migrationBuilder.RenameIndex(
                name: "IX_020_Matches_TournamentId",
                table: "Matches",
                newName: "IX_Matches_TournamentId");

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Tournaments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Tournaments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "Format",
                table: "Tournaments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "BracketGroup",
                table: "TournamentMatches",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.AlterColumn<string>(
                name: "JwtId",
                table: "RefreshTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "TeamName",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Participants",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "News",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<bool>(
                name: "IsPinned",
                table: "News",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "News",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "News",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Matches",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Result",
                table: "Matches",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "MatchFormat",
                table: "Matches",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Courts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(120)",
                oldMaxLength: 120);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Courts",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Courts",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tournaments",
                table: "Tournaments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TournamentMatches",
                table: "TournamentMatches",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                table: "RefreshTokens",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Participants",
                table: "Participants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_News",
                table: "News",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Matches",
                table: "Matches",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Courts",
                table: "Courts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentMatches_TournamentId",
                table: "TournamentMatches",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Participants_TournamentId",
                table: "Participants",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_020_Bookings_Courts_CourtId",
                table: "020_Bookings",
                column: "CourtId",
                principalTable: "Courts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Tournaments_TournamentId",
                table: "Matches",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_020_Members_MemberId",
                table: "Participants",
                column: "MemberId",
                principalTable: "020_Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Participants_Tournaments_TournamentId",
                table: "Participants",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatches_Matches_MatchId",
                table: "TournamentMatches",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TournamentMatches_Tournaments_TournamentId",
                table: "TournamentMatches",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
