using System.IO;

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
