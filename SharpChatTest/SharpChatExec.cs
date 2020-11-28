using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace SharpChatTest {
    public class SharpChatExec : IDisposable {
        private Process Process { get; }

        public SharpChatExec(ushort port, string dbPath) {
            Process = Process.Start(new ProcessStartInfo {
                Arguments = string.Format(@"--dpn null --dbb sqlite --dbpath ""{1}"" --ip 127.0.0.1 --port {0} --testmode", port, dbPath),
                CreateNoWindow = false,
                FileName = GetExePath(),
                UseShellExecute = false,
                WorkingDirectory = Directory.GetCurrentDirectory(),
                RedirectStandardInput = true,
            });
            Thread.Sleep(1000);
        }

        private static string GetExePath() {
            string path = Directory.GetCurrentDirectory();
            string target = Path.GetFileName(path);

            while(!File.Exists(Path.Combine(path, @"SharpChat.sln")))
                path = Path.GetDirectoryName(path);

            path = Path.Combine(
                path, @"SharpChat/bin/Debug", target,
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"SharpChat.exe" : @"SharpChat"
            );

            return path;
        }

        private bool IsDisposable;

        ~SharpChatExec()
            => DoDispose();

        public void Dispose() {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        private void DoDispose() {
            if(IsDisposable)
                return;
            IsDisposable = true;

            Process.StandardInput.WriteLine('\x3');
            Process.StandardInput.Flush();
            Process.WaitForExit();
        }
    }
}
