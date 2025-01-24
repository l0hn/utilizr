using CommandLine;
using System.IO;
using System;
using Utilizr.Globalisation.Localisation;

namespace Utilizr.Console.Commands
{
    internal class ValidateLocales
    {
        internal static void Run(ValidateLocaleOptions options)
        {
            try
            {
                if (!Directory.Exists(options.SourceDirectory))
                    throw new ArgumentException($"{nameof(options.SourceDirectory)} - Folder '{options.SourceDirectory}' does not exist on disk");

                foreach (var moFile in Directory.GetFiles(options.SourceDirectory, "*.mo"))
                {
                    var ietfTag = Path.GetFileNameWithoutExtension(moFile);
                    var ctx = ValidationResourceContext.FromFile(moFile, ietfTag);

                    // TODO: This could be significantly improved, just something quick and dirty to list potentially dodgy translations, e.g. "{ 0 : NO }" as a placeholder.
                    // Might be able to use the rosyln compiler sdk to parse source files and figure out any string format placeholders count/types
                    // Might also be able to figure out singular / plural for better testing, too.
                    // None of that is trivial, however.
                    // Will always show string interpolation translations as errors, although not a good idea to use them inside translations anyway e.g. variable name changes

                    foreach (var entry in ctx.Entries())
                    {
                        try
                        {
                            string.Format(entry.Value, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
                        }
                        catch (Exception ex)
                        {
                            var nl = Environment.NewLine;
                            System.Console.WriteLine($"[{ietfTag}] - {ex.Message}{nl}English: {entry.Key}{nl}Translation: {entry.Value}{nl}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    System.Console.WriteLine("Catastrophic error:");
                    System.Console.WriteLine(ex.Message);
                    System.Console.WriteLine();
                    ex = ex.InnerException;
                }
            }
        }
    }

    [Verb("validate-locales", HelpText = "Iterates throughout all translations to ensure no exceptions will be thrown.")]
    class ValidateLocaleOptions
    {
        [Option(Required = true, HelpText = "The locale folder which contains the *.mo files.")]
        public string? SourceDirectory { get; set; }
    }
}
