﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Volte.Data;
using Volte.Data.Objects;
using Volte.Data.Objects.EventArgs;
using Volte.Extensions;
using Volte.Services;

namespace Volte.Discord
{
    public class VolteHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly GuildService _guild;
        private readonly DefaultWelcomeService _defaultWelcome;
        private readonly ImageWelcomeService _imageWelcome;
        private readonly AutoroleService _autorole;
        private readonly EventService _event;
        private readonly LoggingService _logger;
        private readonly VerificationService _verification;

        public VolteHandler(DiscordSocketClient client,
            CommandService commandService,
            GuildService guildService,
            DefaultWelcomeService defaultWelcomeService,
            ImageWelcomeService imageWelcomeService,
            AutoroleService autoroleService,
            EventService eventService,
            LoggingService loggingService,
            VerificationService verificationService)
        {
            _client = client;
            _service = commandService;
            _guild = guildService;
            _defaultWelcome = defaultWelcomeService;
            _imageWelcome = imageWelcomeService;
            _autorole = autoroleService;
            _event = eventService;
            _logger = loggingService;
            _verification = verificationService;
        }

        public async Task InitAsync()
        {
            var sw = Stopwatch.StartNew();
            var loaded = _service.AddModules(Assembly.GetExecutingAssembly());
            sw.Stop();
            await _logger.Log(LogSeverity.Info, LogSource.Volte,
                $"Loaded {loaded.Count} modules and {loaded.Sum(m => m.Commands.Count)} commands loaded in {sw.ElapsedMilliseconds}ms.");
            _client.Log += async (m) => await _logger.Log(new LogEventArgs(m));
            _client.JoinedGuild += async (guild) => await _guild.OnJoinAsync(new JoinedGuildEventArgs(guild));
            _client.LeftGuild += async (guild) => await _guild.OnLeaveAsync(new LeftGuildEventArgs(guild));
            _client.ReactionAdded += async (m, c, r) =>
                await _verification.CheckReactionAsync(new ReactionAddedEventArgs(m, c, r));
            _client.UserJoined += async (user) =>
            {
                var args = new UserJoinedEventArgs(user);
                if (Config.WelcomeApiKey.IsNullOrWhitespace())
                    await _defaultWelcome.JoinAsync(args);
                else
                    await _imageWelcome.JoinAsync(args);
            };
            _client.UserLeft += async (user) => await _defaultWelcome.LeaveAsync(new UserLeftEventArgs(user));
            _client.UserJoined += async (user) => await _autorole.ApplyRoleAsync(new UserJoinedEventArgs(user));
            _client.Ready += async () => await _event.OnReady(new ReadyEventArgs(_client));
            _client.MessageReceived += async (s) =>
            {
                if (!(s is IUserMessage msg)) return;
                if (msg.Author.IsBot) return;
                if (msg.Channel is IDMChannel)
                {
                    await msg.Channel.SendMessageAsync("Currently, I do not support commands via DM.");
                    return;
                }

                await _event.HandleMessageAsync(new MessageReceivedEventArgs(s));
            };
        }
    }
}