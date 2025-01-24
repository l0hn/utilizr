using CommandLine;
using Utilizr.Globalisation.Helpers;

namespace Utilizr.Console.Commands
{
    internal class GeneratePotFile
    {
        internal static void Run(GeneratePotFileOptions options)
        {
            PotGen.CreatePotFile(options.SourceDirectory!, options.Output!);
        }
    }

    [Verb("gen-pot", HelpText = "Create a POT file by recursively scanning a src directory for gettext methods.")]
    class GeneratePotFileOptions
    {
        [Option(Required = true, HelpText = "The source root directory.")]
        public string? SourceDirectory { get; set; }

        [Option(Required = true, HelpText = "The filename for the output POT file.")]
        public string? Output { get; set; }
    }
}
