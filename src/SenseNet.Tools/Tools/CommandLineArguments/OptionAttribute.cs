using System;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools.CommandLineArguments
{
    /// <summary>
    /// Annotates a property that can be linked to a named argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class CommandLineArgumentAttribute : Attribute
    {
        /// <summary>
        /// Comma or semicolon separated words that will be synonyms of the name.
        /// </summary>
        public string[] Aliases { get; set; }
        /// <summary>
        /// Name of the argument. Optional, default: the name of the annotated property.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Required in argument array or not.
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Text displayed in the detailed parameter information on the usage screen.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Initializes a new instance of the CommandLineArgumentAttribute class.
        /// </summary>
        /// <param name="name">Name of the argument. Optional, default: the name of the annotated property.</param>
        /// <param name="required">Required in argument array or not. Optional, default: false</param>
        /// <param name="aliases">Comma or semicolon separated words that will be synonyms of the name.</param>
        /// <param name="helpText">Text displayed in the detailed parameter information on the usage screen. Optional, default: null.</param>
        public CommandLineArgumentAttribute(string name = null, bool required = false, string aliases = null, string helpText = null)
        {
            this.Aliases = aliases?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray() ?? new string[0];

            this.Name = name;
            this.Required = required;
            this.HelpText = helpText;
        }
    }
}
