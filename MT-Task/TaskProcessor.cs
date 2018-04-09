using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SardineFish.MTTask
{
    public class TaskProcessor
    {
        public MTTaskManager TaskManager { get; internal set; }
        public Thread ProcessThread { get; protected set; }
        public Task ProcessTask { get; protected set; }
        public MTTask Task { get; protected set; }
        public bool IsRunning
        {
            get
            {
                if (Task == null)
                    return false;
                if (Task.UseThread)
                    return ProcessThread != null && (ProcessThread.ThreadState != ThreadState.Stopped);
                else
                    return ProcessTask != null && !(ProcessTask.IsCompleted || ProcessTask.IsCanceled || ProcessTask.IsFaulted);
            }
        }
        public long Cost { get => Task == null ? 0 : Task.Cost; }

        public void Start(MTTask task)
        {
            Task = task;
            task.OnFinish += OnFinishCallback;
            task.OnError += OnErrorCallback;
            task.Processor = this;
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

        void OnFinishCallback()
        {
            Task.OnFinish -= OnFinishCallback;
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

        void OnErrorCallback(Exception ex)
        {
            TaskManager?.InternalErrorCallback(Task, ex);
        }
    }
}
