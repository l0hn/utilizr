using CommandLine;
using System;
using System.Collections.Generic;
using Utilizr.Console.Commands;

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