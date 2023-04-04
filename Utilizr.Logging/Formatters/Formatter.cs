using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
//using Utilizr.Extensions;
using Utilizr.Logging.Interfaces;

namespace Utilizr.Logging.Formatters
{
    /// <summary>
    /// Formatter instances are used to convert a LogRecord to text.
    /// 
    /// The formatter can be initialised with a format string which makes use of knowledge of the LogRecord fields
    /// 
    /// {Category}          Name of the logger (logging channel).
    /// {Level}             string representation of the logging level for the message ("DEBUG", "INFO", "WARNING", "ERROR", "CRITICAL").
    /// {Created}           Time when the LogRecord was created.
    /// {Asctime}           Textual time when LogRecord was created.
    /// {RelativeCreated}   TimeSpan between when the LogRecord was created and when the LogRecord class was loaded.
    /// {Message}           The message for the LogRecord, only computed just as the record is emitted.
    /// </summary>
    public class Formatter : IFormatter
    {
        private readonly string _fmt;
        private readonly string? _dateFormat;

        /// <summary>
        /// Initialise the formatter with the specified format string, or if unspecified a default.
        /// </summary>
        /// <param name="format">Use the specified format string, or if not a default (Defaults.format)</param>
        /// <param name="dateFormat">Allow for custom date formatting using in DateTime.ToString method, or if not a default (Defaults.dateFormat)</param>
        public Formatter(string? format = null, string? dateFormat = null)
        {
            _fmt = format ?? Defaults.Format;
            _dateFormat = dateFormat;
        }

        /// <summary>
        /// Return the creation time of the specified LogRecord as formatted text
        /// </summary>
        /// <param name="record"></param>
        /// <param name="dateFormat"></param>
        /// <returns></returns>
        private static string FormatTime(LogRecord record, string? dateFormat)
        {
            return dateFormat != null
                ? record.Created.ToString(dateFormat)
                : record.Created.ToString(Defaults.DateFormat);
        }

        /// <summary>
        /// Check if the format uses the creation time of the record
        /// </summary>
        private bool UsesTime => !string.IsNullOrEmpty(_fmt) && _fmt.Contains("{Asctime}");

        /// <summary>
        /// Format the specified record as text
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        public string Format(LogRecord record)
        {
            if (UsesTime)
                record.Asctime = FormatTime(record, _dateFormat);

            string s = FormatWith(record);

            record.ErrorText ??= record.Error?.ToString();

            if (record.ErrorText != null)
            {
                s = s.TrimEnd(Environment.NewLine);
                s += Environment.NewLine;

                s += record.ErrorText;
            }

            return s;
        }

        private string FormatWith(LogRecord source)
        {
            if (_fmt == null)
                throw new InvalidOperationException($"{nameof(_fmt)} has null value");

            var formattedStrings = SplitFormat().Select(p => p.Eval(source));
            return string.Join("", formattedStrings.ToArray());
        }

        private IEnumerable<ITextExpression> SplitFormat()
        {
            int exprEndIndex = -1;
            int expStartIndex;

            do
            {
                expStartIndex = IndexOfExpressionStart(_fmt, exprEndIndex + 1);
                if (expStartIndex < 0)
                {
                    //everything after last end brace index.
                    if (exprEndIndex + 1 < _fmt.Length)
                    {
                        yield return new LiteralFormat(_fmt[(exprEndIndex + 1)..]);
                    }
                    break;
                }

                if (expStartIndex - exprEndIndex - 1 > 0)
                {
                    //everything up to next start brace index
                    yield return new LiteralFormat(_fmt.Substring(exprEndIndex + 1, expStartIndex - exprEndIndex - 1));
                }

                int endBraceIndex = IndexOfExpressionEnd(_fmt, expStartIndex + 1);
                if (endBraceIndex < 0)
                {
                    //rest of string, no end brace (could be invalid expression)
                    yield return new FormatExpression(_fmt[expStartIndex..]);
                }
                else
                {
                    exprEndIndex = endBraceIndex;
                    //everything from start to end brace.
                    yield return new FormatExpression(_fmt.Substring(expStartIndex, endBraceIndex - expStartIndex + 1));

                }
            } while (expStartIndex > -1);
        }

        private int IndexOfExpressionStart(string format, int startIndex)
        {
            int index = format.IndexOf('{', startIndex);
            if (index == -1)
            {
                return index;
            }

            //peek ahead.
            if (index + 1 < format.Length)
            {
                char nextChar = format[index + 1];
                if (nextChar == '{')
                {
                    return IndexOfExpressionStart(format, index + 2);
                }
            }

            return index;
        }

        private int IndexOfExpressionEnd(string format, int startIndex)
        {
            int endBraceIndex = format.IndexOf('}', startIndex);
            if (endBraceIndex == -1)
            {
                return endBraceIndex;
            }
            //start peeking ahead until there are no more braces...
            // }}}}
            int braceCount = 0;
            for (int i = endBraceIndex + 1; i < format.Length; i++)
            {
                if (format[i] == '}')
                {
                    braceCount++;
                }
                else
                {
                    break;
                }
            }
            if (braceCount % 2 == 1)
            {
                return IndexOfExpressionEnd(format, endBraceIndex + braceCount + 1);
            }

            return endBraceIndex;
        }

        private interface ITextExpression
        {
            string? Eval(LogRecord o);
        }

        private class FormatExpression : ITextExpression
        {
            public string Expression { get; private set; }
            public string? Format { get; private set; }

            readonly bool _invalidExpression = false;

            public FormatExpression(string expression)
            {
                if (!expression.StartsWith("{") || !expression.EndsWith("}"))
                {
                    _invalidExpression = true;
                    Expression = expression;
                    return;
                }

                string expressionWithoutBraces = expression[1..^1];
                int colonIndex = expressionWithoutBraces.IndexOf(':');
                int commaIndex = expressionWithoutBraces.IndexOf(',');
                if (colonIndex < 0 && commaIndex < 0)
                {
                    Expression = expressionWithoutBraces;
                }
                else
                {
                    int startIndex;
                    if (colonIndex >= 0 && commaIndex >= 0)
                        startIndex = Math.Min(colonIndex, commaIndex);
                    else
                        startIndex = Math.Max(colonIndex, commaIndex);

                    Expression = expressionWithoutBraces[..startIndex];
                    Format = expressionWithoutBraces[startIndex..];
                }
            }

            public string? Eval(LogRecord o)
            {
                if (_invalidExpression)
                {
                    throw new FormatException("Invalid expression");
                }
                try
                {
                    ////Check extra dictionary first before assuming expression is a property
                    //if(o.Extra != null)
                    //{
                    //    //Key names will be wrapped in quotes
                    //    string noQuotes = Expression.Replace("\"", "")
                    //        .Replace("'", "");

                    //    if (o.Extra.ContainsKey(noQuotes))
                    //        return o.Extra[noQuotes].ToString();
                    //}

                    var bindingFlags = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;
                    PropertyInfo? propInfo = null;
                    object? value = null;
                    string? result = null;

                    var indexerStart = Expression.IndexOf('[');
                    var indexerFinish = Expression.IndexOf(']');

                    //Expression is a property and has index value, e.g. Extra["myKey"]
                    if(indexerStart > -1 && indexerFinish > indexerStart)
                    {
                        string propExpression = Expression[..indexerStart];
                        propInfo = o.GetType().GetProperty(propExpression, bindingFlags);
                        string indexer = Expression.Substring(indexerStart + 2, indexerFinish - indexerStart - 3); //+2 -3 to remove quotes

                        if (propInfo!.GetValue(o, null) is IDictionary dictionary)
                        {
                            value = dictionary[indexer];
                            string safeFormat = Format ?? string.Empty;
                            result = string.Format("{0" + safeFormat + "}", value ?? string.Empty);
                        }
                    }
                    else
                    {
                        propInfo = o.GetType().GetProperty(Expression, bindingFlags);
                        value = propInfo!.GetValue(o, null);
                        result = string.IsNullOrEmpty(Format)
                            ? value?.ToString() ?? string.Empty
                            : string.Format("{0" + Format + "}", value ?? string.Empty);
                    }

                    return result;
                }
                catch (Exception ex)
                {
                    throw new FormatException(
                        $"Unable to extract value. Expression={Expression}, Format={Format ?? "Undefined"}",
                        ex
                    );
                }
            }
        }

        private class LiteralFormat : ITextExpression
        {
            public string LiteralText { get; private set; }

            public LiteralFormat(string literalText)
            {
                LiteralText = literalText;
            }

            public string Eval(LogRecord o)
            {
                string literalText = LiteralText
                    .Replace("{{", "{")
                    .Replace("}}", "}");
                return literalText;
            }
        }
    }
}
