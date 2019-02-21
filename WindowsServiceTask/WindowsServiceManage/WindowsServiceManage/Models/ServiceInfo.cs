using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WindowsServiceManage.Models
{
    public class ServiceInfo
    {
        public string Name { get; set; }

        public string NameCn { get; set; }

        public string Path { get; set; }

        public bool IsRun { get; set; }

        public List<JobInfo> JobInfos { get; set; }

    }
}