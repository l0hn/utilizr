using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utilizr.Extensions;

namespace Utilizr
{
    public static class Shell
    {
        public static Task<ShellResult> ExecAsync(string command, params string[] args) 
        {
            return Task.Run(() => Exec(command, args));
        }

        public static Task<ShellResult> ExecAsync(string command, string workingDir, bool asAdmin, params string[] args)
        {
            return Task.Run(() => Exec(command, workingDir, asAdmin, args));
        }

        public static ShellResult Exec(string command, params string[] args)
        {
            return Exec(command, null, false, args);
        }

        public static ShellResult Exec(string command, string? workingDir, bool asAdmin, params string[] args)
        {
            return Exec(command, workingDir, asAdmin, null, args);
        }

        public static ShellResult Exec(
            string command,
            string? workingDir,
            bool asAdmin,
            IEnumerable<ShellEnvironmentVariable>? environmentVariables,
            params string[] args) 
        {
            using var proc = new Process();
            var result = new ShellResult(command, args);

            proc.StartInfo = new ProcessStartInfo(command, string.Join(" ", args)) //command, string.Join(" ", args) KRIS
            {
                UseShellExecute = asAdmin,
                RedirectStandardOutput = !asAdmin,
                RedirectStandardError = !asAdmin,
                Arguments = string.Join(" ", args), //KRIS 
                CreateNoWindow = true
            };
            proc.EnableRaisingEvents = false;
            
            if (workingDir != null)
            {
                proc.StartInfo.FileName = Path.Combine(workingDir, command); //KRIS
                proc.StartInfo.WorkingDirectory = workingDir;
            }
            if (environmentVariables != null)
            {
                foreach (var item in environmentVariables)
                {
                    proc.StartInfo.EnvironmentVariables[item.Name] = item.Value;
                }
            }
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (asAdmin)
            {
                proc.StartInfo.Verb = "runas";
            }

            proc.OutputDataReceived += (o, eventArgs) =>
            {
                if (eventArgs.Data == null)
                    return;

                result.Output += eventArgs.Data.TrimEnd() + Environment.NewLine;
            };

            proc.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (eventArgs.Data == null)
                    return;

                result.ErrorOutput += eventArgs.Data.TrimEnd() + Environment.NewLine;
            };
            proc.Start();

            if (!asAdmin)
            {
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
            }

            proc.WaitForExit();

            result.ExitCode = proc.ExitCode;

            return result;
        }
    }

    public class ShellEnvironmentVariable
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ShellEnvironmentVariable(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    public class ShellResult
    {
        public int ExitCode { get; set;}
        public string? Output { get;  set; }
        public string? ErrorOutput { get;  set; }
        public string Command { get; set; }
        public string[] Args { get; set; }
        public string CommandWithArgs => Args != null && Args.Length > 0
            ? string.Format("{0} {1}", Command, string.Join(" ", Args))
            : Command;

        public ShellResult(string command, string[] args)
        {
            Command = command;
            Args = args;
        }
        
        public void ThrowIfError() {
            if (ExitCode != 0)
            {
                throw new Exception(Info());
            }
        }

        public string Info() {
            StringBuilder sb = new StringBuilder($"cmd: {CommandWithArgs}");
            sb.AppendLine($"exit code: {ExitCode}");
            if (Output.IsNotNullOrEmpty())
            {
                sb.AppendLine($"\noutput:\n{Output}");
            }

            if (ErrorOutput.IsNotNullOrEmpty())
            {
                sb.AppendLine($"\nerror: {ErrorOutput}");
            }

            return sb.ToString();
        }
    }
}
