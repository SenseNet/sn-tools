using System;

namespace SenseNet.Tools.CommandLineArguments
{
    /// <summary>
    /// Annotates a property that can be linked to an argument without a name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class NoNameOptionAttribute : Attribute
    {
        /// <summary>
        /// Determines parameter order on the usage screen.
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// Required in argument array or not.
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// Name of the argument on the usage screen.
        /// </summary>
        public string NameInHelp { get; set; }
        /// <summary>
        /// Text displayed in the detailed parameter information on the usage screen.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Initializes a new instance of the NoNameOptionAttribute class.
        /// </summary>
        /// <param name="order">Parameter order. Required.</param>
        /// <param name="required">Required in argument array or not. Optional, default: false</param>
        /// <param name="nameInHelp">Name of the argument on the usage screen. Optional, default: null.</param>
        /// <param name="helpText">Text displayed in the detailed parameter information on the usage screen. Optional, default: null.</param>
        public NoNameOptionAttribute(int order, bool required = false, string nameInHelp = null, string helpText = null)
        {
            this.Order = order;
            this.Required = required;
            this.NameInHelp = nameInHelp;
            this.HelpText = helpText;
        }
    }
}
