using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Qmmands;
using Volte.Commands;
using Volte.Core;
using Volte.Core.Entities;
using Module = Qmmands.Module;

namespace Gommon
{
    public static partial class Extensions
    {
        public static string SanitizeName(this Module m)
            => m.Name.Replace("Module", string.Empty);
        
        public static string SanitizeParserName(this Type type)
            => type.Name.Replace("Parser", string.Empty);

        public static string GetUsage(this Command c, VolteContext ctx)
            => (c.Remarks ?? "No usage provided")
                .Replace(c.Name.ToLower(), c.AsPrettyString().ToLower())
                .Insert(0, ctx.GuildData.Configuration.CommandPrefix);

        private static string AsPrettyString(this Command c)
            => c.FullAliases.Count > 1 ? $"({c.FullAliases.Join('|')})" : c.Name;

        internal static Task<List<Type>> AddTypeParsersAsync(this CommandService service)
        {
            var assembly = typeof(VolteBot).Assembly;
            var meth = typeof(CommandService).GetMethod("AddTypeParser");
            var parsers = assembly.ExportedTypes.Where(x => x.HasAttribute<VolteTypeParserAttribute>()).ToList();

            var loadedTypes = new List<Type>();
            parsers.ForEach(parserType =>
            {
                var attr = parserType.GetCustomAttribute<VolteTypeParserAttribute>();
                var parser = parserType.GetConstructor(Type.EmptyTypes)?.Invoke(Array.Empty<object>());
                var method = meth?.MakeGenericMethod(
                    parserType.BaseType?.GenericTypeArguments[0]
                    ?? throw new FormatException("CommandService#AddTypeParser() values invalid."));
                method?.Invoke(service, new[] {parser, attr?.OverridePrimitive});
                loadedTypes.Add(parserType);
            });

            return Task.FromResult(loadedTypes);
        }

        public static Command GetCommand(this CommandService service, string name)
            => service.GetAllCommands().FirstOrDefault(x => x.FullAliases.ContainsIgnoreCase(name));

        public static int GetTotalTypeParsers(this CommandService _)
        {
            var customParsers = typeof(VolteBot).Assembly.GetTypes()
                .Count(x => x.HasAttribute<VolteTypeParserAttribute>());
            //add the number of primitive TypeParsers (that come with Qmmands), which is 13, minus bool since we override that one, therefore 12.
            return customParsers + (12); 
        }
    }
}