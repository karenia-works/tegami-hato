using Microsoft.EntityFrameworkCore.Migrations;

namespace Karenia.TegamiHato.Server.Migrations
{
    public partial class FixNullRecentView : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
create or replace view recent_messages as
select 
    channels.channel_id,
    channels.channel_title, 
    max_msg.max_msg_id as msg_id,
    messages.title,
    messages.body_html as body_plain,
    messages.sender_email,
    messages.sender_nickname
from channels
    left outer join (
        select channel_id, max(msg_id) as max_msg_id from messages
        group by channel_id
    ) as max_msg on channels.channel_id = max_msg.channel_id
    join messages on max_msg.max_msg_id = messages.msg_id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
create or replace view recent_messages as
select 
    channels.channel_id,
    channels.channel_title, 
    max_msg.max_msg_id as msg_id,
    messages.title,
    messages.body_html as body_plain,
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
    }
}
