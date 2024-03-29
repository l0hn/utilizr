﻿using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Utilizr.Async;
using Utilizr.Win32.Kernel32;
using Utilizr.Win32.Kernel32.Flags;
using Utilizr.Win32.Kernel32.Structs;
using static Utilizr.Win.Security.ShellProtected;

namespace Utilizr.Win.Util
{
    public static class WinShell
    {
        /// <summary>
        /// Start but don't wait on a process when running as a protected process.
        /// </summary>
        public static unsafe Task<uint> ExecProtectedAsync(
            ProcessStartInfo startInfo,
            ProcessIdCallbackDelegate? pidCallback = null,
            StandardInputCallbackDelegate? getInputDelegate = null,
            StandardOutputDataDelegate? outputDelegate = null,
            StandardErrorDataDelegate? errorDelegate = null)
        {
            return Task.Run(() =>
            {
                SafeFileHandle? parentInputPipeHandle = null;
                SafeFileHandle? childInputPipeHandle = null;
                SafeFileHandle? parentOutputPipeHandle = null;
                SafeFileHandle? childOutputPipeHandle = null;
                SafeFileHandle? parentErrorPipeHandle = null;
                SafeFileHandle? childErrorPipeHandle = null;

                var pi = new PROCESS_INFORMATION();
                try
                {
                    var saProcess = new SECURITY_ATTRIBUTES();
                    saProcess.nLength = (uint)Marshal.SizeOf(saProcess);

                    var saThread = new SECURITY_ATTRIBUTES();
                    saThread.nLength = (uint)Marshal.SizeOf(saThread);

                    var si = new STARTUPINFO();
                    si.cb = (uint)Marshal.SizeOf(si);
                    si.wShowWindow = ShowWindowFlags.SW_SHOW;

                    if (startInfo.RedirectStandardInput || startInfo.RedirectStandardOutput || startInfo.RedirectStandardError)
                    {
                        if (startInfo.RedirectStandardInput)
                        {
                            CreatePipe(out parentInputPipeHandle, out childInputPipeHandle, true);
                        }
                        else
                        {
                            childInputPipeHandle = new SafeFileHandle(Kernel32.GetStdHandle(StdHandleTypes.STD_INPUT_HANDLE), false);
                        }

                        if (startInfo.RedirectStandardOutput)
                        {
                            CreatePipe(out parentOutputPipeHandle, out childOutputPipeHandle, false);
                        }
                        else
                        {
                            childOutputPipeHandle = new SafeFileHandle(Kernel32.GetStdHandle(StdHandleTypes.STD_OUTPUT_HANDLE), false);
                        }

                        if (startInfo.RedirectStandardError)
                        {
                            CreatePipe(out parentErrorPipeHandle, out childErrorPipeHandle, false);
                        }
                        else
                        {
                            childErrorPipeHandle = new SafeFileHandle(Kernel32.GetStdHandle(StdHandleTypes.STD_ERROR_HANDLE), false);
                        }

                        si.hStdInput = childInputPipeHandle.DangerousGetHandle();
                        si.hStdOutput = childOutputPipeHandle.DangerousGetHandle();
                        si.hStdError = childErrorPipeHandle.DangerousGetHandle();

                        si.dwFlags |= (uint)STARTUPINFO_FLAGS.STARTF_USESTDHANDLES;
                    }


                    uint creationFlags = 0;
                    if (startInfo.CreateNoWindow)
                        creationFlags |= ProcessCreationFlags.CREATE_NO_WINDOW;

                    creationFlags |= ProcessCreationFlags.CREATE_UNICODE_ENVIRONMENT;
                    //creationFlags |= ProcessCreationFlags.CREATE_NEW_CONSOLE;

                    string? environmentBlock = null;
                    if (startInfo.EnvironmentVariables != null)
                    {
                        environmentBlock = GetEnvironmentVariablesBlock(startInfo.EnvironmentVariables);
                    }

                    var workingDirectory = startInfo.WorkingDirectory;
                    if (workingDirectory.Length == 0)
                    {
                        workingDirectory = null;
                    }

                    var cmdLine = $"\"{startInfo.FileName}\" {startInfo.Arguments}";
                    try
                    {
                        fixed (char* environmentBlockPtr = environmentBlock)
                        {
                            if (!Kernel32.CreateProcess(
                                null,
                                cmdLine,
                                ref saProcess,
                                ref saThread,
#if DEBUG
                                true,
#else
                                false,
#endif
                                creationFlags,
                                nint.Zero,
                                workingDirectory,
                                ref si,
                                out pi))
                            {
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            }
                        }

                        pidCallback?.Invoke(pi.dwProcessId);
                    }
                    finally
                    {
                        childInputPipeHandle?.Dispose();
                        childOutputPipeHandle?.Dispose();
                        childErrorPipeHandle?.Dispose();
                    }

                    if (startInfo.RedirectStandardInput)
                    {
                        var enc = startInfo.StandardInputEncoding ?? GetEncoding((int)Kernel32.GetConsoleOutputCP());
                        var standardInput = new StreamWriter(new FileStream(parentInputPipeHandle!, FileAccess.Write, 4096, false), enc);
                        standardInput.AutoFlush = true;
                        getInputDelegate?.Invoke(standardInput);
                    }

                    AsyncStreamReader? asyncOut = null;
                    if (startInfo.RedirectStandardOutput)
                    {
                        var enc = startInfo.StandardOutputEncoding ?? GetEncoding((int)Kernel32.GetConsoleOutputCP());
                        asyncOut = new AsyncStreamReader(
                            new FileStream(parentOutputPipeHandle!, FileAccess.Read, 4096, false),
                            (outLine) =>
                            {
                                if (string.IsNullOrEmpty(outLine))
                                    return;

                                outputDelegate?.Invoke(outLine);
                            },
                            enc
                        );
                        asyncOut.BeginReadLine();
                    }

                    AsyncStreamReader? asyncError = null;
                    if (startInfo.RedirectStandardError)
                    {
                        var enc = startInfo.StandardErrorEncoding ?? GetEncoding((int)Kernel32.GetConsoleOutputCP());
                        asyncError = new AsyncStreamReader(
                            new FileStream(parentErrorPipeHandle!, FileAccess.Read, 4096, false),
                            (errLine) =>
                            {
                                if (string.IsNullOrWhiteSpace(errLine))
                                    return;

                                errorDelegate?.Invoke(errLine);
                            },
                            enc
                        );
                        asyncError.BeginReadLine();
                    }

                    Kernel32.WaitForSingleObject(pi.hProcess, Kernel32.WAIT_FOR_OBJECT_INFINITE);

                    if (startInfo.RedirectStandardOutput)
                    {
                        asyncOut!.CancelOperation();
                        asyncOut!.Dispose();
                    }

                    if (startInfo.RedirectStandardError)
                    {
                        asyncError!.CancelOperation();
                        asyncError!.Dispose();
                    }

                    if (!Kernel32.GetExitCodeProcess(pi.hProcess, out uint exitCode))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    return exitCode;
                }
                finally
                {
                    if (pi.hProcess != nint.Zero)
                        Kernel32.CloseHandle(pi.hProcess);

                    if (pi.hThread != nint.Zero)
                        Kernel32.CloseHandle(pi.hThread);

                    parentInputPipeHandle?.Dispose();

                    parentOutputPipeHandle?.Dispose();

                    parentErrorPipeHandle?.Dispose();
                }
            });
        }

        static void CreatePipe(out SafeFileHandle parentHandle, out SafeFileHandle childHandle, bool parentInputs)
        {
            const int DUPLICATE_SAME_ACCESS = 2;
            SECURITY_ATTRIBUTES securityAttributesParent = default;
            securityAttributesParent.bInheritHandle = true;

            SafeFileHandle? hTmp = null;
            try
            {
                if (parentInputs)
                {
                    CreatePipeWithSecurityAttributes(out childHandle, out hTmp, ref securityAttributesParent, 0);
                }
                else
                {
                    CreatePipeWithSecurityAttributes(out hTmp, out childHandle, ref securityAttributesParent, 0);
                }

                var currProc = Process.GetCurrentProcess();
                var currHandle = currProc.Handle;

                if (!Kernel32.DuplicateHandle(currHandle,
                                              hTmp,
                                              currHandle,
                                              out parentHandle,
                                              0,
                                              false,
                                              DUPLICATE_SAME_ACCESS))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            finally
            {
                if (hTmp != null && !hTmp.IsInvalid)
                {
                    hTmp.Dispose();
                }
            }
        }

        static void CreatePipeWithSecurityAttributes(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, ref SECURITY_ATTRIBUTES lpPipeAttributes, int nSize)
        {
            bool ret = Kernel32.CreatePipe(out hReadPipe, out hWritePipe, ref lpPipeAttributes, nSize);
            if (!ret || hReadPipe.IsInvalid || hWritePipe.IsInvalid)
            {
                throw new Win32Exception();
            }
        }

        static Encoding GetEncoding(int codepage)
        {
            const int Utf8CodePage = 65001;

            int defaultEncCodePage = Encoding.GetEncoding(0).CodePage;

            if (defaultEncCodePage == codepage || defaultEncCodePage != Utf8CodePage)
            {
                return Encoding.GetEncoding(codepage);
            }

            //if (codepage != Utf8CodePage)
            //{
            //    return new OSEncoding(codepage);
            //}

            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        }

        static string GetEnvironmentVariablesBlock(StringDictionary sd)
        {
            // https://docs.microsoft.com/en-us/windows/win32/procthread/changing-environment-variables
            // "All strings in the environment block must be sorted alphabetically by name. The sort is
            //  case-insensitive, Unicode order, without regard to locale. Because the equal sign is a
            //  separator, it must not be used in the name of an environment variable."

            var keys = new string[sd.Count];
            sd.Keys.CopyTo(keys, 0);
            Array.Sort(keys, StringComparer.OrdinalIgnoreCase);

            // Join the null-terminated "key=val\0" strings
            var result = new StringBuilder(8 * keys.Length);
            foreach (string key in keys)
            {
                result.Append(key).Append('=').Append(sd[key]).Append('\0');
            }

            return result.ToString();
        }
    }
}
