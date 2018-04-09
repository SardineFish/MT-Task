using System;
using System.Collections.Generic;
using System.Text;
namespace SardineFish.MTTask
{
    public class PriorityQueue<T>
    {
        List<int> Priorities = new List<int>();
        List<T> Items = new List<T>();
        public void EnQueue(int priority, T item)
        {
            lock (Items)
            {
                var i = 0;
                for (i = 0; i < Priorities.Count && Priorities[i] < priority; i++) ;
                Priorities.Insert(i, priority);
                Items.Insert(i, item);
            }
        }
        public T DeQueue()
        {
            lock (Items)
            {
                var item = Items[Items.Count - 1];
                Priorities.RemoveAt(Priorities.Count - 1);
                Items.RemoveAt(Items.Count - 1);
                return item;
            }
        }
        public bool IsEmpty() => Items.Count <= 0;
    }
}