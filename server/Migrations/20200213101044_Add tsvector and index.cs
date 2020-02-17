using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class Addtsvectorandindex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("create extension if not exists pg_trgm");
            migrationBuilder.Sql("create extension if not exists pg_jieba");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "tsvector",
                table: "messages",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_messages_MsgId",
                table: "messages",
                column: "MsgId");

            migrationBuilder.CreateIndex(
                name: "IX_messages_tsvector",
                table: "messages",
                column: "tsvector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.Sql(@"
            CREATE TRIGGER message_search_vector_update BEFORE INSERT OR UPDATE   
            ON messages FOR EACH ROW EXECUTE PROCEDURE
            tsvector_update_trigger(tsvector, 'public.jiebacfg', title, body_plain);");

            migrationBuilder.Sql("UPDATE messages SET title = title;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_messages_MsgId",
                table: "messages");

            migrationBuilder.DropIndex(
                name: "IX_messages_tsvector",
                table: "messages");

            migrationBuilder.DropColumn(
                name: "tsvector",
                table: "messages");

            migrationBuilder.Sql(@"drop trigger message_search_vector_update");
        }
    }
}
