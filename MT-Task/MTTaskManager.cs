using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SardineFish.MTTask
{
    public class MTTaskManager
    {
        /// <summary>
        /// The max number of processors.
        /// </summary>
        public int MaxProcessorCount { get; set; } = 100;
        /// <summary>
        /// The time between twice check of processors in milisecond.
        /// </summary>
        public int UpdateInterval { get; set; } = 10;
        /// <summary>
        /// The task queue
        /// </summary>
        public MTTaskQueue Tasks { get; protected set; } = new MTTaskQueue();
        /// <summary>
        /// The task processors.
        /// </summary>
        public List<TaskProcessor> Processors { get; protected set; } = new List<TaskProcessor>();
        /// <summary>
        /// The max cost allowed. If the total cost of all currently running processors is larger than this value, the rest processors will not get new tasks.
        /// </summary>
        public int MaxCost { get; set; } = int.MaxValue;
        /// <summary>
        /// This manager should always stand by, even if the task queue is empty or nither processor is running.
        /// </summary>
        public bool AlwaysStandBy { get; set; } = false;
        /// <summary>
        /// Invoke when the task queue is empty. 
        /// </summary>
        public virtual event Action OnTaskEmpty;
        /// <summary>
        /// Invoke when an exception was caught during task processing.
        /// </summary>
        public virtual event Action<MTTask, Exception> OnTaskError;
        /// <summary>
        /// The thread on which this manager is running.
        /// </summary>
        protected Thread HandleThread { get; set; }
        /// <summary>
        /// Start the task manager to handle the tasks in task queue.
        /// </summary>
        /// <param name="asynchronous">If true, it will create a new thread to run this manager.</param>
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
        /// <summary>
        /// The core management method.
        /// </summary>
        protected virtual void StartInternal()
        {
            if(MaxProcessorCount > Processors.Count)
                Processors.AddRange(new TaskProcessor[MaxProcessorCount - Processors.Count]);

            while(true)
            {
                int totalCost = 0;
                for (var i = 0; i < Processors.Count; i++)
                {
                    if (Processors[i] == null)
                    {
                        Processors[i] = new TaskProcessor();
                        Processors[i].TaskManager = this;
                    }
                    var proc = Processors[i];

                    if (MaxProcessorCount < Processors.Count && !proc.IsRunning)
                        Processors.RemoveAt(i--);

                    if (proc.IsRunning)
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
        /// <summary>
        /// Callback when error thrown.
        /// </summary>
        /// <param name="task">The task where the exception thrown.</param>
        /// <param name="ex">The exception.</param>
        internal virtual void InternalErrorCallback(MTTask task, Exception ex)
        {
            OnTaskError?.Invoke(task, ex);
        }
    }
}
