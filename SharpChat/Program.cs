using System;
using System.Threading;

namespace SharpChat
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Logger.Write("SharpChat - Multi-Session (PHP) Sock Chat");

            using (ManualResetEvent mre = new ManualResetEvent(false))
            using (SockChatServer scs = new SockChatServer(6770))
            {
                Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
                mre.WaitOne();
            }
        }
    }
}
