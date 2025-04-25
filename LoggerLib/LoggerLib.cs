using System;
using System.IO;
using System.Text;

namespace KimCGTools
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    public enum LogPeriod
    {
        Daily,
        Weekly,
        Monthly
    }

    public class LoggerLib
    {
        private static LoggerLib? _instance;
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private readonly LogPeriod _logPeriod;
        private readonly string _postfix;

        private LoggerLib(LogPeriod pPeriod = LogPeriod.Daily, string pPostfix = "")
        {
            _logPeriod = pPeriod;
            _postfix = pPostfix;
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            _logFilePath = GetLogFilePath(logDirectory);
        }

        private string GetLogFilePath(string pLogDirectory)
        {
            string fileName;
            string yearFolder = Path.Combine(pLogDirectory, DateTime.Now.Year.ToString());
            string monthFolder = Path.Combine(yearFolder, DateTime.Now.Month.ToString("00"));

            string postfix = string.IsNullOrEmpty(_postfix) ? "" : $"_{_postfix}";

            switch (_logPeriod)
            {
                case LogPeriod.Weekly:
                    int weekNumber = (DateTime.Now.DayOfYear - 1) / 7 + 1;
                    fileName = $"log_week{weekNumber}{postfix}.txt";
                    if (!Directory.Exists(monthFolder))
                    {
                        Directory.CreateDirectory(monthFolder);
                    }
                    return Path.Combine(monthFolder, fileName);
                case LogPeriod.Monthly:
                    fileName = $"log_{DateTime.Now:yyyyMM}{postfix}.txt";
                    if (!Directory.Exists(yearFolder))
                    {
                        Directory.CreateDirectory(yearFolder);
                    }
                    return Path.Combine(yearFolder, fileName);
                case LogPeriod.Daily:
                default:
                    fileName = $"log_{DateTime.Now:yyyyMMdd}{postfix}.txt";
                    if (!Directory.Exists(monthFolder))
                    {
                        Directory.CreateDirectory(monthFolder);
                    }
                    return Path.Combine(monthFolder, fileName);
            }
        }

        public static LoggerLib Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LoggerLib();
                }
                return _instance;
            }
        }

        public static LoggerLib GetInstance(LogPeriod pPeriod, string pPostfix = "")
        {
            if (_instance == null || _instance._logPeriod != pPeriod || _instance._postfix != pPostfix)
            {
                _instance = new LoggerLib(pPeriod, pPostfix);
            }
            return _instance;
        }

        public void Log(LogLevel pLevel, string pMessage, Exception? pException = null)
        {
            var logMessage = new StringBuilder();
            logMessage.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{pLevel}] {pMessage}");
            
            if (pException != null)
            {
                logMessage.AppendLine($"Exception: {pException.Message}");
                logMessage.AppendLine($"Stack Trace: {pException.StackTrace}");
            }

            WriteToFile(logMessage.ToString());
            WriteToConsole(logMessage.ToString());
        }

        private void WriteToFile(string pMessage)
        {
            try
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, pMessage);
                }
            }
            catch (Exception pEx)
            {
                Console.WriteLine($"로그 파일 작성 중 오류 발생: {pEx.Message}");
            }
        }

        private void WriteToConsole(string pMessage)
        {
            Console.WriteLine(pMessage);
        }

        // 편의 메서드들
        public void Debug(string pMessage) => Log(LogLevel.Debug, pMessage);
        public void Info(string pMessage) => Log(LogLevel.Info, pMessage);
        public void Warning(string pMessage) => Log(LogLevel.Warning, pMessage);
        public void Error(string pMessage, Exception? pException = null) => Log(LogLevel.Error, pMessage, pException);
        public void Fatal(string pMessage, Exception? pException = null) => Log(LogLevel.Fatal, pMessage, pException);
    }
} 