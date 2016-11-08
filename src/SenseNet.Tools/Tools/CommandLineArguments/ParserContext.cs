using System;
using System.Collections.Generic;
using System.Linq;

namespace SenseNet.Tools.CommandLineArguments
{
    internal class ParserContext
    {
        internal NoNameArgument[] NoNameArguments { get; private set; }
        internal NamedArgument[] NamedArguments { get; private set; }

        internal static ParserContext Create(IEnumerable<Argument> arguments)
        {
            var context = new ParserContext();

            var args = arguments as Argument[] ?? arguments.ToArray();
            context.NamedArguments = args
                .Select(a => a as NamedArgument)
                .Where(a => a != null)
                .OrderBy(a => a.Name)
                .ToArray();

            context.NoNameArguments = args
                .Select(a => a as NoNameArgument)
                .Where(a => a != null)
                .OrderBy(a => a.Order)
                .ToArray();

            return context;
        }

        internal Argument GetArgumentByName(string name)
        {
            return NamedArguments.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        internal Argument GetArgumentByShortName(string name)
        {
            return NamedArguments.FirstOrDefault(x => x.Aliases.Contains(name, StringComparer.OrdinalIgnoreCase));
        }
        internal Argument GetUnnamedArgument(int index)
        {
            if (index < 0 || index >= NoNameArguments.Length)
                return null;
            return NoNameArguments[index];
        }
    }
}
