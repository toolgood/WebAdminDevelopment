using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WindowsServiceManage.Codes;
using WindowsServiceManage.Models;

namespace WindowsServiceManage.Controllers
{
    public class ServiceController : Controller
    {
        // GET: Service
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAllServiceInfos()
        {
            var list = QuartzHelper.GetServices();
            List<ServiceInfo> infos = new List<ServiceInfo>();
            foreach (var item in list) {
                try {
                    infos.Add(QuartzHelper.GetServiceInfo(item, true, true));
                } catch (Exception) { }
            }
            return Json(new { code = 1, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), items = infos }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetServiceInfos(string serviceNames)
        {
            try {
                var sns = serviceNames.Split(",".ToArray(), StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
                List<ServiceInfo> infos = new List<ServiceInfo>();
                foreach (var item in sns) {
                    infos.Add(QuartzHelper.GetServiceInfo(item, true, true));
                }
                return Json(new { code = 1, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), items = infos }, JsonRequestBehavior.AllowGet);
            } catch (Exception ex) {
                return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"执行出错：" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult RunJob(string serviceName, string jobName)
        {
            try {
                var info = QuartzHelper.GetServiceInfo(serviceName, true);
                if (info == null) {
                    return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"服务【{serviceName}】不存在!" }, JsonRequestBehavior.AllowGet);
                }
                var r = QuartzHelper.GetJob(info.Path, jobName);
                if (r == 0) {
                    return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"任务【{jobName}】不存在!" }, JsonRequestBehavior.AllowGet);
                }
                if (r == 2 && info.IsRun) {
                    return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"任务【{jobName}】将被执行或正在执行!" }, JsonRequestBehavior.AllowGet);
                }
                Process.Start(info.Path, jobName);
                return Json(new { code = 1, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"任务【{jobName}】开始执行!" }, JsonRequestBehavior.AllowGet);

            } catch (Exception ex) {
                return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"任务【{jobName}】执行出错：" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult StartServer(string serviceName)
        {
            try {
                var info = QuartzHelper.GetServiceInfo(serviceName);
                if (info == null) {
                    return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"服务【{serviceName}】不存在!" }, JsonRequestBehavior.AllowGet);
                }
                Process.Start(info.Path, "start");
                return Json(new { code = 1, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"服务【{info.NameCn}】开始执行!" }, JsonRequestBehavior.AllowGet);

            } catch (Exception ex) {
                return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"服务【{serviceName}】执行出错：" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult StopServer(string serviceName)
        {
            try {
                var info = QuartzHelper.GetServiceInfo(serviceName);
                if (info == null) {
                    return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"服务【{serviceName}】不存在!" }, JsonRequestBehavior.AllowGet);
                }
                Process.Start(info.Path, "stop");
                return Json(new { code = 1, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"服务【{info.NameCn}】结束执行!" }, JsonRequestBehavior.AllowGet);

            } catch (Exception ex) {
                return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = $"服务【{serviceName}】执行出错：" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult Edit(string serviceName)
        {
            return View();
        }

        [HttpGet]
        public ActionResult EditJob(string serviceName, string jobName, string triggerGroup, string triggerName)
        {
            ViewBag.serviceName = serviceName;
            ViewBag.jobName = jobName;
            ViewBag.triggerGroup = triggerGroup;
            ViewBag.triggerName = triggerName;

            var service = QuartzHelper.GetServiceInfo(serviceName, false, true);
            var job = service.JobInfos.First(q => q.Name == jobName && q.TriggerGroup == triggerGroup && q.TriggerName == triggerName);
            ViewBag.job = job;

            return View();
        }

        [HttpPost]
        public ActionResult SaveJob(string serviceName, string jobName, string description, string triggerGroup, string triggerName, string cron)
        {
            string msg;
            if (QuartzHelper.SaveJob(serviceName, jobName, description, triggerGroup, triggerName, cron,out msg)) {
                return Json(new { code = 1, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = msg }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = msg }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult GetCronExpression(string cronExpression)
        {
            cronExpression = cronExpression.Trim();
            if (CronExpression.IsValidExpression(cronExpression) == false) {

            } else {
                try {
                    var cron = CronExpressionDescriptor.ExpressionDescriptor.GetDescription(cronExpression, new CronExpressionDescriptor.Options() {
                        DayOfWeekStartIndexZero = false,
                        Use24HourTimeFormat = true,
                    });
                    return Json(new { code = 1, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = cron }, JsonRequestBehavior.AllowGet);
                } catch (Exception) { }
            }
            return Json(new { code = 0, date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg = "Cron表达式错误" }, JsonRequestBehavior.AllowGet);
        }


    }
}