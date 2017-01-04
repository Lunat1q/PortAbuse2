using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortAbuse2.Core.Files
{
    public static class FileAccess
    {
        public static void AppendFile(string path, string filename, string text)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.AppendAllText(Path.Combine(path, filename), text);
            }
            catch
            {
                //ignore
            }
        }
    }
}
