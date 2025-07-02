using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Utilizr.Crypto;

namespace Utilizr.Extensions
{
    public static class StringEx
    {
        public static bool IsNullOrEmpty(this string? str) {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotNullOrEmpty(this string? str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrWhitespace(this string? str)
        {
            if (string.IsNullOrEmpty(str))
                return true;

            return !str.Any(p => !char.IsWhiteSpace(p));
        }

        public static bool IsNotNullOrWhitespace(this string? str)
        {
            return !IsNullOrWhitespace(str);
        }

        public static bool IsWhiteSpaceOnly(this string? str)
        {
            if (str.IsNullOrEmpty())
                return false;

            return str.IsNullOrWhitespace();
        }

        /// <summary>
        /// Converts a string in the format of 0.0.0 to a version object
        /// </summary>
        /// <returns>The version.</returns>
        /// <param name="versionStr">Version string.</param>
        public static Version ToVersion(this string versionStr) {
            return new Version(versionStr);
        }

        /// <summary>
        /// Converts an unreliable string to a version object which would usually throw
        /// </summary>
        /// <param name="versionStr">The version.param>
        public static Version ToVersionSafe(this string versionStr)
        {
            var numbers = new List<int>();
            foreach (var p in versionStr.Split('.'))
            {
                try
                {
                    numbers.Add(Convert.ToInt32(p));
                }
                catch
                {
                    numbers.Add(0);
                }
            }

            // pad missing, doesn't like single version if someone has specified something like a year, 2020
            while (numbers.Count < 4)
            {
                numbers.Add(0);
            }

            return new Version(
                numbers[0],
                numbers[1],
                numbers[2],
                numbers[3]
            );
        }

        /// <summary>
        /// Returns the zero-based index of the Nth character inside the string.
        /// </summary>
        /// <param name="s">The string to search</param>
        /// <param name="c">Character to search</param>
        /// <param name="n">The Nth occurrence of the character to search</param>
        /// <returns>If not found -1. Else index of Nth character</returns>
        public static int GetNthIndex(this string s, char c, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == c)
                {
                    count++;
                    if (count == n)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static string GetFirstName(this string fullName)
        {
            if (fullName.IsNullOrEmpty())
                return fullName;

            var parts = fullName.Split(' ');
            if (parts.Length <= 1)
            {
                return fullName;
            }
            return parts[0];
        }

        /// <summary>
        /// Format the specified str and args.
        /// </summary>
        /// <param name="str">String.</param>
        /// <param name="args">Arguments.</param>
        // Analysis disable once InconsistentNaming
        // Lower case method name avoids conflict with static method String.Format()
        public static string Format(this string str, params object[] args)
        {
            return string.Format(str, args);
        }

        public static string StringBeforeLastIndefOf(this string str, char character)
        {
            var i = str.LastIndexOf(character);
            if (i <= 0)
            {
                return "";
            }
            if (i == str.Length - 1)
            {
                return str;
            }
            return str[..i];
        }

        public static string StringAfterLastIndexOf(this string str, char character)
        {
            var i = str.LastIndexOf(character);
            if (i <= 0)
                return string.Empty;

            if (i == str.Length - 1)
                return string.Empty;

            return str[i..];
        }


        public static int GetLineNumber(this string text, int index)
        {
            if (index >= text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"Parameter {nameof(index)} is out of bounds and bigger than string's length.");

            int lineCounter = 1;

            for (int i = 0; i < index; i++)
            {
                if (text[i] == Environment.NewLine[0]) lineCounter++;
            }
            return lineCounter;
        }

        public static int GetColumn(this string text, int index)
        {
            if (index >= text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), $"Parameter {nameof(index)} is out of bounds and bigger than string's length.");

            var newLine ="\n"; //Not using Environment.NewLine here because Regex counts a newline as 1 character, whereas Environment.NewLine on windows is 2 characters
            var lines = text.Split(new[] { newLine }, StringSplitOptions.None);

            int charCounter = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                var lineLength = lines[i].Length + (i > 0 ? newLine.Length : 0);
                charCounter += lineLength;
                if (index < charCounter)
                {
                    return lineLength - (charCounter - index);
                }
            }

            throw new Exception("Uh oh. This shouldn't have happened.");
        }

        public static IEnumerable<string> GraphemeClusters(this string s)
        {
            var enumerator = StringInfo.GetTextElementEnumerator(s);
            while (enumerator.MoveNext())
            {
                yield return (string)enumerator.Current;
            }
        }

        public static string ReverseGraphemeClusters(this string s)
        {
            return string.Join(string.Empty, s.GraphemeClusters().Reverse().ToArray());
        }

        /// <summary>
        /// Expands environment variables
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ExpandVars(this string s)
        {
            return Environment.ExpandEnvironmentVariables(s);
        }


        public static string TrimStart(this string target, string trimString)
        {
            // Substring appears to be ~50% quicker over StartsWith
            var index = trimString.Length;
            if (index > target.Length)
                return target;

            var startString = target[..index];
            if (startString == trimString)
                return target[index..];

            return target;

            //string result = target;

            //if (result.StartsWith(trimString))
            //{
            //    result = result.Substring(trimString.Length);
            //}

            //return result;
        }

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

            //string result = target;
            //if (result.EndsWith(trimString))
            //{
            //    result = result.Substring(0, result.Length - trimString.Length);
            //}

            //return result;
        }

        public static bool HasInvalidSurrogatePair(this string str)
        {
            // https://docs.microsoft.com/en-gb/windows/win32/intl/surrogates-and-supplementary-characters
            // first range: U+D800 - U+DBFF         dec: 55296 - 56319
            // second range: U+DC00 - U+DFFF        dec: 56320 - 57343

            const int highSurrogateLowerThreshold = 55269;
            const int highSurrogateUpperThreshold = 56319;

            const int lowSurrogateLowerThreshold = 56320;
            const int lowSurrogateUpperThreshold = 57343;

            if (str.IsNullOrEmpty())
                return false;

            var chars = str.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                int c = chars[i];
                if (c >= highSurrogateLowerThreshold && c <= highSurrogateUpperThreshold)
                {
                    // next char must be in low threshold
                    if (i >= chars.Length - 1)
                        return true;

                    var nextC = chars[i + 1];
                    if (nextC >= lowSurrogateLowerThreshold && nextC <= lowSurrogateUpperThreshold)
                    {
                        i++; // skip this next char
                        continue;
                    }

                    return true;
                }
                else if (c >= lowSurrogateLowerThreshold && c <= lowSurrogateUpperThreshold)
                {
                    // no high surrogate specified first
                    return true;
                }
            }

            return false;
        }

        public static string HashSHA256(this string str)
        {
            return Hash.SHA256(str);
        }

        public static string HashMD5(this string str)
        {
            return Hash.MD5(str);
        }

        public static string Quote(this string str) {
            return $"\"{str}\"";
        }
    }
}