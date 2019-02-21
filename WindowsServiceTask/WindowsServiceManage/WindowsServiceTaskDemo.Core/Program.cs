using System;

namespace WindowsServiceTaskDemo.Core
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
