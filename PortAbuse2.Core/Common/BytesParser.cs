using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PortAbuse2.Core.Common
{
    internal static class BytesParser
    {
        private static readonly Regex NonAlphaNumericSpecial = new Regex("[^a-zA-Z0-9':{}()/, -]");
        internal static string BytesToStringConverted(byte[] bytes, bool removeNonAscii = false)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    var res = streamReader.ReadToEnd();
                    if (removeNonAscii)
                    {
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
