using System;
using System.Collections.Generic;
using System.Text;

namespace ToolGood.Common.Utils.Loggers
{
    public interface ILogger
    {
        bool UseDebug { get; set; }

        bool UseInfo { get; set; }

        bool UseWarn { get; set; }

        bool UseError { get; set; }

        bool UseFatal { get; set; }

        void Debug(string msg);

        void Info(string msg);

        void Warn(string msg);

        void Error(string msg);

        void Fatal(string msg);

        void WriteLog(string type, string msg);


    }
}
