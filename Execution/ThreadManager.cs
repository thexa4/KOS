using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace kOS.Execution
{
    public class ThreadManager
    {
        private SharedObjects _shared;
        private List<kThread> _threads;
        public List<kThread> Threads { get { return new List<kThread>(_threads); } }

        public ThreadManager(SharedObjects shared)
        {
            _shared = shared;
            _threads = new List<kThread>();
        }

        public kThread CreateThread()
        {
            kThread newThread = new kThread(_shared);
            _threads.Add(newThread);
            return newThread;
        }

        public kThread CreateThread(kThread parentThread, string programName, object[] parameters)
        {
            kThread newThread = new kThread(_shared, programName, parameters);
            _threads.Add(newThread);
            if (parentThread != null) parentThread.ChildThreads.Add(newThread);
            return newThread;
        }

        public void Remove(kThread thread)
        {
            if (_threads.Contains(thread))
            {
                // remove the thread
                _threads.Remove(thread);
                // recursively remove all its child threads
                foreach (kThread child in thread.ChildThreads)
                {
                    Remove(child);
                }
            }
        }
        
        public void Clear()
        {
            _threads.Clear();
        }

        public kThread FindLastForegroundThread()
        {
            kThread lastThread = null;
            // TODO: replace the foreach with a for that searches from the last
            foreach (kThread thread in _threads)
            {
                if (!thread.RunsInBackground)
                {
                    lastThread = thread;
                }
            }

            return lastThread;
        }
    }
}
