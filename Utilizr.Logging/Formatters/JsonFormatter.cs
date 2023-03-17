using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Formatters
{
    public class JsonFormatter : IFormatter
    {
        private static readonly List<string> _keyBlacklist = new() { "Asctime", "Msg", "Extra" };

        public string Format(LogRecord record)
        {
            var jsonObject = Convert(record)
                .Where(pair => !_keyBlacklist.Contains(pair.Key))
                .ToDictionary(kvPair => kvPair.Key, kvPair => kvPair.Value);

            return JsonConvert.SerializeObject(jsonObject);
        }

        static Dictionary<string, object?> Convert<T>(T value) where T : class
        {
            var fields = typeof(T).GetFields();
            var properties = typeof(T).GetProperties();

            var fieldsDict = fields.ToDictionary(x => x.Name, x => x.GetValue(value));
            var propertiesDict = properties.ToDictionary(x => x.Name, x => x.GetValue(value, null));

            return fieldsDict.Union(propertiesDict).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
