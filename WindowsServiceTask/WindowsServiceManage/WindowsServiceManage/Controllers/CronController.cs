﻿using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WindowsServiceManage.Controllers
{
    public class CronController : Controller
    {
        // GET: Cron
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CalcRunTime(string cronExpression)
        {
            try {
                List<string> date = GetTaskeFireTime(cronExpression, 70);
                return Json(date, JsonRequestBehavior.AllowGet);
            } catch (Exception) {
                List<string> date = new List<string>();
                return Json(date, JsonRequestBehavior.AllowGet);
            }
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

    }
}