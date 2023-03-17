using Utilizr.Globalisation.Parsers;

namespace Utilizr.Globalisation.Localisation
{
    internal class DummyResourceContext : ResourceContext
    {
        internal delegate string Single(string s);
        internal delegate string Plural(string s, string p, long n);

        private readonly Single _s;
        private readonly Plural _p;

        public DummyResourceContext(string ieftTag, Single s, Plural p) 
            : base(ieftTag, new MoHeader(), null)
        {
            _s = s;
            _p = p;
        }

        public override string LookupPluralString(string s, string p, long n)
        {
            return _p?.Invoke(s, p, n) ?? (n > 1 ? s : p);
        }

        public override string LookupString(string s)
        {
            return _s?.Invoke(s) ?? s;
        }
    }
}
