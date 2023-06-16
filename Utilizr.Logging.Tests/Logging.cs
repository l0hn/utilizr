using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Utilizr.Logging.Formatters;
using Utilizr.Logging.Handlers;
using Utilizr.Logging.Interfaces;
using Utilizr.Logging.Loggers;

namespace Utilizr.Logging.Tests
{
    [TestFixture]
    public class StaticLoggingTests
    {
        string LogFileNameBase { get { return Path.GetFileNameWithoutExtension(_logFilePath); } }
        string LogFileExtension { get { return Path.GetExtension(_logFilePath); } }
        string LogFileDirectory { get { return Path.GetDirectoryName(_logFilePath)!; } }

        string _logFilePath;
        string _rootLogFileLocation = Path.GetTempFileName() + ".log";
        private readonly Dictionary<string, string> loggerPaths = new();

        private string GetRolledOverPath(int number)
        {
            return Path.Combine(LogFileDirectory, LogFileNameBase + "." + number + LogFileExtension);
        }

        private IEnumerable<string> GetTimedRolledOverPaths(string filePath, Regex regex)
        {
            string prefix = Path.GetFileNameWithoutExtension(filePath) + ".";
            string suffix;

            foreach (string path in Directory.GetFiles(Path.GetDirectoryName(filePath)))
            {
                string targetFileName = Path.GetFileNameWithoutExtension(path);
                string targetExtension = Path.GetExtension(path);
                if ((targetFileName.Substring(0, Math.Min(prefix.Length, targetFileName.Length)) == prefix) && (LogFileExtension == targetExtension))
                {
                    suffix = targetFileName.Substring(prefix.Length);
                    if (regex.Match(suffix).Success)
                        yield return path;
                }
            }
        }

        [SetUp]
        public void SetupLoggers()
        {
            _logFilePath = Path.GetTempFileName() + ".log";
            Log.BasicConfigure(_rootLogFileLocation, "{Asctime} : {Category} : {Level} : {Message}", "yyyy-MM-dd", LoggingLevel.INFO);

            Logger logger = Log.GetLogger("padding");
            string logFileLocation = Path.GetTempFileName();
            IHandler handler = new FileHandler(logFileLocation);
            handler.Formatter = new Formatter("{Category,10}");
            logger.AddHandler(handler);
            logger.Level = LoggingLevel.DEBUG;
            loggerPaths["padding"] = logFileLocation;

            GetLogger("exceptions", Path.GetTempFileName());

            GetLogger("format", Path.GetTempFileName());
        }

        [Test]
        public void TestLoggingOutput()
        {
            StreamReader streamReader = GetStreamReader(_rootLogFileLocation);

            Log.Info("Info Test");
            Log.Debug("Debug Test");
            Log.Error("Error Test");

            //string logOutput = File.ReadAllText(logFileLocation);

            string text = streamReader.ReadToEnd();

            Assert.That(text, Is.EqualTo(string.Format("{0} : root : INFO : Info Test{1}{0} : root : ERROR : Error Test{1}", GetLogDatePart, Environment.NewLine)));
        }

        private string ReadWholeFile(string filePath)
        {
            return new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).ReadToEnd();
        }

        private StreamReader GetStreamReader(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader streamReader = new StreamReader(fileStream);
            fileStream.Seek(0, SeekOrigin.End);

            return streamReader;
        }

        private string GetLogDatePart
        {
            get
            {
                DateTime now = DateTime.Now;
                return string.Format("{0:D4}-{1:D2}-{2:D2}", now.Year, now.Month, now.Day);
            }
        }

        private Logger GetLogger(string name, string filePath)
        {
            Logger logger = Log.GetLogger(name);

            IHandler handler = new FileHandler(filePath);
            handler.Formatter = new Formatter("{Asctime} : {Level} : {Message}", "yyyy-MM-dd");
            logger.AddHandler(handler);
            logger.Level = LoggingLevel.DEBUG;
            loggerPaths[name] = filePath;

            return logger;
        }

        [Test]
        public void TestFormatPadding()
        {
            Logger logger = Log.GetLogger("padding");

            StreamReader streamReader = GetStreamReader(loggerPaths["padding"]);

            logger.Info("---");

            string text = streamReader.ReadToEnd();

            Assert.That(text, Is.EqualTo(string.Format("   padding{0}", Environment.NewLine)));
        }

        [Test]
        public void TestLoggingException()
        {
            Logger logger = Log.GetLogger("exceptions");

            StreamReader streamReader = GetStreamReader(_rootLogFileLocation);

            try
            {
                throw new Exception();
            }
            catch (Exception error)
            {
                logger.Exception(LoggingLevel.ERROR, error, "oops");
            }

            string resultText = streamReader.ReadToEnd();
            string expectedText = $"{GetLogDatePart} : exceptions : ERROR : oops{Environment.NewLine}System.Exception: Exception of type 'System.Exception' was thrown.";

            Assert.That(resultText.StartsWith(expectedText), Is.True);
        }

        [Test]
        public void TestStringFormatting()
        {
            Logger logger = Log.GetLogger("format");
            string logFilePath = loggerPaths["format"];

            logger.Info("{0} {1} {2} {3}. {3}. {3}. {4} {5}", "this", "is", "a", "test", 4.5f, new object());

            string text = ReadWholeFile(logFilePath);

            Assert.That(text, Is.EqualTo(string.Format("{0} : INFO : this is a test. test. test. 4.5 System.Object{1}", GetLogDatePart, Environment.NewLine)));
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public void TestSizeRotation(int backupCount)
        {
            Logger logger = Log.GetLogger("rotating! " + backupCount);

            IHandler handler = new RotatingFileHandler(_logFilePath, 10, backupCount);
            handler.Formatter = new Formatter("{message}");
            logger.AddHandler(handler);

            Assert.That(File.Exists(GetRolledOverPath(1)), Is.False);
            logger.Info("9 chars");
            for (int i = 1; i <= backupCount; i++)
            {
                Assert.That(File.Exists(GetRolledOverPath(i)), Is.False);
                logger.Info("9 chars");
                Assert.That(File.Exists(GetRolledOverPath(i)), Is.True);
            }

            Assert.That(File.Exists(GetRolledOverPath(backupCount + 1)), Is.False);
            logger.Info("9 chars");
            Assert.That(File.Exists(GetRolledOverPath(backupCount + 1)), Is.False);
            logger.Info("15 characters");
            Assert.That(File.Exists(GetRolledOverPath(backupCount + 1)), Is.False);
        }

        [Test]
        public void TestLargeMessageRotation()
        {
            Logger logger = Log.GetLogger("rotating!");

            IHandler handler = new RotatingFileHandler(_logFilePath, 5, 3);
            handler.Formatter = new Formatter("{message}");
            logger.AddHandler(handler);

            Assert.That(File.Exists(GetRolledOverPath(1)), Is.False);
            logger.Info("Exceeds maxBytes!!");
            Assert.That(File.Exists(GetRolledOverPath(1)), Is.True);

            Assert.That(File.Exists(GetRolledOverPath(2)), Is.False);
            logger.Info("Exceeds maxBytes!!");
            Assert.That(File.Exists(GetRolledOverPath(2)), Is.True);
        }

        [Test]
        [TestCase(0, ExpectedResult=false)]
        [TestCase(1000, ExpectedResult = false)]
        [TestCase(1500, ExpectedResult = false)]
        [TestCase(2000, ExpectedResult = true)]
        [TestCase(5000, ExpectedResult = true)]
        public bool TestTimedRotation(int sleepDuration)
        {
            Logger logger = Log.GetLogger("timed " + sleepDuration);
            IHandler handler = new TimedRotatingFileHandler(_logFilePath, new TimeSpan(0, 0, 2), 3);
            handler.Formatter = new Formatter("{message}");
            logger.AddHandler(handler);

            Assert.That(File.Exists(GetRolledOverPath(1)), Is.False);
            int i;
            for (i = 0; i < 10; i++)
                logger.Info("test");

            Regex regex = new Regex(@"^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}$");

            Thread.Sleep(sleepDuration);
            logger.Info("rollover!");

            string rolledOverPath = GetTimedRolledOverPaths(_logFilePath, regex).FirstOrDefault();

            if (string.IsNullOrEmpty(rolledOverPath))
                return false;

            string text = ReadWholeFile(rolledOverPath);
            Assert.That(text, Is.EqualTo(string.Join(Environment.NewLine, Enumerable.Repeat("test", i).ToArray()) + Environment.NewLine));
            return true;
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void TestTimedRotationCleanUp(int backupCount)
        {
            Logger logger = Log.GetLogger("timed " + backupCount);
            IHandler handler = new TimedRotatingFileHandler(_logFilePath, new TimeSpan(0, 0, 2), backupCount);
            handler.Formatter = new Formatter("{message}");
            logger.AddHandler(handler);

            Assert.That(File.Exists(_logFilePath), Is.True);

            Regex regex = new Regex(@"^\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}$");

            for (int i = 0; i < backupCount + 3; i++)
            {
                logger.Info("test");
                int currentBackupCount = GetTimedRolledOverPaths(_logFilePath, regex).Count();
                Assert.That(currentBackupCount, Is.EqualTo(Math.Min(i, backupCount)));
                if (i < (backupCount + 3) - 1)
                    Thread.Sleep(2000);
            }
        }

        [Test]
        public void TestHierarchy()
        {
            Logger childLogger = Log.GetLogger("base.blah.blah.child");
            string childLogPath = Path.GetTempFileName();
            IHandler childHandler = new FileHandler(childLogPath);
            childHandler.Formatter = new Formatter("{message}");
            childLogger.AddHandler(childHandler);

            childLogger.Info("child");

            Logger parentLogger = Log.GetLogger("base");
            string parentLogPath = Path.GetTempFileName();
            IHandler parentHandler = new FileHandler(parentLogPath);
            parentHandler.Formatter = new Formatter("{message}");
            parentLogger.AddHandler(parentHandler);

            childLogger.Info("message from child");
            
            string childText = ReadWholeFile(childLogPath);
            string parentText = ReadWholeFile(parentLogPath);

            Assert.That(childText, Is.EqualTo(string.Format("child{0}message from child{0}", Environment.NewLine)));
            Assert.That(parentText, Is.EqualTo(string.Format("message from child{0}", Environment.NewLine)));
        }

        [Test]
        public void TestLoggingLevel()
        {
            Logger childLogger = Log.GetLogger("parent.child");
            string childLogPath = Path.GetTempFileName();
            IHandler childHandler = new FileHandler(childLogPath);
            childHandler.Formatter = new Formatter("{message}");
            childLogger.AddHandler(childHandler);

            Logger parentLogger = Log.GetLogger("parent");

            List<LoggingLevel> levels = new List<LoggingLevel> 
            {
                LoggingLevel.CRITICAL,
                LoggingLevel.ERROR, 
                LoggingLevel.WARNING, 
                LoggingLevel.INFO, 
                LoggingLevel.DEBUG, 
                LoggingLevel.NOTSET,
            };

            string expectedMessage = "";

            int id = 0;

            foreach (LoggingLevel level_ in levels)
            {
                if (level_ != LoggingLevel.NOTSET)
                {
                    parentLogger.Level = level_;
                    foreach (LoggingLevel level in levels)
                    {
                        childLogger.Message(level, "{0} {1}", id, level.ToString());
                        if (level >= level_)
                            expectedMessage += id + " " + level.ToString() + Environment.NewLine;
                    }
                }
            }

            string output = ReadWholeFile(childLogPath);

            Assert.That(output, Is.EqualTo(expectedMessage));
        }

        [Test]
        public void TestPropagate()
        {
            Logger childLogger = Log.GetLogger("top.bottom");

            Logger parentLogger = Log.GetLogger("top");
            string parentLogPath = Path.GetTempFileName();
            IHandler parentHandler = new FileHandler(parentLogPath);
            parentHandler.Formatter = new Formatter("{message}");
            parentLogger.AddHandler(parentHandler);

            childLogger.Info("on");
            childLogger.Propagate = false;
            childLogger.Info("off");

            string output = ReadWholeFile(parentLogPath);

            Assert.That(output, Is.EqualTo(("on" + Environment.NewLine)));
        }

        [Test]
        public void TestJsonFormatter()
        {
            var logger = Log.GetLogger("json");
            IHandler handler = new FileHandler(_logFilePath);
            handler.Formatter = new JsonFormatter();
            logger.AddHandler(handler);

            string message = "An Error Occurred";
            logger.Error(message);

            var outputString = ReadWholeFile(_logFilePath);

            var outputDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(outputString)!;
            
            Assert.That(outputDict["Message"], Is.EqualTo(message));
            Assert.That(outputDict["LevelName"], Is.EqualTo("ERROR"));
            Assert.That(outputDict["Category"], Is.EqualTo("json"));
        }

        [Test]
        public void TestExtraFormatting()
        {
            var logger = Log.GetLogger("extra");
            IHandler handler = new FileHandler(_logFilePath);
            handler.Formatter = new Formatter(@"{extra[""rxtx""]} : {message}");
            logger.AddHandler(handler);

            StreamReader streamReader = GetStreamReader(_logFilePath);

            logger.Extra(LoggingLevel.CRITICAL, new Dictionary<string, object>() { { "rxtx", "<<"} }, "incoming!");
            Assert.That(streamReader.ReadLine(), Is.EqualTo("<< : incoming!"));

            logger.Extra(LoggingLevel.CRITICAL, new Dictionary<string, object>() { { "rxtx", ">>"} }, "outgoing!");
            Assert.That(streamReader.ReadLine(), Is.EqualTo(">> : outgoing!"));
        }

        [Test]
        public void TestNamePropagates()
        {
            var logger = Log.GetLogger("naming");

            var streamReader = GetStreamReader(_rootLogFileLocation);
            logger.Error("Woah!");
            var output = streamReader.ReadLine();

            Assert.That(output, Is.EqualTo(string.Format("{0} : naming : ERROR : Woah!", GetLogDatePart)));
        }
    }
}
