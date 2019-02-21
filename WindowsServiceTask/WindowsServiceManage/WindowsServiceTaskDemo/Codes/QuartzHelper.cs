using Quartz;
using Quartz.Xml.JobSchedulingData20;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ToolGood.Common;
using Topshelf;

namespace WindowsServiceTaskDemo.Codes
{
    public static class QuartzHelper
    {
        public static void RunArgs(string[] args)
        {
            if (RunArgs2(args)) { return; }

            ConfigUtil.StartWatch();
            HostFactory.Run(x => {
                x.RunAsLocalSystem();

                x.SetServiceName(ConfigurationConstant.WIN_SERVICE_NAME);
                x.SetDisplayName(ConfigurationConstant.WIN_DISPLAY_NAME);
                x.SetDescription(ConfigurationConstant.WIN_SERVICE_NAME);



                x.Service<MainService>(sc => {
                    sc.ConstructUsing(() => new MainService());
                    sc.WhenStarted(a => a.OnStart());
                    sc.WhenStopped(a => a.OnStop());
                    sc.WhenContinued(a => a.OnContinue());
                    sc.WhenPaused(a => a.OnPause());
                });
            });
        }



        public static bool RunArgs2(string[] args)
        {
            var dir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var file = Path.Combine(dir, "Configs", "quartz_jobs.xml");
            if (File.Exists(file) == false) {
                file = Path.Combine(dir, "quartz_jobs.xml");
            }

            if (args.Length == 0) {
                TestJob(file);
                return false;
            }
            if (args.Length == 1) {
                var str = args[0];
                if (str == "CreditApplyMessageTask") { TestJob(file); return true; }
                if ((new string[] { "install", "uninstall", "start", "stop", "continue", "pause" }).Contains(str.ToLower()) == false) {
                    var job = GetJob(file, str);
                    if (job != null) {
                        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]【{str}】任务开始执行...");
                        job.Execute(null);
                        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]【{str}】任务已结束！");
                    }
                    return true;
                }
            }
            return false;
        }
        private static void TestJob(string file)
        {
            Console.WriteLine("----------- 检测文件配置 START -------------");

            #region 加载文件信息
            var xml = File.ReadAllText(file);
            XmlSerializer xs = new XmlSerializer(typeof(QuartzXmlConfiguration20));
            QuartzXmlConfiguration20 data = (QuartzXmlConfiguration20)xs.Deserialize(new StringReader(xml));

            List<jobdetailType> jobNodes = new List<jobdetailType>();
            List<cronTriggerType> triggerTypes = new List<cronTriggerType>();
            if (data.schedule != null) {
                foreach (jobschedulingdataSchedule schedule in data.schedule) {
                    if (schedule?.job != null) {
                        jobNodes.AddRange(schedule.job);
                    }
                }
                foreach (jobschedulingdataSchedule schedule2 in data.schedule) {
                    if (schedule2?.trigger != null) {
                        foreach (var item in schedule2.trigger) {
                            cronTriggerType cronTrigger = item.Item as cronTriggerType;
                            if (cronTrigger == null) continue;
                            triggerTypes.Add(cronTrigger);
                        }
                    }
                }
            }
            #endregion

            #region 检测 jobtype
            if (jobNodes.Count == 0) {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 没有可执行任务！");
            } else {
                foreach (var node in jobNodes) {
                    var type = Type.GetType(node.jobtype);
                    if (type == null) {
                        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]jobtype【{node.jobtype}】出错了...");
                    } else {
                        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]job【{node.name}】检测成功...");
                    }
                }
            }
            #endregion

            #region 检测 trigger
            if (triggerTypes.Count == 0) {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 没有配置trigger！");
            } else {
                foreach (var node in triggerTypes) {
                    const string reg = @"^(((([0-9]|[0-5][0-9])(-([0-9]|[0-5][0-9]))?,)*([0-9]|[0-5][0-9])(-([0-9]|[0-5][0-9]))?)|(([\*]|[0-9]|[0-5][0-9])/([0-9]|[0-5][0-9]))|([\?])|([\*]))[\s](((([0-9]|[0-5][0-9])(-([0-9]|[0-5][0-9]))?,)*([0-9]|[0-5][0-9])(-([0-9]|[0-5][0-9]))?)|(([\*]|[0-9]|[0-5][0-9])/([0-9]|[0-5][0-9]))|([\?])|([\*]))[\s](((([0-9]|[0-1][0-9]|[2][0-3])(-([0-9]|[0-1][0-9]|[2][0-3]))?,)*([0-9]|[0-1][0-9]|[2][0-3])(-([0-9]|[0-1][0-9]|[2][0-3]))?)|(([\*]|[0-9]|[0-1][0-9]|[2][0-3])/([0-9]|[0-1][0-9]|[2][0-3]))|([\?])|([\*]))[\s](((([1-9]|[0][1-9]|[1-2][0-9]|[3][0-1])(-([1-9]|[0][1-9]|[1-2][0-9]|[3][0-1]))?,)*([1-9]|[0][1-9]|[1-2][0-9]|[3][0-1])(-([1-9]|[0][1-9]|[1-2][0-9]|[3][0-1]))?(C)?)|(([1-9]|[0][1-9]|[1-2][0-9]|[3][0-1])/([1-9]|[0][1-9]|[1-2][0-9]|[3][0-1])(C)?)|(L(-[0-9])?)|(L(-[1-2][0-9])?)|(L(-[3][0-1])?)|(LW)|([1-9]W)|([1-3][0-9]W)|([\?])|([\*]))[\s](((([1-9]|0[1-9]|1[0-2])(-([1-9]|0[1-9]|1[0-2]))?,)*([1-9]|0[1-9]|1[0-2])(-([1-9]|0[1-9]|1[0-2]))?)|(([1-9]|0[1-9]|1[0-2])/([1-9]|0[1-9]|1[0-2]))|(((JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)(-(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC))?,)*(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)(-(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC))?)|((JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC)/(JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC))|([\?])|([\*]))[\s]((([1-7](-([1-7]))?,)*([1-7])(-([1-7]))?)|([1-7]/([1-7]))|(((MON|TUE|WED|THU|FRI|SAT|SUN)(-(MON|TUE|WED|THU|FRI|SAT|SUN))?,)*(MON|TUE|WED|THU|FRI|SAT|SUN)(-(MON|TUE|WED|THU|FRI|SAT|SUN))?(C)?)|((MON|TUE|WED|THU|FRI|SAT|SUN)/(MON|TUE|WED|THU|FRI|SAT|SUN)(C)?)|(([1-7]|(MON|TUE|WED|THU|FRI|SAT|SUN))?(L|LW)?)|(([1-7]|MON|TUE|WED|THU|FRI|SAT|SUN)#([1-7])?)|([\?])|([\*]))([\s]?(([\*])?|(19[7-9][0-9])|(20[0-9][0-9]))?| (((19[7-9][0-9])|(20[0-9][0-9]))/((19[7-9][0-9])|(20[0-9][0-9])))?| ((((19[7-9][0-9])|(20[0-9][0-9]))(-((19[7-9][0-9])|(20[0-9][0-9])))?,)*((19[7-9][0-9])|(20[0-9][0-9]))(-((19[7-9][0-9])|(20[0-9][0-9])))?)?)$";
                    var m = Regex.Match(node.cronexpression, reg);
                    if (m.Success) {
                        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]trigger【{node.name}】检测成功...");
                    } else {
                        Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]trigger【{node.name}】出错了...");
                    }
                }
            }
            #endregion

            Console.WriteLine("----------- 检测文件配置 END -------------");
        }
        private static IJob GetJob(string file, string jobName)
        {
            #region 加载文件信息
            var xml = File.ReadAllText(file);
            XmlSerializer xs = new XmlSerializer(typeof(QuartzXmlConfiguration20));
            QuartzXmlConfiguration20 data = (QuartzXmlConfiguration20)xs.Deserialize(new StringReader(xml));

            List<jobdetailType> jobNodes = new List<jobdetailType>();
            if (data.schedule != null) {
                foreach (jobschedulingdataSchedule schedule in data.schedule) {
                    if (((schedule != null) ? schedule.job : null) != null) {
                        jobNodes.AddRange(schedule.job);
                    }
                }
            }
            #endregion

            var node = jobNodes.FirstOrDefault(q => q.name.ToLower() == jobName.ToLower());
            if (node == null) {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]【{jobName}】任务不存在...");
                return null;
            }
            var type = Type.GetType(node.jobtype);
            if (type == null) {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]【{jobName}】任务jobtype【{node.jobtype}】不存在...");
                return null;
            }
            return Activator.CreateInstance(Type.GetType(node.jobtype)) as IJob;
        }


    }
}
