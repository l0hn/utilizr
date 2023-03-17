using System.Collections.Generic;
using System.IO;
using Utilizr.Globalisation.Parsers;
using Utilizr.Logging;

namespace Utilizr.Globalisation.Localisation
{
    public class ResourceContext
    {
        public string IetfLanguageTag { get; set; }
        public MoHeader Header { get; set; }
        protected Dictionary<string, string> _translationLookup;
        protected Dictionary<string, string> _userDefinedTranslations;

        internal ResourceContext(string ietfLanguageTag, MoHeader moHeader, Dictionary<string, string>? translationDictionary)
        {
            IetfLanguageTag = ietfLanguageTag;
            Header = moHeader;
            _translationLookup = translationDictionary ?? new Dictionary<string, string>();
            _userDefinedTranslations = new Dictionary<string, string>();
        }

        public void AddCustomTranslation(string s, string t)
        {
            lock (_userDefinedTranslations)
            {
                if (_userDefinedTranslations.ContainsKey(s))
                    return;

                _userDefinedTranslations[s] = t;
            }
        }

        public virtual string LookupString(string s)
        {
            if (_translationLookup.TryGetValue(s, out string? t))
                return t;

            lock (_userDefinedTranslations)
            {
                if (_userDefinedTranslations.TryGetValue(s, out t))
                {
                    return t;
                }
            }

#if DEBUG
            Log.Warning(L.LogCat, $"[no translation for string] [{s}]");
#endif
            return s;
        }

        public virtual string LookupPluralString(string s, string p, long n)
        {
            int pluralIndex = Plurals.PluralRules.GetPluralIndexForCulture(IetfLanguageTag, n);
            //singular
            string? t;
            if (pluralIndex == 0)
            {
                if (_translationLookup.TryGetValue(s, out t))
                    return t;
            }

            //plural
            string key = s + pluralIndex;
            if (_translationLookup.TryGetValue(key, out string? tp))
            {
                return tp;
            }

            //fallback to 1st plural rule
            if (pluralIndex > 1)
            {
                key = s + 1;
                if (_translationLookup.TryGetValue(key, out tp))
                {
                    return tp;
                }
            }

            //fallback to singular
            if (_translationLookup.TryGetValue(s, out t))
            {
                return t;
            }

            //no luck just return english
            return pluralIndex == 0 ? s : p;
        }

        public static ResourceContext FromFile(string filepath, string ietfLanguageTag)
        {
            using var stream = File.OpenRead(filepath);
            MoParseResult res = MOParser.Parse(stream);
            return new ResourceContext(ietfLanguageTag, res.MoHeader, res.TranslationDictionary);
        }

        public static ResourceContext FromStream(Stream sourceStream, string ietfLanguageTag)
        {
            MoParseResult res = MOParser.Parse(sourceStream);
            return new ResourceContext(ietfLanguageTag, res.MoHeader, res.TranslationDictionary);
        }
    }
}
