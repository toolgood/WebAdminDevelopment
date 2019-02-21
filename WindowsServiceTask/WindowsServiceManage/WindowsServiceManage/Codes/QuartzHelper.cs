using Quartz;
using Quartz.Spi;
using Quartz.Xml.JobSchedulingData20;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using WindowsServiceManage.Models;

namespace WindowsServiceManage.Codes
{
    public static class QuartzHelper
    {


        public static bool SaveJob(string serviceName, string jobName, string description, string triggerGroup, string triggerName, string cron, out string msg)
        {
            //if (Session[SessionAdmin] == null) return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = "请登录！" }, JsonRequestBehavior.AllowGet);
            description = description.Trim();
            if (string.IsNullOrWhiteSpace(description)) {
                msg = "Job名称不能为空";
                return false;
            }
            cron = cron.Trim();
            #region 判断Cron表达式
            if (CronExpression.IsValidExpression(cron) == false) {
                msg = "Cron表达式错误";
                return false;

            } else {
                try {
                    CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cron, new CronExpressionDescriptor.Options() {
                        DayOfWeekStartIndexZero = false,
                        Use24HourTimeFormat = true,
                    });
                    GetTaskeFireTime(cron, 1);
                } catch (Exception) {
                    msg = "Cron表达式错误";
                    return false;
                }
            }
            #endregion


            #region 服务判断
            var info = GetServiceInfo(serviceName, false, false);
            if (info == null) {
                msg = $"服务【{serviceName}】不存在!";
                return false;
            }
            #endregion
            #region 文件判断
            var xmlFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(info.Path), "quartz_jobs.xml");
            if (System.IO.File.Exists(xmlFile) == false) {
                msg = $"服务【{serviceName}】配置文件不存在!";
                return false;
            }
            #endregion
            #region 保存数据
            var xml = System.IO.File.ReadAllText(xmlFile);
            XmlSerializer xs = new XmlSerializer(typeof(QuartzXmlConfiguration20));
            QuartzXmlConfiguration20 data = (QuartzXmlConfiguration20)xs.Deserialize(new StringReader(xml));
            if (data.schedule != null) {
                foreach (jobschedulingdataSchedule schedule in data.schedule) {
                    if (((schedule != null) ? schedule.job : null) != null) {
                        foreach (var item in schedule.job) {
                            if (item.name == jobName) {
                                item.description = description;
                            }
                        }
                    }
                }
                foreach (jobschedulingdataSchedule schedule2 in data.schedule) {
                    if (schedule2 != null && schedule2.trigger != null) {
                        foreach (var item in schedule2.trigger) {
                            cronTriggerType cronTrigger = item.Item as cronTriggerType;
                            if (cronTrigger == null) continue;
                            if (cronTrigger.group == triggerGroup && cronTrigger.name == triggerName) {
                                cronTrigger.cronexpression = cron;
                            }
                        }
                    }
                }
            }
            #endregion
            #region 保存文件
            xml = Serializer(typeof(QuartzXmlConfiguration20), data);
            System.IO.File.Copy(xmlFile, xmlFile + DateTime.Now.ToString("_yyyyMMdd_HHmmss") + ".bak", true);
            System.IO.File.WriteAllText(xmlFile, xml);
            #endregion
            msg = $"操作成功，请注意重启！";
            return true;
        }


        private static string Serializer(Type type, object obj)
        {
            MemoryStream Stream = new MemoryStream();
            XmlSerializer xml = new XmlSerializer(type);
            //序列化对象  
            xml.Serialize(Stream, obj);
            Stream.Position = 0;
            StreamReader sr = new StreamReader(Stream);
            string str = sr.ReadToEnd();

            sr.Dispose();
            Stream.Dispose();

            return str;
        }

        public static List<string> GetServices()
        {
            var app = ConfigurationManager.AppSettings;
            var value = app.Get("windowsTasks");
            return value.Split('|').ToList();
        }

        public static int GetJob(string file, string jobName)
        {
            if (System.IO.File.Exists(file) == false) {
                return 0;
            }
            var xmlFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), "quartz_jobs.xml");
            if (System.IO.File.Exists(xmlFile) == false) {
                return 0;
            }
            var xml = System.IO.File.ReadAllText(xmlFile);
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
            var node = jobNodes.FirstOrDefault(q => q.name.ToLower() == jobName.ToLower());
            if (node == null) { return 0; }

            List<triggerType> triggerEntries = new List<triggerType>();
            if (data.schedule != null) {
                foreach (jobschedulingdataSchedule schedule2 in data.schedule) {
                    if (schedule2 != null && schedule2.trigger != null) {
                        triggerEntries.AddRange(schedule2.trigger);
                    }
                }
            }

            var list = triggerEntries.Where(q => q.Item.jobgroup == node.group && q.Item.jobname == node.name).ToList();
            foreach (var trigger in list) {
                if (trigger.Item is cronTriggerType) {
                    cronTriggerType cronTrigger = (cronTriggerType)trigger.Item;
                    string cronExpression = cronTrigger.cronexpression.Trim();
                    if (CronExpression.IsValidExpression(cronExpression) == false) {
                    } else {
                        if (RunJobLately(cronExpression)) {
                            return 2;
                        }
                    }
                }
            }

            return 1;
        }

        public static List<JobInfo> GetJobInfos(string file)
        {
            List<JobInfo> jobInfos = new List<JobInfo>();
            if (System.IO.File.Exists(file) == false) { return jobInfos; }
            var xmlFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), "quartz_jobs.xml");
            if (System.IO.File.Exists(xmlFile) == false) {
                return jobInfos;
            }
            var xml = System.IO.File.ReadAllText(xmlFile);
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
            List<triggerType> triggerEntries = new List<triggerType>();
            if (data.schedule != null) {
                foreach (jobschedulingdataSchedule schedule2 in data.schedule) {
                    if (schedule2 != null && schedule2.trigger != null) {
                        triggerEntries.AddRange(schedule2.trigger);
                    }
                }
            }

            foreach (var item in jobNodes) {
                var list = triggerEntries.Where(q => q.Item.jobgroup == item.group && q.Item.jobname == item.name).ToList();
                foreach (var trigger in list) {
                    if (trigger.Item is cronTriggerType) {
                        cronTriggerType cronTrigger = (cronTriggerType)trigger.Item;
                        string cronExpression = cronTrigger.cronexpression.Trim();
                        if (CronExpression.IsValidExpression(cronExpression) == false) {
                        } else {
                            var cron = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cronExpression, new CronExpressionDescriptor.Options() {
                                DayOfWeekStartIndexZero = false,
                                Use24HourTimeFormat = true,
                            });
                            jobInfos.Add(new JobInfo() {
                                Name = item.name,
                                TriggerName = cronTrigger.name,
                                TriggerGroup = cronTrigger.group,
                                Description = item.description,
                                CronExpressionDescriptor = cron,
                                Cron = cronExpression,
                                NextStartTime = GetTaskeFireTime(cronExpression, 1)[0]
                            });
                        }
                    }
                }
            }
            return jobInfos;
        }


        public static ServiceInfo GetServiceInfo(string serviceName, bool getRunInfo = false, bool getJobInfos = false)
        {
            var app = ConfigurationManager.AppSettings;
            var value = app.Get(serviceName);
            if (string.IsNullOrEmpty(value)) return null;

            var sp = value.Split('|');
            ServiceInfo serviceInfo = new ServiceInfo();
            serviceInfo.Name = serviceName;
            serviceInfo.NameCn = sp[0].Trim();
            serviceInfo.Path = sp[1].Trim();
            if (string.IsNullOrEmpty(serviceInfo.Path)) return null;

            if (getRunInfo) {
                if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(serviceInfo.Path)).Count() > 0) {
                    serviceInfo.IsRun = true;
                } else {
                    var ps = Process.GetProcesses();
                    foreach (var p in ps) {
                        try {
                            if (p.ProcessName.ToLower() == Path.GetFileNameWithoutExtension(serviceInfo.Path.ToLower())) {
                                serviceInfo.IsRun = true;
                                break;
                            }
                        } catch (Exception) { }
                    }
                }
            }
            if (getJobInfos) {
                try {
                    serviceInfo.JobInfos = GetJobInfos(serviceInfo.Path);
                } catch (Exception) {
                    serviceInfo.JobInfos = new List<JobInfo>();
                }
            }
            return serviceInfo;
        }


        /// <summary>
        /// 获取任务在未来周期内哪些时间会运行
        /// </summary>
        /// <param name="CronExpressionString">Cron表达式</param>
        /// <param name="numTimes">运行次数</param>
        /// <returns>运行时间段</returns>
        public static List<string> GetTaskeFireTime(string CronExpressionString, int numTimes)
        {
            if (numTimes < 0) {
                throw new Exception("参数numTimes值大于等于0");
            }
            //时间表达式
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(CronExpressionString).Build();
            IList<DateTimeOffset> dates = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, numTimes);
            List<string> list = new List<string>();
            foreach (DateTimeOffset dtf in dates) {
                list.Add(TimeZoneInfo.ConvertTimeFromUtc(dtf.DateTime, TimeZoneInfo.Local).ToString());
            }
            return list;
        }

        /// <summary>
        /// 最近要执行
        /// </summary>
        /// <param name="CronExpressionString"></param>
        /// <returns></returns>
        public static bool RunJobLately(string CronExpressionString)
        {
            ITrigger trigger = TriggerBuilder.Create().WithCronSchedule(CronExpressionString).Build();
            IList<DateTimeOffset> dates = TriggerUtils.ComputeFireTimesBetween(trigger as IOperableTrigger, null,
                DateTimeOffset.Now.AddSeconds(-1), DateTimeOffset.Now.AddSeconds(1));
            return dates.Count > 0;
        }
    }
}