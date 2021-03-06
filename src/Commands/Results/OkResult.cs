using System;
using System.Threading.Tasks;
using Discord;
using Gommon;
using Volte.Services;

namespace Volte.Commands.Results
{
    public class OkResult : ActionResult
    {
        public OkResult(string text, bool shouldEmbed = true, EmbedBuilder embed = null,
            Func<IUserMessage, Task> func = null, bool awaitCallback = true)
        {
            _message = text;
            _shouldEmbed = shouldEmbed;
            _embed = embed;
            _callback = func;
            _runFuncAsync = awaitCallback;
        }

        public OkResult(Func<Task> logic, bool awaitFunc = true)
        {
            _separateLogic = logic;
            _runFuncAsync = awaitFunc;
        }

        private readonly bool _runFuncAsync;

        private readonly string _message;
        private readonly bool _shouldEmbed;
        private readonly Func<IUserMessage, Task> _callback;
        private readonly Func<Task> _separateLogic;
        private readonly EmbedBuilder _embed;

        public override async ValueTask<ResultCompletionData> ExecuteResultAsync(VolteContext ctx)
        {
            if (!ctx.Guild.CurrentUser.GetPermissions(ctx.Channel).SendMessages) return new ResultCompletionData();

            if (_separateLogic != null)
            {
                if (_runFuncAsync)
                    await _separateLogic();
                else
                    _ = _separateLogic();

                return new ResultCompletionData();
            }

            var data = ctx.Services.Get<DatabaseService>().GetData(ctx.Guild);

            IUserMessage message;
            if (_embed is null)
            {
                if (_shouldEmbed)
                    if (data.Configuration.ReplyInline)
                        message = await ctx.CreateEmbed(_message).ReplyToAsync(ctx.Message);
                    else
                        message = await ctx.CreateEmbed(_message).SendToAsync(ctx.Channel);
                else
                    if (data.Configuration.ReplyInline)
                        message = await ctx.Message.ReplyAsync(_message);
                    else
                        message = await ctx.Channel.SendMessageAsync(_message);
            }
            else if (ctx.GuildData.Configuration.ReplyInline)
                message = await _embed.ReplyToAsync(ctx.Message);
            else
                message = await _embed.SendToAsync(ctx.Channel);


            if (_callback != null)
            {
                if (_runFuncAsync)
                    await _callback(message);
                else
                    _ = _callback(message);
            }


            return new ResultCompletionData(message);
        }
    }
}