using CommandLine;
using System;
using System.Collections.Generic;
using Utilizr.Console.Commands;

#if WINDOWS
    using Utilizr.Console.Commands.Win;
#endif

namespace Utilizr.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new string[] { "--help" };
            }

            try
            {
#if WINDOWS
                CommandLine.Parser.Default.ParseArguments<
                    GeneratePotFileOptions,
                    GenerateSourceListOptions,
                    ValidateLocaleOptions,
                    FlattenResourceDictionaryOptions>(args)
                        .WithParsed<GeneratePotFileOptions>(GeneratePotFile.Run)
                        .WithParsed<GenerateSourceListOptions>(GenerateSourceList.Run)
                        .WithParsed<ValidateLocaleOptions>(ValidateLocales.Run)
                        .WithParsed<FlattenResourceDictionaryOptions>(FlattenResourceDictionary.Run)
                        .WithNotParsed(HandleParseError);

                return;
#endif
                CommandLine.Parser.Default.ParseArguments<
                    GeneratePotFileOptions,
                    GenerateSourceListOptions,
                    ValidateLocaleOptions>(args)
                        .WithParsed<GeneratePotFileOptions>(GeneratePotFile.Run)
                        .WithParsed<GenerateSourceListOptions>(GenerateSourceList.Run)
                        .WithParsed<ValidateLocaleOptions>(ValidateLocales.Run)
                        .WithNotParsed(HandleParseError);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex);
            }
        }

        static void HandleParseError(IEnumerable<Error> errors)
        {
            System.Console.WriteLine("Failed to parse CLI input:");
            foreach (Error error in errors)
            {
                System.Console.WriteLine(error);
            }
        }
    }
}
