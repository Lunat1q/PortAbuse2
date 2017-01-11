using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortAbuse2.Core.Listener;

namespace PortAbuse2.Core.ApplicationExtensions
{
    public sealed class WarframeApp : IApplicationExtension
    {
        public string[] AppNames => new[] {"Warframe.x64", "Warframe"};
        public bool Active { get; set; }

        private async Task Worker()
        {
            if (Receiver == null) return;
            while (Active)
            {
                var visible = Receiver.ResultObjects.Where(x=>!x.Old && !x.Hidden).OrderBy(x=>x.DetectionStamp);
                var i = 2;
                foreach (var hosts in visible)
                {
                    hosts.ExtraInfo = i.ToString();
                    i++;
                }
                await Task.Delay(2000);
            }
        }

        public void Stop()
        {
            Active = false;
        }


        public void Start()
        {
            Active = true;
            Task.Run(Worker);
        }

        public CoreReceiver Receiver { get; set; }
    }
}
