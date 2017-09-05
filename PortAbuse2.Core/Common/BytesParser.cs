using System.IO;
using System.Text.RegularExpressions;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

namespace PortAbuse2.Core.Common
{
    public static class BytesParser
    {
        private static readonly Regex NonAlphaNumericSpecial = new Regex("[^a-zA-Z0-9':{}()/, -]");
        public static string BytesToStringConverted(byte[] bytes, bool removeNonAscii = false)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var res = streamReader.ReadToEnd();
                    if (removeNonAscii)
                    {
#if DEBUG
                        //if (bytes.Length > 1000)
                        //{
                        //    byte[] decompressed = Decompress(bytes);
                        //    var str = BytesToStringConverted(decompressed);
                        //}
#endif
                        res = RemoveNonAsciiAndUnEscape(res);
                    }
                    return res;
                }
            }
        }

        /// <summary>
        /// Returns empty if JSON parse failed;
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string RemoveNonAsciiAndUnEscape(string input)
        {
#if DEBUG
            var temp = input;
#endif
            input = input.Replace("\"", "'");
            input = input.Replace("\0", " ");
            input = NonAlphaNumericSpecial.Replace(input, "");

            return input;
        }
    }
}
