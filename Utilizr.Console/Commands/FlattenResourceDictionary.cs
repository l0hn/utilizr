using CommandLine;
using System;
using System.IO;
using Utilizr.Globalisation.Helpers;
using Utilizr.WPF.Util;

namespace Utilizr.Console.Commands
{
    internal class FlattenResourceDictionary
    {
        internal static void Run(FlattenResourceDictionaryOptions options)
        {
            //var nl = Environment.NewLine;
            //System.Windows.MessageBox.Show(
            //    $"Assembly: {options.AssemblyPath}{nl}Dictionary: {options.DictionaryPath}{nl}output: {Path.GetFullPath(options.Output!)}"
            //);

            ResourceDictionaryHelper.FlattenResourceDictionary(
                options.AssemblyPath!,
                options.DictionaryPath!,
                options.Output!
            );
        }
    }

    [Verb("flatten-dictionary", HelpText = "Flatten a resource dictionary into a single dictionary (no merged dictionary references).")]
    class FlattenResourceDictionaryOptions
    {
        [Option(Required = true, HelpText = "Absolute assembly path containing the resource dictionary to be used in a pack://application URI.")]
        public string? AssemblyPath { get; set; }

        [Option(Required = true, HelpText = "Path of the resource dictionary relative to the assembled it's contained within.")]
        public string? DictionaryPath { get; set; }

        [Option(Required = true, HelpText = "Full path where to put flattened resource dictionary.")]
        public string? Output { get; set; }
    }
}
