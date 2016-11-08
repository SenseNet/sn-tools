using System.Reflection;

namespace SenseNet.Tools.CommandLineArguments
{
    internal abstract class Argument
    {
        public PropertyInfo Property { get; private set; }
        public bool Required { get; private set; }
        public bool HasName { get; private set; }
        public bool HasValue { get; private set; }
        public string HelpText { get; private set; }

        protected Argument(PropertyInfo property, bool required, bool hasName, bool hasValue, string helpText)
        {
            this.Property = property;
            this.Required = required;
            this.HasName = hasName;
            this.HasValue = hasValue;
            this.HelpText = helpText;
        }

        public abstract string GetUsageHead();
    }

    internal class NamedArgument : Argument
    {
        public string Name { get; private set; }
        public string[] Aliases { get; private set; } //UNDONE: check all aliases vs name violation

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
        public int Order { get; private set; }
        public string NameInHelp { get; private set; }

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
