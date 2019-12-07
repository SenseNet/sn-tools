using System.Reflection;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools.CommandLineArguments
{
    internal abstract class Argument
    {
        public PropertyInfo Property { get; }
        public bool Required { get; }
        public bool HasName { get; }
        public bool HasValue { get; }
        public string HelpText { get; }

        protected Argument(PropertyInfo property, bool required, bool hasName, bool hasValue, string helpText)
        {
            this.Property = property;
            this.Required = required;
            this.HasName = hasName;
            this.HasValue = hasValue;
            this.HelpText = helpText;
        }

        // ReSharper disable once UnusedMemberInSuper.Global
        public abstract string GetUsageHead();
    }

    internal class NamedArgument : Argument
    {
        public string Name { get; }
        public string[] Aliases { get; } //UNDONE: check all aliases vs name violation

        public NamedArgument(CommandLineArgumentAttribute attribute, PropertyInfo property)
            : base(property, attribute.Required, true, property.PropertyType != typeof(bool), attribute.HelpText)
        {
            this.Name = attribute.Name ?? property.Name;
            this.Aliases = attribute.Aliases;
        }

        public override string GetUsageHead()
        {
            var type = this.Property.PropertyType.Name;
            var name = this.Required ? "<-" + Name + ":" + type + ">" : "[-" + Name + ":" + type + "]";
            return name;
        }
    }

    internal class NoNameArgument : Argument
    {
        public int Order { get; }
        public string NameInHelp { get; }

        public NoNameArgument(NoNameOptionAttribute attribute, PropertyInfo property)
            : base(property, attribute.Required, false, true, attribute.HelpText)
        {
            this.Order = attribute.Order;
            this.NameInHelp = attribute.NameInHelp;
        }

        public override string GetUsageHead()
        {

            var name = NameInHelp ?? "Arg" + (Order + 1);
            if (this.Required)
                return "<" + name + ">";
            return "[" + name + "]";
        }
    }
}
