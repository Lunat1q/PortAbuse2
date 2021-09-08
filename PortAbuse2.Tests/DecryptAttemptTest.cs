using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PortAbuse2.Core.Common;
using PortAbuse2.Core.Parser;

namespace PortAbuse2.Tests
{
    [TestClass]
    public class DecryptAttemptTest
    {
        [TestMethod]
        public void TestCompressDecompress()
        {
            string testInput = "{session:'123456adef', warframe:'Lotus'}";
            var testBytes = Encoding.ASCII.GetBytes(testInput);
            var compressed = Compressor.Compress(testBytes);
            var compressedToString = BytesParser.BytesToStringConverted(compressed);
        }

        [TestMethod]
        public void TestWfDecompress()
        {
            var list = new List<string>();
            var data = "d1:af:1f:8e:63:89:ab:70:53:e8:b7:60:38:46:32:1a:62:6f:d4:a3:be:5d:57:95:51:ed:3a:de:7f:44:14:e5:35:e7:56:02:8a:1f:1a:20:0b:08:ce:2a:c4:d3:4d:53:f2:39:2f:20:25:08:be:58:ae:3a:a4:53:b1:16:6b:20:0b:08:34:21:7d:53:31:59:98:61:60:20:23:08:39:65:88:0f:28:57:d4:37:04:20:0b:08:0a:3d:de:eb:e6:58:14:d7:3f:20:0b:08:98:a6:d9:d9:80:57:15:17:4d:20:0b:14:11:ca:b0:6d:21:52:f9:5b:ac:1a:4d:80:99:43:00:02:00:57:5d:31:8f:20:17:08:5e:cd:4a:54:a2:54:04:4b:aa:20:53:08:db:2a:94:77:11:54:04:4d:98:20:0b:01:9e:2d:40:0b:02:7f:21:8d:20:0b:08:e6:70:23:72:c6:53:f6:fd:c2:20:0b:08:ae:78:a6:0e:94:54:4a:90:03:20:0b:08:68:2d:59:16:3d:57:5b:c1:bb:20:47:08:15:71:e5:48:c1:54:b4:22:32:20:17:08:0d:7a:a9:90:ee:52:7f:f3:d4:20:6b:08:5c:22:00:00:63:54:b8:0d:02:20:17:08:6a:47:a9:91:0c:55:22:1d:ae:20:0b:08:02:71:ff:4b:3a:51:52:07:23:20:23:08:99:02:00:01:b1:53:ce:0a:d1:20:17:08:60:32:25:0a:91:56:2b:75:49:20:53:08:c2:1c:d2:55:55:55:fb:31:1a:20:17:08:93:34:83:7e:1a:56:04:36:c8:20:0b:08:97:27:7b:24:b4:58:41:31:2e:20:23:08:05:7e:22:69:84:54:55:8e:53:20:17:08:46:22:c2:3a:8f:52:f9:f5:af:20:53:08:5e:7e:00:03:c9:56:21:51:03:20:23:08:8e:fc:b9:98:0b:56:82:a6:88:20:0b:08:4c:1a:17:11:02:53:5c:ab:a1:20:23:08:8c:20:00:02:3a:51:e5:d5:da:20:0b:08:6c:4a:00:00:12:56:1e:bf:e5:20:23:08:de:d1:b9:a4:e1:53:f0:d2:2b:20:53:08:de:0b:ae:3a:a5:58:d0:75:f5:20:17:08:b4:e4:6a:4e:91:54:20:86:e1:20:17:08:7e:78:70:76:18:51:e2:4b:6b:20:3b:08:32:6d:00:00:0a:59:73:20:6a:20:23:08:2c:be:11:93:fe:52:a1:fa:d8:20:17:08:ec:39:00:00:5b:51:75:82:12:20:0b:07:cc:24:00:00:3d:50:ea:c1:40:2f:08:87:21:00:00:01:55:32:48:7f:20:47:08:4f:11:db:b1:d6:51:e7:bc:c9:20:17:08:c3:64:00:00:09:52:ff:36:3d:20:0b:08:92:3a:00:03:27:56:f8:21:63:20:53:08:af:aa:ca:1f:a7:57:cb:fe:c2:20:0b:08:fa:fa:20:68:07:53:a9:f0:75:20:3b:08:fb:48:9d:24:43:55:c0:87:49:20:0b:08:42:6d:9d:2c:c5:54:d7:71:5d:20:0b:07:6c:20:28:b0:7d:52:4e:f4:40:83:08:32:17:00:00:6b:52:57:2d:74:20:0b:08:d5:19:00:00:04:56:9a:3a:94:20:47:08:ba:5b:2e:97:f1:54:a0:0b:52:20:2f:08:1f:2a:b4:36:59:52:34:ae:45:20:23:08:09:15:00:00:1e:53:3e:16:9b:20:0b:08:2a:13:00:00:03:55:37:e5:04:20:23:08:31:0b:7c:3e:95:51:4a:e1:30:20:17:01:a3:0f:20:ef:03:57:ec:f0:b9:20:47:08:7a:10:ad:4b:b0:59:07:9e:47:20:0b:08:a2:38:b7:56:b1:51:56:80:20:20:23:08:68:4b:00:03:3e:56:98:4a:39:20:17:08:08:0e:2e:95:06:50:ee:86:06:20:17:00:35:20:53:04:00:54:30:40:26:20:53:08:15:22:eb:7f:e9:56:01:3d:54:20:0b:08:74:21:7b:23:d7:51:5a:e7:b4:20:23:08:3a:48:00:00:43:56:d1:f3:f6:20:3b:08:06:f9:7b:26:7b:58:8e:d7:ac:20:0b:08:16:cb:4f:cd:c4:59:14:22:62:20:0b:08:71:64:d4:bb:a4:51:a1:27:6c:20:2f:00:58:21:5b:04:0d:55:47:a0:f9:20:47:07:44:05:1c:a5:6d:59:88:7a:41:37:08:3e:e3:36:a5:41:51:65:73:60:20:23:01:fc:45:21:73:03:57:d2:b9:24:20:17:08:79:42:58:7b:80:55:e8:42:3b:20:2f:07:53:56:04:73:fc:53:8b:8f:42:93:08:d6:54:16:2c:94:56:aa:3a:9c:20:23:08:d1:98:90:a7:30:51:7e:02:28:20:3b:08:63:63:00:00:26:54:e8:43:c0:20:23:07:04:69:34:b7:be:56:9b:6c:43:5f:08:64:5f:2e:7d:ab:55:6f:20:39:20:17:08:5d:27:b3:12:4a:53:04:fc:aa:20:2f:00:78:20:8f:04:08:55:47:69:b6:20:17:08:29:27:1c:a5:f6:56:da:a3:f9:20:2f:08:3c:3a:b4:34:4c:58:e9:17:67:20:0b:08:fb:e2:de:ee:06:59:7d:46:b4:20:0b:08:39:5d:9d:db:9a:57:e6:c7:8d:20:0b:08:da:2c:ff:29:ca:54:99:9b:f8:20:3b:08:b0:6e:0b:d5:79:51:d9:18:8b:20:53:01:3b:49:20:53:03:54:b7:a7:80:20:17:01:ed:37:23:5f:03:51:a1:4b:5f:20:17:07:50:26:00:00:02:51:dc:07:41:df:08:c5:4f:00:00:17:53:ce:a5:9f:20:23:08:b5:5b:25:0a:94:52:90:e5:83:20:17:04:f6:1e:00:00:6c";
            byte[] b2 = StringToByteArray(data);
            var encString = BytesParser.BytesToStringConverted(b2);
            var t = ByteAsChar(b2);
            var t2 = ByteAsChar(b2, true);
            var t3 = ByteAsChar(b2, false, true);
            var t4 = ByteAsChar(b2, true, true);
        }

        private static string ByteAsChar(byte[] data, bool left = false, bool re = false)
        {
            string r = string.Empty;
            var b1 = data[0];
            var b2 = data[1];
            var shift = !re ? b1 : b2;
            
            for (var index = 2; index < data.Length; index++)
            {
                var b = data[index];
                if (left)
                    r += (char)(b ^ shift);
                else
                    r += (char)(b ^ shift);
            }
            return r;
        }

        private static string EncryptOrDecrypt(string text, string key)
        {
            var result = new StringBuilder();

            for (var c = 0; c < text.Length; c++)
                result.Append((char)(text[c] ^ (uint)key[c % key.Length]));

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
