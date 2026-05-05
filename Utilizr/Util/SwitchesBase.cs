using System;
using System.Linq;

namespace Utilizr.Util
{
    /// <summary>
    /// Common command line argument functionality.
    /// </summary>
    public abstract class SwitchesBase
    {
        public string[] CommandLineArguments { get; }

        protected SwitchesBase()
        {
            CommandLineArguments = Environment.GetCommandLineArgs();
        }

        /// <summary>
        /// Whether the supplied argument has an exact match in the arguments array, depending on the comparison parameter.
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public bool HasArgument(string argument, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return CommandLineArguments.Any(p => p.Equals(argument, comparison));
        }

        /// <summary>
        /// The value of arguments supplied in the format arg=value
        /// </summary>
        /// <param name="arg">'arg' from the format 'arg=value'</param>
        /// <param name="args">All command line arguments to search.</param>
        /// <returns></returns>
        protected string? ExtractValueFromArg(string arg, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var foundArg = CommandLineArguments.FirstOrDefault(p => p?.StartsWith(arg, comparison) == true);
            var argWithEquals = $"{arg}=";
            if (foundArg == null || !foundArg.StartsWith(argWithEquals) || foundArg.Length <= argWithEquals.Length)
            {
                return null;
            }

            var value = foundArg.Substring(argWithEquals.Length);
            // empty string if no value specified, ensure always null
            var result = string.IsNullOrEmpty(value) ? null : value;

            if (result == null)
            {
                // Will be invoked before logging setup. Just write to stdout.
                Console.WriteLine($"No value was specified for '{arg}', ignoring supplied argument.");
            }

            return result;
        }

        protected string[] SplitCommaSeparatedArgValue(string? argumentValue)
        {
            if (string.IsNullOrEmpty(argumentValue))
                return Array.Empty<string>();

            return argumentValue.Split(',');
        }
    }
}