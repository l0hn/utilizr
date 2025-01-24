using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Xml;

namespace Utilizr.WPF.Util
{
    public class ResourceDictionaryHelper
    {

        public static void FlattenResourceDictionary(string assemblyPath, string dictionaryPath, string outFile)
        {
            try
            {
                Assembly.LoadFile(assemblyPath);

                if (!UriParser.IsKnownScheme("pack"))
                    new Application();

                var assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);
                var source = GetResourceDictionary(assemblyName, dictionaryPath);
                var destination = new ResourceDictionary();

                FlattenResourceDictionary(source, destination);
                SaveResourceDictionary(destination, Path.GetFullPath(outFile));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);

                MessageBox.Show(ex.Message);
                Environment.Exit(-1);
            }
        }


        public static void FlattenResourceDictionary(ResourceDictionary source, ResourceDictionary destination)
        {
            foreach (DictionaryEntry kvp in source)
            {
                Console.WriteLine($"saving {kvp.Key}: {kvp.Value}");
                destination[kvp.Key] = kvp.Value;
            }

            foreach (var mergedDictionary in source.MergedDictionaries)
            {
                FlattenResourceDictionary(mergedDictionary, destination);
            }
        }

        public static ResourceDictionary GetResourceDictionary(string assemblyName, string name)
        {
            if (!UriParser.IsKnownScheme("pack"))
                new System.Windows.Application();

            var rd = new ResourceDictionary();
            rd.Source = new Uri($"pack://application:,,,/{assemblyName};component/{name}");
            return rd;
        }

        public static void SaveResourceDictionary(ResourceDictionary dictionary, string outFile)
        {
            var xmlSettings = new XmlWriterSettings()
            {
                Indent = true,
                NewLineOnAttributes = true,
            };

            var dir = Path.GetDirectoryName(outFile);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using (var stream = File.Create(outFile))
            using (var writer = XmlTextWriter.Create(stream, xmlSettings))
            {
                XamlWriter.Save(dictionary, writer);
            }
        }
    }
}
