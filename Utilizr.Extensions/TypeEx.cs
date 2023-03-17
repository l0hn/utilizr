using System;
using System.ComponentModel;
using System.Linq;

namespace Utilizr.Extensions
{
    public static class TypeEx
    {
        public static bool IsConvertibleFrom<T>(this Type t)
        {
            var converter = t.GetConverter();
            if (converter != null && converter.CanConvertFrom(typeof(T)))
            {
                return true;
            }
            return false;
        }

        public static TypeConverter GetConverter(this Type t)
        {
            return TypeDescriptor.GetConverter(t);
        }

        public static T? ConvertTo<T>(this object o)
        {
            if (o == null)
                throw new ArgumentException("Cannot convert to T from null object");

            var typeConverter = GetConverter(typeof(T));
            var tConverted = typeConverter.ConvertFromString(o.ToString()!);
            return (T?)tConverted;
        }

        public static bool In<T>(this T t, params T[] values)
        {
            return values.Contains(t);
        }
    }
}
