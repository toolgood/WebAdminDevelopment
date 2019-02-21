using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ToolGood.Common.Utils.Loggers;

namespace ToolGood.Common
{
    public class LogUtil
    {
        private static ConcurrentDictionary<string, List<ILogger>> allLoggers = new ConcurrentDictionary<string, List<ILogger>>();

        public static bool UseDebug { get; set; } = true;

        public static bool UseInfo { get; set; } = true;

        public static bool UseWarn { get; set; } = true;

        public static bool UseError { get; set; } = true;

        public static bool UseFatal { get; set; } = true;

        private const string _log_ = "_ToolGood.Common.LogUtil_";


        public static void WriteLog(string type, string msg, string loggerName = null)
        {
            if (UseDebug) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        logger.WriteLog(type, msg);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        logger.WriteLog(type, msg);
                    }
                }
        
            }
        }


        public static void Debug(string msg, string loggerName = null)
        {
            if (UseDebug) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        logger.Debug(msg);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        logger.Debug(msg);
                    }
                }
            }
        }

        public static void Info(string msg, string loggerName = null)
        {
            if (UseInfo) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        logger.Info(msg);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        logger.Info(msg);
                    }
                }
                
            }
        }

        public static void Warn(string msg, string loggerName = null)
        {
            if (UseWarn) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        logger.Info(msg);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        logger.Info(msg);
                    }
                }
                
            }
        }
        public static void Warn(Exception ex, string memberName = null, string loggerName = null)
        {
            if (UseWarn) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        //logger.Warn(msg, memberName);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        //logger.Warn(msg, memberName);
                    }
                }
               
            }
        }

        public static void Error(string msg, string loggerName = null)
        {
            if (UseError) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        logger.Error(msg);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        logger.Error(msg);
                    }
                }
               
            }
        }
        public static void Error(Exception ex, string memberName = null, string loggerName = null)
        {
            if (UseError) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        //logger.Warn(msg, memberName);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        //logger.Warn(msg, memberName);
                    }
                }
                
            }
        }

        public static void Fatal(string msg, string loggerName = null)
        {
            if (UseFatal) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        logger.Fatal(msg);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        logger.Fatal(msg);
                    }
                }
              
            }
        }
        public static void Fatal(Exception ex, string memberName = null, string loggerName = null)
        {
            if (UseFatal) {
                if (string.IsNullOrEmpty(loggerName) == false) {
                    foreach (var logger in allLoggers[loggerName]) {
                        //logger.Fatal(msg);
                    }
                }
                if (allLoggers.ContainsKey(_log_)) {
                    foreach (var logger in allLoggers[_log_]) {
                        //logger.Fatal(msg);
                    }
                }
               
            }
        }

    }
}
