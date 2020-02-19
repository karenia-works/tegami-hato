using Microsoft.EntityFrameworkCore.Migrations;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class Addrecentmessageview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE OR REPLACE FUNCTION max(uuid, uuid)
RETURNS uuid AS $$
BEGIN
    IF $1 IS NULL OR $1 < $2 THEN
        RETURN $2;
    END IF;

    RETURN $1;
END;
$$ LANGUAGE plpgsql;


create or replace aggregate max(uuid) (
  sfunc = max,
  stype = uuid,
  combinefunc = max,
  parallel = safe,
  sortop = operator (>)
);            
            ");
            migrationBuilder.Sql(@"
create or replace view recent_messages as
select 
    channels.channel_id,
    channels.channel_title, 
    max_msg.max_msg_id as msg_id,
    messages.title,
    messages.body_plain,
    messages.sender_email,
    messages.sender_nickname
from channels
    join (
        select channel_id, max(msg_id) as max_msg_id from messages
        group by channel_id
    ) as max_msg on channels.channel_id = max_msg.channel_id
    join messages on max_msg.max_msg_id = messages.msg_id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"drop view if exists recent_messages");
            migrationBuilder.Sql(@"drop aggregate if exists max(uuid)");
            migrationBuilder.Sql(@"drop function if exists max(uuid,uuid)");
        }
    }
}
