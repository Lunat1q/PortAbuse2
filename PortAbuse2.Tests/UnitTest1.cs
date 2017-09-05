using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Parser;

namespace PortAbuse2.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCompressDecompress()
        {
            string testInput = "{session:'123456adef', warframe:'Lotus'";
            var testBytes = Encoding.ASCII.GetBytes(testInput);
            var compressed = Compressor.Compress(testBytes);
            var compressedToString = BytesParser.BytesToStringConverted(compressed);

        }

        [TestMethod]
        public void TestWfDecompress()
        {
            var data =
                "d5:94:1f:be:49:ad:a7:74:00:39:c1:88:6a:00:23:2f:4c:6f:74:75:73:2f:50:6f:77:65:72:73:75:69:74:73:2f:4e:65:02:63:72:6f:20:05:0e:6b:72:6f:73:50:72:69:6d:65:21:c1:88:6b:00:20:a0:28:0b:55:70:67:72:61:64:65:73:2f:46:6f:63:20:0e:0f:54:61:63:74:69:63:4c:65:6e:73:89:c1:88:6c:00:2d:e0:07:25:03:53:6b:69:6e:e0:01:52:20:05:11:53:68:72:6f:75:64:48:65:6c:6d:65:74:c1:c1:88:6d:00:34:e0:0d:32:0e:41:72:6d:6f:72:2f:43:72:70:43:69:72:63:6c:65:40:0e:00:75:e0:00:0f:00:41:20:39:00:6e:e0:2c:39:06:4c:79:c1:88:6f:00:2b:e0:0d:39:60:a0:80:05:02:4e:6f:62:20:3a:09:6e:69:6d:73:61:c1:88:70:00:28:e0:0d:30:04:53:63:61:72:76:20:0d:04:52:61:7a:6f:72:40:0c:00:66:20:5e:00:71:e0:17:5e:e1:00:58:40:16:20:c9:00:72:e0:2c:c9:00:43:20:98:00:73:e0:0f:98:c0:6a:20:05:0c:44:61:6e:67:6c:65:73:f1:c1:88:74:00:3a:e0:0d:2d:04:53:69:67:69:6c:20:06:07:79:6e:64:69:63:61:74:65:60:0f:14:50:65:72:72:69:6e:53:65:71:75:65:6e:63:65:44:a1:c1:88:75:00:30:a0:3f:e2:08:2d:1d:53:65:61:72:63:68:54:68:65:44:65:61:64:41:75:67:6d:65:6e:74:43:61:72:64:e9:c1:88:76:00:39:a0:35:e0:00:75:05:4d:6f:64:73:2f:57:21:2d:19:72:61:6d:65:2f:41:76:61:74:61:72:53:68:69:65:6c:64:52:65:63:68:61:72:67:65:52:20:85:20:25:05:31:c2:88:77:00:42:e0:15:3e:12:44:75:61:6c:53:74:61:74:2f:43:6f:72:72:75:70:74:65:64:52:20:f1:00:65:60:a2:c0:24:05:11:c2:88:78:00:3e:e0:0c:47:02:4f:72:6f:21:03:05:43:68:61:6c:6c:65:20:36:e0:07:0f:20:23:0c:43:75:6e:6e:69:6e:67:b9:c1:88:79:00:33:e0:0c:43:c0:66:a0:ca:06:41:62:69:6c:69:74:79:60:86:20:1f:05:99:c1:88:7a:00:2f:e0:15:38:e0:00:c4:04:56:69:67:6f:72:20:1b:21:6e:00:7b:e1:00:6e:e0:0e:34:80:6d:08:48:65:61:6c:74:68:4d:61:78:20:1c:20:35:00:7c:e0:1d:35:81:6e:80:35:22:82:00:7d:e2:09:82:20:18:22:81:08:75:72:61:2f:45:6e:65:6d:79:42:7c:04:72:52:65:64:75:23:ef:01:6f:6e:40:17:20:20:05:01:c2:88:7e:00:3c:e0:0c:39:e1:06:47:e0:07:0f:20:23:61:a1:22:20:00:7f:e2:00:20:04:57:65:61:70:6f:22:8f:21:c9:00:70:20:0e:04:50:69:73:74:6f:22:97:22:f0:09:45:6c:65:63:74:72:6f:4d:61:67:e0:05:0d:22:d5:00:80:e2:0f:d5:00:48:20:3b:04:74:65:72:43:75:20:45:03:6d:69:7a:61:40:b7:c0:54:03:48:69:70:73:41:73:05:41:c2:88:81:00:44:e0:07:3f:20:9c:c0:28:06:2f:45:78:70:65:72:74:a0:9a:03:43:72:69:74:20:c2:23:0f:20:22:07:42:65:67:69:6e:6e:65:72:40:21:01:72:74";
            byte[] b2 = StringToByteArray(data);
            var encString = BytesParser.BytesToStringConverted(b2);
            string key = " ";
            var decString = EncryptOrDecrypt(encString, key);
        }

        string EncryptOrDecrypt(string text, string key)
        {
            var result = new StringBuilder();

            for (int c = 0; c < text.Length; c++)
                result.Append((char)((uint)text[c] ^ (uint)key[c % key.Length]));

            return result.ToString();
        }

        private static byte[] StringToByteArray(string hex)
        {
            hex = hex.Replace(":", string.Empty);
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}
