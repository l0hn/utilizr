using System.Collections.Generic;
using System.IO;
using Utilizr.Globalisation.Parsers;

namespace Utilizr.Globalisation.Localisation
{
    /// <summary>
    /// A ResourseContext for validation purposes.
    /// Support for iterating over all translation to ensure *.po have correct formatting.
    /// </summary>
    public class ValidationResourceContext : ResourceContext
    {
        public ValidationResourceContext(string ietfLanguageTag, MoHeader moHeader, Dictionary<string, string> translationDictionary)
            : base(ietfLanguageTag, moHeader, translationDictionary)
        {

        }

        public IEnumerable<KeyValuePair<string, string>> Entries()
        {
            foreach (var item in _translationLookup)
            {
                yield return item;
            }

            lock (_userDefinedTranslations)
            {
                foreach (var item in _userDefinedTranslations)
                {
                    yield return item;
                }
            }
        }

        public static new ValidationResourceContext FromFile(string filepath, string ietfLanguageTag)
        {
            using var stream = File.OpenRead(filepath);
            MoParseResult res = MOParser.Parse(stream);
            return new ValidationResourceContext(ietfLanguageTag, res.MoHeader, res.TranslationDictionary);
        }
    }
}