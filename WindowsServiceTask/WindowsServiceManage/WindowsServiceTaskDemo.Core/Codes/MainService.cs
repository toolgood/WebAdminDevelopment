using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolGood.Common;

namespace WindowsServiceTaskDemo.Codes
{
    public class MainService
    {
        private IScheduler scheduler = null;

        public MainService()
        {
            try {
                //新建一个调度器工工厂
                ISchedulerFactory factory = new StdSchedulerFactory();
                //使用工厂生成一个调度器
                scheduler = factory.GetScheduler().RunSync();
                LogUtil.Info("定时任务初始化成功", ConfigurationConstant.WIN_SERVICE_LOG_NAME);
                Console.WriteLine("MainService构造成功");
            } catch (Exception ex) {
                LogUtil.Error(ex, "MainService()", ConfigurationConstant.WIN_SERVICE_LOG_NAME);
            }
        }

        public void OnStart()
        {
            try {
                LogUtil.Info(string.Format("Windows服务 {0} 正在启动...", ConfigurationConstant.WIN_SERVICE_NAME));
                scheduler.Start();
                try {
                    Console.WriteLine("----------- 任务信息 START -------------");
                    foreach (var groupName in scheduler.GetJobGroupNames().RunSync()) {
                        foreach (JobKey jobKey in scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(groupName)).RunSync()) {
                            String jobName = jobKey.Name;
                            String jobGroup = jobKey.Group;
                            List<ITrigger> triggers = (List<ITrigger>)scheduler.GetTriggersOfJob(jobKey).RunSync();
                            var date = triggers[0].GetFireTimeAfter(DateTimeOffset.Now);
                            Console.WriteLine($"[{jobGroup}-{jobName}]下次执行时间: {date?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")}");
                            LogUtil.Info($"[{jobGroup}-{jobName}]下次执行时间: {date?.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss")}", ConfigurationConstant.WIN_SERVICE_LOG_NAME);
                        }
                    }
                    Console.WriteLine("----------- 任务信息 END -------------");
                } catch (Exception) { }
                LogUtil.Info(string.Format("Windows服务 {0} 已经启动", ConfigurationConstant.WIN_SERVICE_NAME));
            } catch (Exception ex) {
                LogUtil.Error(ex, "MainService-OnStart", ConfigurationConstant.WIN_SERVICE_NAME);
            }
        }

        public void OnStop()
        {
            try {
                LogUtil.Info(string.Format("Windows服务 {0} 正在停止...", ConfigurationConstant.WIN_SERVICE_NAME));
                scheduler.Shutdown(false);
                LogUtil.Info(string.Format("Windows服务 {0} 已经停止", ConfigurationConstant.WIN_SERVICE_NAME));
            } catch (Exception ex) {
                LogUtil.Error(ex, "MainService-OnStop", ConfigurationConstant.WIN_SERVICE_NAME);
            }
        }

        public void OnPause()
        {
            try {
                LogUtil.Info(string.Format("Windows服务 {0} 正在暂停...", ConfigurationConstant.WIN_SERVICE_NAME));
                scheduler.PauseAll();
                LogUtil.Info(string.Format("Windows服务 {0} 已经暂停", ConfigurationConstant.WIN_SERVICE_NAME));
            } catch (Exception ex) {
                LogUtil.Error(ex, "MainService-OnPause", ConfigurationConstant.WIN_SERVICE_NAME);
            }
        }

        public void OnContinue()
        {
            try {
                LogUtil.Info(string.Format("Windows服务 {0} 准备继续执行...", ConfigurationConstant.WIN_SERVICE_NAME));
                scheduler.ResumeAll();
                LogUtil.Info(string.Format("Windows服务 {0} 继续执行", ConfigurationConstant.WIN_SERVICE_NAME));
            } catch (Exception ex) {
                LogUtil.Error(ex, "MainService-OnContinue", ConfigurationConstant.WIN_SERVICE_NAME);
            }
        }
    }

}
