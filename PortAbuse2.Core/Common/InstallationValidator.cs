using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortAbuse2.Core.Common
{
    public class InstallationValidator
    {
        public ValidationResult Validate()
        {
            var installValidation = this.ValidateInstallation();


            if (installValidation != null && installValidation.Count > 0)
            {
                return new ValidationResult(ResultType.Critical, installValidation);
            }
            return new ValidationResult(ResultType.NoIssues);
        }

        private List<string> ValidateInstallation()
        {
            var nPCapFolder = Path.Combine(Environment.SystemDirectory, "Npcap");
            string pcapLib = null;
            string packetLib = null;
            if (Directory.Exists(nPCapFolder))
            {
                var libs = Directory.GetFiles(nPCapFolder, "*.dll");
                packetLib = libs.FirstOrDefault(x => x.EndsWith("Packet.dll", StringComparison.OrdinalIgnoreCase));
                pcapLib = libs.FirstOrDefault(x => x.EndsWith("pcap.dll", StringComparison.OrdinalIgnoreCase));
            }
            if (string.IsNullOrWhiteSpace(packetLib) || string.IsNullOrWhiteSpace(pcapLib))
            {
                return new List<string>{ "NPcap is not installed, program may not operate properly!" };
            }

            return null;
        }
    }
}