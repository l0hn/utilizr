using Utilizr.Logging.Interfaces;
using Utilizr.Logging.Formatters;

namespace Utilizr.Logging
{
    public static class Defaults
    {
        public static IFormatter Formatter => new Formatter();
        public static string Format => "{Asctime} : {Category,10} : {Level,8} : {Message} : {InterestingObjects}";
        public static string DateFormat => "yyyy-MM-dd HH:mm:ss,fff (zzz)";
        public static LoggingLevel Level => LoggingLevel.INFO;
    }
}
