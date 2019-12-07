using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools.CommandLineArguments
{
    /// <summary>
    /// Defines the category of the parsing result.
    /// </summary>
    public enum ResultState
    {
        /// <summary>Successfully parsed.</summary>
        Succesful = 0,
        /// <summary>Unknown error occured.</summary>
        UnknownError,
        /// <summary>Unknown argument in the argument array.</summary>
        UnknownArgument,
        /// <summary>A required argument is missing.</summary>
        MissingArgument,
        /// <summary>Only a value is provided.</summary>
        MissingArgumentName,
        /// <summary>Required value is missing.</summary>
        MissingValue,
        /// <summary>Wrong value type (e.g. trying to convert a string to an integer).</summary>
        InvalidType
    }

    /// <summary>
    /// Provides methods to analyze the given configuration object by its annotated properties,
    /// fills it with values in an argument array and generates information about the analyzed object.
    /// </summary>
    public class ArgumentParser
    {
        /// <summary>
        /// Analyzes the target object by its annotations and fills its properties 
        /// with the values in the given argument array.
        /// </summary>
        /// <returns>An instance of the parser to provide more information.</returns>
        public static ArgumentParser Parse(string[] args, object target)
        {
            var parser = new ArgumentParser
            {
                _target = target,
                _context = ParserContext.Create(AnalyzeTarget(target))
            };
            parser.Parse(args, target, parser._context);
            return parser;
        }

        private static readonly string[] HelpArguments = { "?", "-?", "-h", "-H", "/?", "/h", "/H", "-help", "--help" };

        private object _target;
        private ParserContext _context;

        /// <summary>
        /// Gets a value that indicates wether the argument list contains a help request.
        /// If that is the case, the configuration object will not be filled and the help
        /// text should be provided to the user, using the GetHelpText method of the parser.
        /// </summary>
        public bool IsHelp { get; private set; }

        private ArgumentParser() { }

        private void Parse(string[] args, object target, ParserContext context)
        {
            var requiredArguments = context.NamedArguments.Where(a => a.Required).Select(a => a.Name).ToArray();
            var existingArguments = new List<string>();

            if (IsHelpRequest(args))
            {
                this.IsHelp = true;
                return;
            }

            var mustReadNext = false;
            string lastName = null;
            Argument lastArgument = null;
            var unnamedIndex = 0;
            foreach (var arg in args)
            {
                string name;
                string value;
                Argument argument;

                if (mustReadNext)
                {
                    name = lastName;
                    argument = lastArgument;
                    value = arg;
                    mustReadNext = false;
                }
                else
                {
                    ParseNameAndValue(arg, out name, out value);
                    argument = context.GetArgumentByName(name);
                    if (argument == null)
                    {
                        argument = context.GetArgumentByShortName(name);
                        if (argument == null)
                        {
                            if (name == null)
                            {
                                argument = context.GetUnnamedArgument(unnamedIndex);
                                if (argument == null)
                                    throw new ParsingException(ResultState.MissingArgumentName, null, arg, this);

                                unnamedIndex++;
                            }
                            else
                            {
                                throw new ParsingException(ResultState.UnknownArgument, null, arg, this);
                            }
                        }
                    }
                }

                if (value == null && argument.HasValue)
                {
                    mustReadNext = true;
                    lastName = name;
                    lastArgument = argument;
                }
                else
                {
                    if (argument is NamedArgument namedArg)
                        existingArguments.Add(namedArg.Name);
                    SetProperty(target, argument, value);
                }
            }
            if (mustReadNext)
                throw new ParsingException(ResultState.MissingValue, lastArgument, null, this);

            var missingArgumentNames = string.Join(", ", requiredArguments.Except(existingArguments));
            if (missingArgumentNames.Length > 0)
                throw new ParsingException(ResultState.MissingArgument, null, null, this, "Missing argument: " + missingArgumentNames);
        }
        private void SetProperty(object target, Argument argument, string value)
        {
            var targetType = argument.Property.PropertyType;
            object targetValue;

            if (targetType == typeof(bool))
                targetValue = true;
            else
            {
                try
                {
                    targetValue = Convert.ChangeType(value, targetType);
                }
                catch (FormatException e)
                {
                    throw new ParsingException(ResultState.InvalidType, argument, value, this, e.Message, e);
                }
            }

            argument.Property.SetValue(target, targetValue);
        }

        private static void ParseNameAndValue(string src, out string name, out string value)
        {
            name = value = null;

            if (src.StartsWith("--"))
            {
                name = src.Substring(2);
                src = src.Substring(2);
            }
            else if (src.StartsWith("-") || src.StartsWith("/"))
            {
                name = src.Substring(1);
                src = src.Substring(1);
            }

            if (name == null)
            {
                value = src;
                return;
            }

            var segments = name.Split(new[] { '=', ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 1)
                return;

            name = segments[0];
            value = src.Substring(name.Length + 1);
        }
        private static IEnumerable<Argument> AnalyzeTarget(object target)
        {
            var props = target.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public);

            var result = props
                .Select(AnalyzeProperty)
                .Where(p => p != null)
                .ToArray();

            return result;
        }
        private static Argument AnalyzeProperty(PropertyInfo prop)
        {
            var optAttr = (CommandLineArgumentAttribute)prop.GetCustomAttribute(typeof(CommandLineArgumentAttribute));
            if (optAttr != null)
                return new NamedArgument(optAttr, prop);

            var noNameAttr = (NoNameOptionAttribute)prop.GetCustomAttribute(typeof(NoNameOptionAttribute));
            return noNameAttr != null ? new NoNameArgument(noNameAttr, prop) : null;
        }

        private static bool IsHelpRequest(string[] args)
        {
            return args.Length != 0 && HelpArguments.Contains(args[0], StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns with the generated calling information. For example:
        /// ToolName [-OptionalParameter:String] &lt;-Limit:String&gt;
        /// </summary>
        public string GetUsage()
        {
            var usage = _target.GetType().Assembly.GetName().Name;
            var noname = string.Join(" ", this._context.NoNameArguments.OrderBy(a => a.Order).Select(a => a.GetUsageHead()));
            var named = string.Join(" ", this._context.NamedArguments.OrderBy(a => a.Name).Select(a => a.GetUsageHead()));
            if (!string.IsNullOrEmpty(noname))
                usage += " " + noname;
            if (!string.IsNullOrEmpty(named))
                usage += " " + named;
            return usage + " [?]";
        }
        /// <summary>
        /// Returns the name of the assembly and its current version e.g.: "SnBenchmark 1.0.0.0"
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public string GetAppNameAndVersion()
        {
            if (_target == null)
                throw new InvalidOperationException("Help can be called after Parse_WillBeDeleted is executed");
            var assembly = _target.GetType().Assembly;
            var asmName = assembly.GetName();
            var name = asmName.Name;
            var version = asmName.Version;
            return $"{name} {version}";
        }
        /// <summary>
        /// Returns detailed information about how to use the tool.
        /// Contains the app name and version, usage information and list of the
        /// available command line arguments.
        /// </summary>
        public string GetHelpText()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetAppNameAndVersion());
            sb.AppendLine();

            sb.AppendLine("Usage:");
            sb.AppendLine(GetUsage());
            sb.AppendLine();

            foreach (var arg in this._context.NoNameArguments.OrderBy(a => a.Order))
            {
                sb.Append(arg.GetUsageHead()).AppendFormat(" ({0})", arg.Required ? "required" : "optional").AppendLine();
                sb.Append("    ").AppendLine(arg.HelpText);
                sb.AppendLine();
            }

            foreach (var arg in this._context.NamedArguments.OrderBy(a => a.Name))
            {
                sb.Append(arg.GetUsageHead()).AppendFormat(" ({0})", arg.Required ? "required" : "optional").AppendLine();
                if(arg.Aliases.Length >0)
                    sb.Append("    Alias: ").AppendLine(string.Join(", ", arg.Aliases));
                sb.Append("    ").AppendLine(arg.HelpText);
                sb.AppendLine();
            }

            sb.Append("[?, -?, /?, -h, -H, /h /H -help --help] (optional)").AppendLine();
            sb.AppendLine("    Display this text.");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
