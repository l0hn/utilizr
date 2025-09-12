using System;
using System.Text.RegularExpressions;

namespace Utilizr.Extensions
{
    public static class EnumEx
    {
        public static string ToLowerSnakeCase(this Enum value)
        {
            string name = value.ToString();
            string snake = Regex.Replace(name, @"(?<!^)([A-Z])", "_$1");
            return snake.ToLowerInvariant();
        }
    }
}
