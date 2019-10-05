using System;
using System.IO;
using System.Threading;

namespace SharpChat {
    public class Program {
        public static void Main(string[] args) {
            Console.WriteLine(@"   _____ __                     ________          __ ");
            Console.WriteLine(@"  / ___// /_  ____ __________  / ____/ /_  ____ _/ /_");
            Console.WriteLine(@"  \__ \/ __ \/ __ `/ ___/ __ \/ /   / __ \/ __ `/ __/");
            Console.WriteLine(@" ___/ / / / / /_/ / /  / /_/ / /___/ / / / /_/ / /_  ");
            Console.WriteLine(@"/____/_/ /_/\__,_/_/  / .___/\____/_/ /_/\__,_/\__/  ");
            Console.WriteLine(@"                     / _/            Sock Chat Server");
#if DEBUG
            Console.WriteLine(@"============================================ DEBUG ==");
#endif

            using ManualResetEvent mre = new ManualResetEvent(false);
            using SockChatServer scs = new SockChatServer(6770);
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
            mre.WaitOne();
        }
    }
}
