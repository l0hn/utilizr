using CommandLine;
using Utilizr.Globalisation.Helpers;

namespace Utilizr.Console.Commands
{
    internal class GenerateSourceList
    {
        internal static void Run(GenerateSourceListOptions options)
        {
            SourceList.GenerateSourceList(options.SourceDirectory!, options.Output!);
        }
    }

    [Verb("source-list", HelpText = "Generates source list ready to send to msginit")]
    class GenerateSourceListOptions
    {
        [Option(SetName = "src", Required = true, HelpText = "The source root directory.")]
        public string? SourceDirectory { get; set; }

        [Option(SetName = "out", Required = true, HelpText = "The filename to write the file list.")]
        public string? Output { get; set; }
    }
}
