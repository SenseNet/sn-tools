using System;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools.CommandLineArguments
{
    /// <summary>
    /// Represents an argument parsing error.
    /// </summary>
    [Serializable]
    public class ParsingException : Exception
    {
        /// <summary>
        /// Type of the error that occured during parsing arguments. See the ResultState enumeration for possible values.
        /// </summary>
        public ResultState ErrorCode { get; private set; }
        internal Argument Argument { get; private set; }
        internal string CurrentInput { get; private set; }

        /// <summary>
        /// Original parser instance that can provide a formatted message and a generated help text.
        /// </summary>
        public ArgumentParser Result { get; private set; }

        /// <summary>
        /// Error message to write to the console.
        /// </summary>
        public string FormattedMessage => GetFormattedMessage();

        private string GetFormattedMessage()
        {
            if (ErrorCode == ResultState.MissingArgument)
                return Message;

            return CurrentInput == null
                ? $"Missing value after the last argument: '{GetArgument()}': {Message} (code: '{ErrorCode}')"
                : $"An error occured during parsing '{CurrentInput}' (arg: '{GetArgument()}'): {Message} (code: '{ErrorCode}')";
        }
        private string GetArgument()
        {
            if (this.Argument == null)
                return "??";
            return this.Argument is NamedArgument named ? named.Name : "<noname>";
        }

        internal ParsingException(ResultState errorCode, Argument arg, string currentInput, ArgumentParser parser)
        {
            Initialize(errorCode, arg, currentInput, parser);
        }
        internal ParsingException(ResultState errorCode, Argument arg, string currentInput, ArgumentParser parser, string message)
            : base(message)
        {
            Initialize(errorCode, arg, currentInput, parser);
        }
        internal ParsingException(ResultState errorCode, Argument arg, string currentInput, ArgumentParser parser, string message, Exception inner)
            : base(message, inner)
        {
            Initialize(errorCode, arg, currentInput, parser);
        }

        /// <summary>
        /// Initializes a new instance of the ParsingException class with serialized data.
        /// </summary>
        protected ParsingException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }

        private void Initialize(ResultState errorCode, Argument arg, string currentInput, ArgumentParser parser)
        {
            this.ErrorCode = errorCode;
            this.Argument = arg;
            this.CurrentInput = currentInput;
            this.Result = parser;
        }
    }
}
