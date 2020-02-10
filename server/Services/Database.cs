using System;
using Npgsql;
using Microsoft.EntityFrameworkCore;
using Karenia.TegamiHato.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using NUlid;
using System.Collections.Generic;
using Z.EntityFramework.Plus;
using System.Text.RegularExpressions;

namespace Karenia.TegamiHato.Server.Services
{

    public enum AddResult
    {
        Success,
        AlreadyExist,
        Forbidden,
    }

    public enum UpdateResult
    {
        Success,
        NotExist,
        Forbidden,

        /// <summary>
        /// This edit operation was intended to operate on one object but multiple was found. 
        /// Things might go wrong.
        /// </summary>
        UnexpectedMultiple,
    }

    public class DatabaseService
    {
        public DatabaseService(EmailSystemContext db)
        {
            this.db = db;
        }

        public const int MaxResultPerQuery = 50;
        public EmailSystemContext db;

        /// <summary>
        /// Create a new mailing channel with name and title set
        /// </summary>
        /// <param name="channelName"></param>
        /// <param name="channelTitle"></param>
        /// <returns>Channel ID</returns>
        public async Task<HatoChannel> NewMailingChannel(string? channelName, bool isPublic, string channelTitle)
        {
            var channelId = Ulid.NewUlid();
            var channel = new HatoChannel()
            {
                ChannelId = channelId,
                ChannelUsername = channelName,
                ChannelTitle = channelTitle,
                IsPublic = isPublic
            };
            var result = await db.Channels.AddAsync(channel);
            await db.SaveChangesAsync();
            return channel;
        }

        public async Task<User?> GetUserFromEmail(string email)
        {
            return await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<HatoChannel?> GetChannelFromUsername(string name)
        {
            return await db.Channels.SingleOrDefaultAsync(ch => ch.ChannelUsername == name);
        }


        public async Task<bool> ChannelNameExists(string name)
        {
            if (Ulid.TryParse(name, out var id))
            {
                return await db.Channels.AnyAsync(
                    ch => ch.ChannelUsername == name || ch.ChannelId == id);
            }
            else
            {
                return await db.Channels.AnyAsync(ch => ch.ChannelUsername == name);
            }
        }

        public async Task<User?> GetUser(Ulid userId)
        {
            return await db.Users.SingleOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<HatoChannel?> GetChannel(Ulid channelId)
        {
            return await db.Channels.SingleOrDefaultAsync(ch => ch.ChannelId == channelId);
        }

        /// <summary>
        /// Get messages from channel.
        /// 
        /// <para>
        ///     To get message from the very start, set <code>start</code> as <code>Ulid.MinValue</code>. Same for the very end.
        /// </para>
        /// </summary>
        /// <param name="channelId">Channel ID</param>
        /// <param name="start">Start ID of message (excluding)</param>
        /// <param name="count">Count of message, defaults to 20</param>
        /// <param name="ascending">Whether to get messages in time ascending order; default to false</param>
        /// <returns></returns>
        public IAsyncEnumerable<HatoMessage> GetMessageFromChannel(Ulid channelId, Ulid start, int count = 20, bool ascending = false)
        {
            if (count > MaxResultPerQuery)
                throw new ArgumentOutOfRangeException("count", count, $"A query can only check for at most {MaxResultPerQuery} results.");
            if (ascending)
                return db.Messages
                    .Where(
                        message =>
                            (message.ChannelId == channelId)
                            && (message.MsgId.CompareTo(start) > 0))
                    .Take(count)
                    .OrderBy(message => message.MsgId)
                    .AsAsyncEnumerable();
            else
                return db.Messages
                    .Where(
                        message => (message.ChannelId == channelId)
                        && (message.MsgId.CompareTo(start) < 0)
                    )
                    .Take(count)
                    .OrderByDescending(message => message.MsgId)
                    .AsAsyncEnumerable();
        }

        public async Task SaveMessageIntoChannel(HatoMessage message)
        {
            await SaveMessageIntoChannel(message, Ulid.Empty);
        }

        /// <summary>
        /// Add a message to a specific channel. The message's original ID will be overridden.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public async Task SaveMessageIntoChannel(HatoMessage message, Ulid channelId)
        {
            message.MsgId = Ulid.NewUlid();
            if (channelId != Ulid.Empty)
            {
                message.ChannelId = channelId;
            }

            await db.Messages.AddAsync(message);
            await db.SaveChangesAsync();
        }

#nullable disable
        public class RecentChannelEntry
        {
            public HatoChannel channel { get; set; }
            public string sender { get; set; }
            public string senderEmail { get; set; }
            public string latestMessage { get; set; }
            public DateTime timestamp { get; set; }
        }
#nullable restore

        public IAsyncEnumerable<RecentChannelEntry> GetRecentChannels(Ulid userId, int count = 20, int skip = 0)
        {
            if (count > MaxResultPerQuery)
                throw new ArgumentOutOfRangeException("count", count, $"A query can only check for at most {MaxResultPerQuery} results.");

            return db
                .ChannelUserTable
                .Where(entry => entry.UserId == userId)
                .Select(entry => new
                {
                    channel = entry._Channel,
                    id = entry._Channel._Messages.Max(message => message.MsgId)
                })
                .Join(
                    db.Messages,
                    ch => ch.id,
                    msg => msg.MsgId,
                    (ch, msg) => new RecentChannelEntry()
                    {
                        channel = ch.channel,
                        sender = msg.SenderNickname,
                        senderEmail = msg.SenderEmail,
                        latestMessage = msg.BodyPlain,
                        timestamp = msg.Timestamp
                    })
                .OrderByDescending(ch => ch.timestamp)
                .Skip(skip)
                .Take(count)
                .AsAsyncEnumerable();
        }

        public async Task<AddResult> AddUserToChannel(Ulid userId, Ulid channelId)
        {
            var userAlreadyInChannel = await db
                .ChannelUserTable.Where(
                    entry =>
                        entry.UserId == userId && entry.ChannelId == channelId
                )
                .AnyAsync();

            if (userAlreadyInChannel) return AddResult.AlreadyExist;

            await db.ChannelUserTable.AddAsync(new ChannelUserRelation()
            {
                UserId = userId,
                ChannelId = channelId,

                // TODO: Default permissions?
                CanSendMessage = false,
                CanReceiveMessage = true,
                CanEditRoles = false
            });
            await db.SaveChangesAsync();
            return AddResult.Success;
        }

        public async Task<UpdateResult> EditUserPermissions(Ulid userId, Ulid channelId, ChannelUserRelation newPermissions)
        {
            var row = await db
                .ChannelUserTable
                .SingleOrDefaultAsync(
                    (row) => row.UserId == userId && row.ChannelId == channelId);

            if (row == null) return UpdateResult.NotExist;

            row.CanEditRoles = newPermissions.CanEditRoles;
            row.CanReceiveMessage = newPermissions.CanReceiveMessage;
            row.CanSendMessage = newPermissions.CanSendMessage;
            await db.SaveChangesAsync();
            return UpdateResult.Success;
        }

        public IAsyncEnumerable<User> GetReceivers(Ulid channelId)
        {
            var result = db
                .ChannelUserTable
                .Where(entry => entry.ChannelId == channelId && entry.CanReceiveMessage)
                .IncludeOptimized(entry => entry._User)
                .Select(entry => entry._User)
                .AsAsyncEnumerable();

            return result;
        }

        /// <summary>
        /// Delete the specific message in a specific channel
        /// 
        /// <para>
        ///     The requirement of <c>ChannelId</c> is because we need to eliminate 
        ///     the possibility of deleting a message in one channel from another channel.
        /// </para>
        /// </summary>
        /// <param name="messageId"></param>
        /// <param name="channelId"></param>
        /// <returns>The number of rows deleted</returns>
        public async Task<UpdateResult> DeleteMessage(Ulid messageId, Ulid channelId)
        {
            var result = await db
                .Messages
                .Where(message => message.MsgId == messageId && message.ChannelId == channelId)
                .DeleteFromQueryAsync();

            await db.SaveChangesAsync();

            return result switch
            {
                0 => UpdateResult.NotExist,
                1 => UpdateResult.Success,
                _ => UpdateResult.UnexpectedMultiple
            };
        }

        public async Task<ChannelUserRelation?> GetSubscriptionEntry(Ulid userId, Ulid channelId, bool orNull = true)
        {
            var sub = await db.ChannelUserTable.SingleOrDefaultAsync(
                entry => entry.UserId == userId && entry.ChannelId == channelId
            );
            if (sub == null && !orNull)
                throw new ArgumentException($"User {userId} is not in channel {channelId}");
            else return sub;
        }

        public async Task<bool> CanUserDeleteInChannel(Ulid userId, Ulid channelId)
        {
            var sub = await GetSubscriptionEntry(userId, channelId, orNull: true);
            if (sub == null) return false;
            // TODO: use a specific field for this purpose
            else if (sub.CanEditRoles) return true;
            else return false;
        }

        public async Task<bool> CanUserSendInChannel(Ulid userId, Ulid channelId)
        {
            var sub = await GetSubscriptionEntry(userId, channelId, orNull: true);

            if (sub == null) return false;
            else if (sub.CanSendMessage) return true;
            else return false;
        }

        public async Task<bool> CanUserSendInChannel(string userEmail, Ulid channelId)
        {
            var user = await db.Users.SingleOrDefaultAsync(user => user.Email == userEmail);
            if (user == null) return false;

            return await CanUserSendInChannel(user.UserId, channelId);
        }
    }
}
