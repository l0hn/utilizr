using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static Task<ShellResult> ExecAsync(string command, string? workingDir, bool asAdmin, params string[] args)
        {
            return Task.Run(() => Exec(command, workingDir, asAdmin, args));
        }

        public static Task<ShellResult> ExecAsync(string command, string? workingDir, bool asAdmin, bool useShellExecute, params string[] args)
        {
            return Task.Run(() => Exec(command, workingDir, asAdmin, useShellExecute, null, true, args));
        }

        public static ShellResult Exec(string command, params string[] args)
        {
            return Exec(command, null, false, args);
        }

        public static ShellResult Exec(string command, string? workingDir, bool asAdmin, params string[] args)
        {
            return Exec(command, workingDir, asAdmin, asAdmin, null, true, args);
        }

        public static ShellResult Exec(
            string command,
            string? workingDir,
            bool asAdmin,
            bool useShellExecute,
            IEnumerable<ShellEnvironmentVariable>? environmentVariables,
            bool waitForExit,
            params string[] args) 
        {
            using var proc = new Process();
            var result = new ShellResult(command, args);

            // Running as admin requires the 'Verb' property which is a shell feature, not a process feature.
            // Force to match previous behaviour where we only exposed UseShellExecute via the asAdmin parameter.
            if (asAdmin)
                useShellExecute = true;

            proc.StartInfo = new ProcessStartInfo(command)
            {
                UseShellExecute = useShellExecute,
                RedirectStandardOutput = !useShellExecute,
                RedirectStandardError = !useShellExecute,
                CreateNoWindow = true
            };
            foreach (var arg in args)
            {
                proc.StartInfo.ArgumentList.Add(arg);
            }
            proc.EnableRaisingEvents = false;
            if (workingDir != null)
            {
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

            if (!useShellExecute)
            {
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();
            }

            if (waitForExit)
            {
                proc.WaitForExit();
                result.ExitCode = proc.ExitCode;
            }

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
            var sb = new StringBuilder($"cmd: {CommandWithArgs}{Environment.NewLine}");
            sb.AppendLine($"exit code: {ExitCode}");
            if (Output.IsNotNullOrEmpty())
            {
                sb.AppendLine($"{Environment.NewLine}output:{Environment.NewLine}{Output}");
            }

            if (ErrorOutput.IsNotNullOrEmpty())
            {
                sb.AppendLine($"{Environment.NewLine}error: {ErrorOutput}");
            }

            return sb.ToString();
        }
    }
}
