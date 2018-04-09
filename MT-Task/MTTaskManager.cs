using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SardineFish.MTTask
{
    public class MTTaskManager
    {
        public int MaxProcessorCount { get; set; } = 100;
        public int UpdateInterval { get; set; } = 10;
        public MTTaskQueue Tasks { get; protected set; } = new MTTaskQueue();
        public TaskProcessor[] Processors { get; protected set; }
        public int TotalResources { get; set; } = int.MaxValue;
        protected Thread HandleThread { get; set; }
        public void Start(bool asynchronous = false)
        {
            if (asynchronous)
            {
                HandleThread = new Thread(StartInternal);
                HandleThread.Start();
            }
            else
                StartInternal();
        }

        protected void StartInternal()
        {
            
        }
    }
}
