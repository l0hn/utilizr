// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using Utilizr.Win.Extensions;


if (Process.GetCurrentProcess().TryGetParentProcess(out var parentProcess) && parentProcess != null)
{
    Console.WriteLine(parentProcess.Id);
    Environment.Exit(parentProcess.Id);
}

Environment.Exit(-1);