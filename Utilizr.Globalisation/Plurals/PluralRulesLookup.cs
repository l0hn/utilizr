using System.Collections.Generic;

namespace Utilizr.Globalisation.Plurals
{
    /// <summary>
    /// Class used to lookup plural rule lookup delegates
    /// </summary>
    public static class PluralRules
    {
        private static readonly object LOCK_OBJECT = new();
        //This delegate should return the 0 based index of the plural rule for the specified culture and given number
        public delegate int GetPluralIndexDelegate(string ietfLanguageTag, long n);

        private static Dictionary<string, GetPluralIndexDelegate>? _lookupDictionary;
        private static Dictionary<string, GetPluralIndexDelegate> LookupDictionary
        {
            get
            {
                if (_lookupDictionary == null)
                    PopulateLookupDictionary();

                return _lookupDictionary!;
            }
        }

        private static void PopulateLookupDictionary()
        {
            lock (LOCK_OBJECT)
            {
                if (_lookupDictionary == null)
                {
                    var d = new Dictionary<string, GetPluralIndexDelegate>();
                    //add rules here
                    d["fr-FR"] = (l, n) => n > 1 ? 1 : 0;
                    d["fr"] = d["fr_FR"] = d["fr-FR"];

                    d["pl-PL"] = (l, n) =>
                    {
                        if (n == 1)
                            return 0;

                        if (n % 10 >= 2 && n % 4 <= 4 && (n % 100 < 10 || n % 100 >= 20))
                            return 1;

                        return 2;
                    };
                    d["pl"] = d["pl_PL"] = d["pl-PL"];
                    _lookupDictionary = d;
                }
            }
        }

        private static int DefaultPluralRule(long n)
        {
            return n == 1 ? 0 : 1;
        }


        public static int GetPluralIndexForCulture(string ietfLanguageTag, long n)
        {
            if (LookupDictionary.TryGetValue(ietfLanguageTag, out GetPluralIndexDelegate? value))
            {
                return value(ietfLanguageTag, n);
            }

            return DefaultPluralRule(n);
        }
    }
}
