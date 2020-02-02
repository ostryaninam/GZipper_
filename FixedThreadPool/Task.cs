using System;

namespace FixedThreadPool
{
    public class Task
    {
        private Action action;
        public Task(Action action)
        {
            this.action = action;
        }

        public void Execute()
        {
            isRunning = true;
            action();
        }
    }
}
