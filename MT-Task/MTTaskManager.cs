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
        public List<TaskProcessor> Processors { get; protected set; } = new List<TaskProcessor>();
        public int MaxCost { get; set; } = int.MaxValue;
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
            if(MaxProcessorCount > Processors.Count)
                Processors.AddRange(new TaskProcessor[MaxProcessorCount - Processors.Count]);

            while(true)
            {
                int totalCost = 0;
                for (var i = 0; i < Processors.Count; i++)
                {
                    if (Processors[i] == null)
                        Processors[i] = new TaskProcessor();
                    var proc = Processors[i];

                    if (MaxProcessorCount < Processors.Count && !proc.Running)
                        Processors.RemoveAt(i--);

                    if (proc.Running)
                        proc.Task.Log();
                    else
                    {
                        if (totalCost > MaxCost || Tasks.IsEmpty)
                            continue;
                        
                        proc.Start(Tasks.DeQueue());
                    }
                    totalCost += proc.Cost;
                }

            }
        }
    }
}
