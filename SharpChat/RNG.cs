﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpChat
{
    public static class RNG
    {
        private static readonly Random random = new Random();

        public static int Next()
        {
            lock (random)
                return random.Next();
        }

        public static int Next(int max)
        {
            lock (random)
                return random.Next(max);
        }

        public static int Next(int min, int max)
        {
            lock (random)
                return random.Next(min, max);
        }

        public static void NextBytes(byte[] buffer)
        {
            lock (random)
                random.NextBytes(buffer);
        }

        public static double NextDouble()
        {
            lock (random)
                return random.NextDouble();
        }
    }
}
