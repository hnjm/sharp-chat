using Hamakaze.Headers;
using System;

// TODO
// Figure out disposal behaviour of HttpMessages
// Niceties for reading the body would also be Nice

namespace Hamakaze {
    public class HttpClient : IDisposable {
        public const string PRODUCT_STRING = @"HMKZ";
        public const string VERSION_MAJOR = @"1";
        public const string VERSION_MINOR = @"0";
        public const string USER_AGENT = PRODUCT_STRING + @"/" + VERSION_MAJOR + @"." + VERSION_MINOR;

        private static HttpClient InstanceValue { get; set; }
        public static HttpClient Instance {
            get {
                if(InstanceValue == null)
                    InstanceValue = new HttpClient();
                return InstanceValue;
            }
        }

        private HttpConnectionManager Connections { get; }
        private HttpTaskManager Tasks { get; }

        public string DefaultUserAgent { get; set; } = USER_AGENT;
        public bool ReuseConnections { get; set; } = true;

        public HttpClient() {
            Connections = new HttpConnectionManager();
            Tasks = new HttpTaskManager();
        }

        public HttpTask CreateTask(
            HttpRequestMessage request,
            Action<HttpTask, HttpResponseMessage> onComplete = null,
            Action<HttpTask, Exception> onError = null,
            Action<HttpTask> onCancel = null,
            Action<HttpTask, long, long> onDownloadProgress = null,
            Action<HttpTask, long, long> onUploadProgress = null,
            Action<HttpTask, HttpTask.TaskState> onStateChange = null
        ) {
            if(request == null)
                throw new ArgumentNullException(nameof(request));
            if(string.IsNullOrWhiteSpace(request.UserAgent))
                request.UserAgent = DefaultUserAgent;
            request.Connection = ReuseConnections ? HttpConnectionHeader.KEEP_ALIVE : HttpConnectionHeader.CLOSE;

            HttpTask task = new HttpTask(Connections, request);

            if(onComplete != null)
                task.OnComplete += onComplete;
            if(onError != null)
                task.OnError += onError;
            if(onCancel != null)
                task.OnCancel += onCancel;
            if(onDownloadProgress != null)
                task.OnDownloadProgress += onDownloadProgress;
            if(onUploadProgress != null)
                task.OnUploadProgress += onUploadProgress;
            if(onStateChange != null)
                task.OnStateChange += onStateChange;

            return task;
        }

        public void RunTask(HttpTask task) {
            Tasks.RunTask(task);
        }

        public void SendRequest(
            HttpRequestMessage request,
            Action<HttpTask, HttpResponseMessage> onComplete = null,
            Action<HttpTask, Exception> onError = null,
            Action<HttpTask> onCancel = null,
            Action<HttpTask, long, long> onDownloadProgress = null,
            Action<HttpTask, long, long> onUploadProgress = null,
            Action<HttpTask, HttpTask.TaskState> onStateChange = null
        ) {
            RunTask(CreateTask(request, onComplete, onError, onCancel, onDownloadProgress, onUploadProgress, onStateChange));
        }

        public static void Send(
            HttpRequestMessage request,
            Action<HttpTask, HttpResponseMessage> onComplete = null,
            Action<HttpTask, Exception> onError = null,
            Action<HttpTask> onCancel = null,
            Action<HttpTask, long, long> onDownloadProgress = null,
            Action<HttpTask, long, long> onUploadProgress = null,
            Action<HttpTask, HttpTask.TaskState> onStateChange = null
        ) {
            Instance.SendRequest(request, onComplete, onError, onCancel, onDownloadProgress, onUploadProgress, onStateChange);
        }

        private bool IsDisposed;
        ~HttpClient()
            => DoDispose();
        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }
        private void DoDispose() {
            if(IsDisposed)
                return;
            IsDisposed = true;

            Tasks.Dispose();
            Connections.Dispose();
        }
    }
}
