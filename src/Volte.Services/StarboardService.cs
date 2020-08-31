using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Gommon;
using Volte.Core.Helpers;
using Volte.Core.Entities;

namespace Volte.Services
{
    public sealed class StarboardService : VolteEventService
    {
        private readonly DatabaseService _db;
        private readonly DiscordShardedClient _client;

        // Ensures starboard message creations don't happen twice, and edits are atomic.
        private readonly AsyncDuplicateLock<ulong> _messageWriteLock;

        private readonly DiscordEmoji _starEmoji = DiscordEmoji.FromUnicode(EmojiHelper.Star);

        public StarboardService(DatabaseService databaseService, DiscordShardedClient discordShardedClient,
            AsyncDuplicateLock<ulong> messageWriteLock)
        {
            _db = databaseService;
            _client = discordShardedClient;
            _messageWriteLock = messageWriteLock;
        }


        public override Task DoAsync(EventArgs args)
        {
            return args switch
            {
                MessageReactionAddEventArgs reactionAdd => HandleReactionAddAsync(reactionAdd),
                MessageReactionRemoveEventArgs reactionRemove => HandleReactionRemoveAsync(reactionRemove),
                MessageReactionsClearEventArgs reactionsClear => HandleReactionsClearAsync(reactionsClear),
                _ => Task.CompletedTask
            };
        }

        private async Task HandleReactionAddAsync(MessageReactionAddEventArgs args)
        {
            if (args.Channel is DiscordDmChannel) return;
            if (args.Emoji.Name != EmojiHelper.Star) return;
            if (args.User.IsCurrent) return;
            
            var data = _db.GetData(args.Guild.Id);
            var starboard = data.Configuration.Starboard;
            
            var starboardChannel = await args.Client.GetChannelAsync(starboard.StarboardChannel);
            if (starboardChannel is null) return;
            if (args.Channel == starboardChannel) return; // TODO Support starring the starboard message

            var messageId = args.Message.Id;
            var starrerId = args.User.Id;
            
            if (data.Extras.StarboardedMessages.TryGetValue(messageId, out var entry))
            {
                // Add the star to the database
                if (entry.StarredUserIds.Add(starrerId))
                {
                    // Update message star count
                    using (await _messageWriteLock.LockAsync(messageId))
                    {
                        await UpdateOrPostToStarboardAsync(starboard, args.Message, entry);
                    }
                }
                else
                {
                    // Invalid star! Either the starboard post or the actual message already has a reaction by this user.
                    if (starboard.DeleteInvalidStars)
                    {
                        await args.Message.DeleteReactionAsync(_starEmoji, args.User, "Star reaction is invalid: User has already starred!");
                    }
                }
            }
            else
            {
                if (args.Message.Reactions.FirstOrDefault(e => e.Emoji == _starEmoji)?.Count >= starboard.StarsRequiredToPost)
                {
                    // Create new star message!
                    using (await _messageWriteLock.LockAsync(messageId))
                    {
                        var newEntry = data.Extras.StarboardedMessages.AddOrUpdate(
                            messageId,
                            id => new StarboardEntry
                            {
                                StarredUserIds = {starrerId},
                                MessageId = messageId
                            },
                            (id, existingEntry) =>
                            {
                                existingEntry.StarredUserIds.Add(starrerId);
                                return existingEntry;
                            }
                        );
                        await UpdateOrPostToStarboardAsync(starboard, args.Message, newEntry);
                    }
                }
            }
        }

        private async Task HandleReactionRemoveAsync(MessageReactionRemoveEventArgs args)
        {
            if (args.Channel is DiscordDmChannel) return;
            if (args.Emoji.Name != EmojiHelper.Star) return;
            if (args.User.IsCurrent) return;
            
            var data = _db.GetData(args.Guild.Id);
            var starboard = data.Configuration.Starboard;
            
            var starboardChannel = await args.Client.GetChannelAsync(starboard.StarboardChannel);
            if (starboardChannel is null) return;
            if (args.Channel == starboardChannel) return; // TODO Support starring the starboard message

            var messageId = args.Message.Id;
            var starrerId = args.User.Id;

            if (data.Extras.StarboardedMessages.TryGetValue(messageId, out var entry))
            {
                // Add the star to the database
                if (entry.StarredUserIds.Remove(starrerId))
                {
                    // Update message star count
                    using (await _messageWriteLock.LockAsync(messageId))
                    {
                        if (entry.StarCount < starboard.StarsRequiredToPost)
                        {
                            // In this case, due to locking, StarboardedMessages[messageId] should always == entry,
                            // so we do not need to pass a value to TryRemove.
                            data.Extras.StarboardedMessages.TryRemove(messageId, out _);
                            await UpdateOrPostToStarboardAsync(starboard, args.Message, entry);
                        }
                    }
                }
            }
        }

        private async Task HandleReactionsClearAsync(MessageReactionsClearEventArgs args)
        {
            await Task.Yield();
        }
        
        /// <summary>
        ///     Updates or posts a message to the starboard in a guild.
        ///     Calls to this method should be synchronized to _messageWriteLock beforehand!
        /// </summary>
        /// <param name="starboard">The guild's starboard configuration</param>
        /// <param name="message">The message to star</param>
        /// <param name="entry"></param>
        /// <returns></returns>
        private async Task UpdateOrPostToStarboardAsync(StarboardOptions starboard, DiscordMessage message, StarboardEntry entry)
        {
            var starboardChannel = message.Channel.Guild.GetChannel(starboard.StarboardChannel);
            if (starboardChannel is null)
            {
                return;
            }

            if (entry.StarboardMessageId == 0)
            {
                if (entry.StarCount >= starboard.StarsRequiredToPost)
                {
                    // New message just reached star threshold, send it
                    var newMessage = await PostToStarboardAsync(message, entry.StarCount);
                    entry.StarboardMessageId = newMessage.Id;
                }
            }
            else
            {
                DiscordMessage starboardMessage;
                try
                {
                    starboardMessage = await starboardChannel.GetMessageAsync(entry.StarboardMessageId);
                }
                catch (NotFoundException)
                {
                    // Ignore, maybe log to console
                    return;
                }

                if (entry.StarCount >= starboard.StarsRequiredToPost)
                {
                    // Update existing message
                    var targetMessage = $"{EmojiHelper.Star} {entry.StarCount}";
                    if (starboardMessage.Content != targetMessage)
                    {
                        await starboardMessage.ModifyAsync(targetMessage);
                    }
                }
                else
                {
                    // Unstarred below the limit so delete the message if any
                    await starboardMessage.DeleteAsync();
                    entry.StarboardMessageId = 0;
                }
            }
        }

        private async Task<DiscordMessage> PostToStarboardAsync(DiscordMessage message, int starCount)
        {
            var data = _db.GetData(message.Channel.Guild);
            
            var starboardChannel = message.Channel.Guild.GetChannel(data.Configuration.Starboard.StarboardChannel);
            if (starboardChannel is null)
            {
                return null;
            }

            var e = new DiscordEmbedBuilder()
                .WithSuccessColor()
                .WithDescription(message.Content)
                .WithAuthor(message.Author)
                .AddField("Original Message", message.JumpLink);

            var result = await starboardChannel.SendMessageAsync($"{_starEmoji} {starCount}", embed: e.Build());
            await result.CreateReactionAsync(_starEmoji);
            return result;
        }
    }
}