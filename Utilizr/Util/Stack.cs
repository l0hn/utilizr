using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilizr.Util
{
    public static class Stack
    {
        public static string CallerInfo(int skipFrames = 2, int depth = 8)
        {
            try
            {
                StackTrace trace = new StackTrace(skipFrames);
                return $"{trace.MethodChainStr(depth)}";
            }
            catch (Exception)
            {
            }

            return "(NO CALLER INFO)";
        }

        static string MethodChainStr(this StackTrace trace, int depth)
        {
            int count = 0;
            var sb = new StringBuilder();

            var frames = trace.GetFrames();
            if (frames == null)
                return "";

            foreach (var frame in frames)
            {
                var chevron = sb.Length == 0 ? "" : " < ";
                sb.Append($"{chevron}{frame.GetMethod()?.Name}");

                if (++count >= depth)
                    break;
            }

            return sb.ToString();
        }
    }
}
