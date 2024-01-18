using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Utilizr.Globalisation.Events;
using Utilizr.Globalisation.Localisation;
using Utilizr.Logging;

namespace Utilizr.Globalisation
{
    /// <summary>
    /// Provides Gnu GetText implementation that works better than gettext-cs-util
    /// </summary>
    public static class L
    {
        public static Event LocaleChanged = new();

        private static readonly Dictionary<string, ResourceContext> _lookupDictionary = new();
        private static readonly Dictionary<string, string> _moFileLookup = new();

        private static bool _indexedMoFiles = false;
        public const string LogCat = "Utilizr.Globalisation";

        public static string CurrentLanguage { get; private set; }
#if DEBUG
        public static SupportedLanguage DebugLanguage { get; }
#endif

        static L()
        {
            CurrentLanguage = "en-GB"; // placeholder
#if DEBUG
            DebugLanguage = new SupportedLanguage(name: "Blank", nativeName: "*****", ietfLanguageTag: "blank");
#endif
        }

        private static ReadOnlyCollection<SupportedLanguage>? _supportedLanguages;
        /// <summary>
        /// The list of supported languages, in their native name.
        /// E.g. English, Español, Français, Deutsche
        /// </summary>
        public static ReadOnlyCollection<SupportedLanguage> SupportedLanguages
        {
            get
            {
                var supported = new List<SupportedLanguage>();
                try
                {
                    if (_supportedLanguages != null)
                        return _supportedLanguages;

                    if (!_indexedMoFiles)
                    {
                        IndexMoFiles();
                    }

                    var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

                    foreach (var culture in allCultures)
                    {
                        if (!_moFileLookup.ContainsKey(culture.IetfLanguageTag))
                            continue;

                        supported.Add(
                            new SupportedLanguage(
                                name: culture.TextInfo.ToTitleCase(culture.DisplayName),
                                nativeName: culture.TextInfo.ToTitleCase(culture.NativeName),
                                ietfLanguageTag: culture.IetfLanguageTag
                            )
                        );
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(LogCat, ex, "Failed to get list, defaulting to English only.");
                }

                // We always have English without en-GB.po
                supported.Add(
                    new SupportedLanguage(
                        name: "English",
                        nativeName: "English",
                        ietfLanguageTag: "en"
                    )
                );

#if DEBUG
                //add a dummy 'blank' language that always returns a blanked out string - helpful for finding missing translations/errors
                supported.Add(DebugLanguage);
#endif

                _supportedLanguages = supported.AsReadOnly();
                return _supportedLanguages;
            }
        }

        public static void SetLanguage(string? ietfLanguageTag)
        {
            var ietfLangTag = ietfLanguageTag?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ietfLangTag))
            {
                ietfLangTag = Thread.CurrentThread.CurrentCulture.IetfLanguageTag;
            }

            if (!_indexedMoFiles)
            {
                IndexMoFiles();
            }

            PreloadLanguage(ietfLangTag);

            bool changed = false;
            if (CurrentLanguage != ietfLangTag)
            {
                changed = true;
            }
            CurrentLanguage = ietfLangTag;
            if (changed && LocaleChanged != null)
            {
                LocaleChanged.RaiseEvent();
            }
        }

        private static void PreloadLanguage(string ietfLanguageTag)
        {
            ietfLanguageTag = ietfLanguageTag.ToLower();
            if (!_lookupDictionary.ContainsKey(ietfLanguageTag))
            {
                //load the mo file for the specified language code
                try
                {
                    if (_moFileLookup.TryGetValue(ietfLanguageTag, out string? langRegion))
                    {
                        _lookupDictionary[ietfLanguageTag] = ResourceContext.FromFile(langRegion, ietfLanguageTag);
                    }
                    else if (_moFileLookup.TryGetValue(ietfLanguageTag[..2], out string? langOnly))
                    {
                        _lookupDictionary[ietfLanguageTag] = ResourceContext.FromFile(langOnly, ietfLanguageTag);
                    }
                }
                catch (Exception ex)
                {
                    Log.Exception(LogCat, ex);
                }

#if DEBUG
                //add a dummy 'blank' language that always returns a blanked out string - helpful for finding missing translations/errors
                if (ietfLanguageTag == "blank")
                {
                    _lookupDictionary.Add("blank", new DummyResourceContext("blank", (s) =>
                    {
                        var chars = new char[s.Length];
                        for (int i = 0; i < chars.Length; i++)
                        {
                            chars[i] = char.IsWhiteSpace(s[i])
                                ? s[i]
                                : '*';
                        }
                        return new string(chars);
                    }, (s, p, n) =>
                    {
                        var chars = new char[s.Length];
                        int insideFormatPlaceholder = 0;
                        for (int i = 0; i < chars.Length; i++)
                        {
                            // Don't replace character if whitespace to avoid one long string of *******
                            // Don't replace string format placeholders, such as {0:N0}

                            chars[i] = s[i];
                            if (char.IsWhiteSpace(s[i]))
                                continue;

                            if (s[i] == '{')
                            {
                                insideFormatPlaceholder++;
                                continue;
                            }

                            if (s[i] == '}')
                            {
                                insideFormatPlaceholder--;
                                continue;
                            }

                            if (insideFormatPlaceholder > 0)
                                continue;

                            chars[i] = '*';

                        }
                        return new string(chars);
                    }));
                }
#endif
            }
        }

        private static void IndexMoFiles()
        {
            var moFileBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            IndexMoFiles(moFileBase);
            moFileBase = Path.Combine(moFileBase, "locale");
            IndexMoFiles(moFileBase);
            _indexedMoFiles = true;
        }

        private static void IndexMoFiles(string dir)
        {
            if (!Directory.Exists(dir))
                return;

            try
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    if (Path.GetExtension(file).ToLower() == ".mo")
                    {
                        string locale = Path.GetFileNameWithoutExtension(file);
                        string lang = locale[..2];
                        _moFileLookup[locale.ToLower()] = file;
                        _moFileLookup[lang] = file;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(LogCat, ex);
            }
        }

        /// <summary>
        /// Indexes the mo file with the specified language. Use with android as assets do not have paths.
        /// </summary>
        /// <param name="ietfLanguageTag">Ietf language tag.</param>
        /// <param name="stream">Asset Stream of mo file.</param>
        public static void IndexMoFile(string ietfLanguageTag, Stream stream)
        {
            if (_lookupDictionary.ContainsKey(ietfLanguageTag))
                return;
            try
            {
                _lookupDictionary.Add(ietfLanguageTag, ResourceContext.FromStream(stream, ietfLanguageTag));
                _moFileLookup[ietfLanguageTag] = "";
            }
            catch (Exception ex)
            {
                Log.Exception(LogCat, ex);
            }
        }

        /// <summary>
        /// Translate the source text to the current language.
        /// </summary>
        /// <param name="t">English source text which can also be a composite format string.</param>
        /// <param name="args">Any string object to format.</param>
        /// <returns>Returns the source in the current language, or English if the lookup fails.</returns>
        public static string _(string t, params object[] args)
        {
            var result = string.Empty;
            try
            {
                if (CurrentLanguage != null && _lookupDictionary.ContainsKey(CurrentLanguage))
                {
                    if (_lookupDictionary[CurrentLanguage] != null)
                    {
                        result = _lookupDictionary[CurrentLanguage].LookupString(t);
                        if (args.Length > 0)
                        {
                            result = string.Format(result, args);
                        }
                        return result;
                    }
                }
                result = t;
                if (args.Length > 0)
                {
                    result = string.Format(result, args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        /// <summary>
        /// Translate the source text to the current language.
        /// </summary>
        /// <param name="t">English source text which can also be a composite format string.</param>
        /// <param name="args">Any string object to format.</param>
        /// <returns>Returns the source in the current language, or English if the lookup fails.</returns>
        public static string _P(string t, string tPlural, long n, params object[] args)
        {
            var result = string.Empty;
            if (CurrentLanguage != null && _lookupDictionary.ContainsKey(CurrentLanguage))
            {
                if (_lookupDictionary[CurrentLanguage] != null)
                {
                    result = _lookupDictionary[CurrentLanguage].LookupPluralString(t, tPlural, n);
                    if (args.Length > 0)
                    {
                        result = string.Format(result, args);
                    }
                    return result;
                }
            }
            //couldn't find resource context so return default values
            result = n == 1 ? t : tPlural;
            if (args.Length > 0)
            {
                result = string.Format(result, args);
            }
            return result;
        }

        //[Obsolete("Use _ip() which returns an ITranslatable object. Can then use the Translation property.", false)]
        //public static string _p(MP mp, long n, params object[] args)
        //{
        //    return _p(mp.T, mp.TPlural, n, args);
        //}

        /// <summary>
        /// Marks a string for translation and returns the original (non-translated string).
        /// Useful for storing translatable string in variables.
        /// </summary>
        /// <param name="tT"></param>
        /// <returns></returns>
        public static string _M(string t)
        {
            return t;
        }

        ///// <summary>
        ///// Depreciated. Use <see cref="_m(string, string, Func{long}, Func{LArgsInfo})"/> instead.
        ///// <param name="T"></param>
        ///// <param name="TPlural"></param>
        ///// <returns></returns>
        //[Obsolete("Use _ip which returns ITranslatable. Calling Translation on MP object will throw when created here!", false)]
        //public static MP _m(string T, string TPlural)
        //{
        //    return new MP(T, TPlural, null, null);
        //}

        /// <summary>
        /// Returns an <see cref="ITranslatable"/> object which generates the localised string
        /// at the time of invocation, using a lambda to get the latest string format arguments
        /// </summary>
        /// <param name="t">Singular English string</param>
        /// <param name="args">Optional to return a <see cref="LArgsInfo"/> object.</param>
        /// <returns></returns>
        public static ITranslatable _I(string t, Func<LArgsInfo>? args = null)
        {
            return new MS(t, args);
        }

        /// <summary>
        /// Returns an <see cref="ITranslatable"/> object which generates the localised string
        /// at the time of invocation, using a lambda to get the latest string format arguments
        /// </summary>
        /// <param name="t">Singular English text</param>
        /// <param name="tPlural">Plural English text</param>
        /// <param name="n">Not null. Returns the value to determine whether to show the singular or plural version.</param>
        /// <param name="args">Optional to return a <see cref="LArgsInfo"/> object.</param>
        /// <returns></returns>
        public static ITranslatable _IP(string t, string tPlural, Func<long> n, Func<LArgsInfo>? args = null)
        {
            return new MP(t, tPlural, n, args);
        }

        /// <summary>
        /// Static helper to create LArgsInfo without writing 'new LArgsInfo();'
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static LArgsInfo Args(params object[] fmtArgs)
        {
            return new LArgsInfo(fmtArgs);
        }

        public static void AddCustomTranslation(string ietfLanguageTag, string id, string translation)
        {
            if (string.IsNullOrEmpty(ietfLanguageTag))
                return;

            if (string.IsNullOrEmpty(id))
                return;

            if (string.IsNullOrEmpty(translation))
                return;

            ietfLanguageTag = ietfLanguageTag.ToLower();
            if (!_indexedMoFiles)
                IndexMoFiles();

            PreloadLanguage(ietfLanguageTag);
            if (!_lookupDictionary.TryGetValue(ietfLanguageTag, out ResourceContext? context))
                _lookupDictionary.TryGetValue(ietfLanguageTag[..2], out context);

            if (context == null)
                return;

            context.AddCustomTranslation(id, translation);
        }
    }

    public interface ITranslatable
    {
        // Note: Changing these property names will break bindings
        string English { get; }
        string Translation { get; }
    }

    /// <summary>
    /// Allows translatable singular string to be stored in variables
    /// </summary>
    [DebuggerDisplay("English = {English}\n, Translation = {Translation}")]
    public class MS : ITranslatable
    {
        public string T { get; set; }
        public string Translation
        {
            get
            {
                if (string.IsNullOrEmpty(T))
                    return T;

                var lArgs = _formatArgs?.Invoke();
                return lArgs == null
                    ? L._(T)
                    : L._(T, lArgs.FormatArgs);
            }
        }
        public string English
        {
            get
            {
                var lArgs = _formatArgs?.Invoke();
                return lArgs == null
                    ? T
                    : string.Format(T, lArgs.FormatArgs);
            }
        }

        readonly Func<LArgsInfo>? _formatArgs;

        internal MS(string t, Func<LArgsInfo>? formatArgs)
        {
            T = t;
            _formatArgs = formatArgs;
        }
    }

    public static class ITranslatableExtensions
    {
        /// <summary>
        /// Wraps a normal non-translatable string as an ITranslatable
        /// </summary>
        /// <returns>The string as a translatable.</returns>
        /// <param name="text">Text.</param>
        public static ITranslatable WrapAsTranslatable(this string text)
        {
            return new MS(text, null);
        }

        /// <summary>
        /// Merge multiple ITranslatable instance into one. Useful when dealing with multiple plurals within one piece of text.
        /// </summary>
        /// <param name="iTranslatables">Instances to merge.</param>
        /// <param name="separator">Specific optional separator. Default is no separator.</param>
        /// <returns></returns>
        public static ITranslatable Merge(this IEnumerable<ITranslatable> iTranslatables, string? separator = null)
        {
            if (iTranslatables == null)
                throw new ArgumentException($"{nameof(iTranslatables)} cannot be null");

            var formatStringBuilder = new StringBuilder();
            for (int i = 0; i < iTranslatables.Count(); i++)
            {
                if (separator == null)
                    formatStringBuilder.Append($"{{{i}}}");
                else
                    formatStringBuilder.Append($"{{{i}}}{separator}");
            }

            return L._I(formatStringBuilder.ToString().Trim(), () => L.Args(iTranslatables.Select(p => p.Translation).ToArray()));
        }
    }

    /// <summary>
    /// Allows translatable plural string to be stored in variables
    /// </summary>
    [DebuggerDisplay("English = {English}\n, Translation = {Translation}")]
    public class MP : ITranslatable
    {
        public string T { get; set; }
        public string TPlural { get; set; }
        public string Translation
        {
            get
            {
                long count = _counter.Invoke();

                if (string.IsNullOrEmpty(T) && count == 1)
                    return T;

                if (string.IsNullOrEmpty(TPlural) && count != 1)
                    return TPlural;

                var lArgs = _formatArgs?.Invoke();
                return lArgs == null
                    ? L._P(T, TPlural, count)
                    : L._P(T, TPlural, count, lArgs.FormatArgs);
            }
        }
        public string English
        {
            get
            {
                long count = _counter.Invoke();
                var lArgs = _formatArgs?.Invoke();

                return lArgs == null
                    ? count == 1
                        ? T
                        : TPlural
                    : count == 1
                        ? string.Format(T, lArgs.FormatArgs)
                        : string.Format(TPlural, lArgs.FormatArgs);
            }
        }

        readonly Func<long> _counter;
        readonly Func<LArgsInfo>? _formatArgs;

        internal MP(string t, string tplural, Func<long> counter, Func<LArgsInfo>? formatArgs)
        {
            T = t;
            TPlural = tplural;
            _counter = counter;
            _formatArgs = formatArgs;
        }
    }

    /// <summary>
    /// Wrapper just to make it a little nicer to return an array of object for MS and MP
    /// Declaring new object[] { obj1, obj2, ...} bit more cumbersome than new LArgsInfo(obj1, obj2, ...)
    /// </summary>
    public class LArgsInfo
    {
        public object[] FormatArgs { get; set; }

        public LArgsInfo(params object[] formatArgs)
        {
            FormatArgs = formatArgs;
        }
    }

    [DebuggerDisplay("Name={Name}, NativeName={NativeName}, IetfLanguageTag={IetfLanguageTag})")]
    public class SupportedLanguage
    {
        public string Name { get; set; }
        public string NativeName { get; set; }
        public string IetfLanguageTag { get; set; }

        public SupportedLanguage(string name, string nativeName, string ietfLanguageTag)
        {
            Name = name;
            NativeName = nativeName;
            IetfLanguageTag = ietfLanguageTag;
        }
    }
}
