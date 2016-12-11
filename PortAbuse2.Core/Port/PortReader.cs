using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace PortAbuse2.Core.Port
{
    public class PortMaker
    {
        public static List<Port> GetNetStatPorts()
        {
            var ports = new List<Port>();

            try
            {
                using (var p = new Process())
                {

                    var ps = new ProcessStartInfo
                    {
                        Arguments = "/c start \"notitle\" /B \"netstat.exe\" -a -n -o",
                        FileName = "cmd.exe",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    p.StartInfo = ps;


                    p.Start();

                    var stdOutput = p.StandardOutput;
                    var stdError = p.StandardError;

                    var content = stdOutput.ReadToEnd() + stdError.ReadToEnd();
                    var exitStatus = p.ExitCode.ToString();

                    if (exitStatus != "0")
                    {
                        // Command Errored. Handle Here If Need Be
                    }

                    //Get The Rows
                    var rows = Regex.Split(content, "\r\n");
                    ports.AddRange(from row in rows
                                   select Regex.Split(row, "\\s+")
                        into tokens
                                   where tokens.Length > 4 && (tokens[1].Equals("UDP") || tokens[1].Equals("TCP"))
                                   let localAddress = Regex.Replace(tokens[2], @"\[(.*?)\]", "1.1.1.1")
                                   select new Port
                                   {
                                       Protocol = localAddress.Contains("1.1.1.1") ? $"{tokens[1]}v6" : $"{tokens[1]}v4",
                                       PortNumber = localAddress.Split(':')[1],
                                       ProcessName = tokens[1] == "UDP" ? LookupProcess(Convert.ToInt16(tokens[4])) : LookupProcess(Convert.ToInt16(tokens[5]))
                                   });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return ports;
        }

        public static string LookupProcess(int pid)
        {
            string procName;
            try { procName = Process.GetProcessById(pid).ProcessName; }
            catch (Exception) { procName = "-"; }
            return procName;
        }
    }
}
