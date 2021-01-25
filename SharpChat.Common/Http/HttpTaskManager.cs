using System;
using System.Threading;

namespace SharpChat.Http {
    public class HttpTaskManager : IDisposable {
        private Semaphore Lock { get; }

        public HttpTaskManager(int maxThreads = 5) {
            Lock = new Semaphore(maxThreads, maxThreads);
        }

        public void RunTask(HttpTask task) {
            if(task == null)
                throw new ArgumentNullException(nameof(task));
            if(!Lock.WaitOne())
                throw new Exception(@"Unable to acquire lock.");
            new Thread(() => {
                try {
                    while(task.NextStep());
                } finally {
                    Lock.Release();
                }
            }).Start();
        }

        private bool IsDisposed;
        ~HttpTaskManager()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;
            Lock.Dispose();
        }
    }
}
