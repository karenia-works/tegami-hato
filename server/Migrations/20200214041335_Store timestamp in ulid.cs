using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class Storetimestampinulid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "timestamp",
                table: "messages");

            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "tsvector",
                table: "messages",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "tsvector",
                table: "messages",
                type: "tsvector",
                nullable: false,
                oldClrType: typeof(NpgsqlTsVector),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "timestamp",
                table: "messages",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
