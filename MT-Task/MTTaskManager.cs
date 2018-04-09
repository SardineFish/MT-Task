using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SardineFish.MTTask
{
    public class MTTaskManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="maxProcessorCount">The max number of processors.</param>
        public MTTaskManager(int maxProcessorCount)
        {
            MaxProcessorCount = maxProcessorCount;
        }

        /// <summary>
        /// The max number of processors.
        /// </summary>
        public int MaxProcessorCount { get; set; } = 100;
        /// <summary>
        /// The time between twice check of processors in milisecond.
        /// </summary>
        public int UpdateInterval { get; set; } = 1000;
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
        public long MaxCost { get; set; } = long.MaxValue;
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
        /// Add a task to task queue
        /// </summary>
        /// <param name="task">The task to add.</param>
        /// <param name="priority">The priority of the task, higher priority will be process as soon as posible.</param>
        public void AddTask(MTTask task, int priority = 0)
        {
            Tasks.EnQueue(priority, task);
        }
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
                Console.Clear();
                long totalCost = 0;
                
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
                    totalCost += proc.Cost;
                }

                for (var i = 0; i < Processors.Count && totalCost<MaxCost; i++)
                {
                    var proc = Processors[i];
                    if (!proc.IsRunning)
                    {
                        proc.Start(Tasks.DeQueue());
                        totalCost += proc.Cost;
                    }
                }
                Thread.Sleep(UpdateInterval);
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
