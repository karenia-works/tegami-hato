using System;
using Microsoft.EntityFrameworkCore;
using Karenia.TegamiHato.Server.Models;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Z.EntityFramework.Plus;
using System.Collections;

namespace Karenia.TegamiHato.Server.Services
{

    public enum AddResult
    {
        Success,
        AlreadyExist,
        Forbidden,
        NotFound,
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
        public async Task<HatoChannel> NewMailingChannel(
            string? channelName,
            bool isPublic,
            string channelTitle,
            Ulid creatorId)
        {
            var channelId = Ulid.NewUlid();
            var channel = new HatoChannel()
            {
                ChannelId = channelId,
                ChannelUsername = channelName ?? channelId.ToString(),
                ChannelTitle = channelTitle,
                IsPublic = isPublic
            };
            var result = await db.Channels.AddAsync(channel);

            db.ChannelUserTable.Add(new ChannelUserRelation()
            {
                UserId = creatorId,
                ChannelId = channelId,
                IsCreator = true,
                Permission = UserPermission.FullControl
            });
            var adminRoleId = Ulid.NewUlid();
            var regularRoleId = Ulid.NewUlid();

            await db.SaveChangesAsync();
            return channel;
        }

        public async Task<Ulid?> GetUserIdFromEmail(string email)
        {
            return await db.Users.AsQueryable()
                .Where(u => u.Email == email)
                .Select(u => u.UserId)
                .SingleOrDefaultAsync();
        }

        public async Task<User?> GetUserFromEmail(string email)
        {
            return await db.Users.AsQueryable()
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<HatoChannel?> GetChannelFromUsername(string name)
        {
            return await db.Channels.AsQueryable()
                .SingleOrDefaultAsync(ch => ch.ChannelUsername == name);
        }

        public async Task<HatoChannel?> GetChannelFromUsernameOrId(string name)
        {
            if (Ulid.TryParse(name, out var id))
                return await db
                    .Channels
                    .AsQueryable()
                    .SingleOrDefaultAsync(
                        ch => ch.ChannelId == id
                            || ch.ChannelUsername == name);
            else
                return await db.Channels.AsQueryable()
                    .SingleOrDefaultAsync(ch => ch.ChannelUsername == name);
        }

        public async Task<bool> ChannelNameExists(string name)
        {
            if (Ulid.TryParse(name, out var id))
            {
                return await db.Channels.AsQueryable().AnyAsync(
                    ch => ch.ChannelUsername == name || ch.ChannelId == id);
            }
            else
            {
                return await db.Channels.AsQueryable()
                    .AnyAsync(ch => ch.ChannelUsername == name);
            }
        }

        public async Task<User?> GetUser(Ulid userId)
        {
            return await db.Users.AsQueryable()
                .SingleOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<HatoChannel?> GetChannel(Ulid channelId)
        {
            return await db.Channels.AsQueryable()
                .SingleOrDefaultAsync(ch => ch.ChannelId == channelId);
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
        public async Task<IList<HatoMessage>> GetMessageFromChannelAsync(Ulid channelId, Ulid start, int count = 20, bool ascending = false)
        {
            if (count > MaxResultPerQuery)
                throw new ArgumentOutOfRangeException("count", count, $"A query can only check for at most {MaxResultPerQuery} results.");

            var partial = db.Messages.AsQueryable();

            if (ascending)
                partial = partial.Where(
                        message =>
                            (message.ChannelId == channelId)
                            && (message.MsgId.CompareTo(start) > 0));
            else
                partial = partial.Where(
                        message =>
                            (message.ChannelId == channelId)
                            && (message.MsgId.CompareTo(start) < 0));

            if (ascending)
                partial = partial.OrderBy(message => message.MsgId);
            else
                partial = partial.OrderByDescending(message => message.MsgId);

            partial = partial
                .Include(msg => msg.LinkedAttachments).ThenInclude(att => att.Attachment);
            partial = partial.Take(count);

            var msgs = await partial.ToListAsync();
            msgs.ForEach(
                p =>
                {
                    p.Attachments = p.LinkedAttachments
                        .Select(rel => rel.Attachment)
                        .ToList();
                    foreach (var att in p.Attachments) { att.LinkedMessages = null; }
                    p.LinkedAttachments = null;
                });
            return msgs;
        }

        public async Task<Ulid> SaveMessageIntoChannel(HatoMessage message)
        {
            return await SaveMessageIntoChannel(message, Ulid.Empty);
        }

        /// <summary>
        /// Add a message to a specific channel. The message's original ID will be overridden.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public async Task<Ulid> SaveMessageIntoChannel(HatoMessage message, Ulid channelId)
        {
            if (message.MsgId == Ulid.Empty)
                message.MsgId = Ulid.NewUlid();
            if (channelId != Ulid.Empty)
            {
                message.ChannelId = channelId;
            }

            await db.Messages.AddAsync(message);
            await db.SaveChangesAsync();
            return message.MsgId;
        }


        public IAsyncEnumerable<RecentMessageViewItem> GetRecentChannels(Ulid userId, int count = 20, int skip = 0)
        {
            if (count > MaxResultPerQuery)
                throw new ArgumentOutOfRangeException("count", count, $"A query can only check for at most {MaxResultPerQuery} results.");

            return db
                .RecentMessages.Join(
                    db.ChannelUserTable
                    .AsQueryable()
                    .Where(x => x.UserId == userId),
                    msg => msg.ChannelId,
                    ch => ch.ChannelId,
                    (r, i) => r)
                .Skip(skip)
                .Take(count)
                .AsAsyncEnumerable();
        }

        public async Task<AddResult> AddInvitationLink(
            InvitationLink link
        )
        {
            try
            {
                db.InvitationLinks.Add(link);
                await db.SaveChangesAsync();
                return AddResult.Success;
            }
            catch (DbUpdateException)
            {
                return AddResult.Forbidden;
            }
        }

        public async Task<UpdateResult> DeleteInvitationLink(
            Ulid channelId,
            string linkId
        )
        {
            var deleteNum = await db.InvitationLinks.AsQueryable()
                .Where(
                    link => link.LinkId == linkId
                        && link.ChannelId == channelId)
                        .DeleteFromQueryAsync();
            return deleteNum switch
            {
                0 => UpdateResult.NotExist,
                1 => UpdateResult.Success,
                _ => UpdateResult.UnexpectedMultiple
            };
        }

        public async Task<InvitationLink?> GetInvitationLink(
            string linkId
        )
        {
            return await db.InvitationLinks.AsQueryable()
                .SingleOrDefaultAsync(
                    link => link.LinkId == linkId);
        }
        public async Task<InvitationLink?> GetInvitationLink(
            Ulid channelId,
            string linkId
        )
        {
            return await db.InvitationLinks.AsQueryable()
                .SingleOrDefaultAsync(
                    link => link.LinkId == linkId
                        && link.ChannelId == channelId);
        }

        public async Task<List<InvitationLink>> GetInvitationLinks(
            Ulid channelId
        )
        {
            return await db.InvitationLinks.AsQueryable()
                .Where(inv => inv.ChannelId == channelId)
                .ToListAsync();
        }

        public async Task<AddResult> AddUserToChannel(
            Ulid userId,
            Ulid channelId,
            UserPermission? permission = null,
            bool addToPrivateChannel = false)
        {
            var userAlreadyInChannel = await db
                .ChannelUserTable
                .AsQueryable()
                .Where(
                    entry =>
                        entry.UserId == userId && entry.ChannelId == channelId
                )
                .AnyAsync();

            if (userAlreadyInChannel) return AddResult.AlreadyExist;

            var channel = await db.Channels.AsQueryable()
                .SingleOrDefaultAsync(ch => ch.ChannelId == channelId);

            if (channel == null) return AddResult.NotFound;
            if (!addToPrivateChannel && !channel.IsPublic) return AddResult.Forbidden;

            var relation = new ChannelUserRelation()
            {
                UserId = userId,
                ChannelId = channelId,
                IsCreator = false,
                Permission = permission ?? channel.DefaultPermission
            };

            db.ChannelUserTable.Add(relation);

            await db.SaveChangesAsync();
            return AddResult.Success;
        }

        public async Task<UpdateResult> RemoveUserFromCannel(
            Ulid userId,
            Ulid channelId)
        {
            var deleteResult = await db
               .ChannelUserTable
               .AsQueryable()
               .Where(
                   entry =>
                       entry.UserId == userId && entry.ChannelId == channelId
               )
               .DeleteAsync();

            return deleteResult switch
            {
                0 => UpdateResult.NotExist,
                1 => UpdateResult.Success,
                _ => UpdateResult.UnexpectedMultiple
            };
        }

        [Obsolete]
        public async Task<UpdateResult> EditUserPermissions(Ulid userId, Ulid channelId, ChannelUserRelation newPermissions)
        {
            var row = await db
                .ChannelUserTable
                .AsQueryable()
                .SingleOrDefaultAsync(
                    (row) => row.UserId == userId && row.ChannelId == channelId);

            if (row == null) return UpdateResult.NotExist;

            // row.CanEditRoles = newPermissions.CanEditRoles;
            // row.CanReceiveMessage = newPermissions.CanReceiveMessage;
            // row.CanSendMessage = newPermissions.CanSendMessage;
            await db.SaveChangesAsync();
            return UpdateResult.Success;
        }

        public IAsyncEnumerable<User> GetReceivers(Ulid channelId)
        {
            var result = db
                .ChannelUserTable
                .AsQueryable()
                .Where(entry =>
                    entry.ChannelId == channelId
                    && ((entry.Permission & UserPermission.Receive) != 0))
                .IncludeOptimized(entry => entry._User)
                .Select(entry => entry._User)
                .AsAsyncEnumerable();

            return result;
        }

        public class SimpleGroup<TK, TV> : IGrouping<TK, TV>
        {
            public SimpleGroup(TK key, List<TV> values)
            {
                Key = key;
                Values = values;
            }

            TK Key { get; set; }
            List<TV> Values { get; set; }

            TK IGrouping<TK, TV>.Key => this.Key;


            public IEnumerator<TV> GetEnumerator()
            {
                return this.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.Values.GetEnumerator();
            }
        }

        public async Task<List<IGrouping<string, string>>> GetAllReceiverEmails(
            ICollection<Ulid> channelIds)
        {
            var result = db
                .ChannelUserTable
                .AsQueryable()
                .Where(entry =>
                   channelIds.Contains(entry.ChannelId)
                    && ((entry.Permission & UserPermission.Receive) != 0))
                .Include(entry => entry._Channel)
                .Include(entry => entry._User)
                .AsAsyncEnumerable()
                .GroupBy(entry => entry._Channel.ChannelUsername, entry => entry._User.Email)
                .SelectAwait(async entry =>
                    (IGrouping<string, string>)
                    new SimpleGroup<string, string>(entry.Key, await entry.ToListAsync())
                ).ToListAsync();

            return await result;
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
                .AsQueryable()
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
            var sub = await db.ChannelUserTable
                .AsQueryable()
                .SingleOrDefaultAsync(
                    entry => entry.UserId == userId && entry.ChannelId == channelId
                );
            if (sub == null && !orNull)
                throw new ArgumentException($"User {userId} is not in channel {channelId}");
            else return sub;
        }


        public async Task<bool> CanUserDeleteInChannel(Ulid userId, Ulid channelId)
        {
            return await db.ChannelUserTable
                .AsQueryable()
                .AnyAsync(
                    entry =>
                        entry.UserId == userId
                        && entry.ChannelId == channelId
                        && ((entry.Permission & UserPermission.Edit) != 0)
                );
        }

        public async Task<bool> CanUserSendInChannel(Ulid userId, Ulid channelId)
        {
            return await db.ChannelUserTable
                .AsQueryable()
                .AnyAsync(
                    entry =>
                        entry.UserId == userId
                        && entry.ChannelId == channelId
                        && ((entry.Permission & UserPermission.Send) != 0)
                );
        }

        public async Task<UserPermission> GetUserPermission(Ulid userId, Ulid channelId)
        {
            return await db.ChannelUserTable.AsQueryable().Where(
                    entry =>
                        entry.UserId == userId
                        && entry.ChannelId == channelId
               ).Select(entry => entry.Permission).SingleOrDefaultAsync();
        }

        public async Task<bool> CanUserSendInChannel(string userEmail, Ulid channelId)
        {
            var user = await db.Users.AsQueryable()
                .SingleOrDefaultAsync(user => user.Email == userEmail);
            if (user == null) return false;

            return await CanUserSendInChannel(user.UserId, channelId);
        }

        public async ValueTask<(bool, Ulid)> GenerateLoginCodeOrAddUser(string userEmail, UserLoginCode code)
        {
            var user = await db.Users.AsQueryable()
                .SingleOrDefaultAsync(user => user.Email == userEmail);
            var newUser = user is null;
            if (user is null)
            {
                user = new User()
                {
                    UserId = Ulid.NewUlid(),
                    Email = userEmail,
                    Nickname = null,
                    _LoginCodes = new List<UserLoginCode>()
                };
                db.Add(user);
            }
            user._LoginCodes!.Add(code);
            await db.SaveChangesAsync();
            return (newUser, user.UserId);
        }

        public async Task<bool> CanUserLoginWithCode(string userEmail, string code)
        {
            var now = DateTimeOffset.Now;

            var result = await db.Users.AsQueryable()
                .Where(user => user.Email == userEmail)
                .SelectMany(user => user._LoginCodes.Where(
                        loginCode => loginCode.Code == code && loginCode.Expires > now))
                .DeleteAsync();

            return result > 0;
        }

        public async Task<IList<HatoAttachment>> GetAttachmentsFromIdAsync(IList<Ulid> ids)
        {
            return await this.db.Attachments.AsQueryable()
                .Where(att => ids.Contains(att.AttachmentId))
                .ToListAsync();
        }

        public async Task AddAttachmentEntry(HatoAttachment attachment)
        {
            db.Attachments.Add(attachment);
            await this.db.SaveChangesAsync();
        }

        public async Task AddAttachmentEntries(IEnumerable<HatoAttachment> attachments)
        {
            db.Attachments.AddRange(attachments);
            await this.db.SaveChangesAsync();
        }
    }
}
