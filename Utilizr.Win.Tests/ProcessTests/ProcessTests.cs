using System;
using System.Diagnostics;
using NUnit.Framework;
using Utilizr;

namespace Tests
{
    public class ProcessTests
    {
        [SetUp]
        public void Setup()
        {
            //build the console app
            int result = Shell.Exec("dotnet", "build", "../../../../Utilizr.Win.Tests.get_parent_process_id/get_parent_process_id.csproj").ExitCode;
            if (result != 0)
            {
                throw new Exception("failed to build console project");
            }
        }

        [Test]
        public void GetParentProcess()
        {
            Console.WriteLine($"base dir: {AppContext.BaseDirectory}");            
            var thisProcess = Process.GetCurrentProcess();
            var thisPid = thisProcess.Id;
            var resultFromSubProcess = Shell.Exec("..\\..\\..\\..\\Utilizr.Win.Tests.get_parent_process_id\\bin\\Debug\\net8.0-windows\\get_parent_process_id.exe");

            Assert.That(resultFromSubProcess.ExitCode, Is.EqualTo(thisPid));
        }
    }
}