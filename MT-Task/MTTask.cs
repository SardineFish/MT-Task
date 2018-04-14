using System;
using System.Collections.Generic;
using System.Text;

namespace SardineFish.MTTask
{
    public abstract class MTTask
    {
        public TaskProcessor Processor { get; internal set; }
        public bool UseThread { get; set; } = true;
        public string Status { get; set; }
        public double Progress { get; set; }
        public long Cost { get; set; } = 0;
        public virtual event Action OnFinish;
        public virtual event Action<Exception> OnError;

        int lastPos = 0;
        public abstract void Run();
        public virtual void Finish()
        {
            OnFinish?.Invoke();
        }
        public virtual void Error(Exception ex)
        {
            OnError?.Invoke(ex);
        }
        public virtual void Log()
        {
            lastPos = LogUtility.PrintScrollText(Status, 40, lastPos + 1);
            LogUtility.PrintProgressBar(Progress, 20);
        }
    }
}

