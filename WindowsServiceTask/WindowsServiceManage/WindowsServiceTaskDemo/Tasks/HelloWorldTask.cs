using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolGood.Common;

namespace WindowsServiceTaskDemo.Tasks
{
    public class HelloWorldTask : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            if (context != null && ConfigUtil.GetValue("HelloWorldTask.Run") == "false") { return; }

            LogUtil.Info("[HelloWorldTask]开始执行", "HelloWorldTask-Execute");
            try {





                LogUtil.Info("[HelloWorldTask]执行结束", "HelloWorldTask-Execute");
            } catch (Exception ex) {
                LogUtil.Error("[HelloWorldTask]执行出错了:" + ex.Message, "HelloWorldTask-Execute");
            }
        }
    }
}
