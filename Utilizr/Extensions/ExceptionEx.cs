using System;
using System.Text;

namespace Utilizr.Extensions
{
    public static class ExceptionEx
    {
        /// <summary>
        /// Generates a nice string of messages / stacktraces for the exception, diving into all inner exceptions.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string ToDebugString(this Exception ex)
        {
            var err = ex;
            var sb = new StringBuilder();
            while (err != null)
            {
                sb.AppendLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}");
                err = ex.InnerException;
            }

            return sb.ToString();
        }
    }
}
