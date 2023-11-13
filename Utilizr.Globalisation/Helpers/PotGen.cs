using System;
using System.Diagnostics;

namespace Utilizr.Globalisation.Helpers
{
    public static class PotGen
    {
        const string LIST_FILE = "utilizr_src_files.txt";
        public const string XGETTEXT_FORMAT_STRING = @"-L C# --from-code=UTF-8 --keyword=_N:1,2 --keyword=_IP:1,2 --keyword=_P:1,2 --keyword=_M:1,2 --keyword=_M --keyword=_P --keyword=_I --keyword=_ --files-from=""{0}"" -o ""{1}"" --add-comments=##";
        //public const string XGETTEXT_FORMAT_STRING = @"-L java --from-code=UTF-8 --keyword=n_:1,2 --keyword=ip_:1,2 --keyword=p_:1,2 --keyword=m:1,2 --keyword=m --keyword=p_ --keyword=i_ --keyword=t_ --files-from=""{0}"" -o ""{1}"" --add-comments=##";

        public static void CreatePotFile(string srcRootDir, string outputFile, string? getTextFormat=null)
        {
            Console.WriteLine("indexing source directory");
            if (SourceList.GenerateSourceList(srcRootDir, LIST_FILE) == 0)
            {
                throw new ArgumentException("No source files found in specified source root");
            }

            var platform = Environment.OSVersion.Platform;
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = platform == PlatformID.Unix || platform == PlatformID.MacOSX
                        ? "xgettext"
                        : "xgettext.exe",
                    Arguments = string.Format(getTextFormat ?? XGETTEXT_FORMAT_STRING, LIST_FILE, outputFile),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            p.OutputDataReceived += (sender, args) =>
            {
                Console.WriteLine(args.Data);
            };

            p.ErrorDataReceived += (sender, args) =>
            {
                Console.WriteLine(args.Data);
            };

            Console.WriteLine("Generating pot file...");
            p.Start();
            if (!p.WaitForExit(60000))
            {
                Console.WriteLine("Timeout waiting for xgettext process");
                return;
            }
            if (p.ExitCode != 0)
            {
                Console.WriteLine(p.StandardError.ReadToEnd());
            }
            Console.WriteLine(p.StandardOutput.ReadToEnd());
        }
    }
}