using System;

namespace FixedThreadPool
{
    public class Task
    {
        private Action action;
        bool isRunning;
        public Task(Action action)
        {
            this.action = action;
        }

        public bool IsRunning { get; }

        public void Execute()
        {
            isRunning = true;
            action();
        }
    }
}
