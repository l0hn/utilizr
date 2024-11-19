using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Utilizr.FileSystem;

namespace Tests
{
    public class DirectoryHelperTests
    {
        static List<string> _dummyFiles = new();
        static string _tempDir = Path.Combine(Path.GetTempPath(), "directory_helper", "source", Guid.NewGuid().ToString());
        static string _tempTargetDir = Path.Combine(Path.GetTempPath(), "directory_helper", "target", Guid.NewGuid().ToString()); 

        [SetUp]
        public void Setup()
        {
            if (!Directory.Exists(_tempDir))
            {
                Directory.CreateDirectory(_tempDir);
            }
            for (int i = 0; i < 10; i++)
            {
                var file = Path.Combine(_tempDir, $"test_file_{i}.txt");
                File.WriteAllText(file, $"this is test file #{i}");
                _dummyFiles.Add(file);
            }
        }

        [TearDown]
        public void TearDown() {
            Directory.Delete(_tempDir, true);
            Directory.Delete(_tempTargetDir, true);
            _dummyFiles.Clear();
        }

        [Test]
        public void TestCopyErrorRetrySucceed()
        {
            var targetDir = Path.Combine(_tempTargetDir);

            //lock a file
            using FileStream fs = File.Open(_dummyFiles[3], FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            DirectoryHelper.CopyDirectoryContents (
                _tempDir, 
                targetDir,
                false,
                true,
                null,
                null,
                null,
                errorArgs => {
                    //let it fail an arbitraty 5 times, unlock and retry
                    if (errorArgs.ErrorCountForFile <= 5)
                    {
                        errorArgs.ContinueAction = CopyDirectoryContinueAction.RETRY;
                    }
                    if (errorArgs.ErrorCountForFile == 5)
                    {
                        //unlock the file before the 6th try which should succeed
                        fs.Dispose();
                    }
                    if (errorArgs.ErrorCountForFile > 5) 
                    {
                        //this shouldn't happen
                        errorArgs.ContinueAction = CopyDirectoryContinueAction.FAIL;   
                    }           
                }
            );   

            foreach (var f in _dummyFiles)
            {
                var name = Path.GetFileName(f);
                Assert.IsTrue(File.Exists(Path.Combine(_tempTargetDir, name)));
            }
        }

        [Test]
        public void TestCopyErrorRetryFail()
        {
            var targetDir = Path.Combine(_tempTargetDir);

            //lock a file
            using FileStream fs = File.Open(_dummyFiles[3], FileMode.Open, FileAccess.ReadWrite, FileShare.None);


            Assert.Throws<IOException>(() => {    
                DirectoryHelper.CopyDirectoryContents (
                    _tempDir, 
                    targetDir,
                    false,
                    true,
                    null,
                    null,
                    null,
                    errorArgs => {
                        //let it fail an arbitraty 5 times, unlock and retry
                        if (errorArgs.ErrorCountForFile <= 5)
                        {
                            errorArgs.ContinueAction = CopyDirectoryContinueAction.RETRY;
                        }
                        if (errorArgs.ErrorCountForFile > 5) 
                        {
                            errorArgs.ContinueAction = CopyDirectoryContinueAction.FAIL;   
                        }           
                    }
                );   
            });
        }

        [Test]
        public void TestNoErrorHandler() {
           var targetDir = Path.Combine(_tempTargetDir);

            //lock a file
            using FileStream fs = File.Open(_dummyFiles[3], FileMode.Open, FileAccess.ReadWrite, FileShare.None);

            TestDelegate copyFunc = () => {    
                DirectoryHelper.CopyDirectoryContents (
                    _tempDir, 
                    targetDir,
                    false,
                    true
                );   
            };


            Assert.Throws<IOException>(copyFunc); 

            fs.Dispose();

            Assert.DoesNotThrow(copyFunc);
        }

    }
}