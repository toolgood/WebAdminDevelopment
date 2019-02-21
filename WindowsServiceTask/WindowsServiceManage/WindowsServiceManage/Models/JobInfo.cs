namespace WindowsServiceManage.Models
{
    public class JobInfo
    {
        public string Name { get; set; }

        public string TriggerGroup { get; set; }

        public string TriggerName { get; set; }

        public string Description { get; set; }

        public string Cron { get; set; }

        public string CronExpressionDescriptor { get; set; }

        public string NextStartTime { get; set; }




    }
}