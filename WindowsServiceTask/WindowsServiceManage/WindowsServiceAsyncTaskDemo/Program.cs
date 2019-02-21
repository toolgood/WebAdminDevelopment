using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsServiceTaskDemo.Codes;

namespace WindowsServiceAsyncTaskDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationConstant.WIN_SERVICE_LOG_NAME = "WindowsServiceTaskDemo";
            ConfigurationConstant.WIN_SERVICE_NAME = "WindowsServiceTaskDemo";
            ConfigurationConstant.WIN_DISPLAY_NAME = "同步定时任务";
            ConfigurationConstant.WIN_DESCRIPTION = "WindowsServiceTaskDemo";

            QuartzHelper.RunArgs(args);
        }
    }
}
