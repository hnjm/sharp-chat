using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SquidChat
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Logger.Write("SquidChat - Multi-user (PHP) Sock Chat");

            using (ManualResetEvent mre = new ManualResetEvent(false))
            using (SockChatServer scs = new SockChatServer(6770))
            {
                Console.CancelKeyPress += (s, e) => { e.Cancel = true; mre.Set(); };
                mre.WaitOne();
            }
        }
    }
}
