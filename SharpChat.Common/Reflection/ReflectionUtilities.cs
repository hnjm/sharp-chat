using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SharpChat.Reflection {
    public static class ReflectionUtilities {
        public static void LoadAssemblies(string pattern) {
            IEnumerable<string> loaded = AppDomain.CurrentDomain.GetAssemblies().Select(a => Path.GetFullPath(a.Location));
            IEnumerable<string> files = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), pattern);
            foreach(string file in files) {
                string fullPath = Path.GetFullPath(file);
                if(!loaded.Contains(fullPath))
                    Assembly.LoadFile(fullPath);
            }
        }
    }
}
