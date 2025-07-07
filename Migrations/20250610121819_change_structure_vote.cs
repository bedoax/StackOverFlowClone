using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StackOverFlowClone.Migrations
{
    /// <inheritdoc />
    public partial class change_structure_vote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Answers_TargetId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Questions_TargetId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_TargetId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastActiveAt",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "AnswerId",
                table: "Votes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuestionId",
                table: "Votes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votes_AnswerId",
                table: "Votes",
                column: "AnswerId");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_QuestionId",
                table: "Votes",
                column: "QuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Answers_AnswerId",
                table: "Votes",
                column: "AnswerId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Questions_QuestionId",
                table: "Votes",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Answers_AnswerId",
                table: "Votes");

            migrationBuilder.DropForeignKey(
                name: "FK_Votes_Questions_QuestionId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_AnswerId",
                table: "Votes");

            migrationBuilder.DropIndex(
                name: "IX_Votes_QuestionId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "AnswerId",
                table: "Votes");

            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "Votes");

            migrationBuilder.AddColumn<int>(
                name: "TargetId",
                table: "Votes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastActiveAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Votes_TargetId",
                table: "Votes",
                column: "TargetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Answers_TargetId",
                table: "Votes",
                column: "TargetId",
                principalTable: "Answers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Votes_Questions_TargetId",
                table: "Votes",
                column: "TargetId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
