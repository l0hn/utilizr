namespace Utilizr.Logging
{
    internal static class StringEx
    {
        public static string TrimEnd(this string target, string trimString)
        {
            // Substring appears to be ~50% quicker over EndsWith
            var index = target.Length - trimString.Length;
            if (index < 0)
                return target;

            var endString = target.Substring(index, trimString.Length);
            if (endString == trimString)
                return target[..index];

            return target;
        }
    }
}
