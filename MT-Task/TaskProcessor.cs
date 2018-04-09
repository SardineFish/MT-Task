using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SardineFish.MTTask
{
    public class TaskProcessor
    {
        public Thread ProcessThread { get; protected set; }
        public Task ProcessTask { get; protected set; }
        public MTTask Task { get; protected set; }
        public bool Running
        {
            get
            {
                if (Task == null)
                    return false;
                if (Task.UseThread)
                    return ProcessThread != null && ProcessThread.ThreadState == ThreadState.Running;
                else
                    return ProcessTask != null && !(ProcessTask.IsCompleted || ProcessTask.IsCanceled || ProcessTask.IsFaulted);
            }
        }
        public int Cost { get => Task == null ? 0 : Task.Cost; }

        public void Start(MTTask task)
        {
            Task = task;
            task.OnFinish += Finish;
            if(task.UseThread)
            {
                ProcessThread = new Thread(Task.Run);
                ProcessThread.Start();
            }
            else
            {
                ProcessTask = new Task(task.Run);
                ProcessTask.Start();
            }
        }

        public void Finish()
        {
            Task.OnFinish -= Finish;
            if (ProcessThread != null)
            {
                if (ProcessThread.ThreadState == ThreadState.Running)
                    ProcessThread.Abort();
                ProcessThread = null;
            }
            if (ProcessTask != null)
            {
                ProcessTask.Dispose();
                ProcessTask = null;
            }
            Task = null;
        }
    }
}
