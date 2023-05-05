using System;
using System.Linq;

namespace Utilizr.Util
{
    /// <summary>
    /// Common command line argument functionality.
    /// </summary>
    public abstract class SwitchesBase
    {
        /// <summary>
        /// All command line arguments returned as lower invariant.
        /// </summary>
        protected static string[] GetArgs()
        {
            return Environment.GetCommandLineArgs()
                .Select(p => p.ToLowerInvariant())
                .ToArray();
        }

        /// <summary>
        /// The value of arguments supplied in the format arg=value
        /// </summary>
        /// <param name="arg">'arg' from the format 'arg=value'</param>
        /// <param name="args">All command line arguments to search.</param>
        /// <returns></returns>
        protected string? ExtractValueFromArg(string arg, string[] args)
        {
            var foundArg = args?.FirstOrDefault(p => p?.StartsWith(arg, StringComparison.OrdinalIgnoreCase) == true);
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

        protected string[] SplitCommanSeparatedArgValue(string argumentValue)
        {
            if (string.IsNullOrEmpty(argumentValue))
                return Array.Empty<string>();

            return argumentValue.Split(',');
        }
    }
}
