using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReportToolDemo.ReportServiceTool.Entity
{
    public class TaskEntity
    {
        /// <summary>
        /// 任务
        /// </summary>
        public Task Task { set; get; }

        /// <summary>
        /// 终止线程对象
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { set; get; }
    }
}
